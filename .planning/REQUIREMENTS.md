# Requirements: Quant Trading Workspace

**Defined:** 2026-03-18
**Core Value:** Market data and order-flow events move through the local pipeline into trustworthy, verifiable trading actions without manual glue code or silent failure.

## v1 Requirements

### Data Contract

- [ ] **DATA-01**: Bookmap producers emit a documented JSON contract for alert, dot, wall, and DOM payloads over the local pipeline
- [ ] **DATA-02**: The Python bridge validates malformed or version-mismatched payloads without crashing the ingest loop
- [ ] **DATA-03**: cTrader-originated context can either map into the same event contract or be explicitly documented as unsupported in v1

### Pipeline Reliability

- [ ] **PIPE-01**: The producer and consumer processes can start independently and reconnect automatically without manual file edits
- [ ] **PIPE-02**: Operators can see whether ingest, buffering, inference, and execution stages are currently healthy
- [ ] **PIPE-03**: Pipeline outputs write predictable files with stable schemas for later replay and diagnosis

### Signal Processing

- [ ] **SIGN-01**: The Python analyzer maintains a stable per-symbol state when records arrive asynchronously from the local pipeline
- [ ] **SIGN-02**: A captured session can be replayed or inspected to explain why a signal or trade decision happened

### Execution Controls

- [ ] **EXEC-01**: MT5 order submission uses validated risk, lot, stop-loss, and take-profit inputs before attempting live placement
- [ ] **EXEC-02**: Operators can clearly distinguish simulated behavior from live-eligible execution behavior

### Tooling & Verification

- [ ] **OPS-01**: The workspace has a repeatable build/smoke-test path for the Bookmap, Python, and cTrader components that participate in the pipeline
- [ ] **OPS-02**: Regression checks cover the highest-risk transformations in the integration path (payload parsing, buffering, and execution guards)

## v2 Requirements

### Operations

- **OPER-01**: Operators get a lightweight dashboard or status UI for the whole local pipeline
- **OPER-02**: The system archives replay-ready sessions automatically with metadata and run summaries

### Strategy Expansion

- **STRAT-01**: Additional strategy modules can plug into the normalized event contract without rewriting transport code
- **STRAT-02**: The pipeline supports backtesting/benchmarking of signal logic against captured sessions

## Out of Scope

| Feature | Reason |
|---------|--------|
| Cloud deployment or hosted APIs | The current project is desktop-local and vendor-runtime dependent |
| New standalone indicator families unrelated to the pipeline | Existing indicators are inputs/context, not the milestone target |
| Multi-broker execution abstraction | Current execution focus is MT5 and existing local tooling only |
| Portfolio/risk platform features beyond per-trade controls | Would explode scope before the core pipeline is reliable |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| DATA-01 | Phase 1 | Pending |
| DATA-02 | Phase 1 | Pending |
| DATA-03 | Phase 1 | Pending |
| PIPE-01 | Phase 2 | Pending |
| PIPE-02 | Phase 2 | Pending |
| PIPE-03 | Phase 2 | Pending |
| SIGN-01 | Phase 3 | Pending |
| SIGN-02 | Phase 3 | Pending |
| EXEC-01 | Phase 4 | Pending |
| EXEC-02 | Phase 4 | Pending |
| OPS-01 | Phase 5 | Pending |
| OPS-02 | Phase 5 | Pending |

**Coverage:**
- v1 requirements: 12 total
- Mapped to phases: 12
- Unmapped: 0 ✓

---
*Requirements defined: 2026-03-18*
*Last updated: 2026-03-18 after initial definition*
