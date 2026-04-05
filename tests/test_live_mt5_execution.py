from __future__ import annotations

import json

import joblib
import numpy as np

from ai_analyzer import AIAnalyzer


class StatusProbe:
    def __init__(self) -> None:
        self.updates: list[dict] = []

    def update_stage(self, stage: str, **kwargs) -> None:
        self.updates.append({"stage": stage, **kwargs})

    def publish(self, now=None) -> None:
        return None


class StubModel:
    def __init__(self, prediction: int, probability: float) -> None:
        self.prediction = prediction
        self.probability = probability

    def predict(self, features):
        return np.array([self.prediction])

    def predict_proba(self, features):
        if self.prediction == 1:
            return np.array([[1.0 - self.probability, self.probability]])
        return np.array([[self.probability, 1.0 - self.probability]])


def _write_runtime_bundle(runtime_root) -> None:
    runtime_root.mkdir(parents=True, exist_ok=True)
    _write_serialized_model(runtime_root / "model.pkl")
    (runtime_root / "feature_schema.json").write_text(
        json.dumps({"feature_order": _expected_feature_schema()}, indent=2),
        encoding="utf-8",
    )
    (runtime_root / "train_config.json").write_text(
        json.dumps(
            {
                "label": {
                    "type": "future_return_classification",
                    "horizon_bars": 1,
                    "long_threshold_ticks": 150,
                    "short_threshold_ticks": -150,
                }
            },
            indent=2,
        ),
        encoding="utf-8",
    )
    (runtime_root / "dataset_manifest.json").write_text(
        json.dumps({"dataset_scope": "ctrader_xauusd_h1_baseline"}, indent=2),
        encoding="utf-8",
    )
    (runtime_root / "model_manifest.json").write_text(
        json.dumps(
            {
                "dataset_scope": "ctrader_xauusd_h1_baseline",
                "run_id": "run-001",
                "feature_schema_path": "runtime/feature_schema.json",
                "label_spec": {
                    "type": "future_return_classification",
                    "horizon_bars": 1,
                    "long_threshold_ticks": 150,
                    "short_threshold_ticks": -150,
                },
                "created_at": "2026-03-24T16:00:00Z",
            },
            indent=2,
        ),
        encoding="utf-8",
    )


def _expected_feature_schema() -> list[str]:
    return [
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
    ]


def _write_serialized_model(path) -> None:
    from sklearn.ensemble import RandomForestClassifier

    X_train = np.array(
        [
            [1000, 10000, 5500, 4500, 200, 250, 3000, -50, 3, 1, 120, 360, 18.0, 12.0, 1, 1, 0, 0, 0, 0, 0, 0],
            [-1000, 10000, 4500, 5500, -200, 250, 3000, 50, 3, -1, 120, 360, 18.0, 12.0, 1, 1, 0, 0, 0, 0, 0, 0],
            [0, 8000, 4000, 4000, 0, 100, 3000, 0, 2, 0, 80, 220, 14.0, 9.0, 1, 1, 0, 0, 0, 0, 0, 0],
        ]
    )
    y_train = np.array([1, -1, 0])
    model = RandomForestClassifier(n_estimators=10, random_state=42)
    model.fit(X_train, y_train)
    joblib.dump(model, path)


def test_ai_analyzer_routes_live_signal_to_mt5_when_enabled(runtime_root, fake_mt5_client, monkeypatch):
    _write_runtime_bundle(runtime_root)
    monkeypatch.setenv("QT_ENABLE_LIVE_MT5", "1")
    monkeypatch.setenv("QT_MT5_RISK_PERCENT", "0.25")
    monkeypatch.setenv("QT_MT5_SL_PIPS", "15")
    monkeypatch.setenv("QT_MT5_RR_RATIO", "2.5")
    monkeypatch.setenv("QT_MT5_ORDER_COOLDOWN_SECONDS", "0")

    status = StatusProbe()
    analyzer = AIAnalyzer(fake_mt5_client, status=status)
    fake_mt5_client.connected = True
    analyzer.model = StubModel(prediction=1, probability=0.9)

    analyzer.raw_data_buffer["XAUUSD"] = [
        {
            "instrument": "XAUUSD",
            "timestamp": "2026-03-24T16:00:00Z",
            "close": 3000.0,
            "tick_size": 0.01,
            "level_count": 10,
            "total_volume": 120.0,
            "buy_volume": 80.0,
            "sell_volume": 40.0,
            "delta_sum": 40.0,
            "abs_delta_sum": 60.0,
            "poc_price": 2999.5,
            "wy_count": 1,
            "wy_last_direction_sign": 1,
            "wy_last_wave_volume": 55.0,
            "wy_sum_wave_volume": 55.0,
            "wy_mean_volume_per_time": 5.5,
            "wy_max_wave_efficiency": 2.0,
            "has_volume_profile": 1,
            "has_wyckoff_window": 1,
        }
    ]
    analyzer.latest_state["XAUUSD"] = {}

    analyzer._process_buffer("XAUUSD")

    assert len(fake_mt5_client.placed_orders) == 1
    assert fake_mt5_client.placed_orders[0]["symbol"] == "XAUUSD"
    assert fake_mt5_client.placed_orders[0]["order_type"] == "BUY"
    assert fake_mt5_client.placed_orders[0]["risk_percentage"] == 0.25
    assert fake_mt5_client.placed_orders[0]["sl_pips"] == 15.0
    assert fake_mt5_client.placed_orders[0]["rr_ratio"] == 2.5


def test_ai_analyzer_skips_live_signal_when_position_already_open(runtime_root, fake_mt5_client, monkeypatch):
    _write_runtime_bundle(runtime_root)
    monkeypatch.setenv("QT_ENABLE_LIVE_MT5", "1")
    monkeypatch.setenv("QT_MT5_ORDER_COOLDOWN_SECONDS", "0")

    status = StatusProbe()
    analyzer = AIAnalyzer(fake_mt5_client, status=status)
    fake_mt5_client.connected = True
    fake_mt5_client.open_positions.add("XAUUSD")
    analyzer.model = StubModel(prediction=-1, probability=0.95)

    analyzer.raw_data_buffer["XAUUSD"] = [
        {
            "instrument": "XAUUSD",
            "timestamp": "2026-03-24T16:00:00Z",
            "close": 3000.0,
            "tick_size": 0.01,
            "level_count": 10,
            "total_volume": 120.0,
            "buy_volume": 80.0,
            "sell_volume": 40.0,
            "delta_sum": 40.0,
            "abs_delta_sum": 60.0,
            "poc_price": 2999.5,
            "wy_count": 1,
            "wy_last_direction_sign": 1,
            "wy_last_wave_volume": 55.0,
            "wy_sum_wave_volume": 55.0,
            "wy_mean_volume_per_time": 5.5,
            "wy_max_wave_efficiency": 2.0,
            "has_volume_profile": 1,
            "has_wyckoff_window": 1,
        }
    ]
    analyzer.latest_state["XAUUSD"] = {}

    analyzer._process_buffer("XAUUSD")

    assert fake_mt5_client.placed_orders == []
