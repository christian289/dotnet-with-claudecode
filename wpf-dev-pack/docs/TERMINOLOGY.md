[🇰🇷 한국어](./TERMINOLOGY.ko.md)

# wpf-dev-pack — Terminology

This document defines the MVVM composition styles adopted by wpf-dev-pack
based on the official Microsoft definitions. It is the single source of
truth for the terminology used throughout the plugin.

---

## 1. Four-Axis Separation Model

The relationship between a View and a ViewModel splits into two
**orthogonal, independent axes**.

### Axis 1: Composition Direction (View First vs ViewModel First)

The Microsoft-official classification rule is a single question:
**what is the lookup key for composition / navigation?**

| Lookup key | Classification |
|---|---|
| View type name (string) | **View First Composition** |
| ViewModel type | **ViewModel First Composition** |

- **View First Composition** — the identifier is the View name. Examples:
  - Prism `RequestNavigate("ContentRegion", "HomeView")`
  - Prism `ViewModelLocator.AutoWireViewModel="True"` (the View is the anchor)
  - View code-behind `DataContext = new HomeViewModel();`

- **ViewModel First Composition** — the identifier is the ViewModel type.
  Examples:
  - WPF implicit DataTemplate
    `<DataTemplate DataType="{x:Type vm:HomeViewModel}">`
  - A navigation service that targets a ViewModel type

References:
- https://learn.microsoft.com/dotnet/architecture/maui/navigation
- https://learn.microsoft.com/dotnet/architecture/maui/mvvm#connecting-view-models-to-views

### Axis 2: ViewModel State Management (Stateful vs Stateless)

- **Stateful ViewModel** — the ViewModel instance holds its own state
  directly (the de-facto standard across the Korean WPF ecosystem).
- **Stateless ViewModel** — state is delegated to an external Manager or
  Service, and the ViewModel is transient (the recommended style in
  Stylet and Caliburn.Micro).

### Orthogonality of the Two Axes

| Composition Direction | State Management | Typical example |
|---|---|---|
| View First | Stateful | Prism `RegisterForNavigation` + `RegionManager` (wpf-dev-pack Prism path); a classical Prism `ViewModelLocator` setup |
| View First | Stateless | (rare) View fetches data from external state on every render |
| **ViewModel First** | **Stateful** | **wpf-dev-pack CommunityToolkit.Mvvm path** (`Mappings.xaml` + implicit DataTemplate) |
| ViewModel First | Stateless | Stylet's recommended transient-VM style |

Pre-v1.6.4 docs implicitly assumed "ViewModel First ⇒ Stateless" and used a
single "View First MVVM" label. The two axes are in fact independent, and
wpf-dev-pack actually enforces two distinct combinations depending on the
chosen MVVM framework (see §2).

---

## 2. Adopted Combinations

wpf-dev-pack enforces **different** composition styles depending on the
chosen MVVM framework. Both paths share **Stateful ViewModel** as the
state-management style.

### 2.1 CommunityToolkit.Mvvm path (default)

> **ViewModel First Composition + Stateful ViewModel**

Concrete mechanism: implicit DataTemplate mapping via `Mappings.xaml`.

```xml
<DataTemplate DataType="{x:Type vm:HomeViewModel}">
    <views:HomeView />
</DataTemplate>
```

```csharp
CurrentViewModel = new HomeViewModel();  // the ViewModel instance is the lookup key
```

Details: [`view-viewmodel-wiring-communitytoolkit.md`](../.claude/rules/view-viewmodel-wiring-communitytoolkit.md)

### 2.2 Prism 9 path (alternative)

> **View First Composition + Stateful ViewModel**

Concrete mechanism:
`IContainerRegistry.RegisterForNavigation<View, ViewModel>()` registration
plus `IRegionManager.RequestNavigate("Region", "ViewName")` navigation
(by **view-name string**).

