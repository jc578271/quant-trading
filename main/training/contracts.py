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
    "anchor_timeframe",
    "orderflow_event_id",
    "volumeprofile_event_id",
    "wyckoff_window_start",
    "wyckoff_window_end",
    "open",
    "high",
    "low",
    "close",
    "spread",
    "tick_size",
    "level_count",
    "total_volume",
    "buy_volume",
    "sell_volume",
    "delta_sum",
    "abs_delta_sum",
    "poc_price",
    "poc_distance_to_close_ticks",
    "wy_count",
    "wy_last_direction_sign",
    "wy_last_wave_volume",
    "wy_sum_wave_volume",
    "wy_mean_volume_per_time",
    "wy_max_wave_efficiency",
    "has_volume_profile",
    "has_wyckoff_window",
    "has_bookmap_window",
    "bookmap_event_count_300s",
    "bookmap_dot_count_300s",
    "bookmap_stop_count_300s",
    "bookmap_wall_count_300s",
    "bookmap_signed_value_300s",
    "quarantined_source_count",
)
