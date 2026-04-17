# WPF Dialogs - Prism 9 Version

> **MVVM Framework Rule**: `.claude/rules/dotnet/wpf/prism9.md` 규칙이 적용됩니다.
> CommunityToolkit.Mvvm 사용 시 → [SKILL.md](SKILL.md) 참조

Prism 9 (Community License) 기준 대화 상자 패턴. SKILL.md의 기본 Dialog 대응.

> SKILL.md의 MessageBox, OpenFileDialog, SaveFileDialog 등 WPF 기본 대화 상자는 프레임워크 무관이므로 그대로 사용합니다.
> 이 문서는 Prism의 **IDialogService** 패턴만 다룹니다.

## CommunityToolkit.Mvvm vs Prism 비교

| 항목 | CommunityToolkit.Mvvm | Prism 9 |
|------|----------------------|---------|
| 대화 상자 방식 | `Window.ShowDialog()` 직접 호출 | `IDialogService.ShowDialog()` |
| VM-View 분리 | 수동 구현 필요 | `IDialogAware` 인터페이스 |
| 파라미터 전달 | 속성 직접 설정 | `IDialogParameters` |
| 결과 반환 | `DialogResult` (bool?) | `ButtonResult` enum |
| 닫기 제어 | `DialogResult = true/false` | `DialogCloseListener` |
| DI 등록 | 없음 (수동) | `RegisterDialog<V, VM>()` |

## Prerequisites

```xml
<PackageReference Include="Prism.DryIoc" Version="9.0.537" />
```

## 1. IDialogAware ViewModel

```csharp
namespace MyApp.Modules.Home.ViewModels;

public class ConfirmDialogViewModel : BindableBase, IDialogAware
{
    // Prism 9: Title 속성
    // Prism 9: Title property
    public string Title => "확인";

    // Prism 9: DialogCloseListener (Prism 8의 RequestClose 이벤트 대체)
    // Prism 9: DialogCloseListener (replaces Prism 8 RequestClose event)
    public DialogCloseListener RequestClose { get; }

    private string _message = string.Empty;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    // Commands
    private DelegateCommand? _confirmCommand;
    public DelegateCommand ConfirmCommand =>
        _confirmCommand ??= new DelegateCommand(ExecuteConfirm);

    private void ExecuteConfirm()
    {
        var result = new DialogResult(ButtonResult.OK);
        result.Parameters.Add("confirmed", true);
        RequestClose.Invoke(result);
    }

    private DelegateCommand? _cancelCommand;
    public DelegateCommand CancelCommand =>
        _cancelCommand ??= new DelegateCommand(ExecuteCancel);

    private void ExecuteCancel()
    {
        RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
    }

    // 닫기 가능 여부
    // Whether dialog can be closed
    public bool CanCloseDialog() => true;

    // 대화 상자가 닫힐 때
    // When dialog is closed
    public void OnDialogClosed() { }

    // 대화 상자가 열릴 때 (파라미터 수신)
    // When dialog is opened (receive parameters)
    public void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters.ContainsKey("message"))
        {
            Message = parameters.GetValue<string>("message");
        }
    }
}
```

## 2. Dialog View (XAML)

```xml
<UserControl x:Class="MyApp.Modules.Home.Views.ConfirmDialogView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:prism="http://prismlibrary.com/"
    prism:ViewModelLocator.AutoWireViewModel="True"
    Width="400" Height="200">

    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <TextBlock Grid.Row="0" Text="{Binding Message}"
                   TextWrapping="Wrap" VerticalAlignment="Center"/>

        <StackPanel Grid.Row="1" Orientation="Horizontal"
                    HorizontalAlignment="Right">
            <Button Content="확인" Width="80" Margin="0,0,10,0"
                    Command="{Binding ConfirmCommand}"/>
            <Button Content="취소" Width="80"
                    Command="{Binding CancelCommand}"/>
        </StackPanel>
    </Grid>
</UserControl>
```

> ⚠️ Prism Dialog View는 `Window`가 아닌 **`UserControl`** 입니다. Prism이 자동으로 Window를 감싸줍니다.

## 3. Dialog 등록

```csharp
// Module 또는 App의 RegisterTypes에서 등록
// Register in Module or App's RegisterTypes
public void RegisterTypes(IContainerRegistry containerRegistry)
{
    containerRegistry.RegisterDialog<ConfirmDialogView, ConfirmDialogViewModel>();
}
```

## 4. Dialog 호출

```csharp
public class HomeViewModel : BindableBase
{
    private readonly IDialogService _dialogService;

    public HomeViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    private void ShowConfirm()
    {
        var parameters = new DialogParameters
        {
            { "message", "정말 삭제하시겠습니까?" }
            // Are you sure you want to delete?
        };

        _dialogService.ShowDialog(
            nameof(ConfirmDialogView),
            parameters,
            result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    // 확인 처리
                    // Handle confirmation
                    var confirmed = result.Parameters.GetValue<bool>("confirmed");
                }
            });
    }
}
```

## 5. ButtonResult 값

| ButtonResult | 의미 |
|-------------|------|
| `None` | 결과 없음 (X 버튼 등) |
| `OK` | 확인 |
| `Cancel` | 취소 |
| `Yes` | 예 |
| `No` | 아니오 |
| `Abort` | 중단 |
| `Retry` | 재시도 |
| `Ignore` | 무시 |

## Key Differences from SKILL.md

- **Window 대신 UserControl**: Dialog View를 `UserControl`로 정의 (Prism이 Window 래핑)
- **IDialogAware**: Dialog ViewModel이 구현하는 인터페이스로 수명주기 관리
- **DialogCloseListener**: `DialogResult = true` 대신 `RequestClose.Invoke()` 사용
- **IDialogParameters**: 생성자/속성 대신 Key-Value 파라미터로 데이터 전달/반환
- **RegisterDialog**: DI 등록으로 Dialog View-ViewModel 매핑
- **MVVM 완전 분리**: Dialog 호출 시 View 타입을 직접 참조하지 않고 문자열 이름 사용
