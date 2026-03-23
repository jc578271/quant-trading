from __future__ import annotations

import argparse
import csv
import json
import re
import sys
from pathlib import Path
from typing import Iterable


HISTORY_FILE_SCHEMA = "bookmap-history/v1"
SOURCE_EVENT_SCHEMA = "bookmap-log-row/v1"
EVENT_SOURCE = "bookmap"
EVENT_SOURCE_INSTANCE = "AlertListener"
EXPECTED_COLUMNS = (
    "timestamp",
    "alias",
    "type",
    "side",
    "price",
    "value",
    "price_min",
    "price_max",
    "bid_size",
    "ask_size",
    "duration_sec",
    "raw_text",
)


def non_null(value: str | None) -> str:
    return "" if value is None else value


def sanitize_event_id_part(value: str) -> str:
    return re.sub(r"[^a-zA-Z0-9._-]", "_", non_null(value))


def build_event_id(event: str, instrument: str, timestamp: str) -> str:
    return (
        f"{EVENT_SOURCE}-"
        f"{sanitize_event_id_part(event)}-"
        f"{sanitize_event_id_part(instrument)}-"
        f"{sanitize_event_id_part(timestamp)}"
    )


def normalize_history_event_name(raw_type: str | None) -> str:
    safe = re.sub(r"[^a-z0-9]+", "_", non_null(raw_type).strip().lower())
    safe = safe.strip("_")
    return safe or "log"


def csv_row_to_history_record(
    row: dict[str, str],
    sequence: int,
    *,
    fallback_alias: str = "",
    history_mode: str = "csv_import",
) -> dict[str, object]:
    alias = non_null(row.get("alias")).strip() or fallback_alias
    timestamp = non_null(row.get("timestamp")).strip()
    event = normalize_history_event_name(row.get("type"))

    payload = {
        "type": non_null(row.get("type")),
        "side": non_null(row.get("side")),
        "price": non_null(row.get("price")),
        "value": non_null(row.get("value")),
        "price_min": non_null(row.get("price_min")),
        "price_max": non_null(row.get("price_max")),
        "bid_size": non_null(row.get("bid_size")),
        "ask_size": non_null(row.get("ask_size")),
        "duration_sec": non_null(row.get("duration_sec")),
        "raw_text": non_null(row.get("raw_text")),
    }

    source_meta = {
        "alias": alias,
        "history_mode": history_mode,
    }

    return {
        "schema": HISTORY_FILE_SCHEMA,
        "source_event_schema": SOURCE_EVENT_SCHEMA,
        "source": EVENT_SOURCE,
        "source_instance": EVENT_SOURCE_INSTANCE,
        "event": event,
        "event_id": build_event_id(f"{event}-{sequence}", alias, timestamp),
        "instrument": alias,
        "timestamp": timestamp,
        "sequence": sequence,
        "payload": payload,
        "source_meta": source_meta,
    }


def derive_output_path(input_path: Path, output_dir: Path | None) -> Path:
    target_dir = output_dir if output_dir is not None else input_path.parent
    return target_dir / f"{input_path.stem}.jsonl"


def iter_csv_files(input_path: Path) -> Iterable[Path]:
    if input_path.is_file():
        yield input_path
        return

    yield from sorted(input_path.glob("history_alertlistener_*.csv"))


def convert_file(input_path: Path, output_path: Path, *, overwrite: bool) -> tuple[int, Path]:
    if output_path.exists() and not overwrite:
        raise FileExistsError(f"Output already exists: {output_path}")

    output_path.parent.mkdir(parents=True, exist_ok=True)
    row_count = 0

    with input_path.open("r", encoding="utf-8-sig", newline="") as csv_file, output_path.open(
        "w",
        encoding="utf-8",
        newline="",
    ) as jsonl_file:
        reader = csv.DictReader(csv_file)
        missing_columns = [column for column in EXPECTED_COLUMNS if column not in (reader.fieldnames or [])]
        if missing_columns:
            raise ValueError(
                f"{input_path} is missing expected columns: {', '.join(missing_columns)}"
            )

        fallback_alias = input_path.stem.removeprefix("history_alertlistener_")
        for row_count, row in enumerate(reader, start=1):
            record = csv_row_to_history_record(row, row_count, fallback_alias=fallback_alias)
            json.dump(record, jsonl_file, ensure_ascii=False, separators=(",", ":"))
            jsonl_file.write("\n")

    return row_count, output_path


def build_argument_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Convert legacy history_alertlistener_*.csv files into JSONL records."
    )
    parser.add_argument(
        "input",
        nargs="?",
        default="logs",
        help="Input CSV file or directory containing history_alertlistener_*.csv files. Defaults to ./logs.",
    )
    parser.add_argument(
        "--output-dir",
        help="Optional output directory for the generated .jsonl files. Defaults to the source file directory.",
    )
    parser.add_argument(
        "--overwrite",
        action="store_true",
        help="Overwrite existing .jsonl files if they already exist.",
    )
    return parser


def main(argv: list[str] | None = None) -> int:
    parser = build_argument_parser()
    args = parser.parse_args(argv)

    input_path = Path(args.input).resolve()
    output_dir = Path(args.output_dir).resolve() if args.output_dir else None

    if not input_path.exists():
        parser.error(f"Input path does not exist: {input_path}")

    files = list(iter_csv_files(input_path))
    if not files:
        parser.error(f"No history_alertlistener_*.csv files found in: {input_path}")

    converted = 0
    for file_path in files:
        output_path = derive_output_path(file_path, output_dir)
        rows, written_path = convert_file(file_path, output_path, overwrite=args.overwrite)
        converted += 1
        print(f"Converted {file_path} -> {written_path} ({rows} rows)")

    print(f"Finished converting {converted} file(s).")
    return 0


if __name__ == "__main__":
    sys.exit(main())
