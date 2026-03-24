from __future__ import annotations

import pandas as pd

from training.labels import apply_future_return_labels


LOCKED_LABEL_CONFIG = {
    "type": "future_return_classification",
    "horizon_bars": 1,
    "long_threshold_ticks": 150,
    "short_threshold_ticks": -150,
}


def test_future_return_ticks_uses_next_anchor_close_only():
    dataset = pd.DataFrame(
        [
            {"anchor_timestamp": "2026-03-20T10:00:00Z", "close": 100.0, "tick_size": 0.01},
            {"anchor_timestamp": "2026-03-20T11:00:00Z", "close": 102.0, "tick_size": 0.01},
            {"anchor_timestamp": "2026-03-20T12:00:00Z", "close": 101.0, "tick_size": 0.01},
        ]
    )

    labeled = apply_future_return_labels(dataset, LOCKED_LABEL_CONFIG)

    assert labeled["future_return_ticks_1"].tolist() == [200.0, -100.0]


def test_target_class_uses_locked_tick_thresholds():
    dataset = pd.DataFrame(
        [
            {"anchor_timestamp": "2026-03-20T10:00:00Z", "close": 100.0, "tick_size": 0.01},
            {"anchor_timestamp": "2026-03-20T11:00:00Z", "close": 102.0, "tick_size": 0.01},
            {"anchor_timestamp": "2026-03-20T12:00:00Z", "close": 100.0, "tick_size": 0.01},
            {"anchor_timestamp": "2026-03-20T13:00:00Z", "close": 100.5, "tick_size": 0.01},
        ]
    )

    labeled = apply_future_return_labels(dataset, LOCKED_LABEL_CONFIG)

    assert labeled["target_class_1"].tolist() == [1, -1, 0]


def test_label_builder_drops_rows_without_future_anchor_bar():
    dataset = pd.DataFrame(
        [
            {"anchor_timestamp": "2026-03-20T10:00:00Z", "close": 100.0, "tick_size": 0.01},
            {"anchor_timestamp": "2026-03-20T11:00:00Z", "close": 101.0, "tick_size": 0.01},
        ]
    )

    labeled = apply_future_return_labels(dataset, LOCKED_LABEL_CONFIG)

    assert len(labeled) == 1
    assert labeled["anchor_timestamp"].tolist() == ["2026-03-20T10:00:00Z"]
