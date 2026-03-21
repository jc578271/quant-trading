# Phase 02: Harden Transport & Observability - Research

**Researched:** 2026-03-22
**Domain:** Local TCP transport hardening, pipeline health/status, runtime artifact normalization
**Confidence:** MEDIUM

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

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

### Deferred Ideas (OUT OF SCOPE)
- Richer status UI or dashboard work belongs to later operational scope (`OPER-01`)
- Session archival/rotation beyond stable live runtime files belongs to later replay/operations scope unless planning proves a minimal hook is required in Phase 2
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| PIPE-01 | The producer and consumer processes can start independently and reconnect automatically without manual file edits | Use explicit producer connection state machines, forever retry with backoff, resend `connection_hello` on every reconnect, and drop realtime events while disconnected instead of buffering |
| PIPE-02 | Operators can see whether ingest, buffering, inference, and execution stages are currently healthy | Use Python as the only status publisher, publish a flat `runtime/status.json`, and evaluate stage health from deadlines, counters, and last-known reasons |
| PIPE-03 | Pipeline outputs write predictable files with stable schemas for later replay and diagnosis | Introduce one runtime-path module, flat stable filenames under `runtime/`, atomic status writes, and documented JSONL/CSV schema ownership per artifact |
</phase_requirements>

## Summary

Phase 2 should stay on the existing local TCP + newline-delimited JSON design. The current repo already has the right transport primitives in place: Python accepts NDJSON over `asyncio.start_server()`, Bookmap already sends a `connection_hello`, and each cTrader exporter already opens a `TcpClient` and writes JSON. The gap is lifecycle discipline, not protocol choice. Today Bookmap reconnects once in a fire-and-forget thread, cTrader exporters swallow socket write failures without re-establishing the connection, Python writes runtime artifacts into the current working directory, and there is no canonical operator health surface.

The standard pattern for this phase is: keep Python as the single observer/publisher, move all runtime artifact naming under a repo-root `runtime/`, and add explicit connection/state machinery around the existing sockets instead of introducing a broker, HTTP health server, or per-session storage. Producers should retry forever with bounded backoff, only log transitions, and resend `connection_hello` after every successful reconnect. Python should maintain one in-memory stage registry, emit `runtime/status.json` atomically on heartbeat and state changes, and keep cumulative disconnect/drop counters for the session.

The main planning risk is not implementation complexity, it is consistency. Multiple files currently write logs and CSVs using ad hoc filenames (`quarantine_events.jsonl`, `socket_events.jsonl`, `history_*.csv`, `trade_history.csv`, `model.pkl`) and some writes default to the repo root or user home. If Phase 2 does not centralize runtime paths first, health and artifact work will drift immediately.

**Primary recommendation:** Keep production runtime dependencies to the existing stdlib/socket stack, add a Python-owned `runtime/` artifact/status layer, and require Wave 0 Python tests for socket lifecycle, status publication, and filename/schema rules before broad edits.

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CPython stdlib (`asyncio`, `json`, `logging`, `tempfile`, `os`, `pathlib`) | Target runtime should remain Python 3.10+; official docs verified against Python 3.14.3 | Async TCP server, NDJSON framing, atomic status writes, file/log handling | Already used in `main/`; enough for this phase without adding a service framework |
| Java SE socket/concurrency APIs | Repo pins Java 17 in `bookmap-addons/build.gradle`; official docs verified against Java SE 25 API | Bookmap reconnect loop, keepalive, periodic retry scheduling | Fits existing addon code and avoids extra Bookmap-side dependencies |
| .NET 6 + `cTrader.Automate` | Repo pins `net6.0` and `cTrader.Automate` 1.0.14 | cTrader producer socket lifecycle and export logic | Matches current cTrader project configuration and avoids runtime mismatch risk |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `pytest` | 9.0.2, published 2025-12-06 | Python unit/integration tests for reconnect, status, and runtime artifacts | Add in Wave 0 because the repo has no Python test harness today |
| `junit:junit` | 4.13.2, Maven metadata last updated 2021-02-13 | Existing Bookmap-side unit tests | Keep for Java regression coverage; extend only if Phase 2 touches alert-listener parsing/runtime file naming |
| `logging.handlers.RotatingFileHandler` | Python 3.14.3 stdlib docs verified | Optional Python file log rotation if Phase 2 adds a dedicated runtime log | Use only if a stable `runtime/pipeline.log` is added; otherwise keep stdout plus status JSON |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Existing TCP + NDJSON | ZeroMQ, WebSocket, or a local broker | Unnecessary scope expansion; current producers already speak newline-delimited JSON and Phase 2 is about reliability, not protocol replacement |
| Flat `runtime/status.json` | FastAPI/HTTP health endpoint | Violates the locked file-first operator surface and adds another process concern |
| Flat live files under `runtime/` | Per-session folders or log archives | Deferred by context; session archival belongs later unless a minimal hook is unavoidable |

