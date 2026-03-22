from __future__ import annotations

import asyncio
import json

from pipeline_status import PipelineStatus
from socket_server import SocketServer
from tests.conftest import read_stage, read_status_json


async def _run_server_once(socket_server: SocketServer):
    server = await asyncio.start_server(socket_server.handle_client, "127.0.0.1", 0)
    port = server.sockets[0].getsockname()[1]
    return server, port


async def _wait_for_disconnect(socket_server: SocketServer, timeout: float = 5.0) -> None:
    deadline = asyncio.get_running_loop().time() + timeout
    while socket_server.writer_identities:
        if asyncio.get_running_loop().time() >= deadline:
            raise TimeoutError("socket server did not release client connections in time")
        await asyncio.sleep(0.05)


async def _wait_for_stage(runtime_root, stage: str, predicate, timeout: float = 5.0) -> dict:
    deadline = asyncio.get_running_loop().time() + timeout
    while True:
        try:
            snapshot = read_status_json(runtime_root)
        except FileNotFoundError:
            snapshot = None

        if snapshot is not None:
            stage_record = snapshot["stages"][stage]
            if predicate(stage_record):
                return stage_record

        if asyncio.get_running_loop().time() >= deadline:
            raise TimeoutError(f"timed out waiting for stage {stage}")
        await asyncio.sleep(0.05)


async def _send_records(port: int, records: list[dict], close: bool = True):
    reader, writer = await asyncio.open_connection("127.0.0.1", port)
    for record in records:
        writer.write((json.dumps(record) + "\n").encode("utf-8"))
        await writer.drain()
    if not close:
        return reader, writer
    writer.close()
    await asyncio.wait_for(writer.wait_closed(), timeout=5)
    await asyncio.sleep(0.05)
    return reader, writer


def test_connection_hello_updates_ingest_identity(runtime_root, make_connection_hello):
    async def exercise() -> None:
        status = PipelineStatus(
            session_started_at="2026-03-22T00:00:00Z",
            runtime_root=runtime_root,
        )
        socket_server = SocketServer(
            host="127.0.0.1",
            port=0,
            callback=lambda record: None,
            status=status,
            runtime_root=runtime_root,
        )

        server, port = await _run_server_once(socket_server)
        try:
            _, writer = await _send_records(
                port,
                [make_connection_hello("bookmap", "bm-primary", "ES")],
                close=False,
            )
            ingest = await _wait_for_stage(
                runtime_root,
                "ingest",
                lambda stage: stage["reason"] == "connected: bookmap/bm-primary",
            )
            assert ingest["state"] == "up"
            assert ingest["reason"] == "connected: bookmap/bm-primary"
            assert ingest["source"] == "bookmap"
            assert ingest["source_instance"] == "bm-primary"
            assert ingest["instrument"] == "ES"
            assert ingest["reconnect_count"] == 0
            assert ingest["dropped_events_total"] == 0
            writer.close()
            await asyncio.wait_for(writer.wait_closed(), timeout=5)
            await _wait_for_disconnect(socket_server)
        finally:
            server.close()
            await asyncio.sleep(0)

    asyncio.run(exercise())


def test_disconnect_and_reconnect_preserve_cumulative_drop_counters(
    runtime_root,
    make_connection_hello,
):
    async def exercise() -> None:
        status = PipelineStatus(
            session_started_at="2026-03-22T00:00:00Z",
            runtime_root=runtime_root,
        )
        socket_server = SocketServer(
            host="127.0.0.1",
            port=0,
            callback=lambda record: None,
            status=status,
            runtime_root=runtime_root,
        )

        first_server, first_port = await _run_server_once(socket_server)
        try:
            await _send_records(
                first_port,
                [make_connection_hello("bookmap", "bm-primary", "ES", dropped_events_total=3)],
            )
            await _wait_for_disconnect(socket_server)
        finally:
            first_server.close()
            await asyncio.sleep(0)

        second_server, second_port = await _run_server_once(socket_server)
        try:
            _, writer = await _send_records(
                second_port,
                [
                    make_connection_hello(
                        "bookmap",
                        "bm-primary",
                        "ES",
                        reconnect_count=1,
                        dropped_events_total=7,
                    )
                ],
                close=False,
            )
            ingest = await _wait_for_stage(
                runtime_root,
                "ingest",
                lambda stage: (
                    stage["reason"] == "connected: bookmap/bm-primary"
                    and stage.get("reconnect_count") == 1
                    and stage.get("dropped_events_total") == 7
                ),
            )
            assert ingest["state"] == "up"
            assert ingest["reason"] == "connected: bookmap/bm-primary"
            assert ingest["reconnect_count"] == 1
            assert ingest["dropped_events_total"] == 7
            writer.close()
            await asyncio.wait_for(writer.wait_closed(), timeout=5)
            await _wait_for_disconnect(socket_server)
        finally:
            second_server.close()
            await asyncio.sleep(0)

    asyncio.run(exercise())


