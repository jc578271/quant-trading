# Phase 2: Harden Transport & Observability - Context

**Gathered:** 2026-03-21
**Status:** Ready for planning

<domain>
## Phase Boundary

Make the existing local Bookmap/cTrader -> Python pipeline survive startup ordering, reconnects, and operator troubleshooting without adding new trading features. This phase covers reconnect behavior, pipeline health visibility, degraded/disconnected handling, and predictable runtime artifact layout for the current local desktop workflow.

</domain>

<decisions>
## Implementation Decisions

### Reconnect Policy
- Producers should auto-retry forever when Python is unavailable or the socket drops
- Runtime logging for reconnect behavior should be state-change only, not per retry attempt
- Every successful reconnect must resend `connection_hello` so Python can re-identify the producer explicitly

### Health / Status Surface
- Python owns the source-of-truth health surface for this phase
- Health should be reported per pipeline stage, not per implementation file or per producer process
- Stage states are limited to `up`, `degraded`, and `down`
- Initial stage set is:
  - `ingest`
  - `buffering`
  - `inference`
  - `execution`

### Degraded Behavior While Disconnected
- Realtime events should be dropped immediately while disconnected; do not buffer for replay on reconnect
- Dropped events should not generate per-event logs
- Dropped-event visibility should exist only as counters in the Python status file
- Drop counters should be cumulative for the whole session rather than resetting after each recovery

### Status File Shape
- The health/status file format should be `JSON`
- The file should update on both heartbeat and state change
- Top-level metadata should stay minimal:
  - `updated_at`
  - `session_started_at`
- Each stage should expose at least:
  - `state`
  - `updated_at`
  - `reason`

### Runtime Artifact Layout
- Runtime artifacts for this phase should live under one repo-root folder: `runtime/`
- The layout should use flat stable files rather than nested subfolders or per-session directories
- The runtime layout is for predictable inspection and troubleshooting first; archival/session rotation is not part of this phase unless required by the chosen stable naming scheme

### Claude's Discretion
- Exact heartbeat interval and staleness thresholds that move a stage from `up` to `degraded` or `down`
- Exact stable filenames under `runtime/`, as long as they remain flat, predictable, and aligned with the decisions above
- Internal plumbing that maps producer/process signals into the four pipeline-facing stages

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project Scope and Phase Requirements
- `.planning/PROJECT.md` - project framing, local-first constraints, and reliability-over-expansion priority
- `.planning/REQUIREMENTS.md` - Phase 2 requirements `PIPE-01`, `PIPE-02`, and `PIPE-03`
- `.planning/ROADMAP.md` - Phase 2 boundary, goals, and success criteria
- `.planning/STATE.md` - current project position and active focus on Phase 2

### Prior Decisions That Carry Forward
- `.planning/phases/01-normalize-event-contract/01-CONTEXT.md` - canonical envelope, explicit source identity, and quarantine behavior established in Phase 1

### External Specs
- No external specs were referenced during discussion; requirements are fully captured in the planning docs above

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `main/socket_server.py`: already accepts `connection_hello`, tracks client identity, writes quarantine output, and records socket event taps
- `bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java`: already supports socket reconnect UI, `connection_hello`, runtime CSV/log output, and configurable CSV path handling
- `ctrader-projects/OrderFlowAggregatedV20/src/Order Flow Aggregated v2.0.Lifecycle.cs`: already has reconnect control and producer handshake path
- `ctrader-projects/FreeVolumeProfileV20/src/Free Volume Profile v2.0.Lifecycle.cs`: already has reconnect control and producer handshake path
- `ctrader-projects/WeisWyckoffSystemV20/src/Weis & Wyckoff System v2.0.Lifecycle.cs`: already has reconnect control and producer handshake path

### Established Patterns
- Transport is newline-delimited JSON over localhost
- Producer identity is already explicit through `source`, `source_instance`, and `connection_hello`
- Python already acts as the natural central observer for ingest-side health and runtime file emission
- Operators currently infer health from logs; this phase formalizes that into a status file and stable runtime artifacts

### Integration Points
- Socket lifecycle and health state transitions will center on `main/socket_server.py` and adjacent Python pipeline components
- Bookmap reconnect/drop behavior will continue through `AlertListener.java`
- cTrader reconnect/drop behavior will continue through the three lifecycle/exporter paths
- Existing runtime outputs such as quarantine and socket event taps should be normalized into the new `runtime/` layout rather than duplicated inconsistently

</code_context>

<specifics>
## Specific Ideas

- Status is for operators first, not just for developers reading code
- Reconnect should be automatic enough that startup order stops mattering operationally
- Degradation should stay visible without flooding logs
- Runtime files should be easy to inspect directly from the repo root during live troubleshooting

</specifics>

<deferred>
## Deferred Ideas

- Richer status UI or dashboard work belongs to later operational scope (`OPER-01`)
- Session archival/rotation beyond stable live runtime files belongs to later replay/operations scope unless planning proves a minimal hook is required in Phase 2

</deferred>

---

*Phase: 02-harden-transport-observability*
*Context gathered: 2026-03-21*
