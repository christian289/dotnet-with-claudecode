# CMDS Pattern Refactoring Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extract duplicated rules from 7 agent files into 6 shared rule files, refactor agents to use `@rules/` references, and add Essential (Post-Compact) section to CLAUDE.md.

**Architecture:** Create shared rule files in `.claude/rules/`, replace inline duplicated content in agent `.md` files with `@rules/filename.md` references. Add a compressed-context recovery section to `.claude/CLAUDE.md`. All changes are markdown-only — no code, no hooks, no plugin.json changes.

**Tech Stack:** Markdown files, git

---

## File Map

**Create (6 files):**
- `.claude/rules/mvvm-constraints.md`
- `.claude/rules/resourcedictionary-patterns.md`
- `.claude/rules/freezable-performance.md`
- `.claude/rules/rendering-antipatterns.md`
- `.claude/rules/virtualization-patterns.md`
- `.claude/rules/converter-patterns.md`

**Modify (8 files):**
- `.claude/CLAUDE.md` — add Essential (Post-Compact) section
- `agents/wpf-architect.md` — replace MVVM Constraints + CustomControl Rules with @rules refs
- `agents/wpf-mvvm-expert.md` — replace Critical Constraints with @rules ref
- `agents/wpf-code-reviewer.md` — replace 5 duplicated sections with @rules refs
- `agents/wpf-control-designer.md` — replace Generic.xaml file structure with @rules ref
- `agents/wpf-data-binding-expert.md` — replace MVVM constraint + converter inline with @rules refs
- `agents/wpf-performance-optimizer.md` — replace Freezable + Virtualization sections with @rules refs
- `agents/wpf-xaml-designer.md` — replace ResourceDictionary structure + Generic.xaml with @rules ref

---

### Task 1: Create shared rule files (6 files)

**Files:**
- Create: `.claude/rules/mvvm-constraints.md`
- Create: `.claude/rules/resourcedictionary-patterns.md`
- Create: `.claude/rules/freezable-performance.md`
- Create: `.claude/rules/rendering-antipatterns.md`
- Create: `.claude/rules/virtualization-patterns.md`
- Create: `.claude/rules/converter-patterns.md`

- [ ] **Step 1: Create `mvvm-constraints.md`**

```markdown
# MVVM Constraints

Strict separation between ViewModel and WPF UI layer.

---

## Prohibited References in ViewModel

ViewModel projects must NOT reference:

- `WindowsBase.dll` (contains ICollectionView, Dispatcher)
- `PresentationFramework.dll`
- `PresentationCore.dll`

```csharp
// PROHIBITED in ViewModel
using System.Windows;           // ❌
using System.Windows.Controls;  // ❌
using System.Windows.Media;     // ❌
using System.Windows.Data;      // ❌ (contains ICollectionView)

// ALLOWED in ViewModel
using System.Collections.ObjectModel;  // ✅ BCL
using CommunityToolkit.Mvvm;           // ✅ MVVM toolkit
```

## ViewModel Type Restrictions

- ViewModel can only use **BCL types**: `IEnumerable<T>`, `ObservableCollection<T>`, `string`, `int`, etc.
- `ICollectionView` is in WindowsBase.dll — encapsulate in a Service Layer (WPF project side).
- `MessageBox`, `Window`, `UserControl` — never referenced from ViewModel. Use injected services.

## CommunityToolkit.Mvvm

Recommended MVVM framework for ViewModel implementation. Use source generators:
- `[ObservableProperty]` for bindable properties
- `[RelayCommand]` for commands
- `ObservableObject` as base class
```

- [ ] **Step 2: Create `resourcedictionary-patterns.md`**

