from __future__ import annotations

DATASET_SCOPE_CTRADER_XAUUSD_H1_BASELINE = "ctrader_xauusd_h1_baseline"
CANONICAL_MARKET_MAP = {"XAUUSD": ("GC",), "US500": ("ES",)}

SCOPE_TO_INSTRUMENT = {
    DATASET_SCOPE_CTRADER_XAUUSD_H1_BASELINE: "XAUUSD",
}
ANCHOR_EVENT_BY_SCOPE = {
    DATASET_SCOPE_CTRADER_XAUUSD_H1_BASELINE: "order_flow_aggregated",
}
ANCHOR_TIMEFRAME_BY_SCOPE = {
    DATASET_SCOPE_CTRADER_XAUUSD_H1_BASELINE: "h1",
}

DERIVED_DATASET_FIELDS = (
    "row_id",
    "dataset_scope",
    "instrument",
    "canonical_market",
    "anchor_timestamp",
    "open",
    "high",
    "low",
    "close",
    "spread",
    "tick_size",
    "has_volume_profile",
    "has_wyckoff_window",
    "has_bookmap_window",
    "quarantined_source_count",
)
