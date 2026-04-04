---
status: passed
updated: "2026-04-04T20:15:00+07:00"
phase: "07"
requirements:
  - OHLC-01
  - OHLC-02
  - OHLC-03
  - OHLC-04
---

# Phase 7 Verification

## Goal

Create a dedicated cTrader indicator that exports OHLC bar data as event-contract/v1 JSONL for AI model training, with configurable date range and dual file and socket export.

## Automated Verification

- Targeted project build succeeded for `ctrader-projects/OhlcTrainingExporterV10/src/OhlcTrainingExporterV10.csproj`
- Full solution cTrader build succeeded through `ctrader-projects/Build-CTraderProjects.ps1`
- `OhlcTrainingExporterV10.algo` was copied to `C:\Users\hoang\Documents\cAlgo\Sources\Indicators`

## Requirement Coverage

- `OHLC-01`: `BuildHistoryRecord`, `AppendDirectHistoryJsonl`, and the exporter constants implement the `ohlc-history/v1` and `event-contract/v1` envelopes
- `OHLC-02`: `LoadTickFrom_Data`, `StringDate`, and `TryResolveFromDateTime` implement the required date selection flow
- `OHLC-03`: `ConnectSocket`, `SendHeartbeat`, `SendSocketLine`, and `SendSocketData` implement live socket streaming to `127.0.0.1:5555`
- `OHLC-04`: The project follows the split-file cTrader structure and is wired into the shared build pipeline

## Human Verification

- Load the indicator in cTrader on a live or replay chart
- Trigger an export with a custom date and inspect `logs/history_ohlc_{symbol}.jsonl`
- Confirm the socket status display turns connected when the Python bridge is listening on `127.0.0.1:5555`
