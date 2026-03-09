# Prism Framework Project Structure

Prism module-based architecture for large-scale WPF applications.

## Prism Structure (`--prism`)

```
MyApp/
├── MyApp.sln
├── MyApp/                        # Shell Application
│   ├── App.xaml
│   ├── App.xaml.cs               # PrismApplication
│   ├── MainWindow.xaml           # Shell (Region Host)
│   ├── MainWindow.xaml.cs
│   ├── GlobalUsings.cs
│   └── MyApp.csproj
├── MyApp.Core/                   # Shared interfaces, models
│   ├── Mvvm/
│   │   └── RegionNames.cs
│   ├── Models/
│   ├── Services/
│   └── MyApp.Core.csproj
└── MyApp.Modules.Home/           # Feature Module
    ├── HomeModule.cs             # IModule implementation
    ├── ViewModels/
    │   └── HomeViewModel.cs
    ├── Views/
    │   └── HomeView.xaml
    └── MyApp.Modules.Home.csproj
```

---

## Generated Files

### MyApp.csproj (Shell)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Prism.DryIoc" Version="9.0.537" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MyApp.Core\MyApp.Core.csproj" />
    <ProjectReference Include="..\MyApp.Modules.Home\MyApp.Modules.Home.csproj" />
  </ItemGroup>

</Project>
```

### App.xaml (Prism)

```xml
<prism:PrismApplication x:Class="MyApp.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:prism="http://prismlibrary.com/">
    <Application.Resources>
        <!-- Styles, Themes -->
    </Application.Resources>
</prism:PrismApplication>
```

### App.xaml.cs (Prism)

```csharp
namespace MyApp;

public partial class App : PrismApplication
{
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Register services
        containerRegistry.RegisterSingleton<IMessageService, MessageService>();
    }

    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        // Register modules
        moduleCatalog.AddModule<HomeModule>();
    }
}
```

### MainWindow.xaml (Shell with Region)

```xml
<Window x:Class="MyApp.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:prism="http://prismlibrary.com/"
    Title="MyApp" Height="600" Width="800">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Navigation -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="10">
            <Button Content="Home" Command="{Binding NavigateCommand}"
                    CommandParameter="HomeView" Margin="5"/>
        </StackPanel>

        <!-- Main Content Region -->
        <ContentControl Grid.Row="1"
                        prism:RegionManager.RegionName="ContentRegion"/>
    </Grid>
</Window>
```

### RegionNames.cs

```csharp
namespace MyApp.Core.Mvvm;

public static class RegionNames
{
    public const string ContentRegion = "ContentRegion";
    public const string NavigationRegion = "NavigationRegion";
    public const string StatusBarRegion = "StatusBarRegion";
}
```

### HomeModule.cs

```csharp
namespace MyApp.Modules.Home;

public class HomeModule : IModule
{
    private readonly IRegionManager _regionManager;

    public HomeModule(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        // Register initial View to Region
        _regionManager.RequestNavigate(RegionNames.ContentRegion, "HomeView");
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Register View-ViewModel
        containerRegistry.RegisterForNavigation<HomeView, HomeViewModel>();
    }
}
```

### HomeViewModel.cs (Prism)

```csharp
namespace MyApp.Modules.Home.ViewModels;

public class HomeViewModel : BindableBase, INavigationAware
{
    private string _title = "Home";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private readonly IMessageService _messageService;

    public HomeViewModel(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public DelegateCommand LoadCommand => new(async () =>
    {
        await Task.Delay(1000);
        _messageService.Show("Data loaded");
    });

    #region INavigationAware

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        // Called when navigated to this View
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true; // Reuse existing instance
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        // Called when navigating away from this View
    }

