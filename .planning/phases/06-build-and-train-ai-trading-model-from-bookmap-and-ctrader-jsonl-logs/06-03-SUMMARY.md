# 06-03 Summary

## What Built

- Added runtime manifest support in `main/runtime_paths.py`.
- Added explicit promotion flow in `main/training/promote_model.py` and `scripts/promote_model.py`.
- Updated `AIAnalyzer` to require a compatible promoted artifact, reject wrong scope/schema manifests, and gate bootstrap fallback behind `QT_ALLOW_BOOTSTRAP_MODEL=1`.
- Added runtime Bookmap window mapping for promoted feature extraction and updated operator docs.

## Verification

- `pytest tests/test_model_promotion.py -q` passed
- `pytest tests/test_ai_analyzer_exports.py -q` passed
- `pytest tests/test_training_dataset_contract.py tests/test_training_join_policy.py tests/test_training_labels.py tests/test_training_pipeline.py tests/test_model_promotion.py tests/test_ai_analyzer_exports.py -q` passed
- `python scripts/promote_model.py --scope ctrader_xauusd_h1_baseline --run-id 20260324T163020Z-62c43ee2 --artifacts-root artifacts/models --runtime-root runtime` passed
- Instantiating `AIAnalyzer` against the promoted runtime artifact loaded the promoted model and reported `model loaded`

## Notes

- `gitnexus_impact(target=\"AIAnalyzer\", direction=\"upstream\")` returned `LOW` risk with no direct callers or affected processes before the refactor.
- Final `gitnexus_detect_changes(scope=\"all\")` reported `MEDIUM` risk only because the final scope touched `AIAnalyzer`, runtime path helpers, and runtime docs; affected process reporting pointed at the existing `shutdown` path rather than a wider runtime blast radius.
