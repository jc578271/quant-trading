# Phase 7: cTrader OHLC Training Data Exporter - Research

**Researched:** 2026-04-04
**Domain:** cTrader indicator development for OHLC bar data JSONL export
**Confidence:** HIGH

<user_constraints>
## User Constraints

- Follow event-contract/v1 schema exactly
- Both file JSONL and socket export (dual-export)
- New separate project in ctrader-projects/
- Date input, UI, export logic must match existing indicators exactly
- Output is training data for AI entry-point models
- Other indicators (Wyckoff, OrderFlow, VolumeProfile) serve as context providers
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|-----------------|
| OHLC-01 | Indicator exports OHLC bar data as event-contract/v1 JSONL with configurable date range | Existing Patterns, Architecture |
| OHLC-02 | Date input follows existing LoadTickFrom pattern (Today/Yesterday/OneWeek/Custom dd/MM/yyyy) | Existing Patterns |
| OHLC-03 | Socket export streams live OHLC events to 127.0.0.1:5555 with heartbeat | Existing Patterns |
| OHLC-04 | Split-file project structure matches existing cTrader indicator conventions | Build System |
</phase_requirements>

## Summary

This is a straightforward new cTrader indicator that reuses well-established patterns from 3 existing indicators in the same repo. The Wyckoff indicator is the closest template because it also exports flat per-bar data (not level arrays). The key difference: this indicator's payload is minimal — just OHLC, spread, and tick_size — because its purpose is to provide raw price data for AI training, not analysis features.

**Primary recommendation:** Clone the Wyckoff indicator's Lifecycle, ParamsPanel, and Styles patterns wholesale, strip all Wyckoff-specific analysis, and keep only the OHLC export payload.

## Existing Patterns (Verified from Code)

### Date Input Pattern
```csharp
public enum LoadTickFrom_Data { Today, Yesterday, Before_Yesterday, One_Week, Two_Week, Monthly, Custom }

[Parameter("Load From:", DefaultValue = LoadTickFrom_Data.Today, Group = "==== Export Settings ====")]
public LoadTickFrom_Data LoadTickFrom_Input { get; set; }

[Parameter("Custom (dd/mm/yyyy):", DefaultValue = "00/00/0000", Group = "==== Export Settings ====")]
public string StringDate { get; set; }
```

### Date Parsing
```csharp
DateTime customDate;
if (!DateTime.TryParseExact(StringDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out customDate))
{
    Notifications.ShowPopup("Invalid date format", "Use dd/MM/yyyy", PopupNotificationState.Error);
    return;
}

DateTime fromDateTime = LoadTickFrom_Input switch {
    LoadTickFrom_Data.Today => DateTime.Now.Date,
    LoadTickFrom_Data.Yesterday => DateTime.Now.Date.AddDays(-1),
    // ...
    LoadTickFrom_Data.Custom => customDate,
    _ => DateTime.Now.Date
};
```

### JSONL Export Pattern
- `_isManualCsvExportInProgress` flag
- `ClearAndRecalculate()` triggers `Calculate()` for every bar
- In `Calculate()`, if flag is set, call `ExportCsvData(index)`
- `ExportCsvData` builds payload via `BuildExportPayload`, writes via `AppendDirectHistoryJsonl`
- `AppendDirectHistoryJsonl` uses `StreamWriter(append: true, Utf8NoBom)` — one JSON per line

### Socket Export Pattern
- TCP client to 127.0.0.1:5555
- Connection hello on connect
- Heartbeat every 5 seconds
- Reconnect on disconnect with 5s delay
- Send data only on `IsLastBar`

### File Naming
- `history_ohlc_{SanitizedSymbol}.jsonl` in `D:\projects\quant-trading\logs`

### Event Contract v1 Envelope
```json
{
  "schema": "event-contract/v1",
  "source": "ctrader",
  "source_instance": "OhlcTrainingExporterV10",
  "event": "ohlc_bar",
  "event_id": "ctrader-ohlc_bar-{symbol}-{timestamp}",
  "instrument": "{symbol}",
  "timestamp": "{ISO8601}",
  "payload": {
    "open": 1.0950,
    "high": 1.0960,
    "low": 1.0940,
    "close": 1.0955,
    "spread": 0.0002,
    "tick_size": 0.00001
  },
  "source_meta": {
    "symbol": "EURUSD",
    "timeframe": "m5"
  }
}
```

