# View-ViewModel Mapping - Prism 9 Version

> **MVVM Framework Rule**: `.claude/rules/dotnet/wpf/prism9.md` 규칙이 적용됩니다.
> CommunityToolkit.Mvvm 사용 시 → [SKILL.md](SKILL.md) 참조

Prism 9 (Community License) 기준 네비게이션 패턴. SKILL.md의 DataTemplate 매핑 대응.

## CommunityToolkit.Mvvm vs Prism 비교

| 항목 | CommunityToolkit.Mvvm | Prism 9 |
|------|----------------------|---------|
| View-VM 매핑 | `DataTemplate` (Mappings.xaml) | `RegisterForNavigation<V, VM>()` |
| 네비게이션 | `CurrentViewModel` 속성 교체 | `IRegionManager.RequestNavigate()` |
| VM 자동 연결 | DataTemplate DataType 매칭 | `ViewModelLocator.AutoWireViewModel` |
| 네비게이션 파라미터 | 수동 속성 설정 | `NavigationParameters` |
| 네비게이션 수명 제어 | 없음 | `INavigationAware`, `IConfirmNavigationRequest` |
| 호스팅 컨트롤 | `ContentControl` + Binding | `ContentControl` + `RegionManager.RegionName` |

## Prerequisites

```xml
<PackageReference Include="Prism.DryIoc" Version="9.0.537" />
```

## 1. RegionManager 네비게이션

### Shell (MainWindow.xaml)

```xml
<Window x:Class="MyApp.MainWindow"
    xmlns:prism="http://prismlibrary.com/"
    prism:ViewModelLocator.AutoWireViewModel="True">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- 네비게이션 버튼 -->
        <!-- Navigation buttons -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="10">
            <Button Content="Home" Command="{Binding NavigateCommand}"
                    CommandParameter="HomeView" Margin="5"/>
            <Button Content="Settings" Command="{Binding NavigateCommand}"
                    CommandParameter="SettingsView" Margin="5"/>
        </StackPanel>

        <!-- DataTemplate의 ContentControl 대신 Region 사용 -->
        <!-- Use Region instead of ContentControl with DataTemplate -->
        <ContentControl Grid.Row="1"
                        prism:RegionManager.RegionName="ContentRegion"/>
    </Grid>
</Window>
```

### Shell ViewModel

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

## 2. RegisterForNavigation (View-ViewModel 매핑)

```csharp
// DataTemplate 매핑 (Mappings.xaml) 대신:
// Instead of DataTemplate mapping (Mappings.xaml):
// <DataTemplate DataType="{x:Type vm:HomeViewModel}">
//     <views:HomeView />
// </DataTemplate>

// Prism 9: 모듈에서 등록
// Prism 9: Register in module
public void RegisterTypes(IContainerRegistry containerRegistry)
{
    containerRegistry.RegisterForNavigation<HomeView, HomeViewModel>();
    containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();
}
```

## 3. ViewModelLocator.AutoWireViewModel

```xml
<!-- 각 View에 추가하면 자동으로 ViewModel이 DataContext에 설정됨 -->
<!-- Add to each View for automatic ViewModel DataContext wiring -->
<UserControl x:Class="MyApp.Modules.Home.Views.HomeView"
    xmlns:prism="http://prismlibrary.com/"
    prism:ViewModelLocator.AutoWireViewModel="True">
    <!-- Content -->
</UserControl>
```

**명명 규칙**: `HomeView` → `HomeViewModel` 자동 매핑 (Views ↔ ViewModels 네임스페이스)

## 4. INavigationAware

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

    // View로 네비게이션될 때 호출
    // Called when navigated to this View
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        // 파라미터 수신
        // Receive parameters
        if (navigationContext.Parameters.ContainsKey("userId"))
        {
            var userId = navigationContext.Parameters.GetValue<int>("userId");
            LoadUser(userId);
        }
    }

    // 기존 인스턴스 재사용 여부
    // Whether to reuse existing instance
    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }

    // 다른 View로 이동할 때 호출
    // Called when navigating away
    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        // 정리 로직
        // Cleanup logic
    }
}
```

## 5. NavigationParameters

```csharp
// 파라미터 전달
// Pass parameters
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

## 6. IConfirmNavigationRequest

```csharp
// 네비게이션 전 확인 (미저장 변경사항 등)
// Confirm before navigation (unsaved changes, etc.)
public class EditViewModel : BindableBase, IConfirmNavigationRequest
{
    public void ConfirmNavigationRequest(
        NavigationContext navigationContext,
        Action<bool> continuationCallback)
    {
        if (HasUnsavedChanges)
        {
            var result = MessageBox.Show(
                "저장하지 않은 변경사항이 있습니다. 이동하시겠습니까?",
                // You have unsaved changes. Do you want to navigate away?
                "확인",
                MessageBoxButton.YesNo);

            continuationCallback(result == MessageBoxResult.Yes);
        }
        else
        {
            continuationCallback(true);
        }
    }

    // INavigationAware 멤버 생략
    // INavigationAware members omitted
    public void OnNavigatedTo(NavigationContext ctx) { }
    public bool IsNavigationTarget(NavigationContext ctx) => true;
    public void OnNavigatedFrom(NavigationContext ctx) { }
}
```

## Key Differences from SKILL.md

- **Mappings.xaml 불필요**: DataTemplate 매핑 대신 `RegisterForNavigation`으로 코드에서 등록
- **RegionManager**: `ContentControl.Content` 바인딩 대신 `RegionManager.RegionName` attached property
- **ViewModelLocator**: DataTemplate의 자동 타입 매칭 대신 명명 규칙 기반 자동 연결
- **NavigationParameters**: ViewModel 속성 직접 설정 대신 타입 안전한 파라미터 전달
- **INavigationAware**: 네비게이션 수명주기 콜백 (DataTemplate에는 없는 기능)
- **IConfirmNavigationRequest**: 네비게이션 취소 지원 (미저장 변경사항 확인 등)