**Installation:**
```bash
python -m pip install pytest==9.0.2
```

**Version verification:**
- `pytest` 9.0.2 is the newest version shown in the current PyPI project listing snippet (`2025-12-06`).
- `junit:junit` 4.13.2 is still the latest release in Maven Central metadata.
- `cTrader.Automate` upstream index includes 1.0.16, but this repo is pinned to 1.0.14. Do not upgrade it as part of Phase 2 unless planning discovers a blocker tied to 1.0.14.
- Bookmap API dependencies are repo-pinned to 7.6.0.20 in `bookmap-addons/build.gradle`; no public vendor metadata was verified during this research.

## Architecture Patterns

### Recommended Project Structure
```text
main/
|-- main.py                 # startup/wiring
|-- socket_server.py        # TCP accept loop and client lifecycle
|-- ai_analyzer.py          # buffering, inference, execution handoff
|-- runtime_paths.py        # new: single source of truth for runtime filenames
`-- pipeline_status.py      # new: in-memory stage registry + atomic status publisher

runtime/
|-- status.json
|-- socket_events.jsonl
|-- quarantine_events.jsonl
|-- pipeline.log            # optional; only if Phase 2 adds file logging
|-- history_alert_<alias>.csv
|-- history_order_flow_aggregated.csv
|-- history_volume_profile.csv
|-- history_wyckoff_state.csv
|-- trade_history.csv
`-- model.pkl
```

### Pattern 1: Python-Owned Stage Registry
**What:** One Python module owns the session clock, stage state, counters, thresholds, and publication of `runtime/status.json`.
**When to use:** Always. The context explicitly locks Python as the source of truth.
**Example:**
```python
from dataclasses import dataclass, asdict
from datetime import datetime, timezone
from pathlib import Path
import json
import os
import tempfile

@dataclass
class StageStatus:
    state: str
    updated_at: str
    reason: str
    dropped_events: int = 0
    reconnects: int = 0

def publish_status(snapshot: dict, target: Path) -> None:
    target.parent.mkdir(parents=True, exist_ok=True)
    with tempfile.NamedTemporaryFile(
        "w", encoding="utf-8", dir=target.parent, delete=False
    ) as tmp:
        json.dump(snapshot, tmp, ensure_ascii=True, indent=2)
        tmp.write("\n")
        temp_name = tmp.name
    os.replace(temp_name, target)
```
Source: Python `tempfile` and `os.replace` docs plus locked Phase 2 JSON-status requirement

### Pattern 2: Forever-Retry Producer State Machine
**What:** Each producer keeps a small internal lifecycle: `disconnected -> connecting -> connected`. On connect success it resends `connection_hello`; on write failure it closes the socket, increments a reconnect counter, and re-enters retry with backoff.
**When to use:** Bookmap `AlertListener` and all three cTrader lifecycle exporters.
**Example:**
```java
private final ScheduledExecutorService reconnectLoop =
        Executors.newSingleThreadScheduledExecutor();

private void startReconnectLoop() {
    reconnectLoop.scheduleWithFixedDelay(() -> {
        if (socketWriter != null) {
            return;
        }
        try {
            Socket next = new Socket(HOST, PORT);
            next.setKeepAlive(true);
            synchronized (socketLock) {
                socket = next;
                socketWriter = new PrintWriter(socket.getOutputStream(), true);
                sendConnectionHello();
            }
            logStateChange("connected");
        } catch (IOException ignored) {
            // State-change logging only; no per-attempt log spam.
        }
    }, 0, 5, TimeUnit.SECONDS);
}
```
Source: Java `Socket` and `ScheduledExecutorService` / `ScheduledThreadPoolExecutor` docs, adapted to current `AlertListener` shape

