# Quant Trading Workspace

## What This Is

This is a brownfield workspace of desktop trading tools that already spans Bookmap addons, cTrader indicators, TradingView scripts, and a local Python + MT5 bridge. The current project focus is to turn those separate pieces into one reliable local pipeline where market events flow from Bookmap/cTrader context into Python analysis and then into safe, observable execution decisions.

## Core Value

Market data and order-flow events move through the local pipeline into trustworthy, verifiable trading actions without manual glue code or silent failure.

## Requirements

### Validated

- ✓ Bookmap addons already capture alerts and market events, write CSV history, and can publish socket payloads to the local bridge
- ✓ The Python bridge already accepts socket records, persists history, runs model inference, and can talk to MT5
- ✓ The workspace already contains maintainable cTrader indicator projects for order-flow, volume profile, and Wyckoff analysis

### Active

- [ ] Unify the Bookmap/cTrader -> Python event contract so producers and consumers stop drifting
- [ ] Make the local integration pipeline reconnectable, observable, and safe to operate
- [ ] Verify the handoff from signals to execution so MT5/cTrader-side actions are explicit and testable

### Out of Scope

- New hosted/cloud trading platform - this milestone is about the existing local desktop workflow
- Building unrelated new indicators or strategies - current priority is integrating the existing toolchain
- Full portfolio management, broker abstraction, or enterprise risk engine - too broad for this initialization scope

## Context

The workspace is organized by trading platform, not as one deployable application. `bookmap-addons/` contains Java addons that parse Bookmap alerts, monitor Rithmic connectivity, and emit local signals. `main/` contains the Python bridge that receives socket data, exports history, runs an ML model, and submits MT5 orders. `ctrader-projects/` contains maintained split-source cTrader indicators whose analytics can inform or align with the broader signal pipeline.

The integration already exists in partial form, but it is fragile. The Bookmap -> Python bridge depends on a hardcoded localhost socket, the Python analyzer has an uninitialized `order_book` path, and build/test coverage is uneven across subsystems. The goal is not to reinvent the workspace; it is to make the existing pipeline coherent enough that future phases can extend it safely.

## Constraints

- **Platform**: Windows-first desktop environment - Bookmap paths, PowerShell tooling, MT5, and cTrader outputs are all workstation-centric
- **Runtime**: External vendor runtimes - Bookmap, cTrader, and MT5 behaviors constrain what can be changed and how issues are reproduced
- **Transport**: Localhost IPC - current integration uses `127.0.0.1:5555`, so reliability and startup ordering matter
- **Safety**: Trading actions must fail visibly - live execution cannot silently proceed on malformed or partial signals
- **Codebase shape**: Brownfield polyglot workspace - new work must fit existing Java, Python, and C# subsystems rather than collapse them into one stack

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Treat the whole `quant-trading` workspace as the project scope | The active goal spans Bookmap, Python, MT5, and cTrader rather than one folder | — Pending |
| Prioritize integration reliability over new feature expansion | Existing capabilities already exist but do not form a dependable pipeline yet | — Pending |
| Keep the architecture local-first and desktop-native | Current workflows rely on vendor desktop runtimes and localhost communication | — Pending |
| Treat `ctrader-projects/` as the maintained source of truth for cTrader code | The repo already split maintainable cTrader projects away from raw indicator exports | — Pending |

---
*Last updated: 2026-03-18 after initialization*
