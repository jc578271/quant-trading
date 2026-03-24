# Phase 6: Build and Train AI Trading Model - Research

**Researched:** 2026-03-24
**Domain:** Local offline training pipeline from existing JSONL logs
**Confidence:** MEDIUM

<user_constraints>
## User Constraints

- No Phase 6 `*-CONTEXT.md` exists yet.
- Locked repo constraints:
  - Stay local-first and desktop-native.
  - Do not assume cloud services.
  - Prefer deterministic preprocessing and versioned artifacts.
  - Current Bookmap and cTrader logs are not obviously aligned by instrument or timeframe.
- Canonical market mappings should be supported when dataset scope is defined explicitly, for example:
  - `XAUUSD` on cTrader maps to `GC*` contracts on Bookmap
  - `US500` maps to `ES*` contracts on Bookmap
- Planner discretion:
  - Dataset contract, join policy, label strategy, artifact layout, and promotion flow.
- Out of scope for this phase baseline:
  - Cloud training/inference services
  - Blindly joining unmatched Bookmap and cTrader feeds without a canonical instrument map
  - Live auto-retraining inside the runtime loop
</user_constraints>

<phase_requirements>
## Phase Requirements

`ROADMAP.md` defines Phase 6 but `REQUIREMENTS.md` has no official Phase 6 IDs yet. That is a traceability gap. Use these candidate IDs for planning unless requirements are added first.

| ID | Description | Research Support |
|----|-------------|-----------------|
| MLDATA-01 (candidate) | Raw JSONL logs are cataloged and normalized into a documented, deterministic training dataset contract. | Current Ownership, Actual Log Shapes, Dataset Contract |
| MLDATA-02 (candidate) | Dataset building enforces explicit scope, instrument compatibility, and join rules before rows are merged. | Summary, Actual Log Shapes, Architecture Decisions |
| MLLABEL-01 (candidate) | Labels are generated from local historical price movement without future leakage. | Label Strategy, Pitfalls |
| MLTRAIN-01 (candidate) | Offline training produces versioned artifacts, metrics, and manifests from local data only. | Architecture Decisions, Implementation Slices |
| MLTRAIN-02 (candidate) | Promotion into `runtime/model.pkl` is explicit and separate from training. | Current Ownership, Architecture Decisions, Validation Architecture |
| MLEVAL-01 (candidate) | Walk-forward evaluation and prediction outputs are reproducible enough to replay and compare safely. | Label Strategy, Validation Architecture |
| GAP | Phase 6 has a roadmap entry but no official requirements/traceability row in `.planning/REQUIREMENTS.md`. | This section, Validation Architecture |
</phase_requirements>

## Summary

Phase 6 should be planned as an **offline dataset-and-training pipeline**, not as a change to live startup behavior. Current runtime code still bootstraps a dummy `RandomForestClassifier` inside [`main/ai_analyzer.py`](D:/projects/quant-trading/main/ai_analyzer.py) and loads or writes the runtime artifact at [`runtime/model.pkl`](D:/projects/quant-trading/main/runtime_paths.py). That is acceptable for the existing bridge, but it is the wrong place to build a real training workflow.

The key planning problem is **data scope and alignment**, not model choice. The required files show one Bookmap event stream for `ESM6.CME@RITHMIC` and three cTrader history streams for `XAUUSD`. The cTrader files can support a first local baseline if one series is chosen as the anchor and the others are joined backward in time. Bookmap should also be allowed into training, but only through an explicit canonical instrument map. Under that policy, `GC*` Bookmap contracts can enrich `XAUUSD`, and `ES*` contracts can enrich `US500`; the current `ESM6` sample still cannot enrich the current `XAUUSD` baseline because it maps to a different canonical market family.

**Primary recommendation:** Plan Phase 6 around a cTrader-anchored baseline dataset that supports optional Bookmap enrichment through a canonical instrument map, built deterministically from raw `logs/*.jsonl` into versioned parquet artifacts, with 1-bar-ahead horizon labels, walk-forward evaluation, and an explicit promotion step into `runtime/model.pkl`.

## Current Ownership

