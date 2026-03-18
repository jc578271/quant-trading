# Phase 1: Normalize Event Contract - Context

**Gathered:** 2026-03-18
**Status:** Ready for planning

<domain>
## Phase Boundary

Define and enforce one local event contract for the integration pipeline so Bookmap producers, the three existing cTrader exporters, and the Python ingest path stop drifting. This phase covers contract shape, event coverage, identity rules, and documentation of supported/unsupported event forms; it does not expand the pipeline with new capabilities beyond normalizing what already exists.

</domain>

<decisions>
## Implementation Decisions

### Contract coverage
- v1 uses one shared envelope across both Bookmap and cTrader producers
- All currently active socket-producing sources are in scope for v1:
  - `bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java`
  - `ctrader-projects/OrderFlowAggregatedV20/src/Order Flow Aggregated v2.0.cs`
  - `ctrader-projects/FreeVolumeProfileV20/src/Free Volume Profile v2.0.cs`
  - `ctrader-projects/WeisWyckoffSystemV20/src/Weis & Wyckoff System v2.0.cs`
- cTrader is not treated as a later compatibility layer; its existing exporters must align with the canonical contract in this phase

### Envelope structure
- Use a traceable envelope rather than a minimal or legacy-flat schema
- Envelope should carry canonical metadata such as versioning, source, event type, timestamp, and trace/debug fields
- Payloads are event-shaped: each event gets its own contract, while still allowing source-specific fields when needed
- Canonicalization effort in v1 should focus on the envelope plus well-defined event payloads, not on forcing all producers into one identical domain payload

### Instrument identity
- Every event should expose a canonical `instrument` field
- Raw source identity fields such as `alias` and `symbol` must be preserved for debugging and traceability
- Canonical `instrument` should prefer the raw venue-qualified identity in v1 (example: `ESH6.CME@RITHMIC`)
- If an event cannot be mapped to a valid canonical `instrument`, quarantine it for investigation rather than letting it continue through the signal pipeline

### Claude's Discretion
- Exact top-level field names for the traceable envelope (for example `schema`, `event_id`, `source_instance`, `received_at`)
- Exact event taxonomy and naming rules as long as they remain consistent and documented
- How quarantine is implemented operationally (separate file, folder, or structured log) so long as ambiguous events do not enter the analyzer path
- Validation strictness for optional/source-specific payload fields, provided required envelope fields remain explicit and enforced

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project and phase scope
- `.planning/PROJECT.md` - overall project framing, constraints, and the local-first integration goal
- `.planning/REQUIREMENTS.md` - Phase 1 requirements `DATA-01`, `DATA-02`, and `DATA-03`
- `.planning/ROADMAP.md` - Phase 1 boundary, goals, and success criteria
- `.planning/STATE.md` - current blockers affecting discussion and planning

### Existing workspace context
- `.planning/codebase/STRUCTURE.md` - where the current producers and consumer live
- `.planning/codebase/INTEGRATIONS.md` - current socket transport, local file outputs, and external runtime assumptions
- `.planning/codebase/CONCERNS.md` - known pipeline issues, especially the Python ingest bug and integration fragility

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java`: already emits multiple event families (`alert`, `dom`, `dot`, `wall`) over the local socket and has CSV-oriented normalization helpers
- `main/socket_server.py`: already provides newline-delimited JSON transport and a single callback entry into the analyzer
- `main/ai_analyzer.py`: already branches on `type`, persists event history, and contains the first consumer expectations that the contract must satisfy
- `ctrader-projects/*/src/*.cs` exporters: already serialize dictionaries to JSON and send them to the same socket, giving an immediate set of cTrader producers to normalize

### Established Patterns
- Transport is newline-delimited JSON over `127.0.0.1:5555`
- Bookmap events are currently source-typed with small flat payloads (`alert`, `dom`, `dot`, `wall`)
- cTrader exporters currently send richer analytics payloads such as `order_flow_aggregated` and `volume_profile`, while `WeisWyckoffSystemV20` currently omits a `type` field in its payload builder
- Python consumer logic currently keys heavily off `data.get("type")`, `symbol`, and `timestamp`, and falls back heuristically when `type` is absent

### Integration Points
- Producer-side changes will center on `AlertListener.java` and the three cTrader `BuildExportPayload` / socket send paths
- Consumer-side contract enforcement will center on `main/socket_server.py` and `main/ai_analyzer.py`
- Phase 1 planning should explicitly account for the existing `self.order_book` bug in `main/ai_analyzer.py` because DOM events are already part of the in-scope contract

</code_context>

<specifics>
## Specific Ideas

- Shared envelope first, not a Bookmap-only contract
- Preserve raw source identity while adding a canonical `instrument`
- Prefer venue-qualified identities in v1 rather than inventing a mapping table too early
- Quarantine ambiguous identity cases instead of best-effort forwarding

</specifics>

<deferred>
## Deferred Ideas

None - discussion stayed within phase scope

</deferred>

---

*Phase: 01-normalize-event-contract*
*Context gathered: 2026-03-18*
