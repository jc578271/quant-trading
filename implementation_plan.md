# Fix Replay Mode Time Synchronization

The Bookmap [AlertListener](file:///c:/Users/hoang/projects/quant-trading/bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java#27-796) addon currently uses `System.currentTimeMillis()` to track trade aggregation windows, wall durations, and log timestamps. This uses the host machine's literal clock, which works perfectly in Live trading but breaks completely during Replay Mode. In Replay Mode, historical data is processed much faster than real-time, meaning the simulation clock diverges from the system clock. As a result, timeout logic (like `$50$ms` aggregation) evaluates incorrectly, or logs no longer show up.

## Proposed Changes

### AlertListener.java
- **[MODIFY] [AlertListener.java](file:///c:/Users/hoang/projects/quant-trading/bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java)**
  - Add a helper method `getCurrentTimeMs()` that returns `provider.getCurrentTime() / 1_000_000L`. Bookmap provides timestamps in Nanoseconds, so this converts the simulated timestamp to milliseconds.
  - Replace all occurrences of `System.currentTimeMillis()` for `lastTime`, `startTime`, and `firstSeenTime` calculations with `getCurrentTimeMs()`.
  - Update [logToUI()](file:///c:/Users/hoang/projects/quant-trading/bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java#191-198) and [exportAlert()](file:///c:/Users/hoang/projects/quant-trading/bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java#476-494) functions to format the simulation time (`provider.getCurrentTime()`) into the `yyyy-MM-dd'T'HH:mm:ss.SSSSSSS'Z'` timestamp instead of using `ZonedDateTime.now(...)`. This ensures that logs written to disk and exported to Python have the correct historical simulation timestamps.

### TestClass.java
- **[DELETE] [TestClass.java](file:///c:/Users/hoang/projects/quant-trading/bookmap-addons/src/main/java/com/bookmap/alertlistener/TestClass.java)**
  - Remove the temporary test class used for verifying the API.

## Verification Plan

### Automated/Compilation Verification
- Execute `./gradlew build` utilizing the Bookmap bundled JDK 17 to ensure that `provider.getCurrentTime()` resolves successfully against the `api-simplified` and `api-core` dependencies.

### Manual Verification
- Rebuild the Bookmap Addon JAR.
- Load the addon in Bookmap and enable **Replay Mode** at multiple speeds (e.g., 10x, 50x).
- Verify that Liquidity Walls and Volume Dots log messages appear in the chart tabs without delays or grouping anomalies.
- Verify that the timestamp prefixes on the printed logs in the UI exactly match the historical time on the Bookmap timeline, not the current real-world clock time.
