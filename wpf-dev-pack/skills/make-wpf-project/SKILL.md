---
description: "Scaffolds a complete WPF project structure with MVVM, DI, and best practices. Use when starting a new WPF application, creating a WPF solution from scratch, or setting up project structure with CommunityToolkit.Mvvm or Prism. Usage: /wpf-dev-pack:make-wpf-project <ProjectName> [--minimal|--full|--prism]"
argument-hint: [ProjectName]
---

# WPF Project Scaffolder

**If `$0` is empty, use the AskUserQuestion tool to ask: "Enter the WPF project name (e.g., MyApp, Dashboard)". Do NOT proceed until a valid name is provided. Use the response as the ProjectName for all subsequent steps.**

Scaffold a WPF project named `$0` with MVVM, DI, and best practices.

## Usage

```bash
# Default project (recommended) - CommunityToolkit.Mvvm + GenericHost
/wpf-dev-pack:make-wpf-project $0

# Minimal structure
/wpf-dev-pack:make-wpf-project $0 --minimal

# Full structure (all layers separated)
/wpf-dev-pack:make-wpf-project $0 --full

# Prism framework (module-based architecture)
/wpf-dev-pack:make-wpf-project $0 --prism
```

### Framework Options

| Option | Framework | Features |
|--------|-----------|----------|
| (default) | CommunityToolkit.Mvvm | Lightweight, Source Generator, GenericHost DI |
| `--prism` | Prism.DryIoc | Region Navigation, Module, Dialog Service |

> **Prism details**: See [PRISM.md](PRISM.md) for complete Prism project structure and examples.

---

## Project Structures

### Default Structure

```
$0/
├── $0.sln
├── $0.App/                    # WPF Application
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── GlobalUsings.cs
│   ├── Views/
│   ├── Converters/
│   └── $0.App.csproj
├── $0.ViewModels/             # ViewModel (pure C#)
│   ├── MainViewModel.cs
│   ├── GlobalUsings.cs
│   └── $0.ViewModels.csproj
└── $0.Core/                   # Business logic
    ├── Models/
    ├── Services/
    └── $0.Core.csproj
```

### Minimal Structure

```
$0/
├── $0.sln
└── $0/
    ├── App.xaml
    ├── App.xaml.cs
    ├── MainWindow.xaml
    ├── MainWindow.xaml.cs
    ├── ViewModels/
    │   └── MainViewModel.cs
    ├── Views/
    ├── Models/
    ├── Services/
    └── $0.csproj
```

### Full Structure

```
$0/
├── $0.slnx
├── src/
│   ├── $0.Abstractions/       # Interfaces, abstract classes
│   ├── $0.Core/               # Business logic
│   ├── $0.ViewModels/         # ViewModel
│   ├── $0.WpfServices/        # WPF services (CollectionView etc.)
│   ├── $0.UI/                 # CustomControl library
│   └── $0.App/                # WPF Application
└── tests/
    ├── $0.Core.Tests/
    └── $0.ViewModels.Tests/
```

### Prism Structure (`--prism`)

See [PRISM.md](PRISM.md) for complete structure.

---

## Generated Files

### $0.App.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
    <ApplicationIcon>Resources\app.ico</ApplicationIcon>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.*" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\$0.ViewModels\$0.ViewModels.csproj" />
    <ProjectReference Include="..\$0.Core\$0.Core.csproj" />
  </ItemGroup>

</Project>
```

### App.xaml.cs (with DI)

```csharp
namespace $0;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register services
                services.AddSingleton<IDialogService, DialogService>();

                // Register ViewModels
                services.AddSingleton<MainViewModel>();

                // Register Views
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}
```

### MainViewModel.cs

```csharp
namespace $0.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string _title = "$0";
    [ObservableProperty] private string _statusMessage = "Ready";

    private readonly IDialogService _dialogService;

    public MainViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        StatusMessage = "Loading...";

        try
        {
            // TODO: Load data
            await Task.Delay(1000);
            StatusMessage = "Data loaded successfully";
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Error", ex.Message);
        }
    }
}
```

### GlobalUsings.cs

```csharp
global using System;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Windows;
global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Input;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
```

---

## CLI Commands

Commands to run after project generation:

```bash
# Create solution
dotnet new sln -n $0

# Create projects
dotnet new wpf -n $0.App
dotnet new classlib -n $0.ViewModels
dotnet new classlib -n $0.Core

# Add projects to solution
dotnet sln add $0.App/$0.App.csproj
dotnet sln add $0.ViewModels/$0.ViewModels.csproj
dotnet sln add $0.Core/$0.Core.csproj

# Add project references
dotnet add $0.App reference $0.ViewModels
dotnet add $0.App reference $0.Core
dotnet add $0.ViewModels reference $0.Core

# Add packages
dotnet add $0.App package CommunityToolkit.Mvvm
dotnet add $0.App package Microsoft.Extensions.Hosting
dotnet add $0.ViewModels package CommunityToolkit.Mvvm
```

---

## Prism vs CommunityToolkit.Mvvm

| Feature | CommunityToolkit.Mvvm | Prism |
|---------|----------------------|-------|
| Base Class | `ObservableObject` | `BindableBase` |
| Command | `RelayCommand` (Source Gen) | `DelegateCommand` |
| DI Container | GenericHost (any) | DryIoc, Unity, etc. |
| Navigation | Manual implementation | `IRegionManager` |
| Dialog | Manual implementation | `IDialogService` |
| Module | Not supported | `IModule` |
| Best for | Small-Medium apps | Medium-Large apps |

---

## Next Steps

1. **Add View**: `/wpf-dev-pack:make-wpf-usercontrol`
2. **Add CustomControl**: `/wpf-dev-pack:make-wpf-custom-control`
3. **Add Converter**: `/wpf-dev-pack:make-wpf-converter`

---

## Related Skills

- `structuring-wpf-projects` - Project structure detailed guide
- `configuring-dependency-injection` - DI setup detailed guide
- `implementing-communitytoolkit-mvvm` - MVVM pattern detailed guide
