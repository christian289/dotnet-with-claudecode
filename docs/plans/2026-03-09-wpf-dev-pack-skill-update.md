# wpf-dev-pack 스킬 교정 및 신규 추가 구현 계획

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** wpf-dev-pack의 기존 스킬(CommunityToolkit.Mvvm, Prism)을 최신 API에 맞게 교정하고, 누락된 오픈소스 라이브러리(WPF-UI, LiveCharts2, FluentValidation, ErrorOr) 스킬을 추가하며, wpf-architect 에이전트의 인터뷰 시스템을 7단계로 확장하여 사용자와의 긴밀한 인터랙션을 통해 WPF 코드 생성 정확도를 높인다.

**Architecture:** 기존 스킬 2개 교정 + 신규 스킬 4개 추가 + 에이전트 인터뷰 4→7단계 확장. 각 스킬은 독립적으로 500줄 이내 SKILL.md를 가지며, skills/.claude/CLAUDE.md의 키워드 매핑에 등록한다. 인터뷰 Step 2(프로그램 컨셉)에서 키워드를 분석하여 후속 단계의 기본값을 자동 설정한다.

**Tech Stack:** CommunityToolkit.Mvvm 8.4.0+, Prism 9.0.537, WPF-UI 4.2.0, LiveCharts2 2.0.0-rc6.1, FluentValidation 12.1.1, ErrorOr 2.0.1

---

## Task 1: CommunityToolkit.Mvvm 스킬 교정 (implementing-communitytoolkit-mvvm)

**Files:**
- Modify: `wpf-dev-pack/skills/implementing-communitytoolkit-mvvm/SKILL.md`

**변경 요약:**
- 8.4.0 partial property 문법을 기본 패턴으로 변경
- field 기반 문법을 Legacy 섹션으로 이동
- MVVMTK0045 경고 문서화
- `required`, `private set`, `override` modifier 예시 추가
- `.csproj` LangVersion 요구사항 명시

**Step 1: SKILL.md 교정 내용 작성**

```markdown
---
name: implementing-communitytoolkit-mvvm
description: "Implements MVVM pattern using CommunityToolkit.Mvvm 8.4+ with partial property syntax and source generators. Use when building ViewModels, implementing commands, or migrating from field-based ObservableProperty in WPF."
---

# CommunityToolkit.Mvvm Code Guidelines

CommunityToolkit.Mvvm 8.4+ 기반 MVVM 패턴 구현 가이드.

## Prerequisites

```xml
<!-- .csproj 필수 설정 -->
<PropertyGroup>
  <LangVersion>preview</LangVersion>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.*" />
</ItemGroup>
```

- **.NET 9+ SDK** 필수 (partial property 지원)
- `LangVersion` preview 설정 필수

## Project Structure

```
MyApp.ViewModels/    ← ViewModel Class Library (UI framework independent)
├── UserViewModel.cs
├── GlobalUsings.cs
└── MyApp.ViewModels.csproj
```

- ViewModel 프로젝트에 `System.Windows` 참조 금지
- BCL 타입 + CommunityToolkit.Mvvm만 참조

## ObservableProperty (Partial Property Syntax) — 기본

8.4.0부터 partial property가 **기본 권장 패턴**입니다.

### Single Attribute — Write Inline

```csharp
// ✅ Good: Partial property (8.4+ 기본)
[ObservableProperty] public partial string UserName { get; set; }

[ObservableProperty] public partial int Age { get; set; }

[ObservableProperty] public partial bool IsActive { get; set; }
```

### Multiple Attributes — ObservableProperty Always Inline

```csharp
// ✅ Good: Multiple attributes, ObservableProperty always inline
[NotifyPropertyChangedRecipients]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
[ObservableProperty] public partial string Email { get; set; }

