---
phase: 02
slug: harden-transport-observability
status: ready
nyquist_compliant: true
wave_0_complete: true
created: 2026-03-22
updated: 2026-03-22
---

# Phase 02 - Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | `pytest` 9.0.2 for new Python tests; existing Java suite uses JUnit 4.13.2 |
| **Config file** | `pytest.ini` created in `02-01` Task 1 |
| **Quick run command** | `python -m pytest tests/test_socket_lifecycle.py -q` |
| **Full suite command** | `python -m pytest -q` |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Run the task's `<automated>` verify command
- **After every plan wave:** Run `python -m pytest -q`
- **Before `$gsd-verify-work`:** Full Python suite must be green
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 02-01-01 | 01 | 1 | PIPE-01 | setup/integration | `python -c "import pytest, sys; print(pytest.__version__); sys.exit(0 if pytest.__version__ == '9.0.2' else 1)"; python -m pytest tests/test_socket_lifecycle.py -q` | No - Wave 0 | pending |
| 02-02-01 | 02 | 2 | PIPE-02 | unit/integration | `python -m pytest tests/test_pipeline_status.py -q` | No - Wave 0 | pending |
| 02-03-01 | 03 | 3 | PIPE-03 | unit | `python -m pytest tests/test_runtime_artifacts.py -q` | No - Wave 0 | pending |

*Status: pending / green / red / flaky*

---

## Wave 0 Requirements

- [x] `tests/test_socket_lifecycle.py` planned in `02-01` Task 1
- [x] `tests/test_pipeline_status.py` planned in `02-02` Task 1
- [x] `tests/test_runtime_artifacts.py` planned in `02-03` Task 1
- [x] `tests/conftest.py` planned in `02-01` Task 1
- [x] `pytest.ini` planned in `02-01` Task 1
- [x] `python -m pip install pytest==9.0.2` planned in `02-01` Task 1 when version check fails

Wave 0 coverage is explicit in the current plan set and must complete before later-wave pytest commands run.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Bookmap producer reconnects after Python restart and resends `connection_hello` | PIPE-01 | Bookmap runtime is vendor-hosted and not covered by the new Python harness | Start `main.py`, connect AlertListener, stop `main.py`, wait for producer to enter reconnecting state, restart `main.py`, confirm a fresh `connection_hello` arrives and `runtime/status.json` returns `ingest` to `up` |
| At least one cTrader exporter reconnects after Python restart and resumes live sends | PIPE-01 | cTrader Automate behavior depends on host runtime and cannot be fully simulated in repo tests | Attach one indicator, confirm live stream, stop `main.py`, restart it, verify the indicator reconnects without manual edits and sends a fresh hello followed by live events |
| Execution stage reports `degraded` when MT5 is unavailable but simulator remains usable | PIPE-02 | Requires the actual workstation execution environment and current MT5 availability | Start the Python bridge with MT5 disconnected, inspect `runtime/status.json`, confirm `execution.state` is `degraded` with a simulator-only reason, then reconnect MT5 and confirm recovery |
| Runtime artifacts land under `runtime/` with the documented JSONL keys and CSV headers | PIPE-03 | Requires an integrated live run that generates real files from producers and the Python bridge together | Run the Python bridge long enough to emit live artifacts, inspect `runtime/socket_events.jsonl`, `runtime/quarantine_events.jsonl`, and the generated CSVs, and confirm filenames, keys, and headers match the documented contract |

---

## Validation Sign-Off

- [x] All planned tasks have `<automated>` verify or an explicit checkpoint
- [x] Sampling continuity is preserved across waves
- [x] Wave 0 covers all former MISSING references
- [x] No watch-mode flags are required
- [x] Feedback latency target remains under 30 seconds
- [x] `nyquist_compliant: true` is set in frontmatter

**Approval:** aligned with current Phase 02 plans
