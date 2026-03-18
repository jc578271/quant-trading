# Phase 1: Normalize Event Contract - Research

**Researched:** 2026-03-18
**Domain:** Local event-contract normalization across Bookmap, Python, and cTrader
**Confidence:** HIGH

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- v1 uses one shared envelope across both Bookmap and cTrader producers.
- All currently active socket-producing sources are in scope for v1:
  - `bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java`
  - `ctrader-projects/OrderFlowAggregatedV20/src/Order Flow Aggregated v2.0.cs`
  - `ctrader-projects/FreeVolumeProfileV20/src/Free Volume Profile v2.0.cs`
  - `ctrader-projects/WeisWyckoffSystemV20/src/Weis & Wyckoff System v2.0.cs`
- cTrader is not a later compatibility layer; the existing exporters must align in this phase.
- Use a traceable envelope rather than a minimal or legacy-flat schema.
- Payloads are event-shaped: each event gets its own contract while still allowing source-specific fields when needed.
- Every event exposes a canonical `instrument` field.
- Raw source identity fields such as `alias` and `symbol` must be preserved.
- Canonical `instrument` should prefer raw venue-qualified identity in v1.
- If an event cannot be mapped to a valid canonical `instrument`, quarantine it instead of letting it continue through the signal pipeline.

### Claude's Discretion
- Exact top-level field names for the traceable envelope.
- Exact event taxonomy and naming rules as long as they stay consistent and documented.
- The quarantine implementation as long as ambiguous events never enter the analyzer path.
- Validation strictness for optional and source-specific payload fields, provided required envelope fields are enforced.

### Deferred Ideas (OUT OF SCOPE)
- None - discussion stayed within phase scope.
</user_constraints>

<research_summary>
## Summary

Phase 1 is less about inventing new transport and more about stopping four existing producers plus one Python consumer from encoding the same trading events in incompatible shapes. The stable pattern for a mixed-language local pipeline is to define one versioned envelope, keep source-specific payloads under that envelope, and centralize normalization and rejection at the ingest boundary before downstream strategy logic sees anything.

The current workspace already points to the failure modes the plan must avoid: Bookmap emits flat ad-hoc JSON without guaranteed timestamps on all event families, cTrader exporters emit rich analytics but not the same top-level keys, and `main/ai_analyzer.py` branches directly on `type` while also crashing on DOM because `self.order_book` is never initialized. The practical recommendation is to make the envelope explicit in repo docs, teach producers to emit it, and add one Python normalizer that can both validate v1 messages and safely wrap known legacy messages during rollout.

**Primary recommendation:** Standardize on a shared envelope with producer-required fields `schema`, `source`, `source_instance`, `event`, `event_id`, `instrument`, `timestamp`, `payload`, and `source_meta`, then enforce it at `main/socket_server.py` before `AIAnalyzer` touches the data.
</research_summary>

<standard_stack>
## Standard Stack

The established tools already present in this workspace are enough for Phase 1; the gap is contract discipline, not missing frameworks.

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Java Bookmap addon runtime | Workspace-managed | Emit Bookmap alert, DOM, dot, and wall events | Existing producer path; Phase 1 should refine this path instead of replacing it |
| Python 3.x + stdlib `json`/`logging` | Workspace-managed | Ingest, validate, quarantine, and forward socket events | Current ingest boundary and the best place to enforce one contract |
| `scikit-learn` + `joblib` | Workspace-managed | Existing AI model load/predict path | Downstream consumer already depends on this state and must not be broken by contract work |
| .NET / cTrader `System.Text.Json` | Workspace-managed | Emit cTrader analytics payloads | Existing exporters already serialize dictionaries cleanly and can wrap payloads without new dependencies |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Local TCP socket `127.0.0.1:5555` | Existing pattern | Transport newline-delimited JSON | Keep for this phase; Phase 2 can harden lifecycle and reconnect behavior |
| CSV history exports | Existing pattern | Persist raw event history per event family | Keep for observability, but do not treat CSV layout as the source of truth |
| JSONL quarantine log | New lightweight artifact | Preserve rejected payloads with reasons | Use for malformed or ambiguous records that must not reach analyzer state |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| One shared envelope + event-shaped payloads | One giant unified payload for every event | Too much forced normalization too early across very different sources |
| Boundary normalizer in Python | Duplicated validation inside every producer and analyzer branch | Increases drift and makes mixed rollout harder |
| Quarantine on invalid records | Best-effort forwarding with warnings | Keeps the pipeline moving, but lets ambiguous data contaminate signals |
</standard_stack>

