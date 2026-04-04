# Requirements: Quant Trading Workspace

**Defined:** 2026-03-18
**Core Value:** Market data and order-flow events move through the local pipeline into trustworthy, verifiable trading actions without manual glue code or silent failure.

## v1 Requirements

### Data Contract

- [x] **DATA-01**: Bookmap producers emit a documented JSON contract for alert, dot, wall, and DOM payloads over the local pipeline
- [x] **DATA-02**: The Python bridge validates malformed or version-mismatched payloads without crashing the ingest loop
- [x] **DATA-03**: cTrader-originated context can either map into the same event contract or be explicitly documented as unsupported in v1

### Pipeline Reliability

- [x] **PIPE-01**: The producer and consumer processes can start independently and reconnect automatically without manual file edits
- [x] **PIPE-02**: Operators can see whether ingest, buffering, inference, and execution stages are currently healthy
- [x] **PIPE-03**: Pipeline outputs write predictable files with stable schemas for later replay and diagnosis

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

### Model Training

- [ ] **MLDATA-01**: Raw JSONL logs are cataloged and normalized into a documented deterministic training dataset contract
- [ ] **MLDATA-02**: Dataset building enforces explicit scope, instrument compatibility, canonical instrument mapping, and backward-only joins
- [ ] **MLLABEL-01**: Labels are generated from local historical price movement without future leakage
- [ ] **MLTRAIN-01**: Offline training produces versioned artifacts and metrics from local data only
- [ ] **MLTRAIN-02**: Promotion into `runtime/model.pkl` is explicit and separate from training
- [ ] **MLEVAL-01**: Walk-forward evaluation outputs are reproducible enough to replay and compare safely

### OHLC Training Data Export

- [x] **OHLC-01**: Indicator exports OHLC bar data as event-contract/v1 JSONL with configurable date range
- [x] **OHLC-02**: Date input follows existing LoadTickFrom pattern (Today/Yesterday/One_Week/Custom dd/MM/yyyy)
- [x] **OHLC-03**: Socket export streams live OHLC events to 127.0.0.1:5555 with heartbeat and reconnect
- [x] **OHLC-04**: Split-file project structure matches existing cTrader indicator conventions

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
| DATA-01 | Phase 1 | Complete |
| DATA-02 | Phase 1 | Complete |
| DATA-03 | Phase 1 | Complete |
| PIPE-01 | Phase 2 | Complete |
| PIPE-02 | Phase 2 | Complete |
| PIPE-03 | Phase 2 | Complete |
| SIGN-01 | Phase 3 | Pending |
| SIGN-02 | Phase 3 | Pending |
| EXEC-01 | Phase 4 | Pending |
| EXEC-02 | Phase 4 | Pending |
| OPS-01 | Phase 5 | Pending |
| OPS-02 | Phase 5 | Pending |
| MLDATA-01 | Phase 6 | Pending |
| MLDATA-02 | Phase 6 | Pending |
| MLLABEL-01 | Phase 6 | Pending |
| MLTRAIN-01 | Phase 6 | Pending |
| MLTRAIN-02 | Phase 6 | Pending |
| MLEVAL-01 | Phase 6 | Pending |
| OHLC-01 | Phase 7 | Complete |
| OHLC-02 | Phase 7 | Complete |
| OHLC-03 | Phase 7 | Complete |
| OHLC-04 | Phase 7 | Complete |

**Coverage:**
- v1 requirements: 16 total
- Mapped to phases: 16
- Unmapped: 0 ✓

---
*Requirements defined: 2026-03-18*
*Last updated: 2026-04-04 after Phase 7 execution*
