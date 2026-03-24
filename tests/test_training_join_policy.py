from __future__ import annotations

import json
from pathlib import Path

import pandas as pd

from training.build_dataset import build_dataset
from training.contracts import DATASET_SCOPE_CTRADER_XAUUSD_H1_BASELINE


def test_builder_uses_orderflow_as_anchor_series(tmp_path: Path):
    logs_dir = _make_logs_dir(tmp_path)
    _write_jsonl(
        logs_dir / "history_orderflowaggregated_XAUUSD.jsonl",
        [
            _orderflow_record("of-1", "2026-03-20T10:00:00Z"),
            _orderflow_record("of-2", "2026-03-20T11:00:00Z"),
        ],
    )
    _write_jsonl(logs_dir / "history_volumeprofile_XAUUSD.jsonl", [])
    _write_jsonl(logs_dir / "history_wyckoff_XAUUSD.jsonl", [])

    result = build_dataset(
        scope=DATASET_SCOPE_CTRADER_XAUUSD_H1_BASELINE,
        logs_dir=logs_dir,
        output_root=tmp_path / "artifacts" / "datasets",
        instrument_map_path=_write_instrument_map(tmp_path),
        run_id="anchor-series",
    )

    dataset = pd.read_parquet(result["dataset_path"])
    assert list(dataset["orderflow_event_id"]) == ["of-1", "of-2"]
    assert list(dataset["dataset_scope"].unique()) == [DATASET_SCOPE_CTRADER_XAUUSD_H1_BASELINE]


def test_builder_uses_backward_only_volume_profile_join(tmp_path: Path):
    logs_dir = _make_logs_dir(tmp_path)
    _write_jsonl(
        logs_dir / "history_orderflowaggregated_XAUUSD.jsonl",
        [_orderflow_record("of-1", "2026-03-20T10:00:00Z")],
    )
    _write_jsonl(
        logs_dir / "history_volumeprofile_XAUUSD.jsonl",
        [
            _volume_profile_record("vp-backward", "2026-03-20T09:30:00Z"),
            _volume_profile_record("vp-forward", "2026-03-20T10:30:00Z"),
        ],
    )
    _write_jsonl(logs_dir / "history_wyckoff_XAUUSD.jsonl", [])

    result = build_dataset(
        scope=DATASET_SCOPE_CTRADER_XAUUSD_H1_BASELINE,
        logs_dir=logs_dir,
        output_root=tmp_path / "artifacts" / "datasets",
        instrument_map_path=_write_instrument_map(tmp_path),
        run_id="backward-join",
    )

    dataset = pd.read_parquet(result["dataset_path"])
    assert dataset.loc[0, "volumeprofile_event_id"] == "vp-backward"


def test_builder_maps_gc_aliases_into_xauusd_scope(tmp_path: Path):
    logs_dir = _make_logs_dir(tmp_path)
    _write_jsonl(
        logs_dir / "history_orderflowaggregated_XAUUSD.jsonl",
        [_orderflow_record("of-1", "2026-03-20T10:00:00Z")],
    )
    _write_jsonl(logs_dir / "history_volumeprofile_XAUUSD.jsonl", [])
    _write_jsonl(logs_dir / "history_wyckoff_XAUUSD.jsonl", [])
    _write_jsonl(
        logs_dir / "history_alertlistener_GCJ6.COMEX_RITHMIC.jsonl",
        [_bookmap_record("bm-gc-1", "2026-03-20T09:59:00Z", "GCJ6.COMEX@RITHMIC", "dot", "BID", "15")],
    )

    result = build_dataset(
        scope=DATASET_SCOPE_CTRADER_XAUUSD_H1_BASELINE,
        logs_dir=logs_dir,
        output_root=tmp_path / "artifacts" / "datasets",
        instrument_map_path=_write_instrument_map(tmp_path),
        run_id="gc-alias",
    )

    dataset = pd.read_parquet(result["dataset_path"])
    manifest = json.loads(result["manifest_path"].read_text(encoding="utf-8"))

    assert dataset.loc[0, "has_bookmap_window"]
    assert dataset.loc[0, "bookmap_event_count_300s"] == 1
    assert dataset.loc[0, "bookmap_dot_count_300s"] == 1
    assert dataset.loc[0, "bookmap_signed_value_300s"] == -15.0
    assert manifest["excluded_sources"] == []


