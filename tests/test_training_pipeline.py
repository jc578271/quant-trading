from __future__ import annotations

import json
from pathlib import Path

import pandas as pd

from training.train_model import FEATURE_COLUMNS, build_model, evaluate_walk_forward, train_offline_model


def test_train_run_writes_model_metrics_and_manifest(tmp_path: Path):
    config_path, dataset_root, output_root, _ = _prepare_training_inputs(tmp_path)

    result = train_offline_model(
        config_path=config_path,
        dataset_root=dataset_root,
        output_root=output_root,
        run_id="test-train-run",
    )

    assert (result["run_dir"] / "model.pkl").exists()
    assert (result["run_dir"] / "metrics.json").exists()
    assert (result["run_dir"] / "predictions.parquet").exists()
    assert (result["run_dir"] / "train_config.json").exists()
    assert (result["run_dir"] / "dataset_manifest.json").exists()
    assert (result["run_dir"] / "feature_schema.json").exists()

    metrics = json.loads((result["run_dir"] / "metrics.json").read_text(encoding="utf-8"))
    assert {"train_rows", "validation_rows", "test_rows", "macro_f1", "balanced_accuracy"} <= set(metrics)


def test_walk_forward_evaluation_preserves_time_order(tmp_path: Path):
    _, _, _, dataset = _prepare_training_inputs(tmp_path)
    predictions, metrics = evaluate_walk_forward(dataset.iloc[:-1].copy(), n_splits=3)

    assert predictions["anchor_timestamp"].tolist() == sorted(predictions["anchor_timestamp"].tolist())
    assert all(
        split["max_train_anchor_timestamp"] < split["min_test_anchor_timestamp"]
        for split in metrics["splits"]
    )


def test_training_run_keeps_runtime_model_untouched(tmp_path: Path, monkeypatch):
    config_path, dataset_root, output_root, _ = _prepare_training_inputs(tmp_path)
    runtime_root = tmp_path / "runtime"
    runtime_root.mkdir(parents=True, exist_ok=True)
    runtime_model = runtime_root / "model.pkl"
    runtime_model.write_bytes(b"sentinel-runtime-model")
    monkeypatch.setenv("QT_RUNTIME_ROOT", str(runtime_root))

    train_offline_model(
        config_path=config_path,
        dataset_root=dataset_root,
        output_root=output_root,
        run_id="runtime-untouched",
    )

    assert runtime_model.read_bytes() == b"sentinel-runtime-model"


def test_feature_schema_keeps_bookmap_columns_even_when_current_sample_is_unmatched(tmp_path: Path):
    config_path, dataset_root, output_root, _ = _prepare_training_inputs(tmp_path, matched_bookmap=False)

    result = train_offline_model(
        config_path=config_path,
        dataset_root=dataset_root,
        output_root=output_root,
        run_id="schema-bookmap-columns",
    )

    feature_schema = json.loads((result["run_dir"] / "feature_schema.json").read_text(encoding="utf-8"))

    assert feature_schema["feature_order"] == FEATURE_COLUMNS
    assert "bookmap_event_count_300s" in feature_schema["feature_order"]
    assert "bookmap_signed_value_300s" in feature_schema["feature_order"]


def test_train_model_uses_model_hyperparameters_from_config(tmp_path: Path):
    config_path, dataset_root, output_root, _ = _prepare_training_inputs(tmp_path)
    config = json.loads(config_path.read_text(encoding="utf-8"))
    config["model"].update(
        {
            "n_estimators": 75,
            "random_state": 7,
            "class_weight": "balanced",
            "max_depth": 4,
            "min_samples_leaf": 2,
            "min_samples_split": 3,
            "max_features": "log2",
        }
    )
    config_path.write_text(json.dumps(config, indent=2), encoding="utf-8")

    model = build_model(config["model"])

    assert model.n_estimators == 75
    assert model.random_state == 7
    assert model.class_weight == "balanced"
    assert model.max_depth == 4
    assert model.min_samples_leaf == 2
    assert model.min_samples_split == 3
    assert model.max_features == "log2"


