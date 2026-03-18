# Codebase Structure

**Analysis Date:** 2026-03-18

## Directory Layout

```text
quant-trading/
|-- bookmap-addons/         # Java Bookmap addons, tests, and Gradle build outputs
|-- ctrader-indicators/     # Imported/raw cTrader indicator source, docs, and image assets
|-- ctrader-projects/       # Maintainable multi-file cTrader projects and build tooling
|-- main/                   # Python MT5 + socket + AI bridge
|-- tradingview-indicators/ # Standalone Pine scripts
|-- .dotnet-cli/            # Repo-local dotnet CLI cache
|-- .nuget/                 # Repo-local NuGet package cache
|-- .gitnexus/              # GitNexus metadata/artifacts
|-- quant-trading.sln       # Solution entry for cTrader projects
|-- README.md               # Workspace overview
|-- AI_TRADING_GUIDE.md     # Additional trading/AI notes
`-- walkthrough.md          # Local notes / walkthrough
```

## Directory Purposes

**`bookmap-addons/`:**
- Purpose: Bookmap L1 addons plus Gradle-based build/test flow
- Contains: `src/main/java`, `src/test/java`, `build.gradle`, wrapper files, generated `build/` and `bin/`
- Key files: `src/main/java/com/bookmap/alertlistener/AlertListener.java`, `src/main/java/com/bookmap/rithmicmonitor/SimpleTelegramNotifier.java`
- Subdirectories: `src/main/java/com/bookmap/*` for source and `src/test/java/com/bookmap/*` for tests

**`ctrader-projects/`:**
- Purpose: Maintainable split source for cTrader indicators
- Contains: one folder per indicator, shared build props, PowerShell tooling, central package versions
- Key files: `Build-CTraderProjects.ps1`, `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`
- Subdirectories: `OrderFlowAggregatedV20/src`, `WeisWyckoffSystemV20/src`, `FreeVolumeProfileV20/src`

**`ctrader-indicators/`:**
- Purpose: Upstream/raw cTrader indicator materials and image assets
- Contains: README, screenshots, raw source exports, and its own nested `.git` directory
- Key files: `README.md`, `Raw_Source_Code_Output.txt`
- Subdirectories: `Images/` and repo-internal `.git/`

**`main/`:**
- Purpose: Python-side local service for ingesting market events and simulating/executing trades
- Contains: `main.py`, `socket_server.py`, `ai_analyzer.py`, `mt5_client.py`, `order_simulator.py`, `requirements.txt`
- Key files: `main.py` and `ai_analyzer.py`
- Subdirectories: no real source subfolders; `__pycache__/` is generated

**`tradingview-indicators/`:**
- Purpose: Standalone TradingView scripts
- Contains: `hoang-delta-candle.pine`, `hoang-footprint.pine`
- Key files: both `.pine` scripts
- Subdirectories: none

## Key File Locations

**Entry Points:**
- `main/main.py` - starts the Python bridge
- `bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java` - Bookmap alert listener addon
- `bookmap-addons/src/main/java/com/bookmap/rithmicmonitor/SimpleTelegramNotifier.java` - Bookmap Telegram notifier addon
- `ctrader-projects/Build-CTraderProjects.ps1` - shared cTrader build entry

**Configuration:**
- `.gitignore` - root ignore rules
- `bookmap-addons/config-sample.properties` - sample Telegram/monitor config
- `ctrader-projects/Directory.Build.props` - shared cTrader defaults
- `ctrader-projects/Directory.Packages.props` - central cTrader package versions
- `main/requirements.txt` - Python dependency list

**Core Logic:**
- `ctrader-projects/OrderFlowAggregatedV20/src/` - main order-flow indicator logic and helpers
- `ctrader-projects/WeisWyckoffSystemV20/src/` - Wyckoff indicator logic and helpers
- `ctrader-projects/FreeVolumeProfileV20/src/` - volume profile logic and helpers
- `bookmap-addons/src/main/java/com/bookmap/alertlistener/` - Bookmap alert parsing and socket export
- `main/ai_analyzer.py` - record buffering, model inference, CSV export

**Testing:**
- `bookmap-addons/src/test/java/com/bookmap/alertlistener/AlertListenerCsvTest.java` - only observed automated test suite

**Documentation:**
- `README.md` - workspace overview
- `ctrader-projects/README.md` - cTrader split/merge/build conventions
- `bookmap-addons/README.md` - Bookmap addon setup/build notes
- `ctrader-indicators/README.md` - upstream indicator catalog and screenshots

## Naming Conventions

**Files:**
- cTrader source keeps upstream human-readable names with spaces and version suffixes, e.g. `Order Flow Aggregated v2.0.cs`
- Adjacent helper files append concern suffixes such as `.CustomMA.cs`, `.Filters.cs`, `.ParamsPanel.cs`, `.Styles.cs`
- Java follows standard package/file naming, e.g. `AlertListener.java`
- Python uses `snake_case.py`

**Directories:**
- Top-level directories are platform-oriented (`bookmap-addons`, `ctrader-projects`, `tradingview-indicators`)
- cTrader project folders use PascalCase + version suffix (`OrderFlowAggregatedV20`)

**Special Patterns:**
- `src/bin` and `src/obj` under cTrader projects are generated build output
- `build/` and `bin/` under `bookmap-addons` are generated Gradle/Javac output

## Where to Add New Code

**New cTrader feature:**
- Primary code: the matching `ctrader-projects/<IndicatorName>/src/` folder
- Build wiring: reuse shared props/targets in `ctrader-projects/`
- Raw-source sync: if needed, merge/split via `Split-CTraderIndicator.ps1` and `Merge-CTraderIndicator.ps1`

**New Bookmap addon feature:**
- Implementation: `bookmap-addons/src/main/java/com/bookmap/<feature>/`
- Tests: `bookmap-addons/src/test/java/com/bookmap/<feature>/`

**New Python bridge behavior:**
- Implementation: `main/*.py`
- Tests: no established location yet; add one deliberately if introducing Python tests

## Special Directories

**`.dotnet-cli/` and `.nuget/`:**
- Purpose: repo-local SDK/package caches for deterministic cTrader builds
- Source: populated by local build commands
- Committed: caches exist locally; treat as tooling artifacts, not feature code

**`ctrader-indicators/.git/`:**
- Purpose: nested upstream repository history
- Source: separate git repo inside the workspace
- Committed: local nested repo, so take care when scripting across the workspace root

---

*Structure analysis: 2026-03-18*
*Update when directory structure changes*
