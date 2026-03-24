from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(REPO_ROOT / "main"))

from training.promote_model import promote_model_run


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description=(
            "Promote a blessed offline model run into runtime/model.pkl and "
            "runtime/model_manifest.json. Use --scope ctrader_xauusd_h1_baseline "
            "--run-id <run_id> --artifacts-root artifacts/models --runtime-root runtime."
        )
    )
    parser.add_argument("--scope", required=True, help="Example: --scope ctrader_xauusd_h1_baseline")
    parser.add_argument("--run-id", required=True)
    parser.add_argument("--artifacts-root", default="artifacts/models")
    parser.add_argument("--runtime-root", default="runtime")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    result = promote_model_run(
        scope=args.scope,
        run_id=args.run_id,
        artifacts_root=args.artifacts_root,
        runtime_root=args.runtime_root,
    )
    print(
        json.dumps(
            {
                "runtime_model_path": str(result["runtime_model_path"]),
                "runtime_manifest_path": str(result["runtime_manifest_path"]),
                "runtime_feature_schema_path": str(result["runtime_feature_schema_path"]),
            },
            indent=2,
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
