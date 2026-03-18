---
phase: 01
slug: normalize-event-contract
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-03-18
---

# Phase 01 - Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Mixed-language smoke/build commands |
| **Config file** | none - use existing workspace build entrypoints |
| **Quick run command** | `python -m compileall main` |
| **Full suite command** | `powershell -ExecutionPolicy Bypass -File ".\\ctrader-projects\\Build-CTraderProjects.ps1"; Set-Location .\\bookmap-addons; $env:JAVA_HOME="C:\\Program Files\\Bookmap\\jre"; .\\gradlew.bat alertListenerJar; Set-Location ..; python -m compileall main` |
| **Estimated runtime** | ~120 seconds |

---

## Sampling Rate

- **After every task commit:** Run `python -m compileall main` after Python edits; run the relevant language build after Java or C# edits.
- **After every plan wave:** Run `powershell -ExecutionPolicy Bypass -File ".\\ctrader-projects\\Build-CTraderProjects.ps1"; Set-Location .\\bookmap-addons; $env:JAVA_HOME="C:\\Program Files\\Bookmap\\jre"; .\\gradlew.bat alertListenerJar; Set-Location ..; python -m compileall main`
- **Before `$gsd-verify-work`:** The full mixed build must be green and one quarantine smoke path must be manually confirmed.
- **Max feedback latency:** 120 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 01-01-01 | 01 | 1 | DATA-01 | doc + Java build | `Set-Location .\\bookmap-addons; $env:JAVA_HOME="C:\\Program Files\\Bookmap\\jre"; .\\gradlew.bat alertListenerJar` | ✅ | ⬜ pending |
| 01-02-01 | 02 | 2 | DATA-02 | Python compile | `python -m compileall main` | ✅ | ⬜ pending |
| 01-03-01 | 03 | 2 | DATA-03 | C# build | `powershell -ExecutionPolicy Bypass -File ".\\ctrader-projects\\Build-CTraderProjects.ps1"` | ✅ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- Existing infrastructure covers this phase: Python compile, Bookmap Gradle build, and cTrader build script already exist.
- No new test harness is required before execution, but the executor should add lightweight manual smoke evidence to each SUMMARY.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Accepted canonical event reaches analyzer | DATA-01, DATA-02 | Requires a live producer or replayed socket line | Start the socket server, send one valid v1 JSON line, confirm no quarantine entry is created and analyzer logs continue |
| Invalid or unmappable event is quarantined | DATA-02 | Needs runtime ingest behavior rather than static compile checks | Send one JSON line without `instrument`, confirm `quarantine_events.jsonl` gains a line with `reason` and the server keeps listening |
| cTrader exporters emit mapped event names | DATA-03 | Requires exporter runtime or generated payload inspection | Inspect emitted JSON or temporary debug output to confirm `event` is `order_flow_aggregated`, `volume_profile`, or `wyckoff_state` |

---

## Validation Sign-Off

- [x] All tasks have automated verify or existing build dependencies
- [x] Sampling continuity: no 3 consecutive tasks without automated verify
- [x] Wave 0 covers all missing infrastructure references
- [x] No watch-mode flags
- [x] Feedback latency < 120s for the fastest path
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
