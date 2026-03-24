# Phase 6: Build and Train AI Trading Model - Research

**Researched:** 2026-03-24
**Domain:** Local offline training pipeline for trading-event JSONL logs
**Confidence:** MEDIUM

## User Constraints

- No phase-specific `*-CONTEXT.md` exists yet, so there are no locked per-phase decisions to copy verbatim.
- Locked repo constraints from existing project docs/code:
  - The system is local-first. Cloud deployment and hosted APIs remain out of scope.
  - The current runtime expects a loadable model artifact at `runtime/model.pkl`.
  - Phase 6 must use captured Bookmap and cTrader JSONL logs without breaking current runtime assumptions.
- Claude's discretion for planning:
  - Dataset contract, scope boundaries, join policy, label definition, feature engineering approach, train/eval workflow, artifact versioning, and promotion flow.
- Deferred ideas already out of scope from `REQUIREMENTS.md`:
  - Cloud deployment or hosted APIs
  - Multi-broker execution abstraction
  - Portfolio/risk platform features beyond per-trade controls

<phase_requirements>
## Phase Requirements

> `REQUIREMENTS.md` and `ROADMAP.md` do not yet assign official requirement IDs to Phase 6. The table below proposes candidate IDs the planner can use, but traceability is currently a requirement gap.

| ID | Description | Research Support |
|----|-------------|-----------------|
| MLDATA-01 (candidate) | Raw Bookmap and cTrader JSONL logs can be cataloged and normalized into a documented fixed training dataset contract without silent schema loss. | Standard Stack, Architecture Patterns 1-2, Pitfalls 1-3 |
| MLDATA-02 (candidate) | Dataset building enforces explicit scope and join policy so only same-instrument, time-compatible feeds are merged. | Summary, Architecture Pattern 2, Pitfall 1 |
| MLTRAIN-01 (candidate) | A documented offline workflow produces a versioned model artifact plus metrics, config, and dataset manifest. | Standard Stack, Architecture Pattern 3, Don't Hand-Roll |
| MLTRAIN-02 (candidate) | Promotion to `runtime/model.pkl` is explicit and separate from training so runtime behavior remains stable. | Summary, Architecture Pattern 3, Pitfall 4 |
| MLEVAL-01 (candidate) | Labels, thresholds, walk-forward evaluation, and replay outputs are explicit enough to inspect and improve safely. | Summary, Architecture Pattern 4, Pitfalls 2 and 5 |
| GAP | Official Phase 6 requirement IDs are missing from `.planning/REQUIREMENTS.md` traceability. | Open Questions, Validation Architecture |
</phase_requirements>

## Summary

Phase 6 should be planned as an **offline dataset-and-training pipeline**, not as a modification to the live analyzer loop. Current repo evidence shows that the runtime still loads or creates a dummy scikit-learn model in `main/ai_analyzer.py`, while the actual persisted runtime artifact path is `runtime/model.pkl`. Training directly inside `AIAnalyzer` would make results non-reproducible, couple experimentation to live startup, and risk breaking the existing inference path.

The largest planning constraint is **data contract and scope alignment**, not model selection. Current logs are schema-misaligned and instrument-misaligned: Bookmap histories are `bookmap-history/v1` event streams for `ESM6.CME@RITHMIC` and `GCJ6.COMEX@RITHMIC`, while cTrader histories are nested v2 histories for `XAUUSD` (`orderflow-history/v2`, `volumeprofile-history/v2`, `wyckoff-history/v2`). The cTrader feeds can be aligned into one XAUUSD dataset scope; the current Bookmap feeds cannot be validly fused with that dataset because there is no shared instrument or overlapping canonical market series. The phase therefore needs an explicit **dataset scope / join policy** before any training work.

The safest v1 plan is: build a **cTrader-first baseline dataset** for `XAUUSD`, train an offline scikit-learn model with walk-forward time-series evaluation, store versioned artifacts outside `runtime/`, and add an explicit promotion step that copies a blessed artifact into `runtime/model.pkl`. Treat current Bookmap logs as a separate dataset scope and contract-validation fixture set until same-instrument, overlap-capable captures exist.

