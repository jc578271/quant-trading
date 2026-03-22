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


async def _send_records(port: int, records: list[dict]) -> None:
    reader, writer = await asyncio.open_connection("127.0.0.1", port)
    for record in records:
        writer.write((json.dumps(record) + "\n").encode("utf-8"))
        await writer.drain()
    writer.close()
    await writer.wait_closed()
    await asyncio.sleep(0.05)


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
            await _send_records(
                port,
                [make_connection_hello("bookmap", "bm-primary", "ES")],
            )
        finally:
            server.close()
            await server.wait_closed()

    asyncio.run(exercise())

    ingest = read_stage(runtime_root, "ingest")
    assert ingest["state"] == "up"
    assert ingest["reason"] == "connected: bookmap/bm-primary"
    assert ingest["source"] == "bookmap"
    assert ingest["source_instance"] == "bm-primary"
    assert ingest["instrument"] == "ES"
    assert ingest["reconnect_count"] == 0
    assert ingest["dropped_events_total"] == 0


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
        finally:
            first_server.close()
            await first_server.wait_closed()

        second_server, second_port = await _run_server_once(socket_server)
        try:
            await _send_records(
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
            )
        finally:
            second_server.close()
            await second_server.wait_closed()

    asyncio.run(exercise())

    status_json = read_status_json(runtime_root)
    ingest = status_json["stages"]["ingest"]
    assert ingest["state"] == "up"
    assert ingest["reason"] == "connected: bookmap/bm-primary"
    assert ingest["reconnect_count"] == 1
    assert ingest["dropped_events_total"] == 7


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