| Component | Current Owner | What It Actually Owns | Confidence |
|----------|---------------|-----------------------|------------|
| [`main/ai_analyzer.py`](D:/projects/quant-trading/main/ai_analyzer.py) | Python runtime | Live feature extraction, live inference, and dummy model bootstrap when `runtime/model.pkl` is missing | HIGH |
| [`main/runtime_paths.py`](D:/projects/quant-trading/main/runtime_paths.py) | Python runtime | Canonical path mapping for `runtime/status.json`, `runtime/model.pkl`, and raw history files under `logs/*.jsonl` | HIGH |
| [`scripts/convert_alertlistener_history_csv_to_jsonl.py`](D:/projects/quant-trading/scripts/convert_alertlistener_history_csv_to_jsonl.py) | Offline conversion script | Legacy Bookmap CSV to `bookmap-history/v1` JSONL conversion, including `sequence`, `payload`, and `history_mode` | HIGH |
| [`docs/pipeline-runtime-operations.md`](D:/projects/quant-trading/docs/pipeline-runtime-operations.md) | Documentation | Stale artifact description: it still documents `runtime/*.csv`, while current code/tests use `logs/*.jsonl` histories plus `runtime/model.pkl` | HIGH |
| Repo overall | No dedicated training package yet | There is no current offline dataset builder, feature builder, trainer, or promotion CLI | HIGH |

## Actual Log Shapes

| Dataset | Rows | Schema | Shape | Planning Impact |
|---------|------|--------|-------|-----------------|
| [`logs/history_alertlistener_ESM6.CME_RITHMIC.jsonl`](D:/projects/quant-trading/logs/history_alertlistener_ESM6.CME_RITHMIC.jsonl) | 1213 | `bookmap-history/v1` | Top-level: `schema, source_event_schema, source, source_instance, event, event_id, instrument, timestamp, sequence, payload, source_meta` | Bookmap event scope that becomes trainable only after canonical market mapping and event-window aggregation |
| [`logs/history_orderflowaggregated_XAUUSD.jsonl`](D:/projects/quant-trading/logs/history_orderflowaggregated_XAUUSD.jsonl) | 32 | `orderflow-history/v2` | `bar`, `summary`, `levels[]`, `timeframe=h1`, `instrument=XAUUSD` | Best anchor series for a first cTrader baseline |
| [`logs/history_volumeprofile_XAUUSD.jsonl`](D:/projects/quant-trading/logs/history_volumeprofile_XAUUSD.jsonl) | 21 | `volumeprofile-history/v2` | `profile_type`, `bar`, `summary`, `levels[]`, `timeframe=h1`, `instrument=XAUUSD` | Enrichment series; only 19 exact timestamp overlaps with order flow |
| [`logs/history_wyckoff_XAUUSD.jsonl`](D:/projects/quant-trading/logs/history_wyckoff_XAUUSD.jsonl) | 1416 | `wyckoff-history/v2` | `bar`, `wyckoff`, `wave`, `summary`, `timeframe=Re50`, `instrument=XAUUSD` | Higher-frequency enrichment series; must be window-aggregated before joining |

### Incompatibilities That Matter

- **Canonical market mismatch:** current Bookmap sample is `ESM6.CME@RITHMIC`, which maps to an `ES/US500` family rather than the current `XAUUSD/GC` family.
- **Timeframe mismatch:** cTrader order flow and volume profile are `h1`; Wyckoff is `Re50`; Bookmap has no timeframe field.
- **Shape mismatch:** Bookmap records are event-sequence rows with mostly string payload values; cTrader rows are nested numeric bar records.
- **Coverage mismatch:** order flow has 32 rows, volume profile 21 rows, and only 19 exact timestamp overlaps with order flow.
- **Data quality anomaly:** Wyckoff contains at least one anomalous row with negative `wyckoff.time` and `bar.range_ticks = 89`; dataset building needs explicit quarantine rules.

## Recommended Dataset Contract

Use one contract for **derived training rows**, separate from raw history contracts. Keep raw JSONL immutable under `logs/`; write normalized artifacts under `artifacts/`.

