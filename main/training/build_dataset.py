from __future__ import annotations

import json
from datetime import UTC, datetime
from pathlib import Path
from typing import Any
from uuid import uuid4

import pandas as pd

from training.contracts import (
    ANCHOR_EVENT_BY_SCOPE,
    ANCHOR_TIMEFRAME_BY_SCOPE,
    DERIVED_DATASET_FIELDS,
    SCOPE_TO_INSTRUMENT,
)

DEFAULT_INSTRUMENT_MAP_PATH = Path("configs/training/instrument_map.json")
DEFAULT_SCOPE_OUTPUT_ROOT = Path("artifacts/datasets/ctrader_xauusd_h1_baseline")
BOOKMAP_WINDOW = pd.Timedelta(seconds=300)
WYCKOFF_WINDOW = pd.Timedelta("1h")
VOLUME_PROFILE_TOLERANCE = pd.Timedelta("2h")
_BOOKMAP_SIGN_BY_SIDE = {"BUY": 1.0, "ASK": 1.0, "SELL": -1.0, "BID": -1.0}


def build_dataset(
    scope: str,
    logs_dir: str | Path,
    output_root: str | Path,
    *,
    instrument_map_path: str | Path = DEFAULT_INSTRUMENT_MAP_PATH,
    run_id: str | None = None,
) -> dict[str, Any]:
    if scope not in SCOPE_TO_INSTRUMENT:
        raise ValueError(f"Unsupported dataset scope: {scope}")

    logs_dir = Path(logs_dir)
    output_root = Path(output_root)
    instrument_map_path = Path(instrument_map_path)
    run_id = run_id or _make_run_id()

    anchor_instrument = SCOPE_TO_INSTRUMENT[scope]
    canonical_map = _load_instrument_map(instrument_map_path)
    canonical_market = canonical_map[anchor_instrument][0]
    output_dir = output_root / scope / run_id
    output_dir.mkdir(parents=True, exist_ok=True)

    orderflow_path = logs_dir / "history_orderflowaggregated_XAUUSD.jsonl"
    volume_profile_path = logs_dir / "history_volumeprofile_XAUUSD.jsonl"
    wyckoff_path = logs_dir / "history_wyckoff_XAUUSD.jsonl"

    orderflow_frame = _load_orderflow_frame(orderflow_path, anchor_instrument, ANCHOR_TIMEFRAME_BY_SCOPE[scope])
    if orderflow_frame.empty:
        raise ValueError(f"No anchor rows found in {orderflow_path}")

    volume_profile_frame = _load_volume_profile_frame(
        volume_profile_path,
        anchor_instrument,
        ANCHOR_TIMEFRAME_BY_SCOPE[scope],
    )
    joined_frame = _join_volume_profile(orderflow_frame, volume_profile_frame)
    wyckoff_frame = _load_wyckoff_frame(wyckoff_path, anchor_instrument)
    bookmap_scope_frame, bookmap_excluded_frame, excluded_sources = _load_bookmap_frames(
        logs_dir,
        canonical_map,
        anchor_instrument,
    )

    derived_rows: list[dict[str, Any]] = []
    for row in joined_frame.itertuples(index=False):
        anchor_timestamp = pd.Timestamp(row.anchor_timestamp)
        wyckoff_features = _aggregate_wyckoff_window(wyckoff_frame, anchor_instrument, anchor_timestamp)
        bookmap_features = _aggregate_bookmap_window(
            bookmap_scope_frame,
            bookmap_excluded_frame,
            anchor_timestamp,
        )
        derived_rows.append(
            {
                "row_id": f"{scope}:{row.orderflow_event_id}",
                "dataset_scope": scope,
                "instrument": anchor_instrument,
                "canonical_market": canonical_market,
                "anchor_timestamp": _to_iso(anchor_timestamp),
                "anchor_timeframe": row.anchor_timeframe,
                "orderflow_event_id": row.orderflow_event_id,
                "volumeprofile_event_id": _optional_value(row.volumeprofile_event_id),
                "wyckoff_window_start": _to_iso(anchor_timestamp - WYCKOFF_WINDOW),
                "wyckoff_window_end": _to_iso(anchor_timestamp),
                "open": row.open,
                "high": row.high,
                "low": row.low,
                "close": row.close,
                "spread": row.spread,
                "tick_size": row.tick_size,
                "level_count": row.level_count,
                "total_volume": row.total_volume,
                "buy_volume": row.buy_volume,
                "sell_volume": row.sell_volume,
                "delta_sum": row.delta_sum,
                "abs_delta_sum": row.abs_delta_sum,
                "poc_price": row.poc_price,
                "poc_distance_to_close_ticks": row.poc_distance_to_close_ticks,
                "wy_count": wyckoff_features["wy_count"],
                "wy_last_direction_sign": wyckoff_features["wy_last_direction_sign"],
                "wy_last_wave_volume": wyckoff_features["wy_last_wave_volume"],
                "wy_sum_wave_volume": wyckoff_features["wy_sum_wave_volume"],
                "wy_mean_volume_per_time": wyckoff_features["wy_mean_volume_per_time"],
                "wy_max_wave_efficiency": wyckoff_features["wy_max_wave_efficiency"],
                "has_volume_profile": bool(pd.notna(row.volumeprofile_event_id)),
                "has_wyckoff_window": wyckoff_features["wy_count"] > 0,
                "has_bookmap_window": bookmap_features["bookmap_event_count_300s"] > 0,
                "bookmap_event_count_300s": bookmap_features["bookmap_event_count_300s"],
                "bookmap_dot_count_300s": bookmap_features["bookmap_dot_count_300s"],
                "bookmap_stop_count_300s": bookmap_features["bookmap_stop_count_300s"],
                "bookmap_wall_count_300s": bookmap_features["bookmap_wall_count_300s"],
                "bookmap_signed_value_300s": bookmap_features["bookmap_signed_value_300s"],
                "quarantined_source_count": bookmap_features["quarantined_source_count"],
            }
        )

    derived_frame = pd.DataFrame.from_records(derived_rows, columns=DERIVED_DATASET_FIELDS)
    dataset_path = output_dir / "dataset.parquet"
    manifest_path = output_dir / "dataset_manifest.json"
    source_manifest_path = output_dir / "source_manifest.json"

    derived_frame.to_parquet(dataset_path, index=False)
    _write_json(
        manifest_path,
        {
            "dataset_scope": scope,
            "run_id": run_id,
            "instrument": anchor_instrument,
            "canonical_market": canonical_market,
            "anchor_event": ANCHOR_EVENT_BY_SCOPE[scope],
            "anchor_timeframe": ANCHOR_TIMEFRAME_BY_SCOPE[scope],
            "row_count": int(len(derived_frame)),
            "dataset_path": str(dataset_path),
            "canonical_mapping_examples": ["XAUUSD -> GC", "US500 -> ES"],
            "excluded_sources": excluded_sources,
        },
    )
    _write_json(
        source_manifest_path,
        {
            "paths": {
                "orderflow": str(orderflow_path),
                "volume_profile": str(volume_profile_path),
                "wyckoff": str(wyckoff_path),
            },
            "source_counts": {
                "orderflow_rows": int(len(orderflow_frame)),
                "volume_profile_rows": int(len(volume_profile_frame)),
                "wyckoff_rows": int(len(wyckoff_frame)),
                "bookmap_scope_rows": int(len(bookmap_scope_frame)),
                "bookmap_excluded_rows": int(len(bookmap_excluded_frame)),
            },
        },
    )
    return {
        "dataset_path": dataset_path,
        "manifest_path": manifest_path,
        "source_manifest_path": source_manifest_path,
        "output_dir": output_dir,
        "row_count": int(len(derived_frame)),
    }


