# WPF Project Code Generation Guidelines

## Core Principles

- For UI customization, use a WPF Custom Control Library project.
- For Converters and WPF UI service layers, use a WPF Class Library
  project.
- The MVVM framework is decided per the `mvvm-framework.md` configuration.

---

## 1. Dependency Injection

> **📌 Detailed guide**: see the `/configuring-dependency-injection` skill.

- By default, use `AddSingleton()` only.
- The DI container composition depends on the active MVVM framework:
  - CommunityToolkit.Mvvm → GenericHost (`Microsoft.Extensions.Hosting`)
  - Prism 9 → PrismApplication (`IContainerRegistry`)

---

## 2. Solution and Project Structure

> **📌 Detailed guide**: see the `/structuring-wpf-projects` skill.

**Project naming conventions:**

| Suffix | Type | Purpose |
|--------|------|---------|
| `.Abstractions` | .NET Class Library | Interfaces, abstract classes (IoC) |
| `.Core` | .NET Class Library | Business logic (UI-independent) |
| `.ViewModels` | .NET Class Library | MVVM ViewModels (UI-independent) |
| `.WpfServices` | WPF Class Library | WPF-related services |
| `.WpfApp` | WPF Application | Entry point |
| `.UI` | WPF Custom Control Library | Custom controls |

> ⚠️ When Prism 9 is used, `.Modules.*` project structure may be
> appropriate. See `structuring-wpf-projects/PRISM.md` for details.

---

## 3. MVVM Pattern

> **📌 MVVM framework selection**: see `mvvm-framework.md`.

### Hard Constraints (framework-agnostic)

- **ViewModel classes must not depend on the UI framework.**
  - References to classes whose namespace starts with `System.Windows`
    are forbidden.
  - Exception: ViewModels declared inside a Custom Control project.
- **MVVM constraints apply to ViewModels only.**
  - Converters, Services, and Managers may reference the UI framework.

### Reference Assembly Rules

**Prohibited references in ViewModel projects:**

- ❌ `WindowsBase.dll` (contains `ICollectionView`)
- ❌ `PresentationFramework.dll`
- ❌ `PresentationCore.dll`

**Allowed references in ViewModel projects:**

- ✅ BCL types only (`IEnumerable`, `ObservableCollection`, etc.)
- ✅ The chosen MVVM framework package (CommunityToolkit.Mvvm or
  Prism.Core)

---

## 4. XAML Authoring

> **📌 Detailed guide**: see the `/designing-wpf-customcontrol-architecture` skill.

- Use a stand-alone control style via CustomControl + ResourceDictionary.
- Use `Generic.xaml` only as a `MergedDictionaries` hub.
- Split each control's style into its own XAML file.

---

## 5. CollectionView MVVM Pattern

> **📌 Detailed guide**: see the `/managing-wpf-collectionview-mvvm` skill.

- Encapsulate `CollectionViewSource` access behind a service layer.
- ViewModels expose `IEnumerable` only (no WPF types).

---

## 6. Popup Focus Management

> **📌 Detailed guide**: see the `/managing-wpf-popup-focus` skill.

- When using `Popup`, manage focus via the `PreviewMouseDown` event.

---

## 7. DataTemplate / Navigation Mapping

> **📌 Detailed guide**: see the `/mapping-viewmodel-view-datatemplate` skill.

- View-to-ViewModel mapping depends on the active MVVM framework:
  - CommunityToolkit.Mvvm → `Mappings.xaml` `DataTemplate` mapping
  - Prism 9 → `RegionManager` + `RegisterForNavigation`

---

## 8. High-Performance Rendering (`DrawingContext`)

> **📌 Detailed guide**: see the `/rendering-with-drawingcontext` skill.

- For rendering large numbers of shapes, use `DrawingContext` instead
  of `Shape` (10–50× faster).
- Inherit from `FrameworkElement` and draw inside `OnRender`.
- `Freeze()` is mandatory for `Pen`, `Brush`, and `Geometry`.
- Call `InvalidateVisual()` **once**, after the bulk data load is
  complete.
