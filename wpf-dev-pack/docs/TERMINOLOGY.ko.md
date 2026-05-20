[🇺🇸 English](./TERMINOLOGY.md)

# wpf-dev-pack — 용어 정의

본 문서는 wpf-dev-pack이 채택하는 MVVM Composition 방식과 관련 용어를
Microsoft 공식 정의 기준으로 정의합니다. 본 문서가 plugin 전반에서 사용하는
모든 용어의 단일 출처(single source of truth)입니다.

---

## 1. 4축 분리 모델 (Four-Axis Separation Model)

MVVM에서 View와 ViewModel의 관계는 두 개의 **직교하는 독립적 축**으로
나뉩니다.

### 축 1: Composition Direction (View First vs ViewModel First)

Microsoft 공식 분류 기준은 단 하나의 질문입니다 —
**composition/navigation의 lookup key가 무엇인가?**

| Lookup key | 분류 |
|---|---|
| View 타입 이름 (string) | **View First Composition** |
| ViewModel 타입 | **ViewModel First Composition** |

- **View First Composition** — 식별자가 View 이름. 예:
  - Prism `RequestNavigate("ContentRegion", "HomeView")`
  - Prism `ViewModelLocator.AutoWireViewModel="True"` (View가 anchor)
  - View code-behind `DataContext = new HomeViewModel();`

- **ViewModel First Composition** — 식별자가 ViewModel 타입. 예:
  - WPF implicit DataTemplate
    `<DataTemplate DataType="{x:Type vm:HomeViewModel}">`
  - ViewModel 타입을 target으로 하는 navigation service

참조:
- https://learn.microsoft.com/dotnet/architecture/maui/navigation
- https://learn.microsoft.com/dotnet/architecture/maui/mvvm#connecting-view-models-to-views

### 축 2: ViewModel State Management (Stateful vs Stateless)

- **Stateful ViewModel** — ViewModel 인스턴스가 자신의 상태를 직접 보유
  (한국 WPF 생태계의 사실상 표준).
- **Stateless ViewModel** — 상태는 외부 Manager/Service에 위임, ViewModel은
  transient (Stylet, Caliburn.Micro 권장 스타일).

### 두 축의 직교성

| Composition Direction | State Management | 대표 예시 |
|---|---|---|
| View First | Stateful | Prism `RegisterForNavigation` + `RegionManager` (wpf-dev-pack Prism 경로); 전형적 Prism `ViewModelLocator` 패턴 |
| View First | Stateless | (드묾) View가 매번 외부 State에서 데이터 fetch |
| **ViewModel First** | **Stateful** | **wpf-dev-pack CommunityToolkit.Mvvm 경로** (`Mappings.xaml` + implicit DataTemplate) |
| ViewModel First | Stateless | Stylet의 transient VM 권장 스타일 |

v1.6.4 이전 문서는 "ViewModel First ⇒ Stateless"라는 잘못된 함의를 전제로
"View First MVVM"이라는 단일 라벨을 강제했으나, 두 축은 **독립**이며 실제
wpf-dev-pack은 선택된 프레임워크에 따라 두 가지 조합을 모두 강제합니다
(§2 참조).

---

## 2. 공식 채택 조합

wpf-dev-pack은 사용자가 선택한 MVVM 프레임워크에 따라 **서로 다른**
composition style을 강제합니다. 두 경로 모두 **Stateful ViewModel**을
공통 채택합니다.

### 2.1 CommunityToolkit.Mvvm 경로 (기본)

> **ViewModel First Composition + Stateful ViewModel**

구체 메커니즘: `Mappings.xaml` 기반 implicit DataTemplate 매핑.

```xml
<DataTemplate DataType="{x:Type vm:HomeViewModel}">
    <views:HomeView />
</DataTemplate>
```

```csharp
CurrentViewModel = new HomeViewModel();  // ViewModel 인스턴스가 lookup key
```

상세: [`view-viewmodel-wiring-communitytoolkit.md`](../.claude/rules/view-viewmodel-wiring-communitytoolkit.md)

### 2.2 Prism 9 경로 (대안)

> **View First Composition + Stateful ViewModel**

구체 메커니즘:
`IContainerRegistry.RegisterForNavigation<View, ViewModel>()` 등록 +
`IRegionManager.RequestNavigate("Region", "ViewName")` 네비게이션
(**view name string** 기반).

```csharp
containerRegistry.RegisterForNavigation<HomeView, HomeViewModel>();
// ...
_regionManager.RequestNavigate("ContentRegion", "HomeView");  // View name이 lookup key
```

