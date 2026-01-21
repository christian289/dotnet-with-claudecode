# WpfDISample Solution Creation Script
#
# Usage:
#   .\Create-WpfDISample.ps1
#
# Requirements:
#   - .NET 10 SDK

$ErrorActionPreference = "Stop"
$projectName = "WpfDISample"

Write-Host "=== Creating $projectName Solution ===" -ForegroundColor Cyan

# 1. Create solution and projects
Write-Host "`n[1/9] Creating solution..." -ForegroundColor Yellow
dotnet new sln -n $projectName

Write-Host "`n[2/9] Creating WPF Application project..." -ForegroundColor Yellow
dotnet new wpf -n "$projectName.App"

Write-Host "`n[3/9] Creating Class Library project..." -ForegroundColor Yellow
dotnet new classlib -n "$projectName.ViewModels" -f net10.0

# 2. Add projects to solution
Write-Host "`n[4/9] Adding projects to solution..." -ForegroundColor Yellow
dotnet sln "$projectName.sln" add "$projectName.App/$projectName.App.csproj"
dotnet sln "$projectName.sln" add "$projectName.ViewModels/$projectName.ViewModels.csproj"

# 3. Add NuGet packages
Write-Host "`n[5/9] Adding NuGet packages..." -ForegroundColor Yellow
dotnet add "$projectName.App" package CommunityToolkit.Mvvm
dotnet add "$projectName.App" package Microsoft.Extensions.DependencyInjection
dotnet add "$projectName.App" package Microsoft.Extensions.Hosting
dotnet add "$projectName.ViewModels" package CommunityToolkit.Mvvm

# 4. Add project reference
Write-Host "`n[6/9] Adding project reference..." -ForegroundColor Yellow
dotnet add "$projectName.App" reference "$projectName.ViewModels/$projectName.ViewModels.csproj"

# 5. Remove default files and create directories
Write-Host "`n[7/9] Cleaning default files and creating directories..." -ForegroundColor Yellow
Remove-Item "$projectName.App/MainWindow.xaml" -ErrorAction SilentlyContinue
Remove-Item "$projectName.App/MainWindow.xaml.cs" -ErrorAction SilentlyContinue
Remove-Item "$projectName.ViewModels/Class1.cs" -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path "$projectName.App/Views" -Force | Out-Null

# 6. Create custom files
Write-Host "`n[8/9] Creating custom files..." -ForegroundColor Yellow

# App.xaml (Remove StartupUri for DI)
@'
<Application x:Class="WpfDISample.App.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
    </Application.Resources>
</Application>
'@ | Set-Content "$projectName.App/App.xaml" -Encoding UTF8

# App.xaml.cs (GenericHost configuration)
@'
namespace WpfDISample.App;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        // Create GenericHost and register services
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register ViewModels
                services.AddTransient<MainViewModel>();

                // Register Views
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Get MainWindow from ServiceProvider
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }

        base.OnExit(e);
    }
}
'@ | Set-Content "$projectName.App/App.xaml.cs" -Encoding UTF8

# WpfDISample.App/GlobalUsings.cs
@'
global using System;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Windows;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using WpfDISample.ViewModels;
global using WpfDISample.App.Views;
'@ | Set-Content "$projectName.App/GlobalUsings.cs" -Encoding UTF8

# Views/MainWindow.xaml
@'
<Window x:Class="WpfDISample.App.Views.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:viewmodels="clr-namespace:WpfDISample.ViewModels;assembly=WpfDISample.ViewModels"
        mc:Ignorable="d"
        d:DataContext="{d:DesignInstance Type=viewmodels:MainViewModel}"
        Title="WPF DI Sample"
        Width="400"
        Height="300"
        WindowStartupLocation="CenterScreen">

    <Grid Margin="20">
        <StackPanel VerticalAlignment="Center"
                    HorizontalAlignment="Center">
            <TextBlock Text="{Binding Message}"
                       FontSize="24"
                       FontWeight="Bold"
                       HorizontalAlignment="Center" />
            <Button Content="Click Me"
                    Margin="0,20,0,0"
                    Padding="20,10"
                    Command="{Binding ChangeMessageCommand}" />
        </StackPanel>
    </Grid>
</Window>
'@ | Set-Content "$projectName.App/Views/MainWindow.xaml" -Encoding UTF8

# Views/MainWindow.xaml.cs
@'
namespace WpfDISample.App.Views;

public partial class MainWindow : Window
{
    // ViewModel injection through Constructor Injection
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
'@ | Set-Content "$projectName.App/Views/MainWindow.xaml.cs" -Encoding UTF8

# WpfDISample.ViewModels/GlobalUsings.cs
@'
global using System;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.Linq;
global using System.Threading.Tasks;
global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Input;
'@ | Set-Content "$projectName.ViewModels/GlobalUsings.cs" -Encoding UTF8

# MainViewModel.cs
@'
namespace WpfDISample.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string _message = "Hello, Dependency Injection!";

    [RelayCommand]
    private void ChangeMessage()
    {
        Message = $"Button clicked at {DateTime.Now:HH:mm:ss}";
    }
}
'@ | Set-Content "$projectName.ViewModels/MainViewModel.cs" -Encoding UTF8

# 7. Migrate sln to slnx and remove old sln
Write-Host "`n[9/9] Migrating to slnx format..." -ForegroundColor Yellow
dotnet sln "$projectName.sln" migrate
Remove-Item "$projectName.sln" -ErrorAction SilentlyContinue

# 8. Verify build
Write-Host "`n=== Verifying Build ===" -ForegroundColor Cyan
dotnet build "$projectName.slnx"

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n=== $projectName Solution Created Successfully! ===" -ForegroundColor Green
    Write-Host "Run: dotnet run --project $projectName.App" -ForegroundColor White
} else {
    Write-Host "`n=== Build Failed ===" -ForegroundColor Red
}