**Primary recommendation:** Use a cTrader-first, fixed-schema parquet training pipeline with explicit dataset scopes, backward `merge_asof` alignment, horizon-based labels, walk-forward `TimeSeriesSplit` evaluation, and explicit model promotion into `runtime/model.pkl`.

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Python | 3.12.10 | Training/runtime interpreter | Matches observed local environment and current pytest setup |
| `pandas` | 3.0.0 | JSONL ingestion, flattening, as-of joins, feature tables | Standard tabular/time-series prep library; official docs cover `read_json`, `json_normalize`, and `merge_asof` |
| `pyarrow` | 23.0.1 | Parquet read/write for normalized datasets and predictions | Standard local columnar storage; faster and more reproducible than ad hoc JSONL feature caches |
| `numpy` | 2.3.4 | Numeric arrays and deterministic feature calculations | Already in repo requirements and standard for sklearn-compatible data |
| `scikit-learn` | 1.8.0 | Baseline model training, walk-forward evaluation, persisted pipeline | Already used in repo, supports `Pipeline`, `TimeSeriesSplit`, metrics, and persisted estimators |
| `joblib` | 1.5.3 | Persist/load trained sklearn artifact | Matches current runtime `model.pkl` loading pattern and official sklearn persistence guidance |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `pytest` | 9.0.2 | Regression tests for dataset building, training, and promotion | Use for every new phase-6 pipeline component |
| stdlib `argparse` + `json` | bundled | CLI/config + manifests | Use instead of adding Hydra/MLflow unless the phase scope expands materially |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `scikit-learn` baseline | XGBoost/LightGBM | Stronger on large tabular sets, but adds non-trivial dependency/runtime complexity and is not needed for the first reproducible pipeline |
| local manifest JSON + artifact dirs | MLflow/DVC | More experiment management features, but overkill for a local-only first training phase |
| parquet feature store | JSONL-derived ad hoc CSV/JSON artifacts | Simpler short-term, but slower, larger, and less stable for replayable train/eval runs |

**Installation:**
```bash
pip install numpy==2.3.4 pandas==3.0.0 pyarrow==23.0.1 scikit-learn==1.8.0 joblib==1.5.3 pytest==9.0.2
```

**Version verification:** As of 2026-03-24, the versions above were verified against official PyPI project pages/search results. Direct PyPI JSON fetch was blocked by `robots.txt`, so PyPI page/search results were used instead. Keep training dependencies in a separate pinned file such as `main/requirements-train.txt` so runtime dependencies are not destabilized by Phase 6.

## Architecture Patterns

### Recommended Project Structure
```text
main/
├── training/
│   ├── build_dataset.py      # JSONL -> normalized parquet + manifests
│   ├── build_features.py     # fixed-schema feature table + labels
│   ├── train_model.py        # offline train/eval run
│   ├── promote_model.py      # explicit copy into runtime/model.pkl
│   └── contracts.py          # dataset-scope and feature-schema validation
configs/
├── training/
│   ├── ctrader_xauusd_baseline.json
│   └── bookmap_event_only.json
artifacts/
├── datasets/
│   └── {scope}/{run_id}/
├── models/
│   └── {scope}/{run_id}/
tests/
├── fixtures/phase6/
└── test_training_*.py
runtime/
└── model.pkl                 # promoted artifact only, never direct training output
```

### Observed Dataset Inventory
| Scope | Files | Rows | Time Range | Notes |
|------|------|------|-----------|------|
| `bookmap_esm6_events` | `history_alertlistener_ESM6.CME_RITHMIC.jsonl` | 1188 | 2026-03-23 17:33Z -> 2026-03-24 14:47Z | Event stream with nested string payload fields |
| `bookmap_gcj6_events` | `history_alertlistener_GCJ6.COMEX_RITHMIC.jsonl` | 2415 | 2026-03-23 17:31Z -> 2026-03-24 14:46Z | Event stream with mixed event taxonomy |
| `ctrader_xauusd_orderflow_h1` | `history_orderflowaggregated_XAUUSD.jsonl` | 32 | 2026-03-20 07:00Z -> 2026-03-23 15:00Z | H1 anchor candidate |
| `ctrader_xauusd_volumeprofile_h1` | `history_volumeprofile_XAUUSD.jsonl` | 21 | 2026-03-20 20:00Z -> 2026-03-23 17:00Z | H1 enrichment candidate |
| `ctrader_xauusd_wyckoff_re50` | `history_wyckoff_XAUUSD.jsonl` | 1416 | 2026-03-20 06:09Z -> 2026-03-23 16:57Z | Higher-frequency enrichment candidate |

### Pattern 1: Dataset Scope Before Model Scope
**What:** Define a `dataset_scope` first. A scope is the smallest valid combination of source family, canonical instrument, anchor series, and label definition that can be trained without ambiguous joins.

