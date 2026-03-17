# CommunityToolkit.Mvvm 코드 규칙

> `mvvm-framework.md`에서 CommunityToolkit.Mvvm이 선택된 경우 적용됩니다.

---

## NuGet 패키지

```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.*" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.*" />
```

---

## ViewModel 작성 규칙

### Base Class

- `ObservableObject` 상속
- `partial class` 필수 (소스 제너레이터)

### 속성 (ObservableProperty)

- **단일 속성**: `[ObservableProperty]`를 필드와 같은 줄에 inline 작성
- **다중 속성**: 다른 attribute는 별도 줄, `[ObservableProperty]`는 **항상 마지막 줄에 inline**

```csharp
// 단일 속성
// Single attribute
[ObservableProperty] private string _userName = string.Empty;

// 다중 속성
// Multiple attributes
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
[ObservableProperty] private string _email = string.Empty;
```

### .NET 9+ Partial Property

```csharp
[ObservableProperty] public partial string UserName { get; set; }
```

### Command (RelayCommand)

- `[RelayCommand]` attribute로 소스 제너레이터 사용
- CanExecute는 `[RelayCommand(CanExecute = nameof(CanXxx))]`
- Async는 `Task` 반환 메서드에 자동 적용

### 연관 속성 알림

- `[NotifyPropertyChangedFor(nameof(Xxx))]` 사용
- `[NotifyCanExecuteChangedFor(nameof(XxxCommand))]` 사용

---

## DI 패턴

- GenericHost (`Host.CreateDefaultBuilder()`) 사용
- `IServiceCollection`에 서비스 등록
- `App()` 생성자에서 Host 생성, `OnStartup`에서 시작

---

## 스킬 참조

MVVM 관련 스킬 사용 시 **SKILL.md**의 코드 예제를 따릅니다.

| 스킬 | 참조 파일 |
|------|----------|
| `implementing-communitytoolkit-mvvm` | SKILL.md |
| `configuring-dependency-injection` | SKILL.md |
| `structuring-wpf-projects` | SKILL.md |
| `mapping-viewmodel-view-datatemplate` | SKILL.md |
| `creating-wpf-dialogs` | SKILL.md |
| `managing-wpf-application-lifecycle` | SKILL.md |
| 기타 MVVM 관련 스킬 | SKILL.md |
