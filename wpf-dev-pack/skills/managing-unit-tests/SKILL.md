---
description: >
  Inventory-driven unit test governance workflow for any .NET test project.
  First updates a test inventory markdown table (single source of truth),
  then generates or modifies tests following Happy/Boundary/Error grouping
  and the `Method_Scenario_Expected` naming convention, and finally verifies
  that the inventory table matches the actual test code. Framework-agnostic —
  works with xUnit, NUnit, or MSTest, and with any mocking library.

  Use When:
    - User asks to add, remove, modify, or refactor unit tests
      (e.g. "add unit test", "테스트 추가", "organize tests", "test coverage review",
      "sync test inventory").
    - Any file matching `*Tests.cs` or `*Test.cs` is being created or edited
      in a dedicated test project.
    - User requests a coverage gap review against an existing test inventory.
    - User asks to check whether inventory documentation and test code are in sync.
    - User mentions Happy/Boundary/Error classification or wants to impose
      a consistent test governance policy across a codebase.

  Do NOT Use When:
    - The task is UI automation testing (FlaUI, Appium, Playwright) — use
      the matching UI testing skill instead.
    - The change is production code only and has no test impact.
    - The user is asking for integration or end-to-end tests that span
      multiple services with a real DI container — escalate and discuss
      scope first; this skill is for isolated unit tests only.
    - The codebase has no test project yet — first scaffold one via
      `testing-wpf-viewmodels` or an equivalent setup skill.
user-invocable: false
model: sonnet
---

# Managing Unit Tests

Workflow for managing every unit test in a test project through a single inventory table as the source of truth. Guarantees that documentation and test code never drift apart.

This skill governs **how tests are classified, named, and tracked**. For concrete pattern examples (PropertyChanged, Commands, async flows) see the [`testing-wpf-viewmodels`](../testing-wpf-viewmodels/SKILL.md) skill.

## Core Principles

1. **Inventory first, code second.** A test that is not listed in the table must not exist.
2. **Never "code first, inventory later."** If you discover an existing test, reflect it in the table before doing anything else.
3. **Happy / Boundary / Error Path** classification is mandatory. If a group is missing, leave an explicit rationale in the table.
4. Method naming: `Method_Scenario_Expected`.
5. `DisplayName` must be a clear declarative sentence. Language is a project-level choice — just stay consistent.
6. Mocking libraries (Moq / NSubstitute / FakeItEasy) are only for **external dependencies**. Use real instances or fakes for internal domain objects.
7. Forbidden tests: POCO default-value checks, inheritance-relation assertions, hard-coded collection count checks.
8. Do not test private methods directly. If a branch is unreachable through the public contract, treat it as a design defect.

## Reference Documents

| Document | Purpose |
|----------|---------|
| [`references/test-inventory-template.md`](references/test-inventory-template.md) | Inventory table template. Copy it when initializing a new test project. |
| [`examples/SampleServiceTests.cs`](examples/SampleServiceTests.cs) | Reference test class template showing the required structure. |
| [`../testing-wpf-viewmodels/SKILL.md`](../testing-wpf-viewmodels/SKILL.md) | ViewModel-specific test patterns (PropertyChanged, Commands, service mocking). |

## Test Classification

| Group | Definition | Example |
|-------|------------|---------|
| **Happy** | Valid input / normal state → expected success result | `Activate_ValidCode_Succeeds` |
| **Boundary** | Edge cases (empty input, zero, min/max, inactive state, null-safe paths) | `CleanupOldFiles_EmptyDirectory_DoesNotThrow` |
| **Error** | Invalid input / exceptions / failure results / detection events | `Activate_InvalidFormat_ReturnsError` |

For **validators / detectors**, the "successful detection" case is classified as **Error** — surfacing a failure state *is* the error path of such logic.

## Workflow

### 1. Interpret the Request → Identify the Scenario

Classify the user's request into one of the following:

| Type | Example | Steps |
|------|---------|-------|
| **Add new tests** | "Add tests for OrderService" | Steps 2–6 |
| **Modify existing tests** | "Strengthen Activate error cases" | Steps 3, 5, 6 |
| **Remove tests** | "Delete the LogLevels count test" | Steps 3, 5, 6 |
| **Coverage audit** | "Check current test gaps" | Step 2 + gap-analysis section only |
| **Inventory sync** | "Verify inventory matches code" | Step 6 only |

### 2. Read the Inventory & Identify Targets

Read `{TestProject}/.claude/test-inventory.md` and determine:

- If the inventory file does not exist yet, copy [`references/test-inventory-template.md`](references/test-inventory-template.md) to create it.
- Is the target class/method already in the table?
- Which group (Happy / Boundary / Error) does the requested scenario belong to?
- Is it mentioned in the 🔴 High / 🟡 Medium / 🟢 Low **coverage gap** sections?

**If absent**: prepare to add a new section. **If present**: decide which row to modify or extend.

### 3. Update the Inventory Table (Before Writing Code)

For a new target, add a section with the following template:

```markdown
### `{Project}/{Namespace}/{ClassName}`

| # | Method / Scenario | Group | Test |
|---|-------------------|-------|------|
| 1 | {baseline happy case} | Happy | `{Method}_{Scenario}_{Expected}` |
| 2 | {empty / boundary}   | Boundary | `{Method}_{Scenario}_{Expected}` |
| 3 | {exception / failure}| Error | `{Method}_{Scenario}_{Expected}` |
```