```markdown
# ResourceDictionary Patterns

Rules for organizing WPF styles and resources.

---

## Generic.xaml = MergedDictionaries Hub Only

Generic.xaml must NOT contain any inline Style or ControlTemplate definitions.
It serves only as a hub that merges individual XAML files.

```xml
<!-- Generic.xaml — Hub only -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="/Themes/CustomButton.xaml"/>
        <ResourceDictionary Source="/Themes/CustomTextBox.xaml"/>
    </ResourceDictionary.MergedDictionaries>
</ResourceDictionary>
```

## Separate Each Control Style

Each control gets its own XAML file under `Themes/`:

```
MyControl.UI/
├── Themes/
│   ├── Generic.xaml           ← MergedDictionaries hub only
│   ├── CustomButton.xaml      ← Button style/template
│   └── CustomTextBox.xaml     ← TextBox style/template
```

## Resource Definition Order

- `x:Key` resources (Brushes, converters) must be defined **before** the Style that references them.
- Within a ResourceDictionary file: resources first, then Style.

## StaticResource vs DynamicResource

- **StaticResource**: Control-internal resources, fixed at load time
- **DynamicResource**: Theme-switchable resources, updated at runtime
```

- [ ] **Step 3: Create `freezable-performance.md`**

```markdown
# Freezable Performance

All Freezable objects must be frozen for performance and thread safety.

---

## Rule

Call `Freeze()` on every `Freezable` object (`Brush`, `Pen`, `Geometry`, `Transform`) after creation.

## Why

- Unfrozen Freezable objects create change-tracking overhead
- Frozen objects can be shared across threads
- Frozen objects consume less memory

## Pattern

```csharp
// CORRECT — freeze immediately after creation
var brush = new SolidColorBrush(Colors.Blue);
brush.Freeze();  // ✅

var pen = new Pen(brush, 1);
pen.Freeze();  // ✅

var geometry = new EllipseGeometry(new Point(0, 0), 10, 10);
geometry.Freeze();  // ✅

// WRONG — unfrozen resources
var brush = new SolidColorBrush(Colors.Blue);  // ❌ Not frozen
```

## In OnRender / DrawingContext

Create and freeze resources in the constructor or field initializer, NOT inside `OnRender()`:

```csharp
public class OptimizedElement : FrameworkElement
{
    private readonly Pen _pen;
    private readonly Brush _brush;

    public OptimizedElement()
    {
        _pen = new Pen(Brushes.Black, 1);
        _pen.Freeze();

        _brush = Brushes.Blue.Clone();
        _brush.Freeze();
    }

    protected override void OnRender(DrawingContext dc)
    {
        // Reuse frozen resources — do NOT create new ones here
        dc.DrawEllipse(_brush, _pen, new Point(50, 50), 10, 10);
    }
}
```
```

- [ ] **Step 4: Create `rendering-antipatterns.md`**

```markdown
# Rendering Anti-Patterns

Common WPF rendering mistakes that cause performance degradation.

---

## InvalidateVisual() in Loops

**ANTI-PATTERN**: Calling `InvalidateVisual()` inside a loop triggers a full re-render per iteration.

```csharp
// ❌ WRONG — triggers N re-renders
foreach (var point in points)
{
    _data.Add(point);
    InvalidateVisual();  // Called in loop!
}

// ✅ CORRECT — triggers 1 re-render
_data.AddRange(points);
InvalidateVisual();  // Called ONCE after all changes
```

## Creating Resources in OnRender

**ANTI-PATTERN**: Creating `Brush`/`Pen`/`Geometry` inside `OnRender()` allocates on every frame.

```csharp
// ❌ WRONG — allocates every render
protected override void OnRender(DrawingContext dc)
{
    var brush = new SolidColorBrush(Colors.Red);  // New allocation!
    dc.DrawRectangle(brush, null, new Rect(0, 0, 100, 100));
}

// ✅ CORRECT — reuse frozen resources (see freezable-performance.md)
```

## Batch Update Pattern

When updating data that triggers visual changes, batch all mutations before requesting a single re-render:

```csharp
public void AddPoints(IEnumerable<Point> points)
{
    _points.AddRange(points);
    InvalidateVisual();  // ONE call after all data added
}
```
```

