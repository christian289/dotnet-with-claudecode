# Prohibitions

Items explicitly banned from wpf-dev-pack.
Do NOT introduce any of these during code generation, review, or refactoring.

For terminology used below (View First / ViewModel First Composition,
Stateful / Stateless ViewModel), see [`docs/TERMINOLOGY.md`](../../docs/TERMINOLOGY.md).

Adopted combinations (per chosen MVVM framework):
- **CommunityToolkit.Mvvm path** — ViewModel First Composition + Stateful ViewModel
  (Mappings.xaml + implicit DataTemplate)
- **Prism 9 path** — View First Composition + Stateful ViewModel
  (`RegisterForNavigation` + `IRegionManager.RequestNavigate`)

---

## P-001: Mixing alternate composition mechanisms in the same project

### P-001-a: Prism `ViewModelLocator.AutoWireViewModel` is prohibited

**Prohibited:**
```xml
<Window prism:ViewModelLocator.AutoWireViewModel="True" />
```

**Classification:** View First Composition (an *alternate* mechanism inside Prism)

**Reason:** The Prism path's single matching mechanism is
`RegisterForNavigation` + `IRegionManager.RequestNavigate`. `ViewModelLocator`
locates and attaches the ViewModel by naming convention after the View is
instantiated; using both mechanisms in the same Prism project produces two
parallel matching paths, increasing maintenance and cognitive cost.

### P-001-b: `DataContext = new XxxViewModel()` in View code-behind is prohibited

**Prohibited:**
```csharp
public XxxView()
{
    InitializeComponent();
    DataContext = new XxxViewModel();   // Prohibited
}
```

**Classification:** View First Composition (imperative variant)

**Reason:** In the CommunityToolkit.Mvvm path the single matching mechanism
is `Mappings.xaml` + implicit DataTemplate (ViewModel First). A View directly
choosing its own VM breaks that single-path policy. In the Prism path the
single mechanism is `RegisterForNavigation`; manual `DataContext` assignment
likewise breaks single-path.

### P-001-c: Inline XAML `DataContext` declaration is prohibited

**Prohibited:**
```xml
<UserControl.DataContext>
    <vm:XxxViewModel />   <!-- Prohibited -->
</UserControl.DataContext>
```

**Classification:** View First Composition (declarative variant)

**Reason:** Same as P-001-b. Declarative specification of the VM type by the
View is still the View choosing its own VM.

### P-001-d: Introducing any other matching mechanism is prohibited

**Prohibited:** Adding any View-VM matching mechanism beyond the one selected
for the chosen framework (e.g., naming-convention auto-matching, custom
locators, reflection-based wiring).

**Reason:** Guarantees a single matching path per project. Co-existence of
multiple mechanisms increases maintenance cost and cognitive load.

---

## P-002: `System.Windows.*` UI types in ViewModel classes (except `ICommand`)

**Prohibited:** References to `System.Windows.*` UI types
(`Visibility`, `Brush`, `ImageSource`, `Thickness`, `Window`, etc.) from
inside ViewModel classes.

**Allowed exception:** `System.Windows.Input.ICommand` (used by both
`RelayCommand` and `DelegateCommand`).

**Reason:** Guarantees ViewModel testability and isolates the WPF framework
dependency. Complies with the "view model is unaware of the view" principle
from Microsoft's MVVM guidance. UI-type conversion belongs in the View layer
(converters, triggers, value converters).

---

## P-003: Stateless ViewModel + transient IoC registration pattern

**Prohibited:** Registering all ViewModels as transient lifetime in the IoC
container and delegating ViewModel state to an external Service/Manager.

**Classification:** Stateless ViewModel composition style
(common in Stylet, Caliburn.Micro recommended patterns)

**Reason:** wpf-dev-pack adopts **Stateful ViewModel** as the standard state
management style across both framework paths. Stateless-VM patterns belong to
separate MVVM framework ecosystems and are outside this plugin's scope.

---

## P-004: Mixing the two adopted paths in a single project

**Prohibited:** Within a single project, using both `Mappings.xaml`
(CommunityToolkit path) and `RegisterForNavigation` + `IRegionManager`
(Prism path) for View-VM wiring.

**Reason:** Each project picks one MVVM framework. The chosen framework
selects exactly one matching mechanism. Mixing the two produces ambiguous
View resolution and defeats the single-matching-path guarantee.

---

## Quick reference

| Pattern | Classification | Policy |
|---|---|---|
| `Mappings.xaml` implicit DataTemplate | ViewModel First (Stateful) | ✅ Adopted (CommunityToolkit path) |
| Prism `RegisterForNavigation` + `RequestNavigate("View")` | View First (Stateful) | ✅ Adopted (Prism path) |
| Prism `ViewModelLocator.AutoWireViewModel` | View First (Stateful) | ❌ Prohibited (P-001-a) |
| code-behind `DataContext = new VM()` | View First (imperative) | ❌ Prohibited (P-001-b) |
| Inline XAML `<UserControl.DataContext>` | View First (declarative) | ❌ Prohibited (P-001-c) |
| Naming-convention / reflection-based wiring | (any) | ❌ Prohibited (P-001-d) |
| `System.Windows.*` in ViewModel (except `ICommand`) | — | ❌ Prohibited (P-002) |
| Stateless VM + transient IoC | Stateless VM | ❌ Prohibited (P-003) |
| Mixing `Mappings.xaml` and Prism `RegionManager` | — | ❌ Prohibited (P-004) |