def test_connection_hello_accepts_dotnet_timestamp_precision(runtime_root, make_connection_hello):
    async def exercise() -> None:
        status = PipelineStatus(
            session_started_at="2026-03-22T00:00:00Z",
            runtime_root=runtime_root,
        )
        socket_server = SocketServer(
            host="127.0.0.1",
            port=0,
            callback=lambda record: None,
            status=status,
            runtime_root=runtime_root,
        )

        server, port = await _run_server_once(socket_server)
        try:
            _, writer = await _send_records(
                port,
                [
                    make_connection_hello(
                        "bookmap",
                        "bm-primary",
                        "ES",
                        timestamp="2026-03-22T18:23:19.0350000+00:00",
                    )
                ],
                close=False,
            )
            ingest = await _wait_for_stage(
                runtime_root,
                "ingest",
                lambda stage: stage["reason"] == "connected: bookmap/bm-primary",
            )
            assert ingest["state"] == "up"
            assert ingest["updated_at"] == "2026-03-22T18:23:19.0350000+00:00"
            writer.close()
            await asyncio.wait_for(writer.wait_closed(), timeout=5)
            await _wait_for_disconnect(socket_server)
        finally:
            server.close()
            await asyncio.sleep(0)

    asyncio.run(exercise())


def test_connection_heartbeat_refreshes_ingest_timestamp(
    runtime_root,
    make_connection_hello,
    make_connection_heartbeat,
):
    async def exercise() -> None:
        status = PipelineStatus(
            session_started_at="2026-03-22T00:00:00Z",
            runtime_root=runtime_root,
        )
        socket_server = SocketServer(
            host="127.0.0.1",
            port=0,
            callback=lambda record: None,
            status=status,
            runtime_root=runtime_root,
        )

        server, port = await _run_server_once(socket_server)
        try:
            _, writer = await _send_records(
                port,
                [
                    make_connection_hello("bookmap", "bm-primary", "ES"),
                    make_connection_heartbeat(
                        "bookmap",
                        "bm-primary",
                        "ES",
                        timestamp="2026-03-22T00:00:05Z",
                    ),
                ],
                close=False,
            )
            ingest = await _wait_for_stage(
                runtime_root,
                "ingest",
                lambda stage: stage["updated_at"] == "2026-03-22T00:00:05Z",
            )
            assert ingest["state"] == "up"
            assert ingest["reason"] == "connected: bookmap/bm-primary"
            writer.close()
            await asyncio.wait_for(writer.wait_closed(), timeout=5)
            await _wait_for_disconnect(socket_server)
        finally:
            server.close()
            await asyncio.sleep(0)

    asyncio.run(exercise())


def test_stop_closes_connected_clients(runtime_root, make_connection_hello):
    async def exercise() -> None:
        status = PipelineStatus(
            session_started_at="2026-03-22T00:00:00Z",
            runtime_root=runtime_root,
        )
        socket_server = SocketServer(
            host="127.0.0.1",
            port=0,
            callback=lambda record: None,
            status=status,
            runtime_root=runtime_root,
        )

        server, port = await _run_server_once(socket_server)
        socket_server.server = server
        try:
            reader, writer = await _send_records(
                port,
                [make_connection_hello("bookmap", "bm-primary", "ES")],
                close=False,
            )
            await _wait_for_stage(
                runtime_root,
                "ingest",
                lambda stage: stage["reason"] == "connected: bookmap/bm-primary",
            )
            await asyncio.wait_for(socket_server.stop(), timeout=5)
            assert await asyncio.wait_for(reader.readline(), timeout=5) == b""
            await _wait_for_disconnect(socket_server)
            assert not socket_server.active_writers
        finally:
            server.close()
            await asyncio.sleep(0)

    asyncio.run(exercise())


def test_status_publisher_marks_ingest_degraded_then_down_after_10_and_30_seconds(
    runtime_root,
):
    status = PipelineStatus(
        session_started_at="2026-03-22T00:00:00Z",
        runtime_root=runtime_root,
    )
    status.update_stage(
        "ingest",
        state="up",
        reason="disconnected: bookmap/bm-primary",
        updated_at="2026-03-22T00:00:00Z",
    )

    degraded_snapshot = status.publish(now="2026-03-22T00:00:11Z")
    assert degraded_snapshot["stages"]["ingest"]["state"] == "degraded"
    assert degraded_snapshot["stages"]["ingest"]["reason"] == "disconnected: bookmap/bm-primary"

    down_snapshot = status.publish(now="2026-03-22T00:00:31Z")
    assert down_snapshot["stages"]["ingest"]["state"] == "down"
    assert down_snapshot["stages"]["ingest"]["reason"] == "disconnected: bookmap/bm-primary"