def _load_orderflow_frame(path: Path, instrument: str, timeframe: str) -> pd.DataFrame:
    records = []
    for record in _read_jsonl(path):
        if record.get("event") != "order_flow_aggregated":
            continue
        if record.get("instrument") != instrument:
            continue
        if str(record.get("timeframe", "")).lower() != timeframe.lower():
            continue
        if not record.get("bar_closed", False):
            continue

        bar = record.get("bar") or {}
        summary = record.get("summary") or {}
        records.append(
            {
                "anchor_timestamp": _parse_timestamp(record["timestamp"]),
                "anchor_timeframe": record.get("timeframe"),
                "orderflow_event_id": record.get("event_id"),
                "instrument": record.get("instrument"),
                "open": float(bar.get("open", 0.0)),
                "high": float(bar.get("high", 0.0)),
                "low": float(bar.get("low", 0.0)),
                "close": float(bar.get("close", 0.0)),
                "spread": float(bar.get("spread", 0.0)),
                "tick_size": float(bar.get("tick_size", 0.0)),
                "level_count": int(summary.get("level_count", 0)),
                "total_volume": float(summary.get("total_volume", 0.0)),
                "buy_volume": float(summary.get("buy_volume", 0.0)),
                "sell_volume": float(summary.get("sell_volume", 0.0)),
                "delta_sum": float(summary.get("delta_sum", 0.0)),
                "abs_delta_sum": float(summary.get("abs_delta_sum", 0.0)),
                "poc_price": float(summary.get("poc_price", 0.0)),
                "poc_distance_to_close_ticks": float(summary.get("poc_distance_to_close_ticks", 0.0)),
            }
        )
    if not records:
        return pd.DataFrame(
            columns=[
                "anchor_timestamp",
                "anchor_timeframe",
                "orderflow_event_id",
                "instrument",
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
            ]
        )
    return pd.DataFrame.from_records(records).sort_values("anchor_timestamp").reset_index(drop=True)


