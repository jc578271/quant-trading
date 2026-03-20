---
phase: 01-normalize-event-contract
plan: 02
subsystem: integration
tags: [python, validation, quarantine, socket, analyzer]
requires:
  - phase: 01-01
    provides: event-contract/v1 document and canonical Bookmap envelope
provides:
  - Python contract normalization for v1 and known legacy payloads
  - Socket-boundary quarantine logging for malformed or unsupported events
  - Canonical envelope consumption in AIAnalyzer with initialized DOM state
affects: [ctrader-exporters, phase-02-transport, signal-state]
tech-stack:
  added: []
  patterns: [boundary normalization, quarantine logging, canonical envelope consumption]
key-files:
  created: [main/event_contract.py]
  modified: [main/socket_server.py, main/ai_analyzer.py]
key-decisions:
  - "The Python boundary accepts known legacy producer shapes during rollout but normalizes everything into event-contract/v1 before downstream handling."
  - "Rejected records are quarantined with the original raw payload and a precise reason instead of being silently dropped."
patterns-established:
  - "Boundary guard pattern: normalize immediately after JSON parsing and continue listening after rejection."
  - "Consumer pattern: prefer canonical envelope fields and fall back to legacy top-level keys only for compatibility."
requirements-completed: [DATA-02]
duration: 3min
completed: 2026-03-20
---

# Phase 01: Normalize Event Contract Summary

**Python-side event normalization, quarantine logging, and canonical envelope consumption for the analyzer**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-20T22:08:58+07:00
- **Completed:** 2026-03-20T22:11:59+07:00
- **Tasks:** 3
- **Files modified:** 3

## Accomplishments
- Added `main/event_contract.py` to normalize both v1 envelopes and the currently known legacy Bookmap/cTrader shapes into one canonical record.
- Patched `main/socket_server.py` to quarantine rejected records in `quarantine_events.jsonl` with `received_at`, `reason`, and the raw payload while keeping the socket loop alive.
- Updated `AIAnalyzer` to initialize DOM state, read canonical `event` and `payload` fields, and keep downstream feature extraction compatible.

## Task Commits

Each task was committed atomically:

1. **Task 1: Create the Python contract normalizer** - `3cb1268` (feat)
2. **Task 2: Add validation and quarantine at the socket boundary** - `47fdd7d` (fix)
3. **Task 3: Teach AIAnalyzer to consume canonical envelopes and fix DOM state setup** - `6808596` (fix)

**Plan metadata:** pending in working tree

## Files Created/Modified
- `main/event_contract.py` - Defines schema versioning, event normalization, identity preservation, and rejection reasons.
- `main/socket_server.py` - Normalizes inbound records, quarantines bad events, and logs contract rejections distinctly from JSON parse failures.
- `main/ai_analyzer.py` - Consumes canonical envelopes, initializes `self.order_book`, and keeps legacy field fallback during rollout.

## Decisions Made
- Preserved legacy input support at the boundary so rollout can happen without forcing all producers to upgrade simultaneously.
- Stored quarantine records as JSON lines so rejected events remain inspectable without breaking the listener loop.

## Deviations from Plan

None - implementation matched the plan.

## Issues Encountered

- The host does not expose a Python interpreter on `PATH`, and `py -3` reports no installed Python 3 runtime, so `compileall` could not be executed in this environment.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Phase 2 can build transport and observability on top of one normalized Python ingest boundary.
- Remaining verification for this plan is environmental: rerun `py -3 -m compileall main` on a host with Python 3 installed.

---
*Phase: 01-normalize-event-contract*
*Completed: 2026-03-20*
