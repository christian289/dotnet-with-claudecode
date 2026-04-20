[🇺🇸 English](./README.md)

<div align="center">

# 🎨 wpf-dev-pack

### Claude Code를 위한 최고의 WPF 개발 도구 키트

[![Version](https://img.shields.io/badge/version-1.6.2-blue.svg)](https://github.com/christian289/dotnet-with-claudecode)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET_SDK-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![Claude Code](https://img.shields.io/badge/Claude%20Code-Plugin-orange.svg)](https://claude.ai)

**55개 스킬** · **10개 전문 에이전트** · **1개 MCP 서버**

[설치](#-설치) · [빠른 시작](#-빠른-시작) · [기능](#-기능) · [문서](#-문서)

---

</div>

## ✨ 하이라이트

> **MVVM 방식**: wpf-dev-pack은 **View First MVVM**을 채택합니다. View가 먼저 생성되고 **DataTemplate 매핑** 또는 **DI 컨테이너 직접 resolve**로 ViewModel을 결정합니다. ViewModelLocator는 금지입니다(`.claude/rules/prohibitions.md` 참조). ViewModel First 방식(ViewModel이 View를 생성)은 사용하지 않습니다.

<table>
<tr>
<td width="50%">

### 🤖 AI 기반 개발
- **10개 전문 에이전트**로 다양한 WPF 작업 수행
- 전략적 결정을 위한 **Opus급** 아키텍트
- WPF 키워드 **자동 감지**
- 듀얼 프레임워크 지원을 위한 **Prism 9** 컴패니언 파일

</td>
<td width="50%">

### 🛠️ 완벽한 도구 키트
- WPF 전 영역을 다루는 **55개 스킬**
- **모범 사례** 내장

</td>
</tr>
<tr>
<td width="50%">

### 📚 스마트 문서화
- **Microsoft Learn** 플러그인 (마켓플레이스에서 설치)
- 최신 문서를 위한 **Context7** (외부)
- 시맨틱 코드 분석을 위한 **Serena** (외부)

</td>
<td width="50%">

### ⚡ 고성능
- **DrawingContext** 렌더링 패턴
- **가상화** 전략
- **메모리 최적화** 기법

</td>
</tr>
</table>

---

## 📦 설치

### 마켓플레이스에서 설치 (권장)

```bash
# 1단계: 마켓플레이스 추가 (최초 1회)
/plugin marketplace add christian289/dotnet-with-claudecode

# 2단계: 플러그인 설치
/plugin install wpf-dev-pack@dotnet-claude-plugins
```

### 로컬 설치

```bash
claude --plugin-dir ./wpf-dev-pack
```

### 업데이트

```bash
# 수동 업데이트
claude plugin update wpf-dev-pack@dotnet-claude-plugins

# 또는 마켓플레이스 자동 업데이트 활성화
/plugin → Marketplaces → dotnet-claude-plugins → Enable auto-update
```

> **참고:** 서드파티 마켓플레이스는 기본적으로 자동 업데이트가 비활성화되어 있습니다.

### 요구사항

| 요구사항 | 버전 | 비고 |
|----------|------|------|
| .NET SDK | **10.0+** | file-based app 훅 실행에 필수 |
| Claude Code | 최신 | - |
| uv | 최신 | Serena MCP용 |

> **대상 프레임워크 vs SDK**: .NET 10 SDK는 **wpf-dev-pack 실행**에 필요합니다 (훅이 file-based app 사용).
> 생성되는 WPF 프로젝트는 **.NET 8 이상을 대상**으로 설정할 수 있습니다 — 필요시 .NET 10과 함께 해당 버전 SDK를 설치하세요.

### 필수 플러그인 종속성

wpf-dev-pack 에이전트는 다음 Claude Code 플러그인이 별도로 설치되어야 합니다:

| 플러그인 | MCP 서버 | 용도 |
|---------|----------|------|
| **[context7](https://github.com/upstash/context7)** | context7 | 최신 라이브러리/프레임워크 문서 조회 |
| **[serena](https://github.com/oraios/serena)** | serena | 시맨틱 코드 분석, 심볼 네비게이션 |
| **[microsoft-docs](https://github.com/MicrosoftDocs/mcp)** | microsoft-learn | 공식 Microsoft 문서 및 코드 샘플 조회 |
| **[csharp-lsp](https://github.com/razzmatazz/csharp-language-server)** | csharp | C# Language Server Protocol (정의, 참조, 진단) |

> **참고:** wpf-dev-pack은 런타임에 플러그인 가용성을 확인하고 누락된 경우 경고합니다.

Claude Code 마켓플레이스 또는 `/install-plugin` 명령으로 설치하세요.

---

## 🚀 빠른 시작

### 새 WPF 프로젝트 생성

```bash
# CommunityToolkit.Mvvm 사용 (권장)
/wpf-dev-pack:make-wpf-project MyApp

# Prism Framework 사용
/wpf-dev-pack:make-wpf-project MyApp --prism
```

### 컴포넌트 생성

```bash
# CustomControl
/wpf-dev-pack:make-wpf-custom-control MyButton Button

# UserControl
/wpf-dev-pack:make-wpf-usercontrol SearchBox

# Converter
/wpf-dev-pack:make-wpf-converter BoolToVisibility

# Behavior
/wpf-dev-pack:make-wpf-behavior SelectAllOnFocus TextBox
```

### 도움 요청

```
"고성능 차트 컨트롤은 어떻게 만드나요?"
"이 ViewModel을 MVVM 관점에서 검토해주세요"
"대용량 데이터셋을 위해 이 렌더링 코드를 최적화해주세요"
```

---

## 🎯 요구사항 인터뷰 시스템

`wpf-architect`를 호출하면 **적응형 경로 기반 인터뷰**가 정확한 요구사항을 파악합니다:

### 작동 방식

```
Step 1: 작업 유형 선택
   ├─→ 경로 A: 새 프로젝트 생성 (7단계)
   ├─→ 경로 B: 기존 분석/개선 (5단계)
   ├─→ 경로 C: 기능 구현 (5단계)
   └─→ 경로 D: 디버깅/수정 (4단계)
```

각 경로는 자유 입력 단계에서 **키워드 분석**을 통해 후속 단계의 기본값을 자동 설정합니다.

### 인터뷰 경로

| 경로 | 작업 유형 | 단계 | 초점 |
|------|-----------|:----:|------|
| **A** | 새 프로젝트 생성 | 7 | 컨셉 → 아키텍처 → 규모 → 복잡도 → 라이브러리 → 기능 영역 |
| **B** | 분석/개선 | 5 | 분석 목표 → 분석 모드 → 범위 → 출력 형식 |
| **C** | 기능 구현 | 5 | 기능 설명 → 구현 방식 → 라이브러리 → 기능 영역 |
| **D** | 디버깅/수정 | 4 | 문제 증상 → 문제 유형 → 문제 영역 |

### 예시 플로우 (경로 A)

```
User: "WPF로 차트 앱 만들고 싶어요"

wpf-architect: [A-1] 어떤 앱인가요? 컨셉을 설명해주세요.
   → User: "실시간 주식 차트 대시보드"
   (키워드 감지: "차트", "실시간" → LiveCharts2, 성능 기본값 자동 설정)

wpf-architect: [A-2] 아키텍처 패턴은?
   → User 선택: "MVVM + CommunityToolkit"

wpf-architect: [A-3] 프로젝트 규모는?
   → User 선택: "중간 (5-15개 View)"

wpf-architect: [A-5] 서드파티 라이브러리?
   → 자동 추천: LiveCharts2 ✓, WPF-UI (선택)

결과: LiveCharts2 + DrawingContext 스킬 + wpf-performance-optimizer 활성화
```

---

## 🧠 자동 트리거 시스템

wpf-dev-pack은 [oh-my-claudecode](https://github.com/Yeachan-Heo/oh-my-claudecode)에서 영감을 받은 지능형 키워드 감지 시스템을 사용합니다. WPF, C#, .NET 키워드를 언급하면 관련 스킬이 **자동으로 활성화**됩니다.

### 작동 방식

1. **키워드 감지**: 프롬프트에서 WPF/.NET 키워드 스캔
2. **스킬 활성화**: 일치하는 스킬 자동 로드
3. **에이전트 추천**: 복잡한 작업에 전문 에이전트 제안

### 트리거 예시

| 사용자 입력 | 자동 활성화 |
|-------------|-------------|
| "CustomControl 만들어줘" | `authoring-wpf-controls` |
| "MVVM 패턴 적용" | `implementing-communitytoolkit-mvvm` |
| "DrawingContext로 렌더링" | `rendering-with-drawingcontext` |
| "성능 최적화 필요" | `rendering-wpf-high-performance` + `wpf-performance-optimizer` 에이전트 |
| "아키텍처 검토" | `wpf-architect` 에이전트 추천 |

### 무음 트리거

일부 스킬은 알림 없이 활성화됩니다:
- `formatting-wpf-csharp-code` - 코드 서식
- `using-xaml-property-element-syntax` - XAML 구문
- `managing-literal-strings` - 문자열 관리

### 키워드 카테고리

<details>
<summary><b>📌 주요 WPF 키워드 (펼치려면 클릭)</b></summary>

| 카테고리 | 키워드 |
|----------|--------|
| **컨트롤** | `customcontrol`, `dependencyproperty`, `templatepart`, `controltemplate` |
| **MVVM** | `mvvm`, `viewmodel`, `relaycommand`, `observableproperty` |
| **렌더링** | `drawingcontext`, `drawingvisual`, `onrender`, `invalidatevisual` |
| **성능** | `virtualizingstackpanel`, `freeze`, `freezable`, `bitmapcache` |
| **이벤트** | `routedevent`, `command`, `inputbinding`, `dragdrop` |
| **스타일링** | `resourcedictionary`, `generic.xaml`, `storyboard`, `animation` |
| **스레딩** | `dispatcher`, `invoke`, `begininvoke` |

</details>

<details>
<summary><b>🔷 .NET 키워드 (펼치려면 클릭)</b></summary>

| 카테고리 | 키워드 |
|----------|--------|
| **비동기** | `async`, `await`, `task`, `valuetask`, `configureawait` |
| **병렬** | `parallel`, `plinq`, `concurrentdictionary` |
| **메모리** | `span`, `memory<`, `arraypool`, `stackalloc` |
| **I/O** | `pipeline`, `pipereader`, `pipewriter` |
| **패턴** | `repository pattern`, `pubsub`, `channel` |

</details>

---

## 🎯 기능

### 🤖 전문 에이전트

> **Claude Pro 사용자**: `wpf-architect`와 `wpf-code-reviewer`는 `-low` 버전 사용 (Opus → Sonnet)

| 에이전트 | 모델 | 전문 분야 |
|----------|:----:|-----------|
| 🏗️ **wpf-architect** | Opus | 전략적 아키텍처 및 설계 결정 |
| 🎨 **wpf-control-designer** | Sonnet | CustomControl 구현 |
| 📐 **wpf-xaml-designer** | Sonnet | XAML 스타일 및 템플릿 |
| 🔄 **wpf-mvvm-expert** | Sonnet | MVVM 패턴 및 CommunityToolkit |
| 🔗 **wpf-data-binding-expert** | Sonnet | 복잡한 바인딩 및 유효성 검사 |
| ⚡ **wpf-performance-optimizer** | Sonnet | 렌더링 및 성능 |
| 🔍 **wpf-code-reviewer** | Opus | 코드 품질 분석 |
| 📝 **code-formatter** | Haiku | C# 서식 및 스타일 |
| 🔧 **serena-initializer** | Haiku | 프로젝트 설정 |

### 🔌 MCP 서버

| 플러그인 | MCP 서버 | 용도 |
|---------|----------|------|
| **HandMirrorMcp** | HandMirrorMcp | .NET 어셈블리/NuGet 검사 (내장) |
| **context7** | context7 | 라이브러리/프레임워크 문서 |
| **sequential-thinking** | sequential-thinking | 단계별 분석 |
| **serena** | serena | 시맨틱 코드 분석 |
| **microsoft-docs** | microsoft-learn | 공식 Microsoft 문서 |
| **csharp-lsp** | csharp | C# LSP 코드 인텔리전스 |

> [필수 플러그인 종속성](#필수-플러그인-종속성)에서 설치 방법을 확인하세요.

### 📚 카테고리별 스킬

<details>
<summary><b>🎨 UI & 컨트롤 (5개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `authoring-wpf-controls` | 컨트롤 작성 패턴 |
| `designing-wpf-customcontrol-architecture` | CustomControl 아키텍처 |
| `displaying-slider-index` | Slider UI 패턴 |
| `binding-enum-command-parameters` | Enum 바인딩 패턴 |
| `configuring-wpf-themeinfo` | ThemeInfo 설정 |

</details>

<details>
<summary><b>🔗 데이터 바인딩 & MVVM (7개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `implementing-communitytoolkit-mvvm` | CommunityToolkit.Mvvm |
| `advanced-data-binding` | 고급 바인딩 패턴 (MultiBinding, PriorityBinding) |
| `using-converter-markup-extension` | Converter MarkupExtension 패턴 |
| `implementing-wpf-validation` | 유효성 검사 전략 |
| `managing-wpf-collectionview-mvvm` | MVVM에서 CollectionView |
| `configuring-dependency-injection` | DI 설정 |
| `structuring-wpf-projects` | 프로젝트 구조 |

</details>

<details>
<summary><b>⚡ 성능 & 렌더링 (9개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `rendering-with-drawingcontext` | DrawingContext 렌더링 |
| `rendering-with-drawingvisual` | DrawingVisual 렌더링 |
| `rendering-wpf-architecture` | 렌더링 아키텍처 |
| `rendering-wpf-high-performance` | 고성능 렌더링 |
| `implementing-hit-testing` | 히트 테스트 |
| `virtualizing-wpf-ui` | UI 가상화 |
| `optimizing-wpf-memory` | 메모리 최적화 |
| `checking-image-bounds-transform` | 이미지 변환 |
| `navigating-visual-logical-tree` | 트리 탐색 |

</details>

<details>
<summary><b>⌨️ 입력 & 이벤트 (2개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `routing-wpf-events` | 라우티드 이벤트 |
| `managing-wpf-popup-focus` | Popup 포커스 관리 |

</details>

<details>
<summary><b>🎨 스타일링 & 리소스 (4개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `managing-styles-resourcedictionary` | 스타일 & 리소스 |
| `resolving-icon-font-inheritance` | 아이콘 폰트 |
| `using-xaml-property-element-syntax` | XAML 속성 요소 구문 |
| `formatting-wpf-csharp-code` | 코드 서식 |

</details>

<details>
<summary><b>🔧 애플리케이션 & 스레딩 (6개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `managing-wpf-application-lifecycle` | 앱 생명주기 |
| `threading-wpf-dispatcher` | Dispatcher & 스레딩 |
| `preventing-dispatcher-deadlock` | 이벤트 핸들러의 sync-over-async 데드락 방지 |
| `shutting-down-wpf-gracefully` | OnMainWindowClose / OnExplicitShutdown 기반 비동기 종료 |
| `embedding-pdb-in-exe` | PDB 임베딩 |
| `publishing-wpf-apps` | 배포 & 인스톨러 |

</details>

<details>
<summary><b>📦 서드파티 라이브러리 (8개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `integrating-wpfui-fluent` | WPF-UI (Wpf.Ui) Fluent Design 통합 |
| `integrating-livecharts2` | LiveCharts2 차트 라이브러리 |
| `validating-with-fluentvalidation` | FluentValidation + INotifyDataErrorInfo 브리지 |
| `handling-errors-with-erroror` | ErrorOr 결과 패턴 (서비스 계층) |
| `integrating-nodify` | Nodify 노드 기반 에디터 컨트롤 |
| `scottplot-syncing-modifier-keys-for-mousewheel` | ScottPlot 마우스 휠 줌 수정자 키 동기화 |
| `flaui-cross-process-input` | FlaUI 크로스 프로세스 입력 보정 |
| `flaui-wpf-element-discovery` | FlaUI WPF 요소 탐색 문제 해결 |

</details>

<details>
<summary><b>🔷 .NET 공통 (3개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `configuring-console-app-di` | 콘솔 앱 DI |
| `implementing-repository-pattern` | Repository 패턴 |
| `managing-literal-strings` | 문자열 관리 |

</details>

<details>
<summary><b>🔄 Prism 9 컴패니언 (13개 PRISM.md 파일)</b></summary>

12개 스킬에 Prism 9 (Community License) 대응 `PRISM.md` 컴패니언 파일을 제공합니다:

| 스킬 | PRISM.md 주요 내용 |
|------|-------------------|
| `implementing-communitytoolkit-mvvm` | BindableBase, SetProperty, DelegateCommand |
| `configuring-dependency-injection` | PrismApplication, IContainerRegistry |
| `structuring-wpf-projects` | IModule 기반 모듈 아키텍처 |
| `managing-wpf-application-lifecycle` | PrismApplication 라이프사이클 |
| `binding-enum-command-parameters` | DelegateCommand\<T\> |
| `implementing-wpf-validation` | ValidatableBindableBase |
| `managing-wpf-collectionview-mvvm` | BindableBase + IContainerRegistry |
| `validating-with-fluentvalidation` | ValidatableBindableBase\<T\> 브릿지 |
| `implementing-repository-pattern` | IContainerRegistry DI |
| `displaying-slider-index` | SetProperty + RaisePropertyChanged |

> 각 스킬의 SKILL.md (CommunityToolkit.Mvvm)와 PRISM.md (Prism 9)는 상호 참조합니다.

</details>

<details>
<summary><b>🧪 테스트 (1개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `testing-wpf-viewmodels` | xUnit + NSubstitute로 ViewModel 단위 테스트 |

</details>

<details>
<summary><b>🏗️ 스캐폴딩 (7개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `make-wpf-project` | MVVM/DI 포함 WPF 프로젝트 스캐폴딩 |
| `make-wpf-custom-control` | CustomControl 생성 |
| `make-wpf-usercontrol` | UserControl 생성 |
| `make-wpf-converter` | IValueConverter 생성 |
| `make-wpf-behavior` | Behavior<T> 생성 |
| `make-wpf-viewmodel` | ViewModel + View + DI + DataTemplate 매핑 생성 |
| `make-wpf-service` | 서비스 인터페이스 + 구현 + DI 등록 |

</details>

---

## 📁 플러그인 구조

```
wpf-dev-pack/
├── 📁 .claude-plugin/
│   └── plugin.json           # 플러그인 매니페스트
├── 📁 agents/                 # 10개 전문 에이전트
│   ├── wpf-architect.md           # Opus
│   ├── wpf-code-reviewer.md       # Opus
│   ├── wpf-control-designer.md    # Sonnet
│   ├── wpf-xaml-designer.md       # Sonnet
│   ├── wpf-mvvm-expert.md         # Sonnet
│   ├── wpf-data-binding-expert.md # Sonnet
│   ├── wpf-performance-optimizer.md # Sonnet
│   ├── code-formatter.md          # Haiku
│   └── serena-initializer.md      # Haiku
├── 📁 skills/                 # 55개 스킬
├── 📁 hooks/                  # 이벤트 훅
├── 📄 .mcp.json               # MCP 설정 (HandMirrorMcp만)
├── 📄 README.md
└── 📄 LICENSE
```

---

## 🔧 설정

### Serena MCP 설정

> ⚠️ **필수**: Serena를 사용하려면 [uv](https://docs.astral.sh/uv/)를 설치하세요.

```bash
# Serena 로컬 테스트
uvx --from git+https://github.com/oraios/serena serena start-mcp-server
```

### C# LSP (IntelliSense용 필수)

```bash
claude /install-plugin csharp-lsp
```

---

## 📖 문서

### 공식 참고 자료

- 📘 [WPF Samples (Microsoft)](https://github.com/microsoft/WPF-Samples)
- 📗 [WPF Graphics & Multimedia](https://learn.microsoft.com/dotnet/desktop/wpf/graphics-multimedia/)
- 📙 [Claude Code Plugin Spec](https://code.claude.com/docs/en/plugins-reference)

### 아키텍처 참고

- [oh-my-claudecode](https://github.com/Yeachan-Heo/oh-my-claudecode) - 에이전트 기반 오케스트레이션 패턴

---

## 🤝 기여

기여를 환영합니다! Pull Request를 자유롭게 제출해주세요.

---

## 📄 라이선스

MIT 라이선스 - 자세한 내용은 [LICENSE](LICENSE)를 참조하세요.

---

<div align="center">

**Made with ❤️ by vincent lee**

[⬆ 맨 위로](#-wpf-dev-pack)

</div>