상세: [`view-viewmodel-wiring-prism.md`](../.claude/rules/view-viewmodel-wiring-prism.md)

### 2.3 두 경로의 공통점

- **Stateful ViewModel**을 표준 상태 관리 방식으로 채택.
- 프레임워크별로 **단 하나의 매칭 메커니즘**만 허용. 동일 프로젝트 내에
  복수의 View-VM 매칭 경로를 공존시키지 않음.
- ViewModel 클래스는 `System.Windows.*` UI 타입을 참조하지 않음
  (`ICommand` 제외).

---

## 3. 명시적으로 금지하는 패턴

전체 규칙은 [`prohibitions.md`](../.claude/rules/prohibitions.md) 참조.

요약:

| 금지 패턴 | 분류 | 금지 이유 |
|---|---|---|
| Prism `ViewModelLocator.AutoWireViewModel="True"` | View First (Prism의 대체 메커니즘) | Prism 경로의 단일 매칭 메커니즘은 `RegisterForNavigation`. `ViewModelLocator`는 그와 경합함 |
| View code-behind `DataContext = new XxxViewModel()` | View First (imperative) | CommunityToolkit 경로의 단일 매칭 경로를 깸 |
| `<UserControl.DataContext><vm:XxxVM /></UserControl.DataContext>` | View First (declarative) | 위와 동일 |
| Stateless ViewModel + Transient IoC 등록 | Stateless VM composition | 별도의 framework 체계(Stylet/Caliburn)에 적합 — plugin 범위 밖 |
| `Mappings.xaml`/`RegisterForNavigation` 외 매칭 메커니즘 도입 | (any) | 프로젝트당 단일 매칭 경로 유지 |

---

## 4. 용어 변경 이력

v1.6.4 이전 문서는 wpf-dev-pack의 채택 방식을 일괄적으로
**"View First MVVM"** 으로 라벨링했으나, 이는 Microsoft 공식 정의와
충돌하는 **부정확한 표현**이었습니다 — 특히 CommunityToolkit.Mvvm 경로
(`Mappings.xaml` DataTemplate)는 lookup key가 ViewModel 타입이므로
정확히는 **ViewModel First Composition**입니다.

라벨이 다음과 같이 정정되었습니다:

| 시기 | 라벨 |
|---|---|
| v1.6.4 이전 | "View First MVVM" (단일 라벨) — 부정확 |
| v1.6.4+ (CommunityToolkit 경로) | "ViewModel First Composition + Stateful ViewModel" |
| v1.6.4+ (Prism 경로) | "View First Composition + Stateful ViewModel" |

> **Plugin이 강제하는 실제 코드 패턴은 변경되지 않았습니다.**
> 라벨만 표준 용어로 정정되었으므로 사용자 코드 수정은 불필요합니다.

---

## 5. 참고 문헌

| 주제 | URL |
|---|---|
| MVVM — Connecting view models to views | https://learn.microsoft.com/dotnet/architecture/maui/mvvm#connecting-view-models-to-views |
| Navigation — View first vs ViewModel first | https://learn.microsoft.com/dotnet/architecture/maui/navigation |
| CommunityToolkit.Mvvm | https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/ |
| WPF Overview | https://learn.microsoft.com/dotnet/desktop/wpf/overview/ |

---

## 부록 A — 핵심 개념 요약 (Quick Reference)

### View First vs ViewModel First 판정법

> **질문 하나로 판정**: composition/navigation의 lookup key가 무엇인가?
> - View name이 key → **View First**
> - ViewModel type이 key → **ViewModel First**

### 패턴별 분류표

| 패턴 | Lookup key | 분류 | plugin 정책 |
|---|---|---|---|
| `Mappings.xaml` implicit DataTemplate | ViewModel 타입 | ViewModel First (Stateful) | ✅ 채택 (CommunityToolkit 경로) |
| Prism `RegisterForNavigation` + `RequestNavigate("View")` | View name (string) | View First (Stateful) | ✅ 채택 (Prism 경로) |
| Prism `ViewModelLocator.AutoWireViewModel` | View name | View First (Stateful) | ❌ 금지 (Prism 경로의 단일 메커니즘은 `RegisterForNavigation`) |
| code-behind `DataContext = new VM()` | (View가 VM 직접 선택) | View First (imperative) | ❌ 금지 |
| Stylet transient VM + naming convention | ViewModel 타입 | ViewModel First (Stateless) | ❌ 금지 (범위 밖) |
