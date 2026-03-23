from __future__ import annotations

import json
from pathlib import Path

from order_simulator import OrderSimulator
from runtime_paths import (
    ALERT_HISTORY_KEYS,
    MODEL_FILE_NAME,
    ORDER_FLOW_HISTORY_FILE_NAME,
    ORDER_FLOW_HISTORY_KEYS,
    QUARANTINE_EVENT_KEYS,
    QUARANTINE_EVENTS_FILE_NAME,
    SOCKET_EVENT_KEYS,
    SOCKET_EVENTS_FILE_NAME,
    STATUS_FILE_NAME,
    TRADE_HISTORY_FILE_NAME,
    TRADE_HISTORY_KEYS,
    VOLUME_PROFILE_HISTORY_FILE_NAME,
    VOLUME_PROFILE_HISTORY_KEYS,
    WYCKOFF_STATE_HISTORY_FILE_NAME,
    WYCKOFF_STATE_HISTORY_KEYS,
    alert_history_file,
    event_history_file,
    logs_root,
    quarantine_events_file,
    runtime_root,
    socket_events_file,
    status_file,
    trade_history_file,
)
from socket_server import SocketServer


def test_runtime_paths_resolve_repo_root_runtime_directory(monkeypatch):
    monkeypatch.delenv("QT_RUNTIME_ROOT", raising=False)
    monkeypatch.delenv("QT_LOGS_ROOT", raising=False)
    resolved_runtime_root = runtime_root()
    resolved_logs_root = logs_root()

    assert resolved_runtime_root.name == "runtime"
    assert resolved_runtime_root.parent == Path(__file__).resolve().parent.parent
    assert resolved_logs_root.name == "logs"
    assert resolved_logs_root.parent == Path(__file__).resolve().parent.parent


def test_event_history_file_names_are_exact():
    assert event_history_file("orderflow").name == ORDER_FLOW_HISTORY_FILE_NAME
    assert event_history_file("volumeprofile").name == VOLUME_PROFILE_HISTORY_FILE_NAME
    assert event_history_file("wyckoff_state").name == WYCKOFF_STATE_HISTORY_FILE_NAME
    assert alert_history_file("GCJ6.COMEX_RITHMIC").name == "history_alertlistener_GCJ6.COMEX_RITHMIC.jsonl"


def test_order_simulator_defaults_to_runtime_trade_history():
    simulator = OrderSimulator()

    assert Path(simulator.csv_path).as_posix().endswith("logs/trade_history.jsonl")
    assert Path(simulator.csv_path).name == TRADE_HISTORY_FILE_NAME


def test_status_and_quarantine_paths_are_flat():
    runtime_dir = runtime_root()
    logs_dir = logs_root()

    assert status_file().parent == runtime_dir
    assert socket_events_file().parent == logs_dir
    assert quarantine_events_file().parent == logs_dir
    assert status_file().name == STATUS_FILE_NAME
    assert socket_events_file().name == SOCKET_EVENTS_FILE_NAME
    assert quarantine_events_file().name == QUARANTINE_EVENTS_FILE_NAME


def test_history_and_trade_logs_resolve_to_logs_directory():
    logs_dir = logs_root()

    assert event_history_file("orderflow").parent == logs_dir
    assert event_history_file("volumeprofile").parent == logs_dir
    assert event_history_file("wyckoff_state").parent == logs_dir
    assert alert_history_file("GCJ6.COMEX_RITHMIC").parent == logs_dir
    assert trade_history_file().parent == logs_dir


def test_socket_events_jsonl_schema_is_exact(runtime_root):
    server = SocketServer(runtime_root=runtime_root)
    server.append_socket_event(
        "2026-03-22T00:00:00Z",
        "bookmap/bm-primary [ES]",
        {"event": "alert"},
        {"event": "alert", "instrument": "ES"},
    )

    record = json.loads((runtime_root / SOCKET_EVENTS_FILE_NAME).read_text(encoding="utf-8").strip())
    assert list(record.keys()) == list(SOCKET_EVENT_KEYS)


def test_connection_heartbeat_does_not_write_socket_events(runtime_root):
    server = SocketServer(runtime_root=runtime_root)
    server._publish_heartbeat(
        {
            "kind": "connection_heartbeat",
            "source": "bookmap",
            "source_instance": "bm-primary",
            "instrument": "ES",
            "timestamp": "2026-03-22T00:00:05Z",
        },
        "2026-03-22T00:00:05Z",
        object(),
    )

    assert not (runtime_root / SOCKET_EVENTS_FILE_NAME).exists()


def test_quarantine_events_jsonl_schema_is_exact(runtime_root):
    server = SocketServer(runtime_root=runtime_root)
    server.quarantine_record(
        "2026-03-22T00:00:00Z",
        "bad payload",
        {"event": "broken"},
    )

    record = json.loads((runtime_root / QUARANTINE_EVENTS_FILE_NAME).read_text(encoding="utf-8").strip())
    assert list(record.keys()) == list(QUARANTINE_EVENT_KEYS)


def test_history_contracts_are_exact():
    assert ORDER_FLOW_HISTORY_KEYS == (
        "schema",
        "source_event_schema",
        "source",
        "source_instance",
        "event",
        "event_id",
        "instrument",
        "timeframe",
        "timestamp",
        "bar_closed",
        "bar",
        "summary",
        "levels",
        "source_meta",
    )
    assert VOLUME_PROFILE_HISTORY_KEYS == (
        "schema",
        "source_event_schema",
        "source",
        "source_instance",
        "event",
        "event_id",
        "instrument",
        "timeframe",
        "timestamp",
        "profile_type",
        "bar_closed",
        "bar",
        "summary",
        "levels",
        "source_meta",
    )
    assert WYCKOFF_STATE_HISTORY_KEYS == (
        "schema",
        "source_event_schema",
        "source",
        "source_instance",
        "event",
        "event_id",
        "instrument",
        "timeframe",
        "timestamp",
        "bar_closed",
        "bar",
        "wyckoff",
        "wave",
        "summary",
        "source_meta",
    )
    assert ALERT_HISTORY_KEYS == (
        "schema",
        "source_event_schema",
        "source",
        "source_instance",
        "event",
        "event_id",
        "instrument",
        "timestamp",
        "sequence",
        "payload",
        "source_meta",
    )


def test_trade_history_contract_is_exact(runtime_root):
    csv_path = runtime_root / TRADE_HISTORY_FILE_NAME
    simulator = OrderSimulator({"trade_log": str(csv_path)})
    simulator._log_trade_csv(
        {
            "timestamp": "2026-03-22T00:00:00Z",
            "symbol": "XAUUSD",
            "direction": "BUY",
            "entry_price": 3000.0,
            "sl_price": 2990.0,
            "tp_price": 3020.0,
            "lot_size": 1.0,
            "exit_price": 3010.0,
            "exit_reason": "TP",
            "pnl_pips": 100.0,
            "pnl_dollar": 100.0,
            "pnl_percent": 1.0,
            "balance_before": 10000.0,
            "balance_after": 10100.0,
        }
    )

    record = json.loads(csv_path.read_text(encoding="utf-8").splitlines()[0])
    assert list(record.keys()) == list(TRADE_HISTORY_KEYS)
