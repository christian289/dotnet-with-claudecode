# WPF Project 코드 생성 지침

## 핵심 원칙

- UI 커스터마이징 시 WPF Custom Control Library 프로젝트 사용
- Converter, WPF UI Service Layer는 WPF Class Library 프로젝트 사용
- MVVM 프레임워크는 `mvvm-framework.md` 설정에 따라 결정

---

## 1. Dependency Injection

> **📌 상세 가이드**: `/configuring-dependency-injection` skill 참조

- 기본적으로 AddSingleton()만 사용
- DI 컨테이너 구성 방식은 MVVM 프레임워크에 따라 결정:
  - CommunityToolkit.Mvvm → GenericHost (`Microsoft.Extensions.Hosting`)
  - Prism 9 → PrismApplication (`IContainerRegistry`)

---

## 2. 솔루션 및 프로젝트 구조

> **📌 상세 가이드**: `/structuring-wpf-projects` skill 참조

**프로젝트 명명 규칙:**

| 접미사 | 타입 | 용도 |
|--------|------|------|
| `.Abstractions` | .NET Class Library | Interface, abstract class (IoC) |
| `.Core` | .NET Class Library | 비즈니스 로직 (UI 독립) |
| `.ViewModels` | .NET Class Library | MVVM ViewModel (UI 독립) |
| `.WpfServices` | WPF Class Library | WPF 관련 서비스 |
| `.WpfApp` | WPF Application | 실행 진입점 |
| `.UI` | WPF Custom Control Library | 커스텀 컨트롤 |

> ⚠️ Prism 9 사용 시 `.Modules.*` 프로젝트 구조 추가 가능. 상세는 `structuring-wpf-projects/PRISM.md` 참조.

---

## 3. MVVM 패턴

> **📌 MVVM 프레임워크 선택**: `mvvm-framework.md` 참조

### 핵심 제약 (프레임워크 공통)

- **ViewModel 클래스에 UI 프레임워크 의존성 금지**
  - `System.Windows`로 시작하는 클래스 참조 금지
  - 예외: Custom Control 프로젝트 내부 ViewModel
- **MVVM 제약은 ViewModel에만 적용**
  - Converter, Service, Manager는 UI 프레임워크 참조 가능

### 참조 어셈블리 규칙

**ViewModel 프로젝트 참조 금지:**
- ❌ `WindowsBase.dll` (ICollectionView 포함)
- ❌ `PresentationFramework.dll`
- ❌ `PresentationCore.dll`

**ViewModel 프로젝트 참조 가능:**
- ✅ BCL 타입만 (IEnumerable, ObservableCollection 등)
- ✅ MVVM 프레임워크 패키지 (CommunityToolkit.Mvvm 또는 Prism.Core)

---

## 4. XAML 코드 작성

> **📌 상세 가이드**: `/designing-wpf-customcontrol-architecture` skill 참조

- CustomControl + ResourceDictionary를 통한 Stand-Alone Control Style 사용
- Generic.xaml은 MergedDictionaries 허브로만 사용
- 각 컨트롤 스타일을 개별 XAML 파일로 분리

---

## 5. CollectionView MVVM 패턴

> **📌 상세 가이드**: `/managing-wpf-collectionview-mvvm` skill 참조

- Service Layer를 통해 CollectionViewSource 접근 캡슐화
- ViewModel은 IEnumerable만 사용 (WPF 타입 노출 금지)

---

## 6. Popup 포커스 관리

> **📌 상세 가이드**: `/managing-wpf-popup-focus` skill 참조

- Popup 사용 시 PreviewMouseDown 이벤트로 포커스 관리 필수

---

## 7. DataTemplate / Navigation 매핑

> **📌 상세 가이드**: `/mapping-viewmodel-view-datatemplate` skill 참조

- View-ViewModel 매핑 방식은 MVVM 프레임워크에 따라 결정:
  - CommunityToolkit.Mvvm → Mappings.xaml DataTemplate 매핑
  - Prism 9 → RegionManager + RegisterForNavigation

---

## 8. 고성능 렌더링 (DrawingContext)

> **📌 상세 가이드**: `/rendering-with-drawingcontext` skill 참조

- 대량 도형 렌더링 시 Shape 대신 DrawingContext 사용 (10-50배 성능 향상)
- FrameworkElement 상속 후 OnRender에서 직접 그리기
- Pen, Brush, Geometry에 Freeze() 적용 필수
- InvalidateVisual()은 데이터 추가 완료 후 **한 번만** 호출
