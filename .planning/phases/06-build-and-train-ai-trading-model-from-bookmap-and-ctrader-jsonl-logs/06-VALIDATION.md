---
phase: 06
slug: build-and-train-ai-trading-model-from-bookmap-and-ctrader-jsonl-logs
status: ready
nyquist_compliant: true
wave_0_complete: true
created: 2026-03-24
updated: 2026-03-24
---

# Phase 06 - Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | `pytest` 9.0.2 |
| **Config file** | `pytest.ini` |
| **Quick run command** | `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests/test_training_dataset_contract.py tests/test_training_join_policy.py tests/test_training_labels.py tests/test_training_pipeline.py tests/test_model_promotion.py -q` |
| **Full suite command** | `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests -q` |
| **Estimated runtime** | ~45 seconds |

---

## Sampling Rate

- **After every task commit:** Run `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests/test_training_dataset_contract.py tests/test_training_join_policy.py tests/test_training_labels.py tests/test_training_pipeline.py tests/test_model_promotion.py -q`
- **After every plan wave:** Run `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests -q`
- **Before `$gsd-verify-work`:** Full suite must be green
- **Max feedback latency:** 45 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 06-01-01 | 01 | 1 | MLDATA-01 | unit | `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests/test_training_dataset_contract.py -q` | No - Wave 0 | pending |
| 06-01-02 | 01 | 1 | MLDATA-02 | unit/integration | `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests/test_training_join_policy.py -q` | No - Wave 0 | pending |
| 06-02-01 | 02 | 2 | MLLABEL-01 | unit | `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests/test_training_labels.py -q` | No - Wave 0 | pending |
| 06-02-02 | 02 | 2 | MLTRAIN-01 | integration | `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests/test_training_pipeline.py -q` | No - Wave 0 | pending |
| 06-03-01 | 03 | 3 | MLTRAIN-02 | integration | `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests/test_model_promotion.py -q` | No - Wave 0 | pending |

*Status: pending / green / red / flaky*

---

## Wave 0 Requirements

- [x] `tests/test_training_dataset_contract.py` is planned in `06-01`
- [x] `tests/test_training_join_policy.py` is planned in `06-01`
- [x] `tests/test_training_labels.py` is planned in `06-02`
- [x] `tests/test_training_pipeline.py` is planned in `06-02`
- [x] `tests/test_model_promotion.py` is planned in `06-03`
- [x] Existing `pytest.ini` already provides Python test discovery

Wave 0 coverage is explicit in the current plan set and must land before model promotion is trusted.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Dataset catalog applies canonical market mapping before deciding whether Bookmap rows can enrich training | MLDATA-02 | Requires the real `logs/` contents, not only fixtures | Run the dataset catalog/build commands against repo `logs/` and confirm the manifest documents `XAUUSD -> GC` and `US500 -> ES`, then excludes current `ESM6` rows from the `ctrader_xauusd_h1_baseline` sample because they map to the wrong canonical market |
| A full local train run writes one versioned artifact bundle with manifest and metrics | MLTRAIN-01 | Requires actual artifact output and filesystem inspection | Run the training CLI on the built dataset and confirm `artifacts/models/{scope}/{run_id}/` contains `model.pkl`, `metrics.json`, `train_config.json`, `feature_schema.json`, and `dataset_manifest.json` |
| Runtime only changes after explicit promotion and uses the same Bookmap mapping logic as offline training | MLTRAIN-02 | Requires live runtime files and startup behavior | Train a model, confirm `runtime/model.pkl` is unchanged until promotion, run the promotion command, then start `main/main.py` and confirm the promoted model is loaded with the mapped Bookmap feature schema rather than falling back silently |

---

## Validation Sign-Off

- [x] All planned tasks have `<automated>` verify or an explicit checkpoint
- [x] Sampling continuity is preserved across waves
- [x] Wave 0 covers all planned Phase 06 verification files
- [x] No watch-mode flags are required
- [x] Feedback latency remains under 45 seconds
- [x] `nyquist_compliant: true` is set in frontmatter

**Approval:** aligned with current Phase 06 plans
