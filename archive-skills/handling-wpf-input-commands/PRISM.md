# WPF Input and Commands - Prism 9 Version

> **MVVM Framework Rule**: `.claude/rules/dotnet/wpf/prism9.md` 규칙이 적용됩니다.
> CommunityToolkit.Mvvm 사용 시 → [SKILL.md](SKILL.md) 참조

Prism 9 (Community License) 기준 커맨드 패턴. SKILL.md의 CommunityToolkit.Mvvm 대응.

> SKILL.md의 RoutedCommand, InputBinding, 키보드/마우스 이벤트 처리는 WPF 기본 기능이므로 그대로 사용합니다.
> 이 문서는 MVVM Command와 Prism 전용 CompositeCommand만 다룹니다.

## CommunityToolkit.Mvvm vs Prism 비교

| 항목 | CommunityToolkit.Mvvm | Prism 9 |
|------|----------------------|---------|
| MVVM Command | `[RelayCommand]` | `DelegateCommand` |
| Async Command | `[RelayCommand]` + Task | `AsyncDelegateCommand` |
| 복합 Command | 없음 | `CompositeCommand` |
| CanExecute 연동 | `[NotifyCanExecuteChangedFor]` | `.ObservesProperty()` |

## 1. DelegateCommand (MVVM Command)

```csharp
// CommunityToolkit.Mvvm 버전:
// [RelayCommand]
// private void Delete() { /* ... */ }

// Prism 9 버전:
// Prism 9 version:
public class MainViewModel : BindableBase
{
    private DelegateCommand? _deleteCommand;
    public DelegateCommand DeleteCommand =>
        _deleteCommand ??= new DelegateCommand(ExecuteDelete, CanDelete)
            .ObservesProperty(() => SelectedItem);

    private void ExecuteDelete()
    {
        // 삭제 로직
        // Delete logic
    }

    private bool CanDelete() => SelectedItem is not null;
}
```

### XAML InputBinding 연결

```xml
<Window.InputBindings>
    <!-- MVVM DelegateCommand 바인딩 -->
    <!-- MVVM DelegateCommand binding -->
    <KeyBinding Key="Delete" Command="{Binding DeleteCommand}"/>
    <KeyBinding Key="S" Modifiers="Control" Command="{Binding SaveCommand}"/>
</Window.InputBindings>
```

## 2. CompositeCommand (Prism 전용)

여러 Command를 하나로 묶어 동시 실행. 모든 하위 Command의 `CanExecute`가 `true`일 때만 실행 가능.

### 정의 (Core 프로젝트)

```csharp
namespace MyApp.Core.Commands;

// 전역 CompositeCommand 정의
// Define global CompositeCommand
public interface IApplicationCommands
{
    CompositeCommand SaveAllCommand { get; }
}

public class ApplicationCommands : IApplicationCommands
{
    public CompositeCommand SaveAllCommand { get; } = new CompositeCommand();
}
```

### DI 등록

```csharp
protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();
}
```

### 하위 Command 등록 (Module ViewModel)

```csharp
public class DocumentViewModel : BindableBase
{
    public DocumentViewModel(IApplicationCommands applicationCommands)
    {
        // 개별 SaveCommand를 SaveAllCommand에 등록
        // Register individual SaveCommand to SaveAllCommand
        applicationCommands.SaveAllCommand.RegisterCommand(SaveCommand);
    }

    private DelegateCommand? _saveCommand;
    public DelegateCommand SaveCommand =>
        _saveCommand ??= new DelegateCommand(ExecuteSave);

    private void ExecuteSave()
    {
        // 이 문서 저장
        // Save this document
    }
}
```

### Shell에서 사용

```xml
<!-- SaveAll: 모든 등록된 SaveCommand 동시 실행 -->
<!-- SaveAll: Execute all registered SaveCommands simultaneously -->
<Button Content="Save All"
        Command="{Binding ApplicationCommands.SaveAllCommand}"/>
```

```csharp
public class MainWindowViewModel : BindableBase
{
    public IApplicationCommands ApplicationCommands { get; }

    public MainWindowViewModel(IApplicationCommands applicationCommands)
    {
        ApplicationCommands = applicationCommands;
    }
}
```

## Key Differences from SKILL.md

- **DelegateCommand**: `[RelayCommand]` 소스 제너레이터 대신 수동 작성
- **ObservesProperty**: `[NotifyCanExecuteChangedFor]` 대신 fluent API로 CanExecute 자동 재평가
- **CompositeCommand**: Prism 전용 기능으로 여러 모듈의 Command를 하나로 묶어 실행
- **RoutedCommand/InputBinding**: WPF 기본 기능이므로 SKILL.md와 동일하게 사용