def _load_volume_profile_frame(path: Path, instrument: str, timeframe: str) -> pd.DataFrame:
    records = []
    for record in _read_jsonl(path):
        if record.get("event") != "volume_profile":
            continue
        if record.get("instrument") != instrument:
            continue
        if str(record.get("timeframe", "")).lower() != timeframe.lower():
            continue
        if not record.get("bar_closed", False):
            continue
        records.append(
            {
                "instrument": record.get("instrument"),
                "volumeprofile_timestamp": _parse_timestamp(record["timestamp"]),
                "volumeprofile_event_id": record.get("event_id"),
            }
        )
    if not records:
        return pd.DataFrame(columns=["instrument", "volumeprofile_timestamp", "volumeprofile_event_id"])
    return pd.DataFrame.from_records(records).sort_values("volumeprofile_timestamp").reset_index(drop=True)


def _join_volume_profile(orderflow_frame: pd.DataFrame, volume_profile_frame: pd.DataFrame) -> pd.DataFrame:
    if volume_profile_frame.empty:
        joined = orderflow_frame.copy()
        joined["volumeprofile_event_id"] = pd.NA
        return joined

    return pd.merge_asof(
        orderflow_frame.sort_values("anchor_timestamp"),
        volume_profile_frame.sort_values("volumeprofile_timestamp"),
        left_on="anchor_timestamp",
        right_on="volumeprofile_timestamp",
        by="instrument",
        direction="backward",
        tolerance=pd.Timedelta("2h"),
    )


