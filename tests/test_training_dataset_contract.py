from __future__ import annotations

import json
from pathlib import Path

from training.contracts import (
    CANONICAL_MARKET_MAP,
    DATASET_SCOPE_CTRADER_XAUUSD_H1_BASELINE,
    DERIVED_DATASET_FIELDS,
)


REPO_ROOT = Path(__file__).resolve().parent.parent


def test_phase_6_requirement_ids_are_mapped():
    requirements_text = (REPO_ROOT / ".planning" / "REQUIREMENTS.md").read_text(encoding="utf-8")
    roadmap_text = (REPO_ROOT / ".planning" / "ROADMAP.md").read_text(encoding="utf-8")

    assert "MLDATA-01" in requirements_text
    assert "MLDATA-02" in requirements_text
    assert "MLLABEL-01" in requirements_text
    assert "MLTRAIN-01" in requirements_text
    assert "MLTRAIN-02" in requirements_text
    assert "MLEVAL-01" in requirements_text
    assert "[MLDATA-01, MLDATA-02, MLLABEL-01, MLTRAIN-01, MLTRAIN-02, MLEVAL-01]" in roadmap_text


def test_contract_locks_ctrader_xauusd_baseline_scope():
    contract_text = (REPO_ROOT / "docs" / "ai-model-data-contract.md").read_text(encoding="utf-8")

    assert DATASET_SCOPE_CTRADER_XAUUSD_H1_BASELINE == "ctrader_xauusd_h1_baseline"
    assert "ctrader_xauusd_h1_baseline" in contract_text
    assert DERIVED_DATASET_FIELDS == (
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


def test_contract_maps_xauusd_to_gc_bookmap_root():
    instrument_map = json.loads(
        (REPO_ROOT / "configs" / "training" / "instrument_map.json").read_text(encoding="utf-8")
    )

    assert instrument_map["XAUUSD"] == ["GC"]
    assert CANONICAL_MARKET_MAP["XAUUSD"] == ("GC",)


def test_contract_maps_us500_to_es_bookmap_root():
    instrument_map = json.loads(
        (REPO_ROOT / "configs" / "training" / "instrument_map.json").read_text(encoding="utf-8")
    )

    assert instrument_map["US500"] == ["ES"]
    assert CANONICAL_MARKET_MAP["US500"] == ("ES",)