[NotifyDataErrorInfo]
[Required(ErrorMessage = "Name is required.")]
[MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
[ObservableProperty] public partial string Name { get; set; }

[NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
[NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
[ObservableProperty] public partial User? SelectedUser { get; set; }
```

### Custom Access Modifiers

```csharp
// private set (외부 읽기 전용)
[ObservableProperty] public partial string Status { get; private set; }

// required (생성 시 필수 초기화)
[ObservableProperty] public required partial string Id { get; set; }

// override (상속 시 재정의)
[ObservableProperty] public override partial string DisplayName { get; set; }
```

### Bad Example

```csharp
// ❌ Bad: ObservableProperty on separate line
[NotifyPropertyChangedRecipients]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
[ObservableProperty]
public partial string Email { get; set; }
```

## Complete ViewModel Example

```csharp
namespace MyApp.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public sealed partial class UserViewModel : ObservableObject
{
    // Single attribute
    [ObservableProperty] public partial string FirstName { get; set; }
    [ObservableProperty] public partial string LastName { get; set; }
    [ObservableProperty] public partial int Age { get; set; }

    // Multiple attributes — ObservableProperty inline
    [NotifyPropertyChangedRecipients]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [ObservableProperty] public partial string Email { get; set; }

    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
    [ObservableProperty] public partial User? SelectedUser { get; set; }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        // Save logic
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Email);
}
```

## Key Rules

- **Partial Property가 기본**: `[ObservableProperty] public partial string Name { get; set; }`
- **Single Attribute**: `[ObservableProperty]`를 프로퍼티 선언과 같은 줄에 inline 작성
- **Multiple Attributes**: 다른 속성은 별도 줄, `[ObservableProperty]`는 **항상 마지막 줄에 inline**
- **Access Modifiers**: `private set`, `required`, `override`, `sealed`, `new` 지원
- **Analyzer**: MVVMTK0045 경고 발생 시 field → partial property로 마이그레이션

## MVVMTK0045 Warning

field 기반 `[ObservableProperty]` 사용 시 **MVVMTK0045** 경고 발생:
> "Using [ObservableProperty] on fields is not AOT compatible in WinRT scenarios"

**해결**: Visual Studio Code Fixer로 자동 변환 가능 (Ctrl+. → "Use partial property")

## Legacy: Field-Based Syntax (.NET 8 이하)

.NET 8 이하 또는 C# 13 미만에서는 field 기반 문법 사용:

```csharp
// .NET 8 이하 전용 — 새 프로젝트에서는 partial property 사용
[ObservableProperty] private string _userName = string.Empty;
[ObservableProperty] private int _age;
```

**주의**: field 기반은 AOT 비호환이며, 8.4.0부터 MVVMTK0045 경고 발생.
```

**Step 2: 파일 저장 후 diff 확인**

Run: `git diff wpf-dev-pack/skills/implementing-communitytoolkit-mvvm/SKILL.md`

**Step 3: Commit**

```bash
git add wpf-dev-pack/skills/implementing-communitytoolkit-mvvm/SKILL.md
git commit -m "fix(wpf-dev-pack): update CommunityToolkit.Mvvm skill to 8.4+ partial property syntax"
```

---

## Task 2: Prism PRISM.md 교정

**Files:**
- Modify: `wpf-dev-pack/commands/make-wpf-project/PRISM.md`

**변경 요약:**
- IDialogService + DialogCloseListener (Prism 9 신규) 패턴 추가
- IEventAggregator 모듈 간 통신 패턴 추가
- 듀얼 라이선스 안내 추가
- 버전 명시 (`9.0.537`)

**Step 1: PRISM.md 교정 내용 작성**

기존 파일 끝(`## Prism vs CommunityToolkit.Mvvm` 테이블 뒤)에 다음 섹션들을 추가:

```markdown
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
```

기존 버전 와일드카드도 교정:

- `Prism.DryIoc Version="9.0.*"` → `Prism.DryIoc Version="9.0.537"`
- `Prism.Core` → `Prism.Core Version="9.0.537"`
- `Prism.Wpf` → `Prism.Wpf Version="9.0.537"`

**Step 2: 파일 저장 후 diff 확인**

Run: `git diff wpf-dev-pack/commands/make-wpf-project/PRISM.md`

**Step 3: Commit**

```bash
git add wpf-dev-pack/commands/make-wpf-project/PRISM.md
git commit -m "fix(wpf-dev-pack): update Prism skill to 9.0.537 with DialogService and EventAggregator"
```

---

## Task 3: WPF-UI 신규 스킬 (integrating-wpfui-fluent)

**Files:**
- Create: `wpf-dev-pack/skills/integrating-wpfui-fluent/SKILL.md`

**Step 1: SKILL.md 작성**

```markdown
---
name: integrating-wpfui-fluent
description: "Integrates WPF-UI (Wpf.Ui) library for Fluent Design in WPF applications. Use when building modern UI with FluentWindow, NavigationView, SnackbarService, or theme management."
---

# WPF-UI (Wpf.Ui) Integration Guide

Wpf.Ui 4.x 기반 Fluent Design WPF 앱 구현 가이드.

## NuGet Package

```xml
<PackageReference Include="WPF-UI" Version="4.2.*" />
```

## 1. FluentWindow Setup

### App.xaml

```xml
<Application x:Class="MyApp.App"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="pack://application:,,,/Wpf.Ui;component/Styles/Controls.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### MainWindow.xaml

```xml
<ui:FluentWindow x:Class="MyApp.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml"
    Title="MyApp" Height="600" Width="800"
    ExtendsContentIntoTitleBar="True"
    WindowBackdropType="Mica">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <ui:TitleBar Grid.Row="0" Title="MyApp" />

        <ui:NavigationView Grid.Row="1"
            x:Name="RootNavigation"
            PaneDisplayMode="Left"
            IsBackButtonVisible="Collapsed">

            <ui:NavigationView.MenuItems>
                <ui:NavigationViewItem Content="Home"
                    TargetPageType="{x:Type local:HomePage}"
                    Icon="{ui:SymbolIcon Home24}" />
                <ui:NavigationViewItem Content="Settings"
                    TargetPageType="{x:Type local:SettingsPage}"
                    Icon="{ui:SymbolIcon Settings24}" />
            </ui:NavigationView.MenuItems>

        </ui:NavigationView>
    </Grid>
</ui:FluentWindow>
```

### MainWindow.xaml.cs

```csharp
namespace MyApp;

public partial class MainWindow : FluentWindow
{
    public MainWindow(
        MainWindowViewModel viewModel,
        INavigationService navigationService,
        ISnackbarService snackbarService,
        IContentDialogService contentDialogService)
    {
        DataContext = viewModel;
        InitializeComponent();

        navigationService.SetNavigationControl(RootNavigation);
        snackbarService.SetSnackbarPresenter(RootSnackbar);
        contentDialogService.SetDialogHost(RootContentDialog);
    }
}
```

## 2. DI Registration (GenericHost)

```csharp
namespace MyApp;

public partial class App : Application
{
    private static readonly IHost _host = Host.CreateDefaultBuilder()
        .ConfigureServices((_, services) =>
        {
            // Wpf.Ui Services
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<ISnackbarService, SnackbarService>();
            services.AddSingleton<IContentDialogService, ContentDialogService>();
            services.AddSingleton<IThemeService, ThemeService>();

            // Windows
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();

            // Pages (Transient for navigation)
            services.AddTransient<HomePage>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<SettingsViewModel>();
        })
        .Build();

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
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

## 3. Navigation Service

```csharp
// Page는 ui:INavigableView<TViewModel>을 구현
// Page implements ui:INavigableView<TViewModel>
public partial class HomePage : Page, INavigableView<HomeViewModel>
{
    public HomeViewModel ViewModel { get; }

    public HomePage(HomeViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = viewModel;
        InitializeComponent();
    }
}
```

### 프로그래밍 방식 네비게이션

```csharp
// ViewModel에서 INavigationService 주입
// Inject INavigationService in ViewModel
[RelayCommand]
private void NavigateToSettings()
{
    _navigationService.Navigate(typeof(SettingsPage));
}
```

## 4. Snackbar

```csharp
// ViewModel에서 사용
// Use in ViewModel
_snackbarService.Show(
    "성공",          // Title
    "저장되었습니다.", // Message
    ControlAppearance.Success,
    new SymbolIcon(SymbolRegular.Checkmark24),
    TimeSpan.FromSeconds(3));
```

### XAML (MainWindow에 Presenter 배치)

```xml
<ui:SnackbarPresenter x:Name="RootSnackbar" Grid.Row="2" />
```

## 5. ContentDialog

```csharp
// ViewModel에서 사용
// Use in ViewModel
var result = await _contentDialogService.ShowSimpleDialogAsync(
    new SimpleContentDialogCreateOptions
    {
        Title = "삭제 확인",
        Content = "정말 삭제하시겠습니까?",
        PrimaryButtonText = "삭제",
        CloseButtonText = "취소"
    });

if (result == ContentDialogResult.Primary)
{
    // 삭제 처리
    // Handle deletion
}
```

### XAML (MainWindow에 Host 배치)

```xml
<ui:ContentDialogService x:Name="RootContentDialog" Grid.Row="1" />
```

## 6. Theme Management

```csharp
// 테마 전환
// Switch theme
ApplicationThemeManager.Apply(ApplicationTheme.Dark);
ApplicationThemeManager.Apply(ApplicationTheme.Light);

// 시스템 테마 감지 + 자동 적용
// Detect system theme + auto-apply
ApplicationThemeManager.ApplySystemTheme();
```

## 7. CommunityToolkit.Mvvm 통합

WPF-UI는 CommunityToolkit.Mvvm과 자연스럽게 통합됩니다:

```csharp
public sealed partial class HomeViewModel : ObservableObject
{
    private readonly ISnackbarService _snackbarService;

    public HomeViewModel(ISnackbarService snackbarService)
    {
        _snackbarService = snackbarService;
    }

    [ObservableProperty] public partial string SearchText { get; set; }

    [RelayCommand]
    private void Search()
    {
        _snackbarService.Show("검색", $"'{SearchText}' 검색 중...",
            ControlAppearance.Info, null, TimeSpan.FromSeconds(2));
    }
}
```

## Key Rules

- `FluentWindow` 상속 (일반 `Window` 대신)
- `ExtendsContentIntoTitleBar="True"` + `ui:TitleBar` 조합
- Services는 GenericHost에서 Singleton 등록
- Pages는 Transient 등록 (NavigationView가 관리)
- Page는 `INavigableView<TViewModel>` 구현
- `INavigationService.SetNavigationControl()`은 MainWindow 생성자에서 호출

## 참고

- [WPF UI Documentation](https://wpfui.lepo.co/)
- [GitHub - lepoco/wpfui](https://github.com/lepoco/wpfui)
```

**Step 2: 파일 생성 확인**

Run: `ls wpf-dev-pack/skills/integrating-wpfui-fluent/`

**Step 3: Commit**

```bash
git add wpf-dev-pack/skills/integrating-wpfui-fluent/SKILL.md
git commit -m "feat(wpf-dev-pack): add WPF-UI (Wpf.Ui) integration skill"
```

---

## Task 4: LiveCharts2 신규 스킬 (integrating-livecharts2)

**Files:**
- Create: `wpf-dev-pack/skills/integrating-livecharts2/SKILL.md`

**Step 1: SKILL.md 작성**

```markdown
---
name: integrating-livecharts2
description: "Integrates LiveCharts2 for data visualization in WPF with SkiaSharp rendering. Use when building charts, graphs, or dashboards with CartesianChart, PieChart, or real-time data updates."
---

# LiveCharts2 Integration Guide

LiveCharts2 (SkiaSharpView.WPF) 기반 데이터 시각화 가이드.

## NuGet Package

```xml
<!-- ⚠️ 프리릴리스: --version 명시 필수 -->
<!-- ⚠️ Prerelease: --version flag required -->
<PackageReference Include="LiveChartsCore.SkiaSharpView.WPF" Version="2.0.0-rc6.1" />
```

```bash
dotnet add package LiveChartsCore.SkiaSharpView.WPF --version 2.0.0-rc6.1
```

## 1. App Initialization

```csharp
// App.xaml.cs — OnStartup에서 한 번 호출
// App.xaml.cs — Call once in OnStartup
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    LiveCharts.Configure(config =>
        config
            .AddSkiaSharp()
            .AddDefaultMappers()
            .AddDefaultTheme());
}
```

## 2. CartesianChart (Line, Column, Bar)

### XAML

```xml
xmlns:lvc="clr-namespace:LiveChartsCore.SkiaSharpView.WPF;assembly=LiveChartsCore.SkiaSharpView.WPF"

<lvc:CartesianChart
    Series="{Binding Series}"
    XAxes="{Binding XAxes}"
    YAxes="{Binding YAxes}" />
```

### ViewModel

```csharp
public sealed partial class ChartViewModel : ObservableObject
{
    // ✅ ObservableCollection 사용 (List 금지 — 동적 업데이트 불가)
    // ✅ Use ObservableCollection (not List — no dynamic updates)
    [ObservableProperty]
    public partial ObservableCollection<ISeries> Series { get; set; } = [
        new LineSeries<double>
        {
            Values = new ObservableCollection<double> { 3, 5, 7, 2, 8 },
            Name = "매출"
        },
        new ColumnSeries<double>
        {
            Values = new ObservableCollection<double> { 2, 4, 1, 6, 3 },
            Name = "비용"
        }
    ];

    [ObservableProperty]
    public partial Axis[] XAxes { get; set; } = [
        new Axis { Name = "월", Labels = ["1월", "2월", "3월", "4월", "5월"] }
    ];

    [ObservableProperty]
    public partial Axis[] YAxes { get; set; } = [
        new Axis { Name = "금액 (만원)" }
    ];
}
```

## 3. PieChart

```xml
<lvc:PieChart Series="{Binding PieSeries}" />
```

```csharp
[ObservableProperty]
public partial ObservableCollection<ISeries> PieSeries { get; set; } = [
    new PieSeries<double> { Values = [45], Name = "A 제품" },
    new PieSeries<double> { Values = [30], Name = "B 제품" },
    new PieSeries<double> { Values = [25], Name = "C 제품" }
];
```

## 4. Real-Time Data Update

```csharp
private readonly ObservableCollection<double> _values = [0, 0, 0, 0, 0];

public ObservableCollection<ISeries> Series { get; } = [];

public ChartViewModel()
{
    Series.Add(new LineSeries<double> { Values = _values });
}

[RelayCommand]
private void AddDataPoint()
{
    // ⚠️ UI 스레드에서 수정해야 함 (Dispatcher 사용)
    // ⚠️ Must modify on UI thread (use Dispatcher)
    _values.Add(Random.Shared.Next(0, 100));

    if (_values.Count > 50)
    {
        _values.RemoveAt(0);
    }
}
```

## 5. Common Mistakes

| 실수 | 올바른 방법 |
|------|------------|
| `List<ISeries>` 사용 | `ObservableCollection<ISeries>` 사용 필수 |
| Values에 `List<T>` | 동적 업데이트 시 `ObservableCollection<T>` |
| LiveCharts v1 API (`SeriesCollection`, `ChartValues<T>`) | v2 API (`ISeries`, `LineSeries<T>`) |
| 안정 버전으로 패키지 참조 | `2.0.0-rc6.1` 프리릴리스, `--version` 명시 |
| 고빈도 업데이트 시 직접 수정 | UI 스레드에서 수정 또는 `AutoUpdateEnabled` 제어 |

## 6. Namespace Reference

```csharp
// GlobalUsings.cs
global using LiveChartsCore;
global using LiveChartsCore.SkiaSharpView;
global using LiveChartsCore.SkiaSharpView.Painting;
global using SkiaSharp;
```

- `LiveChartsCore` — ISeries, Axis
- `LiveChartsCore.SkiaSharpView` — LineSeries, ColumnSeries, PieSeries
- `LiveChartsCore.SkiaSharpView.WPF` — CartesianChart, PieChart (XAML 컨트롤)

## Key Rules

- `ObservableCollection<ISeries>` 필수 (List 금지)
- Values도 동적 업데이트 시 `ObservableCollection<T>`
- App 초기화에서 `LiveCharts.Configure()` 한 번 호출
- 프리릴리스 버전: `--version 2.0.0-rc6.1` 명시
- ViewModel에서 WPF 참조 없이 사용 가능 (ISeries는 LiveChartsCore 네임스페이스)

## 참고

- [LiveCharts2 Docs](https://livecharts.dev/)
- [GitHub - beto-rodriguez/LiveCharts2](https://github.com/beto-rodriguez/LiveCharts2)
```

**Step 2: 파일 생성 확인**

Run: `ls wpf-dev-pack/skills/integrating-livecharts2/`

**Step 3: Commit**

```bash
git add wpf-dev-pack/skills/integrating-livecharts2/SKILL.md
git commit -m "feat(wpf-dev-pack): add LiveCharts2 integration skill"
```

---

## Task 5: FluentValidation 신규 스킬 (validating-with-fluentvalidation)

**Files:**
- Create: `wpf-dev-pack/skills/validating-with-fluentvalidation/SKILL.md`

**Step 1: SKILL.md 작성**

```markdown
---
name: validating-with-fluentvalidation
description: "Implements FluentValidation with WPF INotifyDataErrorInfo bridge for form validation. Use when building complex validation rules with RuleFor, AbstractValidator, or integrating FluentValidation with CommunityToolkit.Mvvm."
---

# FluentValidation + WPF Integration Guide

FluentValidation 12.x를 WPF MVVM 패턴에 통합하는 가이드.

## NuGet Packages

```xml
<PackageReference Include="FluentValidation" Version="12.1.*" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="12.1.*" />
```

> ⚠️ FluentValidation 12는 **.NET 8 이상** 필수 (.NET Standard, .NET 5/6/7 지원 제거)

## 1. Validator 정의

```csharp
namespace MyApp.Core.Validators;

public sealed class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("이름을 입력하세요.")
            // Name is required.
            .MinimumLength(2).WithMessage("이름은 2자 이상이어야 합니다.");
            // Name must be at least 2 characters.

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("이메일을 입력하세요.")
            // Email is required.
            .EmailAddress().WithMessage("올바른 이메일 형식이 아닙니다.");
            // Invalid email format.

        RuleFor(x => x.Age)
            .InclusiveBetween(1, 150).WithMessage("유효한 나이를 입력하세요.");
            // Please enter a valid age.
    }
}
```

## 2. INotifyDataErrorInfo Bridge

FluentValidation → WPF 바인딩 연결을 위한 Base ViewModel:

```csharp
namespace MyApp.ViewModels;

