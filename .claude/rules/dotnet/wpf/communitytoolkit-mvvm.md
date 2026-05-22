# CommunityToolkit.Mvvm Coding Rules

> Applies when CommunityToolkit.Mvvm is the active MVVM framework (see
> `mvvm-framework.md`).

---

## NuGet Packages

```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.*" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.*" />
```

---

## ViewModel Authoring Rules

### Base Class

- Inherit from `ObservableObject`.
- The class must be declared `partial` (required by the source
  generator).

### Properties (`ObservableProperty`)

- **Single property**: place `[ObservableProperty]` on the same line as
  the backing field (inline).
- **Multiple attributes**: put the other attributes on their own lines
  and **always keep `[ObservableProperty]` last, inline** with the
  field declaration.

```csharp
// Single attribute
[ObservableProperty] private string _userName = string.Empty;

// Multiple attributes
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
[ObservableProperty] private string _email = string.Empty;
```

### .NET 9+ Partial Property

```csharp
[ObservableProperty] public partial string UserName { get; set; }
```

### Commands (`RelayCommand`)

- Use the `[RelayCommand]` attribute (source generator).
- For CanExecute: `[RelayCommand(CanExecute = nameof(CanXxx))]`.
- Async commands are automatically wired up when the method returns
  `Task`.

### Notifying Related Properties

- Use `[NotifyPropertyChangedFor(nameof(Xxx))]`.
- Use `[NotifyCanExecuteChangedFor(nameof(XxxCommand))]`.

---

## DI Pattern

- Use GenericHost (`Host.CreateDefaultBuilder()`).
- Register services on `IServiceCollection`.
- Create the host inside the `App()` constructor; start it from
  `OnStartup`.

---

## Skill References

When using an MVVM-related skill, follow the **SKILL.md** code examples.

| Skill | Reference File |
|-------|----------------|
| `implementing-communitytoolkit-mvvm` | SKILL.md |
| `configuring-dependency-injection` | SKILL.md |
| `structuring-wpf-projects` | SKILL.md |
| `mapping-viewmodel-view-datatemplate` | SKILL.md |
| `managing-wpf-application-lifecycle` | SKILL.md |
| Other MVVM-related skills | SKILL.md |
