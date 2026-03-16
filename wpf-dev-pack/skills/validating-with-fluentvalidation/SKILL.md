---
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

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private int _age;

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
