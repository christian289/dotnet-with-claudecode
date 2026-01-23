---
name: make-wpf-project
description: "Scaffolds a complete WPF project structure with MVVM, DI, and best practices. Usage: /wpf-dev-pack:make-wpf-project <ProjectName> [--minimal|--full|--prism]"
---

# WPF Project Scaffolder

Generates WPF project structure with MVVM, DI, and best practices.

## Usage

```bash
# Default project (recommended) - CommunityToolkit.Mvvm + GenericHost
/wpf-dev-pack:make-wpf-project MyApp

# Minimal structure
/wpf-dev-pack:make-wpf-project MyApp --minimal

# Full structure (all layers separated)
/wpf-dev-pack:make-wpf-project MyApp --full

# Prism framework (module-based architecture)
/wpf-dev-pack:make-wpf-project MyApp --prism
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
MyApp/
├── MyApp.sln
├── MyApp.App/                    # WPF Application
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── GlobalUsings.cs
│   ├── Views/
│   ├── Converters/
│   └── MyApp.App.csproj
├── MyApp.ViewModels/             # ViewModel (pure C#)
│   ├── MainViewModel.cs
│   ├── GlobalUsings.cs
│   └── MyApp.ViewModels.csproj
└── MyApp.Core/                   # Business logic
    ├── Models/
    ├── Services/
    └── MyApp.Core.csproj
```

### Minimal Structure

```
MyApp/
├── MyApp.sln
└── MyApp/
    ├── App.xaml
    ├── App.xaml.cs
    ├── MainWindow.xaml
    ├── MainWindow.xaml.cs
    ├── ViewModels/
    │   └── MainViewModel.cs
    ├── Views/
    ├── Models/
    ├── Services/
    └── MyApp.csproj
```

### Full Structure

```
MyApp/
├── MyApp.slnx
├── src/
│   ├── MyApp.Abstractions/       # Interfaces, abstract classes
│   ├── MyApp.Core/               # Business logic
│   ├── MyApp.ViewModels/         # ViewModel
│   ├── MyApp.WpfServices/        # WPF services (CollectionView etc.)
│   ├── MyApp.UI/                 # CustomControl library
│   └── MyApp.App/                # WPF Application
└── tests/
    ├── MyApp.Core.Tests/
    └── MyApp.ViewModels.Tests/
```

### Prism Structure (`--prism`)

See [PRISM.md](PRISM.md) for complete structure.

---

## Generated Files

### MyApp.App.csproj

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
    <ProjectReference Include="..\MyApp.ViewModels\MyApp.ViewModels.csproj" />
    <ProjectReference Include="..\MyApp.Core\MyApp.Core.csproj" />
  </ItemGroup>

</Project>
```

### App.xaml.cs (with DI)

```csharp
namespace MyApp;

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
namespace MyApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string _title = "MyApp";
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
dotnet new sln -n MyApp

# Create projects
dotnet new wpf -n MyApp.App
dotnet new classlib -n MyApp.ViewModels
dotnet new classlib -n MyApp.Core

# Add projects to solution
dotnet sln add MyApp.App/MyApp.App.csproj
dotnet sln add MyApp.ViewModels/MyApp.ViewModels.csproj
dotnet sln add MyApp.Core/MyApp.Core.csproj

# Add project references
dotnet add MyApp.App reference MyApp.ViewModels
dotnet add MyApp.App reference MyApp.Core
dotnet add MyApp.ViewModels reference MyApp.Core

# Add packages
dotnet add MyApp.App package CommunityToolkit.Mvvm
dotnet add MyApp.App package Microsoft.Extensions.Hosting
dotnet add MyApp.ViewModels package CommunityToolkit.Mvvm
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