- [ ] **Step 5: Create `virtualization-patterns.md`**

```markdown
# Virtualization Patterns

Required for any ItemsControl displaying large collections (100+ items).

---

## ItemsControl with VirtualizingStackPanel

Replace default `StackPanel` with `VirtualizingStackPanel`:

```xml
<!-- ❌ WRONG — no virtualization, creates UI elements for ALL items -->
<ItemsControl ItemsSource="{Binding ThousandsOfItems}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <StackPanel/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
</ItemsControl>

<!-- ✅ CORRECT — only creates visible UI elements -->
<ItemsControl ItemsSource="{Binding ThousandsOfItems}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
</ItemsControl>
```

## ListView / ListBox Attached Properties

```xml
<ListView ItemsSource="{Binding Items}"
          VirtualizingPanel.IsVirtualizing="True"
          VirtualizingPanel.VirtualizationMode="Recycling"
          VirtualizingPanel.ScrollUnit="Pixel">
</ListView>
```

## Key Settings

| Property | Value | Purpose |
|----------|-------|---------|
| `IsVirtualizing` | `True` | Enable virtualization |
| `VirtualizationMode` | `Recycling` | Reuse item containers (faster than `Standard`) |
| `ScrollUnit` | `Pixel` | Smooth scrolling (vs `Item` for snapping) |
```

- [ ] **Step 6: Create `converter-patterns.md`**

```markdown
# Converter Patterns

Standards for implementing IValueConverter and IMultiValueConverter.

---

## Singleton Pattern

Every converter must provide a `static Instance` property to avoid repeated allocations:

```csharp
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public static BoolToVisibilityConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
            return DependencyProperty.UnsetValue;

        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
```

## Pure Function Requirement

- Converters must be **pure functions** — no side effects, no state mutation
- Always handle `null` and `DependencyProperty.UnsetValue` inputs
- If bidirectional binding is not needed, throw `NotSupportedException` in `ConvertBack`

## TemplateBinding vs Binding in ControlTemplate

```xml
<!-- ✅ FASTER — use TemplateBinding in ControlTemplate -->
<Border Background="{TemplateBinding Background}"/>

<!-- SLOWER — only use when TemplateBinding doesn't work (e.g., converters, multi-binding) -->
<Border Background="{Binding Background, RelativeSource={RelativeSource TemplatedParent}}"/>
```

TemplateBinding is a compile-time optimization. Use Binding with TemplatedParent only when you need converter, StringFormat, or other Binding-specific features.
```

- [ ] **Step 7: Verify all 6 files exist**

Run: `ls .claude/rules/` from the wpf-dev-pack directory.
Expected: 9 files total (3 existing + 6 new):
```
converter-patterns.md
freezable-performance.md
mvvm-constraints.md
prohibitions.md
rendering-antipatterns.md
resourcedictionary-patterns.md
view-viewmodel-wiring-communitytoolkit.md
view-viewmodel-wiring-prism.md
virtualization-patterns.md
```

- [ ] **Step 8: Commit**

```bash
git add .claude/rules/mvvm-constraints.md .claude/rules/resourcedictionary-patterns.md .claude/rules/freezable-performance.md .claude/rules/rendering-antipatterns.md .claude/rules/virtualization-patterns.md .claude/rules/converter-patterns.md
git commit -m "feat: add 6 shared rule files extracted from agent definitions

Extract duplicated rules into .claude/rules/:
- mvvm-constraints: System.Windows prohibition, BCL-only types
- resourcedictionary-patterns: Generic.xaml hub, separate style files
- freezable-performance: Freeze() requirement for Brush/Pen/Geometry
- rendering-antipatterns: InvalidateVisual loop prohibition
- virtualization-patterns: VirtualizingStackPanel for large collections
- converter-patterns: singleton Instance, TemplateBinding preference"
```