**When to use:** Always. This is the first design decision for Phase 6.

**Recommendation:** Start with `ctrader_xauusd_h1_baseline` as the promoted-model scope. Keep `bookmap_esm6_events` and `bookmap_gcj6_events` as separate scopes until matching price/label series exist.

**Example:**
```json
{
  "dataset_scope": "ctrader_xauusd_h1_baseline",
  "anchor_source": "orderflow-history/v2",
  "anchor_event": "order_flow_aggregated",
  "instrument": "XAUUSD",
  "join_sources": [
    "volumeprofile-history/v2",
    "wyckoff-history/v2"
  ],
  "label_spec": {
    "type": "future_return_classification",
    "horizon_bars": 3,
    "long_threshold_ticks": 150,
    "short_threshold_ticks": -150
  }
}
```

### Pattern 2: Anchor-On-One-Series, Enrich With Backward As-Of Joins
**What:** Use one anchor series per dataset scope, then join slower/faster feeds backward in time with explicit tolerance. Do not inner-join or nearest-join raw trading feeds casually.

**When to use:** When building the feature table for multi-feed cTrader training.

**Recommendation:** Use H1 order-flow bars as the anchor for the first XAUUSD dataset. Join volume-profile H1 rows with exact or short-tolerance backward matching, then aggregate the higher-frequency Wyckoff stream into rolling summaries up to the anchor timestamp.

**Why:** `pandas.merge_asof` is purpose-built for nearest-time joins, but only after sorting and only when the join semantics are explicit. This prevents accidental future leakage.

**Example:**
```python
# Source: https://pandas.pydata.org/docs/reference/api/pandas.merge_asof.html
dataset = pd.merge_asof(
    left=order_flow.sort_values("timestamp"),
    right=volume_profile.sort_values("timestamp"),
    on="timestamp",
    by="instrument",
    direction="backward",
    tolerance=pd.Timedelta("2h"),
)
```

### Pattern 3: Offline Train -> Evaluate -> Promote
**What:** Training writes versioned artifacts to `artifacts/models/{scope}/{run_id}/`. Promotion is a separate command that copies one blessed `model.pkl` to `runtime/model.pkl`.

**When to use:** Always. Never let `train_model.py` write directly to `runtime/model.pkl`.

**Why:** Current runtime assumptions remain intact, and the promoted model becomes an explicit operational decision instead of a side effect of experimentation.

**Recommended artifact payload per run:**
- `model.pkl`
- `metrics.json`
- `train_config.json`
- `dataset_manifest.json`
- `feature_schema.json`
- `label_spec.json`
- `predictions.parquet`
- `environment.json`

### Pattern 4: Horizon-Based Labels First, Not Live PnL Labels
**What:** Define labels from future price movement on the anchor series, not from simulated trades or runtime side effects.

**When to use:** For the first baseline model in this repo.

**Recommendation:** Use a 3-class horizon label on the anchor close:
- `1` if future close over `N` anchor bars rises above a positive tick threshold
- `-1` if it falls below a negative tick threshold
- `0` otherwise

**Why:** There is no current `trade_history.jsonl` dataset available for supervised labels, and current runtime trade generation is itself driven by the dummy model. Using live-trade output as training truth would create circular labels.

### Anti-Patterns to Avoid
- **Training in `AIAnalyzer` startup:** This makes experiments non-reproducible and changes runtime behavior as a side effect.
- **Joining Bookmap and cTrader solely on timestamp:** Current files are instrument-misaligned and would create false training rows.
- **Feeding raw `levels` arrays directly to sklearn as variable-length objects:** First aggregate them into a fixed schema.
- **Writing training intermediates back into `logs/`:** Keep `logs/` immutable as raw capture history and write derived data under `artifacts/`.
- **Using shuffled train/test splits:** This leaks future information in time-ordered trading data.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Time-series cross-validation | Custom split logic | `sklearn.model_selection.TimeSeriesSplit` | Official implementation preserves time order and supports `gap` and `test_size` |
| Timestamp-nearest feed joins | Manual loop-based join code | `pandas.merge_asof` | Correct semantics for backward/forward/nearest joins and explicit tolerances |
| JSON flattening for nested records | Ad hoc dict walking everywhere | `pandas.read_json(lines=True)` + `pandas.json_normalize` | Faster to write, easier to test, and standard for semi-structured JSON |
| Local training dataset cache format | One-off CSV dumps | `pyarrow` parquet datasets | Columnar storage is smaller, faster, and easier to replay |
| Model persistence | Custom binary/JSON serialization | `joblib.dump` / `joblib.load` | Already matches runtime expectations for sklearn artifacts |

