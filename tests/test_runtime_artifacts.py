from __future__ import annotations

import json
from pathlib import Path

from order_simulator import OrderSimulator
from runtime_paths import (
    ALERT_HISTORY_HEADERS,
    MODEL_FILE_NAME,
    ORDER_FLOW_HISTORY_FILE_NAME,
    ORDER_FLOW_HISTORY_HEADERS,
    QUARANTINE_EVENT_KEYS,
    QUARANTINE_EVENTS_FILE_NAME,
    SOCKET_EVENT_KEYS,
    SOCKET_EVENTS_FILE_NAME,
    STATUS_FILE_NAME,
    TRADE_HISTORY_FILE_NAME,
    TRADE_HISTORY_HEADERS,
    VOLUME_PROFILE_HISTORY_FILE_NAME,
    VOLUME_PROFILE_HISTORY_HEADERS,
    WYCKOFF_STATE_HISTORY_FILE_NAME,
    WYCKOFF_STATE_HISTORY_HEADERS,
    alert_history_file,
    event_history_file,
    quarantine_events_file,
    runtime_root,
    socket_events_file,
    status_file,
)
from socket_server import SocketServer


def test_runtime_paths_resolve_repo_root_runtime_directory(monkeypatch):
    monkeypatch.delenv("QT_RUNTIME_ROOT", raising=False)
    resolved_runtime_root = runtime_root()

    assert resolved_runtime_root.name == "runtime"
    assert resolved_runtime_root.parent == Path(__file__).resolve().parent.parent


def test_event_history_file_names_are_exact():
    assert event_history_file("orderflow").name == ORDER_FLOW_HISTORY_FILE_NAME
    assert event_history_file("volumeprofile").name == VOLUME_PROFILE_HISTORY_FILE_NAME
    assert event_history_file("wyckoff_state").name == WYCKOFF_STATE_HISTORY_FILE_NAME
    assert alert_history_file("GCJ6.COMEX_RITHMIC").name == "history_alert_GCJ6.COMEX_RITHMIC.csv"


def test_order_simulator_defaults_to_runtime_trade_history():
    simulator = OrderSimulator()

    assert Path(simulator.csv_path).as_posix().endswith("runtime/trade_history.csv")
    assert Path(simulator.csv_path).name == TRADE_HISTORY_FILE_NAME


def test_status_and_quarantine_paths_are_flat():
    runtime_dir = runtime_root()

    assert status_file().parent == runtime_dir
    assert socket_events_file().parent == runtime_dir
    assert quarantine_events_file().parent == runtime_dir
    assert status_file().name == STATUS_FILE_NAME
    assert socket_events_file().name == SOCKET_EVENTS_FILE_NAME
    assert quarantine_events_file().name == QUARANTINE_EVENTS_FILE_NAME


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


def test_quarantine_events_jsonl_schema_is_exact(runtime_root):
    server = SocketServer(runtime_root=runtime_root)
    server.quarantine_record(
        "2026-03-22T00:00:00Z",
        "bad payload",
        {"event": "broken"},
    )

    record = json.loads((runtime_root / QUARANTINE_EVENTS_FILE_NAME).read_text(encoding="utf-8").strip())
    assert list(record.keys()) == list(QUARANTINE_EVENT_KEYS)


def test_history_csv_headers_are_exact():
    assert ",".join(ORDER_FLOW_HISTORY_HEADERS) == (
        "timestamp,instrument,symbol,source,source_instance,event,deltaRank,"
        "volumesRank,volumesRankUp,volumesRankDown,spread"
    )
    assert ",".join(VOLUME_PROFILE_HISTORY_HEADERS) == (
        "timestamp,instrument,symbol,source,source_instance,event,profile_type,vpPOC,vpVAH,vpVAL,spread"
    )
    assert ",".join(WYCKOFF_STATE_HISTORY_HEADERS) == (
        "timestamp,instrument,symbol,source,source_instance,event,wyckoffVolume,wyckoffTime,zigZag,"
        "waveVolume,wavePrice,waveVolPrice,waveDirection,spread"
    )
    assert ",".join(ALERT_HISTORY_HEADERS) == (
        "Timestamp,AlertNumber,Symbol,AlertName,Value,Price,Popup,RawText"
    )


def test_trade_history_headers_are_exact(runtime_root):
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

    header_line = csv_path.read_text(encoding="utf-8").splitlines()[0]
    assert header_line == ",".join(TRADE_HISTORY_HEADERS)
