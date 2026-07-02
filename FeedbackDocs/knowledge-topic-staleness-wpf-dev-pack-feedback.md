# wpf-dev-pack Feedback — Knowledge-topic content drift (stale/incorrect API guidance across several topics)

- **Purpose**: A documentation-focused WPF session cross-checked several existing knowledge topics against the actually-published NuGet packages/assemblies (using assembly/resource inspection and current official docs) instead of trusting the prose as written. Seven concrete inaccuracies and one systemic pattern were found across topics touching WPF-UI, Prism 9 DI, async command error handling, unit testing, and publishing. All were corrected directly in the local knowledge clone during the session; this document exists so the same corrections can be verified and merged upstream.
- **Scope**: 7 knowledge-topic content corrections (no new skills), 1 general process recommendation. No scaffolder changes. No version bump needed (knowledge-only edits, per repo convention).

---

## 0. Summary (priority)

| ID | Kind | Target | Priority | One-liner |
|----|------|--------|----------|-----------|
| 1 | Augment (knowledge) | `knowledge/integrating-wpfui-fluent/TOPIC.md` | High | App.xaml sample merges a resource via a pack-URI path that no longer exists in the current package |
| 2 | Augment (knowledge) | `knowledge/configuring-dependency-injection/PRISM.md` | High | Falsely states the Prism DI container has no Scoped lifetime support |
| 3 | Augment (knowledge) | `knowledge/preventing-dispatcher-deadlock/TOPIC.md` | High | References a container-wide exception-handler registration API that does not exist |
| 4 | Augment (knowledge) | `knowledge/testing-wpf-viewmodels/TOPIC.md` | Medium | Assertion-library version pin has no explanation, hiding a license-tier trap one major up |
| 5 | Augment (knowledge) | `knowledge/testing-wpf-viewmodels/TOPIC.md` | Low-Medium | Only documents the legacy test-runner combo, omits the modern alternative |
| 6 | Augment (knowledge) | `knowledge/publishing-wpf-apps/INSTALLERS.md` | Medium | Installer/updater package pin resolves only to a stale pre-release; a documented CLI flag no longer matches current syntax |
| 7 | Augment (knowledge) | `knowledge/implementing-communitytoolkit-mvvm/TOPIC.md` | Low | Minimum-supported-framework claim is inaccurate (omits a broader-compatibility asset the package actually ships) |
| 8 | Process recommendation | (repo-wide, `knowledge/**`) | Medium | Static version pins in knowledge topics drift silently from the actual latest release over time |

---

## 1. WPF-UI resource-merge sample uses a pack-URI that no longer resolves

### Phenomenon and causality
**Phenomenon**: Following the documented `App.xaml` sample — merging a themed control-style resource dictionary via a fixed `pack://application:,,,/<Library>;component/<Path>/Controls.xaml`-style URI — throws a `XamlParseException` (inner `IOException`, "Cannot locate resource") at application startup.

**Cause**: WPF-UI (`Wpf.Ui`) restructured its bundled resources in a later release: the aggregate "controls" resource dictionary and the theme dictionaries moved to different internal paths, and the library now ships dedicated `MarkupExtension`-based helpers (`ThemesDictionary` taking a `Theme` parameter, and `ControlsDictionary`) specifically so consumers don't need to hardcode the internal resource path. The old flat `Source="pack://.../Styles/Controls.xaml"` path is a relic of an earlier package layout and is absent from the current package's embedded resource manifest (verifiable by loading the assembly and enumerating `IResourceReader` entries from its `*.g.resources` stream).

**Effect**: Any project generated from this sample crashes on first run with no useful guidance toward the real fix, because the exception surfaces as a generic resource-location failure rather than pointing at an API/version mismatch.

### Proposal (concrete change)
In the `App.xaml` section, replace the raw `ResourceDictionary Source="pack://.../Styles/Controls.xaml"` merge with the library's own `ThemesDictionary`/`ControlsDictionary` markup extensions (requiring the library's XAML namespace), and add a short inline note that the extensions resolve the real internal resource paths so the sample doesn't regress the same way again if internal paths move once more. Loosen the pinned package version pattern from an exact two-segment pin to a floating major-version wildcard, since the resource-merge API shape has been stable across the whole major version line — an exact pin is what let the sample go stale silently.

### Adjacent skill boundaries / cross-links
None — this is isolated to the Fluent-UI integration topic. Worth a one-line cross-reference from any topic that scaffolds a new WPF-UI-based shell, noting "verify resource-merge API against the installed package version, not from memory."

---

## 2. Prism DI container's Scoped-lifetime support is documented as absent when it exists

### Phenomenon and causality
**Phenomenon**: A comparison table and accompanying warning text state that the Prism-based DI container does not support a "Scoped" registration lifetime, framing it as web-only and unnecessary in desktop apps.

