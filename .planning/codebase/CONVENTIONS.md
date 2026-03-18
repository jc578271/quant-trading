# Coding Conventions

**Analysis Date:** 2026-03-18

## Naming Patterns

**Files:**
- Match the subsystem you are editing rather than forcing one repo-wide style
- cTrader files preserve upstream descriptive names with spaces and version numbers, e.g. `Free Volume Profile v2.0.cs`
- cTrader helper files use suffix-based decomposition such as `.NodesAnalizer.cs`, `.ParamsPanel.cs`, `.Styles.cs`
- Java uses standard PascalCase class filenames and package directories
- Python uses `snake_case.py`

**Functions / Methods:**
- C# uses PascalCase for most methods (`ClearAndRecalculate`, `VolumeInitialize`, `CreateOrReset_cTraderIndicators`)
- Java uses camelCase for methods (`reconnectSocket`, `startMonitoring`, `checkDataTimeout`)
- Python uses snake_case (`process_data`, `place_order_with_risk`)

**Variables / Types:**
- Java constants use `UPPER_SNAKE_CASE`
- C# mixes PascalCase, camelCase, and underscore-heavy legacy names inside giant indicator files; preserve local style within each file
- Parameter/info holder types in cTrader often end with `_Info` or `Params`

## Code Style

**Formatting:**
- No repo-wide formatter config was found
- C# and Java use semicolons and long multi-line files; line length is not tightly constrained
- Python uses straightforward standard-library style with logging strings and inline comments
- Existing source includes some mojibake/non-ASCII corruption in comments and logs; avoid spreading that pattern in new edits

**Linting:**
- No ESLint, Roslyn analyzer ruleset, or Python lint config was found at repo root
- Style enforcement appears manual and subsystem-specific

## Import Organization

**Observed order:**
- C# starts with `using` blocks, then attributes, then classes
- Java groups platform imports first, then JDK imports
- Python keeps standard library imports before third-party packages in most files

**Grouping:**
- Existing files are not consistently auto-sorted
- Minimize import churn in legacy files unless you are already touching the relevant block

## Error Handling

**Patterns:**
- Python favors boundary-level `try/except` with logging and continued processing (`main/socket_server.py`, `main/ai_analyzer.py`)
- Java addons often log to console/UI and keep the addon alive rather than throwing upward
- cTrader indicator files rely heavily on defensive comments, guard flags, and manual reset flows instead of a shared error abstraction

**Guidance for new work:**
- Match the local subsystem style
- Prefer logging context at the runtime boundary instead of silently swallowing failures
- Avoid introducing a new error-handling framework unless you are refactoring a subsystem intentionally

## Logging

**Framework:**
- Python uses `logging`
- Java uses `System.out.println`, `System.err.println`, Swing status labels, and CSV logging
- cTrader code appears to rely more on chart/UI behavior and inline comments than structured logging

**Patterns:**
- Runtime state transitions and integration failures are usually logged
- Debug-style prints still exist in production paths, especially in `main/ai_analyzer.py` and `SimpleTelegramNotifier.java`

## Comments

**When to Comment:**
- cTrader sources contain heavy explanatory comments documenting platform quirks, performance workarounds, and upstream attribution
- Java and Python files use fewer comments and lean more on descriptive method names
- When touching the large cTrader indicators, explain platform-specific "why" rather than restating the code

**TODO / Notes:**
- No strong TODO format is established
- Historical attribution and migration notes are common in cTrader files; preserve them when editing adjacent logic

## Function and Module Design

**Observed patterns:**
- cTrader modules are still large and partially decomposed; helpers are extracted by concern, not by strict domain layering
- Java classes are centered around one addon per file with inner helper types
- Python modules are small and direct, with one main class per file and minimal abstraction

**Practical guidance:**
- Extend the existing subsystem pattern first
- For cTrader code, prefer adding or updating an adjacent helper file before growing the primary indicator file even more
- For Bookmap/Python, keep one responsibility per class/module and avoid hidden cross-file coupling

---

*Convention analysis: 2026-03-18*
*Update when patterns change*
