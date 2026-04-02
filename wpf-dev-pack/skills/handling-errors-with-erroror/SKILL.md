---
description: "Implements ErrorOr result pattern for service layer error handling in WPF MVVM. Use when returning errors from services instead of throwing exceptions, or integrating ErrorOr with FluentValidation and CommunityToolkit.Mvvm."
user-invocable: false
model: sonnet
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

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _hasError;

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