```csharp
containerRegistry.RegisterForNavigation<HomeView, HomeViewModel>();
// ...
_regionManager.RequestNavigate("ContentRegion", "HomeView");  // View name is the lookup key
```

Details: [`view-viewmodel-wiring-prism.md`](../.claude/rules/view-viewmodel-wiring-prism.md)

### 2.3 Shared

- **Stateful ViewModel** is the standard state-management style on both
  paths.
- Each framework allows **exactly one** matching mechanism; multiple
  View-VM matching paths must not co-exist in the same project.
- ViewModel classes must not reference `System.Windows.*` UI types (except
  `ICommand`).

---

## 3. Explicitly Prohibited Patterns

Full rules live in [`prohibitions.md`](../.claude/rules/prohibitions.md).

Summary:

| Prohibited pattern | Classification | Reason |
|---|---|---|
| Prism `ViewModelLocator.AutoWireViewModel="True"` | View First (Prism's alternate mechanism) | The Prism path's single matching mechanism is `RegisterForNavigation`; `ViewModelLocator` competes with it |
| View code-behind `DataContext = new XxxViewModel()` | View First (imperative) | Breaks the CommunityToolkit path's single matching path |
| `<UserControl.DataContext><vm:XxxVM /></UserControl.DataContext>` | View First (declarative) | Same as above |
| Stateless ViewModel + transient IoC registration | Stateless VM composition | A different framework ecosystem (Stylet / Caliburn) — out of scope |
| Any matching mechanism beyond `Mappings.xaml` / `RegisterForNavigation` | (any) | Single matching path per project must be preserved |

---

## 4. Terminology Change History

Pre-v1.6.4 docs uniformly labeled wpf-dev-pack's adopted style as
**"View First MVVM"**, which conflicts with Microsoft's official
definition. In particular, the CommunityToolkit.Mvvm path
(`Mappings.xaml` DataTemplate) uses the ViewModel type as the lookup key
and is therefore **ViewModel First Composition**, not View First.

The labels were corrected as follows:

| Era | Label |
|---|---|
| Pre-v1.6.4 | "View First MVVM" (single label) — incorrect |
| v1.6.4+ (CommunityToolkit path) | "ViewModel First Composition + Stateful ViewModel" |
| v1.6.4+ (Prism path) | "View First Composition + Stateful ViewModel" |

> **The actual code patterns enforced by the plugin have not changed.**
> Only the labels have been corrected to standard terminology, so no user
> code modification is required.

---

## 5. References

| Topic | URL |
|---|---|
| MVVM — Connecting view models to views | https://learn.microsoft.com/dotnet/architecture/maui/mvvm#connecting-view-models-to-views |
| Navigation — View first vs ViewModel first | https://learn.microsoft.com/dotnet/architecture/maui/navigation |
| CommunityToolkit.Mvvm | https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/ |
| WPF Overview | https://learn.microsoft.com/dotnet/desktop/wpf/overview/ |

---

## Appendix A — Quick Reference

### View First vs ViewModel First Decision Rule

> **One-question test**: what is the lookup key for composition / navigation?
> - View name is the key → **View First**
> - ViewModel type is the key → **ViewModel First**

### Pattern Classification Table

| Pattern | Lookup key | Classification | Plugin policy |
|---|---|---|---|
| `Mappings.xaml` implicit DataTemplate | ViewModel type | ViewModel First (Stateful) | ✅ Adopted (CommunityToolkit path) |
| Prism `RegisterForNavigation` + `RequestNavigate("View")` | View name (string) | View First (Stateful) | ✅ Adopted (Prism path) |
| Prism `ViewModelLocator.AutoWireViewModel` | View name | View First (Stateful) | ❌ Prohibited (Prism path's single mechanism is `RegisterForNavigation`) |
| code-behind `DataContext = new VM()` | (View picks its own VM) | View First (imperative) | ❌ Prohibited |
| Stylet transient VM + naming convention | ViewModel type | ViewModel First (Stateless) | ❌ Prohibited (out of scope) |
