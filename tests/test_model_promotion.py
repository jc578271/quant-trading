from __future__ import annotations

import json
from pathlib import Path

import joblib
import numpy as np

from ai_analyzer import AIAnalyzer
from runtime_paths import model_manifest_file
from training.promote_model import promote_model_run


class StatusProbe:
    def __init__(self) -> None:
        self.updates: list[dict] = []

    def update_stage(self, stage: str, **kwargs) -> None:
        self.updates.append({"stage": stage, **kwargs})

    def publish(self, now=None) -> None:
        return None

    @property
    def last_reason(self) -> str | None:
        if not self.updates:
            return None
        return self.updates[-1].get("reason")


def test_promotion_copies_blessed_artifact_into_runtime(tmp_path: Path):
    artifacts_root, runtime_root = _prepare_artifact_run(tmp_path)

    result = promote_model_run(
        scope="ctrader_xauusd_h1_baseline",
        run_id="run-001",
        artifacts_root=artifacts_root,
        runtime_root=runtime_root,
    )

    assert (runtime_root / "model.pkl").exists()
    assert (runtime_root / "feature_schema.json").exists()
    assert (runtime_root / "train_config.json").exists()
    assert (runtime_root / "dataset_manifest.json").exists()
    manifest = json.loads((runtime_root / "model_manifest.json").read_text(encoding="utf-8"))
    assert manifest["dataset_scope"] == "ctrader_xauusd_h1_baseline"
    assert manifest["run_id"] == "run-001"
    assert manifest["feature_schema_path"] == "runtime/feature_schema.json"
    assert result["runtime_manifest_path"] == runtime_root / "model_manifest.json"


def test_promotion_does_not_train_new_model(tmp_path: Path):
    artifacts_root, runtime_root = _prepare_artifact_run(tmp_path)
    before_runs = sorted((artifacts_root / "ctrader_xauusd_h1_baseline").iterdir())

    promote_model_run(
        scope="ctrader_xauusd_h1_baseline",
        run_id="run-001",
        artifacts_root=artifacts_root,
        runtime_root=runtime_root,
    )

    after_runs = sorted((artifacts_root / "ctrader_xauusd_h1_baseline").iterdir())
    assert before_runs == after_runs


def test_runtime_paths_expose_model_manifest_file(runtime_root: Path):
    assert model_manifest_file() == runtime_root / "model_manifest.json"
    assert model_manifest_file().name == "model_manifest.json"


def test_ai_analyzer_rejects_manifest_with_wrong_dataset_scope(runtime_root: Path, fake_mt5_client):
    _write_runtime_bundle(runtime_root, dataset_scope="wrong_scope")
    status = StatusProbe()

    analyzer = AIAnalyzer(fake_mt5_client, status=status)

    assert analyzer.model is None
    assert status.last_reason == "incompatible model manifest"


def test_ai_analyzer_rejects_manifest_with_wrong_feature_schema(runtime_root: Path, fake_mt5_client):
    _write_runtime_bundle(runtime_root, feature_schema=["wrong_feature"])
    status = StatusProbe()

    analyzer = AIAnalyzer(fake_mt5_client, status=status)

    assert analyzer.model is None
    assert status.last_reason == "incompatible model manifest"


def test_ai_analyzer_reports_training_artifact_missing_without_bootstrap_env(
    runtime_root: Path,
    fake_mt5_client,
    monkeypatch,
):
    monkeypatch.delenv("QT_ALLOW_BOOTSTRAP_MODEL", raising=False)
    status = StatusProbe()

    analyzer = AIAnalyzer(fake_mt5_client, status=status)

    assert analyzer.model is None
    assert status.last_reason == "training artifact missing"
    assert not (runtime_root / "model.pkl").exists()


def test_ai_analyzer_maps_gc_aliases_into_xauusd_bookmap_window(
    runtime_root: Path,
    fake_mt5_client,
):
    _write_runtime_bundle(runtime_root)
    status = StatusProbe()
    analyzer = AIAnalyzer(fake_mt5_client, status=status)

    analyzer.process_data(
        {
            "source": "bookmap",
            "event": "dot",
            "instrument": "GCJ6.COMEX@RITHMIC",
            "timestamp": "2026-03-24T16:00:00Z",
            "payload": {"side": "ASK", "value": "15"},
            "source_meta": {"alias": "GCJ6.COMEX@RITHMIC"},
        }
    )

    features = analyzer._current_bookmap_features("XAUUSD", "2026-03-24T16:00:00Z")

    assert features["has_bookmap_window"]
    assert features["bookmap_event_count_300s"] == 1
    assert features["bookmap_dot_count_300s"] == 1
    assert features["bookmap_signed_value_300s"] == 15.0


