---
description: "Generates WPF ViewModel with View, DI registration, and DataTemplate mapping in one step. Use when creating a new screen, adding a View-ViewModel pair, or scaffolding ViewModel boilerplate with DI wiring. Usage: /wpf-dev-pack:make-wpf-viewmodel <ViewModelName> [--with-view] [--no-mapping]"
---

# WPF ViewModel Generator

Generates ViewModel class, optional View, DI registration, and DataTemplate mapping in a single command.
Follows **View First MVVM** pattern — View determines its ViewModel.

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

### Step 1: Argument Parsing

- 1st argument (required): ViewModel name (without `ViewModel` suffix — auto-appended)
  - `Dashboard` → `DashboardViewModel.cs` + `DashboardView.xaml`
- `--with-view`: Generate View XAML + code-behind
- `--no-mapping`: Skip DataTemplate mapping registration

### Step 2: Locate Target Projects

Search for solution file and identify projects by naming convention:

| Project Suffix | Purpose | Files Placed |
|----------------|---------|--------------|
| `.ViewModels` | ViewModel project | `{Name}ViewModel.cs` |
| `.WpfApp` | WPF Application | `Views/{Name}View.xaml`, DI registration |
| `.WpfServices` | WPF Services | (referenced for DI) |

**Fallback**: If no `.ViewModels` project exists, place ViewModel in `ViewModels/` folder of main WPF project.

### Step 3: Generate ViewModel

Create `{Name}ViewModel.cs`:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace {Namespace}.ViewModels;

public sealed partial class {Name}ViewModel : ObservableObject
{
    [ObservableProperty] private string _title = "{Name}";

    [RelayCommand]
    private void Loaded()
    {
        // TODO: Initialize data
        // TODO: 데이터 초기화
    }
}
```

### Step 4: Generate View (if --with-view)

Create `Views/{Name}View.xaml`:

```xml
<UserControl x:Class="{Namespace}.Views.{Name}View"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="{ViewModelNamespace}"
             mc:Ignorable="d"
             d:DataContext="{d:DesignInstance vm:{Name}ViewModel}"
             d:DesignHeight="450" d:DesignWidth="800">
    <Grid>
        <TextBlock Text="{Binding Title}"
                   HorizontalAlignment="Center"
                   VerticalAlignment="Center"
                   FontSize="24" />
    </Grid>
</UserControl>
```

Create `Views/{Name}View.xaml.cs`:

```csharp
namespace {Namespace}.Views;

public partial class {Name}View
{
    public {Name}View()
    {
        InitializeComponent();
    }
}
```

### Step 5: Register in DI Container

Locate `App.xaml.cs` and add registration inside `ConfigureServices`:

```csharp
// In ConfigureServices method
services.AddSingleton<{Name}ViewModel>();
```

If `--with-view`:

```csharp
services.AddSingleton<{Name}ViewModel>();
services.AddSingleton<{Name}View>();
```

### Step 6: Add DataTemplate Mapping (unless --no-mapping)

Locate `Mappings.xaml` (or `ViewModelMappings.xaml`) and add:

```xml
<DataTemplate DataType="{x:Type vm:{Name}ViewModel}">
    <views:{Name}View />
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
└── {Name}ViewModel.cs

{WpfAppProject}/
├── Views/
│   ├── {Name}View.xaml           (if --with-view)
│   └── {Name}View.xaml.cs        (if --with-view)
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