public abstract partial class ValidatableViewModel<TModel> : ObservableObject, INotifyDataErrorInfo
{
    private readonly IValidator<TModel> _validator;
    private readonly Dictionary<string, List<string>> _errors = [];

    protected ValidatableViewModel(IValidator<TModel> validator)
    {
        _validator = validator;
    }

    public bool HasErrors => _errors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _errors.SelectMany(e => e.Value);

        return _errors.TryGetValue(propertyName, out var errors)
            ? errors
            : [];
    }

    protected void ValidateProperty(TModel model, string propertyName)
    {
        var result = _validator.Validate(model);

        ClearErrors(propertyName);

        foreach (var error in result.Errors.Where(e => e.PropertyName == propertyName))
        {
            AddError(propertyName, error.ErrorMessage);
        }
    }

    protected bool ValidateAll(TModel model)
    {
        var result = _validator.Validate(model);

        // 전체 에러 초기화 후 재설정
        // Clear all errors and rebuild
        var previousProperties = _errors.Keys.ToList();
        _errors.Clear();

        foreach (var prop in previousProperties)
            OnErrorsChanged(prop);

        foreach (var error in result.Errors)
        {
            AddError(error.PropertyName, error.ErrorMessage);
        }

        return result.IsValid;
    }

    private void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
            _errors[propertyName] = [];

        if (!_errors[propertyName].Contains(error))
        {
            _errors[propertyName].Add(error);
            OnErrorsChanged(propertyName);
        }
    }

    private void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
            OnErrorsChanged(propertyName);
    }

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        OnPropertyChanged(nameof(HasErrors));
    }
}
```

## 3. ViewModel 구현

```csharp
namespace MyApp.ViewModels;

public sealed partial class UserFormViewModel : ValidatableViewModel<User>
{
    private readonly User _user = new();

    public UserFormViewModel(IValidator<User> validator) : base(validator) { }

    [ObservableProperty] public partial string Name { get; set; }
    [ObservableProperty] public partial string Email { get; set; }
    [ObservableProperty] public partial int Age { get; set; }

    partial void OnNameChanged(string value)
    {
        _user.Name = value;
        ValidateProperty(_user, nameof(Name));
    }

    partial void OnEmailChanged(string value)
    {
        _user.Email = value;
        ValidateProperty(_user, nameof(Email));
    }

