from __future__ import annotations

import asyncio
import contextlib
import logging
import os
from pathlib import Path

from pipeline_status import HEARTBEAT_SECONDS, PipelineStatus, isoformat_utc, utc_now

logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s")


async def _heartbeat_loop(status: PipelineStatus) -> None:
    while True:
        await asyncio.sleep(HEARTBEAT_SECONDS)
        status.publish()


def _load_env_file(env_path: Path | None = None) -> Path:
    resolved_path = env_path or Path(__file__).resolve().parent / ".env"
    if not resolved_path.exists():
        return resolved_path
    for raw_line in resolved_path.read_text(encoding="utf-8").splitlines():
        line = raw_line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        name, value = line.split("=", 1)
        os.environ[name.strip()] = value.strip()
    return resolved_path


def _initialize_status(
    status: PipelineStatus,
    session_started_at: str,
    *,
    mt5_connected: bool,
    simulator_enabled: bool,
    execution_errors_total: int,
) -> None:
    status.update_stage(
        "ingest",
        state="up",
        reason="listening; awaiting producers",
        updated_at=session_started_at,
        connected_sources=[],
        reconnects_total=0,
        dropped_events_total=0,
    )
    status.update_stage(
        "buffering",
        state="degraded",
        reason="awaiting normalized records",
        updated_at=session_started_at,
        buffered_symbols=0,
        queued_records=0,
    )
    status.update_stage(
        "execution",
        state="up" if mt5_connected else "degraded",
        reason="mt5 connected" if mt5_connected else "mt5 disconnected",
        updated_at=session_started_at,
        mt5_connected=mt5_connected,
        simulator_enabled=simulator_enabled,
        execution_errors_total=execution_errors_total,
    )
    status.publish(now=session_started_at)


async def main() -> None:
    from ai_analyzer import AIAnalyzer
    from mt5_client import MT5Client
    from socket_server import SocketServer

    _load_env_file()
    session_started_at = isoformat_utc(utc_now())
    logging.info("Starting Quantum Trade AI System...")

    status = PipelineStatus(session_started_at=session_started_at)
    mt5_client = MT5Client()
    mt5_connected = mt5_client.connect()
    if not mt5_connected:
        logging.error("Failed to connect to MT5. Ensure MT5 is running and AutoTrading is enabled.")

    analyzer = AIAnalyzer(mt5_client, status=status)
    _initialize_status(
        status,
        session_started_at,
        mt5_connected=mt5_connected,
        simulator_enabled=False,
        execution_errors_total=0 if mt5_connected else 1,
    )
    server = SocketServer(
        host="127.0.0.1",
        port=5555,
        callback=analyzer.process_data,
        status=status,
    )
    heartbeat_task = asyncio.create_task(_heartbeat_loop(status))
    server_task = asyncio.create_task(server.start())

    try:
        await server_task
    except asyncio.CancelledError:
        logging.info("Shutting down Application...")
    finally:
        heartbeat_task.cancel()
        if not server_task.done():
            server_task.cancel()
        with contextlib.suppress(asyncio.CancelledError):
            await heartbeat_task
        with contextlib.suppress(asyncio.CancelledError):
            await server_task
        await server.stop()

        shutdown_at = isoformat_utc(utc_now())
        status.update_stage("ingest", state="down", reason="shutdown", updated_at=shutdown_at)
        status.update_stage("buffering", state="down", reason="shutdown", updated_at=shutdown_at)
        status.update_stage("inference", state="down", reason="shutdown", updated_at=shutdown_at)
        status.update_stage(
            "execution",
            state="down",
            reason="shutdown",
            updated_at=shutdown_at,
            mt5_connected=mt5_client.connected,
            simulator_enabled=False,
        )
        status.publish(now=shutdown_at)

        analyzer.shutdown()
        mt5_client.disconnect()


if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        logging.info("Application terminated by user.")
