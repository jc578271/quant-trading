---
phase: 01-normalize-event-contract
plan: 03
subsystem: integration
tags: [ctrader, dotnet, contract, envelope, docs]
requires:
  - phase: 01-01
    provides: event-contract/v1 document and canonical event taxonomy
provides:
  - Shared v1 envelope emission for all three cTrader exporters
  - Explicit wyckoff_state event naming for WeisWyckoffSystemV20
  - cTrader-to-contract mapping reference with unsupported edges documented
affects: [python-ingest, phase-02-transport, build-verification]
tech-stack:
  added: []
  patterns: [canonical exporter envelope, explicit unsupported-edge documentation]
key-files:
  created: [docs/ctrader-contract-alignment-v1.md]
  modified: [ctrader-projects/OrderFlowAggregatedV20/src/Order Flow Aggregated v2.0.cs, ctrader-projects/FreeVolumeProfileV20/src/Free Volume Profile v2.0.cs, ctrader-projects/WeisWyckoffSystemV20/src/Weis & Wyckoff System v2.0.cs]
key-decisions:
  - "Each cTrader exporter now emits the same top-level envelope while keeping exporter-specific analytics inside payload."
  - "Weis/Wyckoff no longer relies on the analyzer to infer its event type; it emits wyckoff_state directly."
patterns-established:
  - "Exporter pattern: build canonical envelope fields centrally, keep raw producer identity in source_meta."
  - "Boundary documentation pattern: document unsupported edges explicitly instead of implying parity between producer families."
requirements-completed: [DATA-03]
duration: 11min
completed: 2026-03-20
---

# Phase 01: Normalize Event Contract Summary

**cTrader exporters aligned to event-contract/v1 with explicit wyckoff_state events and documented unsupported edges**

## Performance

- **Duration:** 11 min
- **Started:** 2026-03-20T22:08:20+07:00
- **Completed:** 2026-03-20T22:18:55+07:00
- **Tasks:** 3
- **Files modified:** 4

## Accomplishments
- Wrapped `OrderFlowAggregatedV20` and `FreeVolumeProfileV20` exports in the shared v1 envelope with `payload` and `source_meta`.
- Refactored `WeisWyckoffSystemV20` to emit explicit `wyckoff_state` events instead of relying on missing legacy type inference.
- Added `docs/ctrader-contract-alignment-v1.md` to map each exporter to its canonical event and call out unsupported v1 edges clearly.

## Task Commits

Each task was committed atomically:

1. **Task 1: Wrap OrderFlowAggregated and FreeVolumeProfile payloads in the shared envelope** - `c880fc9` (feat)
2. **Task 2: Convert Weis/Wyckoff export to explicit `wyckoff_state` events** - `32066cf` (feat)
3. **Task 3: Document cTrader mappings and rebuild exporter projects** - `855b90c` (docs)

**Plan metadata:** pending in working tree

## Files Created/Modified
- `docs/ctrader-contract-alignment-v1.md` - Documents canonical cTrader event mapping, payload ownership, and unsupported edges.
- `ctrader-projects/OrderFlowAggregatedV20/src/Order Flow Aggregated v2.0.cs` - Emits `order_flow_aggregated` through the shared v1 envelope.
- `ctrader-projects/FreeVolumeProfileV20/src/Free Volume Profile v2.0.cs` - Emits `volume_profile` through the shared v1 envelope.
- `ctrader-projects/WeisWyckoffSystemV20/src/Weis & Wyckoff System v2.0.cs` - Emits explicit `wyckoff_state` events with canonical envelope metadata.

## Decisions Made
- Kept analytics such as `vpPOC`, `profile_type`, `waveVolume`, and `waveDirection` nested under `payload` so the envelope stays stable across producers.
- Documented unsupported edges explicitly rather than implying Bookmap and cTrader produce identical microstructure and profile metrics.

## Deviations from Plan

None in the implementation. The only outstanding issue is environment verification.

## Issues Encountered

- `Build-CTraderProjects.ps1` fails on this host before compilation because the installed .NET 10 SDK is missing workload resolver SDK folders (`Microsoft.NET.SDK.WorkloadAutoImportPropsLocator` and `Microsoft.NET.SDK.WorkloadManifestTargetsLocator`). This is an environment/toolchain problem, not a contract-code compile error.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- cTrader exporters now align with the same event taxonomy consumed by the Python boundary.
- Remaining verification for this plan is environmental: rerun `powershell -ExecutionPolicy Bypass -File .\ctrader-projects\Build-CTraderProjects.ps1` on a host with a working .NET/cTrader build toolchain.

---
*Phase: 01-normalize-event-contract*
*Completed: 2026-03-20*