---

### Task 2: Refactor wpf-architect.md

**Files:**
- Modify: `agents/wpf-architect.md:36-59`

- [ ] **Step 1: Replace embedded WPF Coding Rules section**

In `agents/wpf-architect.md`, replace lines 36-59 (the `## WPF Coding Rules (Embedded)` section through the end of `### CustomControl Rules`) with shared rule references:

```markdown
## Shared Rules

@rules/mvvm-constraints.md
@rules/resourcedictionary-patterns.md

## Project Structure

- `.Abstractions` - Interface, abstract class (IoC)
- `.Core` - Business logic (UI independent)
- `.ViewModels` - MVVM ViewModel (UI independent)
- `.WpfServices` - WPF related services
- `.WpfApp` - Execution entry point
- `.UI` - WPF Custom Control Library
```

This replaces:
- "WPF Coding Rules (Embedded)" header
- "MVVM Constraints" subsection (lines 38-45) → `@rules/mvvm-constraints.md`
- "CustomControl Rules" subsection (lines 55-58) → `@rules/resourcedictionary-patterns.md`
- Keeps "Project Structure" subsection (unique to architect)

- [ ] **Step 2: Verify the file is valid markdown**

Run: `head -70 agents/wpf-architect.md` from wpf-dev-pack directory.
Expected: frontmatter intact, `## Shared Rules` section with @rules refs, `## Project Structure` preserved, `## Requirements Interview` follows.

- [ ] **Step 3: Commit**

```bash
git add agents/wpf-architect.md
git commit -m "refactor(wpf-architect): extract duplicated rules to shared @rules/ references

Replace inline MVVM Constraints and CustomControl Rules with:
- @rules/mvvm-constraints.md
- @rules/resourcedictionary-patterns.md
Keep unique Project Structure section."
```

---

### Task 3: Refactor wpf-mvvm-expert.md

**Files:**
- Modify: `agents/wpf-mvvm-expert.md:22-29`

- [ ] **Step 1: Replace Critical Constraints with shared rule reference**

In `agents/wpf-mvvm-expert.md`, replace lines 22-29 (from `## Critical Constraints` through the 4 constraint items) with:

```markdown
## Shared Rules

@rules/mvvm-constraints.md

## Critical Constraints

- ✅ Use CommunityToolkit.Mvvm
- ✅ Collections use ObservableCollection<T>
- ✅ CollectionView operations in Service Layer
```

This removes the duplicated MVVM restriction bullet points and replaces them with the shared rule reference, while keeping the positive guidance that's specific to this agent's implementation focus.

- [ ] **Step 2: Verify the file is valid**

Run: `head -35 agents/wpf-mvvm-expert.md` from wpf-dev-pack directory.
Expected: frontmatter intact, `## Shared Rules` with @rules ref, then `## Critical Constraints` with only positive rules.

- [ ] **Step 3: Commit**

```bash
git add agents/wpf-mvvm-expert.md
git commit -m "refactor(wpf-mvvm-expert): extract MVVM constraints to shared @rules/ reference

Replace inline System.Windows prohibition with @rules/mvvm-constraints.md.
Keep CommunityToolkit.Mvvm positive guidance."
```

---

### Task 4: Refactor wpf-code-reviewer.md

**Files:**
- Modify: `agents/wpf-code-reviewer.md:53-179`

This agent has the most duplication (5 sections to extract).

- [ ] **Step 1: Replace Review Checklist sections with shared rule references**

In `agents/wpf-code-reviewer.md`, replace the content of sections 1 (MVVM Violation Check), 2 (Performance Anti-Patterns), and 3 (Best Practices) — lines 53 through 179 — with shared rule references plus only the unique review-specific content:

