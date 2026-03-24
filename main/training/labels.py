from __future__ import annotations

from typing import Any

import pandas as pd


def apply_future_return_labels(dataset: pd.DataFrame, label_config: dict[str, Any]) -> pd.DataFrame:
    horizon_bars = int(label_config["horizon_bars"])
    long_threshold_ticks = int(label_config["long_threshold_ticks"])
    short_threshold_ticks = int(label_config["short_threshold_ticks"])
    if horizon_bars != 1:
        raise ValueError("Wave 2 locks horizon_bars to 1 for the baseline config")

    labeled = dataset.sort_values("anchor_timestamp").reset_index(drop=True).copy()
    next_close = labeled["close"].shift(-horizon_bars)
    future_return_ticks_1 = (next_close - labeled["close"]) / labeled["tick_size"]
    labeled["future_return_ticks_1"] = future_return_ticks_1
    labeled["target_class_1"] = labeled["future_return_ticks_1"].apply(
        lambda value: _classify_future_return(
            value,
            long_threshold_ticks=long_threshold_ticks,
            short_threshold_ticks=short_threshold_ticks,
        )
    )
    labeled = labeled.dropna(subset=["future_return_ticks_1"]).reset_index(drop=True)
    labeled["target_class_1"] = labeled["target_class_1"].astype(int)
    return labeled


def _classify_future_return(
    value: float | None,
    *,
    long_threshold_ticks: int,
    short_threshold_ticks: int,
) -> int | None:
    if pd.isna(value):
        return None
    if value >= long_threshold_ticks:
        return 1
    if value <= short_threshold_ticks:
        return -1
    return 0
