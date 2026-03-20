import asyncio
from datetime import datetime, timezone
import json
import logging

from event_contract import normalize_record

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

QUARANTINE_FILE = "quarantine_events.jsonl"
SOCKET_EVENTS_FILE = "socket_events.jsonl"


def quarantine_record(received_at, reason, raw):
    quarantine_event = {
        "received_at": received_at,
        "reason": reason,
        "raw": raw,
    }
    try:
        with open(QUARANTINE_FILE, "a", encoding="utf-8") as quarantine_file:
            quarantine_file.write(json.dumps(quarantine_event, ensure_ascii=True) + "\n")
    except OSError as error:
        logging.error(f"Failed to quarantine event: {error}")


def append_socket_event(received_at, client_label, raw, normalized):
    socket_event = {
        "received_at": received_at,
        "client": client_label,
        "raw": raw,
        "normalized": normalized,
    }
    try:
        with open(SOCKET_EVENTS_FILE, "a", encoding="utf-8") as socket_events_file:
            socket_events_file.write(json.dumps(socket_event, ensure_ascii=True) + "\n")
    except OSError as error:
        logging.error(f"Failed to write socket event log: {error}")

class SocketServer:
    def __init__(self, host='127.0.0.1', port=5555, callback=None):
        self.host = host
        self.port = port
        self.callback = callback
        self.server = None

    @staticmethod
    def _format_client_label(record):
        source = record.get("source", "unknown")
        source_instance = record.get("source_instance", "unknown")
        instrument = record.get("instrument") or "unknown-instrument"
        return f"{source}/{source_instance} [{instrument}]"

    @staticmethod
    def _is_connection_hello(record):
        return isinstance(record, dict) and record.get("kind") == "connection_hello"

    async def handle_client(self, reader, writer):
        addr = writer.get_extra_info('peername')
        client_label = None
        live_socket_logged = False
        logging.info(f"Connection opened from {addr}, awaiting client identity")
        
        while True:
            try:
                # Use readline() because JSONs can be very large (e.g. Order Flow Footprint)
                line = await reader.readline()
                if not line:
                    break
                    
                message = line.decode('utf-8').strip()
                if not message:
                    continue
                    
                try:
                    record = json.loads(message)
                    if self._is_connection_hello(record):
                        client_label = self._format_client_label(record)
                        logging.info(f"Client {addr} identified as {client_label}")
                        continue

                    received_at = datetime.now(timezone.utc).isoformat().replace("+00:00", "Z")
                    normalized_record, rejection_reason = normalize_record(record, received_at)
                    if rejection_reason:
                        quarantine_record(received_at, rejection_reason, record)
                        rejected_label = client_label or f"unidentified client {addr}"
                        logging.error(f"Rejected event from {rejected_label}: {rejection_reason}")
                        continue
                    if client_label is None:
                        client_label = self._format_client_label(normalized_record)
                        logging.info(f"Client {addr} identified as {client_label}")

                    append_socket_event(received_at, client_label, record, normalized_record)

                    if not live_socket_logged:
                        source_name = normalized_record.get("source_instance") or normalized_record.get("source") or "unknown"
                        logging.info(f"LIVE SOCKET ON: {source_name}")
                        live_socket_logged = True

                    if self.callback:
                        self.callback(normalized_record)
                except json.JSONDecodeError:
                    malformed_label = client_label or f"unidentified client {addr}"
                    logging.error(f"Malformed JSON from {malformed_label}: {message[:100]}...")
                        
            except ConnectionResetError:
                break
            except Exception as e:
                error_label = client_label or f"unidentified client {addr}"
                logging.error(f"Socket error from {error_label}: {e}")
                break

        disconnect_label = client_label or f"unidentified client {addr}"
        logging.info(f"Client {disconnect_label} disconnected.")
        writer.close()
        await writer.wait_closed()

    async def start(self):
        self.server = await asyncio.start_server(self.handle_client, self.host, self.port)
        addrs = ', '.join(str(sock.getsockname()) for sock in self.server.sockets)
        logging.info(f"Socket Server listening on {addrs} for cTrader data...")

        async with self.server:
            await self.server.serve_forever()
