# External Integrations

**Analysis Date:** 2026-03-18

## APIs & External Services

**Trading Platform SDKs:**
- Bookmap L1 API - host addon lifecycle, market-depth, trade, and UI callbacks
  - SDK/Client: `api-core` and `api-simplified` declared in `bookmap-addons/build.gradle`
  - Auth: none inside the repo; Bookmap installation/runtime provides access
  - Entry points: `AlertListener.java`, `SimpleTelegramNotifier.java`

- cTrader Automate - host runtime for custom indicators
  - SDK/Client: `cTrader.Automate` from `ctrader-projects/Directory.Packages.props`
  - Auth: managed by cTrader desktop, not by this repo
  - Entry points: indicator classes in `ctrader-projects/*/src/*.cs`

**External APIs:**
- Telegram Bot API - outgoing alert delivery from `bookmap-addons/src/main/java/com/bookmap/rithmicmonitor/SimpleTelegramNotifier.java`
  - Integration method: raw HTTPS requests via `HttpURLConnection`
  - Auth: bot token + chat ID stored in addon config/properties
  - Usage: alert and timeout notifications

- MetaTrader 5 terminal API - order placement and account/symbol queries from `main/mt5_client.py`
  - Integration method: `MetaTrader5` Python package
  - Auth: implicit through the locally running MT5 terminal/session

## Data Storage

**Local Files:**
- CSV history files - `history_*.csv`, `history_alert_*.csv`, `trade_history.csv`
  - Writer locations: `main/ai_analyzer.py`, `main/order_simulator.py`, `bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java`
  - Purpose: local replay/audit trail of events and simulated trades

- Model file - `model.pkl`
  - Writer location: `main/ai_analyzer.py`
  - Purpose: persisted sklearn model used for future runs

- User properties files
  - `AlertListener.properties`
  - `SimpleTelegramNotifier.properties`
  - Location: user home directory via Java `System.getProperty("user.home")`

**Databases / Caches:**
- No shared database was found
- Repo-local caches exist for `.dotnet-cli`, `.nuget`, and Gradle outputs

## Authentication & Identity

**Auth Provider:**
- None for application users; this is a desktop-tool workspace, not a web app

**Credentialed Integrations:**
- Telegram requires `botToken` and `chatId`
- MT5 access depends on the locally authenticated terminal session

## Monitoring & Observability

**Logs:**
- Python logs to stdout/stderr via `logging`
- Java addons log to console and UI labels
- Market event audit trails are written to CSV

**Error Tracking / Analytics:**
- No Sentry, Datadog, or hosted analytics integration was found

## CI/CD & Deployment

**Hosting:**
- None; artifacts are loaded manually into desktop trading platforms

**Build / Distribution:**
- cTrader: `.algo` copied into `Documents\cAlgo\Sources\Indicators`
- Bookmap: jars emitted to `bookmap-addons/build/libs`
- Python: local process started manually from `main/main.py`

**CI Pipeline:**
- No GitHub Actions or other CI configuration was found in the workspace root

## Environment Configuration

**Development:**
- Bookmap path assumptions are embedded in `bookmap-addons/build.gradle`
- Python socket host/port is hardcoded to `127.0.0.1:5555` in both Java and Python
- cTrader output folder is defined in `ctrader-projects/Directory.Build.props`

**Production / Live Use:**
- Environment is effectively the trader's workstation
- Secrets are workstation-local rather than environment-variable driven

## Webhooks & Callbacks

**Incoming runtime callbacks:**
- Bookmap invokes `onDepth`, `onTrade`, `onMarketMode`, and UI callbacks in Java addons
- cTrader invokes `Initialize()` and `Calculate(int index)` in indicator classes

**Outgoing network calls:**
- Local TCP socket from Bookmap `AlertListener` to Python `SocketServer`
- HTTPS calls from `SimpleTelegramNotifier` to the Telegram Bot API
- MT5 terminal RPC through the `MetaTrader5` package

---

*Integration audit: 2026-03-18*
*Update when adding/removing external services*