    partial void OnAgeChanged(int value)
    {
        _user.Age = value;
        ValidateProperty(_user, nameof(Age));
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private void Submit()
    {
        if (ValidateAll(_user))
        {
            // 제출 처리
            // Handle submission
        }
    }

    private bool CanSubmit() => !HasErrors;
}
```

## 4. DI Registration

```csharp
services.AddValidatorsFromAssemblyContaining<UserValidator>();

// 기본 수명: Scoped → WPF에서는 Singleton 권장
// Default lifetime: Scoped → Singleton recommended for WPF
services.AddValidatorsFromAssemblyContaining<UserValidator>(ServiceLifetime.Singleton);

services.AddTransient<UserFormViewModel>();
```

## 5. XAML Binding

```xml
<TextBox Text="{Binding Name,
                UpdateSourceTrigger=PropertyChanged,
                ValidatesOnNotifyDataErrors=True}" />

<TextBox Text="{Binding Email,
                UpdateSourceTrigger=PropertyChanged,
                ValidatesOnNotifyDataErrors=True}" />
```

> ⚠️ `UpdateSourceTrigger=PropertyChanged` 필수. 없으면 포커스 이탈 시에만 검증.

## 6. ObservableValidator와의 관계

| 비교 | ObservableValidator | FluentValidation |
|------|-------------------|------------------|
| 규칙 정의 | DataAnnotations (`[Required]`) | Fluent API (`RuleFor`) |
| 복잡한 규칙 | 제한적 | 교차 필드, 조건부, 컬렉션 검증 |
| DI 통합 | 불필요 | `AddValidatorsFromAssembly` |
| 적합한 경우 | 단순 폼 검증 | 복잡한 비즈니스 규칙 |

**혼용 금지**: ObservableValidator와 FluentValidation을 동시 사용하면 에러가 중복/누락됩니다. 하나만 선택하세요.

## 7. Common Mistakes

| 실수 | 올바른 방법 |
|------|------------|
| Model 대신 ViewModel을 직접 검증 | `AbstractValidator<Model>` 사용, ViewModel에서 Model 동기화 |
| 매 변경마다 전체 Validate 호출 | `ValidateProperty`로 해당 속성만 필터링 |
| ErrorsChanged 미발생 | `_errors` 변경 후 반드시 `ErrorsChanged` 이벤트 |
| Singleton Validator에 Scoped 의존성 주입 | Captive Dependency 주의, WPF에서는 Singleton 권장 |
| UpdateSourceTrigger 미설정 | `PropertyChanged` 명시 필수 |

## Key Rules

- Validator는 Model(POCO)을 대상으로 정의
- ViewModel에서 Model 동기화 후 `ValidateProperty()` 호출
- `INotifyDataErrorInfo` 브릿지로 WPF 바인딩 연결
- `AddValidatorsFromAssemblyContaining<T>()` 으로 DI 일괄 등록
- ObservableValidator와 혼용 금지
- See also: `implementing-wpf-validation` (WPF 기본 검증), `handling-errors-with-erroror` (서비스 레이어 에러 처리)

## 참고

- [FluentValidation Docs](https://docs.fluentvalidation.net/)
- [v12 Upgrade Guide](https://docs.fluentvalidation.net/en/latest/upgrading-to-12.html)
```

**Step 2: 파일 생성 확인**

Run: `ls wpf-dev-pack/skills/validating-with-fluentvalidation/`

**Step 3: Commit**

```bash
git add wpf-dev-pack/skills/validating-with-fluentvalidation/SKILL.md
git commit -m "feat(wpf-dev-pack): add FluentValidation + WPF integration skill"
```

---

## Task 6: ErrorOr 신규 스킬 (handling-errors-with-erroror)

**Files:**
- Create: `wpf-dev-pack/skills/handling-errors-with-erroror/SKILL.md`

**Step 1: SKILL.md 작성**

```markdown
---
name: handling-errors-with-erroror
description: "Implements ErrorOr result pattern for service layer error handling in WPF MVVM. Use when returning errors from services instead of throwing exceptions, or integrating ErrorOr with FluentValidation and CommunityToolkit.Mvvm."
---

# ErrorOr Result Pattern Guide

ErrorOr 2.x 기반 서비스 레이어 에러 처리 가이드.

## NuGet Package

```xml
<PackageReference Include="ErrorOr" Version="2.0.*" />
```

- .NET 6+ / .NET Standard 2.0 호환
- ViewModel 프로젝트에서 안전하게 참조 가능 (WPF 의존성 없음)

## 1. 기본 패턴

### Service Layer

```csharp
namespace MyApp.Core.Services;

public sealed class UserService(IUserRepository repository)
{
    public ErrorOr<User> GetUser(int id)
    {
        var user = repository.FindById(id);

        if (user is null)
            return Error.NotFound("User.NotFound", "사용자를 찾을 수 없습니다.");
            // User not found.

        return user;
    }

    public ErrorOr<Created> CreateUser(string name, string email)
    {
        if (repository.ExistsByEmail(email))
            return Error.Conflict("User.DuplicateEmail", "이미 사용 중인 이메일입니다.");
            // Email is already in use.

        repository.Add(new User(name, email));
        return Result.Created;
    }
}
```

### ViewModel (Match/Switch)

```csharp
public sealed partial class UserViewModel : ObservableObject
{
    private readonly UserService _userService;

    [ObservableProperty] public partial string Name { get; set; }
    [ObservableProperty] public partial string ErrorMessage { get; set; }
    [ObservableProperty] public partial bool HasError { get; set; }

    [RelayCommand]
    private void LoadUser(int userId)
    {
        _userService.GetUser(userId).Switch(
            user =>
            {
                HasError = false;
                Name = user.Name;
            },
            errors =>
            {
                HasError = true;
                ErrorMessage = errors.First().Description;
            });
    }
}
```

## 2. Error Types

| Factory Method | 용도 |
|---------------|------|
| `Error.Failure()` | 일반 실패 |
| `Error.Unexpected()` | 예상치 못한 오류 |
| `Error.Validation()` | 유효성 검증 실패 |
| `Error.Conflict()` | 리소스 충돌 |
| `Error.NotFound()` | 리소스 미발견 |
| `Error.Unauthorized()` | 인증 실패 |
| `Error.Forbidden()` | 권한 없음 |

```csharp
// code, description, metadata 지정
// Specify code, description, metadata
var error = Error.Validation(
    code: "User.InvalidAge",
    description: "나이는 1~150 사이여야 합니다.",
    // Age must be between 1 and 150.
    metadata: new Dictionary<string, object> { ["Field"] = "Age" });
```

## 3. Then 체이닝

```csharp
ErrorOr<string> result = _userService.GetUser(userId)
    .Then(user => user.Email)
    .FailIf(email => string.IsNullOrEmpty(email),
        Error.Validation("User.NoEmail", "이메일이 없습니다."))
    // User has no email.
    .Then(email => email.ToUpperInvariant());
```

## 4. FluentValidation 통합

```csharp
namespace MyApp.Core.Extensions;

public static class FluentValidationExtensions
{
    public static List<Error> ToErrors(this FluentValidation.Results.ValidationResult result)
    {
        return result.Errors
            .Select(e => Error.Validation(e.PropertyName, e.ErrorMessage))
            .ToList();
    }
}
```

### Service에서 사용

```csharp
public ErrorOr<Updated> UpdateUser(UpdateUserRequest request)
{
    var validation = _validator.Validate(request);

    if (!validation.IsValid)
        return validation.ToErrors();

    // 비즈니스 로직 처리
    // Process business logic
    _repository.Update(request.ToUser());
    return Result.Updated;
}
```

## 5. 레이어별 에러 전략

```
┌─────────────────────────────────────────┐
│  ViewModel Layer                        │
│  ErrorOr<T>.Switch() → UI 상태 업데이트  │
│  ErrorOr<T>.Switch() → Update UI state  │
├─────────────────────────────────────────┤
│  Service Layer                          │
│  return ErrorOr<T> (예외 대신 결과 반환)  │
│  return ErrorOr<T> (result instead of   │
│  exceptions)                            │
├─────────────────────────────────────────┤
│  Infrastructure Layer                   │
│  try-catch → Error 변환                  │
│  try-catch → Convert to Error           │
└─────────────────────────────────────────┘
```

```csharp
// Infrastructure → Service 경계에서 예외 변환
// Convert exceptions at Infrastructure → Service boundary
public ErrorOr<User> GetUserFromApi(int id)
{
    try
    {
        var user = _httpClient.GetFromJsonAsync<User>($"/api/users/{id}").Result;
        return user ?? Error.NotFound("User.NotFound", "사용자를 찾을 수 없습니다.");
        // User not found.
    }
    catch (HttpRequestException ex)
    {
        return Error.Unexpected("Api.Error", ex.Message);
    }
}
```

## 6. Common Mistakes

| 실수 | 올바른 방법 |
|------|------------|
| `IsError` 확인 없이 `.Value` 접근 | `Match`/`Switch`로 양쪽 경로 처리 |
| 모든 곳에서 ErrorOr 사용 (과도한 적용) | Service Layer 경계에서만 사용 |
| Exception과 ErrorOr 혼용 | 레이어별 명확한 규칙 수립 |
| 빈 `List<Error>` 반환 | 에러 없으면 성공 값 반환 |
| Error 객체를 UI에 직접 바인딩 | `Error.Description` 문자열로 변환 후 바인딩 |
| ViewModel에서 성공만 처리 | `Switch`/`Match`로 에러 경로 필수 처리 |

## Key Rules

- Service Layer의 반환 타입으로 `ErrorOr<T>` 사용 (예외 대신)
- ViewModel에서 `Switch`/`Match`로 양쪽 경로 필수 처리
- FluentValidation 결과는 `.ToErrors()` 확장 메서드로 변환
- Infrastructure 예외는 `try-catch` → `Error` 변환
- ViewModel 프로젝트에서 안전하게 참조 가능 (BCL만 의존)
- See also: `validating-with-fluentvalidation` (폼 검증), `implementing-wpf-validation` (WPF 기본 검증)

## 참고

- [ErrorOr GitHub](https://github.com/amantinband/error-or)
- [NuGet - ErrorOr](https://www.nuget.org/packages/erroror)
```

