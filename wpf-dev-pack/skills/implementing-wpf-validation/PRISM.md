# WPF Validation - Prism 9 Version

> **MVVM Framework Rule**: `.claude/rules/dotnet/wpf/prism9.md` 규칙이 적용됩니다.
> CommunityToolkit.Mvvm 사용 시 → [SKILL.md](SKILL.md) 참조

Prism 9 (Community License) 기준 유효성 검증 패턴. SKILL.md의 CommunityToolkit.Mvvm 대응.

> SKILL.md의 ValidationRule, IDataErrorInfo, Error Template XAML은 프레임워크 무관이므로 그대로 사용합니다.
> 이 문서는 ViewModel 기반 검증 (INotifyDataErrorInfo)의 Prism 버전만 다룹니다.

## CommunityToolkit.Mvvm vs Prism 비교

| 항목 | CommunityToolkit.Mvvm | Prism 9 |
|------|----------------------|---------|
| 검증 Base Class | `ObservableValidator` | 없음 (수동 구현) |
| DataAnnotations | `[Required]`, `[Range]` 자동 연동 | 수동 `INotifyDataErrorInfo` 구현 |
| 속성별 검증 | `ValidateProperty()` (내장) | 수동 구현 |
| 전체 검증 | `ValidateAllProperties()` (내장) | 수동 구현 |

## ValidatableBindableBase 패턴

Prism에는 `ObservableValidator`가 없으므로 `BindableBase` + `INotifyDataErrorInfo`를 결합한 Base Class를 직접 구현합니다.

```csharp
namespace MyApp.Core.Mvvm;

public abstract class ValidatableBindableBase : BindableBase, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = [];

    public bool HasErrors => _errors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _errors.SelectMany(e => e.Value);

        return _errors.TryGetValue(propertyName, out var errors)
            ? errors
            : Enumerable.Empty<string>();
    }

    protected void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
            _errors[propertyName] = [];

        if (!_errors[propertyName].Contains(error))
        {
            _errors[propertyName].Add(error);
            OnErrorsChanged(propertyName);
        }
    }

    protected void ClearErrors(string propertyName)
    {
        if (_errors.Remove(propertyName))
            OnErrorsChanged(propertyName);
    }

    protected void ClearAllErrors()
    {
        var properties = _errors.Keys.ToList();
        _errors.Clear();
        foreach (var prop in properties)
            OnErrorsChanged(prop);
    }

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        RaisePropertyChanged(nameof(HasErrors));
    }
}
```

## ViewModel 구현

```csharp
namespace MyApp.ViewModels;

public class RegistrationViewModel : ValidatableBindableBase
{
    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
                ValidateEmail();
        }
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                ValidatePassword();
                ValidateConfirmPassword();
            }
        }
    }

    private string _confirmPassword = string.Empty;
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            if (SetProperty(ref _confirmPassword, value))
                ValidateConfirmPassword();
        }
    }

    // 검증 메서드
    // Validation methods
    private void ValidateEmail()
    {
        ClearErrors(nameof(Email));

        if (string.IsNullOrWhiteSpace(Email))
            AddError(nameof(Email), "이메일을 입력하세요.");
            // Please enter an email address.
        else if (!Email.Contains('@'))
            AddError(nameof(Email), "올바른 이메일 형식이 아닙니다.");
            // Invalid email format.
    }

    private void ValidatePassword()
    {
        ClearErrors(nameof(Password));

        if (Password.Length < 8)
            AddError(nameof(Password), "비밀번호는 8자 이상이어야 합니다.");
            // Password must be at least 8 characters.

        if (!Password.Any(char.IsDigit))
            AddError(nameof(Password), "비밀번호에 숫자가 포함되어야 합니다.");
            // Password must contain a digit.
    }

    private void ValidateConfirmPassword()
    {
        ClearErrors(nameof(ConfirmPassword));

        if (ConfirmPassword != Password)
            AddError(nameof(ConfirmPassword), "비밀번호가 일치하지 않습니다.");
            // Passwords do not match.
    }

    // Command
    private DelegateCommand? _submitCommand;
    public DelegateCommand SubmitCommand =>
        _submitCommand ??= new DelegateCommand(ExecuteSubmit, CanSubmit)
            .ObservesProperty(() => HasErrors)
            .ObservesProperty(() => Email);

    private void ExecuteSubmit()
    {
        ValidateAll();
        if (!HasErrors)
        {
            // 제출 처리
            // Handle submission
        }
    }

    private bool CanSubmit() => !HasErrors && !string.IsNullOrEmpty(Email);

    private void ValidateAll()
    {
        ValidateEmail();
        ValidatePassword();
        ValidateConfirmPassword();
    }
}
```

## XAML (SKILL.md와 동일)

```xml
<TextBox Text="{Binding Email,
                UpdateSourceTrigger=PropertyChanged,
                ValidatesOnNotifyDataErrors=True}"/>
```

## 검증 방식 비교 (Prism 관점)

| 요구사항 | 권장 방식 |
|---------|----------|
| 단순 XAML 검증 | ValidationRule (프레임워크 무관) |
| ViewModel 기반 검증 | `ValidatableBindableBase` (위 패턴) |
| 복잡한 비즈니스 규칙 | FluentValidation + `ValidatableBindableBase` 브릿지 |

## Key Differences from SKILL.md

- **ObservableValidator 없음**: Prism에는 DataAnnotations 자동 연동 Base Class가 없음
- **ValidatableBindableBase**: `BindableBase` + `INotifyDataErrorInfo`를 결합한 커스텀 Base Class 필요
- **수동 검증**: `ValidateProperty()` / `ValidateAllProperties()` 대신 각 속성별 검증 메서드 직접 구현
- **SetProperty 콜백**: `partial void OnXxxChanged()` 대신 `SetProperty()` 반환값으로 검증 트리거
- **ObservesProperty**: Command의 CanExecute가 `HasErrors` 변경을 자동 감지
