# Testing Patterns

**Analysis Date:** 2026-03-18

## Test Framework

**Runner:**
- JUnit 4.13.2 for Java tests in `bookmap-addons`
- No automated Python, cTrader, or Pine test framework was found in this workspace

**Assertion Library:**
- JUnit built-in assertions such as `assertEquals` and `assertTrue`

**Run Commands:**
```bash
.\gradlew.bat test
.\gradlew.bat test --tests com.bookmap.alertlistener.AlertListenerCsvTest
powershell -ExecutionPolicy Bypass -File .\ctrader-projects\Build-CTraderProjects.ps1
```

## Test File Organization

**Location:**
- Java tests live under `bookmap-addons/src/test/java`
- Current coverage is concentrated in `bookmap-addons/src/test/java/com/bookmap/alertlistener/AlertListenerCsvTest.java`

**Naming:**
- Java uses `*Test.java`
- No distinct integration-test or e2e naming pattern was found

**Structure:**
```text
bookmap-addons/
|-- src/main/java/com/bookmap/alertlistener/AlertListener.java
`-- src/test/java/com/bookmap/alertlistener/AlertListenerCsvTest.java
```

## Test Structure

**Observed suite organization:**
- One public test class per source concern
- Flat `@Test` methods with descriptive names such as `parsesHiddenBidDevelopment`
- Assertions focus on parsed field values and file output side effects

**Patterns:**
- Tests are deterministic and mostly pure-data checks
- Temporary files are used for CSV append behavior
- There is no shared fixture library or custom test harness yet

## Mocking

**Framework:**
- No mocking framework usage was observed in the existing Java tests

**Patterns:**
- Current tests prefer direct object construction over mocks
- External Bookmap runtime behavior is not deeply simulated

**What is being tested today:**
- CSV row parsing
- CSV escaping/serialization
- Output header behavior
- Bookmap-side semantic normalization for alert rows

## Coverage

**Current state:**
- Java parsing/export paths have some regression coverage
- cTrader indicator execution paths appear untested
- Python socket, model inference, and MT5 order placement appear untested
- TradingView scripts appear untested beyond manual platform validation

**Coverage target:**
- No explicit threshold or CI enforcement was found

## Test Types

**Unit Tests:**
- Present only for `bookmap-addons` CSV parsing logic

**Integration Tests:**
- Not formalized
- The repo relies heavily on manual integration with Bookmap, cTrader, MT5, and TradingView runtimes

**Manual Verification:**
- Build cTrader projects and verify copied `.algo` files load in cTrader
- Build Bookmap jars and verify addon behavior inside Bookmap
- Run `main/main.py` with MT5 open and feed socket traffic from Bookmap or cTrader-side senders

## Common Gaps

**Highest-risk untested areas:**
- cTrader `Initialize` / `Calculate` behavior in the very large indicator classes
- Python DOM/order-book path in `main/ai_analyzer.py`
- End-to-end Bookmap -> socket -> Python -> MT5 workflow
- Telegram notifier behavior and timeout scheduling logic

**Suggested testing direction for future work:**
- Expand JUnit tests around `AlertListener`
- Add Python unit tests around `AIAnalyzer` buffer processing and `OrderSimulator`
- Add smoke-build verification for all `.csproj` files after cTrader changes

---

*Testing analysis: 2026-03-18*
*Update when test patterns change*