**Step 2: 파일 생성 확인**

Run: `ls wpf-dev-pack/skills/handling-errors-with-erroror/`

**Step 3: Commit**

```bash
git add wpf-dev-pack/skills/handling-errors-with-erroror/SKILL.md
git commit -m "feat(wpf-dev-pack): add ErrorOr result pattern skill"
```

---

## Task 7: 키워드 매핑 및 카테고리 인덱스 업데이트

**Files:**
- Modify: `wpf-dev-pack/skills/.claude/CLAUDE.md`

**변경 요약:**
신규 4개 스킬의 키워드 트리거 매핑 추가 및 카테고리 인덱스 업데이트.

**Step 1: 키워드 매핑 추가**

`### WPF Keywords` 테이블 끝에 추가:

```markdown
| `wpf-ui`, `wpfui`, `fluentwindow`, `navigationview` | `integrating-wpfui-fluent` |
| `livecharts`, `cartesianchart`, `piechart`, `iseries` | `integrating-livecharts2` |
| `fluentvalidation`, `abstractvalidator`, `rulefor` | `validating-with-fluentvalidation` |
| `erroror`, `result pattern`, `error.validation` | `handling-errors-with-erroror` |
```

`## Skill Category Index` 테이블에 새 카테고리 행 추가:

```markdown
| **3rd Party Libraries** | `integrating-wpfui-fluent`, `integrating-livecharts2`, `validating-with-fluentvalidation`, `handling-errors-with-erroror` |
```

**Step 2: 파일 저장 후 diff 확인**

Run: `git diff wpf-dev-pack/skills/.claude/CLAUDE.md`

**Step 3: Commit**

```bash
git add wpf-dev-pack/skills/.claude/CLAUDE.md
git commit -m "feat(wpf-dev-pack): add keyword mappings for new 3rd-party library skills"
```

---

## Task 8: implementing-wpf-validation 스킬에 FluentValidation 참조 추가

**Files:**
- Modify: `wpf-dev-pack/skills/implementing-wpf-validation/SKILL.md`

**변경 요약:**
기존 validation 스킬의 Summary 테이블에 FluentValidation 행 추가, See also 참조.

**Step 1: Summary 테이블 끝에 행 추가**

`## 6. Summary` 테이블에:

```markdown
| Complex business rules | FluentValidation (`validating-with-fluentvalidation` skill) |
| Service layer errors | ErrorOr (`handling-errors-with-erroror` skill) |
```

**Step 2: 파일 저장 후 diff 확인**

Run: `git diff wpf-dev-pack/skills/implementing-wpf-validation/SKILL.md`

**Step 3: Commit**

```bash
git add wpf-dev-pack/skills/implementing-wpf-validation/SKILL.md
git commit -m "fix(wpf-dev-pack): add FluentValidation and ErrorOr references to validation skill"
```

---

## Task 9: wpf-dev-pack .claude/CLAUDE.md 자동 트리거 업데이트

**Files:**
- Modify: `wpf-dev-pack/.claude/CLAUDE.md`

**변경 요약:**
신규 스킬 4개에 대한 자동 트리거 키워드를 메인 CLAUDE.md에도 반영.

**Step 1: `## Core Rules` 아래 Keyword Detection 목록에 추가**

기존 키워드 목록 뒤에 3rd-party 라이브러리 키워드 그룹 추가. (실제 키워드 감지는 skills/.claude/CLAUDE.md에서 관리하지만, 메인 CLAUDE.md에도 참조를 추가하여 일관성 유지)

**Step 2: Commit**

```bash
git add wpf-dev-pack/.claude/CLAUDE.md
git commit -m "fix(wpf-dev-pack): add 3rd-party library trigger references to main CLAUDE.md"
```

---

## Task 10: wpf-architect 에이전트 적응형 인터뷰 시스템

**Files:**
- Modify: `wpf-dev-pack/agents/wpf-architect.md`

**변경 요약:**
기존 4단계 고정 인터뷰를 **경로별 적응형 인터뷰**로 교체. Step 1에서 선택한 Task Type에 따라 후속 단계의 질문 내용, 선택지, 단계 수가 모두 변경됨.

**Step 1: 인터뷰 섹션 교체**

기존 `## Requirements Interview (MUST RUN FIRST)` ~ `## Interview Summary Template` 섹션 전체를 아래로 교체:

````markdown
## Requirements Interview (MUST RUN FIRST)

Before any work, conduct an adaptive interview using AskUserQuestion tool.
Step 1 selection determines the entire interview path — questions, options, and step count all change.

---

### Step 1: Task Type (All Paths)
```
AskUserQuestion:
  question: "What task can I help you with?"
  header: "Task Type"
  options:
    - label: "Create new WPF project"
      description: "New project scaffolding with recommended structure"
    - label: "Analyze/improve existing project"
      description: "Analyze current codebase, extract patterns, or suggest improvements"
    - label: "Implement specific feature"
      description: "Add a feature to an existing project"
    - label: "Debug/fix issues"
      description: "Diagnose and fix specific problems"
```

**Routing:**
- "Create new WPF project" → **Path A** (7 steps)
- "Analyze/improve existing project" → **Path B** (5 steps)
- "Implement specific feature" → **Path C** (5 steps)
- "Debug/fix issues" → **Path D** (4 steps)

---

## Path A: Create New WPF Project (7 Steps)

### A-2: Program Concept (Free Input + Keyword Analysis)
```
AskUserQuestion:
  question: "어떤 프로그램을 만들려고 하시나요? 목적과 주요 기능을 자유롭게 설명해주세요."
  header: "Program Concept"
```

**Keyword Analysis → Auto-Routing:**

사용자 입력에서 키워드를 감지하여 후속 단계의 기본값을 자동 설정:

| Detected Keyword | Auto-Set Default |
|-----------------|------------------|
| "대시보드", "dashboard", "모니터링", "차트", "그래프" | A-6: LiveCharts2 추천 |
| "모던 UI", "Fluent", "Mica", "material" | A-6: WPF-UI 추천 |
| "입력 폼", "회원가입", "등록", "검증", "validation" | A-6: FluentValidation 추천 |
| "API", "서비스", "에러 처리", "result pattern" | A-6: ErrorOr 추천 |
| "모듈", "플러그인", "대규모", "enterprise" | A-3: Prism 추천, A-4: 엔터프라이즈 선택 |
| "프로토타입", "간단한", "테스트", "POC" | A-4: 경량 선택 |
| "데이터 관리", "CRUD" | A-6: FluentValidation + ErrorOr 추천 |

### A-3: Architecture Pattern
```
AskUserQuestion:
  question: "Which architecture pattern would you like to use?"
  header: "Architecture"
  options:
    - label: "MVVM + CommunityToolkit (Recommended)"
      description: "Modern MVVM with source generators, best for maintainable apps"
    - label: "Code-behind (Simple)"
      description: "Direct event handlers, best for quick prototypes"
    - label: "Prism Framework"
      description: "Enterprise MVVM with modules, regions, and navigation"
    - label: "No preference"
      description: "I'll recommend based on your project complexity"
```

