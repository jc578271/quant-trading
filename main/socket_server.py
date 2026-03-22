from __future__ import annotations

import asyncio
import contextlib
import json
import logging
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

from event_contract import normalize_record
from pipeline_status import PipelineStatus
from runtime_paths import quarantine_events_file, socket_events_file

logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s")


def utc_timestamp() -> str:
    return datetime.now(timezone.utc).isoformat().replace("+00:00", "Z")


def _append_jsonl(path: Path, record: dict[str, Any], error_message: str) -> None:
    try:
        path.parent.mkdir(parents=True, exist_ok=True)
        with path.open("a", encoding="utf-8") as output_file:
            output_file.write(json.dumps(record, ensure_ascii=True) + "\n")
    except OSError as error:
        logging.error("%s: %s", error_message, error)


class SocketServer:
    def __init__(
        self,
        host: str = "127.0.0.1",
        port: int = 5555,
        callback=None,
        status: PipelineStatus | None = None,
        runtime_root: str | Path | None = None,
    ) -> None:
        self.host = host
        self.port = port
        self.callback = callback
        self.status = status
        self.runtime_root = Path(runtime_root) if runtime_root else None
        self.server = None
        self.connected_clients: dict[tuple[str, str], dict[str, Any]] = {}
        self.writer_identities: dict[asyncio.StreamWriter, tuple[str, str]] = {}
        self.active_writers: set[asyncio.StreamWriter] = set()

    def _ingest_status_fields(self) -> dict[str, Any]:
        connected_sources = sorted(
            f"{client['source']}/{client['source_instance']}"
            for client in self.connected_clients.values()
        )
        return {
            "connected_sources": connected_sources,
            "reconnects_total": sum(client["reconnect_count"] for client in self.connected_clients.values()),
            "dropped_events_total": sum(client["dropped_events_total"] for client in self.connected_clients.values()),
        }

    def _runtime_file(self, default_factory, filename: str) -> Path:
        if self.runtime_root:
            self.runtime_root.mkdir(parents=True, exist_ok=True)
            return self.runtime_root / filename
        return default_factory()

    def quarantine_record(self, received_at: str, reason: str, raw: dict[str, Any]) -> None:
        _append_jsonl(
            self._runtime_file(quarantine_events_file, "quarantine_events.jsonl"),
            {
                "received_at": received_at,
                "reason": reason,
                "raw": raw,
            },
            "Failed to quarantine event",
        )

    def append_socket_event(
        self,
        received_at: str,
        client_label: str,
        raw: dict[str, Any],
        normalized: dict[str, Any],
    ) -> None:
        _append_jsonl(
            self._runtime_file(socket_events_file, "socket_events.jsonl"),
            {
                "received_at": received_at,
                "client": client_label,
                "raw": raw,
                "normalized": normalized,
            },
            "Failed to write socket event log",
        )

    @staticmethod
    def _format_client_label(record: dict[str, Any]) -> str:
        source = record.get("source", "unknown")
        source_instance = record.get("source_instance", "unknown")
        instrument = record.get("instrument") or "unknown-instrument"
        return f"{source}/{source_instance} [{instrument}]"

    @staticmethod
    def _is_connection_hello(record: dict[str, Any]) -> bool:
        return isinstance(record, dict) and record.get("kind") == "connection_hello"

    @staticmethod
    def _is_connection_heartbeat(record: dict[str, Any]) -> bool:
        return isinstance(record, dict) and record.get("kind") == "connection_heartbeat"

    @staticmethod
    def _identity_key(record: dict[str, Any]) -> tuple[str, str]:
        return (
            str(record.get("source", "unknown")),
            str(record.get("source_instance", "unknown")),
        )

    @staticmethod
    def _counter_value(record: dict[str, Any], key: str) -> int:
        try:
            return int(record.get(key, 0))
        except (TypeError, ValueError):
            return 0

    def _upsert_identity(
        self,
        record: dict[str, Any],
        timestamp: str,
        writer: asyncio.StreamWriter,
    ) -> dict[str, Any]:
        source, source_instance = self._identity_key(record)
        identity_key = (source, source_instance)
        existing_identity = self.connected_clients.get(identity_key, {})
        identity = {
            "source": source,
            "source_instance": source_instance,
            "instrument": record.get("instrument")
            or existing_identity.get("instrument")
            or "unknown-instrument",
            "reconnect_count": self._counter_value(record, "reconnect_count")
            if "reconnect_count" in record
            else existing_identity.get("reconnect_count", 0),
            "dropped_events_total": self._counter_value(record, "dropped_events_total")
            if "dropped_events_total" in record
            else existing_identity.get("dropped_events_total", 0),
            "last_seen_at": timestamp,
        }
        self.connected_clients[identity_key] = identity
        self.writer_identities[writer] = identity_key
        return identity

    def _publish_identity_state(self, identity: dict[str, Any], timestamp: str) -> None:
        if self.status is None:
            return

        self.status.update_stage(
            "ingest",
            state="up",
            reason=f"connected: {identity['source']}/{identity['source_instance']}",
            updated_at=timestamp,
            **self._ingest_status_fields(),
            source=identity["source"],
            source_instance=identity["source_instance"],
            instrument=identity["instrument"],
            reconnect_count=identity["reconnect_count"],
        )
        self.status.publish(now=timestamp)

    def _publish_connected(self, hello: dict[str, Any], timestamp: str, writer: asyncio.StreamWriter) -> None:
        identity = self._upsert_identity(hello, timestamp, writer)
        self._publish_identity_state(identity, timestamp)

    def _publish_heartbeat(
        self,
        heartbeat: dict[str, Any],
        timestamp: str,
        writer: asyncio.StreamWriter,
    ) -> None:
        identity = self._upsert_identity(heartbeat, timestamp, writer)
        self._publish_identity_state(identity, timestamp)

    def _publish_disconnect(self, writer: asyncio.StreamWriter, timestamp: str) -> None:
        if self.status is None:
            return

        identity_key = self.writer_identities.pop(writer, None)
        if identity_key is None:
            return

        disconnected_identity = self.connected_clients.pop(identity_key, None)
        if disconnected_identity is None:
            return

        if self.connected_clients:
            latest_identity = max(
                self.connected_clients.values(),
                key=lambda client: client["last_seen_at"],
            )
            self.status.update_stage(
                "ingest",
                state="up",
                reason=f"connected: {latest_identity['source']}/{latest_identity['source_instance']}",
                updated_at=latest_identity["last_seen_at"],
                **self._ingest_status_fields(),
                source=latest_identity["source"],
                source_instance=latest_identity["source_instance"],
                instrument=latest_identity["instrument"],
                reconnect_count=latest_identity["reconnect_count"],
            )
        else:
            self.status.update_stage(
                "ingest",
                state="up",
                reason=f"disconnected: {disconnected_identity['source']}/{disconnected_identity['source_instance']}",
                updated_at=timestamp,
                **self._ingest_status_fields(),
                source=disconnected_identity["source"],
                source_instance=disconnected_identity["source_instance"],
                instrument=disconnected_identity["instrument"],
                reconnect_count=disconnected_identity["reconnect_count"],
            )
        self.status.publish(now=timestamp)

    async def handle_client(self, reader: asyncio.StreamReader, writer: asyncio.StreamWriter) -> None:
        addr = writer.get_extra_info("peername")
        client_label = None
        live_socket_logged = False
        self.active_writers.add(writer)
        logging.info("Connection opened from %s, awaiting client identity", addr)

        while True:
            try:
                line = await reader.readline()
                if not line:
                    break

                message = line.decode("utf-8").strip()
                if not message:
                    continue

                try:
                    record = json.loads(message)
                    if self._is_connection_hello(record):
                        client_label = self._format_client_label(record)
                        received_at = str(record.get("timestamp") or utc_timestamp())
                        self._publish_connected(record, received_at, writer)
                        logging.info("Client %s identified as %s", addr, client_label)
                        continue
                    if self._is_connection_heartbeat(record):
                        client_label = client_label or self._format_client_label(record)
                        received_at = str(record.get("timestamp") or utc_timestamp())
                        self._publish_heartbeat(record, received_at, writer)
                        continue

                    received_at = utc_timestamp()
                    normalized_record, rejection_reason = normalize_record(record, received_at)
                    if rejection_reason:
                        self.quarantine_record(received_at, rejection_reason, record)
                        rejected_label = client_label or f"unidentified client {addr}"
                        logging.error("Rejected event from %s: %s", rejected_label, rejection_reason)
                        continue
                    if client_label is None:
                        client_label = self._format_client_label(normalized_record)
                        logging.info("Client %s identified as %s", addr, client_label)

                    self.append_socket_event(received_at, client_label, record, normalized_record)

                    if not live_socket_logged:
                        source_name = (
                            normalized_record.get("source_instance")
                            or normalized_record.get("source")
                            or "unknown"
                        )
                        logging.info("LIVE SOCKET ON: %s", source_name)
                        live_socket_logged = True

                    if self.callback:
                        self.callback(normalized_record)
                except json.JSONDecodeError:
                    malformed_label = client_label or f"unidentified client {addr}"
                    logging.error("Malformed JSON from %s: %s...", malformed_label, message[:100])
            except ConnectionResetError:
                break
            except Exception as error:
                error_label = client_label or f"unidentified client {addr}"
                logging.error("Socket error from %s: %s", error_label, error)
                break

        disconnect_label = client_label or f"unidentified client {addr}"
        logging.info("Client %s disconnected.", disconnect_label)
        self._publish_disconnect(writer, utc_timestamp())
        self.active_writers.discard(writer)
        writer.close()
        with contextlib.suppress(ConnectionError, RuntimeError):
            await writer.wait_closed()

    async def start(self) -> None:
        self.server = await asyncio.start_server(self.handle_client, self.host, self.port)
        addrs = ", ".join(str(sock.getsockname()) for sock in self.server.sockets)
        logging.info("Socket Server listening on %s for cTrader data...", addrs)

        async with self.server:
            await self.server.serve_forever()

    async def stop(self) -> None:
        server = self.server
        if server is not None:
            server.close()

        writers = list(self.active_writers)
        for writer in writers:
            writer.close()
        for writer in writers:
            with contextlib.suppress(asyncio.TimeoutError, ConnectionError, RuntimeError):
                await asyncio.wait_for(writer.wait_closed(), timeout=1.0)
        if server is not None:
            await server.wait_closed()
            self.server = None