If the entry was in the coverage-gap section, **remove it from the gap list** (the gap has been filled).

**Update the statistics**: adjust the "total executed tests" count and gap counts at the bottom of the inventory.

### 4. Generate / Modify the Test Code

Create or modify `{TestProject}/{ClassName}Tests.cs`. Follow the structural template in [`examples/SampleServiceTests.cs`](examples/SampleServiceTests.cs).

Structural requirements:

- `sealed class {ClassName}Tests` — no inheritance extension intended.
- Implement `IDisposable` **only** when the test holds disposable resources (temp files, streams); clean up in `Dispose`.
- Use `#region Happy Path / Boundary / Error Path / Helpers`. If a category has no cases, omit the region entirely — never leave an empty region.
- Method naming: `{Method}_{Scenario}_{Expected}`.
- `DisplayName`: clear declarative sentence, consistent with the project's language policy.

> For concrete ViewModel test patterns (PropertyChanged, Commands, async), see [`testing-wpf-viewmodels`](../testing-wpf-viewmodels/SKILL.md).

### 5. Build & Run Tests

```sh
dotnet build {TestProject}/{TestProject}.csproj -c Debug
dotnet test {TestProject}/{TestProject}.csproj --no-build --filter "FullyQualifiedName~{ClassName}Tests"
```

- Confirm zero build errors.
- Confirm that the new tests and all existing tests pass.

### 6. Inventory ↔ Test Consistency Check (Mandatory Closing Step)

Run **every** check below and report the results to the user:

#### 6a. Table entries ↔ actual test methods

```sh
# Extract every test-method name referenced in the inventory
grep -oE '`\w+_\w+_\w+`' {TestProject}/.claude/test-inventory.md | sort -u

# Extract every test-method name from the code
grep -rE 'public\s+(void|async\s+Task)\s+\w+_\w+_\w+' {TestProject} \
  | grep -oE '\b\w+_\w+_\w+\b(?=\()' | sort -u
```

The symmetric difference must be empty. If it is not, decide which side is wrong and fix it.

#### 6b. Missing Happy / Boundary / Error groups

For every target class, verify all three groups exist in the inventory. If one is missing:
- Determine whether it is truly impossible for that public method.
- If impossible, mark the row with `—` and a comment explaining why.
- If possible but missing, add the test (repeat Steps 3–5).

#### 6c. Statistics consistency

- Test execution count = count reported by `dotnet test --no-build`.
- Gap count = actual number of gap-section rows.

#### 6d. Naming violations

```sh
# Flag test methods with fewer than two underscores
grep -rE 'public\s+(void|async\s+Task)\s+\w+\b' {TestProject} \
  | grep -vE '\b\w+_\w+_\w+\b'
```

Rename violators.

#### 6e. DisplayName presence

```sh
# Flag [Fact] / [Theory] attributes missing DisplayName
grep -rE '\[Fact\]|\[Theory\]' {TestProject}
```

Enforce DisplayName consistently if the project mandates it.

### 7. Report to the User

Report in the following format:

```
**Inventory update**
- Added / modified entries: ...
- Removed entries: ...

**Test code changes**
- Added: N (Happy / Boundary / Error breakdown)
- Modified: N
- Removed: N

**Verification results**
- Build: 0 errors
- Tests: N/N passing
- Inventory ↔ code consistency: OK / N mismatches
- Naming convention: OK / N violations
- Missing Happy/Boundary/Error: OK / N targets
```

## Common Pitfalls

| Pitfall | Mitigation |
|---------|-----------|
| Added tests but forgot to update the table | Always execute Step 3 before Step 4. Step 6a catches this before commit. |
| Added table entry but forgot to write the test | Step 6a flags "listed but missing in code". |
| Only wrote the Happy case | Step 6b catches missing groups. Either provide all three or mark `—` with rationale. |
| Testing a POCO's default values | Forbidden. Exception: when the default value is part of the business contract — document the rationale inline. |
| Inheritance assertions like `Assert.IsType<Base>(exactMatch: false)` | Tautological — the compiler already guarantees this. |
| Hard-coded collection counts (`Assert.Equal(N, items.Count)`) | Assert on the meaningful shape (items, ordering) instead. |
| Mocking internal domain objects | Use real instances or fakes. Reserve mocking libraries for external dependencies. |
| Missing file-I/O isolation | Use `Path.GetTempPath()` + `Guid.NewGuid():N` directories and clean up in `Dispose`. |
| Direct `DateTime.Now` reference in time-sensitive tests | Use relative time (`AddDays(-N)`) or a time abstraction. |
| `async void` test methods | Always write `async Task`. Wait on external signals with `Task.WhenAny(target, Task.Delay(timeout))` to avoid hangs. |

## When NOT to Use

- **UI automation tests** (FlaUI, Appium, Playwright) — use the dedicated UI testing skill. This skill is for unit tests only.
- **Integration tests** (real DI container, real DB / filesystem interaction) — scope separately.
- **Private-method tests** — reach them through the public contract. Otherwise suspect a design defect.
- **Test project not yet scaffolded** — set one up first (see `testing-wpf-viewmodels`) before applying this skill.
