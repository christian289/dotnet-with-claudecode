---
description: "Generates WPF ViewModel with View, DI registration, and DataTemplate mapping in one step. Use when creating a new screen, adding a View-ViewModel pair, or scaffolding ViewModel boilerplate with DI wiring. Usage: /wpf-dev-pack:make-wpf-viewmodel <ViewModelName> [--with-view] [--no-mapping]"
argument-hint: [ViewModelName]
---

# WPF ViewModel Generator

**If `$0` is empty, use the AskUserQuestion tool to ask: "Enter the ViewModel name (e.g., Dashboard, Settings)". Do NOT proceed until a valid name is provided. Use the response as the ViewModelName for all subsequent steps.**

Generate a `$0ViewModel` class with optional View, DI registration, and DataTemplate mapping.
Follows **View First MVVM** pattern — View determines its ViewModel.

- Replace `{Namespace}` with the project's root namespace detected from csproj or existing code.
- Replace `{ViewModelNamespace}` with the ViewModel project's CLR namespace for XAML xmlns declaration.

## Usage

```bash
# ViewModel + View + DI + DataTemplate mapping (full)
/wpf-dev-pack:make-wpf-viewmodel Dashboard --with-view

# ViewModel + DI only (no View, no mapping)
/wpf-dev-pack:make-wpf-viewmodel Settings

# ViewModel + View without DataTemplate mapping
/wpf-dev-pack:make-wpf-viewmodel Report --with-view --no-mapping
```

---

## Execution Procedure

### Step 1: Parse $0

- `$0` is the ViewModel name (without `ViewModel` suffix — auto-appended)
  - e.g., `Dashboard` → `DashboardViewModel.cs` + `DashboardView.xaml`
- `--with-view` flag: Generate View XAML + code-behind
- `--no-mapping` flag: Skip DataTemplate mapping registration

### Step 2: Locate Target Projects

Search for solution file and identify projects by naming convention:

| Project Suffix | Purpose | Files Placed |
|----------------|---------|--------------|
| `.ViewModels` | ViewModel project | `$0ViewModel.cs` |
| `.WpfApp` | WPF Application | `Views/$0View.xaml`, DI registration |
| `.WpfServices` | WPF Services | (referenced for DI) |

**Fallback**: If no `.ViewModels` project exists, place ViewModel in `ViewModels/` folder of main WPF project.

### Step 3: Generate ViewModel

Create `$0ViewModel.cs`:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace {Namespace}.ViewModels;

public sealed partial class $0ViewModel : ObservableObject
{
    [ObservableProperty] private string _title = "$0";

    [RelayCommand]
    private void Loaded()
    {
        // TODO: Initialize data
        // TODO: 데이터 초기화
    }
}
```

### Step 4: Generate View (if --with-view)

Create `Views/$0View.xaml`:

```xml
<UserControl x:Class="{Namespace}.Views.$0View"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="{ViewModelNamespace}"
             mc:Ignorable="d"
             d:DataContext="{d:DesignInstance vm:$0ViewModel}"
             d:DesignHeight="450" d:DesignWidth="800">
    <Grid>
        <TextBlock Text="{Binding Title}"
                   HorizontalAlignment="Center"
                   VerticalAlignment="Center"
                   FontSize="24" />
    </Grid>
</UserControl>
```

Create `Views/$0View.xaml.cs`:

```csharp
namespace {Namespace}.Views;

public partial class $0View
{
    public $0View()
    {
        InitializeComponent();
    }
}
```

### Step 5: Register in DI Container

Locate `App.xaml.cs` and add registration inside `ConfigureServices`:

```csharp
// In ConfigureServices method
services.AddSingleton<$0ViewModel>();
```

If `--with-view`:

```csharp
services.AddSingleton<$0ViewModel>();
services.AddSingleton<$0View>();
```

### Step 6: Add DataTemplate Mapping (unless --no-mapping)

Locate `Mappings.xaml` (or `ViewModelMappings.xaml`) and add:

```xml
<DataTemplate DataType="{x:Type vm:$0ViewModel}">
    <views:$0View />
</DataTemplate>
```

If `Mappings.xaml` does not exist, create it and merge into `App.xaml`:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Mappings.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### Step 7: Report Results

Output list of generated/modified files and next steps guidance.

---

## Generated File Structure

```
{ViewModelsProject}/
└── $0ViewModel.cs

{WpfAppProject}/
├── Views/
│   ├── $0View.xaml           (if --with-view)
│   └── $0View.xaml.cs        (if --with-view)
├── Mappings.xaml                  (modified or created)
└── App.xaml.cs                    (DI registration added)
```

---

## Error Handling

- Missing ViewModel name → output usage instructions
- No WPF project found → suggest `/wpf-dev-pack:make-wpf-project` first
- Duplicate ViewModel → warn and abort
- Missing Mappings.xaml → create with App.xaml merge

> **Prism 9 사용자**: See [PRISM.md](PRISM.md) for Prism-specific generation patterns.