<architecture_patterns>
## Architecture Patterns

### Recommended Project Structure
```
docs/
├── event-contract-v1.md
└── ctrader-contract-alignment-v1.md

main/
├── event_contract.py
├── socket_server.py
└── ai_analyzer.py
```

### Pattern 1: Envelope first, payload second
**What:** Put cross-cutting metadata at the top level and keep domain specifics under `payload`.
**When to use:** Any time multiple producers emit related but not identical event families.
**Example:**
```json
{
  "schema": "event-contract/v1",
  "source": "bookmap",
  "source_instance": "AlertListener",
  "event": "dom",
  "event_id": "bookmap-dom-ESH6.CME@RITHMIC-2026-03-18T21:00:00Z",
  "instrument": "ESH6.CME@RITHMIC",
  "timestamp": "2026-03-18T21:00:00Z",
  "payload": {
    "action": "added",
    "isBid": true,
    "price": 6000,
    "size": 25
  },
  "source_meta": {
    "alias": "ESH6.CME@RITHMIC"
  }
}
```

### Pattern 2: Normalize at the ingest boundary
**What:** `main/socket_server.py` should validate and normalize once, then pass only canonical envelopes to `AIAnalyzer`.
**When to use:** Mixed-language pipelines where producer rollout is incremental.
**Example:**
```python
normalized, error = normalize_record(record, received_at=received_at)
if error:
    quarantine_record(raw=record, reason=error, received_at=received_at)
else:
    callback(normalized)
```

### Pattern 3: Preserve raw identity and payload context
**What:** Keep `alias`, `symbol`, `profile_type`, and similar source-native fields in `source_meta` or `payload`.
**When to use:** When downstream debugging or replay needs the source-native view as well as the canonical one.
**Example:**
```python
instrument = envelope["instrument"]
raw_symbol = envelope["source_meta"].get("symbol")
raw_alias = envelope["source_meta"].get("alias")
```

### Anti-Patterns to Avoid
- **Top-level field drift:** Letting some events use `type`, some use `event`, and some omit both recreates the current failure mode.
- **Analyzer-owned normalization:** `AIAnalyzer` should consume normalized events, not guess producer intent from half-structured dictionaries.
- **Dropping invalid records silently:** Errors must become logs and quarantine artifacts, not missing trades or unexplained gaps.
</architecture_patterns>

<dont_hand_roll>
## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Contract governance | Per-file ad-hoc JSON strings with different keys | One versioned doc plus one Python normalizer | Mixed-language drift is the main failure source today |
| Identity recovery | Heuristics spread across analyzer branches | Single `extract_instrument()` rule | Avoids different symbols for the same event family |
| Malformed-event handling | `try/except` that only logs parse failure | JSONL quarantine with explicit rejection reason | Keeps forensic evidence and protects downstream state |

**Key insight:** The hard part here is not JSON serialization; it is preventing five code paths from teaching the pipeline five different truths about what an event is.
</dont_hand_roll>

<common_pitfalls>
## Common Pitfalls

### Pitfall 1: Silent schema drift
**What goes wrong:** Producers keep adding or renaming fields and the Python side starts depending on whichever arrived last.
**Why it happens:** There is no canonical contract artifact and no validation gate before analyzer code.
**How to avoid:** Version the envelope, document per-event payloads, and route every record through one normalizer.
**Warning signs:** Branches like `data.get("type", "indicator")` or missing fallbacks for one exporter.

### Pitfall 2: Identity mismatch between `alias`, `symbol`, and `instrument`
**What goes wrong:** Records from the same instrument land in different buffers or CSVs because each source uses a different identity field.
**Why it happens:** Source-native field names leak into business logic.
**How to avoid:** Require canonical `instrument` and preserve raw identity separately under `source_meta`.
**Warning signs:** Buffers keyed by `symbol` for some events and `alias` for others.

### Pitfall 3: Quarantine becomes a black hole
**What goes wrong:** Invalid events stop reaching the analyzer, but nobody can tell why or how often.
**Why it happens:** Rejection paths log too little context or overwrite previous evidence.
**How to avoid:** Append JSONL records with `received_at`, `reason`, and the original payload.
**Warning signs:** Repeated "Malformed JSON" style errors without the event family or instrument in logs.
</common_pitfalls>