### Recommended Baseline Scope

- `dataset_scope`: `ctrader_xauusd_h1_baseline`
- `anchor_series`: `order_flow_aggregated` H1
- `join_series`:
  - `volume_profile` H1 via backward exact/as-of join
  - `wyckoff_state` via backward window aggregation up to the anchor timestamp
  - `bookmap_alertlistener` via backward event-window aggregation after canonical mapping
- `canonical_instrument_map`:
  - `XAUUSD` -> Bookmap root `GC`
  - `US500` -> Bookmap root `ES`
- `excluded_from_current_xauusd_sample`:
  - `history_alertlistener_ESM6.CME_RITHMIC.jsonl` because it maps to `ES/US500`, not `XAUUSD/GC`

### Training Row Fields

| Group | Required Fields |
|------|-----------------|
| Identity | `row_id`, `dataset_scope`, `instrument`, `anchor_timestamp`, `anchor_timeframe`, `anchor_event_id` |
| Provenance | `source_schemas`, `source_files`, `orderflow_event_id`, `volumeprofile_event_id`, `wyckoff_window_start`, `wyckoff_window_end`, `build_run_id` |
| Anchor Bar | `open`, `high`, `low`, `close`, `spread`, `tick_size` |
| Order Flow Features | `level_count`, `total_volume`, `buy_volume`, `sell_volume`, `delta_sum`, `abs_delta_sum`, `poc_price`, `poc_distance_to_close_ticks`, plus fixed `levels[]` aggregates |
| Volume Profile Features | `profile_type`, `vp_poc`, `vp_vah`, `vp_val`, `vp_total_volume`, `vp_value_area_width_ticks`, `vp_poc_to_close_ticks` |
| Wyckoff Features | `wy_count`, `wy_last_direction_sign`, `wy_last_wave_volume`, `wy_sum_wave_volume`, `wy_mean_volume_per_time`, `wy_max_wave_efficiency`, `wy_last_close_to_zigzag_ticks` |
| Bookmap Features | `has_bookmap_window`, `bookmap_event_count_300s`, `bookmap_dot_count_300s`, `bookmap_stop_count_300s`, `bookmap_wall_count_300s`, `bookmap_signed_value_300s` |
| Missingness Flags | `has_volume_profile`, `has_wyckoff_window`, `quarantined_source_count` |
| Labels | `future_return_ticks_1`, `target_class_1`, `label_horizon_bars`, `label_threshold_ticks` |

### Artifact Layout

```text
artifacts/
├── datasets/{scope}/{run_id}/
│   ├── raw/
│   ├── normalized/
│   ├── training_rows.parquet
│   ├── dataset_manifest.json
│   ├── feature_schema.json
│   └── label_spec.json
└── models/{scope}/{run_id}/
    ├── model.pkl
    ├── metrics.json
    ├── predictions.parquet
    └── train_config.json
```

## Recommended Label / Target Strategy

**Use only the anchor series to define the target.** Do not use runtime trade history and do not derive labels from Bookmap events.

- **Anchor:** `history_orderflowaggregated_XAUUSD.jsonl`
- **Feature cutoff:** a row may use only source data with `timestamp <= anchor_timestamp`
- **Primary numeric target:** `future_return_ticks_1 = (close[t+1] - close[t]) / tick_size`
- **Baseline class target:**
  - `1` if `future_return_ticks_1 >= long_threshold_ticks`
  - `-1` if `future_return_ticks_1 <= short_threshold_ticks`
  - `0` otherwise
- **Why 1-bar horizon first:** there are only 32 anchor rows in scope; longer horizons shrink the runnable sample too aggressively for the first pipeline slice
- **Evaluation:** keep both the numeric target and derived class in artifacts so later phases can compare regression vs classification without rebuilding the dataset

## Architecture Decisions

1. **Offline package, not runtime startup**
   - Add `main/training/` for dataset build, feature build, training, and promotion commands.
   - Do not train inside `AIAnalyzer`.