```markdown
## Shared Rules

@rules/mvvm-constraints.md
@rules/freezable-performance.md
@rules/rendering-antipatterns.md
@rules/virtualization-patterns.md
@rules/converter-patterns.md
@rules/resourcedictionary-patterns.md

## Review Checklist

### 1. MVVM Violation Check

Apply rules from `@rules/mvvm-constraints.md`. Additionally check:

#### Direct UI Manipulation in ViewModel
```csharp
// VIOLATION
public void UpdateUI()
{
    _textBox.Text = "Updated";  // ❌ Direct control access
    MessageBox.Show("Done");     // ❌ UI from ViewModel
}

// CORRECT: Use binding and services
[ObservableProperty]
private string _message;  // ✅ Bind to TextBox.Text

// Inject dialog service
private readonly IDialogService _dialogService;
```

#### Business Logic in Code-Behind
```csharp
// VIOLATION: Logic in .xaml.cs
private void Button_Click(object sender, RoutedEventArgs e)
{
    var result = CalculateTotal();  // ❌ Business logic
    SaveToDatabase(result);          // ❌ Data access
}

// CORRECT: Use Command binding
```

### 2. Performance Anti-Patterns

Apply rules from `@rules/freezable-performance.md`, `@rules/rendering-antipatterns.md`, `@rules/virtualization-patterns.md`.

### 3. Best Practices

#### DependencyProperty Callback Exception Handling
```csharp
// BEST PRACTICE: Validate in callback
private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
    if (d is not MyControl control) return;  // ✅ Null check

    if (e.NewValue is not int newValue) return;  // ✅ Type check

    // Safe to proceed
}
```

Apply rules from `@rules/converter-patterns.md` (TemplateBinding preference) and `@rules/resourcedictionary-patterns.md` (Generic.xaml hub pattern).
```

- [ ] **Step 2: Verify Review Output Format section is preserved**

Run: `tail -30 agents/wpf-code-reviewer.md` from wpf-dev-pack directory.
Expected: `## Review Output Format` section intact with the markdown template.

- [ ] **Step 3: Commit**

```bash
git add agents/wpf-code-reviewer.md
git commit -m "refactor(wpf-code-reviewer): extract 5 duplicated sections to shared @rules/ references

Replace inline MVVM, Freezable, InvalidateVisual, Virtualization,
Converter, and ResourceDictionary rules with @rules/ references.
Keep unique: LSP integration, UI manipulation check, code-behind check,
DependencyProperty callback, review output format."
```

---

### Task 5: Refactor wpf-control-designer.md

**Files:**
- Modify: `agents/wpf-control-designer.md:103-113`

- [ ] **Step 1: Replace File Structure Pattern with shared rule reference**

In `agents/wpf-control-designer.md`, replace the `## File Structure Pattern` section (lines 104-113) with a reference to the shared rule plus a simplified reminder:

```markdown
## File Structure

See `@rules/resourcedictionary-patterns.md` for Generic.xaml hub pattern and individual style file organization.
```

- [ ] **Step 2: Verify the file is valid**

Run: `tail -10 agents/wpf-control-designer.md` from wpf-dev-pack directory.
Expected: `## File Structure` section with @rules reference as last section.

- [ ] **Step 3: Commit**

```bash
git add agents/wpf-control-designer.md
git commit -m "refactor(wpf-control-designer): extract Generic.xaml pattern to shared @rules/ reference

Replace inline file structure pattern with @rules/resourcedictionary-patterns.md."
```

---

### Task 6: Refactor wpf-data-binding-expert.md

**Files:**
- Modify: `agents/wpf-data-binding-expert.md:26-31`

- [ ] **Step 1: Replace Critical Constraints with shared rule references**

In `agents/wpf-data-binding-expert.md`, replace lines 26-31 (the `## Critical Constraints` section) with:

```markdown
## Shared Rules

@rules/mvvm-constraints.md
@rules/converter-patterns.md

## Critical Constraints

- ❌ Code-behind에서 직접 데이터 조작 금지
- ✅ MVVM 패턴 준수
- ✅ Converter는 순수 함수로 구현 (side-effect 없음)
```