| Selection | Activate Skills | Delegate To |
|-----------|-----------------|-------------|
| MVVM + CommunityToolkit | `implementing-communitytoolkit-mvvm`, `structuring-wpf-projects` | `wpf-mvvm-expert` |
| Code-behind | Basic WPF patterns only | - |
| Prism | `make-wpf-project --prism` | - |
| No preference | Analyze complexity, then recommend | - |

### A-4: Project Scale
```
AskUserQuestion:
  question: "프로젝트 구조를 어느 정도로 가져갈까요?"
  header: "Project Scale"
  options:
    - label: "경량 (Lightweight)"
      description: "단일 프로젝트, 빠른 시작, 프로토타입에 적합"
    - label: "표준 (Standard)"
      description: "App + ViewModels + Core 분리, 중소규모 앱에 적합"
    - label: "엔터프라이즈 (Enterprise)"
      description: "전체 레이어 분리 (Abstractions, Core, ViewModels, Services, UI, App)"
```

| Selection | `make-wpf-project` Option |
|-----------|--------------------------|
| 경량 | `--minimal` |
| 표준 | (default) |
| 엔터프라이즈 | `--full` |

### A-5: Complexity Level
```
AskUserQuestion:
  question: "Select your preferred complexity level"
  header: "Complexity"
  options:
    - label: "Simple & Quick"
      description: "Standard controls, basic bindings, quick results"
    - label: "Balanced"
      description: "CustomControls, proper MVVM, good maintainability"
    - label: "Advanced / High-Performance"
      description: "DrawingContext, virtualization, optimized rendering"
```

| Selection | Activate Skills | Agents |
|-----------|-----------------|--------|
| Simple | Basic WPF, simple bindings | - |
| Balanced | `authoring-wpf-controls`, `customizing-controltemplate` | `wpf-control-designer` |
| Advanced | `rendering-with-drawingcontext`, `virtualizing-wpf-ui` | `wpf-performance-optimizer` |

### A-6: Open Source Libraries (Multi-Select, Keyword-Based Recommendations)
```
AskUserQuestion:
  question: "사용할 오픈소스 라이브러리를 선택해주세요. 기본 추천에서 변경하거나 추가할 수 있습니다."
  header: "Open Source Libraries"
  multiSelect: true
  options:
    - label: "UI: WPF-UI (Fluent Design)"
      description: "FluentWindow, NavigationView, Mica backdrop, modern controls"
    - label: "Chart: LiveCharts2 (SkiaSharp)"
      description: "CartesianChart, PieChart, real-time data visualization"
    - label: "Validation: FluentValidation"
      description: "Fluent API validation rules with INotifyDataErrorInfo bridge"
    - label: "Error Handling: ErrorOr"
      description: "Result pattern for service layer, replaces exceptions"
    - label: "기본만 사용 (No additional libraries)"
      description: "CommunityToolkit.Mvvm + GenericHost only"
    - label: "기타 (직접 입력)"
      description: "위에 없는 라이브러리를 직접 입력"
```

A-2에서 감지된 키워드에 따라 해당 옵션에 "⭐ 추천" 표시.

| Selection | Activate Skill |
|-----------|---------------|
| WPF-UI | `integrating-wpfui-fluent` |
| LiveCharts2 | `integrating-livecharts2` |
| FluentValidation | `validating-with-fluentvalidation` |
| ErrorOr | `handling-errors-with-erroror` |
| 기타 | Context7 MCP로 해당 라이브러리 문서 조회 |

### A-7: Feature Areas (Multi-Select)
```
AskUserQuestion:
  question: "Select all feature areas you need"
  header: "Features"
  multiSelect: true
  options:
    - label: "UI/Controls"
      description: "CustomControl, UserControl, ControlTemplate"
    - label: "Data Binding/Validation"
      description: "Complex bindings, validation, converters"
    - label: "Rendering/Graphics"
      description: "Drawing, shapes, visual effects"
    - label: "Animation/Media"
      description: "Storyboards, transitions, media playback"
```

| Selection | Skills | Recommended Agent |
|-----------|--------|-------------------|
| UI/Controls | `authoring-wpf-controls`, `developing-wpf-customcontrols` | `wpf-control-designer` |
| Data Binding | `advanced-data-binding`, `implementing-wpf-validation` | `wpf-data-binding-expert` |
| Rendering | `rendering-with-drawingcontext`, `rendering-with-drawingvisual` | `wpf-performance-optimizer` |
| Animation | `creating-wpf-animations`, `integrating-wpf-media` | `wpf-xaml-designer` |

### A-Summary → 프로젝트 스캐폴딩 실행

---

## Path B: Analyze/Improve Existing Project (5 Steps)

### B-2: Analysis Goal (Free Input + Keyword Analysis)
```
AskUserQuestion:
  question: "어떤 부분을 분석하거나 개선하고 싶으신가요? (예: '성능 병목 찾기', 'MVVM 패턴 위반 검출', '오픈소스 코드에서 렌더링 기법 추출')"
  header: "Analysis Goal"
```

**Keyword Analysis → Auto-Routing:**

| Detected Keyword | Auto-Set Default |
|-----------------|------------------|
| "성능", "느림", "메모리", "렌더링" | B-3: 성능 분석 추천 |
| "MVVM", "패턴", "구조", "리팩토링" | B-3: 코드 품질 리뷰 추천 |
| "아키텍처", "레이어", "DI", "의존성" | B-3: 아키텍처 진단 추천 |
| "오픈소스", "clone", "기법", "패턴 추출", "요점" | B-3: 오픈소스 코드 분석 추천 |

### B-3: Analysis Mode
```
AskUserQuestion:
  question: "어떤 방식으로 분석할까요?"
  header: "Analysis Mode"
  options:
    - label: "코드 품질 리뷰"
      description: "MVVM 위반, 의존성 방향, 네이밍 컨벤션 검사"
    - label: "성능 분석"
      description: "렌더링 안티패턴, Freeze 누락, 가상화 미적용 탐지"
    - label: "아키텍처 진단"
      description: "프로젝트 구조, 레이어 분리, DI 설정 평가"
    - label: "오픈소스 코드 분석"
      description: "특정 기법/패턴 추출 및 요점 정리"
```

| Selection | Activate Skills | Agents |
|-----------|-----------------|--------|
| 코드 품질 리뷰 | `implementing-communitytoolkit-mvvm`, `structuring-wpf-projects` | `wpf-code-reviewer` |
| 성능 분석 | `rendering-wpf-high-performance`, `optimizing-wpf-memory`, `virtualizing-wpf-ui` | `wpf-performance-optimizer` |
| 아키텍처 진단 | `structuring-wpf-projects`, `configuring-dependency-injection` | `wpf-architect` (self) |
| 오픈소스 코드 분석 | 대상 코드에서 감지된 기술에 따라 동적 활성화 | `wpf-architect` (self) |

### B-4: Analysis Scope
```
AskUserQuestion:
  question: "분석 대상 범위를 선택해주세요."
  header: "Analysis Scope"
  options:
    - label: "전체 솔루션"
      description: "모든 프로젝트를 대상으로 분석"
    - label: "특정 프로젝트/폴더"
      description: "분석 대상 경로를 지정"
    - label: "특정 파일"
      description: "개별 파일 단위 분석"
```

### B-5: Output Format
```
AskUserQuestion:
  question: "분석 결과를 어떤 형식으로 받고 싶으신가요?"
  header: "Output Format"
  options:
    - label: "문제점 + 수정 코드 제안"
      description: "발견된 이슈마다 수정 코드 예시 포함"
    - label: "요약 보고서"
      description: "우선순위별 권장사항 목록"
    - label: "핵심 패턴 추출"
      description: "분석 대상에서 기법/패턴만 정리"
```

### B-Summary → 분석 실행

---

## Path C: Implement Specific Feature (5 Steps)

### C-2: Feature Description (Free Input + Keyword Analysis)
```
AskUserQuestion:
  question: "구현할 기능을 설명해주세요. (예: '사용자 설정 페이지 추가', '실시간 차트 대시보드', '드래그 앤 드롭으로 항목 정렬')"
  header: "Feature Description"
```

**Keyword Analysis → Auto-Routing:**