2. **cTrader-anchored, Bookmap-enriched promoted model**
   - First promoted model scope is `ctrader_xauusd_h1_baseline`.
   - Allow Bookmap enrichment only when a Bookmap alias maps into the same canonical market family as the cTrader anchor, for example `XAUUSD -> GC*` and `US500 -> ES*`.
   - Treat unmatched Bookmap captures as cataloged-but-unjoined rather than silently merged.

3. **Deterministic preprocessing**
   - Input: immutable `logs/*.jsonl`
   - Output: versioned parquet + manifest artifacts
   - Every run records source files, row counts, schema ids, join policy, and quarantined rows.

4. **Explicit promotion**
   - Training writes only to `artifacts/models/...`.
   - A separate `promote_model.py` copies one blessed artifact into `runtime/model.pkl`.

5. **Trust code over stale docs**
   - Planning should follow [`main/runtime_paths.py`](D:/projects/quant-trading/main/runtime_paths.py) and [`tests/test_runtime_artifacts.py`](D:/projects/quant-trading/tests/test_runtime_artifacts.py), not the older CSV-based runtime doc.

## Implementation Slices

### Slice 1: Catalog Raw Logs and Freeze Dataset Scope

- Add a Phase 6 dataset manifest schema.
- Implement per-source schema validation and quarantine rules.
- Record row counts, time ranges, instruments, timeframes, and overlap stats.
- Output: `dataset_manifest.json` and source-level contract tests.

### Slice 2: Build Deterministic cTrader Training Rows

- Flatten order flow, volume profile, and Wyckoff histories.
- Aggregate `levels[]` into fixed numeric features.
- Join H1/H1 with backward exact-or-asof rules.
- Aggregate Wyckoff rows into anchor-window summaries.
- Output: `training_rows.parquet`, `feature_schema.json`, `label_spec.json`.

### Slice 3: Train and Evaluate Offline

- Train a simple sklearn baseline from the derived training rows.
- Use walk-forward splits only.
- Persist metrics, predictions, feature importances, and config alongside the model.
- Output: `artifacts/models/{scope}/{run_id}/...`.

### Slice 4: Promotion, Runtime Handoff, and Docs

- Add explicit model promotion into `runtime/model.pkl`.
- Document how runtime and offline artifacts differ.
- Add tests that guarantee training does not mutate live runtime artifacts.
- Output: promotion CLI, operator notes, and regression coverage.

## Standard Stack

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Python | existing repo/runtime | Training CLI and artifact pipeline | Matches the current desktop-native Python bridge |
| `numpy` | existing `main/requirements.txt` dep | Numeric feature arrays | Already part of the current analyzer stack |
| `pandas` | existing `main/requirements.txt` dep | JSONL ingestion, flattening, time-aware joins | Best fit for deterministic tabular preprocessing |
| `scikit-learn` | existing `main/requirements.txt` dep | Baseline classifier and walk-forward evaluation | Already used in runtime code today |
| `joblib` | existing `main/requirements.txt` dep | Model persistence | Matches current `model.pkl` load pattern |
| `pyarrow` | new pinned dep to add | Parquet dataset storage | Needed for versioned, deterministic training artifacts |
| `pytest` | existing repo test framework | Dataset/training regression tests | `pytest.ini` and current tests already exist |

## Common Pitfalls

- **False fusion of Bookmap and cTrader data:** same folder does not mean same market scope, and same timestamp does not mean same canonical market family.
- **Future leakage through joins:** all enrichment joins must be backward-only relative to the anchor timestamp.
- **Variable-length `levels[]` features:** convert them to fixed aggregates before modeling.
- **Training writing to runtime:** only promotion may touch `runtime/model.pkl`.
- **Over-reading tiny samples:** current counts are enough to build a pipeline, not to claim robust trading edge.

## Open Questions

1. **Should Phase 6 stop at a cTrader-only baseline, or also define canonical Bookmap mappings now?**
   - Recommendation: define the canonical mappings now and implement optional Bookmap enrichment immediately, while keeping unmatched captures excluded from the current sample.

2. **Should volume-profile gaps drop anchor rows or survive via missingness flags?**
   - Recommendation: keep rows, set `has_volume_profile = false`, and record the join miss in the manifest.

