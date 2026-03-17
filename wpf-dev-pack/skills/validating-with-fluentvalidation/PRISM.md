# FluentValidation + WPF - Prism 9 Version

> **MVVM Framework Rule**: `.claude/rules/dotnet/wpf/prism9.md` 규칙이 적용됩니다.
> CommunityToolkit.Mvvm 사용 시 → [SKILL.md](SKILL.md) 참조

Prism 9 (Community License) 기준 FluentValidation 통합. SKILL.md의 CommunityToolkit.Mvvm 대응.

> SKILL.md의 Validator 정의 (`AbstractValidator<T>`), XAML 바인딩, DI 등록은 프레임워크 무관이므로 그대로 사용합니다.
> 이 문서는 INotifyDataErrorInfo 브릿지의 BindableBase 버전만 다룹니다.

## CommunityToolkit.Mvvm vs Prism 비교

| 항목 | CommunityToolkit.Mvvm | Prism 9 |
|------|----------------------|---------|
| 브릿지 Base Class | `ValidatableViewModel<T> : ObservableObject` | `ValidatableBindableBase<T> : BindableBase` |
| 속성 변경 콜백 | `partial void OnXxxChanged()` | `SetProperty()` 내부 콜백 |
| DI 등록 | `services.AddValidatorsFromAssembly...` | `containerRegistry`에 수동 등록 |

## ValidatableBindableBase\<T\> (BindableBase + FluentValidation)

```csharp
namespace MyApp.Core.Mvvm;

public abstract class ValidatableBindableBase<TModel> : BindableBase, INotifyDataErrorInfo
{
    private readonly IValidator<TModel> _validator;
    private readonly Dictionary<string, List<string>> _errors = [];

    protected ValidatableBindableBase(IValidator<TModel> validator)
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
            AddError(propertyName, error.ErrorMessage);
    }

    protected bool ValidateAll(TModel model)
    {
        var result = _validator.Validate(model);

        var previousProperties = _errors.Keys.ToList();
        _errors.Clear();
        foreach (var prop in previousProperties)
            OnErrorsChanged(prop);

        foreach (var error in result.Errors)
            AddError(error.PropertyName, error.ErrorMessage);

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
        RaisePropertyChanged(nameof(HasErrors));
    }
}
```

## ViewModel 구현

```csharp
namespace MyApp.ViewModels;

public class UserFormViewModel : ValidatableBindableBase<User>
{
    private readonly User _user = new();

    public UserFormViewModel(IValidator<User> validator) : base(validator) { }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                _user.Name = value;
                ValidateProperty(_user, nameof(Name));
            }
        }
    }

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
            {
                _user.Email = value;
                ValidateProperty(_user, nameof(Email));
            }
        }
    }

    private DelegateCommand? _submitCommand;
    public DelegateCommand SubmitCommand =>
        _submitCommand ??= new DelegateCommand(ExecuteSubmit, CanSubmit)
            .ObservesProperty(() => HasErrors);

    private void ExecuteSubmit()
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

## DI 등록 (IContainerRegistry)

```csharp
protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    // FluentValidation Validator 등록
    // Register FluentValidation validators
    containerRegistry.RegisterSingleton<IValidator<User>, UserValidator>();

    // ViewModel 등록
    // Register ViewModel
    containerRegistry.Register<UserFormViewModel>();
}
```

> ⚠️ Prism은 `AddValidatorsFromAssemblyContaining<T>()` 확장 메서드를 직접 지원하지 않습니다. 개별 등록하거나 DryIoc 컨테이너에 직접 접근하여 일괄 등록할 수 있습니다.

## Key Differences from SKILL.md

- **BindableBase 상속**: `ObservableObject` 대신 `BindableBase` 상속
- **SetProperty 콜백**: `partial void OnXxxChanged()` 대신 `SetProperty()` 성공 시 검증 호출
- **DelegateCommand**: `[RelayCommand]` 대신 수동 `DelegateCommand` 작성
- **개별 DI 등록**: `AddValidatorsFromAssembly` 대신 `RegisterSingleton<IValidator<T>, Validator>()` 개별 등록
- **Validator 정의 동일**: `AbstractValidator<T>` 정의는 변경 없음
