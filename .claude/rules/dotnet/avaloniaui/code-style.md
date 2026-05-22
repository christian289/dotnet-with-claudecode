# AvaloniaUI Project Code Generation Guidelines

## Core Principles

- Use the general .NET C# code guidelines as the baseline.
- Apply the same MVVM principles as the WPF guidelines.
- For UI customization, use an Avalonia Custom Control Library project.
- For Converters and AvaloniaUI service layers, use an Avalonia Class
  Library project.
- Use the **CommunityToolkit.Mvvm** NuGet package.

---

## 1. Dependency Injection

> **📌 Detailed guide**: see the `/configuring-avalonia-dependency-injection` skill.

- By default, use `AddSingleton()` only.
- Compose the DI container via GenericHost.

---

## 2. Solution and Project Structure

> **📌 Detailed guide**: see the `/structuring-avalonia-projects` skill.

**Project naming conventions:**

| Suffix | Type | Purpose |
|--------|------|---------|
| `.Abstractions` | .NET Class Library | Interfaces, abstract classes (IoC) |
| `.Core` | .NET Class Library | Business logic (UI-independent) |
| `.ViewModels` | .NET Class Library | MVVM ViewModels (UI-independent) |
| `.AvaloniaServices` | Avalonia Class Library | Avalonia-related services |
| `.AvaloniaLib` | Avalonia Class Library | Reusable Avalonia components |
| `.AvaloniaApp` | Avalonia Application | Entry point |
| `.UI` | Avalonia Custom Control Library | Custom controls |

---

## 3. MVVM Pattern

> **📌 Detailed guide**: see the `/implementing-communitytoolkit-mvvm` skill.

### Hard Constraints

- **ViewModel classes must not depend on the UI framework.**
  - References to any namespace starting with `Avalonia` are forbidden.
  - Exception: ViewModels declared inside a Custom Control project.
- **MVVM constraints apply to ViewModels only.**
  - Converters, Services, and Managers may reference the UI framework.

### Reference Assembly Rules

**Prohibited references in ViewModel projects:**

- ❌ `Avalonia.Base.dll`
- ❌ `Avalonia.Controls.dll`
- ❌ `Avalonia.Markup.Xaml.dll`

**Allowed references in ViewModel projects:**

- ✅ BCL types only (`IEnumerable`, `ObservableCollection`, etc.)
- ✅ `CommunityToolkit.Mvvm`

---

## 4. AXAML Authoring

> **📌 Detailed guide**: see the `/designing-avalonia-customcontrol-architecture` skill.

- Use a stand-alone control style via CustomControl + ControlTheme.
- Use `Generic.axaml` only as a `MergedDictionaries` hub.
- Split each control's ControlTheme into its own AXAML file.
- Use `StyledProperty` (not `DependencyProperty`).
- Apply styles via the CSS-like `Classes` property.
- Manage state via pseudo-classes (`:pointerover`, `:pressed`, etc.).

---

## 5. CollectionView Patterns

> **📌 Detailed guide**: see the `/using-avalonia-collectionview` skill.

**⚠️ AvaloniaUI does not support WPF's `CollectionViewSource`.**

- Use `DataGridCollectionView` (recommended).
- Alternatively, use ReactiveUI + DynamicData.

---

## 6. View ↔ ViewModel Mapping via DataTemplate

> **📌 Detailed guide**: see the `/mapping-viewmodel-view-datatemplate` skill.

- Define ViewModel-to-View `DataTemplate` mappings in `Mappings.axaml`.
- Bind a ViewModel to `ContentControl.Content` to have its View
  rendered automatically.

---

## 7. WPF vs AvaloniaUI — Key Differences

| Item | WPF | AvaloniaUI |
|------|-----|-----------|
| File extension | `.xaml` | `.axaml` |
| Style definition | `Style` + `ControlTemplate` | `ControlTheme` |
| State management | `Trigger`, `DataTrigger` | Pseudo-classes, Style Selectors |
| CSS support | ❌ | ✅ (`Classes` property) |
| Resource merging | `MergedDictionaries` + `ResourceDictionary` | `MergedDictionaries` + `ResourceInclude` |
| Dependency properties | `DependencyProperty` | `StyledProperty`, `DirectProperty` |
| CollectionView | `CollectionViewSource` | `DataGridCollectionView`, ReactiveUI |

**⚠️ ResourceInclude vs MergeResourceInclude:**

- **`ResourceInclude`**: used inside a regular `ResourceDictionary` file.
- **`MergeResourceInclude`**: used only inside `Application.Resources`
  in `App.axaml`.

---

## 8. Required NuGet Packages

```xml
<!-- AvaloniaUI Application -->
<ItemGroup>
  <PackageReference Include="Avalonia" Version="11.0.*" />
  <PackageReference Include="Avalonia.Desktop" Version="11.0.*" />
  <PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.*" />
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.*" />
  <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
</ItemGroup>

<!-- Optional: DataGrid support -->
<ItemGroup>
  <PackageReference Include="Avalonia.Controls.DataGrid" Version="11.0.*" />
</ItemGroup>

<!-- Optional: ReactiveUI support -->
<ItemGroup>
  <PackageReference Include="ReactiveUI.Avalonia" Version="20.1.*" />
  <PackageReference Include="DynamicData" Version="9.0.*" />
</ItemGroup>
```

---

## 9. Checklist

### AvaloniaUI Project

- [ ] ViewModels do not reference Avalonia.
- [ ] ViewModels use BCL types only.
- [ ] CustomControls inherit from an existing Avalonia control.
- [ ] CustomControls use `StyledProperty`.
- [ ] `Generic.axaml` is used only as a `MergedDictionaries` hub.
- [ ] Each control's ControlTheme lives in its own AXAML file.
- [ ] Styles are applied via the `Classes` property.
- [ ] State management uses pseudo-classes.
- [ ] `DataGridCollectionView` is used instead of `CollectionView`.
- [ ] `App.axaml.cs` wires up GenericHost and the DI container.

---

## 10. Cautions

### ⚠️ Common Mistakes

1. Referencing the `Avalonia` namespace from a ViewModel — MVVM violation.
2. Referencing `Avalonia.Base.dll` / `Avalonia.Controls.dll` from a
   ViewModel project.
3. Inheriting CustomControls directly from `TemplatedControl` — they
   should inherit from an existing control.
4. Using WPF's `DependencyProperty` verbatim — use `StyledProperty`.
5. Using WPF's `Trigger` verbatim — use pseudo-classes and Style
   Selectors.
6. Using WPF's `CollectionViewSource` — use `DataGridCollectionView`
   or ReactiveUI.
7. Writing ControlThemes directly inside `Generic.axaml` — split into
   per-control files and pull them in via `ResourceInclude`.
8. Forgetting to configure GenericHost in `App.axaml.cs`.

---

## 11. Official Documentation

- [AvaloniaUI Documentation](https://docs.avaloniaui.net/)
- [Styled Properties](https://docs.avaloniaui.net/docs/guides/custom-controls/defining-properties)
- [Control Themes](https://docs.avaloniaui.net/docs/guides/styles-and-resources/control-themes)
- [MVVM Pattern](https://docs.avaloniaui.net/docs/concepts/the-mvvm-pattern/)
- [Dependency Injection](https://docs.avaloniaui.net/docs/guides/implementation-guides/how-to-use-dependency-injection)
