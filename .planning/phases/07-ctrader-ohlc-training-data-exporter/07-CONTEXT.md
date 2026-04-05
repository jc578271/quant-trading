# Phase 7: cTrader OHLC Training Data Exporter - Context

**Gathered:** 2026-04-04
**Source:** User request + existing cTrader indicator analysis

## Goal

Create a new cTrader indicator (`OhlcTrainingExporterV10`) that exports OHLC bar data (open, high, low, close) as JSONL for AI training. The indicator follows event-contract/v1 schema and matches the UI/export/date-input patterns of existing indicators (Wyckoff, OrderFlow, VolumeProfile).

## User Requirements

1. **Export JSONL** with OHLC data (highest, lowest, open, close) for each bar
2. **Date input**: User selects a start date (e.g., 12/03/2026), exports from that date to the latest bar
3. **Timeframe aware**: Exports bars at the chart's current timeframe (e.g., M5)
4. **Purpose**: Training data for AI entry-point signals; other cTrader indicators (Wyckoff, OrderFlow, VolumeProfile) provide context
5. **Match existing patterns**: Logic, UI export panel, date input handling identical to existing indicators
6. **Event-contract/v1 schema**: Follow the project's canonical event envelope
7. **Both file JSONL and socket export**: Full dual-export like existing indicators
8. **New separate project**: `OhlcTrainingExporterV10/` in `ctrader-projects/`

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| New project `OhlcTrainingExporterV10` | Avoids breaking existing indicator functionality |
| Event name: `ohlc_bar` | Distinguishes from `wyckoff_state`, `order_flow_aggregated`, `volume_profile` |
| History schema: `ohlc-history/v1` | Follows versioned schema pattern |
| History file: `history_ohlc_{symbol}.jsonl` | Follows `history_{type}_{symbol}.jsonl` naming |
| Minimal payload: OHLC + spread + tick_size | Just the price data needed for AI training context |
| Same LoadTickFrom_Data enum | Consistent date selection with existing indicators |
| Same ParamsPanel pattern | Consistent UI across all indicators |

## Constraints

- Must use `cTrader.Automate` NuGet package (version 1.0.14 via Directory.Packages.props)
- Must target `net6.0` (via Directory.Build.props)
- Must use partial class split-file pattern for maintainability
- Must follow UTF-8 without BOM for all text exports
- Must use ISO8601 timestamps (.ToString("o"))
- Must use CultureInfo.InvariantCulture for all parsing
- Output folder default: `D:\projects\quant-trading\logs`
- Socket target: `127.0.0.1:5555`

## Dependencies

- No dependency on other phases (standalone cTrader indicator)
- Produces JSONL that Phase 6 training pipeline can consume
