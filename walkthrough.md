# Walkthrough: Historical Data Backfill Simulation

I have enhanced [AlertListener.java](file:///c:/Users/hoang/projects/quant-trading/bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java) to support historical data backfill detection, allowing the addon to behave correctly when "simulating" historical ticks.

## Changes Made

### 1. Unified Historical Mode Detection
- Implemented `velox.api.layer1.simplified.HistoricalModeListener` (supported in Core API).
- Added `isLive` flag (volatile) to track state.
- Implemented [onRealtimeStart()](file:///c:/Users/hoang/projects/quant-trading/bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java#742-749) to signal the end of simulation.

### 2. Market Event Refinement
- **Log Markers**: All logs generated during backfill now start with [(HISTORY)](file:///c:/Users/hoang/projects/quant-trading/bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java#109-120) (e.g., `[DOT] (HISTORY) GC | BUY ...`).
- **Socket Suppression**: Historical `DOT`, `WALL`, and `ALERT` events are now suppressed from being sent to the Python AI socket, ensuring the AI only processes live data.
- **Alert Filtering**: Historical alerts are logged to the UI with the [(HISTORY)](file:///c:/Users/hoang/projects/quant-trading/bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java#109-120) tag but do not trigger popups or socket exports.

### 3. Visual Verification
- Added a system log message: `SYSTEM: Real-time data started. Historical backfill complete.` when the simulation ends.

## Verification Plan
- [x] Compilation: Verified correct interface and package names from API jars.
- [ ] Integration: User can run the addon and observe [(HISTORY)](file:///c:/Users/hoang/projects/quant-trading/bookmap-addons/src/main/java/com/bookmap/alertlistener/AlertListener.java#109-120) logs during loading.
- [ ] Transition: Observe the system message when backfill finishes.