def test_builder_rejects_es_aliases_for_xauusd_scope(tmp_path: Path):
    logs_dir = _make_logs_dir(tmp_path)
    _write_jsonl(
        logs_dir / "history_orderflowaggregated_XAUUSD.jsonl",
        [_orderflow_record("of-1", "2026-03-20T10:00:00Z")],
    )
    _write_jsonl(logs_dir / "history_volumeprofile_XAUUSD.jsonl", [])
    _write_jsonl(logs_dir / "history_wyckoff_XAUUSD.jsonl", [])
    _write_jsonl(
        logs_dir / "history_alertlistener_ESM6.CME_RITHMIC.jsonl",
        [_bookmap_record("bm-es-1", "2026-03-20T09:59:00Z", "ESM6.CME@RITHMIC", "stop", "ASK", "56")],
    )

    result = build_dataset(
        scope=DATASET_SCOPE_CTRADER_XAUUSD_H1_BASELINE,
        logs_dir=logs_dir,
        output_root=tmp_path / "artifacts" / "datasets",
        instrument_map_path=_write_instrument_map(tmp_path),
        run_id="es-rejected",
    )

    dataset = pd.read_parquet(result["dataset_path"])
    manifest = json.loads(result["manifest_path"].read_text(encoding="utf-8"))

    assert not dataset.loc[0, "has_bookmap_window"]
    assert dataset.loc[0, "bookmap_event_count_300s"] == 0
    assert manifest["excluded_sources"][0]["excluded_reason"] == "canonical_market_mismatch"
    assert manifest["excluded_sources"][0]["mapped_instrument"] == "US500"


def test_builder_excludes_bookmap_scope_from_ctrader_baseline(tmp_path: Path):
    test_builder_rejects_es_aliases_for_xauusd_scope(tmp_path)


def _make_logs_dir(tmp_path: Path) -> Path:
    logs_dir = tmp_path / "logs"
    logs_dir.mkdir(parents=True, exist_ok=True)
    return logs_dir


def _write_instrument_map(tmp_path: Path) -> Path:
    config_dir = tmp_path / "configs" / "training"
    config_dir.mkdir(parents=True, exist_ok=True)
    instrument_map_path = config_dir / "instrument_map.json"
    instrument_map_path.write_text(
        json.dumps({"XAUUSD": ["GC"], "US500": ["ES"]}, indent=2),
        encoding="utf-8",
    )
    return instrument_map_path


def _write_jsonl(path: Path, records: list[dict]) -> None:
    path.write_text(
        "\n".join(json.dumps(record, separators=(",", ":")) for record in records),
        encoding="utf-8",
    )


def _orderflow_record(event_id: str, timestamp: str) -> dict:
    return {
        "event": "order_flow_aggregated",
        "event_id": event_id,
        "instrument": "XAUUSD",
        "timeframe": "h1",
        "timestamp": timestamp,
        "bar_closed": True,
        "bar": {
            "open": 3000.0,
            "high": 3010.0,
            "low": 2990.0,
            "close": 3005.0,
            "spread": 0.2,
            "tick_size": 0.01,
        },
        "summary": {
            "level_count": 11,
            "total_volume": 1200,
            "buy_volume": 650,
            "sell_volume": 550,
            "delta_sum": 100,
            "abs_delta_sum": 300,
            "poc_price": 3002.0,
            "poc_distance_to_close_ticks": -300,
        },
    }


def _volume_profile_record(event_id: str, timestamp: str) -> dict:
    return {
        "event": "volume_profile",
        "event_id": event_id,
        "instrument": "XAUUSD",
        "timeframe": "h1",
        "timestamp": timestamp,
        "bar_closed": True,
    }


def _bookmap_record(
    event_id: str,
    timestamp: str,
    alias: str,
    event: str,
    side: str,
    value: str,
) -> dict:
    return {
        "event": event,
        "event_id": event_id,
        "instrument": alias,
        "timestamp": timestamp,
        "payload": {
            "side": side,
            "value": value,
        },
        "source_meta": {
            "alias": alias,
        },
    }