### Pattern 3: Reconnect-on-Send for cTrader
**What:** cTrader exporters should treat `_tcpStream == null` or any write exception as a disconnected state, close/reset the client, and let the next eligible send or a throttled timer attempt reconnection.
**When to use:** All current cTrader lifecycle files, because they already swallow write exceptions and do not recover automatically.
**Example:**
```csharp
private DateTime _nextReconnectAt = DateTime.MinValue;

private void EnsureSocketConnected()
{
    if (_tcpClient != null && _tcpClient.Connected)
        return;
    if (DateTime.UtcNow < _nextReconnectAt)
        return;

    _nextReconnectAt = DateTime.UtcNow.AddSeconds(5);
    ConnectSocket();
}

private void SafeWrite(byte[] data)
{
    try
    {
        EnsureSocketConnected();
        _networkStream?.Write(data, 0, data.Length);
    }
    catch
    {
        try { _networkStream?.Close(); } catch {}
        try { _tcpClient?.Close(); } catch {}
        _networkStream = null;
        _tcpClient = null;
    }
}
```
Source: Current cTrader lifecycle patterns in repo, plus .NET TCP client guidance

### Pattern 4: Deadline-Based Health Evaluation
**What:** Stage status should be computed from the last observed heartbeat/success timestamp plus thresholds, not from boolean flags alone.
**When to use:** All four pipeline-facing stages.
**Recommended thresholds:** heartbeat every 5s; move to `degraded` after 10s stale; move to `down` after 30s stale.
**Why these values:** They are fast enough for local troubleshooting without causing noisy flapping in a desktop workflow.

### Recommended Stage Mapping
| Stage | Recommended signal inputs | `up` | `degraded` | `down` |
|-------|---------------------------|------|------------|--------|
| `ingest` | Python server bound, last producer hello/event, disconnect/drop counters | Server listening and at least one producer recently active | Server listening but producers stale/disconnected or drop counters rising | Server bind failure or fatal accept-loop failure |
| `buffering` | Analyzer accepts normalized records and processes buffers on schedule | Records enter and buffers flush normally | Records arriving but backlog/process lag exceeds threshold | Analyzer callback failing or buffers not processing at all |
| `inference` | Model loaded, feature extraction runs, inference calls succeed when applicable | Model available and recent inference path succeeded | Model loaded but recent inference errors/staleness observed | Model unavailable or repeated inference failures block progress |
| `execution` | `MT5Client.connected`, simulator availability, recent order-path errors | MT5 connected and execution path callable | MT5 disconnected but simulator path alive, or recent execution errors without total outage | Execution path unavailable entirely |

### Recommended Stable Runtime Filenames
| File | Format | Owner | Notes |
|------|--------|-------|-------|
| `runtime/status.json` | JSON | Python | Source-of-truth operator status surface |
| `runtime/socket_events.jsonl` | JSONL | Python | Append-only normalized/raw ingress tap |
| `runtime/quarantine_events.jsonl` | JSONL | Python | Append-only invalid/rejected record log |
| `runtime/pipeline.log` | text log | Python | Optional; use only if Phase 2 adds file logging |
| `runtime/history_alert_<alias>.csv` | CSV | Python or Bookmap, but not both | Keep alias-specific alert history stable |
| `runtime/history_order_flow_aggregated.csv` | CSV | Python | Rename current ad hoc history output to one stable filename |
| `runtime/history_volume_profile.csv` | CSV | Python | Stable filename |
| `runtime/history_wyckoff_state.csv` | CSV | Python | Stable filename |
| `runtime/trade_history.csv` | CSV | Python | Existing simulator artifact, relocated under `runtime/` |
| `runtime/model.pkl` | binary | Python | Existing model file, relocated under `runtime/` |

