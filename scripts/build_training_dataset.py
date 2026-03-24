from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(REPO_ROOT / "main"))

from training.build_dataset import build_dataset
from training.contracts import DATASET_SCOPE_CTRADER_XAUUSD_H1_BASELINE


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Build the deterministic Phase 6 training dataset.",
    )
    parser.add_argument(
        "--scope",
        default=DATASET_SCOPE_CTRADER_XAUUSD_H1_BASELINE,
        choices=[DATASET_SCOPE_CTRADER_XAUUSD_H1_BASELINE],
        help="Dataset scope to build; use --scope ctrader_xauusd_h1_baseline for the Wave 1 baseline.",
    )
    parser.add_argument("--logs-dir", default="logs")
    parser.add_argument("--output-root", default="artifacts/datasets")
    parser.add_argument("--instrument-map", default="configs/training/instrument_map.json")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    result = build_dataset(
        scope=args.scope,
        logs_dir=args.logs_dir,
        output_root=args.output_root,
        instrument_map_path=args.instrument_map,
    )
    print(
        json.dumps(
            {
                "dataset_path": str(result["dataset_path"]),
                "manifest_path": str(result["manifest_path"]),
                "source_manifest_path": str(result["source_manifest_path"]),
                "row_count": result["row_count"],
            },
            indent=2,
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
