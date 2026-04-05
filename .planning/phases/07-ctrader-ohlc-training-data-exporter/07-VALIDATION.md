# Phase 7: cTrader OHLC Training Data Exporter - Validation

**Created:** 2026-04-04

## Validation Approach

Build-time compilation verification + runtime functional test on cTrader platform.

## Phase Requirements -> Test Map

| Req ID | Behavior | Test Type | Automated Command | Exists? |
|--------|----------|-----------|-------------------|---------|
| OHLC-01 | Indicator exports OHLC as event-contract/v1 JSONL | build + manual | `Build-CTraderProjects.ps1` | No (Wave 0) |
| OHLC-02 | Date input follows LoadTickFrom pattern | build | compilation check | No (Wave 0) |
| OHLC-03 | Socket export streams to 127.0.0.1:5555 | build + manual | compilation check | No (Wave 0) |
| OHLC-04 | Split-file project structure | build | `Build-CTraderProjects.ps1` | No (Wave 0) |

## Build Verification

```powershell
# Full build
.\ctrader-projects\Build-CTraderProjects.ps1

# Check .algo output
Test-Path "$env:USERPROFILE\Documents\cAlgo\Sources\Indicators\OhlcTrainingExporterV10.algo"
```

## Functional Verification (Manual on cTrader)

1. Load indicator on any chart (e.g., XAUUSD M5)
2. Set "Load From" to "Custom", enter a date like "01/03/2026"
3. Click "Export" button
4. Verify `logs/history_ohlc_XAUUSD.jsonl` contains correct schema and OHLC data
5. Verify each line has `schema: "ohlc-history/v1"`, `event: "ohlc_bar"`, and OHLC in `bar`
6. Verify socket connection status shows "connected" when Python bridge is running

## Sampling Rate

- **Per task:** Build compilation check
- **Phase gate:** Full build + manual cTrader test