**Cause**: The claim is simply incorrect for the current major version of the Prism DI abstraction and its default container-extension implementation. The registration interface exposes a `RegisterScoped(...)` family of overloads, and the concrete container extension implements scope creation (a `CreateScope()`-style method plus a "current scope" accessor). This was verified by inspecting the real compiled registration-interface and container-extension assemblies rather than relying on prior documentation or memory.

**Effect**: A reader designing a per-operation "unit of work" lifetime (e.g., wrapping a multi-step business transaction) would wrongly rule out an available, working DI feature and either reach for a heavier custom solution or bolt on manual instance-per-call management, when the framework already provides the primitive.

### Proposal (concrete change)
Correct the warning callout and the lifetime-comparison table row: state that Scoped lifetime **is** supported via `RegisterScoped(...)` plus explicit scope creation, note it is still rarely needed in a typical desktop app (most objects are effectively singleton- or per-view-model-scoped already), but no longer claim it is unsupported. Update the "Key Differences" summary list at the bottom of the same document to match.

### Adjacent skill boundaries / cross-links
Cross-link from any topic discussing "unit of work" / transactional service patterns in a DI-heavy desktop app, since this corrects a capability readers may have assumed was unavailable.

---

## 3. Async command error-handling sample references a non-existent global exception-handler registration API

### Phenomenon and causality
**Phenomenon**: The async-command section of a dispatcher-deadlock-prevention topic states that a framework-provided async command type "routes exceptions to the registered error handler," naming a specific method on the DI container-registration interface as the registration point for that handler.

**Cause**: No such method exists on the container-registration interface in the current major version of the framework (confirmed by a full member enumeration of the compiled registration-interface assembly and a targeted name-pattern search for anything resembling a global/registry-wide exception hook — zero matches). The framework's actual mechanism for this async command type is a fluent, per-command exception callback attached directly to the command instance at construction time, not a container-wide registration.

**Effect**: A reader trying to wire centralized async-command error handling would search for and fail to find the named API, then either give up on centralized handling entirely or spend time on a fruitless framework-internals search — exactly the kind of dead end this knowledge base exists to prevent.

### Proposal (concrete change)
Replace the reference to the non-existent container-registry method with the real fluent per-command exception-handling method, shown chained directly onto the command's construction (`new AsyncDelegateCommand(...).Catch(ex => ...)`-shaped), and note explicitly that there is no container-wide hook in the current major version — so readers don't go looking for one.

### Adjacent skill boundaries / cross-links
This topic already cross-links to a "graceful shutdown" topic and a "dispatcher scheduling" topic; no change needed there, just the local correction.

---

## 4. Assertion-library version pin lacks the license-tier rationale that makes the pin correct

### Phenomenon and causality
**Phenomenon**: FluentAssertions is pinned to its last Apache-2.0-licensed major version (7.x) with no comment explaining why — while version 8.0 onward requires a paid Xceed Commercial License for organizational/commercial use (its Community tier covers non-commercial use only).

**Cause**: The library's licensing model changed at the 8.0 boundary. The pinned 7.x version predates that boundary and is correctly chosen for unrestricted commercial use, but the document gives no indication that floating the wildcard past that boundary changes the licensing terms.

**Effect**: A reader who "helpfully" bumps the floating version pin to chase the latest release — a reasonable, encouraged habit in general — would silently take on a commercial licensing obligation without any signal in the document that this happened. This is a trap precisely because the correct pin looks, on its face, like ordinary staleness.

### Proposal (concrete change)
Add an inline comment next to the pin stating plainly that it is intentional (not stale), naming the licensing boundary at the next major version and the specific consequence (paid commercial license required past that point for commercial/organizational use), so a reader cannot mistake the old pin for an oversight.

### Adjacent skill boundaries / cross-links
Same pattern already exists and is correctly handled elsewhere in this knowledge base for a different dual-licensed MVVM/modularity framework — worth using that existing callout's wording style as a template for consistency.

---

## 5. Unit-testing topic documents only the legacy test-runner stack, without surfacing the modern alternative

### Phenomenon and causality
**Phenomenon**: The NuGet package list for setting up ViewModel unit tests shows only the older VSTest-based `xunit`/`xunit.runner.visualstudio` v2 combination, with no mention that `xunit.v3` — built on Microsoft.Testing.Platform (MTP), the platform's modern native test-execution model — is now the recommended path for new projects on current SDK versions.

