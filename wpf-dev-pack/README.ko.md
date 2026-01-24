[🇺🇸 English](./README.md)

<div align="center">

# 🎨 wpf-dev-pack

### Claude Code를 위한 최고의 WPF 개발 도구 키트

[![Version](https://img.shields.io/badge/version-1.3.5-blue.svg)](https://github.com/christian289/dotnet-with-claudecode)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![Claude Code](https://img.shields.io/badge/Claude%20Code-Plugin-orange.svg)](https://claude.ai)

**57개 스킬** · **11개 전문 에이전트** · **5개 명령어** · **1개 MCP 서버**

[설치](#-설치) · [빠른 시작](#-빠른-시작) · [기능](#-기능) · [문서](#-문서)

---

</div>

## ✨ 하이라이트

<table>
<tr>
<td width="50%">

### 🤖 AI 기반 개발
- **11개 전문 에이전트**로 다양한 WPF 작업 수행
- 전략적 결정을 위한 **Opus급** 아키텍트
- WPF 키워드 **자동 감지**

</td>
<td width="50%">

### 🛠️ 완벽한 도구 키트
- WPF 전 영역을 다루는 **57개 스킬**
- 즉시 스캐폴딩을 위한 **5개 명령어**
- **모범 사례** 내장

</td>
</tr>
<tr>
<td width="50%">

### 📚 스마트 문서화
- **MicrosoftDocs** 통합 (내장)
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
| .NET SDK | 10.0+ | hooks 실행용 |
| Claude Code | 최신 | - |
| uv | 최신 | Serena MCP용 |

### 필수 MCP 종속성

wpf-dev-pack의 전체 기능을 사용하려면 다음 MCP 서버가 필요합니다:

| MCP 서버 | 용도 | 사용처 |
|----------|------|--------|
| **Context7** | 최신 라이브러리 문서 | 대부분의 에이전트 |
| **Sequential-thinking** | 단계별 분석 | Opus급 에이전트 |
| **Serena** | 시맨틱 코드 분석 | 모든 에이전트 |

> **참고:** 이미 설치되어 있을 수 있는 일반적으로 사용되는 MCP입니다.
> wpf-dev-pack은 런타임에 가용성을 확인하고 누락된 경우 경고합니다.

**설치되지 않은 경우 `~/.claude/.mcp.json`에 추가:**

```json
{
  "mcpServers": {
    "context7": {
      "command": "npx",
      "args": ["-y", "@upstash/context7-mcp@latest"],
      "windows": {
        "command": "cmd",
        "args": ["/c", "npx", "-y", "@upstash/context7-mcp@latest"]
      }
    },
    "sequential-thinking": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-sequential-thinking"],
      "windows": {
        "command": "cmd",
        "args": ["/c", "npx", "-y", "@modelcontextprotocol/server-sequential-thinking"]
      }
    },
    "serena": {
      "command": "uvx",
      "args": ["--from", "git+https://github.com/oraios/serena", "serena", "start-mcp-server"]
    }
  }
}
```

> **왜 외부인가요?** 이 MCP들은 여러 플러그인에서 공통으로 사용됩니다. wpf-dev-pack에 포함하면 이미 설정한 사용자에게 중복이 발생합니다.

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

## 🧠 자동 트리거 시스템

wpf-dev-pack은 [oh-my-claudecode](https://github.com/Yeachan-Heo/oh-my-claudecode)에서 영감을 받은 지능형 키워드 감지 시스템을 사용합니다. WPF, C#, .NET 키워드를 언급하면 관련 스킬이 **자동으로 활성화**됩니다.

### 작동 방식

1. **키워드 감지**: 프롬프트에서 WPF/.NET 키워드 스캔
2. **스킬 활성화**: 일치하는 스킬 자동 로드
3. **에이전트 추천**: 복잡한 작업에 전문 에이전트 제안

### 트리거 예시

| 사용자 입력 | 자동 활성화 |
|-------------|-------------|
| "CustomControl 만들어줘" | `authoring-wpf-controls`, `developing-wpf-customcontrols` |
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
| 🏗️ **wpf-architect-low** | Sonnet | 아키텍처 분석 (Claude Pro) |
| 🎨 **wpf-control-designer** | Sonnet | CustomControl 구현 |
| 📐 **wpf-xaml-designer** | Sonnet | XAML 스타일 및 템플릿 |
| 🔄 **wpf-mvvm-expert** | Sonnet | MVVM 패턴 및 CommunityToolkit |
| 🔗 **wpf-data-binding-expert** | Sonnet | 복잡한 바인딩 및 유효성 검사 |
| ⚡ **wpf-performance-optimizer** | Sonnet | 렌더링 및 성능 |
| 🔍 **wpf-code-reviewer** | Opus | 코드 품질 분석 |
| 🔍 **wpf-code-reviewer-low** | Sonnet | 코드 리뷰 (Claude Pro) |
| 📝 **code-formatter** | Haiku | C# 서식 및 스타일 |
| 🔧 **serena-initializer** | Haiku | 프로젝트 설정 |

### 🔌 MCP 서버

**내장:**

| 서버 | 용도 |
|------|------|
| **MicrosoftDocs** | 공식 Microsoft 문서 |

**필수 (외부):**

| 서버 | 용도 | 비고 |
|------|------|------|
| **Context7** | 최신 라이브러리 문서 | 별도 설치 |
| **Sequential-thinking** | 단계별 분석 | 별도 설치 |
| **Serena** | 시맨틱 코드 분석 | 별도 설치 |

> [필수 MCP 종속성](#필수-mcp-종속성)에서 설치 방법을 확인하세요.

### 📚 카테고리별 스킬

<details>
<summary><b>🎨 UI & 컨트롤 (15개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `authoring-wpf-controls` | 컨트롤 작성 패턴 |
| `customizing-controltemplate` | ControlTemplate 커스터마이징 |
| `designing-wpf-customcontrol-architecture` | CustomControl 아키텍처 |
| `developing-wpf-customcontrols` | CustomControl 개발 |
| `implementing-wpf-adorners` | Adorner 구현 |
| `understanding-wpf-content-model` | Content Model 패턴 |
| `creating-wpf-dialogs` | 대화상자 |
| `creating-wpf-flowdocument` | FlowDocument 생성 |
| `using-wpf-behaviors-triggers` | Behavior & Trigger |
| `using-xaml-property-element-syntax` | XAML 구문 패턴 |
| `using-converter-markup-extension` | Converter 패턴 |
| `displaying-slider-index` | Slider UI 패턴 |
| `binding-enum-command-parameters` | Enum 바인딩 패턴 |
| `localizing-wpf-applications` | 지역화 |
| `implementing-wpf-automation` | UI Automation |

</details>

<details>
<summary><b>🔗 데이터 바인딩 & MVVM (8개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `implementing-communitytoolkit-mvvm` | CommunityToolkit.Mvvm |
| `advanced-data-binding` | 고급 바인딩 패턴 |
| `implementing-wpf-validation` | 유효성 검사 전략 |
| `managing-wpf-collectionview-mvvm` | MVVM에서 CollectionView |
| `mapping-viewmodel-view-datatemplate` | View-ViewModel 매핑 |
| `configuring-dependency-injection` | DI 설정 |
| `defining-wpf-dependencyproperty` | DependencyProperty |
| `structuring-wpf-projects` | 프로젝트 구조 |

</details>

<details>
<summary><b>⚡ 성능 & 렌더링 (10개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `rendering-with-drawingcontext` | DrawingContext 렌더링 |
| `rendering-with-drawingvisual` | DrawingVisual 렌더링 |
| `rendering-wpf-architecture` | 렌더링 아키텍처 |
| `rendering-wpf-high-performance` | 고성능 렌더링 |
| `implementing-2d-graphics` | 2D 그래픽 |
| `implementing-hit-testing` | 히트 테스트 |
| `virtualizing-wpf-ui` | UI 가상화 |
| `optimizing-wpf-memory` | 메모리 최적화 |
| `checking-image-bounds-transform` | 이미지 변환 |
| `navigating-visual-logical-tree` | 트리 탐색 |

</details>

<details>
<summary><b>🎬 애니메이션 & 미디어 (3개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `creating-wpf-animations` | 애니메이션 생성 |
| `integrating-wpf-media` | 미디어 통합 |
| `using-wpf-clipboard` | 클립보드 작업 |

</details>

<details>
<summary><b>⌨️ 입력 & 이벤트 (4개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `handling-wpf-input-commands` | 입력 & 명령 |
| `routing-wpf-events` | 라우티드 이벤트 |
| `implementing-wpf-dragdrop` | 드래그 앤 드롭 |
| `managing-wpf-popup-focus` | Popup 포커스 관리 |

</details>

<details>
<summary><b>🎨 스타일링 & 리소스 (3개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `managing-styles-resourcedictionary` | 스타일 & 리소스 |
| `resolving-icon-font-inheritance` | 아이콘 폰트 |
| `formatting-wpf-csharp-code` | 코드 서식 |

</details>

<details>
<summary><b>🔧 애플리케이션 & 스레딩 (3개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `managing-wpf-application-lifecycle` | 앱 생명주기 |
| `threading-wpf-dispatcher` | Dispatcher & 스레딩 |
| `migrating-wpf-to-dotnet` | .NET 마이그레이션 |

</details>

<details>
<summary><b>🔷 .NET 공통 (12개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `configuring-console-app-di` | 콘솔 앱 DI |
| `handling-async-operations` | 비동기 패턴 |
| `implementing-io-pipelines` | I/O 파이프라인 |
| `implementing-pubsub-pattern` | Pub/Sub 패턴 |
| `implementing-repository-pattern` | Repository 패턴 |
| `managing-literal-strings` | 문자열 관리 |
| `optimizing-fast-lookup` | 빠른 조회 |
| `optimizing-io-operations` | I/O 최적화 |
| `optimizing-memory-allocation` | 메모리 할당 |
| `processing-parallel-tasks` | 병렬 처리 |
| `using-generated-regex` | Source-generated regex |

</details>

---

## 📁 플러그인 구조

```
wpf-dev-pack/
├── 📁 .claude-plugin/
│   └── plugin.json           # 플러그인 매니페스트
├── 📁 agents/                 # 11개 전문 에이전트
│   ├── wpf-architect.md           # Opus
│   ├── wpf-architect-low.md       # Sonnet (Claude Pro)
│   ├── wpf-code-reviewer.md       # Opus
│   ├── wpf-code-reviewer-low.md   # Sonnet (Claude Pro)
│   ├── wpf-control-designer.md    # Sonnet
│   ├── wpf-xaml-designer.md       # Sonnet
│   ├── wpf-mvvm-expert.md         # Sonnet
│   ├── wpf-data-binding-expert.md # Sonnet
│   ├── wpf-performance-optimizer.md # Sonnet
│   ├── code-formatter.md          # Haiku
│   └── serena-initializer.md      # Haiku
├── 📁 commands/               # 5개 사용자 명령어
│   ├── make-wpf-custom-control/
│   ├── make-wpf-project/
│   ├── make-wpf-converter/
│   ├── make-wpf-behavior/
│   └── make-wpf-usercontrol/
├── 📁 skills/                 # 57개 스킬
├── 📁 hooks/                  # 이벤트 훅
├── 📄 .mcp.json               # MCP 설정 (MicrosoftDocs만)
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
