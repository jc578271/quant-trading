# cTrader Contract Alignment v1

This document records how the three cTrader exporters map into `event-contract/v1` and where the cTrader and Bookmap producer families intentionally diverge.

## Canonical Event Mapping

| Exporter | source_instance | event | Legacy shape replaced |
| --- | --- | --- | --- |
| `OrderFlowAggregatedV20` | `OrderFlowAggregatedV20` | `order_flow_aggregated` | Flat payload with top-level `type`, bar fields, and rank maps |
| `FreeVolumeProfileV20` | `FreeVolumeProfileV20` | `volume_profile` | Flat payload with top-level `type`, `profile_type`, and volume profile metrics |
| `WeisWyckoffSystemV20` | `WeisWyckoffSystemV20` | `wyckoff_state` | Flat payload with no top-level `type`; analyzer no longer infers the event name |

All three exporters now emit the shared envelope:

- `schema = "event-contract/v1"`
- `source = "ctrader"`
- `source_instance = exporter-specific instance name`
- `event = canonical event name`
- `event_id = ctrader-{event}-{symbol}-{timestamp:o}`
- `instrument = Symbol.Name`
- `timestamp = Bars.OpenTimes[index].ToString("o")`
- `payload = event-specific analytics`
- `source_meta = raw producer identity`

## Source Meta Rules

Each cTrader exporter preserves raw producer identity in `source_meta`:

- `symbol`
- `timeframe`
- `legacy_type` for `order_flow_aggregated` and `volume_profile`

`WeisWyckoffSystemV20` does not emit `legacy_type` because the old shape never supplied a trustworthy top-level type. The canonical `wyckoff_state` event name is now explicit in the envelope.

## Payload Ownership By Exporter

### `order_flow_aggregated`

Owned payload keys:

- `open`
- `high`
- `low`
- `close`
- `volumesRank`
- `volumesRankUp`
- `volumesRankDown`
- `deltaRank`
- `minMaxDelta`
- `spread`

### `volume_profile`

Owned payload keys:

- `profile_type`
- `open`
- `high`
- `low`
- `close`
- `vpPOC`
- `vpVAH`
- `vpVAL`
- `vpTotalVolume`
- `volumesRank`
- `volumesRankUp`
- `volumesRankDown`
- `deltaRank`
- `minMaxDelta`
- `spread`

### `wyckoff_state`

Owned payload keys:

- `open`
- `high`
- `low`
- `close`
- `wyckoffVolume`
- `wyckoffTime`
- `zigZag`
- `waveVolume`
- `wavePrice`
- `waveVolPrice`
- `waveDirection`
- `spread`

## Unsupported Edges For v1

The current v1 alignment is intentionally asymmetric in a few places. These unsupported edges must stay explicit:

- cTrader exporters do not emit the Bookmap microstructure event families `alert`, `dom`, `dot`, or `wall`.
- Bookmap producers do not emit cTrader profile metrics such as `vpPOC`, `vpVAH`, `vpVAL`, `vpTotalVolume`, or `profile_type`.
- Bookmap producers do not emit cTrader Wyckoff and wave-state fields such as `wyckoffVolume`, `wyckoffTime`, `zigZag`, `waveVolume`, `wavePrice`, `waveVolPrice`, or `waveDirection`.
- cTrader exporters use `Symbol.Name` as the canonical `instrument` in v1 because they do not currently expose a richer venue-qualified identity at the emitter boundary.

## Alignment Notes

- cTrader exporters share the same envelope as Bookmap, but they keep source-specific analytics inside `payload`.
- CSV export remains backward-compatible with the legacy flat column names even though socket payloads now use nested `payload` and `source_meta`.
- Downstream normalization should treat the envelope as canonical and the legacy flat cTrader shape as transitional history only.