**Cause**: The legacy `xunit` v2 meta-package has had no new releases in well over a year (effectively frozen but still fully functional and supported by the current SDK's legacy execution mode), while `xunit.v3` and MTP have become the ecosystem's forward-looking recommendation, including in xunit's own official getting-started guidance.

**Effect**: Not a breakage — the documented legacy combination still builds and runs correctly. But a reader scaffolding a brand-new test project today, with no signal that a more current path exists, would default into the legacy generation purely because it's the only one shown, missing a deliberate choice they should be allowed to make.

### Proposal (concrete change)
Add a short callout above the package list noting that the shown combination is the legacy (still fully supported) path, and that new projects on a current SDK may prefer the newer test-framework major version paired with the platform's native test-execution mode, with a pointer to that ecosystem's own migration/getting-started documentation. Keep the existing sample as-is (it remains valid) rather than rewriting it.

### Adjacent skill boundaries / cross-links
A separate, framework-agnostic test-governance topic already exists and explicitly defers version/setup specifics to this topic — no change needed there.

---

## 6. Installer/updater package pin resolves to a stale pre-release; a documented CLI flag has drifted

### Phenomenon and causality
**Phenomenon**: (a) Velopack's NuGet package is pinned with a `0.*` floating pattern that, given the package's actual release history, can only ever resolve to its very last pre-1.0 release — even though the package has since been stable on a 1.x line for some time. (b) A documented `vpk pack ... --delta` CLI invocation passes the delta-update flag with no value, but the current `vpk` CLI requires that flag to take an explicit mode argument (e.g. `BestSpeed`).

**Cause**: Velopack crossed its 1.0 stability boundary after the pin and CLI example were written, and neither was revisited afterward. Velopack's own documentation explicitly recommends keeping the globally-installed `vpk` CLI version in step with the referenced package version for compatibility — but the CLI install instruction (`dotnet tool install -g vpk`) shown alongside the stale pin has no version pin at all, so it installs the current (1.x) CLI, creating a version mismatch against the documented `0.*` package pin if followed literally.

**Effect**: Following the sample as written would pull in an outdated pre-1.0 library alongside a mismatched current-generation CLI tool, and the delta-update command would not match the CLI's actual accepted syntax — a reader would need to work out the correct invocation and package version themselves before the deployment pipeline functions.

### Proposal (concrete change)
Update the package pin to a floating major-version pattern matching the current stable line, update the delta-update CLI example to pass an explicit mode argument, and add a one-line note next to the CLI install instruction that the CLI and package versions should be kept in step per the tool's own compatibility guidance.

### Adjacent skill boundaries / cross-links
None additional — self-contained within the installers reference document.

---

## 7. MVVM toolkit's minimum-supported-framework statement is inaccurate

### Phenomenon and causality
**Phenomenon**: The prerequisites section for a widely-used MVVM source-generator toolkit states a single minimum-supported-.NET-version requirement.

**Cause**: The package, as actually published, ships multiple target-framework assets — including `netstandard2.0`/`netstandard2.1` assets — meaning it is genuinely usable on materially older targets (including older .NET Framework and early .NET Core) than the stated minimum suggests. The stated minimum version is only actually required to obtain one specific asset carrying WinRT-specific functionality, not for baseline usage of the toolkit.

**Effect**: Low real-world impact for a project already targeting a current .NET version, but the claim is factually wrong and could misinform a reader evaluating whether the toolkit is viable for a lower-target-framework project.

### Proposal (concrete change)
Replace the single minimum-version bullet with an accurate description of the package's actual shipped target-framework assets, and clarify that the higher minimum applies only to the WinRT-specific asset, not to baseline usage.

### Adjacent skill boundaries / cross-links
None.

---

## 8. General pattern: static NuGet version pins in knowledge topics drift silently from the actual latest release

### Phenomenon and causality
**Phenomenon**: Across the topics reviewed in this session, several unrelated package version pins (a DI/hosting package pin one major generation behind the version aligned with a current target framework, an error-result-pattern library pin one minor generation behind current, a test-SDK package pin one major generation behind current) were each individually harmless — none were broken, just not the latest available — but collectively point at a systemic gap: nothing in the authoring/maintenance process re-checks a pin against the package registry's actual current state after the topic is written.

**Cause**: Version pins are prose, written once at authoring time, and (unlike compiled code) nothing fails loudly when they drift — a floating wildcard pin quietly keeps resolving to whatever exists within its pinned range, masking that a newer major/minor line has since become current. Nothing distinguishes, at a glance, a pin that is "current" from one that is merely "still technically valid."

**Effect**: Individually low-severity, but the same silent-drift mechanism is also what produced the higher-severity findings in this document (the pack-URI break and the incorrect capability claims did not originate from version drift, but a genuinely broken pin — like the installer-package one in item 6 — is indistinguishable from a merely-outdated one until someone manually re-verifies against the live registry). Left unaddressed, the corpus accumulates more of these over time as packages continue releasing.

### Proposal (concrete change)
Process recommendation rather than a single content edit: when a knowledge topic pins a specific package version (exact or floating), periodically re-verify the pin against live package-registry metadata (available versions, current major/minor line) rather than trusting the prose as authoritative — the same verification method used to find every item in this document (inspecting real published package metadata and, where a concrete API/resource claim is made, the actual compiled assembly or resource manifest, cross-checked against the library's current official documentation). Consider flagging, in the maintainer-facing contribution guidance, that a version pin without an inline rationale comment should be treated as "advisory, verify before trusting" rather than "authoritative."

### Adjacent skill boundaries / cross-links
Applies repo-wide to any `knowledge/**/TOPIC.md` or companion file that pins a package version — not specific to any one topic.