def _load_wyckoff_frame(path: Path, instrument: str) -> pd.DataFrame:
    records = []
    for record in _read_jsonl(path):
        if record.get("event") != "wyckoff_state":
            continue
        if record.get("instrument") != instrument:
            continue
        wave = record.get("wave") or {}
        summary = record.get("summary") or {}
        records.append(
            {
                "timestamp": _parse_timestamp(record["timestamp"]),
                "instrument": record.get("instrument"),
                "direction_sign": int(wave.get("direction_sign", 0)),
                "wave_volume": float(wave.get("volume", 0.0)),
                "volume_per_time": float(summary.get("volume_per_time", 0.0)),
                "wave_efficiency": float(summary.get("wave_efficiency", 0.0)),
            }
        )
    if not records:
        return pd.DataFrame(
            columns=[
                "timestamp",
                "instrument",
                "direction_sign",
                "wave_volume",
                "volume_per_time",
                "wave_efficiency",
            ]
        )
    return pd.DataFrame.from_records(records).sort_values("timestamp").reset_index(drop=True)


def _aggregate_wyckoff_window(
    wyckoff_frame: pd.DataFrame,
    instrument: str,
    anchor_timestamp: pd.Timestamp,
) -> dict[str, Any]:
    if wyckoff_frame.empty:
        return {
            "wy_count": 0,
            "wy_last_direction_sign": 0,
            "wy_last_wave_volume": 0.0,
            "wy_sum_wave_volume": 0.0,
            "wy_mean_volume_per_time": 0.0,
            "wy_max_wave_efficiency": 0.0,
        }

    window_start = anchor_timestamp - WYCKOFF_WINDOW
    window = wyckoff_frame[
        (wyckoff_frame["instrument"] == instrument)
        & (wyckoff_frame["timestamp"] > window_start)
        & (wyckoff_frame["timestamp"] <= anchor_timestamp)
    ]
    if window.empty:
        return {
            "wy_count": 0,
            "wy_last_direction_sign": 0,
            "wy_last_wave_volume": 0.0,
            "wy_sum_wave_volume": 0.0,
            "wy_mean_volume_per_time": 0.0,
            "wy_max_wave_efficiency": 0.0,
        }

    return {
        "wy_count": int(len(window)),
        "wy_last_direction_sign": int(window.iloc[-1]["direction_sign"]),
        "wy_last_wave_volume": float(window.iloc[-1]["wave_volume"]),
        "wy_sum_wave_volume": float(window["wave_volume"].sum()),
        "wy_mean_volume_per_time": float(window["volume_per_time"].mean()),
        "wy_max_wave_efficiency": float(window["wave_efficiency"].max()),
    }


def _load_bookmap_frames(
    logs_dir: Path,
    canonical_map: dict[str, tuple[str, ...]],
    anchor_instrument: str,
) -> tuple[pd.DataFrame, pd.DataFrame, list[dict[str, Any]]]:
    scope_rows: list[dict[str, Any]] = []
    excluded_rows: list[dict[str, Any]] = []

    for path in sorted(logs_dir.glob("history_alertlistener_*.jsonl")):
        for record in _read_jsonl(path):
            alias = (
                (record.get("source_meta") or {}).get("alias")
                or record.get("instrument")
                or ""
            )
            mapped_instrument, canonical_root = _map_bookmap_alias(alias, canonical_map)
            row = {
                "event_id": record.get("event_id"),
                "event": str(record.get("event", "")).lower(),
                "timestamp": _parse_timestamp(record["timestamp"]),
                "alias": alias,
                "mapped_instrument": mapped_instrument,
                "canonical_root": canonical_root,
                "value": _parse_float((record.get("payload") or {}).get("value")),
                "side": str((record.get("payload") or {}).get("side", "")).upper(),
            }
            if mapped_instrument == anchor_instrument:
                scope_rows.append(row)
                continue

            excluded_row = {
                "event_id": row["event_id"],
                "timestamp": _to_iso(row["timestamp"]),
                "alias": alias,
                "mapped_instrument": mapped_instrument,
                "canonical_root": canonical_root,
                "excluded_reason": "canonical_market_mismatch",
            }
            excluded_rows.append(excluded_row)

    scope_frame = pd.DataFrame.from_records(scope_rows)
    if not scope_frame.empty:
        scope_frame = scope_frame.sort_values("timestamp").reset_index(drop=True)

    excluded_frame = pd.DataFrame.from_records(excluded_rows)
    if not excluded_frame.empty:
        excluded_frame["timestamp"] = excluded_frame["timestamp"].map(_parse_timestamp)
        excluded_frame = excluded_frame.sort_values("timestamp").reset_index(drop=True)

    return scope_frame, excluded_frame, excluded_rows


