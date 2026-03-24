from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(REPO_ROOT / "main"))

from training.train_model import train_offline_model


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description=(
            "Train the Phase 6 offline baseline model and write a versioned bundle under "
            "artifacts/models/ctrader_xauusd_h1_baseline with model.pkl, metrics.json, "
            "predictions.parquet, train_config.json, dataset_manifest.json, and feature_schema.json."
        )
    )
    parser.add_argument("--config", default="configs/training/ctrader_xauusd_baseline.json")
    parser.add_argument("--dataset-root", default="artifacts/datasets")
    parser.add_argument("--output-root", default="artifacts/models")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    result = train_offline_model(
        config_path=args.config,
        dataset_root=args.dataset_root,
        output_root=args.output_root,
    )
    print(
        json.dumps(
            {
                "run_dir": str(result["run_dir"]),
                "metrics_path": str(result["metrics_path"]),
                "predictions_path": str(result["predictions_path"]),
                "dataset_manifest_path": str(result["dataset_manifest_path"]),
                "feature_schema_path": str(result["feature_schema_path"]),
                "model_path": str(result["model_path"]),
            },
            indent=2,
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
