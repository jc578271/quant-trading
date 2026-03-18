# Codebase Concerns

**Analysis Date:** 2026-03-18

## Tech Debt

**Very large cTrader indicator classes:**
- Issue: Core indicators remain multi-thousand-line files with many nested helper types and platform workarounds
- Files: `ctrader-projects/OrderFlowAggregatedV20/src/Order Flow Aggregated v2.0.cs`, `ctrader-projects/FreeVolumeProfileV20/src/Free Volume Profile v2.0.cs`, `ctrader-projects/WeisWyckoffSystemV20/src/Weis & Wyckoff System v2.0.cs`
- Impact: Small edits are hard to reason about, and regressions are difficult to isolate
- Fix approach: continue extracting stable helper concerns into adjacent files and add smoke/regression checks around the extracted behavior

**Generated artifacts live inside working source trees:**
- Issue: `bookmap-addons/build`, `bookmap-addons/bin`, `ctrader-projects/*/src/bin`, and `ctrader-projects/*/src/obj` are present in the repo workspace
- Impact: review noise, stale binaries, and easy confusion between source-of-truth and build output
- Fix approach: tighten ignore rules and document which artifacts are intentionally preserved

## Known Bugs

**Python DOM path references undefined order book state:**
- Symptoms: DOM (`type == "dom"`) messages can fail at runtime
- Trigger: `AIAnalyzer.process_data()` receives DOM payloads before any missing-field guard masks the path
- File: `main/ai_analyzer.py`
- Root cause: `self.order_book` is used but never initialized in `AIAnalyzer.__init__`
- Workaround: avoid or disable the DOM branch until the field is initialized properly

**GitNexus graph appears partially broken locally:**
- Symptoms: repo overview exists, but graph-backed queries/resources report missing `.gitnexus/kuzu`
- Trigger: using `gitnexus` query/resource features in this workspace
- Files: `.gitnexus/`, local tooling state
- Impact: code-intelligence workflows are less reliable than expected
- Workaround: re-run analysis/debug the local GitNexus setup before relying on graph features

## Security Considerations

**Telegram credentials stored in plain properties files:**
- Risk: `botToken` and `chatId` are written to user-home config files from `SimpleTelegramNotifier.java`
- Current mitigation: none beyond local workstation storage
- Recommendations: move secrets to a safer local secret store or encrypt/protect the config file

**Broad cTrader access rights:**
- Risk: indicators declare `AccessRights.FullAccess`, which expands what they can do on the machine/runtime
- Files: cTrader indicator entry files in `ctrader-projects/*/src/*.cs`
- Current mitigation: user manually chooses which indicators to load
- Recommendations: keep feature scope tight and review any new file/network access carefully

## Performance Bottlenecks

**Heavy startup/recalculation paths in cTrader indicators:**
- Problem: source comments explicitly mention high memory use and expensive first-run behavior
- Files: `ctrader-projects/OrderFlowAggregatedV20/src/Order Flow Aggregated v2.0.cs`, `ctrader-projects/FreeVolumeProfileV20/src/Free Volume Profile v2.0.cs`
- Cause: very large in-memory state, repeated series creation, and redraw-heavy chart logic
- Improvement path: profile hot paths before edits and prefer targeted extraction over broad rewrites

## Fragile Areas

**Bookmap -> Python socket bridge:**
- Why fragile: two separate processes must agree on host, port, JSON shape, and availability
- Files: `bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java`, `main/socket_server.py`, `main/ai_analyzer.py`
- Common failures: socket unavailable, malformed JSON, silent schema drift
- Safe modification: change both sides together and add narrow regression tests around payload shape
- Test coverage: minimal end-to-end coverage

**Split cTrader source model:**
- Why fragile: maintainable source lives in `ctrader-projects`, while related raw/upstream material also exists in `ctrader-indicators`
- Common failures: editing the wrong source of truth or forgetting merge/split steps
- Safe modification: treat `ctrader-projects/*/src` as primary for maintained indicators and use the PowerShell merge/split scripts deliberately
- Test coverage: mostly manual build/runtime validation

## Dependencies at Risk

**Desktop-platform coupling:**
- Risk: Bookmap APIs, cTrader Automate, and MT5 desktop integration are all external vendor-controlled runtimes
- Impact: vendor updates can break builds or runtime behavior without changes in this repo
- Migration plan: pin versions where possible and keep build docs current

## Test Coverage Gaps

**cTrader indicators:**
- What's not tested: chart lifecycle behavior, parameter-panel flows, and recalculation logic
- Risk: visual/runtime regressions are easy to ship unnoticed
- Priority: High

**Python execution path:**
- What's not tested: socket ingestion, feature extraction, inference thresholds, MT5 order submission, and the DOM branch
- Risk: runtime-only failures during live trading workflows
- Priority: High

---

*Concerns audit: 2026-03-18*
*Update as issues are fixed or new ones discovered*