3. **Should the runtime keep consuming a classifier with `-1/0/1`, or should Phase 6 also add a probability/threshold adapter?**
   - Recommendation: keep the first promoted artifact classifier-compatible to minimize Phase 6 runtime churn.

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | `pytest` |
| Config file | [`pytest.ini`](D:/projects/quant-trading/pytest.ini) |
| Quick run command | `python -m pytest tests/test_training_contracts.py -q` |
| Full suite command | `python -m pytest tests -q` |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| MLDATA-01 | Raw logs normalize into one documented derived-row contract | unit | `python -m pytest tests/test_training_contracts.py -q` | ❌ Wave 0 |
| MLDATA-02 | Join rules reject instrument/timeframe mismatches and enforce backward joins | unit | `python -m pytest tests/test_training_alignment.py -q` | ❌ Wave 0 |
| MLLABEL-01 | Labels use only future anchor closes and no feature leakage | unit | `python -m pytest tests/test_training_labels.py -q` | ❌ Wave 0 |
| MLTRAIN-01 | Offline training writes versioned artifacts and manifests | integration | `python -m pytest tests/test_training_runner.py -q` | ❌ Wave 0 |
| MLTRAIN-02 | Promotion copies only blessed artifact into `runtime/model.pkl` | integration | `python -m pytest tests/test_model_promotion.py -q` | ❌ Wave 0 |
| MLEVAL-01 | Walk-forward evaluation emits deterministic metrics/predictions | integration | `python -m pytest tests/test_training_eval.py -q` | ❌ Wave 0 |

### Sampling Rate

- **Per task commit:** targeted Phase 6 pytest file
- **Per wave merge:** `python -m pytest tests -q`
- **Phase gate:** full suite green plus one deterministic end-to-end training run on fixture data before `/gsd:verify-work`

### Wave 0 Gaps

- [ ] `tests/test_training_contracts.py`
- [ ] `tests/test_training_alignment.py`
- [ ] `tests/test_training_labels.py`
- [ ] `tests/test_training_runner.py`
- [ ] `tests/test_model_promotion.py`
- [ ] `tests/test_training_eval.py`
- [ ] `main/training/` package does not exist yet
- [ ] Add one pinned training dependency file for `pyarrow` and any new training-only deps
- [ ] Normalize the Python invocation for this workstation if `python -m pytest` is not currently stable in shell

## Sources

### Primary (HIGH confidence)

- [`main/ai_analyzer.py`](D:/projects/quant-trading/main/ai_analyzer.py) - current model bootstrap, live feature extraction, and inference ownership
- [`main/runtime_paths.py`](D:/projects/quant-trading/main/runtime_paths.py) - runtime/log artifact authority
- [`scripts/convert_alertlistener_history_csv_to_jsonl.py`](D:/projects/quant-trading/scripts/convert_alertlistener_history_csv_to_jsonl.py) - Bookmap history conversion contract
- [`docs/event-contract-v1.md`](D:/projects/quant-trading/docs/event-contract-v1.md) - canonical v1 event envelope
- [`tests/test_runtime_artifacts.py`](D:/projects/quant-trading/tests/test_runtime_artifacts.py) - current artifact-path expectations
- Required JSONL logs listed in the task prompt - row counts, keys, timeframes, and overlap evidence

### Secondary (MEDIUM confidence)

- [`docs/pipeline-runtime-operations.md`](D:/projects/quant-trading/docs/pipeline-runtime-operations.md) - useful ownership intent, but stale relative to code/tests
- [`main/requirements.txt`](D:/projects/quant-trading/main/requirements.txt) - current Python dependency baseline

## Metadata

**Confidence breakdown:**
- Current ownership: HIGH - directly verified in local code and tests
- Log schema findings: HIGH - directly verified from the required JSONL files
- Dataset contract recommendation: MEDIUM - grounded in local evidence but still a design recommendation
- Label strategy: MEDIUM - best fit for current data volume, but thresholds remain a planning choice

**Research date:** 2026-03-24
**Valid until:** 2026-04-23 or until new aligned same-instrument logs are added