### History Schema (JSONL record)
```json
{
  "schema": "ohlc-history/v1",
  "source": "ctrader",
  "source_instance": "OhlcTrainingExporterV10",
  "event": "ohlc_bar",
  "event_id": "ctrader-ohlc_bar-{symbol}-{timestamp}",
  "instrument": "{symbol}",
  "timestamp": "{ISO8601}",
  "bar_closed": true,
  "bar": {
    "open": 1.0950,
    "high": 1.0960,
    "low": 1.0940,
    "close": 1.0955,
    "spread": 0.0002,
    "tick_size": 0.00001,
    "range_ticks": 20
  },
  "source_meta": {
    "symbol": "EURUSD",
    "timeframe": "m5",
    "history_mode": true
  }
}
```

## Architecture

### File Structure
```
ctrader-projects/
  OhlcTrainingExporterV10/
    split-manifest.json
    src/
      OHLC Training Exporter v1.0.cs           # Main: attributes, parameters, fields
      OHLC Training Exporter v1.0.Lifecycle.cs  # Initialize, Calculate, export logic
      OHLC Training Exporter v1.0.ParamsPanel.cs # UI panel, date input, buttons
      OHLC Training Exporter v1.0.Styles.cs     # Dark/light theme colors
      OhlcTrainingExporterV10.csproj            # Project file
```

### Class: `OhlcTrainingExporterV10 : Indicator`
- `[Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]`
- Partial class across 4 files

### Parameters
| Parameter | Type | Group | Default |
|-----------|------|-------|---------|
| `LoadTickFrom_Input` | `LoadTickFrom_Data` enum | Export Settings | Today |
| `StringDate` | string | Export Settings | "00/00/0000" |
| `CsvOutputFolder` | string | Python AI Export | `D:\projects\quant-trading\logs` |
| `PanelAlignment_Input` | `PanelAlignment` enum | Panel | Bottom_Right |

### UI Elements
- Export button (triggers manual JSONL export)
- Reconnect button (for socket)
- Status indicator (socket connection state)
- Panel with date selection, export folder, alignment

## Implementation Slices

### Slice 1: Project scaffold and main class
- Create csproj, main .cs file with parameters and fields
- Create Styles.cs with dark/light theme

### Slice 2: Lifecycle and export logic
- Initialize(): socket, event subscriptions, UI setup
- Calculate(): bar data collection, export when flagged
- Export methods: BuildExportPayload, AppendDirectHistoryJsonl, SendSocketData

### Slice 3: ParamsPanel UI
- ParamDefinition array matching existing patterns
- Date input handling with validation
- Export/Reconnect buttons
- Panel rendering and alignment

### Slice 4: Build and split-manifest
- split-manifest.json configuration
- Build verification
- Copy to cAlgo indicators directory

## Validation Architecture

### Test Type
Build-time verification (C# compilation) + manual functional test on cTrader

### Verification Steps
1. Project compiles via `Build-CTraderProjects.ps1`
2. `.algo` file produced and copied to cAlgo indicators directory
3. Indicator loads on a cTrader chart
4. Export button produces `history_ohlc_{symbol}.jsonl` with correct schema
5. Socket connection sends data to 127.0.0.1:5555

## Sources

### Primary (HIGH confidence)
- `ctrader-projects/WeisWyckoffSystemV20/src/*.cs` — primary template for all patterns
- `ctrader-projects/OrderFlowAggregatedV20/src/*.cs` — secondary reference
- `ctrader-projects/FreeVolumeProfileV20/src/*.cs` — secondary reference
- `ctrader-projects/Directory.Build.props` — build configuration
- `ctrader-projects/Directory.Build.targets` — post-build copy
- `ctrader-projects/Build-CTraderProjects.ps1` — build script

## Metadata

**Confidence breakdown:**
- Existing patterns: HIGH — directly verified from production indicator code
- Architecture: HIGH — follows exact same structure as 3 existing indicators
- Build system: HIGH — reuses existing Directory.Build.* infrastructure

**Research date:** 2026-04-04
**Valid until:** 2026-05-04 or until cTrader Automate SDK changes