def _prepare_artifact_run(tmp_path: Path) -> tuple[Path, Path]:
    artifacts_root = tmp_path / "artifacts" / "models"
    runtime_root = tmp_path / "runtime"
    run_dir = artifacts_root / "ctrader_xauusd_h1_baseline" / "run-001"
    run_dir.mkdir(parents=True, exist_ok=True)
    runtime_root.mkdir(parents=True, exist_ok=True)
    (run_dir / "model.pkl").write_bytes(b"trained-model")
    (run_dir / "feature_schema.json").write_text(
        json.dumps({"feature_order": _expected_feature_schema()}, indent=2),
        encoding="utf-8",
    )
    (run_dir / "train_config.json").write_text(
        json.dumps(
            {
                "label": {
                    "type": "future_return_classification",
                    "horizon_bars": 1,
                    "long_threshold_ticks": 150,
                    "short_threshold_ticks": -150,
                }
            },
            indent=2,
        ),
        encoding="utf-8",
    )
    (run_dir / "dataset_manifest.json").write_text(
        json.dumps({"dataset_scope": "ctrader_xauusd_h1_baseline"}, indent=2),
        encoding="utf-8",
    )
    return artifacts_root, runtime_root


def _write_runtime_bundle(
    runtime_root: Path,
    *,
    dataset_scope: str = "ctrader_xauusd_h1_baseline",
    feature_schema: list[str] | None = None,
) -> None:
    runtime_root.mkdir(parents=True, exist_ok=True)
    _write_serialized_model(runtime_root / "model.pkl")
    (runtime_root / "feature_schema.json").write_text(
        json.dumps({"feature_order": feature_schema or _expected_feature_schema()}, indent=2),
        encoding="utf-8",
    )
    (runtime_root / "train_config.json").write_text(
        json.dumps(
            {
                "label": {
                    "type": "future_return_classification",
                    "horizon_bars": 1,
                    "long_threshold_ticks": 150,
                    "short_threshold_ticks": -150,
                }
            },
            indent=2,
        ),
        encoding="utf-8",
    )
    (runtime_root / "dataset_manifest.json").write_text(
        json.dumps({"dataset_scope": dataset_scope}, indent=2),
        encoding="utf-8",
    )
    (runtime_root / "model_manifest.json").write_text(
        json.dumps(
            {
                "dataset_scope": dataset_scope,
                "run_id": "run-001",
                "feature_schema_path": "runtime/feature_schema.json",
                "label_spec": {
                    "type": "future_return_classification",
                    "horizon_bars": 1,
                    "long_threshold_ticks": 150,
                    "short_threshold_ticks": -150,
                },
                "created_at": "2026-03-24T16:00:00Z",
            },
            indent=2,
        ),
        encoding="utf-8",
    )


def _expected_feature_schema() -> list[str]:
    return [
        "level_count",
        "total_volume",
        "buy_volume",
        "sell_volume",
        "delta_sum",
        "abs_delta_sum",
        "poc_price",
        "poc_distance_to_close_ticks",
        "wy_count",
        "wy_last_direction_sign",
        "wy_last_wave_volume",
        "wy_sum_wave_volume",
        "wy_mean_volume_per_time",
        "wy_max_wave_efficiency",
        "has_volume_profile",
        "has_wyckoff_window",
        "has_bookmap_window",
        "bookmap_event_count_300s",
        "bookmap_dot_count_300s",
        "bookmap_stop_count_300s",
        "bookmap_wall_count_300s",
        "bookmap_signed_value_300s",
    ]


def _write_serialized_model(path: Path) -> None:
    from sklearn.ensemble import RandomForestClassifier

    X_train = np.array(
        [
            [1000, 10000, 5500, 4500, 200, 250, 3000, -50, 3, 1, 120, 360, 18.0, 12.0, 1, 1, 0, 0, 0, 0, 0, 0],
            [-1000, 10000, 4500, 5500, -200, 250, 3000, 50, 3, -1, 120, 360, 18.0, 12.0, 1, 1, 0, 0, 0, 0, 0, 0],
            [0, 8000, 4000, 4000, 0, 100, 3000, 0, 2, 0, 80, 220, 14.0, 9.0, 1, 1, 0, 0, 0, 0, 0, 0],
        ]
    )
    y_train = np.array([1, -1, 0])
    model = RandomForestClassifier(n_estimators=10, random_state=42)
    model.fit(X_train, y_train)
    joblib.dump(model, path)
