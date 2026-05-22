# WPF MVVM Framework Selection Rules

Detect the MVVM framework used by the project and apply the matching
code patterns.

---

## 1. Detection Order

```
1. If CLAUDE.local.md declares an explicit framework → use that.
2. Detect from .csproj NuGet references:
   - Prism.DryIoc or Prism.Unity → Prism 9
   - CommunityToolkit.Mvvm        → CommunityToolkit.Mvvm
3. Detect from keywords in the user conversation:
   - "prism", "bindablebase", "delegatecommand" → Prism 9
   - "communitytoolkit", "observableproperty", "relaycommand" → CommunityToolkit.Mvvm
4. Otherwise → CommunityToolkit.Mvvm (default).
```

---

## 2. How to Set It in CLAUDE.local.md

Add one of the following snippets to `CLAUDE.local.md` at the project
root:

```markdown
## MVVM Framework
- This project uses **CommunityToolkit.Mvvm**.
```

```markdown
## MVVM Framework
- This project uses **Prism 9**.
```

---

## 3. Skill Reference Routing

| MVVM Framework | Skill Code Examples | DI Pattern | Navigation |
|----------------|---------------------|------------|------------|
| **CommunityToolkit.Mvvm** | See `SKILL.md` | GenericHost | DataTemplate mapping |
| **Prism 9** | See `PRISM.md` | PrismApplication | RegionManager |

### Application Rules

```
WHEN generating ViewModel code:
  IF Prism 9 → Use BindableBase, SetProperty, DelegateCommand
  IF CommunityToolkit.Mvvm → Use ObservableObject, [ObservableProperty], [RelayCommand]

WHEN referencing skill examples:
  IF Prism 9 → Load PRISM.md (if present), otherwise SKILL.md
  IF CommunityToolkit.Mvvm → Load SKILL.md

WHEN setting up DI:
  IF Prism 9 → PrismApplication + IContainerRegistry
  IF CommunityToolkit.Mvvm → GenericHost + IServiceCollection
```

---

## 4. Per-Framework Detail Rules

- CommunityToolkit.Mvvm → see `communitytoolkit-mvvm.md`
- Prism 9 → see `prism9.md`