This extracts the MVVM constraint (`ViewModel에서 System.Windows 참조 금지`) and converter singleton pattern to shared rules, while keeping the data-binding-specific constraints inline.

- [ ] **Step 2: Verify the file is valid**

Run: `head -40 agents/wpf-data-binding-expert.md` from wpf-dev-pack directory.
Expected: frontmatter intact, `## Shared Rules` with 2 @rules refs, then `## Critical Constraints` with remaining unique items.

- [ ] **Step 3: Commit**

```bash
git add agents/wpf-data-binding-expert.md
git commit -m "refactor(wpf-data-binding-expert): extract MVVM + converter rules to shared @rules/ references

Replace inline System.Windows prohibition and converter singleton pattern with:
- @rules/mvvm-constraints.md
- @rules/converter-patterns.md
Keep unique: code-behind prohibition, pure function requirement."
```

---

### Task 7: Refactor wpf-performance-optimizer.md

**Files:**
- Modify: `agents/wpf-performance-optimizer.md:91-127`

- [ ] **Step 1: Replace Freezable and Virtualization sections with shared rule references**

In `agents/wpf-performance-optimizer.md`, replace the `### Freezable Pattern` section (lines 92-108) and `### VirtualizingStackPanel Pattern` section (lines 110-127) with shared rule references:

```markdown
### Freezable Pattern

@rules/freezable-performance.md

### Rendering Best Practices

@rules/rendering-antipatterns.md

### VirtualizingStackPanel Pattern

@rules/virtualization-patterns.md
```

Keep the surrounding content intact: DrawingContext Pattern (lines 26-60), DrawingVisual Pattern (lines 63-89), BitmapCache Pattern (lines 129-139), Dispatcher Priority (lines 141-156), Performance Checklist, Anti-Patterns.

- [ ] **Step 2: Verify unique sections preserved**

Run: `grep -n "^###" agents/wpf-performance-optimizer.md` from wpf-dev-pack directory.
Expected: DrawingContext, DrawingVisual, Freezable (now @rules ref), Rendering Best Practices (new), VirtualizingStackPanel (now @rules ref), BitmapCache, Dispatcher Priority all present.

- [ ] **Step 3: Commit**

```bash
git add agents/wpf-performance-optimizer.md
git commit -m "refactor(wpf-performance-optimizer): extract Freezable + Virtualization rules to shared @rules/ references

Replace inline Freezable and VirtualizingStackPanel patterns with:
- @rules/freezable-performance.md
- @rules/rendering-antipatterns.md
- @rules/virtualization-patterns.md
Keep unique: DrawingContext, DrawingVisual, BitmapCache, Dispatcher Priority."
```

---

### Task 8: Refactor wpf-xaml-designer.md

**Files:**
- Modify: `agents/wpf-xaml-designer.md:24-39`

- [ ] **Step 1: Replace ResourceDictionary and Generic.xaml sections with shared rule reference**

In `agents/wpf-xaml-designer.md`, replace lines 24-39 (from `### ResourceDictionary Structure` through the Generic.xaml code example) with:

```markdown
### ResourceDictionary Structure

@rules/resourcedictionary-patterns.md
```

Keep the subsequent sections intact: `### Individual Control Style Pattern` (lines 41-63), `### Resource Reference Rules` (lines 66-68), `### ControlTemplate Best Practices` (lines 70-91), `### Animation Pattern` (lines 93-101), `## Design Checklist` (lines 103-112).

- [ ] **Step 2: Verify unique sections preserved**

Run: `grep -n "^###" agents/wpf-xaml-designer.md` from wpf-dev-pack directory.
Expected: ResourceDictionary Structure (now @rules ref), Individual Control Style Pattern, Resource Reference Rules, ControlTemplate Best Practices, Animation Pattern all present.

