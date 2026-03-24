# AI Model Data Contract

## Promoted Baseline Scope

The first promoted training scope is `ctrader_xauusd_h1_baseline`.

## Anchor Series

The anchor series for this baseline is `order_flow_aggregated` from `logs/history_orderflowaggregated_XAUUSD.jsonl`.
Each derived row is anchored to one closed `XAUUSD` H1 order-flow record.

## Canonical Market Mapping

Bookmap events may enrich a derived row only after alias-root mapping through `configs/training/instrument_map.json`.

Canonical mapping examples:

- `XAUUSD -> GC` root, including `GC*` Bookmap aliases
- `US500 -> ES` root, including `ES*` Bookmap aliases

Rows that fail canonical-market matching are excluded from the baseline manifest rather than merged implicitly.
