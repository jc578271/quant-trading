from __future__ import annotations

import json
import logging
import os
from collections import defaultdict, deque
from datetime import UTC, datetime, timedelta
from pathlib import Path
from typing import Any

from order_simulator import OrderSimulator
from runtime_paths import (
    event_history_file,
    event_history_keys,
    model_file,
    model_manifest_file,
    runtime_root,
)

EXPECTED_DATASET_SCOPE = "ctrader_xauusd_h1_baseline"
EXPECTED_FEATURE_SCHEMA = [
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
INSTRUMENT_TO_CANONICAL_ROOT = {
    "XAUUSD": "GC",
    "US500": "ES",
}
CANONICAL_ROOT_TO_INSTRUMENT = {value: key for key, value in INSTRUMENT_TO_CANONICAL_ROOT.items()}
BOOKMAP_WINDOW = timedelta(seconds=300)
BOOKMAP_SIGN_BY_SIDE = {"BUY": 1.0, "ASK": 1.0, "SELL": -1.0, "BID": -1.0}


class AIAnalyzer:
    def __init__(self, mt5_client, model_path=None, sim_config=None, status=None):
        self.mt5_client = mt5_client
        self.status = status
        self.raw_data_buffer: dict[str, list[dict[str, Any]]] = {}
        self.latest_state: dict[str, dict[str, Any]] = {}
        self.order_book: dict[str, dict[bool, dict[Any, Any]]] = {}
        self.bookmap_windows_by_canonical_market: dict[str, deque[dict[str, Any]]] = defaultdict(deque)
        self.last_buffer_process_time = 0
        self.model_path = Path(model_path) if model_path else model_file()
        self.model_manifest_path = model_manifest_file()
        self.inference_errors_total = 0
        self.model_status_reason = "training artifact missing"
        self.feature_keys = list(EXPECTED_FEATURE_SCHEMA)
        self.model = None
        self.model = self._load_or_train_initial_model()

        self.simulator = OrderSimulator(
            sim_config
            or {
                "balance": 10000,
                "min_rr": 2.0,
                "sl_mode": "fixed",
                "sl_value": 20,
                "tp_mode": "rr",
                "tp_value": 2.0,
                "lot_mode": "auto",
                "lot_value": 1.0,
                "pip_size": 0.01,
                "pip_value_per_lot": 10.0,
            }
        )

    def _publish_buffering_status(self, reason, state="degraded"):
        if self.status is None:
            return
        buffered_symbols = sum(1 for records in self.raw_data_buffer.values() if records)
        queued_records = sum(len(records) for records in self.raw_data_buffer.values())
        self.status.update_stage(
            "buffering",
            state=state,
            reason=reason,
            buffered_symbols=buffered_symbols,
            queued_records=queued_records,
        )
        self.status.publish()

    def _publish_inference_status(self, reason, state="up", last_inference_at=None):
        if self.status is None:
            return
        self.status.update_stage(
            "inference",
            state=state,
            reason=reason,
            model_loaded=self.model is not None,
            last_inference_at=last_inference_at,
            inference_errors_total=self.inference_errors_total,
        )
        self.status.publish()

    def _load_or_train_initial_model(self):
        try:
            import joblib
            import numpy as np
            from sklearn.ensemble import RandomForestClassifier

            if self.model_path.exists() and self.model_manifest_path.exists():
                manifest = json.loads(self.model_manifest_path.read_text(encoding="utf-8"))
                if not self._manifest_is_compatible(manifest):
                    self.model_status_reason = "incompatible model manifest"
                    self._publish_inference_status(self.model_status_reason, state="degraded")
                    return None

                model = joblib.load(self.model_path)
                self.model_status_reason = "model loaded"
                self._publish_inference_status(self.model_status_reason, state="up")
                return model

            if os.environ.get("QT_ALLOW_BOOTSTRAP_MODEL") == "1":
                logging.warning("QT_ALLOW_BOOTSTRAP_MODEL=1 enabled; bootstrapping fallback model")
                X_train = np.array(
                    [
                        [1000, 10000, 5500, 4500, 200, 250, 3000, -50, 3, 1, 120, 360, 18.0, 12.0, 1, 1, 0, 0, 0, 0, 0, 0],
                        [-1000, 10000, 4500, 5500, -200, 250, 3000, 50, 3, -1, 120, 360, 18.0, 12.0, 1, 1, 0, 0, 0, 0, 0, 0],
                        [0, 8000, 4000, 4000, 0, 100, 3000, 0, 2, 0, 80, 220, 14.0, 9.0, 1, 1, 0, 0, 0, 0, 0, 0],
                    ]
                )
                y_train = np.array([1, -1, 0])
                model = RandomForestClassifier(n_estimators=50, random_state=42)
                model.fit(X_train, y_train)
                self.model_path.parent.mkdir(parents=True, exist_ok=True)
                joblib.dump(model, self.model_path)
                self.model_status_reason = "bootstrap model loaded"
                self._publish_inference_status(self.model_status_reason, state="up")
                return model

            self.model_status_reason = "training artifact missing"
            self._publish_inference_status(self.model_status_reason, state="degraded")
            return None
        except ModuleNotFoundError as error:
            self.inference_errors_total += 1
            self.model_status_reason = "model dependencies unavailable"
            logging.error("Model dependencies unavailable: %s", error)
            self._publish_inference_status(self.model_status_reason, state="degraded")
            return None
        except Exception:
            self.inference_errors_total += 1
            self.model_status_reason = "model load failed"
            self._publish_inference_status(self.model_status_reason, state="degraded")
            raise

    def _manifest_is_compatible(self, manifest: dict[str, Any]) -> bool:
        if manifest.get("dataset_scope") != EXPECTED_DATASET_SCOPE:
            logging.error("Promoted model dataset scope mismatch: %s", manifest.get("dataset_scope"))
            return False

        feature_schema_path = self._resolve_runtime_path(manifest.get("feature_schema_path", "feature_schema.json"))
        if not feature_schema_path.exists():
            logging.error("Promoted feature schema missing: %s", feature_schema_path)
            return False

        feature_schema = json.loads(feature_schema_path.read_text(encoding="utf-8"))
        feature_order = feature_schema.get("feature_order")
        if feature_order != EXPECTED_FEATURE_SCHEMA:
            logging.error("Promoted feature schema mismatch")
            return False

        self.feature_keys = list(feature_order)
        return True

    def _resolve_runtime_path(self, manifest_path_value: str) -> Path:
        if not manifest_path_value:
            return runtime_root() / "feature_schema.json"
        candidate = Path(manifest_path_value)
        if candidate.is_absolute():
            return candidate
        parts = list(candidate.parts)
        if parts and parts[0] == "runtime":
            return runtime_root().joinpath(*parts[1:])
        return runtime_root() / candidate

    def export_individual_csv(self, symbol, data):
        import json as _json

        msg_type = data.get("event", data.get("type", "unknown"))
        if msg_type == "unknown":
            if "wyckoffVolume" in data:
                msg_type = "wyckoff_state"
            elif "vpPOC" in data:
                msg_type = "volume_profile"
            elif "deltaRank" in data:
                msg_type = "order_flow_aggregated"

        keys = event_history_keys(msg_type)
        if not keys:
            logging.warning("Skipping runtime JSONL export for unsupported event type: %s", msg_type)
            return

        filename = event_history_file(msg_type)
        record = data.copy()
        if "instrument" not in record:
            record["instrument"] = symbol
        ordered_record = {key: record.get(key) for key in keys}
        filename.parent.mkdir(parents=True, exist_ok=True)

        try:
            with filename.open("a", encoding="utf-8") as handle:
                handle.write(_json.dumps(ordered_record, ensure_ascii=False, separators=(",", ":")))
                handle.write("\n")
        except Exception as error:
            logging.error("JSONL Export Error for %s: %s", filename, error)

    def process_data(self, data):
        import time

        if not data:
            return

        event = data.get("event", data.get("type", "indicator"))
        payload = data.get("payload", data)
        if not isinstance(payload, dict):
            payload = data
        source_meta = data.get("source_meta") or {}
        symbol = data.get("instrument", data.get("symbol", source_meta.get("symbol", "EURUSD")))

        if self._is_bookmap_event(data, event):
            self._record_bookmap_event(event, data, payload, source_meta)
            if event == "dom":
                self._update_order_book(payload, data, source_meta, symbol)
            return

        flattened = self._flatten_runtime_record(data, payload)
        flattened["instrument"] = symbol
        if "symbol" not in flattened and "symbol" in source_meta:
            flattened["symbol"] = source_meta["symbol"]

        if symbol not in self.raw_data_buffer:
            self.raw_data_buffer[symbol] = []
            self.latest_state[symbol] = {}

        self.raw_data_buffer[symbol].append(flattened)
        self._publish_buffering_status("awaiting buffered data", state="up")

        now = time.time()
        if len(self.raw_data_buffer[symbol]) > 100 or (now - self.last_buffer_process_time > 2.0):
            self._process_buffer(symbol)
            self.last_buffer_process_time = now

    def _is_bookmap_event(self, data: dict[str, Any], event: str) -> bool:
        if data.get("source") == "bookmap":
            return True
        source_meta = data.get("source_meta") or {}
        return "alias" in source_meta or event in {"alert", "dom", "dot", "stop", "wall", "wall_added", "wall_removed"}

    def _update_order_book(self, payload: dict[str, Any], data: dict[str, Any], source_meta: dict[str, Any], symbol: str) -> None:
        alias = payload.get("alias", data.get("alias", source_meta.get("alias", symbol)))
        is_bid = payload.get("isBid", data.get("isBid"))
        price = payload.get("price", data.get("price"))
        size = payload.get("size", data.get("size"))
        if alias not in self.order_book:
            self.order_book[alias] = {True: {}, False: {}}
        side_book = self.order_book[alias][is_bid]
        if size == 0:
            side_book.pop(price, None)
        else:
            side_book[price] = size

    def _record_bookmap_event(
        self,
        event: str,
        data: dict[str, Any],
        payload: dict[str, Any],
        source_meta: dict[str, Any],
    ) -> None:
        alias = payload.get("alias", data.get("instrument", source_meta.get("alias", ""))) or source_meta.get("alias", "")
        mapped_symbol, canonical_root = self._map_alias_to_symbol(alias)
        if not mapped_symbol or not canonical_root:
            return

        timestamp_text = data.get("timestamp")
        if not timestamp_text:
            return
        timestamp = _parse_timestamp(timestamp_text)
        normalized_event = _normalize_bookmap_event(event)
        side = str(payload.get("side", "")).upper()
        value = _safe_float(payload.get("value"))
        signed_value = BOOKMAP_SIGN_BY_SIDE.get(side, 0.0) * value

        window = self.bookmap_windows_by_canonical_market[canonical_root]
        window.append(
            {
                "timestamp": timestamp,
                "event": normalized_event,
                "signed_value": signed_value,
            }
        )
        self._prune_bookmap_window(window, timestamp)

    def _prune_bookmap_window(self, window: deque[dict[str, Any]], current_timestamp: datetime) -> None:
        cutoff = current_timestamp - BOOKMAP_WINDOW
        while window and window[0]["timestamp"] <= cutoff:
            window.popleft()

    def _current_bookmap_features(self, symbol: str, timestamp_text: str) -> dict[str, Any]:
        canonical_root = INSTRUMENT_TO_CANONICAL_ROOT.get(symbol)
        if not canonical_root:
            return self._empty_bookmap_features()

        timestamp = _parse_timestamp(timestamp_text)
        window = self.bookmap_windows_by_canonical_market[canonical_root]
        self._prune_bookmap_window(window, timestamp)
        active_events = [
            event
            for event in window
            if event["timestamp"] <= timestamp and event["timestamp"] > timestamp - BOOKMAP_WINDOW
        ]
        if not active_events:
            return self._empty_bookmap_features()

        return {
            "has_bookmap_window": 1,
            "bookmap_event_count_300s": len(active_events),
            "bookmap_dot_count_300s": sum(1 for event in active_events if event["event"] == "dot"),
            "bookmap_stop_count_300s": sum(1 for event in active_events if event["event"] == "stop"),
            "bookmap_wall_count_300s": sum(1 for event in active_events if event["event"] == "wall"),
            "bookmap_signed_value_300s": sum(event["signed_value"] for event in active_events),
        }

    def _empty_bookmap_features(self) -> dict[str, Any]:
        return {
            "has_bookmap_window": 0,
            "bookmap_event_count_300s": 0,
            "bookmap_dot_count_300s": 0,
            "bookmap_stop_count_300s": 0,
            "bookmap_wall_count_300s": 0,
            "bookmap_signed_value_300s": 0.0,
        }

    def _map_alias_to_symbol(self, alias: str) -> tuple[str | None, str | None]:
        normalized_alias = alias.upper()
        for canonical_root, instrument in CANONICAL_ROOT_TO_INSTRUMENT.items():
            if normalized_alias.startswith(canonical_root):
                return instrument, canonical_root
        return None, None

    def _flatten_runtime_record(self, data: dict[str, Any], payload: dict[str, Any]) -> dict[str, Any]:
        flattened = dict(data)
        flattened.update(payload)
        for nested_key in ("bar", "summary", "wyckoff", "wave"):
            nested = flattened.get(nested_key)
            if isinstance(nested, dict):
                flattened.update(nested)
        if "direction" in flattened and "waveDirection" not in flattened:
            flattened["waveDirection"] = flattened["direction"]
        if "direction_sign" in flattened and "wy_last_direction_sign" not in flattened:
            flattened["wy_last_direction_sign"] = flattened["direction_sign"]
        if "volume" in flattened and "wave" in flattened and "wy_last_wave_volume" not in flattened:
            flattened["wy_last_wave_volume"] = flattened["volume"]
        return flattened

    def _process_buffer(self, symbol):
        buffer = self.raw_data_buffer[symbol]
        if not buffer:
            return
        self._publish_buffering_status("processing buffered data", state="up")
        if self.model is None:
            self._publish_inference_status(self.model_status_reason, state="degraded")
            self.raw_data_buffer[symbol] = []
            self._publish_buffering_status("awaiting normalized records", state="degraded")
            return

        import numpy as np

        buffer.sort(key=lambda item: item.get("timestamp", ""))
        state = self.latest_state[symbol]

        for record in buffer:
            state.update(record)
            features = self._extract_feature_vector(symbol, state, record)
            if features is None or np.all(features == 0):
                continue

            current_price = _safe_float(record.get("close", state.get("close", 0.0)))
            spread = _safe_float(state.get("spread", 0.0))
            self.simulator.check_open_trades(symbol, current_price)

            try:
                prediction = self.model.predict(features)[0]
                probability = self._prediction_probability(features)
                self._publish_inference_status(
                    "model loaded",
                    state="up",
                    last_inference_at=record.get("timestamp"),
                )
                if probability > 0.65:
                    if prediction == 1:
                        self.simulator.open_trade(1, symbol, current_price, spread)
                    elif prediction == -1:
                        self.simulator.open_trade(-1, symbol, current_price, spread)
            except Exception as error:
                logging.error("AI Prediction Error: %s", error)
                self.inference_errors_total += 1
                self._publish_inference_status("prediction failed", state="degraded")

        self.raw_data_buffer[symbol] = []
        self._publish_buffering_status("awaiting normalized records", state="degraded")

    def _prediction_probability(self, features) -> float:
        if not hasattr(self.model, "predict_proba"):
            return 1.0
        probabilities = self.model.predict_proba(features)
        if probabilities.size == 0:
            return 1.0
        return float(probabilities.max())

    def _extract_feature_vector(self, symbol: str, state: dict[str, Any], record: dict[str, Any]):
        import numpy as np

        timestamp_text = record.get("timestamp") or state.get("timestamp")
        if not timestamp_text:
            return None

        current_price = _safe_float(state.get("close", 0.0))
        tick_size = _safe_float(state.get("tick_size", 0.01)) or 0.01
        level_count = int(state.get("level_count", len(state.get("levels", []) if isinstance(state.get("levels"), list) else [])))
        total_volume = _coalesce_numeric(state, "total_volume", fallback=_sum_values(state.get("volumesRank", 0)))
        buy_volume = _coalesce_numeric(state, "buy_volume", fallback=0.0)
        sell_volume = _coalesce_numeric(state, "sell_volume", fallback=0.0)
        delta_sum = _coalesce_numeric(state, "delta_sum", fallback=_sum_values(state.get("deltaRank", 0)))
        abs_delta_sum = _coalesce_numeric(state, "abs_delta_sum", fallback=_sum_abs_values(state.get("deltaRank", 0)))
        poc_price = _coalesce_numeric(state, "poc_price", fallback=state.get("vpPOC", current_price))
        poc_distance_to_close_ticks = _coalesce_numeric(
            state,
            "poc_distance_to_close_ticks",
            fallback=(poc_price - current_price) / tick_size if tick_size else 0.0,
        )
        wy_last_direction_sign = int(
            _coalesce_numeric(
                state,
                "wy_last_direction_sign",
                fallback=1 if state.get("waveDirection") == "Up" else -1 if state.get("waveDirection") == "Down" else 0,
            )
        )
        wy_last_wave_volume = _coalesce_numeric(state, "wy_last_wave_volume", fallback=state.get("waveVolume", 0.0))
        wy_sum_wave_volume = _coalesce_numeric(state, "wy_sum_wave_volume", fallback=wy_last_wave_volume)
        wy_mean_volume_per_time = _coalesce_numeric(state, "wy_mean_volume_per_time", fallback=state.get("volume_per_time", 0.0))
        wy_max_wave_efficiency = _coalesce_numeric(state, "wy_max_wave_efficiency", fallback=state.get("wave_efficiency", 0.0))
        wy_count = int(_coalesce_numeric(state, "wy_count", fallback=1 if wy_last_wave_volume else 0))
        has_volume_profile = int(bool(state.get("has_volume_profile", state.get("vpPOC") is not None or state.get("poc_price") is not None)))
        has_wyckoff_window = int(bool(state.get("has_wyckoff_window", wy_count > 0)))
        bookmap_features = self._current_bookmap_features(symbol, timestamp_text)

        values = [
            level_count,
            total_volume,
            buy_volume,
            sell_volume,
            delta_sum,
            abs_delta_sum,
            poc_price,
            poc_distance_to_close_ticks,
            wy_count,
            wy_last_direction_sign,
            wy_last_wave_volume,
            wy_sum_wave_volume,
            wy_mean_volume_per_time,
            wy_max_wave_efficiency,
            has_volume_profile,
            has_wyckoff_window,
            bookmap_features["has_bookmap_window"],
            bookmap_features["bookmap_event_count_300s"],
            bookmap_features["bookmap_dot_count_300s"],
            bookmap_features["bookmap_stop_count_300s"],
            bookmap_features["bookmap_wall_count_300s"],
            bookmap_features["bookmap_signed_value_300s"],
        ]
        return np.array([values], dtype=float)

    def shutdown(self):
        self.simulator.print_summary()


def _normalize_bookmap_event(event: str) -> str:
    event = str(event).lower()
    if event.startswith("wall"):
        return "wall"
    if "stop" in event:
        return "stop"
    if event == "dot":
        return "dot"
    return event


def _safe_float(value: Any) -> float:
    if value in (None, ""):
        return 0.0
    try:
        return float(value)
    except (TypeError, ValueError):
        return 0.0


def _sum_values(value: Any) -> float:
    if isinstance(value, dict):
        return float(sum(_safe_float(item) for item in value.values()))
    if isinstance(value, list):
        return float(sum(_safe_float(item) for item in value))
    return _safe_float(value)


def _sum_abs_values(value: Any) -> float:
    if isinstance(value, dict):
        return float(sum(abs(_safe_float(item)) for item in value.values()))
    if isinstance(value, list):
        return float(sum(abs(_safe_float(item)) for item in value))
    return abs(_safe_float(value))


def _coalesce_numeric(state: dict[str, Any], key: str, *, fallback: Any) -> float:
    if key in state and state[key] not in (None, ""):
        return _safe_float(state[key])
    return _safe_float(fallback)


def _parse_timestamp(value: str) -> datetime:
    return datetime.fromisoformat(value.replace("Z", "+00:00")).astimezone(UTC)