<code_examples>
## Code Examples

### Canonical event taxonomy
```text
Bookmap: alert, dom, dot, wall
cTrader: order_flow_aggregated, volume_profile, wyckoff_state
```

### Minimal normalizer contract
```python
SCHEMA_VERSION = "event-contract/v1"
SUPPORTED_EVENTS = {
    "alert",
    "dom",
    "dot",
    "wall",
    "order_flow_aggregated",
    "volume_profile",
    "wyckoff_state",
}
```

### Quarantine record shape
```json
{
  "received_at": "2026-03-18T21:00:00Z",
  "reason": "missing instrument",
  "raw": { "...": "original payload" }
}
```
</code_examples>

## Validation Architecture

Phase 1 spans Java, Python, and C#, so validation should sample the narrowest useful checks after each change rather than waiting for one big end-to-end run.

- Use `python -m compileall main` as the fastest feedback loop after consumer-side changes.
- Use `powershell -ExecutionPolicy Bypass -File ".\\ctrader-projects\\Build-CTraderProjects.ps1"` after cTrader exporter changes.
- Use `$env:JAVA_HOME = "C:\\Program Files\\Bookmap\\jre"; .\\gradlew.bat alertListenerJar` from `bookmap-addons` after AlertListener changes.
- Require one manual smoke pass with one accepted event and one quarantined event before declaring the phase complete.

<open_questions>
## Open Questions

1. **Should producers or the Python boundary own `received_at`?**
   - What we know: Producers can stamp `timestamp`, but only the socket server knows when the line was actually received.
   - What's unclear: None at the contract level.
   - Recommendation: Treat `received_at` as consumer-populated metadata, not a producer-required field.

2. **How much legacy compatibility should remain after all producers are aligned?**
   - What we know: A temporary legacy wrapper is useful during rollout.
   - What's unclear: Whether Phase 1 execution should remove legacy fallbacks immediately after producer updates.
   - Recommendation: Keep compatibility for the known legacy flat shapes in Phase 1, then tighten in Phase 2 once restarts and rollout sequencing are more predictable.
</open_questions>

<sources>
## Sources

### Primary (HIGH confidence)
- `D:\projects\quant-trading\.planning\phases\01-normalize-event-contract\01-CONTEXT.md`
- `D:\projects\quant-trading\bookmap-addons\src\main\java\com\bookmap\alertlistener\AlertListener.java`
- `D:\projects\quant-trading\main\socket_server.py`
- `D:\projects\quant-trading\main\ai_analyzer.py`
- `D:\projects\quant-trading\ctrader-projects\OrderFlowAggregatedV20\src\Order Flow Aggregated v2.0.cs`
- `D:\projects\quant-trading\ctrader-projects\FreeVolumeProfileV20\src\Free Volume Profile v2.0.cs`
- `D:\projects\quant-trading\ctrader-projects\WeisWyckoffSystemV20\src\Weis & Wyckoff System v2.0.cs`

### Secondary (MEDIUM confidence)
- `D:\projects\quant-trading\.planning\ROADMAP.md`
- `D:\projects\quant-trading\.planning\REQUIREMENTS.md`
- GitNexus repo metadata for `quant-trading`; graph queries remain blocked by missing `.gitnexus\kuzu`

### Tertiary (LOW confidence - needs validation)
- None - this research is grounded in local source and already-discussed constraints.
</sources>

<metadata>
## Metadata

**Research scope:**
- Core technology: mixed-language local event ingestion
- Ecosystem: Bookmap Java addon, Python analyzer, cTrader exporters
- Patterns: versioned envelopes, boundary normalization, quarantine logging
- Pitfalls: schema drift, identity mismatch, silent rejection

**Confidence breakdown:**
- Standard stack: HIGH - based on current workspace components
- Architecture: HIGH - directly derived from the actual producer and consumer paths
- Pitfalls: HIGH - already visible in current code
- Code examples: HIGH - scoped to the exact Phase 1 target state

**Research date:** 2026-03-18
**Valid until:** 2026-04-17
</metadata>

---

*Phase: 01-normalize-event-contract*
*Research completed: 2026-03-18*
*Ready for planning: yes*
