# wpf-dev-pack 용어 정의 (Terminology)

본 문서는 wpf-dev-pack이 채택하는 MVVM Composition 방식과 관련 용어를
Microsoft 공식 정의 기준으로 정의합니다. 본 문서가 모든 용어의 단일
출처(single source of truth)입니다.

This document defines the MVVM composition styles adopted by wpf-dev-pack
based on the official Microsoft definitions. This file is the single source
of truth for terminology used throughout the plugin.

---

## 1. 4축 분리 모델 (Four-Axis Separation Model)

MVVM에서 View와 ViewModel의 관계는 두 개의 **직교하는 독립적 축**으로
나뉩니다.

The relationship between View and ViewModel splits into two orthogonal
independent axes.

### 축 1: Composition Direction (View First vs ViewModel First)

Microsoft 공식 정의에 따른 분류 기준은 단 하나입니다 — **composition/
navigation의 lookup key가 무엇인가**.

The single Microsoft-official classification rule is: **what is the lookup
key for composition/navigation?**

| Lookup key | 분류 / Classification |
|---|---|
| View 타입 이름 / View type name (string) | **View First Composition** |
| ViewModel 타입 / ViewModel type | **ViewModel First Composition** |

- **View First Composition** — 식별자가 View 이름. 예:
  - Prism `RequestNavigate("ContentRegion", "HomeView")`
  - Prism `ViewModelLocator.AutoWireViewModel="True"` (View가 anchor)
  - View code-behind `DataContext = new HomeViewModel();`

- **ViewModel First Composition** — 식별자가 ViewModel 타입. 예:
  - WPF implicit DataTemplate
    `<DataTemplate DataType="{x:Type vm:HomeViewModel}">`
  - ViewModel 타입을 target으로 하는 navigation service

참조 / References:
- https://learn.microsoft.com/dotnet/architecture/maui/navigation
- https://learn.microsoft.com/dotnet/architecture/maui/mvvm#connecting-view-models-to-views

### 축 2: ViewModel State Management (Stateful vs Stateless)

- **Stateful ViewModel** — ViewModel 인스턴스가 자신의 상태를 직접 보유
  (한국 WPF 생태계의 사실상 표준).
  ViewModel instances hold their own state directly (de-facto standard in
  the Korean WPF ecosystem).

- **Stateless ViewModel** — 상태는 외부 Manager/Service에 위임, ViewModel은
  transient (Stylet, Caliburn.Micro 권장 스타일).
  State is delegated to an external Manager/Service; ViewModel is transient
  (recommended by Stylet, Caliburn.Micro).

### 두 축의 직교성 (Orthogonality)

| Composition Direction | State Management | 대표 예시 / Typical example |
|---|---|---|
| View First | Stateful | Prism `RegisterForNavigation` + `RegionManager` (wpf-dev-pack Prism path) / 전형적 Prism `ViewModelLocator` 패턴 |
| View First | Stateless | (드묾 / rare) View가 매번 외부 State에서 데이터 fetch |
| **ViewModel First** | **Stateful** | **wpf-dev-pack CommunityToolkit.Mvvm path** (Mappings.xaml + implicit DataTemplate) |
| ViewModel First | Stateless | Stylet의 transient VM 권장 스타일 |

v1.5.x 문서는 "ViewModel First ⇒ Stateless"라는 잘못된 함의를 전제하고
"View First MVVM"이라는 단일 라벨을 강제했으나, 두 축은 **독립**이며
실제 wpf-dev-pack은 두 가지 조합을 모두 강제합니다(아래 §2 참조).

The v1.5.x docs implicitly assumed "ViewModel First ⇒ Stateless" and used a
single "View First MVVM" label. The two axes are in fact **independent**, and
wpf-dev-pack actually enforces two distinct combinations depending on the
chosen MVVM framework (see §2 below).

---

## 2. wpf-dev-pack 공식 채택 조합 (Adopted Combinations)

wpf-dev-pack은 사용자가 선택한 MVVM 프레임워크에 따라 **서로 다른**
composition style을 강제합니다. 두 경로 모두 **Stateful ViewModel**을
공통으로 채택합니다.

wpf-dev-pack enforces **different** composition styles depending on the
chosen MVVM framework. Both paths share **Stateful ViewModel** as the state
management style.

### 2.1 CommunityToolkit.Mvvm 경로 (기본 / default)

> **ViewModel First Composition + Stateful ViewModel**

구체 메커니즘 / Concrete mechanism:
`Mappings.xaml` 기반 implicit DataTemplate 매핑
(implicit DataTemplate mapping via Mappings.xaml)

```xml
<DataTemplate DataType="{x:Type vm:HomeViewModel}">
    <views:HomeView />
</DataTemplate>
```

```csharp
CurrentViewModel = new HomeViewModel();  // ViewModel 인스턴스가 lookup key
                                         // ViewModel instance is the lookup key
```

상세 / Details: [`view-viewmodel-wiring-communitytoolkit.md`](../.claude/rules/view-viewmodel-wiring-communitytoolkit.md)

### 2.2 Prism 9 경로 (대안 / alternative)

> **View First Composition + Stateful ViewModel**

구체 메커니즘 / Concrete mechanism:
`IContainerRegistry.RegisterForNavigation<View, ViewModel>()` 등록 +
`IRegionManager.RequestNavigate("Region", "ViewName")` 네비게이션
(registration + navigation by **view name string**).

```csharp
containerRegistry.RegisterForNavigation<HomeView, HomeViewModel>();
// ...
_regionManager.RequestNavigate("ContentRegion", "HomeView");  // View name이 lookup key
                                                              // View name is the lookup key
```

