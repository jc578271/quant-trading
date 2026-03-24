from __future__ import annotations

import json
import shutil
from datetime import UTC, datetime
from pathlib import Path
from typing import Any
from uuid import uuid4

import joblib
import pandas as pd
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import balanced_accuracy_score, f1_score
from sklearn.model_selection import TimeSeriesSplit

from training.labels import apply_future_return_labels

FEATURE_COLUMNS = [
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


def train_offline_model(
    config_path: str | Path,
    dataset_root: str | Path,
    output_root: str | Path,
    *,
    run_id: str | None = None,
) -> dict[str, Any]:
    config_path = Path(config_path)
    dataset_root = Path(dataset_root)
    output_root = Path(output_root)

    train_config = json.loads(config_path.read_text(encoding="utf-8"))
    dataset_scope = train_config["dataset_scope"]
    dataset_run_dir = _latest_dataset_run_dir(dataset_root / dataset_scope)
    dataset_path = dataset_run_dir / "dataset.parquet"
    dataset_manifest_path = dataset_run_dir / "dataset_manifest.json"
    dataset_frame = pd.read_parquet(dataset_path)
    labeled_frame = apply_future_return_labels(dataset_frame, train_config["label"])
    if len(labeled_frame) <= int(train_config["evaluation"]["n_splits"]):
        raise ValueError("Not enough labeled rows for walk-forward evaluation")

    predictions_frame, metrics = evaluate_walk_forward(
        labeled_frame,
        n_splits=int(train_config["evaluation"]["n_splits"]),
        model_config=train_config.get("model", {}),
    )

    final_model = build_model(train_config.get("model", {}))
    final_model.fit(_feature_matrix(labeled_frame), labeled_frame["target_class_1"])

    run_id = run_id or _make_run_id()
    run_dir = output_root / dataset_scope / run_id
    run_dir.mkdir(parents=True, exist_ok=True)

    joblib.dump(final_model, run_dir / "model.pkl")
    _write_json(run_dir / "metrics.json", metrics)
    _write_json(run_dir / "train_config.json", train_config)
    _write_json(
        run_dir / "feature_schema.json",
        {
            "dataset_scope": dataset_scope,
            "feature_order": FEATURE_COLUMNS,
        },
    )
    shutil.copyfile(dataset_manifest_path, run_dir / "dataset_manifest.json")
    predictions_frame.to_parquet(run_dir / "predictions.parquet", index=False)

    return {
        "run_dir": run_dir,
        "metrics_path": run_dir / "metrics.json",
        "predictions_path": run_dir / "predictions.parquet",
        "dataset_manifest_path": run_dir / "dataset_manifest.json",
        "feature_schema_path": run_dir / "feature_schema.json",
        "model_path": run_dir / "model.pkl",
    }


def evaluate_walk_forward(
    dataset: pd.DataFrame,
    *,
    n_splits: int,
    model_config: dict[str, Any] | None = None,
) -> tuple[pd.DataFrame, dict[str, Any]]:
    ordered = dataset.sort_values("anchor_timestamp").reset_index(drop=True).copy()
    splitter = TimeSeriesSplit(n_splits=3)
    if n_splits != 3:
        raise ValueError("Wave 2 locks walk-forward evaluation to n_splits=3")
    model_config = model_config or {}

    prediction_rows: list[dict[str, Any]] = []
    split_summaries: list[dict[str, Any]] = []
    last_test_rows = 0

    for fold_number, (train_index, test_index) in enumerate(splitter.split(ordered), start=1):
        train_frame = ordered.iloc[train_index].copy()
        test_frame = ordered.iloc[test_index].copy()
        model = build_model(model_config)
        model.fit(_feature_matrix(train_frame), train_frame["target_class_1"])
        predictions = model.predict(_feature_matrix(test_frame))
        last_test_rows = len(test_frame)

        for row, prediction in zip(test_frame.itertuples(index=False), predictions):
            prediction_rows.append(
                {
                    "row_id": row.row_id,
                    "anchor_timestamp": row.anchor_timestamp,
                    "target_class_1": int(row.target_class_1),
                    "prediction": int(prediction),
                    "fold": fold_number,
                }
            )

        split_summaries.append(
            {
                "fold": fold_number,
                "train_rows": int(len(train_frame)),
                "test_rows": int(len(test_frame)),
                "max_train_anchor_timestamp": str(train_frame.iloc[-1]["anchor_timestamp"]),
                "min_test_anchor_timestamp": str(test_frame.iloc[0]["anchor_timestamp"]),
            }
        )

    predictions_frame = pd.DataFrame.from_records(prediction_rows).sort_values("anchor_timestamp").reset_index(drop=True)
    metrics = {
        "train_rows": int(len(ordered)),
        "validation_rows": int(len(predictions_frame)),
        "test_rows": int(last_test_rows),
        "macro_f1": float(
            f1_score(
                predictions_frame["target_class_1"],
                predictions_frame["prediction"],
                average="macro",
                zero_division=0,
            )
        ),
        "balanced_accuracy": float(
            balanced_accuracy_score(
                predictions_frame["target_class_1"],
                predictions_frame["prediction"],
            )
        ),
        "splits": split_summaries,
    }
    return predictions_frame, metrics


def build_model(model_config: dict[str, Any] | None = None) -> RandomForestClassifier:
    model_config = model_config or {}
    model_type = model_config.get("type", "random_forest_classifier")
    if model_type != "random_forest_classifier":
        raise ValueError(f"Unsupported model type: {model_type}")

    return RandomForestClassifier(
        n_estimators=int(model_config.get("n_estimators", 200)),
        random_state=int(model_config.get("random_state", 42)),
        class_weight=model_config.get("class_weight", "balanced_subsample"),
        max_depth=_optional_int(model_config.get("max_depth")),
        min_samples_leaf=int(model_config.get("min_samples_leaf", 1)),
        min_samples_split=int(model_config.get("min_samples_split", 2)),
        max_features=model_config.get("max_features", "sqrt"),
    )


def _feature_matrix(dataset: pd.DataFrame) -> pd.DataFrame:
    features = dataset.loc[:, FEATURE_COLUMNS].copy()
    for column in ("has_volume_profile", "has_wyckoff_window", "has_bookmap_window"):
        features[column] = features[column].astype(int)
    return features


def _latest_dataset_run_dir(scope_root: Path) -> Path:
    if not scope_root.exists():
        raise FileNotFoundError(f"Dataset scope directory not found: {scope_root}")
    run_dirs = sorted(path for path in scope_root.iterdir() if path.is_dir())
    if not run_dirs:
        raise FileNotFoundError(f"No dataset runs found under {scope_root}")
    return run_dirs[-1]


def _write_json(path: Path, payload: dict[str, Any]) -> None:
    path.write_text(json.dumps(payload, indent=2, ensure_ascii=False), encoding="utf-8")


def _make_run_id() -> str:
    return datetime.now(tz=UTC).strftime("%Y%m%dT%H%M%SZ") + "-" + uuid4().hex[:8]


def _optional_int(value: Any) -> int | None:
    if value in (None, "", "none"):
        return None
    return int(value)
