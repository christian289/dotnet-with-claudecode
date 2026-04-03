# MVVM Constraints

Rules for enforcing strict MVVM layer separation in wpf-dev-pack projects.

---

## Prohibited DLL References in ViewModel Projects

ViewModel projects must NOT reference these assemblies:

- `WindowsBase.dll`
- `PresentationFramework.dll`
- `PresentationCore.dll`

If any of these appear in a `.csproj` targeting the ViewModel layer, remove them immediately.

---

## Prohibited Using Statements

The following namespaces must never appear in ViewModel code:

```csharp
// Prohibited in ViewModel projects
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;   // except ICommand — use CommunityToolkit.Mvvm instead
```

---

## Allowed Using Statements

These namespaces are safe for ViewModels:

```csharp
// Allowed
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
```

---

## ViewModel Type Restrictions

ViewModel properties and constructor parameters must use BCL types only:

- `string`, `int`, `double`, `bool`, `decimal`, `DateTime`, `Guid`
- `ObservableCollection<T>`, `List<T>`, `IEnumerable<T>`
- Other ViewModels or domain model types

Do NOT expose WPF types (`Brush`, `Visibility`, `ImageSource`, `Thickness`, etc.) as ViewModel properties. Convert in the View layer using converters or triggers.

---

## CommunityToolkit.Mvvm

Prefer `CommunityToolkit.Mvvm` for all ViewModel base classes and commands:

- Inherit from `ObservableObject` or `ObservableRecipient`
- Use `[ObservableProperty]` for bindable properties (source-generated)
- Use `[RelayCommand]` for commands (source-generated)
- Never hand-roll `INotifyPropertyChanged` or `ICommand` implementations