    #endregion
}
```

### GlobalUsings.cs (Prism)

```csharp
global using System;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Windows;
global using Prism.Commands;
global using Prism.Ioc;
global using Prism.Modularity;
global using Prism.Mvvm;
global using Prism.Navigation.Regions;
global using Prism.DryIoc;
global using MyApp.Core.Mvvm;
```

---

## CLI Commands (Prism)

```bash
# Create solution
dotnet new sln -n MyApp

# Create projects
dotnet new wpf -n MyApp
dotnet new classlib -n MyApp.Core
dotnet new classlib -n MyApp.Modules.Home

# Add WPF support to module (manually edit csproj for Windows)
# Change <TargetFramework>net10.0</TargetFramework> to net10.0-windows
# Add <UseWPF>true</UseWPF>

# Add projects to solution
dotnet sln add MyApp/MyApp.csproj
dotnet sln add MyApp.Core/MyApp.Core.csproj
dotnet sln add MyApp.Modules.Home/MyApp.Modules.Home.csproj

# Add project references
dotnet add MyApp reference MyApp.Core
dotnet add MyApp reference MyApp.Modules.Home
dotnet add MyApp.Modules.Home reference MyApp.Core

# Add packages
dotnet add MyApp package Prism.DryIoc --version 9.0.537
dotnet add MyApp.Core package Prism.Core --version 9.0.537
dotnet add MyApp.Modules.Home package Prism.DryIoc --version 9.0.537
dotnet add MyApp.Modules.Home package Prism.Wpf --version 9.0.537
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

## License Notice (Prism 9+)

> ⚠️ Prism 9부터 **듀얼 라이선스** 모델 적용

| License | 대상 | 비용 |
|---------|------|------|
| **Community** (무료) | 연매출 100만 USD 미만 또는 외부 투자 300만 USD 미만 | 무료 |
| **Commercial Plus** | 대규모 조직, 상용 지원 | $499/개발자/년 |

---

## IDialogService (Prism 9)

### Dialog ViewModel

```csharp
namespace MyApp.Modules.Home.ViewModels;

public class ConfirmDialogViewModel : BindableBase, IDialogAware
{
    public string Title => "Confirm";

    // Prism 9: DialogCloseListener (속성)
    // Prism 9: DialogCloseListener (property)
    public DialogCloseListener RequestClose { get; }

    public DelegateCommand ConfirmCommand => new(() =>
    {
        RequestClose.Invoke(new DialogResult(ButtonResult.OK));
    });

    public DelegateCommand CancelCommand => new(() =>
    {
        RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
    });

    public bool CanCloseDialog() => true;

    public void OnDialogClosed() { }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        // 파라미터 수신
        // Receive parameters
    }
}
```

### Dialog 등록 (Module)

```csharp
public void RegisterTypes(IContainerRegistry containerRegistry)
{
    containerRegistry.RegisterDialog<ConfirmDialogView, ConfirmDialogViewModel>();
}
```

### Dialog 호출 (ViewModel)

```csharp
private readonly IDialogService _dialogService;

private void ShowConfirm()
{
    _dialogService.ShowDialog("ConfirmDialogView", new DialogParameters(), result =>
    {
        if (result.Result == ButtonResult.OK)
        {
            // 확인 처리
            // Handle confirmation
        }
    });
}
```

---

## IEventAggregator

모듈 간 느슨한 결합 통신.

### Event 정의 (Core)

```csharp
namespace MyApp.Core.Events;

public class UserSelectedEvent : PubSubEvent<int> { }
```

### Event 발행

```csharp
private readonly IEventAggregator _eventAggregator;

private void SelectUser(int userId)
{
    _eventAggregator.GetEvent<UserSelectedEvent>().Publish(userId);
}
```

### Event 구독

```csharp
public HomeViewModel(IEventAggregator eventAggregator)
{
    eventAggregator.GetEvent<UserSelectedEvent>()
        .Subscribe(OnUserSelected, ThreadOption.UIThread);
}

private void OnUserSelected(int userId)
{
    // 사용자 선택 처리
    // Handle user selection
}
```
