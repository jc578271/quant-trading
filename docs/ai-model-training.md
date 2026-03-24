# AI Model Training

## Build Dataset

```powershell
python scripts/build_training_dataset.py --scope ctrader_xauusd_h1_baseline --logs-dir logs --output-root artifacts/datasets
```

## Train Baseline Model

```powershell
python scripts/train_model.py --config configs/training/ctrader_xauusd_baseline.json --dataset-root artifacts/datasets --output-root artifacts/models
```

## Promote Runtime Artifact

```powershell
python scripts/promote_model.py --scope ctrader_xauusd_h1_baseline --run-id <run_id> --artifacts-root artifacts/models --runtime-root runtime
```

## Canonical Market Mapping

- `XAUUSD -> GC`
- `US500 -> ES`

Bookmap enrichment is allowed only after canonical alias mapping. If a Bookmap alias maps to a different canonical family, the dataset builder records that row in the manifest with `canonical_market_mismatch` instead of joining it into the baseline.

## Artifact Layout

```text
artifacts/
  datasets/
    ctrader_xauusd_h1_baseline/
      {run_id}/
        dataset.parquet
        dataset_manifest.json
        source_manifest.json
  models/
    ctrader_xauusd_h1_baseline/
      {run_id}/
        model.pkl
        metrics.json
        predictions.parquet
        train_config.json
        dataset_manifest.json
        feature_schema.json
```

## Runtime Boundary

Offline training must only read from `artifacts/datasets` and write to `artifacts/models`.

training must never write directly to runtime/model.pkl

`runtime/model.pkl` is a promoted artifact, and raw histories remain under `logs/*.jsonl`.
