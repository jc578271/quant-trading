from __future__ import annotations

import json
from pathlib import Path

import pytest


class FakeMT5Client:
    def __init__(self) -> None:
        self.connected = False

    def connect(self) -> bool:
        self.connected = True
        return True

    def disconnect(self) -> None:
        self.connected = False


@pytest.fixture
def runtime_root(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> Path:
    runtime_dir = tmp_path / "runtime"
    monkeypatch.setenv("QT_RUNTIME_ROOT", str(runtime_dir))
    runtime_dir.mkdir(parents=True, exist_ok=True)
    return runtime_dir


@pytest.fixture
def fake_mt5_client() -> FakeMT5Client:
    return FakeMT5Client()


@pytest.fixture
def make_connection_hello():
    def factory(
        source: str,
        source_instance: str,
        instrument: str,
        reconnect_count: int = 0,
        dropped_events_total: int = 0,
    ) -> dict:
        return {
            "kind": "connection_hello",
            "source": source,
            "source_instance": source_instance,
            "instrument": instrument,
            "timestamp": "2026-03-22T00:00:00Z",
            "reconnect_count": reconnect_count,
            "dropped_events_total": dropped_events_total,
        }

    return factory


@pytest.fixture
def make_event():
    def factory(event: str, instrument: str, payload: dict | None = None) -> dict:
        return {
            "event": event,
            "source": "bookmap",
            "source_instance": "fixture",
            "instrument": instrument,
            "timestamp": "2026-03-22T00:00:01Z",
            "payload": payload or {},
        }

    return factory


def read_status_json(runtime_root: Path) -> dict:
    return json.loads((runtime_root / "status.json").read_text(encoding="utf-8"))


def read_stage(runtime_root: Path, stage: str) -> dict:
    return read_status_json(runtime_root)["stages"][stage]
