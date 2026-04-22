---
name: configuring-avalonia-dependency-injection
description: "Configures GenericHost and Dependency Injection in AvaloniaUI applications. Registers service lifetimes (singleton, transient, scoped), wires ViewModels to Views, and sets up hosted services. Use when setting up DI container, registering services, or implementing IoC patterns in AvaloniaUI projects."
---

# AvaloniaUI Dependency Injection with GenericHost

Apply the same GenericHost pattern in AvaloniaUI as in WPF.

## Setup Workflow

1. Add NuGet packages to the App project
2. Configure `App.axaml.cs` with GenericHost
3. Register services and ViewModels
4. Use constructor injection in Views and ViewModels
5. Verify: app launches and resolves dependencies without exceptions

## Required NuGet Packages

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.*" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.*" />
</ItemGroup>
```

## App.axaml.cs — GenericHost Configuration

```csharp
namespace MyApp;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public partial class App : Application
{
    private IHost? _host;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IUserRepository, UserRepository>();
                services.AddSingleton<IUserService, UserService>();
                services.AddTransient<IDialogService, DialogService>();

                services.AddTransient<MainViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = _host.Services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
```

## Constructor Injection in Views

```csharp
namespace MyApp.Views;

using Avalonia.Controls;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
```

## Template Project

The `templates/` folder contains a complete .NET 9 AvaloniaUI DI example project with App, Views, and ViewModels.