def _aggregate_bookmap_window(
    scope_frame: pd.DataFrame,
    excluded_frame: pd.DataFrame,
    anchor_timestamp: pd.Timestamp,
) -> dict[str, Any]:
    window_start = anchor_timestamp - BOOKMAP_WINDOW
    scope_window = _slice_time_window(scope_frame, window_start, anchor_timestamp)
    excluded_window = _slice_time_window(excluded_frame, window_start, anchor_timestamp)

    signed_value = 0.0
    if not scope_window.empty:
        signed_value = float(
            sum(
                _BOOKMAP_SIGN_BY_SIDE.get(side, 0.0) * float(value)
                for side, value in zip(scope_window["side"], scope_window["value"])
            )
        )

    return {
        "bookmap_event_count_300s": int(len(scope_window)),
        "bookmap_dot_count_300s": int((scope_window.get("event", pd.Series(dtype=str)) == "dot").sum()),
        "bookmap_stop_count_300s": int((scope_window.get("event", pd.Series(dtype=str)) == "stop").sum()),
        "bookmap_wall_count_300s": int((scope_window.get("event", pd.Series(dtype=str)) == "wall").sum()),
        "bookmap_signed_value_300s": signed_value,
        "quarantined_source_count": int(len(excluded_window)),
    }


def _slice_time_window(frame: pd.DataFrame, window_start: pd.Timestamp, window_end: pd.Timestamp) -> pd.DataFrame:
    if frame.empty:
        return frame
    return frame[(frame["timestamp"] > window_start) & (frame["timestamp"] <= window_end)]


def _load_instrument_map(path: Path) -> dict[str, tuple[str, ...]]:
    raw_map = json.loads(path.read_text(encoding="utf-8"))
    return {instrument.upper(): tuple(root.upper() for root in roots) for instrument, roots in raw_map.items()}


def _map_bookmap_alias(
    alias: str,
    canonical_map: dict[str, tuple[str, ...]],
) -> tuple[str | None, str | None]:
    normalized_alias = alias.upper()
    for instrument, roots in canonical_map.items():
        for root in roots:
            if normalized_alias.startswith(root):
                return instrument, root
    return None, None


def _read_jsonl(path: Path) -> list[dict[str, Any]]:
    if not path.exists():
        return []
    records: list[dict[str, Any]] = []
    for line in path.read_text(encoding="utf-8").splitlines():
        if not line.strip():
            continue
        records.append(json.loads(line))
    return records


def _write_json(path: Path, payload: dict[str, Any]) -> None:
    path.write_text(json.dumps(payload, indent=2, ensure_ascii=False), encoding="utf-8")


def _parse_timestamp(value: str) -> pd.Timestamp:
    timestamp = pd.Timestamp(value)
    if timestamp.tzinfo is None:
        return timestamp.tz_localize(UTC)
    return timestamp.tz_convert(UTC)


def _parse_float(value: Any) -> float:
    if value in (None, ""):
        return 0.0
    return float(value)


def _optional_value(value: Any) -> Any:
    return None if pd.isna(value) else value


def _to_iso(value: pd.Timestamp) -> str:
    return value.tz_convert(UTC).isoformat().replace("+00:00", "Z")


def _make_run_id() -> str:
    return datetime.now(tz=UTC).strftime("%Y%m%dT%H%M%SZ") + "-" + uuid4().hex[:8]
