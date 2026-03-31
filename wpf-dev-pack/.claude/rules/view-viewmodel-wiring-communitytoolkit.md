# View-ViewModel Wiring — CommunityToolkit.Mvvm

Applies when CommunityToolkit.Mvvm is the active MVVM framework.

---

## Core Principles

1. `DataTemplate` must be defined **without `x:Key`** — only `DataType` — for automatic type matching.
2. `Mappings.xaml` must be merged into `Application.Resources`.
3. ViewModel **instances** (not types) are bound to `ContentControl.Content`.
4. Views automatically receive the ViewModel as their `DataContext` — no separate wiring needed.
5. ViewModel type matching is exact — inheritance is not considered.

---

## Mappings.xaml

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:vm="clr-namespace:MyApp.ViewModels;assembly=MyApp.ViewModels"
                    xmlns:views="clr-namespace:MyApp.Views">

    <DataTemplate DataType="{x:Type vm:HomeViewModel}">
        <views:HomeView />
    </DataTemplate>

    <DataTemplate DataType="{x:Type vm:SettingsViewModel}">
        <views:SettingsView />
    </DataTemplate>

</ResourceDictionary>
```

## App.xaml — Merge Mappings.xaml

```xml
<Application x:Class="MyApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Mappings.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

## Navigation via CurrentViewModel Property

```csharp
namespace MyApp.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private object? _currentViewModel;

    public MainWindowViewModel()
    {
        CurrentViewModel = new HomeViewModel();
    }

    [RelayCommand]
    private void NavigateToHome()
    {
        CurrentViewModel = new HomeViewModel();
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentViewModel = new SettingsViewModel();
    }
}
```

## MainWindow.xaml — ContentControl Binding

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>

        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="10">
            <Button Content="Home" Command="{Binding NavigateToHomeCommand}" Margin="5" />
            <Button Content="Settings" Command="{Binding NavigateToSettingsCommand}" Margin="5" />
        </StackPanel>

        <!-- ViewModel bound to Content → DataTemplate auto-renders the View -->
        <ContentControl Grid.Row="1" Content="{Binding CurrentViewModel}" />
    </Grid>
</Window>
```

```csharp
namespace MyApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}
```

## View Implementation

Views are UserControls. Use `d:DataContext` for design-time support.

```xml
<UserControl x:Class="MyApp.Views.HomeView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:vm="clr-namespace:MyApp.ViewModels;assembly=MyApp.ViewModels"
             mc:Ignorable="d"
             d:DataContext="{d:DesignInstance Type=vm:HomeViewModel}">
    <Grid>
        <TextBlock Text="{Binding WelcomeMessage}" FontSize="24"
                   HorizontalAlignment="Center" VerticalAlignment="Center" />
    </Grid>
</UserControl>
```

## HierarchicalDataTemplate for TreeView

For recursive data (folder trees, org charts), use `HierarchicalDataTemplate`:

```xml
<HierarchicalDataTemplate DataType="{x:Type vm:FolderViewModel}"
                          ItemsSource="{Binding Children}">
    <StackPanel Orientation="Horizontal">
        <Image Source="/Icons/folder.png" Width="16" Height="16" Margin="0,0,5,0" />
        <TextBlock Text="{Binding Name}" VerticalAlignment="Center" />
    </StackPanel>
</HierarchicalDataTemplate>

<!-- Leaf nodes use plain DataTemplate -->
<DataTemplate DataType="{x:Type vm:FileViewModel}">
    <StackPanel Orientation="Horizontal">
        <Image Source="/Icons/file.png" Width="16" Height="16" />
        <TextBlock Text="{Binding FileName}" Margin="5,0" />
    </StackPanel>
</DataTemplate>
```

| DataTemplate | HierarchicalDataTemplate |
|---|---|
| Flat data | Hierarchical/nested data |
| No children | Has `ItemsSource` for children |
| `ContentControl` | `TreeView`, `Menu`, `MenuItem` |

## Project Structure

```
MyApp.WpfApp/
├── Views/
│   ├── HomeView.xaml
│   ├── HomeView.xaml.cs
│   ├── SettingsView.xaml
│   └── SettingsView.xaml.cs
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
└── Mappings.xaml              ← DataTemplate definitions

MyApp.ViewModels/
├── MainWindowViewModel.cs
├── HomeViewModel.cs
└── SettingsViewModel.cs
```
