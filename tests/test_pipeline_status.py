from __future__ import annotations

import os
from datetime import datetime, timezone

from main import _initialize_status
from pipeline_status import PipelineStatus, parse_timestamp
from tests.conftest import read_status_json


def test_status_snapshot_has_only_updated_at_and_session_started_at_top_level_metadata(runtime_root):
    status = PipelineStatus(
        session_started_at="2026-03-22T00:00:00Z",
        runtime_root=runtime_root,
    )

    snapshot = status.snapshot(now="2026-03-22T00:00:00Z")

    assert set(snapshot.keys()) == {"updated_at", "session_started_at", "stages"}
    assert set(snapshot["stages"].keys()) == {"ingest", "buffering", "inference", "execution"}
    for stage in snapshot["stages"].values():
        assert {"state", "updated_at", "reason"} <= set(stage.keys())


def test_status_publish_is_atomic_and_keeps_stage_reason(runtime_root, monkeypatch):
    status = PipelineStatus(
        session_started_at="2026-03-22T00:00:00Z",
        runtime_root=runtime_root,
    )
    replace_calls = []
    original_replace = os.replace

    def tracking_replace(source, destination):
        replace_calls.append((source, destination))
        original_replace(source, destination)

    monkeypatch.setattr("pipeline_status.os.replace", tracking_replace)
    status.update_stage("ingest", state="up", reason="connected: bookmap/bm-primary")
    status.publish(now="2026-03-22T00:00:01Z")

    assert replace_calls
    source, destination = replace_calls[0]
    assert destination == runtime_root / "status.json"
    assert source != destination
    assert read_status_json(runtime_root)["stages"]["ingest"]["reason"] == "connected: bookmap/bm-primary"


def test_stage_state_transitions_use_only_up_degraded_down(runtime_root):
    status = PipelineStatus(
        session_started_at="2026-03-22T00:00:00Z",
        runtime_root=runtime_root,
    )
    status.update_stage(
        "ingest",
        state="up",
        reason="connected",
        updated_at="2026-03-22T00:00:00Z",
    )

    initial = status.publish(now="2026-03-22T00:00:00Z")
    degraded = status.publish(now="2026-03-22T00:00:11Z")
    down = status.publish(now="2026-03-22T00:00:31Z")

    assert initial["stages"]["ingest"]["state"] == "up"
    assert degraded["stages"]["ingest"]["state"] == "degraded"
    assert down["stages"]["ingest"]["state"] == "down"
    assert {
        initial["stages"]["ingest"]["state"],
        degraded["stages"]["ingest"]["state"],
        down["stages"]["ingest"]["state"],
    } <= {"up", "degraded", "down"}


def test_parse_timestamp_accepts_dotnet_seven_digit_fractional_seconds():
    parsed = parse_timestamp("2026-03-22T18:23:28.5598427+00:00")

    assert parsed == datetime(2026, 3, 22, 18, 23, 28, 559842, tzinfo=timezone.utc)


def test_parse_timestamp_accepts_epoch_with_dotnet_fractional_precision():
    parsed = parse_timestamp("1970-01-01T00:00:00.0000000+00:00")

    assert parsed == datetime(1970, 1, 1, 0, 0, 0, 0, tzinfo=timezone.utc)


def test_execution_stage_is_degraded_when_mt5_disconnects_but_simulator_available(runtime_root):
    status = PipelineStatus(
        session_started_at="2026-03-22T00:00:00Z",
        runtime_root=runtime_root,
    )

    _initialize_status(
        status,
        "2026-03-22T00:00:00Z",
        mt5_connected=False,
        simulator_enabled=True,
        execution_errors_total=1,
    )

    execution = read_status_json(runtime_root)["stages"]["execution"]
    assert execution["state"] == "degraded"
    assert execution["reason"] == "mt5 disconnected; simulator only"
    assert execution["mt5_connected"] is False
    assert execution["simulator_enabled"] is True
