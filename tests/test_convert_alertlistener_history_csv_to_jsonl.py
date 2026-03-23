from __future__ import annotations

import csv
import importlib.util
import json
from pathlib import Path


SCRIPT_PATH = (
    Path(__file__).resolve().parent.parent / "scripts" / "convert_alertlistener_history_csv_to_jsonl.py"
)
SPEC = importlib.util.spec_from_file_location("convert_alertlistener_history_csv_to_jsonl", SCRIPT_PATH)
MODULE = importlib.util.module_from_spec(SPEC)
assert SPEC is not None and SPEC.loader is not None
SPEC.loader.exec_module(MODULE)


def test_csv_row_to_history_record_matches_alertlistener_contract():
    record = MODULE.csv_row_to_history_record(
        {
            "timestamp": "2026-03-24T00:00:00.0000000Z",
            "alias": "ESH6.CME@RITHMIC",
            "type": "WALL ADDED",
            "side": "BID",
            "price": "6700",
            "value": "125",
            "price_min": "",
            "price_max": "",
            "bid_size": "",
            "ask_size": "",
            "duration_sec": "5",
            "raw_text": "WALL ADDED BID 125 at 6700",
        },
        7,
    )

    assert record["schema"] == "bookmap-history/v1"
    assert record["source_event_schema"] == "bookmap-log-row/v1"
    assert record["event"] == "wall_added"
    assert record["instrument"] == "ESH6.CME@RITHMIC"
    assert record["sequence"] == 7
    assert record["payload"]["duration_sec"] == "5"
    assert record["source_meta"]["history_mode"] == "csv_import"


def test_convert_file_writes_jsonl_records(tmp_path):
    csv_path = tmp_path / "history_alertlistener_ESH6.CME_RITHMIC.csv"
    with csv_path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.writer(handle)
        writer.writerow(MODULE.EXPECTED_COLUMNS)
        writer.writerow(
            [
                "2026-03-24T00:00:00.0000000Z",
                "ESH6.CME@RITHMIC",
                "DOT",
                "ASK",
                "6775.25",
                "94",
                "6775.00",
                "6775.50",
                "20",
                "114",
                "",
                "DOT ASK 94 @ 6775.25",
            ]
        )

    output_path = tmp_path / "history_alertlistener_ESH6.CME_RITHMIC.jsonl"
    rows, written_path = MODULE.convert_file(csv_path, output_path, overwrite=False)

    assert rows == 1
    assert written_path == output_path

    lines = output_path.read_text(encoding="utf-8").splitlines()
    assert len(lines) == 1
    record = json.loads(lines[0])
    assert record["event"] == "dot"
    assert record["payload"]["price_min"] == "6775.00"
    assert record["payload"]["ask_size"] == "114"


def test_iter_csv_files_filters_alertlistener_history_pattern(tmp_path):
    target = tmp_path / "history_alertlistener_GCJ6.COMEX.csv"
    other = tmp_path / "other.csv"
    target.write_text("", encoding="utf-8")
    other.write_text("", encoding="utf-8")

    files = list(MODULE.iter_csv_files(tmp_path))

    assert files == [target]
