# Prism 9 코드 규칙

> `mvvm-framework.md`에서 Prism 9이 선택된 경우 적용됩니다.

---

## NuGet 패키지

| 프로젝트 | 패키지 |
|----------|--------|
| Shell (WPF App) | `Prism.DryIoc` (9.0.537) |
| Core (공유) | `Prism.Core` (9.0.537) |
| Module | `Prism.DryIoc` (9.0.537) |

> ⚠️ **Prism.Magician** (유료 소스 제너레이터)은 사용하지 않습니다. 모든 코드는 수동 작성합니다.

---

## ViewModel 작성 규칙

### Base Class

- `BindableBase` 상속
- `partial class` 불필요 (소스 제너레이터 미사용)

### 속성 (SetProperty)

```csharp
private string _userName = string.Empty;
public string UserName
{
    get => _userName;
    set => SetProperty(ref _userName, value);
}
```

### 연관 속성 알림

```csharp
set
{
    if (SetProperty(ref _field, value))
    {
        RaisePropertyChanged(nameof(ComputedProperty));
    }
}
```

### Command (DelegateCommand)

```csharp
private DelegateCommand? _saveCommand;
public DelegateCommand SaveCommand =>
    _saveCommand ??= new DelegateCommand(ExecuteSave, CanSave)
        .ObservesProperty(() => Email);
```

- Async: `AsyncDelegateCommand`
- 제네릭: `DelegateCommand<T?>` (값 타입은 Nullable)
- CanExecute 자동 재평가: `.ObservesProperty(() => Xxx)`
- 수동 재평가: `RaiseCanExecuteChanged()`

---

## DI 패턴

- `PrismApplication` 상속 (GenericHost 불필요)
- `RegisterTypes(IContainerRegistry)` 오버라이드에서 서비스 등록
- `CreateShell()`에서 MainWindow 반환 (자동 표시)

### 등록 API

| 용도 | API |
|------|-----|
| Singleton | `containerRegistry.RegisterSingleton<I, T>()` |
| Transient | `containerRegistry.Register<I, T>()` |
| Navigation | `containerRegistry.RegisterForNavigation<V, VM>()` |
| Dialog | `containerRegistry.RegisterDialog<V, VM>()` |

---

## 네비게이션

- `IRegionManager.RequestNavigate()` 사용
- `ViewModelLocator.AutoWireViewModel="True"` XAML 설정
- `INavigationAware` 인터페이스로 네비게이션 수명주기 관리

---

## 모듈 아키텍처

- `IModule` 인터페이스 구현
- `ConfigureModuleCatalog()`에서 모듈 등록
- 모듈 간 통신: `IEventAggregator` + `PubSubEvent<T>`

---

## 스킬 참조

MVVM 관련 스킬 사용 시 **PRISM.md**의 코드 예제를 따릅니다.

| 스킬 | 참조 파일 |
|------|----------|
| `implementing-communitytoolkit-mvvm` | PRISM.md |
| `configuring-dependency-injection` | PRISM.md |
| `structuring-wpf-projects` | PRISM.md |
| `mapping-viewmodel-view-datatemplate` | PRISM.md |
| `creating-wpf-dialogs` | PRISM.md |
| `managing-wpf-application-lifecycle` | PRISM.md |
| 기타 MVVM 관련 스킬 | PRISM.md (있을 경우) |

> PRISM.md가 없는 스킬 (XAML, 렌더링, 3rd-party 등)은 SKILL.md를 그대로 사용합니다.
