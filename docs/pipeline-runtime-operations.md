# Pipeline Runtime Operations

All live runtime artifacts for Phase 2 belong under the flat repo-root `runtime/` directory.

## Runtime Files

| File | Owner | Format | Notes |
|------|-------|--------|-------|
| `runtime/status.json` | Python status publisher | JSON | Top-level metadata is only `updated_at`, `session_started_at`, and `stages` |
| `runtime/socket_events.jsonl` | `main/socket_server.py` | JSONL | One normalized event tap record per line |
| `runtime/quarantine_events.jsonl` | `main/socket_server.py` | JSONL | One rejected event record per line |
| `runtime/history_order_flow_aggregated.csv` | `main/ai_analyzer.py` | CSV | Stable order-flow history export |
| `runtime/history_volume_profile.csv` | `main/ai_analyzer.py` | CSV | Stable volume-profile history export |
| `runtime/history_wyckoff_state.csv` | `main/ai_analyzer.py` | CSV | Stable Wyckoff-state history export |
| `runtime/history_alert_<alias>.csv` | `main/ai_analyzer.py` | CSV | Alert history per producer alias |
| `runtime/trade_history.csv` | `main/order_simulator.py` | CSV | Paper-trade lifecycle log |
| `runtime/model.pkl` | `main/ai_analyzer.py` | Binary | Local model artifact |

## JSONL Schemas

`runtime/socket_events.jsonl`

`received_at, client, raw, normalized`

`runtime/quarantine_events.jsonl`

`received_at, reason, raw`

## CSV Headers

`runtime/history_order_flow_aggregated.csv`

`timestamp,instrument,symbol,source,source_instance,event,deltaRank,volumesRank,volumesRankUp,volumesRankDown,spread`

`runtime/history_volume_profile.csv`

`timestamp,instrument,symbol,source,source_instance,event,profile_type,vpPOC,vpVAH,vpVAL,spread`

`runtime/history_wyckoff_state.csv`

`timestamp,instrument,symbol,source,source_instance,event,wyckoffVolume,wyckoffTime,zigZag,waveVolume,wavePrice,waveVolPrice,waveDirection,spread`

`runtime/history_alert_<alias>.csv`

`Timestamp,AlertNumber,Symbol,AlertName,Value,Price,Popup,RawText`

`runtime/trade_history.csv`

`timestamp,symbol,direction,entry_price,sl_price,tp_price,lot_size,exit_price,exit_reason,pnl_pips,pnl_dollar,pnl_percent,balance_before,balance_after`

## Ownership Rules

- Python owns the flat runtime artifact layout and stable filenames.
- Bookmap `AlertListener` does not create duplicate alert-history CSV or log files in this phase.
- Operators should inspect `runtime/status.json` first for ingest, buffering, inference, and execution health.
