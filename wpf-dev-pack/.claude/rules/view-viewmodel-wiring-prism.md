<!--
=============================================================================
AI Agent Reading Hint — wpf-dev-pack Composition Style Anchor
=============================================================================

CANONICAL DEFINITIONS (per docs/TERMINOLOGY.md):
  - Composition Direction: View First
    (Microsoft Learn: navigation identifier = View type name string)
  - State Management: Stateful ViewModel
  - Concrete mechanism: IContainerRegistry.RegisterForNavigation<View, VM>()
                        + IRegionManager.RequestNavigate("Region", "ViewName")

PROHIBITED (per prohibitions.md):
  - prism:ViewModelLocator.AutoWireViewModel="True"                  (alternate View First mechanism, P-001-a)
  - DataContext = new XxxViewModel() in View code-behind             (P-001-b)
  - <UserControl.DataContext><vm:XxxVM /></UserControl.DataContext>  (P-001-c)
  - Other matching mechanisms (naming convention, reflection, etc.)  (P-001-d)
  - System.Windows.* types in ViewModel (except ICommand)            (P-002)
  - Transient ViewModel + external state manager pattern             (Stateless VM, P-003)
  - Mixing this Prism path with the Mappings.xaml path               (P-004)

NOTE FOR AI AGENTS:
  Earlier plugin docs (≤ v1.5.x / v1.6.4) labeled the plugin uniformly as
  "View First MVVM". For this Prism path the underlying classification (View
  First) is actually correct, but the label was applied to the CommunityToolkit
  path too where it was incorrect. v1.6.4 corrects the labels per path. The
  enforced code rules are unchanged. Use v1.6.4+ terminology in generated
  code and comments.
=============================================================================
-->

# View-ViewModel Wiring — Prism 9

Applies when Prism 9 is the active MVVM framework.
Mappings.xaml is NOT used with Prism.

> **Composition style**: View First Composition + Stateful ViewModel
> (`RegisterForNavigation` + `IRegionManager`). See [`docs/TERMINOLOGY.md`](../../docs/TERMINOLOGY.md).

---

## Registration in App.xaml.cs

```csharp
protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    containerRegistry.RegisterForNavigation<HomeView, HomeViewModel>();
    containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();
}
```

## Shell — Region Definition

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns:prism="http://prismlibrary.com/">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>

        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="10">
            <Button Content="Home" Command="{Binding NavigateCommand}"
                    CommandParameter="HomeView" Margin="5" />
            <Button Content="Settings" Command="{Binding NavigateCommand}"
                    CommandParameter="SettingsView" Margin="5" />
        </StackPanel>

        <!-- Region instead of ContentControl + DataTemplate -->
        <ContentControl Grid.Row="1"
                        prism:RegionManager.RegionName="ContentRegion" />
    </Grid>
</Window>
```

## Shell ViewModel — Navigation

```csharp
namespace MyApp.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private readonly IRegionManager _regionManager;

    public MainWindowViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }

    private DelegateCommand<string>? _navigateCommand;
    public DelegateCommand<string> NavigateCommand =>
        _navigateCommand ??= new DelegateCommand<string>(ExecuteNavigate);

    private void ExecuteNavigate(string viewName)
    {
        _regionManager.RequestNavigate("ContentRegion", viewName);
    }
}
```

## NavigationParameters

```csharp
private void NavigateToDetail(int userId)
{
    var parameters = new NavigationParameters
    {
        { "userId", userId },
        { "mode", "edit" }
    };

    _regionManager.RequestNavigate("ContentRegion", "DetailView", parameters);
}
```

## INavigationAware — Lifecycle Callbacks

```csharp
namespace MyApp.ViewModels;

public class HomeViewModel : BindableBase, INavigationAware
{
    // Called when navigated to this View
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters.ContainsKey("userId"))
        {
            var userId = navigationContext.Parameters.GetValue<int>("userId");
            LoadUser(userId);
        }
    }

    // Whether to reuse existing instance
    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }

    // Called when navigating away
    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        // Cleanup logic
    }
}
```

## IConfirmNavigationRequest — Cancel Navigation

```csharp
public class EditViewModel : BindableBase, IConfirmNavigationRequest
{
    public void ConfirmNavigationRequest(
        NavigationContext navigationContext,
        Action<bool> continuationCallback)
    {
        if (HasUnsavedChanges)
        {
            var result = MessageBox.Show(
                "You have unsaved changes. Navigate away?",
                "Confirm", MessageBoxButton.YesNo);
            continuationCallback(result == MessageBoxResult.Yes);
        }
        else
        {
            continuationCallback(true);
        }
    }

    public void OnNavigatedTo(NavigationContext ctx) { }
    public bool IsNavigationTarget(NavigationContext ctx) => true;
    public void OnNavigatedFrom(NavigationContext ctx) { }
}
```

## Comparison with CommunityToolkit.Mvvm

| Item | CommunityToolkit.Mvvm | Prism 9 |
|---|---|---|
| View-VM mapping | `DataTemplate` in Mappings.xaml | `RegisterForNavigation<V, VM>()` |
| Navigation | Replace `CurrentViewModel` property | `IRegionManager.RequestNavigate()` |
| VM auto-wiring | DataTemplate DataType matching | DI container auto-resolve |
| Parameters | Manual property assignment | `NavigationParameters` (type-safe) |
| Lifecycle | None | `INavigationAware`, `IConfirmNavigationRequest` |
| Hosting control | `ContentControl` + Binding | `ContentControl` + `RegionManager.RegionName` |
