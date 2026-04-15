# Unit Test Inventory

Every unit test in `{TestProject}` is organized on two axes — **target class × Happy / Boundary / Error** — in this table. When adding or modifying tests, update this table **first**, then write or edit the code.

- **Governing skill**: `managing-unit-tests` (reads this file, adds/updates rows, generates/edits tests, and verifies inventory ↔ code consistency).
- **Reference template class**: `{plugin}/skills/managing-unit-tests/examples/SampleServiceTests.cs`.

## Classification Rules

| Group | Definition | Example |
|-------|------------|---------|
| **Happy** | Valid input / normal state → expected success result | `Activate_ValidCode_Succeeds` |
| **Boundary** | Edge conditions (empty input, zero, min/max, inactive state, null-safe paths) | `CleanupOldFiles_EmptyDirectory_DoesNotThrow` |
| **Error** | Invalid input / exceptions / failure results / detection events | `Activate_InvalidFormat_ReturnsError` |

For validators and detectors, the "successful detection" case is classified as **Error** (surfacing a failure state *is* the error path of that logic).

## Test Inventory

<!--
  Duplicate the section below per target class.
  Remove groups that do not apply — but document the rationale in the table.
-->

### `{Project}/{Namespace}/{ClassName}`

| # | Method / Scenario | Group | Test |
|---|-------------------|-------|------|
| 1 | {baseline happy case} | Happy | `{Method}_{Scenario}_{Expected}` |
| 2 | {empty / boundary}    | Boundary | `{Method}_{Scenario}_{Expected}` |
| 3 | {exception / failure} | Error | `{Method}_{Scenario}_{Expected}` |

<!-- Example — delete once real entries exist:

### `SampleProject/Services/OrderService`

| # | Method / Scenario | Group | Test |
|---|-------------------|-------|------|
| 1 | Valid order placement | Happy | `PlaceOrder_ValidOrder_ReturnsSuccess` |
| 2 | Zero quantity rejected | Boundary | `PlaceOrder_ZeroQuantity_ReturnsValidationError` |
| 3 | Null order throws | Error | `PlaceOrder_NullOrder_ThrowsArgumentNullException` |

-->

## Coverage Gap Analysis

Areas currently **uncovered or shallow**. Pick from this list when adding new tests. Record priority and rationale.

### 🔴 High — Business critical / security / data integrity

| Target | Current state | Tests needed |
|--------|---------------|--------------|
| `{Project}/{Namespace}/{ClassName}` | No tests | {describe} |

### 🟡 Medium — Infrastructure / configuration / tooling

| Target | Current state | Tests needed |
|--------|---------------|--------------|
| `{Project}/{Namespace}/{ClassName}` | Indirect coverage only | {describe} |

### 🟢 Low — Helper utilities

| Target | Current state | Tests needed |
|--------|---------------|--------------|
| `{Project}/{Namespace}/{ClassName}` | No tests | {describe} |

### ℹ️ Intentionally uncovered (not in scope)

- {e.g. WPF UserControl / Window classes — covered by UI automation tests}
- {e.g. DI container registration — integration-scope only}
- {e.g. P/Invoke wrappers — covered indirectly through higher-level features}

## Statistics

| Metric | Value |
|--------|-------|
| Test files | 0 |
| Total executed tests | **0** |
| High-priority gaps | 0 areas |
| Medium-priority gaps | 0 areas |

> **Note**: Gap-analysis counts are based on static review. Measure actual line coverage separately with `dotnet test --collect:"XPlat Code Coverage"` and record the latest numbers here when a coverage tool is introduced.
