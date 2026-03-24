# 06-02 Summary

## What Built

- Locked the baseline label policy in `configs/training/ctrader_xauusd_baseline.json`.
- Added deterministic future-return labeling in `main/training/labels.py`.
- Added offline train/eval pipeline in `main/training/train_model.py` and `scripts/train_model.py`.
- Added training docs and regression coverage for label generation, walk-forward evaluation, artifact emission, and runtime isolation.

## Verification

- `pytest tests/test_training_labels.py -q` passed
- `pytest tests/test_training_pipeline.py -q` passed
- `python scripts/train_model.py --config configs/training/ctrader_xauusd_baseline.json --dataset-root artifacts/datasets --output-root artifacts/models` passed

## Notes

- Real training run: `artifacts/models/ctrader_xauusd_h1_baseline/20260324T163020Z-62c43ee2/`
- Metrics snapshot: `train_rows=31`, `validation_rows=21`, `test_rows=7`, `macro_f1=0.4167`, `balanced_accuracy=0.5045`
- Offline training did not mutate `runtime/model.pkl`; promotion remained a separate step.