상세 / Details: [`view-viewmodel-wiring-prism.md`](../.claude/rules/view-viewmodel-wiring-prism.md)

### 2.3 두 경로의 공통점 (Shared)

- **Stateful ViewModel**을 표준 상태 관리 방식으로 채택.
- 프레임워크별로 **단 하나의 매칭 메커니즘**만 허용. 동일 프로젝트 내에
  복수의 View-VM 매칭 경로를 공존시키지 않음.
- ViewModel 클래스가 `System.Windows.*` UI 타입을 참조하지 않음
  (`ICommand` 제외).

- Adopt **Stateful ViewModel** as the standard state management style.
- Allow **exactly one** matching mechanism per framework; do not co-exist
  multiple View-VM matching paths in the same project.
- ViewModel classes do not reference `System.Windows.*` UI types (except
  `ICommand`).

---

## 3. 명시적으로 금지하는 패턴 (Explicitly Prohibited Patterns)

상세 규칙은 [`prohibitions.md`](../.claude/rules/prohibitions.md)을 참조.
See [`prohibitions.md`](../.claude/rules/prohibitions.md) for full rules.

요약 / Summary:

| 금지 패턴 / Prohibited pattern | 분류 / Classification | 금지 이유 / Reason |
|---|---|---|
| Prism `ViewModelLocator.AutoWireViewModel="True"` | View First (Prism의 alternate mechanism) | 동일 프로젝트 내 단일 매칭 경로 강제 — Prism 경로의 표준은 `RegisterForNavigation`임 |
| View code-behind `DataContext = new XxxViewModel()` | View First (imperative) | CommunityToolkit 경로의 ViewModel First 단일성을 깸 |
| `<UserControl.DataContext><vm:XxxVM /></UserControl.DataContext>` | View First (declarative) | 위와 동일 |
| Stateless ViewModel + Transient IoC 등록 패턴 | Stateless VM composition | Stylet/Caliburn 류 별도 framework 체계, plugin 적용 범위 밖 |
| `Mappings.xaml`/`RegisterForNavigation` 외 매칭 메커니즘 도입 | (any) | 단일 매칭 경로 강제 |

---

## 4. v1.5.x와의 용어 차이 (Terminology Change History)

v1.5.x 이하 문서에서는 wpf-dev-pack의 채택 방식을 일괄적으로
**"View First MVVM"** 으로 라벨링했습니다. 이는 Microsoft 공식 정의와
충돌하는 **부정확한 표현**이었습니다 — 특히 CommunityToolkit.Mvvm 경로
(`Mappings.xaml` DataTemplate)는 lookup key가 ViewModel 타입이므로
정확히는 **ViewModel First Composition**입니다.

The pre-v1.5.x docs uniformly labeled wpf-dev-pack's adopted style as
**"View First MVVM"**, which conflicts with Microsoft's official definition.
In particular, the CommunityToolkit.Mvvm path (`Mappings.xaml` DataTemplate)
uses the ViewModel type as the lookup key and is therefore **ViewModel
First Composition**, not View First.

v1.6.4에서 라벨이 다음과 같이 정정되었습니다:
v1.6.4 corrects the labels as follows:

| 시기 / When | 라벨 / Label |
|---|---|
| v1.5.x 이하 | "View First MVVM" (단일 라벨) — 부정확 |
| v1.6.4+ (CommunityToolkit 경로) | "ViewModel First Composition + Stateful ViewModel" |
| v1.6.4+ (Prism 경로) | "View First Composition + Stateful ViewModel" |

> **Plugin이 강제하는 실제 코드 패턴은 변경되지 않았습니다.**
> 라벨만 표준 용어로 정정되었으므로 사용자 코드 수정은 불필요합니다.
>
> **The actual code patterns enforced by the plugin have not changed.**
> Only the labels have been corrected to standard terminology, so no user
> code modification is required.

---

## 5. 참고 문헌 (References)

| 주제 / Topic | URL |
|---|---|
| MVVM — Connecting view models to views | https://learn.microsoft.com/dotnet/architecture/maui/mvvm#connecting-view-models-to-views |
| Navigation — View first vs ViewModel first | https://learn.microsoft.com/dotnet/architecture/maui/navigation |
| CommunityToolkit.Mvvm | https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/ |
| WPF Overview | https://learn.microsoft.com/dotnet/desktop/wpf/overview/ |

---

## 부록 A — 핵심 개념 요약 (Quick Reference)

### View First vs ViewModel First 판정법 (Decision Rule)

> **질문 하나로 판정 / One-question test**:
> composition/navigation의 lookup key가 무엇인가? / What is the lookup key?
> - View name이 key → **View First**
> - ViewModel type이 key → **ViewModel First**

### 패턴별 분류표 (Pattern Classification Table)

| 패턴 / Pattern | Lookup key | 분류 / Classification | plugin 정책 / Policy |
|---|---|---|---|
| `Mappings.xaml` implicit DataTemplate | ViewModel Type | ViewModel First (Stateful) | ✅ 채택 (CommunityToolkit 경로) |
| Prism `RegisterForNavigation` + `RequestNavigate("View")` | View name (string) | View First (Stateful) | ✅ 채택 (Prism 경로) |
| Prism `ViewModelLocator.AutoWireViewModel` | View name | View First (Stateful) | ❌ 금지 (Prism 경로의 표준은 `RegisterForNavigation`) |
| code-behind `DataContext = new VM()` | (View가 VM 직접 선택 / View picks VM) | View First (imperative) | ❌ 금지 |
| Stylet transient VM + naming convention | ViewModel Type | ViewModel First (Stateless) | ❌ 금지 (범위 밖) |
