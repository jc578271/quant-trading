# 06-01 Summary

## What Built

- Locked the Phase 6 dataset contract and canonical map for `XAUUSD -> GC` and `US500 -> ES`.
- Added the deterministic dataset builder in `main/training/build_dataset.py` plus the CLI in `scripts/build_training_dataset.py`.
- Added regression coverage for the dataset contract and backward-only join policy.

## Verification

- `pytest tests/test_training_dataset_contract.py -q` passed
- `pytest tests/test_training_join_policy.py -q` passed
- `python scripts/build_training_dataset.py --scope ctrader_xauusd_h1_baseline --logs-dir logs --output-root artifacts/datasets` passed
- Real manifest confirmed `ESM6.CME@RITHMIC` rows were excluded with `canonical_market_mismatch`

## Notes

- The real baseline dataset produced 32 `XAUUSD` rows.
- Current real sample includes no matched Bookmap rows inside the 300-second anchor windows, so Bookmap feature columns remain present but zero-filled for this run.
