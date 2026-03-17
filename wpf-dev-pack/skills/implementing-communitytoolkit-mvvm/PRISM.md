# CommunityToolkit.Mvvm → Prism 9 Version

> **MVVM Framework Rule**: `.claude/rules/dotnet/wpf/prism9.md` 규칙이 적용됩니다.
> CommunityToolkit.Mvvm 사용 시 → [SKILL.md](SKILL.md) 참조

Prism 9 (Community License) 기준 MVVM 패턴. SKILL.md의 CommunityToolkit.Mvvm 대응.

> ⚠️ Prism.Magician (유료 소스 제너레이터)은 사용하지 않습니다. 모든 코드는 수동 작성입니다.

## CommunityToolkit.Mvvm vs Prism 비교

| 항목 | CommunityToolkit.Mvvm | Prism 9 |
|------|----------------------|---------|
| Base Class | `ObservableObject` | `BindableBase` |
| 속성 변경 알림 | `[ObservableProperty]` (소스 제너레이터) | `SetProperty()` 수동 작성 |
| Command | `[RelayCommand]` (소스 제너레이터) | `DelegateCommand` 수동 작성 |
| Async Command | `[RelayCommand]` + `Task` 반환 | `AsyncDelegateCommand` |
| CanExecute 연동 | `[NotifyCanExecuteChangedFor]` | `.ObservesProperty()` / `.ObservesCanExecute()` |
| 연관 속성 알림 | `[NotifyPropertyChangedFor]` | `RaisePropertyChanged()` 수동 호출 |
| Validation | `ObservableValidator` | 수동 `INotifyDataErrorInfo` 구현 |
| NuGet | `CommunityToolkit.Mvvm` | `Prism.Core` |

## Prerequisites

```xml
<PackageReference Include="Prism.Core" Version="9.0.537" />
```

## 1. BindableBase 속성 패턴

### 기본 속성 (SetProperty)

```csharp
// CommunityToolkit.Mvvm 버전:
// [ObservableProperty] private string _userName = string.Empty;

// Prism 9 버전:
// Prism 9 version:
public class UserViewModel : BindableBase
{
    private string _userName = string.Empty;
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }
}
```

### 연관 속성 알림 (RaisePropertyChanged)

```csharp
// CommunityToolkit.Mvvm 버전:
// [NotifyPropertyChangedFor(nameof(FullName))]
// [ObservableProperty] private string _firstName = string.Empty;

// Prism 9 버전:
// Prism 9 version:
private string _firstName = string.Empty;
public string FirstName
{
    get => _firstName;
    set
    {
        if (SetProperty(ref _firstName, value))
        {
            RaisePropertyChanged(nameof(FullName));
        }
    }
}

public string FullName => $"{FirstName} {LastName}";
```

## 2. DelegateCommand 패턴

### 기본 Command

```csharp
// CommunityToolkit.Mvvm 버전:
// [RelayCommand]
// private void Save() { /* ... */ }

// Prism 9 버전:
// Prism 9 version:
private DelegateCommand? _saveCommand;
public DelegateCommand SaveCommand =>
    _saveCommand ??= new DelegateCommand(ExecuteSave);

private void ExecuteSave()
{
    // 저장 로직
    // Save logic
}
```

### CanExecute 연동

```csharp
// CommunityToolkit.Mvvm 버전:
// [RelayCommand(CanExecute = nameof(CanSave))]
// private void Save() { /* ... */ }
// private bool CanSave() => !string.IsNullOrWhiteSpace(Email);

// Prism 9 버전: ObservesProperty로 자동 재평가
// Prism 9 version: Auto re-evaluate with ObservesProperty
private DelegateCommand? _saveCommand;
public DelegateCommand SaveCommand =>
    _saveCommand ??= new DelegateCommand(ExecuteSave, CanSave)
        .ObservesProperty(() => Email);

private bool CanSave() => !string.IsNullOrWhiteSpace(Email);
```

### 제네릭 Command

```csharp
// CommunityToolkit.Mvvm 버전:
// [RelayCommand]
// private void SelectTool(ViewerTool tool) { CurrentTool = tool; }

// Prism 9 버전:
// Prism 9 version:
private DelegateCommand<ViewerTool?>? _selectToolCommand;
public DelegateCommand<ViewerTool?> SelectToolCommand =>
    _selectToolCommand ??= new DelegateCommand<ViewerTool?>(ExecuteSelectTool);

private void ExecuteSelectTool(ViewerTool? tool)
{
    if (tool.HasValue)
        CurrentTool = tool.Value;
}
```

### AsyncDelegateCommand

```csharp
// CommunityToolkit.Mvvm 버전:
// [RelayCommand]
// private async Task LoadDataAsync() { /* ... */ }

// Prism 9 버전:
// Prism 9 version:
private AsyncDelegateCommand? _loadDataCommand;
public AsyncDelegateCommand LoadDataCommand =>
    _loadDataCommand ??= new AsyncDelegateCommand(ExecuteLoadDataAsync);

private async Task ExecuteLoadDataAsync()
{
    // 비동기 로딩
    // Async loading
    await Task.Delay(1000);
}
```

## 3. 완전한 ViewModel 예제

```csharp
namespace MyApp.ViewModels;

using Prism.Commands;
using Prism.Mvvm;

public class UserViewModel : BindableBase
{
    // 속성
    // Properties
    private string _firstName = string.Empty;
    public string FirstName
    {
        get => _firstName;
        set
        {
            if (SetProperty(ref _firstName, value))
            {
                RaisePropertyChanged(nameof(FullName));
                SaveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string _lastName = string.Empty;
    public string LastName
    {
        get => _lastName;
        set
        {
            if (SetProperty(ref _lastName, value))
            {
                RaisePropertyChanged(nameof(FullName));
            }
        }
    }

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string FullName => $"{FirstName} {LastName}";

    // Commands
    private DelegateCommand? _saveCommand;
    public DelegateCommand SaveCommand =>
        _saveCommand ??= new DelegateCommand(ExecuteSave, CanSave)
            .ObservesProperty(() => Email);

    private void ExecuteSave()
    {
        // 저장 로직
        // Save logic
    }

    private bool CanSave() =>
        !string.IsNullOrWhiteSpace(FirstName) &&
        !string.IsNullOrWhiteSpace(Email);

    private AsyncDelegateCommand? _loadCommand;
    public AsyncDelegateCommand LoadCommand =>
        _loadCommand ??= new AsyncDelegateCommand(ExecuteLoadAsync);

    private async Task ExecuteLoadAsync()
    {
        // 비동기 로딩
        // Async loading
        await Task.Delay(1000);
    }
}
```

## 4. GlobalUsings.cs (Prism)

```csharp
global using Prism.Commands;
global using Prism.Mvvm;
```

## Key Differences from SKILL.md

- **소스 제너레이터 없음**: `[ObservableProperty]`, `[RelayCommand]` 사용 불가. 모든 속성과 Command를 수동 작성
- **partial class 불필요**: 소스 제너레이터를 사용하지 않으므로 `partial` 키워드 불필요
- **ObservesProperty**: Prism 전용 기능으로 CanExecute 자동 재평가 지원
- **RaiseCanExecuteChanged**: CommunityToolkit의 `[NotifyCanExecuteChangedFor]` 대신 수동 호출
- **Prism.Magician 미사용**: 유료 소스 제너레이터이므로 Community License에서는 수동 작성 필수