**Key insight:** The hard part in this phase is not “train a model”; it is producing a fixed, replayable, leakage-safe tabular dataset from nested event logs. Use proven data/table/splitting primitives and reserve custom code for repo-specific contract validation and feature aggregation.

## Common Pitfalls

### Pitfall 1: False Cross-Source Alignment
**What goes wrong:** Bookmap and cTrader rows get merged because timestamps overlap loosely, even though they represent different instruments and market contexts.

**Why it happens:** The filenames all live under `logs/`, which makes them look like one training corpus when they are actually separate dataset scopes.

**How to avoid:** Require join preconditions:
- same canonical instrument
- overlapping time range
- declared anchor series
- explicit tolerance and direction

**Warning signs:** Training rows have mixed `instrument` values, missing enrichment fields after join, or sudden row-count collapse.

### Pitfall 2: Future Leakage Through Joins or Labels
**What goes wrong:** Feature rows accidentally include future Wyckoff state, future volume profile, or label data from the evaluation window.

**Why it happens:** Raw feeds are asynchronous and nested. Naive joins or rolling windows can silently pull future observations.

**How to avoid:** Sort by timestamp, use backward `merge_asof`, write one label builder with strict horizon rules, and keep split logic after feature/label construction but before any scaling/tuning.

**Warning signs:** Unrealistically strong validation metrics on tiny datasets, especially when row counts are low.

### Pitfall 3: Variable-Length `levels` Arrays Become Unstable Features
**What goes wrong:** Order-flow and volume-profile `levels` arrays are passed through as raw lists or expanded into inconsistent columns.

**Why it happens:** The source logs are rich but sparse and not shape-stable.

**How to avoid:** Aggregate `levels` into a fixed feature schema first, such as:
- top-N volume concentration
- delta skew
- distance of POC/VAH/VAL to close in ticks
- count of positive vs negative delta levels

**Warning signs:** Training code depends on Python objects instead of numeric columns, or feature columns differ across runs.

### Pitfall 4: Training Overwrites Live Runtime Artifact
**What goes wrong:** An experimental run replaces `runtime/model.pkl` and silently changes live inference behavior.

**Why it happens:** The current code path already loads/saves `runtime/model.pkl`, so it is tempting to reuse it directly.

**How to avoid:** Only `promote_model.py` should touch `runtime/model.pkl`. Training writes to versioned artifact directories only.

**Warning signs:** Runtime model timestamp changes after offline experimentation, or analyzer behavior changes without promotion metadata.

### Pitfall 5: Treating Current Sample Size as Production-Ready
**What goes wrong:** The team over-interprets metrics from 32 H1 order-flow rows, 21 H1 volume-profile rows, and 1416 Wyckoff rows.

**Why it happens:** The phase goal is “usable artifact,” but the currently available data is enough for pipeline implementation and smoke evaluation, not reliable alpha claims.

**How to avoid:** Plan acceptance around reproducibility, explicit labels, and stable evaluation output first. Treat performance thresholds as provisional until longer same-scope captures exist.

**Warning signs:** High variance across folds, class collapse, or metrics changing materially from tiny label-threshold changes.

### Pitfall 6: Trusting Outdated Runtime Docs
**What goes wrong:** Implementation plans target CSV runtime files described in `docs/pipeline-runtime-operations.md` instead of the JSONL/log layout enforced by code and tests.

**Why it happens:** The docs lag the Phase 2 code changes.

**How to avoid:** Treat `main/runtime_paths.py` and `tests/test_runtime_artifacts.py` as the source of truth until docs are corrected.

**Warning signs:** Plans mention `runtime/history_*.csv` or write derived training artifacts into old CSV paths.

## Code Examples

Verified patterns from official sources:

### Read JSONL In Chunks
```python
# Sources:
# - https://pandas.pydata.org/docs/reference/api/pandas.read_json.html
frames = []
for chunk in pd.read_json(path, lines=True, chunksize=5000):
    frames.append(chunk)
df = pd.concat(frames, ignore_index=True)
```

