from __future__ import annotations

import json
import os
import tempfile
from copy import deepcopy
from datetime import datetime, timedelta, timezone
from pathlib import Path
from typing import Any

from runtime_paths import runtime_root as default_runtime_root


DEFAULT_STAGES = ("ingest", "buffering", "inference", "execution")
HEARTBEAT_SECONDS = 5
DEGRADED_AFTER_SECONDS = 10
DOWN_AFTER_SECONDS = 30


def utc_now() -> datetime:
    return datetime.now(timezone.utc)


def isoformat_utc(value: datetime) -> str:
    return value.astimezone(timezone.utc).isoformat().replace("+00:00", "Z")


def parse_timestamp(value: str) -> datetime:
    return datetime.fromisoformat(value.replace("Z", "+00:00")).astimezone(timezone.utc)


class PipelineStatus:
    def __init__(
        self,
        session_started_at: str | None = None,
        runtime_root: str | Path | None = None,
    ) -> None:
        self.runtime_root = Path(runtime_root) if runtime_root else default_runtime_root()
        self.runtime_root.mkdir(parents=True, exist_ok=True)
        self.session_started_at = session_started_at or isoformat_utc(utc_now())
        timestamp = self.session_started_at
        self.stages = {
            stage: {
                "state": "down",
                "updated_at": timestamp,
                "reason": "not initialized",
            }
            for stage in DEFAULT_STAGES
        }

    def update_stage(
        self,
        stage: str,
        *,
        state: str | None = None,
        reason: str | None = None,
        updated_at: str | None = None,
        **extra: Any,
    ) -> None:
        if stage not in self.stages:
            raise KeyError(f"Unknown stage: {stage}")

        timestamp = updated_at or isoformat_utc(utc_now())
        record = self.stages[stage]
        if state is not None:
            record["state"] = state
        if reason is not None:
            record["reason"] = reason
        record["updated_at"] = timestamp
        for key, value in extra.items():
            if value is not None:
                record[key] = value

    def snapshot(self, now: str | datetime | None = None) -> dict[str, Any]:
        reference = self._coerce_datetime(now)
        snapshot_stages = deepcopy(self.stages)
        for stage_name, stage in snapshot_stages.items():
            snapshot_stages[stage_name]["state"] = self._evaluate_stage_state(stage, reference)

        return {
            "updated_at": isoformat_utc(reference),
            "session_started_at": self.session_started_at,
            "stages": snapshot_stages,
        }

    def publish(self, now: str | datetime | None = None) -> dict[str, Any]:
        snapshot = self.snapshot(now=now)
        target = self.runtime_root / "status.json"
        target.parent.mkdir(parents=True, exist_ok=True)
        with tempfile.NamedTemporaryFile(
            "w",
            encoding="utf-8",
            dir=self.runtime_root,
            delete=False,
        ) as temporary_file:
            json.dump(snapshot, temporary_file, ensure_ascii=True, indent=2)
            temporary_file.write("\n")
            temporary_name = temporary_file.name
        os.replace(temporary_name, target)
        return snapshot

    def _evaluate_stage_state(self, stage: dict[str, Any], reference: datetime) -> str:
        last_updated = parse_timestamp(stage["updated_at"])
        age = reference - last_updated
        if age >= timedelta(seconds=DOWN_AFTER_SECONDS):
            return "down"
        if age >= timedelta(seconds=DEGRADED_AFTER_SECONDS):
            return "degraded"
        return stage["state"]

    @staticmethod
    def _coerce_datetime(value: str | datetime | None) -> datetime:
        if value is None:
            return utc_now()
        if isinstance(value, str):
            return parse_timestamp(value)
        return value.astimezone(timezone.utc)
