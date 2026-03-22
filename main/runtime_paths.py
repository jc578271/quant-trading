from __future__ import annotations

import os
from pathlib import Path


_RUNTIME_ROOT_ENV = "QT_RUNTIME_ROOT"
_LOGS_ROOT_ENV = "QT_LOGS_ROOT"
_REPO_ROOT = Path(__file__).resolve().parent.parent
STATUS_FILE_NAME = "status.json"
SOCKET_EVENTS_FILE_NAME = "socket_events.jsonl"
QUARANTINE_EVENTS_FILE_NAME = "quarantine_events.jsonl"
ORDER_FLOW_HISTORY_FILE_NAME = "history_order_flow_aggregated.csv"
VOLUME_PROFILE_HISTORY_FILE_NAME = "history_volume_profile.csv"
WYCKOFF_STATE_HISTORY_FILE_NAME = "history_wyckoff_state.csv"
TRADE_HISTORY_FILE_NAME = "trade_history.csv"
MODEL_FILE_NAME = "model.pkl"

SOCKET_EVENT_KEYS = ("received_at", "client", "raw", "normalized")
QUARANTINE_EVENT_KEYS = ("received_at", "reason", "raw")
ORDER_FLOW_HISTORY_HEADERS = (
    "timestamp",
    "instrument",
    "symbol",
    "source",
    "source_instance",
    "event",
    "deltaRank",
    "volumesRank",
    "volumesRankUp",
    "volumesRankDown",
    "spread",
)
VOLUME_PROFILE_HISTORY_HEADERS = (
    "timestamp",
    "instrument",
    "symbol",
    "source",
    "source_instance",
    "event",
    "profile_type",
    "vpPOC",
    "vpVAH",
    "vpVAL",
    "spread",
)
WYCKOFF_STATE_HISTORY_HEADERS = (
    "timestamp",
    "instrument",
    "symbol",
    "source",
    "source_instance",
    "event",
    "wyckoffVolume",
    "wyckoffTime",
    "zigZag",
    "waveVolume",
    "wavePrice",
    "waveVolPrice",
    "waveDirection",
    "spread",
)
ALERT_HISTORY_HEADERS = (
    "Timestamp",
    "AlertNumber",
    "Symbol",
    "AlertName",
    "Value",
    "Price",
    "Popup",
    "RawText",
)
TRADE_HISTORY_HEADERS = (
    "timestamp",
    "symbol",
    "direction",
    "entry_price",
    "sl_price",
    "tp_price",
    "lot_size",
    "exit_price",
    "exit_reason",
    "pnl_pips",
    "pnl_dollar",
    "pnl_percent",
    "balance_before",
    "balance_after",
)

EVENT_HISTORY_FILE_NAMES = {
    "orderflow": ORDER_FLOW_HISTORY_FILE_NAME,
    "order_flow_aggregated": ORDER_FLOW_HISTORY_FILE_NAME,
    "volumeprofile": VOLUME_PROFILE_HISTORY_FILE_NAME,
    "volume_profile": VOLUME_PROFILE_HISTORY_FILE_NAME,
    "wyckoff": WYCKOFF_STATE_HISTORY_FILE_NAME,
    "wyckoffstate": WYCKOFF_STATE_HISTORY_FILE_NAME,
    "wyckoff_state": WYCKOFF_STATE_HISTORY_FILE_NAME,
}
EVENT_HISTORY_HEADERS = {
    "orderflow": ORDER_FLOW_HISTORY_HEADERS,
    "order_flow_aggregated": ORDER_FLOW_HISTORY_HEADERS,
    "volumeprofile": VOLUME_PROFILE_HISTORY_HEADERS,
    "volume_profile": VOLUME_PROFILE_HISTORY_HEADERS,
    "wyckoff": WYCKOFF_STATE_HISTORY_HEADERS,
    "wyckoffstate": WYCKOFF_STATE_HISTORY_HEADERS,
    "wyckoff_state": WYCKOFF_STATE_HISTORY_HEADERS,
}


def runtime_root() -> Path:
    override = os.environ.get(_RUNTIME_ROOT_ENV)
    root = Path(override) if override else _REPO_ROOT / "runtime"
    root.mkdir(parents=True, exist_ok=True)
    return root


def logs_root() -> Path:
    override = os.environ.get(_LOGS_ROOT_ENV)
    if override:
        root = Path(override)
    else:
        runtime_override = os.environ.get(_RUNTIME_ROOT_ENV)
        root = Path(runtime_override).parent / "logs" if runtime_override else _REPO_ROOT / "logs"
    root.mkdir(parents=True, exist_ok=True)
    return root


def status_file() -> Path:
    return runtime_root() / STATUS_FILE_NAME


def socket_events_file() -> Path:
    return logs_root() / SOCKET_EVENTS_FILE_NAME


def quarantine_events_file() -> Path:
    return logs_root() / QUARANTINE_EVENTS_FILE_NAME


def trade_history_file() -> Path:
    return logs_root() / TRADE_HISTORY_FILE_NAME


def model_file() -> Path:
    return runtime_root() / MODEL_FILE_NAME


def event_history_headers(event_name: str) -> tuple[str, ...]:
    return EVENT_HISTORY_HEADERS.get(_normalize_event_name(event_name), ())


def event_history_file(event_name: str) -> Path:
    normalized_name = _normalize_event_name(event_name)
    file_name = EVENT_HISTORY_FILE_NAMES.get(normalized_name)
    if file_name is not None:
        return logs_root() / file_name

    safe_name = "".join(
        character.lower()
        for character in normalized_name
        if character.isalnum() or character in {"_", "-"}
    )
    return logs_root() / f"history_{safe_name}.csv"


def alert_history_file(alias: str) -> Path:
    safe_alias = "".join(
        character if character.isalnum() or character in {".", "-"} else "_"
        for character in alias
    )
    return logs_root() / f"history_alertlistener_{safe_alias}.csv"


def _normalize_event_name(event_name: str) -> str:
    return "".join(
        character.lower()
        for character in event_name
        if character.isalnum() or character in {"_", "-"}
    )