def _prepare_training_inputs(
    tmp_path: Path,
    *,
    matched_bookmap: bool = True,
) -> tuple[Path, Path, Path, pd.DataFrame]:
    config_dir = tmp_path / "configs" / "training"
    dataset_run_dir = tmp_path / "artifacts" / "datasets" / "ctrader_xauusd_h1_baseline" / "run-001"
    output_root = tmp_path / "artifacts" / "models"
    config_dir.mkdir(parents=True, exist_ok=True)
    dataset_run_dir.mkdir(parents=True, exist_ok=True)
    output_root.mkdir(parents=True, exist_ok=True)

    config_path = config_dir / "ctrader_xauusd_baseline.json"
    config_path.write_text(
        json.dumps(
            {
                "dataset_scope": "ctrader_xauusd_h1_baseline",
                "label": {
                    "type": "future_return_classification",
                    "horizon_bars": 1,
                    "long_threshold_ticks": 150,
                    "short_threshold_ticks": -150,
                },
                "evaluation": {
                    "method": "walk_forward",
                    "n_splits": 3,
                },
                "model": {
                    "type": "random_forest_classifier",
                    "n_estimators": 200,
                    "random_state": 42,
                },
            },
            indent=2,
        ),
        encoding="utf-8",
    )

    dataset = _make_training_dataset(matched_bookmap=matched_bookmap)
    dataset.to_parquet(dataset_run_dir / "dataset.parquet", index=False)
    (dataset_run_dir / "dataset_manifest.json").write_text(
        json.dumps(
            {
                "dataset_scope": "ctrader_xauusd_h1_baseline",
                "row_count": int(len(dataset)),
                "canonical_mapping_examples": ["XAUUSD -> GC", "US500 -> ES"],
            },
            indent=2,
        ),
        encoding="utf-8",
    )
    return config_path, tmp_path / "artifacts" / "datasets", output_root, dataset


def _make_training_dataset(*, matched_bookmap: bool) -> pd.DataFrame:
    rows = []
    closes = [100.0, 102.0, 100.0, 102.0, 100.0, 98.0, 100.0, 98.0, 100.0, 102.0, 100.0, 102.0]
    for index, close in enumerate(closes):
        bookmap_count = 2 if matched_bookmap and index % 2 == 0 else 0
        rows.append(
            {
                "row_id": f"row-{index}",
                "dataset_scope": "ctrader_xauusd_h1_baseline",
                "instrument": "XAUUSD",
                "canonical_market": "GC",
                "anchor_timestamp": f"2026-03-20T{index:02d}:00:00Z",
                "anchor_timeframe": "h1",
                "orderflow_event_id": f"of-{index}",
                "volumeprofile_event_id": f"vp-{index}",
                "wyckoff_window_start": f"2026-03-20T{max(index - 1, 0):02d}:00:00Z",
                "wyckoff_window_end": f"2026-03-20T{index:02d}:00:00Z",
                "open": close - 1.0,
                "high": close + 1.0,
                "low": close - 2.0,
                "close": close,
                "spread": 0.2,
                "tick_size": 0.01,
                "level_count": 10 + index,
                "total_volume": 1000 + index * 10,
                "buy_volume": 500 + index * 5,
                "sell_volume": 480 + index * 5,
                "delta_sum": (-1) ** index * 200.0,
                "abs_delta_sum": 250.0 + index,
                "poc_price": close - 0.5,
                "poc_distance_to_close_ticks": -50.0,
                "wy_count": 3,
                "wy_last_direction_sign": 1 if index % 2 == 0 else -1,
                "wy_last_wave_volume": 120.0 + index,
                "wy_sum_wave_volume": 340.0 + index,
                "wy_mean_volume_per_time": 18.5 + index,
                "wy_max_wave_efficiency": 11.0 + index,
                "has_volume_profile": True,
                "has_wyckoff_window": True,
                "has_bookmap_window": bool(bookmap_count),
                "bookmap_event_count_300s": bookmap_count,
                "bookmap_dot_count_300s": bookmap_count,
                "bookmap_stop_count_300s": 0,
                "bookmap_wall_count_300s": 0,
                "bookmap_signed_value_300s": float(bookmap_count * 25),
                "quarantined_source_count": 0,
                "future_return_ticks_1": (closes[index + 1] - close) / 0.01 if index < len(closes) - 1 else None,
                "target_class_1": (
                    1
                    if index < len(closes) - 1 and (closes[index + 1] - close) / 0.01 >= 150
                    else -1
                    if index < len(closes) - 1 and (closes[index + 1] - close) / 0.01 <= -150
                    else 0
                ),
            }
        )
    return pd.DataFrame.from_records(rows)
