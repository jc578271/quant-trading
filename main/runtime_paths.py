from __future__ import annotations

import os
from pathlib import Path


_RUNTIME_ROOT_ENV = "QT_RUNTIME_ROOT"
_REPO_ROOT = Path(__file__).resolve().parent.parent


def runtime_root() -> Path:
    override = os.environ.get(_RUNTIME_ROOT_ENV)
    root = Path(override) if override else _REPO_ROOT / "runtime"
    root.mkdir(parents=True, exist_ok=True)
    return root


def status_file() -> Path:
    return runtime_root() / "status.json"


def socket_events_file() -> Path:
    return runtime_root() / "socket_events.jsonl"


def quarantine_events_file() -> Path:
    return runtime_root() / "quarantine_events.jsonl"


def trade_history_file() -> Path:
    return runtime_root() / "trade_history.csv"


def model_file() -> Path:
    return runtime_root() / "model.pkl"


def event_history_file(event_name: str) -> Path:
    safe_name = "".join(
        character.lower()
        for character in event_name
        if character.isalnum() or character in {"_", "-"}
    )
    return runtime_root() / f"history_{safe_name}.csv"


def alert_history_file(alias: str) -> Path:
    safe_alias = "".join(
        character if character.isalnum() or character in {".", "-"} else "_"
        for character in alias
    )
    return runtime_root() / f"history_alert_{safe_alias}.csv"
