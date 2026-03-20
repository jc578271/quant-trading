---
phase: 01-normalize-event-contract
plan: 01
subsystem: integration
tags: [bookmap, java, contract, schema, socket]
requires: []
provides:
  - Versioned event-contract/v1 specification for Bookmap and cTrader producers
  - Canonical Bookmap envelope emission for alert, dom, dot, and wall events
  - Rebuilt AlertListener jar after the contract refactor
affects: [python-ingest, ctrader-exporters, phase-02-transport]
tech-stack:
  added: []
  patterns: [canonical event envelope, source_meta preservation, producer-supplied timestamps]
key-files:
  created: [docs/event-contract-v1.md]
  modified: [bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java]
key-decisions:
  - "The contract locks required top-level fields to schema, source, source_instance, event, event_id, instrument, timestamp, payload, and source_meta."
  - "Bookmap-specific keys such as alias, popup, and raw alert details stay inside payload or source_meta instead of drifting into top-level fields."
patterns-established:
  - "Producer envelope pattern: emit canonical top-level metadata and keep event-specific details nested in payload."
  - "Compatibility pattern: downstream consumers may temporarily normalize legacy shapes, but producers should emit event-contract/v1 directly."
requirements-completed: [DATA-01]
duration: 3min
completed: 2026-03-20
---

# Phase 01: Normalize Event Contract Summary

**Versioned event-contract/v1 documentation plus a unified Bookmap emitter for alert, DOM, dot, and wall events**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-20T21:49:23+07:00
- **Completed:** 2026-03-20T21:52:23+07:00
- **Tasks:** 3
- **Files modified:** 3

## Accomplishments
- Added an in-repo `event-contract/v1` reference that defines required fields, event taxonomy, quarantine triggers, and Bookmap/cTrader examples.
- Refactored `AlertListener` so all Bookmap socket paths emit one canonical envelope with producer timestamps, event ids, payload, and source metadata.
- Rebuilt the Bookmap addon jar after the refactor to verify the contract changes still compile.

## Task Commits

Each task was committed atomically:

1. **Task 1: Create the versioned v1 contract document** - `90dc957` (feat)
2. **Task 2: Refactor AlertListener socket sends to one contract emitter** - `7a10e6a` (feat)
3. **Task 3: Rebuild the Bookmap addon after the contract refactor** - `866190e` (chore)

**Plan metadata:** pending in working tree

## Files Created/Modified
- `docs/event-contract-v1.md` - Defines the canonical v1 envelope, supported event families, and quarantine rules.
- `bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java` - Emits canonical Bookmap events through `sendContractEvent(...)`.
- `bookmap-addons/build/libs/alert-listener.jar` - Rebuilt addon artifact after the contract refactor.

## Decisions Made
- Kept producer-specific keys in `payload` and `source_meta` so the top-level contract stays stable across producer types.
- Required Bookmap to stamp event time itself rather than relying on the Python boundary to invent timestamps.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Python ingest can now normalize against a concrete v1 contract instead of multiple implicit Bookmap shapes.
- cTrader exporter alignment can target the same envelope and event names defined here.

---
*Phase: 01-normalize-event-contract*
*Completed: 2026-03-20*