### Anti-Patterns to Avoid
- **Producer-owned health files:** Conflicts with the locked "Python owns the source of truth" rule and guarantees drift.
- **Per-retry or per-drop logging:** Violates the locked state-change-only logging decision and will flood operators.
- **Multiple modules generating filenames independently:** Current repo already shows filename drift; centralize runtime paths first.
- **Replay queues for disconnected realtime traffic:** Explicitly out of scope; dropped events must remain counters only.
- **Nested runtime folders or per-session live directories:** Conflicts with the locked flat `runtime/` layout.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Reliable message transport for this phase | A new broker, queue, or custom replay cache | Existing localhost TCP + NDJSON with reconnect/backoff | Current producers already speak it; reliability gaps are lifecycle-related, not transport-feature-related |
| Operator health surface | A dashboard or web app | `runtime/status.json` + runtime docs | The phase explicitly locks a JSON health file and defers UI/dashboard work |
| File publication safety | Naive overwrite-in-place for status | Temp file + `os.replace()` | Prevents partial/truncated status reads during live troubleshooting |
| Runtime path selection | Per-module `open("history_*.csv")` logic | One `runtime_paths.py` module | Current repo already mixes cwd-relative and home-relative writes |
| Producer identity recovery | Heuristic inference after reconnect | Mandatory `connection_hello` on every successful reconnect | Already established in Phase 1 and locked again in Phase 2 |

**Key insight:** The expensive bugs in this domain come from lifecycle drift and artifact drift, not from missing middleware. Keep the stack boring and make ownership explicit.

## Common Pitfalls

### Pitfall 1: Single-Shot Reconnect That Looks Automatic
**What goes wrong:** Bookmap retries once in a spawned thread; cTrader exporters reconnect only on button press or incidental calls and silently drop writes after exceptions.
**Why it happens:** Current code closes sockets on error but does not schedule a persistent reconnect loop.
**How to avoid:** Add explicit forever-retry with throttled backoff and `connection_hello` on every success.
**Warning signs:** `socketWriter == null` or `_networkStream == null` persists after the Python server comes back up.

### Pitfall 2: Status Truth Split Across Modules
**What goes wrong:** Stage status, drop counters, and reasons drift because multiple components try to publish them.
**Why it happens:** It is tempting to let each producer or analyzer component write "its own" status.
**How to avoid:** Only Python publishes `runtime/status.json`; everyone else emits signals/counters into Python-owned state.
**Warning signs:** Conflicting timestamps or reasons for the same stage from different code paths.

### Pitfall 3: Torn `status.json`
**What goes wrong:** Operators or tests read partially written JSON and misclassify the pipeline as broken.
**Why it happens:** Overwriting the status file directly is not atomic.
**How to avoid:** Write to a temp file in `runtime/` and atomically replace the target.
**Warning signs:** Intermittent JSON parse errors or empty reads while the system is running.

### Pitfall 4: Log Flood During Disconnects
**What goes wrong:** A noisy reconnect loop hides the real state transition and makes troubleshooting harder.
**Why it happens:** Logging every retry attempt or dropped event seems useful early and becomes operationally useless immediately.
**How to avoid:** Log only state changes; keep cumulative counters in `status.json`.
**Warning signs:** Hundreds of repeated "could not connect" or "dropped event" lines for one outage.

### Pitfall 5: Runtime Files Still Follow the Process Working Directory
**What goes wrong:** Files appear in the repo root, user home, or vendor runtime folders depending on how the process was launched.
**Why it happens:** Current code uses bare filenames (`quarantine_events.jsonl`, `socket_events.jsonl`, `trade_history.csv`, `model.pkl`) and Bookmap uses `System.getProperty("user.home")`.
**How to avoid:** Introduce one repo-root runtime path resolver and route every writer through it.
**Warning signs:** Operators have to ask "which folder did this run write into?"

### Pitfall 6: Hidden Drops During Recovery
**What goes wrong:** Realtime events disappear during outages and the operator cannot tell whether the loss was one event or thousands.
**Why it happens:** Current producers swallow write exceptions and Phase 2 explicitly forbids replay buffering.
**How to avoid:** Increment cumulative per-producer and per-stage drop counters in Python status, but do not emit per-event logs.
**Warning signs:** Stage looks `up` again but there is no evidence of what was lost while disconnected.

