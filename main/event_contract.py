from copy import deepcopy


SCHEMA_VERSION = "event-contract/v1"

BOOKMAP_EVENTS = {"alert", "dom", "dot", "wall"}
CTRADER_EVENTS = {"order_flow_aggregated", "volume_profile", "wyckoff_state"}
SUPPORTED_EVENTS = BOOKMAP_EVENTS | CTRADER_EVENTS
IDENTITY_KEYS = (
    "alias",
    "symbol",
    "instrument",
    "source",
    "source_instance",
    "profile_type",
    "timeframe",
)


def normalize_record(record, received_at):
    if not isinstance(record, dict):
        return None, "schema mismatch"

    schema = record.get("schema")
    if schema is not None:
        if schema != SCHEMA_VERSION:
            return None, "schema mismatch"
        return _normalize_v1(record, received_at)

    return _normalize_legacy(record, received_at)


def _normalize_v1(record, received_at):
    payload = record.get("payload")
    source_meta = record.get("source_meta") or {}
    if payload is None:
        payload = {}
    if not isinstance(payload, dict) or not isinstance(source_meta, dict):
        return None, "schema mismatch"

    event = record.get("event") or record.get("type")
    if event not in SUPPORTED_EVENTS:
        return None, "unsupported event"

    instrument = _extract_instrument(record, payload, source_meta)
    if not instrument:
        return None, "missing instrument"

    timestamp = record.get("timestamp")
    if not timestamp:
        return None, "missing timestamp"

    normalized_source_meta = deepcopy(source_meta)
    _preserve_identity(normalized_source_meta, record, payload)

    normalized = {
        "schema": SCHEMA_VERSION,
        "source": record.get("source") or _infer_source(event),
        "source_instance": record.get("source_instance") or _default_source_instance(event),
        "event": event,
        "event_id": record.get("event_id") or _build_event_id(record, event, instrument, timestamp),
        "instrument": instrument,
        "timestamp": timestamp,
        "received_at": received_at,
        "payload": deepcopy(payload),
        "source_meta": normalized_source_meta,
    }
    return normalized, None


def _normalize_legacy(record, received_at):
    event = _infer_legacy_event(record)
    if event not in SUPPORTED_EVENTS:
        return None, "unsupported event"

    instrument = _extract_instrument(record, record, {})
    if not instrument:
        return None, "missing instrument"

    timestamp = record.get("timestamp")
    if not timestamp:
        return None, "missing timestamp"

    payload = deepcopy(record)
    source_meta = {}
    _preserve_identity(source_meta, record, payload)

    for key in (
        "schema",
        "event",
        "type",
        "instrument",
        "timestamp",
        "source",
        "source_instance",
        "event_id",
        "source_meta",
    ):
        payload.pop(key, None)

    normalized = {
        "schema": SCHEMA_VERSION,
        "source": record.get("source") or _infer_source(event),
        "source_instance": record.get("source_instance") or _default_source_instance(event),
        "event": event,
        "event_id": record.get("event_id") or _build_event_id(record, event, instrument, timestamp),
        "instrument": instrument,
        "timestamp": timestamp,
        "received_at": received_at,
        "payload": payload,
        "source_meta": source_meta,
    }
    return normalized, None


def _infer_legacy_event(record):
    event = record.get("event") or record.get("type")
    if event in SUPPORTED_EVENTS:
        return event
    if "wyckoffVolume" in record or "waveVolume" in record:
        return "wyckoff_state"
    return event


def _extract_instrument(record, payload, source_meta):
    for candidate in (
        record.get("instrument"),
        payload.get("instrument") if isinstance(payload, dict) else None,
        source_meta.get("instrument") if isinstance(source_meta, dict) else None,
        record.get("alias"),
        payload.get("alias") if isinstance(payload, dict) else None,
        source_meta.get("alias") if isinstance(source_meta, dict) else None,
        record.get("symbol"),
        payload.get("symbol") if isinstance(payload, dict) else None,
        source_meta.get("symbol") if isinstance(source_meta, dict) else None,
    ):
        if isinstance(candidate, str) and candidate.strip():
            return candidate.strip()
    return None


def _preserve_identity(target, record, payload):
    for key in IDENTITY_KEYS:
        value = record.get(key)
        if value in (None, "") and isinstance(payload, dict):
            value = payload.get(key)
        if value not in (None, "") and key not in target:
            target[key] = value


def _infer_source(event):
    if event in BOOKMAP_EVENTS:
        return "bookmap"
    return "ctrader"


def _default_source_instance(event):
    if event in BOOKMAP_EVENTS:
        return "legacy-bookmap"
    return "legacy-ctrader"


def _build_event_id(record, event, instrument, timestamp):
    source = record.get("source") or _infer_source(event)
    clean_event = event.replace(" ", "-")
    return f"{source}-{clean_event}-{instrument}-{timestamp}"
