# Event Contract v1

This document defines the first shared envelope for local pipeline events emitted by Bookmap and cTrader producers.

## Schema

- `schema: event-contract/v1`
- Transport: newline-delimited JSON over the existing local socket boundary
- Scope: Bookmap `alert`, `dom`, `dot`, `wall` and cTrader `order_flow_aggregated`, `volume_profile`, `wyckoff_state`

## Required Envelope

Every producer-required event must emit exactly these top-level fields:

```json
{
  "schema": "event-contract/v1",
  "source": "bookmap",
  "source_instance": "AlertListener",
  "event": "dot",
  "event_id": "bookmap-dot-ESH6.CME@RITHMIC-2026-03-20T09:15:31.456Z",
  "instrument": "ESH6.CME@RITHMIC",
  "timestamp": "2026-03-20T09:15:31.456Z",
  "payload": {},
  "source_meta": {}
}
```

## Field Rules

- `schema`: must be the literal contract identifier `event-contract/v1`
- `source`: producer family such as `bookmap` or `ctrader`
- `source_instance`: concrete emitter name, strategy, addon, or indicator instance
- `event`: canonical event family name from the event taxonomy
- `event_id`: producer-generated stable unique id for the record
- `instrument`: canonical instrument identity for downstream routing
- `timestamp`: event time supplied by the producer, in UTC ISO-8601 form
- `payload`: event-specific body
- `source_meta`: raw producer identity and debugging context that should not be discarded

## Event Taxonomy

The v1 event taxonomy is:

- `alert`
- `dom`
- `dot`
- `wall`
- `order_flow_aggregated`
- `volume_profile`
- `wyckoff_state`

Event names are case-sensitive and must use these exact lowercase values. Unsupported event names are quarantined.

## Canonical Instrument Rule

- `instrument` must prefer the venue-qualified raw identity when the producer has one.
- Example preferred identities: `ESH6.CME@RITHMIC`, `NQH6.CME@RITHMIC`
- Raw source fields such as Bookmap `alias` and cTrader `symbol` must still be preserved in `source_meta`.
- If a producer only has a symbol-like identifier in v1, that value may be used as `instrument`, but the raw field still belongs in `source_meta`.
- If no valid instrument can be derived, the record is quarantined.

## Timestamp Rule

- Producers must stamp the event time themselves.
- `timestamp` is the market event or producer event time, not a Python receipt time.
- `received_at` is added by the Python boundary.
- `received_at` must never be invented by a producer as a substitute for `timestamp`.

## Payload Expectations

- Keep cross-cutting metadata in the envelope and event-specific fields in `payload`.
- Preserve raw identity, routing, and debugging details in `source_meta`.
- Payloads may differ by event family; v1 does not force Bookmap and cTrader into one identical domain payload.

### Bookmap payload guidance

- `alert`: keep alert text, alert count, and popup state in `payload`; preserve raw `symbol` in `source_meta`
- `dom`: keep DOM action and level values in `payload`; preserve raw `alias` in `source_meta`
- `dot`: keep trade direction and aggregate size in `payload`; preserve raw `alias` in `source_meta`
- `wall`: keep wall action, side, price, size, and duration in `payload`; preserve raw `alias` in `source_meta`

### cTrader payload guidance

- `order_flow_aggregated`: preserve exported order-flow metrics in `payload`; keep raw `symbol` and timeframe in `source_meta`
- `volume_profile`: preserve profile metrics in `payload`; keep raw `symbol`, timeframe, and `profile_type` in `source_meta`
- `wyckoff_state`: preserve Wyckoff and wave state values in `payload`; keep raw `symbol` and timeframe in `source_meta`

## Quarantine Triggers

The Python ingest boundary must quarantine records that hit any of these conditions:

- `missing instrument`
- `missing timestamp`
- `unsupported event`
- `schema mismatch`

Quarantined records must not enter the analyzer path.

## Producer Notes

- Bookmap `AlertListener` is the phase-1 reference producer for the v1 envelope.
- cTrader exporters remain in scope for the same contract even if they are normalized later in the phase.
- Legacy top-level keys such as `type`, `alias`, or `symbol` do not replace the envelope fields in v1. They belong in `payload` or `source_meta`.

## Bookmap Example

```json
{
  "schema": "event-contract/v1",
  "source": "bookmap",
  "source_instance": "AlertListener",
  "event": "wall",
  "event_id": "bookmap-wall-ESH6.CME@RITHMIC-2026-03-20T09:12:30.000Z",
  "instrument": "ESH6.CME@RITHMIC",
  "timestamp": "2026-03-20T09:12:30.000Z",
  "payload": {
    "action": "added",
    "isBid": true,
    "price": 6025,
    "size": 180,
    "duration": 7
  },
  "source_meta": {
    "alias": "ESH6.CME@RITHMIC"
  }
}
```

## cTrader Example

```json
{
  "schema": "event-contract/v1",
  "source": "ctrader",
  "source_instance": "OrderFlowAggregatedV20",
  "event": "order_flow_aggregated",
  "event_id": "ctrader-order-flow-aggregated-US100-2026-03-20T09:10:00.000Z",
  "instrument": "US100",
  "timestamp": "2026-03-20T09:10:00.000Z",
  "payload": {
    "open": 20482.1,
    "high": 20496.8,
    "low": 20470.4,
    "close": 20490.2,
    "volumesRank": 78.0,
    "delta": 1520.0
  },
  "source_meta": {
    "symbol": "US100",
    "timeframe": "Minute1"
  }
}
```

## Compatibility Notes

- Downstream Python normalization may temporarily wrap known legacy producer shapes during rollout, but canonical downstream handling should target this envelope.
- Future revisions that add fields must version the schema instead of silently extending `event-contract/v1`.
