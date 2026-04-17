[🇺🇸 English](./README.md)

# Skills

WPF 및 .NET 개발을 위한 전문 스킬입니다.

> **📦 아카이브**: 공식 문서와 중복되는 스킬은 `../../archive-skills/`로 이동되었습니다. `microsoft-docs` MCP 플러그인으로 커버되는 주제(DependencyProperty, ControlTemplate, Storyboard, DragDrop, async/await, Span<T>, GeneratedRegex 등)는 더 이상 활성 스킬이 아닙니다.

## 카테고리별 스킬

### 🎨 UI & 컨트롤

| 스킬 | 설명 |
|------|------|
| `authoring-wpf-controls` | 컨트롤 작성 결정 가이드 (UserControl/Control/FrameworkElement) |
| `designing-wpf-customcontrol-architecture` | CustomControl 아키텍처 |
| `configuring-wpf-themeinfo` | ThemeInfo 설정 |
| `binding-enum-command-parameters` | Enum 바인딩 패턴 |
| `displaying-slider-index` | Slider UI 패턴 |

### 🔗 데이터 바인딩 & MVVM

| 스킬 | 설명 |
|------|------|
| `implementing-communitytoolkit-mvvm` | CommunityToolkit.Mvvm |
| `implementing-wpf-validation` | 유효성 검사 전략 |
| `managing-wpf-collectionview-mvvm` | MVVM의 CollectionView |
| `mapping-viewmodel-view-datatemplate` | View-ViewModel 매핑 |
| `configuring-dependency-injection` | DI 설정 |
| `structuring-wpf-projects` | 프로젝트 구조 |

### ⚡ 성능 & 렌더링

| 스킬 | 설명 |
|------|------|
| `rendering-with-drawingcontext` | DrawingContext 렌더링 |
| `rendering-with-drawingvisual` | DrawingVisual 렌더링 |
| `rendering-wpf-architecture` | 렌더링 아키텍처 |
| `rendering-wpf-high-performance` | 고성능 렌더링 |
| `implementing-hit-testing` | Hit Testing |
| `virtualizing-wpf-ui` | UI 가상화 |
| `optimizing-wpf-memory` | 메모리 최적화 (누수 패턴) |
| `checking-image-bounds-transform` | 이미지 변환 |
| `navigating-visual-logical-tree` | 트리 탐색 헬퍼 |

### ⌨️ 입력 & 이벤트

| 스킬 | 설명 |
|------|------|
| `routing-wpf-events` | 라우티드 이벤트 (커스텀 이벤트 생성) |
| `managing-wpf-popup-focus` | Popup 포커스 관리 |

### 🎨 스타일 & 리소스

| 스킬 | 설명 |
|------|------|
| `managing-styles-resourcedictionary` | 스타일 & 리소스 |
| `resolving-icon-font-inheritance` | 아이콘 폰트 |
| `formatting-wpf-csharp-code` | 코드 포매팅 |

### 🔧 애플리케이션 & 스레딩

| 스킬 | 설명 |
|------|------|
| `managing-wpf-application-lifecycle` | 앱 수명주기 (Single Instance, IPC) |
| `threading-wpf-dispatcher` | Dispatcher & 스레딩 |

### 🔷 .NET 공통

| 스킬 | 설명 |
|------|------|
| `configuring-console-app-di` | 콘솔 앱 DI |
| `implementing-repository-pattern` | Repository 패턴 |
| `managing-literal-strings` | 문자열 관리 |

### 🧪 테스트

| 스킬 | 설명 |
|------|------|
| `testing-wpf-viewmodels` | ViewModel xUnit 테스트 |
| `managing-unit-tests` | 단위 테스트 전략 |

### 🛠️ 스캐폴딩

| 스킬 | 설명 |
|------|------|
| `make-wpf-project` | WPF 프로젝트 스캐폴드 |
| `make-wpf-custom-control` | CustomControl 스캐폴드 |
| `make-wpf-usercontrol` | UserControl 스캐폴드 |
| `make-wpf-converter` | IValueConverter 스캐폴드 |
| `make-wpf-behavior` | Behavior 스캐폴드 |
| `make-wpf-viewmodel` | ViewModel + View + DataTemplate 스캐폴드 |
| `make-wpf-service` | 서비스 클래스 스캐폴드 |

### 🔌 3rd Party 라이브러리

| 스킬 | 설명 |
|------|------|
| `integrating-wpfui-fluent` | WPF-UI Fluent Design |
| `integrating-livecharts2` | LiveCharts2 데이터 시각화 |
| `integrating-nodify` | Nodify 노드 에디터 |
| `validating-with-fluentvalidation` | FluentValidation 통합 |
| `handling-errors-with-erroror` | ErrorOr 결과 패턴 |
| `flaui-cross-process-input` | FlaUI 크로스 프로세스 입력 수정 |
| `flaui-prism-dialog-discovery` | FlaUI + Prism 다이얼로그 탐색 |
| `flaui-wpf-element-discovery` | FlaUI WPF 요소 탐색 |
| `scottplot-syncing-modifier-keys-for-mousewheel` | ScottPlot 수정자 키 동기화 |

### 📦 빌드 & 배포

| 스킬 | 설명 |
|------|------|
| `embedding-pdb-in-exe` | PDB 임베드 |
| `publishing-wpf-apps` | 배포 & 인스톨러 가이드 |

## 스킬 구조

각 스킬 디렉토리 구성:
- `SKILL.md` - 메인 스킬 문서
- `PRISM.md` - Prism 9 컴패니언 (해당 시)
- `QUICKREF.md` - 빠른 참조 (선택)
- 추가 리소스 (스크립트, 템플릿)

## 사용법

스킬은 키워드로 자동 트리거되거나 직접 호출 가능:

```
/wpf-dev-pack:implementing-communitytoolkit-mvvm
```