### Flatten Nested JSON And Preserve Fixed Columns
```python
# Source: https://pandas.pydata.org/docs/reference/api/pandas.json_normalize.html
records = [json.loads(line) for line in path.read_text(encoding="utf-8").splitlines()]
flat = pd.json_normalize(records)
```

### Backward As-Of Join For Time-Ordered Feeds
```python
# Source: https://pandas.pydata.org/docs/reference/api/pandas.merge_asof.html
joined = pd.merge_asof(
    left=anchor.sort_values("timestamp"),
    right=enrichment.sort_values("timestamp"),
    on="timestamp",
    by="instrument",
    direction="backward",
    tolerance=pd.Timedelta("2h"),
)
```

### Leakage-Safe Train/Eval With A Persisted Pipeline
```python
# Sources:
# - https://scikit-learn.org/stable/modules/generated/sklearn.pipeline.Pipeline.html
# - https://scikit-learn.org/stable/modules/generated/sklearn.model_selection.TimeSeriesSplit.html
# - https://scikit-learn.org/stable/modules/generated/sklearn.ensemble.HistGradientBoostingClassifier.html
# - https://scikit-learn.org/stable/model_persistence.html
from sklearn.ensemble import HistGradientBoostingClassifier
from sklearn.model_selection import TimeSeriesSplit
from sklearn.pipeline import Pipeline
import joblib

pipeline = Pipeline(
    [
        ("model", HistGradientBoostingClassifier(random_state=42))
    ]
)

splitter = TimeSeriesSplit(n_splits=5, gap=1)

for train_idx, test_idx in splitter.split(X):
    pipeline.fit(X.iloc[train_idx], y.iloc[train_idx])
    proba = pipeline.predict_proba(X.iloc[test_idx])

joblib.dump(pipeline, artifact_dir / "model.pkl")
```