- [ ] **Step 3: Commit**

```bash
git add agents/wpf-xaml-designer.md
git commit -m "refactor(wpf-xaml-designer): extract ResourceDictionary pattern to shared @rules/ reference

Replace inline Generic.xaml hub pattern with @rules/resourcedictionary-patterns.md.
Keep unique: Individual Control Style, ControlTemplate, Animation patterns."
```

---

### Task 9: Add Essential (Post-Compact) to CLAUDE.md

**Files:**
- Modify: `.claude/CLAUDE.md` — insert after line 28 ("MVVM Approach" section)

- [ ] **Step 1: Add Essential (Post-Compact) section**

In `.claude/CLAUDE.md`, insert a new section after the "MVVM Approach: View First" section (after line 28) and before the ".NET Version Configuration" section (line 30):

```markdown

---

## Essential (Post-Compact)

These rules MUST survive context compression. If prior context is lost, re-read this section:

1. **ViewModelLocator 금지** — DI + DataTemplate 매핑만 사용 (`rules/prohibitions.md`)
2. **ViewModel에 System.Windows 참조 금지** — BCL 타입만 사용 (`rules/mvvm-constraints.md`)
3. **Freezable 객체는 반드시 Freeze()** — Brush, Pen, Geometry (`rules/freezable-performance.md`)
4. **Generic.xaml = MergedDictionaries hub 전용** (`rules/resourcedictionary-patterns.md`)
5. **HandMirror로 API 시그니처 검증 후 코드 작성**
6. **View First MVVM** — framework별 wiring은 `rules/` 참조

```

- [ ] **Step 2: Verify CLAUDE.md structure**

Run: `grep -n "^## " .claude/CLAUDE.md` from wpf-dev-pack directory.
Expected sections in order:
1. `## Required Plugin Dependencies`
2. `## MVVM Approach: View First`
3. `## Essential (Post-Compact)` (NEW)
4. `## .NET Version Configuration`
5. `## Core Rules`
6. `## Requirements Interview System`
7. `## Trigger Priority`
8. `## Trigger Behavior`

- [ ] **Step 3: Commit**

```bash
git add .claude/CLAUDE.md
git commit -m "feat: add Essential (Post-Compact) section to CLAUDE.md

6 critical rules that must survive context compression:
- ViewModelLocator prohibition
- ViewModel System.Windows restriction
- Freezable Freeze() requirement
- Generic.xaml hub-only pattern
- HandMirror API verification
- View First MVVM pattern"
```

---

### Task 10: Final verification and summary commit

- [ ] **Step 1: Verify rules/ directory has 9 files**

Run: `ls -1 .claude/rules/` from wpf-dev-pack directory.
Expected: 9 files (3 existing + 6 new).

- [ ] **Step 2: Verify no duplicated rules remain in agents**

Run: `grep -l "No System.Windows references in ViewModel" agents/*.md` from wpf-dev-pack directory.
Expected: NO matches (the literal string should only exist in `.claude/rules/mvvm-constraints.md` now).

Run: `grep -l "Generic.xaml serves only as MergedDictionaries hub" agents/*.md` from wpf-dev-pack directory.
Expected: NO matches.

Run: `grep -l "brush.Freeze();  // REQUIRED" agents/*.md` from wpf-dev-pack directory.
Expected: NO matches.

- [ ] **Step 3: Verify all agents reference @rules/**

Run: `grep -l "@rules/" agents/*.md` from wpf-dev-pack directory.
Expected: All 7 agent files listed.

- [ ] **Step 4: Verify Essential section exists in CLAUDE.md**

Run: `grep "Essential (Post-Compact)" .claude/CLAUDE.md` from wpf-dev-pack directory.
Expected: Match found.

- [ ] **Step 5: No additional commit needed — all changes already committed per task**

Verify with `git log --oneline` that all 9 commits are present.
