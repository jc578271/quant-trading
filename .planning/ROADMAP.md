# Roadmap: Quant Trading Workspace

## Overview

This roadmap turns the existing trading workspace into a coherent local pipeline in stages: first make the event contract explicit, then harden transport and observability, then stabilize signal state, then add safe execution controls, and finally lock the workflow down with repeatable build/test coverage.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

- [x] **Phase 1: Normalize Event Contract** - Make Bookmap/Python/cTrader pipeline inputs explicit and consistent (completed 2026-03-20)
- [ ] **Phase 2: Harden Transport & Observability** - Make the local pipeline reconnectable and diagnosable
- [ ] **Phase 3: Stabilize Signal State** - Turn asynchronous records into explainable per-symbol state
- [ ] **Phase 4: Add Safe Execution Controls** - Gate live execution behind validated inputs and explicit modes
- [ ] **Phase 5: Lock In Build & Verification** - Add repeatable build/smoke/regression workflow for the integrated path

## Phase Details

### Phase 1: Normalize Event Contract
**Goal**: Define and enforce one local event contract for the integration pipeline so producers and consumers stop drifting.
**Depends on**: Nothing (first phase)
**Requirements**: [DATA-01, DATA-02, DATA-03]
**Success Criteria** (what must be TRUE):
  1. The Bookmap -> Python payload shapes used in v1 are documented and versioned in the repo
  2. The Python ingest loop rejects malformed payloads visibly instead of failing implicitly
  3. The role of cTrader-derived context in the pipeline is explicitly mapped or explicitly deferred
**Plans**: 3 plans

Plans:
- [x] 01-01: Audit current payload producers and capture the normalized event schema
- [x] 01-02: Patch ingest-side validation and obvious contract breakages in the Python bridge
- [x] 01-03: Document cTrader alignment points and unsupported contract edges

### Phase 2: Harden Transport & Observability
**Goal**: Make the local pipeline survive startup ordering, reconnects, and operational troubleshooting.
**Depends on**: Phase 1
**Requirements**: [PIPE-01, PIPE-02, PIPE-03]
**Success Criteria** (what must be TRUE):
  1. Producers and consumers can be restarted independently without permanently breaking the pipeline
  2. Operators can tell which stage is up, down, or degraded without reading raw source code
  3. Output files and logs follow predictable naming and schema rules
**Plans**: 3 plans

Plans:
- [ ] 02-01: Improve socket lifecycle and reconnection behavior across Java and Python endpoints
- [ ] 02-02: Add health/status signaling for ingest, buffering, inference, and execution stages
- [ ] 02-03: Standardize runtime artifacts, file naming, and operational docs

### Phase 3: Stabilize Signal State
**Goal**: Make signal formation deterministic enough to inspect, replay, and trust.
**Depends on**: Phase 2
**Requirements**: [SIGN-01, SIGN-02]
**Success Criteria** (what must be TRUE):
  1. Per-symbol state updates remain coherent when records arrive asynchronously
  2. Captured sessions can be replayed or inspected to explain a generated signal
  3. Known ingest-state bugs in the active pipeline path are removed or isolated
**Plans**: 2 plans

Plans:
- [ ] 03-01: Refactor and harden Python state assembly for asynchronous records
- [ ] 03-02: Add replay/inspection workflow for captured sessions and signal explanations

### Phase 4: Add Safe Execution Controls
**Goal**: Make execution intent explicit and block unsafe or ambiguous order placement.
**Depends on**: Phase 3
**Requirements**: [EXEC-01, EXEC-02]
**Success Criteria** (what must be TRUE):
  1. MT5 execution validates risk, size, and SL/TP inputs before placement
  2. Users can clearly distinguish simulation-only behavior from live-eligible behavior
  3. Failed execution attempts are visible and diagnosable
**Plans**: 2 plans

Plans:
- [ ] 04-01: Harden execution-side validation and failure reporting in the Python/MT5 bridge
- [ ] 04-02: Introduce explicit execution modes and operator-facing safeguards

### Phase 5: Lock In Build & Verification
**Goal**: Ensure the integrated path can be rebuilt, smoke-tested, and regression-checked repeatedly.
**Depends on**: Phase 4
**Requirements**: [OPS-01, OPS-02]
**Success Criteria** (what must be TRUE):
  1. Developers have one repeatable workflow for building the relevant Java, Python, and cTrader components
  2. Highest-risk payload, buffering, and execution-guard paths have regression coverage
  3. The next person can validate the pipeline without rediscovering the workflow from scratch
**Plans**: 2 plans

Plans:
- [ ] 05-01: Create repeatable build/smoke workflow for the integrated pipeline
- [ ] 05-02: Add regression coverage for contract parsing, state buffering, and execution guards

## Progress

**Execution Order:**
Phases execute in numeric order: 1 -> 2 -> 3 -> 4 -> 5

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Normalize Event Contract | 3/3 | Complete | 2026-03-20 |
| 2. Harden Transport & Observability | 0/3 | Not started | - |
| 3. Stabilize Signal State | 0/2 | Not started | - |
| 4. Add Safe Execution Controls | 0/2 | Not started | - |
| 5. Lock In Build & Verification | 0/2 | Not started | - |