### Write A Reusable Parquet Dataset
```python
# Source: https://arrow.apache.org/docs/python/parquet.html
import pyarrow as pa
import pyarrow.parquet as pq

table = pa.Table.from_pandas(feature_df)
pq.write_table(table, artifact_dir / "features.parquet")
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Startup-time dummy training inside `AIAnalyzer` | Offline train/eval pipeline with explicit artifact promotion | Phase 6 target | Reproducible experiments without changing live startup behavior |
| Direct runtime writes implied by old docs | `logs/*.jsonl` raw histories + `runtime/model.pkl`/`runtime/status.json` in code/tests | Phase 2 completed 2026-03-23 | Training should ingest JSONL histories, not outdated CSV paths |
| Treat all feeds as one pool | Dataset scopes defined by source + instrument + anchor + label spec | Phase 6 planning requirement | Prevents invalid cross-source joins and lets multiple models coexist safely |

**Deprecated/outdated:**
- `docs/pipeline-runtime-operations.md` runtime CSV descriptions: outdated relative to `main/runtime_paths.py` and `tests/test_runtime_artifacts.py`
- “Train from runtime buffer and save immediately”: outdated for any repeatable ML workflow in this repo

## Open Questions

1. **What official requirement IDs should Phase 6 own?**
   - What we know: `ROADMAP.md` says Phase 6 requirements are TBD.
   - What's unclear: Traceability IDs and acceptance language are missing.
   - Recommendation: Add official Phase 6 IDs before execution starts; use the candidate IDs above as a starting point.

2. **Is the first promoted model allowed to be cTrader-only?**
   - What we know: Current logs only support a valid multi-feed scope for `XAUUSD` on cTrader.
   - What's unclear: Whether the milestone expects one unified cross-source model or any usable local model artifact.
   - Recommendation: Lock the first promoted artifact to `ctrader_xauusd_h1_baseline`. Keep Bookmap as separate scope work unless matching captures are added.

3. **What exact label horizon and thresholds should be locked for the baseline?**
   - What we know: There is no current supervised trade-history target.
   - What's unclear: Required horizon and threshold policy for `long/flat/short`.
   - Recommendation: Freeze one simple horizon-based label spec in planning. Do not explore multiple competing target formulations in the same first implementation wave.

4. **Do we need Bookmap price alignment before Bookmap model training?**
   - What we know: Current Bookmap histories contain event payloads and prices encoded as strings, but no aligned cTrader-compatible anchor series.
   - What's unclear: Whether a Bookmap-only event model is useful enough for this phase.
   - Recommendation: Treat Bookmap logs as separate contract and featurization scope for now, not as part of the first promoted trading model.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | `pytest` 9.0.2 |
| Config file | `pytest.ini` |
| Quick run command | `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests/test_training_dataset_contract.py -q` |
| Full suite command | `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests -q` |

**Current verification note:** Test discovery was confirmed with `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests --collect-only -q`. Existing collected tests cover runtime artifacts, socket lifecycle, pipeline status, and alert-listener conversion, but nothing yet for Phase 6 training behavior.

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| MLDATA-01 | Normalize raw JSONL into fixed-schema dataset tables | unit | `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests/test_training_dataset_contract.py -q` | ❌ Wave 0 |
| MLDATA-02 | Reject invalid same-run cross-source joins and enforce dataset scopes | unit | `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests/test_training_join_policy.py -q` | ❌ Wave 0 |
| MLTRAIN-01 | Train run writes model, metrics, manifests, and feature schema deterministically | integration | `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests/test_training_pipeline.py -q` | ❌ Wave 0 |
| MLTRAIN-02 | Promotion copies only blessed artifact into `runtime/model.pkl` | integration | `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests/test_model_promotion.py -q` | ❌ Wave 0 |
| MLEVAL-01 | Label generation and walk-forward splits are deterministic and leakage-safe | unit | `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests/test_training_labels.py -q` | ❌ Wave 0 |

### Sampling Rate
- **Per task commit:** targeted phase-6 pytest file for the touched area
- **Per wave merge:** `& "$env:LOCALAPPDATA\Programs\Python\Python312\python.exe" -m pytest tests -q`
- **Phase gate:** Full suite green plus one reproducible train run producing the same manifest/config/schema outputs on the same inputs

### Wave 0 Gaps
- [ ] `tests/test_training_dataset_contract.py` — validates schema normalization and per-scope row contracts
- [ ] `tests/test_training_join_policy.py` — validates same-instrument and tolerance-based join rules
- [ ] `tests/test_training_labels.py` — validates horizon labels and no-future-leakage behavior
- [ ] `tests/test_training_pipeline.py` — validates artifact directory layout and metrics output
- [ ] `tests/test_model_promotion.py` — validates explicit promotion into `runtime/model.pkl`
- [ ] `tests/fixtures/phase6/` — small frozen JSONL fixtures for one cTrader scope and one Bookmap scope

## Sources

### Primary (HIGH confidence)
- Local repo files: `main/ai_analyzer.py`, `main/runtime_paths.py`, `main/order_simulator.py`, `docs/event-contract-v1.md`, `tests/test_runtime_artifacts.py`, `tests/test_ai_analyzer_exports.py`
- scikit-learn Pipeline docs: https://scikit-learn.org/stable/modules/generated/sklearn.pipeline.Pipeline.html
- scikit-learn TimeSeriesSplit docs: https://scikit-learn.org/stable/modules/generated/sklearn.model_selection.TimeSeriesSplit.html
- scikit-learn HistGradientBoostingClassifier docs: https://scikit-learn.org/stable/modules/generated/sklearn.ensemble.HistGradientBoostingClassifier.html
- scikit-learn model persistence docs: https://scikit-learn.org/stable/model_persistence.html
- pandas `read_json` docs: https://pandas.pydata.org/docs/reference/api/pandas.read_json.html
- pandas `json_normalize` docs: https://pandas.pydata.org/docs/reference/api/pandas.json_normalize.html
- pandas `merge_asof` docs: https://pandas.pydata.org/docs/reference/api/pandas.merge_asof.html
- Apache Arrow parquet docs: https://arrow.apache.org/docs/python/parquet.html

### Secondary (MEDIUM confidence)
- PyPI pandas project page/search result: https://pypi.org/project/pandas/
- PyPI scikit-learn project page/search result: https://pypi.org/project/scikit-learn/
- PyPI numpy project page/search result: https://pypi.org/project/numpy/
- PyPI joblib project page/search result: https://pypi.org/project/joblib/
- PyPI pyarrow project page/search result: https://pypi.org/project/pyarrow/
- PyPI pytest project page/search result: https://pypi.org/project/pytest/

### Tertiary (LOW confidence)
- None

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - aligned with current repo dependencies and verified against official docs/PyPI pages
- Architecture: MEDIUM - contract and promotion design are well supported, but label scope still needs a planning decision
- Pitfalls: HIGH - driven directly by observed repo schema mismatch, instrument mismatch, and outdated runtime docs

**Research date:** 2026-03-24
**Valid until:** 2026-04-23