## Code Examples

Verified patterns from official sources and current repo constraints:

### Atomic Status Publish
```python
from datetime import datetime, timezone

def build_snapshot(stage_map: dict, session_started_at: str) -> dict:
    return {
        "updated_at": datetime.now(timezone.utc).isoformat().replace("+00:00", "Z"),
        "session_started_at": session_started_at,
        "stages": stage_map,
    }
```
Source: Phase 2 locked status-file shape + Python datetime/json tooling already used in `main/socket_server.py`

### Async NDJSON Server Read Loop
```python
async def handle_client(reader, writer):
    while True:
        line = await reader.readline()
        if not line:
            break
        record = json.loads(line.decode("utf-8").strip())
        ...
```
Source: Python asyncio streams docs and current `main/socket_server.py`

### Java Periodic Reconnect Scheduling
```java
ScheduledFuture<?> reconnectTask = executor.scheduleWithFixedDelay(
        this::attemptReconnect,
        0,
        5,
        TimeUnit.SECONDS
);
```
Source: Java scheduled executor docs

### Python Rotating Log File
```python
from logging.handlers import RotatingFileHandler

handler = RotatingFileHandler("runtime/pipeline.log", maxBytes=1_000_000, backupCount=3)
```
Source: Python `logging.handlers` docs

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Manual reconnect button or one-shot retry | Long-lived reconnect loop with bounded backoff and hello-on-reconnect | Mature standard before 2026; already partially reflected in current repo but not completed | Startup order stops mattering operationally |
| Operator reads console/UI logs to infer health | Machine-readable stage status in one JSON file | Mature standard before 2026 | Troubleshooting becomes scriptable and visible without reading code |
| Ad hoc cwd/home-relative output files | Central runtime root with stable live filenames | Mature standard before 2026 | Artifacts become predictable and testable |

**Deprecated/outdated:**
- Silent socket write failure with swallowed exception: replace with explicit disconnect accounting and reconnect scheduling.
- Writing runtime artifacts directly under repo root or user home: replace with flat `runtime/`.
- Per-retry/per-drop log lines: replace with state-change logs plus counters.

## Open Questions

1. **Should cTrader use a background timer or reconnect-on-send only?**
   - What we know: current exporters already call `ConnectSocket()` from UI/export paths and use `TcpClient` directly.
   - What's unclear: whether the cTrader runtime imposes any practical constraints on background timers/threads in these indicators.
   - Recommendation: plan for reconnect-on-send with throttling as the minimum safe baseline; add a timer only if manual runtime validation shows it is stable in cTrader.

2. **How should `execution` be classified when MT5 is unavailable but the simulator is alive?**
   - What we know: `main/main.py` proceeds even if `MT5Client.connect()` fails, and the analyzer still has an `OrderSimulator`.
   - What's unclear: whether operators should treat that as partial health or outage in Phase 2.
   - Recommendation: mark `execution` as `degraded` with reason `mt5 disconnected; simulator only` until Phase 4 introduces explicit execution modes.

3. **Do stage thresholds need to vary by producer or symbol?**
   - What we know: the phase locks stage-level health, not producer-level health.
   - What's unclear: whether one noisy producer should degrade only `ingest` or cascade into downstream stages immediately.
   - Recommendation: keep thresholds global for Phase 2 and degrade downstream stages only when their own deadlines are missed; do not make health symbol-specific yet.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | `pytest` 9.0.2 for new Python tests; existing Java suite uses JUnit 4.13.2 |
| Config file | `none` - add `pytest.ini` or `pyproject.toml` in Wave 0 |
| Quick run command | `python -m pytest tests/test_socket_lifecycle.py -q` |
| Full suite command | `python -m pytest -q` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| PIPE-01 | Producers can start before/after Python and reconnect automatically without manual edits | integration | `python -m pytest tests/test_socket_lifecycle.py -q` | No - Wave 0 |
| PIPE-02 | Operators can inspect current ingest/buffering/inference/execution stage health from one file | unit/integration | `python -m pytest tests/test_pipeline_status.py -q` | No - Wave 0 |
| PIPE-03 | Runtime outputs use stable filenames and schemas under `runtime/` | unit | `python -m pytest tests/test_runtime_artifacts.py -q` | No - Wave 0 |

