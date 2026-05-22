# Prism 9 Coding Rules

> Applies when Prism 9 is the active MVVM framework (see
> `mvvm-framework.md`).

---

## NuGet Packages

| Project | Package |
|---------|---------|
| Shell (WPF App) | `Prism.DryIoc` (9.0.537) |
| Core (shared) | `Prism.Core` (9.0.537) |
| Module | `Prism.DryIoc` (9.0.537) |

> ⚠️ **Prism.Magician** (paid source generator) is not used. All code is
> written by hand.

---

## ViewModel Authoring Rules

### Base Class

- Inherit from `BindableBase`.
- `partial class` is not required (no source generator).

### Properties (`SetProperty`)

```csharp
private string _userName = string.Empty;
public string UserName
{
    get => _userName;
    set => SetProperty(ref _userName, value);
}
```

### Notifying Related Properties

```csharp
set
{
    if (SetProperty(ref _field, value))
    {
        RaisePropertyChanged(nameof(ComputedProperty));
    }
}
```

### Commands (`DelegateCommand`)

```csharp
private DelegateCommand? _saveCommand;
public DelegateCommand SaveCommand =>
    _saveCommand ??= new DelegateCommand(ExecuteSave, CanSave)
        .ObservesProperty(() => Email);
```

- Async: `AsyncDelegateCommand`.
- Generic: `DelegateCommand<T?>` (use Nullable for value types).
- Automatic CanExecute re-evaluation: `.ObservesProperty(() => Xxx)`.
- Manual re-evaluation: `RaiseCanExecuteChanged()`.

---

## DI Pattern

- Inherit from `PrismApplication` (GenericHost is not used).
- Register services in the `RegisterTypes(IContainerRegistry)` override.
- Return the MainWindow from `CreateShell()` (it is shown automatically).

### Registration APIs

| Purpose | API |
|---------|-----|
| Singleton | `containerRegistry.RegisterSingleton<I, T>()` |
| Transient | `containerRegistry.Register<I, T>()` |
| Navigation | `containerRegistry.RegisterForNavigation<V, VM>()` |
| Dialog | `containerRegistry.RegisterDialog<V, VM>()` |

---

## Navigation

- Use `IRegionManager.RequestNavigate()`.
- Set `ViewModelLocator.AutoWireViewModel="True"` in XAML.
- Manage the navigation lifecycle via the `INavigationAware` interface.

---

## Module Architecture

- Implement the `IModule` interface.
- Register modules in `ConfigureModuleCatalog()`.
- For inter-module communication: `IEventAggregator` + `PubSubEvent<T>`.

---

## Skill References

When using an MVVM-related skill, follow the **PRISM.md** code examples.

| Skill | Reference File |
|-------|----------------|
| `implementing-communitytoolkit-mvvm` | PRISM.md |
| `configuring-dependency-injection` | PRISM.md |
| `structuring-wpf-projects` | PRISM.md |
| `mapping-viewmodel-view-datatemplate` | PRISM.md |
| `managing-wpf-application-lifecycle` | PRISM.md |
| Other MVVM-related skills | PRISM.md (if present) |

> Skills without a PRISM.md companion (XAML, rendering, 3rd-party
> integrations, etc.) keep using SKILL.md unchanged.
