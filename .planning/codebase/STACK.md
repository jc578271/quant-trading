# Technology Stack

**Analysis Date:** 2026-03-18

## Languages

**Primary:**
- C# / .NET 6 - Maintained cTrader indicators under `ctrader-projects/*/src/*.cs`
- Java 17 - Bookmap addons under `bookmap-addons/src/main/java`

**Secondary:**
- Python 3.x - Local MT5 + AI bridge under `main/*.py`
- Pine Script v5/v6 - TradingView indicators under `tradingview-indicators/*.pine`
- PowerShell - Build and split/merge tooling under `ctrader-projects/*.ps1`

## Runtime

**Environment:**
- cTrader Automate runtime - executes indicator classes marked with `[Indicator(...)]`
- Bookmap 7.6.0+ with Bookmap L1 API - loads addon jars from `bookmap-addons/build/libs`
- Desktop Python runtime - runs the local socket server and MT5 integration in `main/main.py`
- MetaTrader 5 desktop terminal - required by `main/mt5_client.py`

**Package Manager:**
- NuGet / `dotnet build` - C# projects use central package management via `ctrader-projects/Directory.Packages.props`
- Gradle 9.0 wrapper - Java addons use `bookmap-addons/gradlew.bat`
- pip-compatible `requirements.txt` - Python deps listed in `main/requirements.txt`
- Lockfiles: no `package-lock.json`, `poetry.lock`, or `requirements-lock`; dependency resolution is mostly implicit

## Frameworks

**Core:**
- `cTrader.Automate` 1.0.14 - indicator SDK for all `ctrader-projects/*/*.csproj`
- Bookmap API `api-core` and `api-simplified` 7.6.0.20 - addon SDK in `bookmap-addons/build.gradle`
- `MetaTrader5` Python package - MT5 terminal bridge in `main/mt5_client.py`
- `scikit-learn` + `numpy` + `pandas` + `joblib` - local model/inference flow in `main/ai_analyzer.py`

**Testing:**
- JUnit 4.13.2 - only observed automated tests in `bookmap-addons/src/test/java`

**Build/Dev:**
- MSBuild / `dotnet build` - orchestrated by `ctrader-projects/Build-CTraderProjects.ps1`
- Gradle wrapper - builds Bookmap jars and runs Java tests
- Repo-local `.dotnet-cli` and `.nuget` caches - configured by `ctrader-projects/Build-CTraderProjects.ps1`

## Key Dependencies

**Critical:**
- `cTrader.Automate` 1.0.14 - cTrader indicator API surface and chart/runtime objects
- `com.bookmap.api:api-core` 7.6.0.20 - Bookmap addon host integration
- `com.bookmap.api:api-simplified` 7.6.0.20 - simplified Bookmap API access
- `MetaTrader5` - live trading terminal connection for the Python bridge
- `scikit-learn` - local `RandomForestClassifier` used for trade inference in `main/ai_analyzer.py`

**Infrastructure:**
- `junit:junit` 4.13.2 - Java regression tests for CSV parsing/export behavior
- `numpy` / `pandas` / `joblib` - feature preparation and model persistence in Python

## Configuration

**Environment:**
- Bookmap Telegram config is persisted in user-home properties files from `bookmap-addons/src/main/java/com/bookmap/rithmicmonitor/SimpleTelegramNotifier.java`
- Bookmap alert listener writes user config to `AlertListener.properties` in the user profile
- Python runtime uses local files such as `model.pkl`, `trade_history.csv`, and `history_*.csv`
- cTrader output path defaults to `Documents\cAlgo\Sources\Indicators` via `ctrader-projects/Directory.Build.props`

**Build:**
- `quant-trading.sln` - Visual Studio solution for the maintained cTrader projects
- `ctrader-projects/Directory.Build.props` and `ctrader-projects/Directory.Build.targets` - shared .NET defaults and post-build `.algo` copy
- `bookmap-addons/build.gradle` - Bookmap addon dependencies and jar outputs
- `main/requirements.txt` - Python dependency manifest

## Platform Requirements

**Development:**
- Windows-first workflow: PowerShell scripts, Bookmap install paths, MT5 desktop, and cTrader output folders are Windows-oriented
- Java 17 required for `bookmap-addons`
- .NET SDK compatible with `net6.0` required for `ctrader-projects`

**Production / Runtime:**
- No centralized deployment target; this repo ships desktop trading tools and indicators
- Runtime distribution is manual: `.algo` files for cTrader, `.jar` files for Bookmap, and a local Python process for the AI bridge

---

*Stack analysis: 2026-03-18*
*Update after major dependency changes*