### Sampling Rate
- **Per task commit:** `python -m pytest tests/test_socket_lifecycle.py -q`
- **Per wave merge:** `python -m pytest -q`
- **Phase gate:** Python phase tests green, plus manual smoke of Bookmap and at least one cTrader exporter reconnecting against a restarted Python server before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `tests/test_socket_lifecycle.py` - covers `PIPE-01`
- [ ] `tests/test_pipeline_status.py` - covers `PIPE-02`
- [ ] `tests/test_runtime_artifacts.py` - covers `PIPE-03`
- [ ] `tests/conftest.py` - fake producer fixtures and temporary runtime directory helpers
- [ ] `pytest.ini` or `pyproject.toml` - declare pytest configuration
- [ ] Framework install: `python -m pip install pytest==9.0.2`

## Sources

### Primary (HIGH confidence)
- Local code inspection:
  - `main/socket_server.py`
  - `main/main.py`
  - `main/ai_analyzer.py`
  - `main/mt5_client.py`
  - `main/order_simulator.py`
  - `main/event_contract.py`
  - `bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java`
  - `ctrader-projects/OrderFlowAggregatedV20/src/Order Flow Aggregated v2.0.Lifecycle.cs`
  - `ctrader-projects/FreeVolumeProfileV20/src/Free Volume Profile v2.0.Lifecycle.cs`
  - `ctrader-projects/WeisWyckoffSystemV20/src/Weis & Wyckoff System v2.0.Lifecycle.cs`
  - `bookmap-addons/build.gradle`
  - `ctrader-projects/Directory.Build.props`
  - `ctrader-projects/Directory.Packages.props`
  - `.gitignore`
- Python docs:
  - https://docs.python.org/3/library/asyncio-stream.html - `asyncio.start_server`, `StreamReader.readline`, stream writer lifecycle
  - https://docs.python.org/3/library/logging.handlers.html - `FileHandler`, `RotatingFileHandler`
  - https://docs.python.org/3/library/tempfile.html - temporary-file creation for safe publication
  - https://docs.python.org/3/library/os.html - `os.replace()` atomic replacement semantics
- Java docs:
  - https://docs.oracle.com/en/java/javase/25/docs/api/java.base/java/net/Socket.html - socket options such as keepalive
  - https://docs.oracle.com/en/java/javase/25/docs/api/java.base/java/util/concurrent/ScheduledExecutorService.html - scheduling reconnect attempts
  - https://docs.oracle.com/en/java/javase/25/docs/api/java.base/java/util/concurrent/ScheduledThreadPoolExecutor.html - `scheduleWithFixedDelay`
- .NET docs:
  - https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets?view=net-11.0 - `TcpClient` and network socket namespace
  - https://learn.microsoft.com/en-us/dotnet/api/system.threading.timer.-ctor?view=net-10.0 - timer-based periodic work option
- Package/version sources:
  - https://repo1.maven.org/maven2/junit/junit/maven-metadata.xml - latest JUnit 4 metadata
  - https://api.nuget.org/v3-flatcontainer/ctrader.automate/index.json - available `cTrader.Automate` package versions

### Secondary (MEDIUM confidence)
- https://pypi.org/project/pytest/7.3.0/ - current PyPI listing snippet shows `pytest` 9.0.2 released 2025-12-06
- `.planning/codebase/STRUCTURE.md` - repo layout and existing test location
- `.planning/codebase/INTEGRATIONS.md` - current transport and artifact inventory
- `.planning/codebase/CONCERNS.md` - existing fragility notes and test gaps

### Tertiary (LOW confidence)
- None

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - grounded in current repo config plus official language/runtime docs
- Architecture: MEDIUM - core ownership pattern is clear, but exact thresholds and cTrader reconnect trigger shape are still design choices
- Pitfalls: HIGH - directly supported by current code inspection and current-phase locked decisions

**Research date:** 2026-03-22
**Valid until:** 2026-04-21
