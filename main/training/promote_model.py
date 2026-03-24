from __future__ import annotations

import json
import shutil
from datetime import UTC, datetime
from pathlib import Path
from typing import Any


PROMOTED_RUNTIME_FILES = (
    "model.pkl",
    "feature_schema.json",
    "train_config.json",
    "dataset_manifest.json",
)


def promote_model_run(
    scope: str,
    run_id: str,
    artifacts_root: str | Path,
    runtime_root: str | Path,
) -> dict[str, Any]:
    artifacts_root = Path(artifacts_root)
    runtime_root = Path(runtime_root)
    source_run_dir = artifacts_root / scope / run_id
    if not source_run_dir.exists():
        raise FileNotFoundError(f"Artifact run not found: {source_run_dir}")

    runtime_root.mkdir(parents=True, exist_ok=True)
    for file_name in PROMOTED_RUNTIME_FILES:
        shutil.copyfile(source_run_dir / file_name, runtime_root / file_name)

    train_config = json.loads((source_run_dir / "train_config.json").read_text(encoding="utf-8"))
    manifest = {
        "dataset_scope": scope,
        "run_id": run_id,
        "feature_schema_path": "runtime/feature_schema.json",
        "label_spec": train_config["label"],
        "created_at": datetime.now(tz=UTC).isoformat().replace("+00:00", "Z"),
    }
    manifest_path = runtime_root / "model_manifest.json"
    manifest_path.write_text(json.dumps(manifest, indent=2, ensure_ascii=False), encoding="utf-8")
    return {
        "runtime_model_path": runtime_root / "model.pkl",
        "runtime_manifest_path": manifest_path,
        "runtime_feature_schema_path": runtime_root / "feature_schema.json",
    }
