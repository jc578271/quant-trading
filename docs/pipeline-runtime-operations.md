# Pipeline Runtime Operations

All live runtime artifacts belong under the flat repo-root `runtime/` directory, while raw histories remain under `logs/*.jsonl`.

## Runtime Files

| File | Owner | Format | Notes |
|------|-------|--------|-------|
| `runtime/status.json` | Python status publisher | JSON | Top-level metadata is only `updated_at`, `session_started_at`, and `stages` |
| `runtime/socket_events.jsonl` | `main/socket_server.py` | JSONL | One normalized event tap record per line |
| `runtime/quarantine_events.jsonl` | `main/socket_server.py` | JSONL | One rejected event record per line |
| `logs/history_orderflowaggregated.jsonl` | `main/ai_analyzer.py` | JSONL | Stable order-flow history export |
| `logs/history_volumeprofile.jsonl` | `main/ai_analyzer.py` | JSONL | Stable volume-profile history export |
| `logs/history_wyckoff.jsonl` | `main/ai_analyzer.py` | JSONL | Stable Wyckoff-state history export |
| `logs/history_alertlistener_<alias>.jsonl` | Bookmap `AlertListener` | JSONL | Alert history per producer alias |
| `logs/trade_history.jsonl` | `main/order_simulator.py` | JSONL | Paper-trade lifecycle log |
| `runtime/model.pkl` | promoted by `scripts/promote_model.py` | Binary | `runtime/model.pkl` is a promoted artifact |
| `runtime/model_manifest.json` | promoted by `scripts/promote_model.py` | JSON | Active model manifest for runtime compatibility checks |

## JSONL Schemas

`runtime/socket_events.jsonl`

`received_at, client, raw, normalized`

`runtime/quarantine_events.jsonl`

`received_at, reason, raw`

## Raw History Location

Raw order-flow, volume-profile, Wyckoff, Bookmap alert-listener, and trade histories remain under `logs/*.jsonl`. Promotion only affects `runtime/model.pkl` and `runtime/model_manifest.json`.

## Ownership Rules

- Python owns the flat runtime artifact layout and stable filenames.
- Bookmap `AlertListener` does not create duplicate alert-history CSV or log files in this phase.
- Operators should inspect `runtime/status.json` first for ingest, buffering, inference, and execution health.
- Operators should follow build -> train -> promote -> verify; offline training never activates runtime by itself.
