# Architecture

**Analysis Date:** 2026-03-18

## Pattern Overview

**Overall:** Polyglot desktop-tool workspace grouped by trading platform rather than a single deployable application.

**Key Characteristics:**
- Platform-specific subprojects live side by side: cTrader, Bookmap, TradingView, and a local Python bridge
- Most execution is event-driven: market ticks, Bookmap callbacks, socket messages, and indicator lifecycle hooks
- State is predominantly local and file-based: properties files, CSV history, `.algo` artifacts, and `model.pkl`
- Cross-platform reuse is minimal; reuse happens mostly inside each subsystem

## Layers

**Platform Runtime Layer:**
- Purpose: Host code inside trading platforms or local desktop runtimes
- Contains: cTrader indicators in `ctrader-projects/*/src/*.cs`, Bookmap addons in `bookmap-addons/src/main/java`, Python socket server in `main/main.py`
- Depends on: cTrader Automate, Bookmap APIs, MT5 terminal, local OS/network
- Used by: Trader manually loading indicators/addons or starting the Python process

**Domain Logic Layer:**
- Purpose: Compute market-derived signals, parse alerts, aggregate order-flow, and simulate/manage trades
- Contains: indicator `Initialize` / `Calculate` flows, Bookmap alert parsing, `AIAnalyzer`, `OrderSimulator`
- Depends on: runtime callback APIs plus local file persistence
- Used by: entry-point classes and callbacks

**Support / Tooling Layer:**
- Purpose: Build, package, and transform source layouts
- Contains: `ctrader-projects/Build-CTraderProjects.ps1`, split/merge scripts, Gradle build logic, repo-level docs
- Depends on: local SDK installs and repo-local caches
- Used by: developer workflows rather than live runtime behavior

## Data Flow

**Bookmap -> Python AI pipeline:**
1. Bookmap loads `AlertListener` or `SimpleTelegramNotifier` from `bookmap-addons`
2. Bookmap callbacks such as `onDepth`, `onTrade`, and alert handlers receive market events
3. `AlertListener` normalizes events, logs CSV rows, and pushes JSON over `127.0.0.1:5555`
4. `main/socket_server.py` accepts line-delimited JSON and forwards records to `AIAnalyzer.process_data`
5. `main/ai_analyzer.py` buffers, forward-fills, exports CSV history, runs local model inference, and updates `OrderSimulator`
6. `main/mt5_client.py` can place risk-managed orders through the MT5 terminal

**cTrader indicator execution:**
1. Developer builds a project from `ctrader-projects/*/src/*.csproj`
2. Post-build copy target writes the resulting `.algo` into `Documents\cAlgo\Sources\Indicators`
3. cTrader loads indicator classes such as `OrderFlowTicksV20`, `FreeVolumeProfileV20`, and `WeisWyckoffSystemV20`
4. `Initialize()` wires indicator state, parameters, controls, and helper series
5. `Calculate(int index)` consumes bar/tick updates and redraws chart state

**TradingView execution:**
1. A `.pine` script from `tradingview-indicators` is pasted/imported into TradingView
2. TradingView lower-timeframe requests and script state drive rendering fully inside the platform

**State Management:**
- Local-only and mostly in-memory per process, with persistence via CSV/properties/model files
- No shared database or central service boundary inside this workspace

## Key Abstractions

**Indicator Classes:**
- Purpose: Long-lived chart-bound analytics modules
- Examples: `ctrader-projects/OrderFlowAggregatedV20/src/Order Flow Aggregated v2.0.cs`, `ctrader-projects/WeisWyckoffSystemV20/src/Weis & Wyckoff System v2.0.cs`
- Pattern: runtime-owned classes with callback overrides (`Initialize`, `Calculate`)

**Addon Listener Classes:**
- Purpose: React to Bookmap market data and alerts
- Examples: `bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java`, `bookmap-addons/src/main/java/com/bookmap/rithmicmonitor/SimpleTelegramNotifier.java`
- Pattern: API listener / admin adapter implementations

**Helper Modules:**
- Purpose: Pull bulky specialized logic out of giant indicator files
- Examples: `*.CustomMA.cs`, `*.Filters.cs`, `*.NodesAnalizer.cs`, `*.ParamsPanel.cs`, `*.Styles.cs`
- Pattern: static helper classes or UI helper classes adjacent to the main indicator

## Entry Points

**Python bridge entry:**
- Location: `main/main.py`
- Triggers: manual process start
- Responsibilities: connect MT5, create `AIAnalyzer`, and run the socket server

**Bookmap addon entry:**
- Location: `bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java`
- Triggers: Bookmap plugin load
- Responsibilities: register listeners, reconnect socket, aggregate market events, write CSV output

**cTrader build entry:**
- Location: `ctrader-projects/Build-CTraderProjects.ps1`
- Triggers: manual PowerShell invocation or IDE build
- Responsibilities: configure local caches and call `dotnet build`

## Error Handling

**Strategy:** Mostly log-and-continue at runtime boundaries rather than fail-fast shutdown.

**Patterns:**
- Python catches malformed JSON and inference errors in `main/socket_server.py` and `main/ai_analyzer.py`
- Java addons print to console/UI and keep trying to run even when socket or Telegram calls fail
- cTrader indicators rely on defensive comments/workarounds inside large methods rather than centralized exception policy

## Cross-Cutting Concerns

**Logging:**
- Python uses `logging`
- Java addons use `System.out`, `System.err`, UI labels, and persisted CSV logs

**Persistence:**
- CSV files are used heavily for market-event history and trade history
- User config is stored in home-directory `.properties` files for Bookmap addons

**External Process Coupling:**
- The Python bridge assumes a local MT5 terminal
- The Bookmap alert listener assumes the Python socket server is available on localhost

---

*Architecture analysis: 2026-03-18*
*Update when major patterns change*