| Detected Keyword | Auto-Set Default |
|-----------------|------------------|
| "차트", "그래프", "대시보드", "시각화" | C-4: LiveCharts2 추천 |
| "모던", "Fluent", "네비게이션", "테마" | C-4: WPF-UI 추천 |
| "폼", "입력", "검증", "유효성" | C-4: FluentValidation 추천 |
| "API", "서비스 호출", "에러" | C-4: ErrorOr 추천 |
| "드래그", "애니메이션" | C-5: Animation/Media 추천 |
| "커스텀 컨트롤", "렌더링" | C-5: UI/Controls 또는 Rendering 추천 |

### C-3: Implementation Approach
```
AskUserQuestion:
  question: "어떤 방식으로 구현할까요?"
  header: "Implementation Approach"
  options:
    - label: "기존 코드에 통합"
      description: "현재 프로젝트의 패턴과 구조를 따라 구현"
    - label: "독립 모듈로 추가"
      description: "새 프로젝트/폴더로 분리하여 구현"
    - label: "프로토타입 먼저"
      description: "최소한의 코드로 빠르게 검증 후 정리"
```

| Selection | Behavior |
|-----------|----------|
| 기존 코드에 통합 | 기존 프로젝트의 아키텍처/네이밍 컨벤션 분석 후 따름 |
| 독립 모듈로 추가 | `structuring-wpf-projects` 스킬로 새 프로젝트 구조 생성 |
| 프로토타입 먼저 | 최소한의 단일 파일 구현, 추상화 없음 |

### C-4: Open Source Libraries (Multi-Select, Keyword-Based Recommendations)
```
AskUserQuestion:
  question: "이 기능 구현에 필요한 라이브러리가 있나요?"
  header: "Libraries for This Feature"
  multiSelect: true
  options:
    - label: "UI: WPF-UI (Fluent Design)"
      description: "FluentWindow, NavigationView, Mica backdrop, modern controls"
    - label: "Chart: LiveCharts2 (SkiaSharp)"
      description: "CartesianChart, PieChart, real-time data visualization"
    - label: "Validation: FluentValidation"
      description: "Fluent API validation rules with INotifyDataErrorInfo bridge"
    - label: "Error Handling: ErrorOr"
      description: "Result pattern for service layer, replaces exceptions"
    - label: "추가 라이브러리 없음"
      description: "기존 프로젝트의 의존성만 사용"
    - label: "기타 (직접 입력)"
      description: "위에 없는 라이브러리를 직접 입력"
```

C-2에서 감지된 키워드에 따라 해당 옵션에 "⭐ 추천" 표시.

### C-5: Feature Areas (Multi-Select)
```
AskUserQuestion:
  question: "이 기능이 속하는 영역을 선택해주세요."
  header: "Feature Areas"
  multiSelect: true
  options:
    - label: "UI/Controls"
      description: "CustomControl, UserControl, ControlTemplate"
    - label: "Data Binding/Validation"
      description: "Complex bindings, validation, converters"
    - label: "Rendering/Graphics"
      description: "Drawing, shapes, visual effects"
    - label: "Animation/Media"
      description: "Storyboards, transitions, media playback"
```

### C-Summary → 구현 시작

---

## Path D: Debug/Fix Issues (4 Steps)

### D-2: Problem Description (Free Input + Keyword Analysis)
```
AskUserQuestion:
  question: "어떤 문제가 발생하고 있나요? 증상, 에러 메시지, 재현 조건을 설명해주세요. (예: '바인딩이 동작하지 않음', 'UI가 멈춤', 'CustomControl 스타일이 적용 안 됨')"
  header: "Problem Description"
```

**Keyword Analysis → Auto-Routing:**

| Detected Keyword | Auto-Set Default |
|-----------------|------------------|
| "바인딩", "Binding", "값이 안 바뀜" | D-3: 데이터 관련 문제 추천 |
| "스타일", "ControlTemplate", "안 보임", "깨짐" | D-3: UI 표시 문제 추천 |
| "느림", "멈춤", "프리징", "메모리" | D-3: 성능 문제 추천 |
| "크래시", "예외", "NullReference", "Exception" | D-3: 크래시/예외 추천 |
| "빌드", "컴파일", "패키지", "XAML 파싱" | D-3: 빌드/설정 오류 추천 |

### D-3: Problem Type
```
AskUserQuestion:
  question: "문제 유형을 선택해주세요."
  header: "Problem Type"
  options:
    - label: "UI가 의도대로 표시되지 않음"
      description: "레이아웃 깨짐, 스타일 미적용, 컨트롤 미표시"
    - label: "데이터가 올바르게 동작하지 않음"
      description: "바인딩 실패, 값 미갱신, 검증 오류"
    - label: "성능 문제"
      description: "UI 멈춤, 느린 렌더링, 메모리 누수"
    - label: "크래시/예외"
      description: "앱 강제 종료, 미처리 예외"
    - label: "빌드/설정 오류"
      description: "컴파일 에러, 패키지 충돌, 리소스 미발견"
```

| Selection | Activate Skills | Agents |
|-----------|-----------------|--------|
| UI 표시 문제 | `customizing-controltemplate`, `managing-styles-resourcedictionary`, `navigating-visual-logical-tree` | `wpf-xaml-designer` |
| 데이터 문제 | `advanced-data-binding`, `implementing-wpf-validation`, `implementing-communitytoolkit-mvvm` | `wpf-data-binding-expert` |
| 성능 문제 | `rendering-wpf-high-performance`, `optimizing-wpf-memory`, `virtualizing-wpf-ui`, `threading-wpf-dispatcher` | `wpf-performance-optimizer` |
| 크래시/예외 | `managing-wpf-application-lifecycle`, `threading-wpf-dispatcher` | `wpf-code-reviewer` |
| 빌드/설정 오류 | `configuring-wpf-themeinfo`, `configuring-dependency-injection` | `wpf-code-reviewer` |

### D-4: Problem Area (Multi-Select)
```
AskUserQuestion:
  question: "문제가 발생하는 구체적인 영역을 선택해주세요."
  header: "Problem Area"
  multiSelect: true
  options:
    - label: "XAML / ControlTemplate / Style"
      description: "XAML 마크업, 스타일, 템플릿 관련"
    - label: "ViewModel / Data Binding"
      description: "ViewModel, 바인딩 경로, 커맨드 관련"
    - label: "CustomControl / DependencyProperty"
      description: "커스텀 컨트롤, 의존성 속성 관련"
    - label: "Rendering / DrawingContext"
      description: "렌더링, 그래픽, OnRender 관련"
    - label: "Threading / Dispatcher"
      description: "UI 스레드, 비동기 작업, Dispatcher 관련"
    - label: "3rd Party Library (WPF-UI, LiveCharts 등)"
      description: "서드파티 라이브러리 관련 문제"
```

| Selection | Additional Skills |
|-----------|------------------|
| XAML / ControlTemplate | `designing-wpf-customcontrol-architecture` |
| ViewModel / Data Binding | `mapping-viewmodel-view-datatemplate` |
| CustomControl / DependencyProperty | `defining-wpf-dependencyproperty`, `authoring-wpf-controls` |
| Rendering / DrawingContext | `rendering-with-drawingcontext`, `rendering-with-drawingvisual` |
| Threading / Dispatcher | `threading-wpf-dispatcher`, `handling-async-operations` |
| 3rd Party Library | `integrating-wpfui-fluent`, `integrating-livecharts2` (해당 라이브러리에 따라) |

### D-Summary → 디버깅 시작

---

## Interview Summary Templates

### Path A Summary
```markdown
## 📋 Requirements Summary

### Task: Create new WPF project
### Concept: [A-2 입력]
### Architecture: [A-3 선택]
### Scale: [A-4 선택]
### Complexity: [A-5 선택]
### Libraries: [A-6 선택]
### Feature Areas: [A-7 선택]
### Auto-Detected Keywords: [A-2 키워드]

### Recommended Approach:
- **Skills to activate**: [list]
- **Agents to delegate**: [list]
- **Commands to use**: [make-wpf-project with options]
- **Libraries to include**: [NuGet packages with versions]
```

### Path B Summary
```markdown
## 📋 Analysis Summary

### Task: Analyze/improve existing project
### Goal: [B-2 입력]
### Analysis Mode: [B-3 선택]
### Scope: [B-4 선택]
### Output Format: [B-5 선택]
### Auto-Detected Keywords: [B-2 키워드]

### Analysis Plan:
- **Skills to activate**: [list]
- **Agent to delegate**: [agent]
- **Target**: [scope details]
```

### Path C Summary
```markdown
## 📋 Feature Summary

### Task: Implement specific feature
### Feature: [C-2 입력]
### Approach: [C-3 선택]
### Libraries: [C-4 선택]
### Feature Areas: [C-5 선택]
### Auto-Detected Keywords: [C-2 키워드]

### Implementation Plan:
- **Skills to activate**: [list]
- **Agents to delegate**: [list]
- **Libraries to include**: [NuGet packages with versions]
```

### Path D Summary
```markdown
## 📋 Debug Summary

### Task: Debug/fix issues
### Problem: [D-2 입력]
### Type: [D-3 선택]
### Areas: [D-4 선택]
### Auto-Detected Keywords: [D-2 키워드]

### Debug Plan:
- **Skills to activate**: [list]
- **Agent to delegate**: [agent]
- **Investigation targets**: [areas to check]
```
````

**Step 2: skills frontmatter에 신규 스킬 추가**

```yaml
skills:
  - structuring-wpf-projects
  - implementing-communitytoolkit-mvvm
  - managing-wpf-collectionview-mvvm
  - mapping-viewmodel-view-datatemplate
  - configuring-dependency-injection
  - integrating-wpfui-fluent
  - integrating-livecharts2
  - validating-with-fluentvalidation
  - handling-errors-with-erroror
```

**Step 3: Commit**

```bash
git add wpf-dev-pack/agents/wpf-architect.md
git commit -m "feat(wpf-dev-pack): replace fixed 4-step interview with adaptive path-based interview system"
```

---

## Task 11: wpf-architect-low 에이전트 동일 적용

**Files:**
- Modify: `wpf-dev-pack/agents/wpf-architect-low.md`

**변경 요약:**
Task 10과 동일한 적응형 인터뷰를 wpf-architect-low (Sonnet 버전)에도 적용.

**Step 1: wpf-architect.md와 동일한 인터뷰 섹션 적용**

Task 10의 교체 내용을 그대로 적용. 차이점:
- 헤더: `# WPF Architect (Low) - Architecture Advisor` 유지
- Note 유지: `> **Note**: This is the lightweight (Sonnet) version.`

**Step 2: skills frontmatter 동일하게 업데이트**

**Step 3: Commit**

```bash
git add wpf-dev-pack/agents/wpf-architect-low.md
git commit -m "feat(wpf-dev-pack): sync architect-low with adaptive interview system"
```

---

## Task 12: agents/.claude/CLAUDE.md 위임 가이드 업데이트

**Files:**
- Modify: `wpf-dev-pack/agents/.claude/CLAUDE.md`

**변경 요약:**
인터뷰 플로우 다이어그램을 경로별 적응형 구조로 업데이트.

**Step 1: Interview Flow 섹션 교체**

기존 `## Requirements Interview System` ~ 끝까지 아래로 교체:

```markdown
## Requirements Interview System

`wpf-architect` and `wpf-architect-low` use an **adaptive interview system** where Step 1 determines the entire interview path.

### Path Overview

| Path | Task Type | Steps | Focus |
|------|-----------|-------|-------|
| **A** | Create new project | 7 | 컨셉 → 아키텍처 → 규모 → 복잡도 → 라이브러리 → 기능 영역 |
| **B** | Analyze/improve | 5 | 분석 목표 → 분석 모드 → 범위 → 출력 형식 |
| **C** | Implement feature | 5 | 기능 설명 → 구현 방식 → 라이브러리 → 기능 영역 |
| **D** | Debug/fix | 4 | 문제 증상 → 문제 유형 → 문제 영역 |

### Interview Flow

```
Step 1: Task Type
   ├─→ A: Create new project (7 steps)
   │   ├─ A-2: 프로그램 컨셉 (자유 입력 → 키워드 분석)
   │   ├─ A-3: 아키텍처 패턴
   │   ├─ A-4: 프로젝트 규모
   │   ├─ A-5: 복잡도
   │   ├─ A-6: 오픈소스 라이브러리 (키워드 기반 추천)
   │   └─ A-7: 기능 영역
   │
   ├─→ B: Analyze/improve (5 steps)
   │   ├─ B-2: 분석 목표 (자유 입력 → 키워드 분석)
   │   ├─ B-3: 분석 모드 (코드 품질/성능/아키텍처/오픈소스)
   │   ├─ B-4: 분석 범위 (전체/특정 프로젝트/특정 파일)
   │   └─ B-5: 출력 형식 (수정 코드/요약/패턴 추출)
   │
   ├─→ C: Implement feature (5 steps)
   │   ├─ C-2: 기능 설명 (자유 입력 → 키워드 분석)
   │   ├─ C-3: 구현 방식 (통합/독립 모듈/프로토타입)
   │   ├─ C-4: 라이브러리 (키워드 기반 추천)
   │   └─ C-5: 기능 영역
   │
   └─→ D: Debug/fix (4 steps)
       ├─ D-2: 문제 증상 (자유 입력 → 키워드 분석)
       ├─ D-3: 문제 유형 (UI/데이터/성능/크래시/빌드)
       └─ D-4: 문제 영역 (XAML/ViewModel/CustomControl/Rendering/Threading/3rd Party)
```

### Keyword Analysis

All free-input steps (A-2, B-2, C-2, D-2) analyze user input for keywords and auto-set defaults for subsequent steps. This reduces interview friction while maintaining precision.

### Usage

The interview starts automatically when `wpf-architect` is invoked. Users can:
- Click preset options for common choices
- Type custom responses in free-input steps
- Follow the recommended path or override defaults
```

**Step 2: Agent Mapping 테이블에 경로별 에이전트 매핑 추가**

기존 Agent Mapping 테이블 뒤에 추가:

```markdown
### Path-Specific Agent Routing

| Path | Selection | Primary Agent |
|------|-----------|---------------|
| B | 코드 품질 리뷰 | `wpf-code-reviewer` |
| B | 성능 분석 | `wpf-performance-optimizer` |
| B | 아키텍처 진단 | `wpf-architect` (self) |
| B | 오픈소스 코드 분석 | `wpf-architect` (self) |
| D | UI 표시 문제 | `wpf-xaml-designer` |
| D | 데이터 문제 | `wpf-data-binding-expert` |
| D | 성능 문제 | `wpf-performance-optimizer` |
| D | 크래시/예외 | `wpf-code-reviewer` |
| D | 빌드/설정 오류 | `wpf-code-reviewer` |
```

**Step 3: Commit**

```bash
git add wpf-dev-pack/agents/.claude/CLAUDE.md
git commit -m "feat(wpf-dev-pack): update delegation guide with adaptive path-based interview flow"
```

---

## 실행 순서 요약

| Task | 유형 | 파일 | 의존성 |
|------|------|------|--------|
| 1 | 교정 | implementing-communitytoolkit-mvvm/SKILL.md | 없음 |
| 2 | 교정 | commands/make-wpf-project/PRISM.md | 없음 |
| 3 | 신규 | skills/integrating-wpfui-fluent/SKILL.md | 없음 |
| 4 | 신규 | skills/integrating-livecharts2/SKILL.md | 없음 |
| 5 | 신규 | skills/validating-with-fluentvalidation/SKILL.md | 없음 |
| 6 | 신규 | skills/handling-errors-with-erroror/SKILL.md | 없음 |
| 7 | 매핑 | skills/.claude/CLAUDE.md | Task 3-6 완료 후 |
| 8 | 참조 | skills/implementing-wpf-validation/SKILL.md | Task 5-6 완료 후 |
| 9 | 참조 | .claude/CLAUDE.md | Task 7 완료 후 |
| 10 | 인터뷰 | agents/wpf-architect.md | Task 3-6 완료 후 |
| 11 | 인터뷰 | agents/wpf-architect-low.md | Task 10 완료 후 |
| 12 | 참조 | agents/.claude/CLAUDE.md | Task 10 완료 후 |

**병렬 그룹 A (독립):** Task 1-6 — 스킬 교정/추가, 모두 병렬 가능
**병렬 그룹 B (A 이후):** Task 7, 8, 10 — 매핑/참조/인터뷰, A 완료 후 병렬 가능
**순차 그룹 C (B 이후):** Task 9, 11, 12 — 최종 참조 업데이트
