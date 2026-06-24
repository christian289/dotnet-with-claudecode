[🇺🇸 English](./README.md)

<div align="center">

# 🎨 wpf-dev-pack

### Claude Code를 위한 최고의 WPF 개발 도구 키트

[![Version](https://img.shields.io/badge/version-1.8.0-blue.svg)](https://github.com/christian289/dotnet-with-claudecode)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET_SDK-10.0.300+-purple.svg)](https://dotnet.microsoft.com/)
[![Claude Code](https://img.shields.io/badge/Claude%20Code-Plugin-orange.svg)](https://claude.ai)

**28개 스킬** · **10개 전문 에이전트** · **2개 MCP 서버**

[설치](#-설치) · [빠른 시작](#-빠른-시작) · [기능](#-기능) · [문서](#-문서)

---

</div>

## ✨ 하이라이트

> **MVVM Composition 방식**: wpf-dev-pack은 선택한 MVVM 프레임워크에 따라 **단일 매칭 경로**를 강제합니다. 두 경로 모두 **Stateful ViewModel**을 사용합니다:
> - **CommunityToolkit.Mvvm** (기본) → **ViewModel First Composition**, `Mappings.xaml` + implicit DataTemplate.
> - **Prism 9** (대안) → **View First Composition**, `RegisterForNavigation` + `IRegionManager.RequestNavigate`.
>
> Prism `ViewModelLocator.AutoWireViewModel`, View code-behind `DataContext = new VM()`, XAML 인라인 `DataContext`, Stateless-VM 패턴은 모두 금지됩니다 ([`wpf-rule-prohibitions`](./skills/wpf-rule-prohibitions/SKILL.md) 및 [`docs/TERMINOLOGY.ko.md`](./docs/TERMINOLOGY.ko.md) 참조).
>
> v1.6.4 이전 문서는 이 둘을 일괄적으로 "View First MVVM"으로 라벨링했으나, 이는 Microsoft 공식 정의와 충돌했습니다 (`Mappings.xaml`의 lookup key는 ViewModel 타입이므로 ViewModel First). v1.6.4에서 경로별 라벨로 정정되었으며, 강제되는 코드 규칙은 변경되지 않았습니다.

<table>
<tr>
<td width="50%">

### 🤖 AI 기반 개발
- **10개 전문 에이전트**로 다양한 WPF 작업 수행
- **세션 모델 그대로 사용** — 에이전트가 현재 모델을 상속
- **MCP 기반 지식 제공** — WpfDevPackMcp가 답변 전 검색(search-before-answer)
- 듀얼 프레임워크 지원을 위한 **Prism 9** 컴패니언 파일

</td>
<td width="50%">

### 🛠️ 완벽한 도구 키트
- **19개 커맨드 스킬** (+ 에이전트에 preload되는 내부 rule-skill 9개) + MCP로 온디맨드 제공되는 WPF 지식
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
| .NET SDK | **10.0.300+** | file-based app 훅 실행에 필수 |
| Claude Code | 최신 | - |
| uv | 최신 | Serena MCP용 |

> **대상 프레임워크 vs SDK**: .NET SDK 10.0.300 이상은 **wpf-dev-pack 실행**에 필요합니다 (훅이 file-based app 사용).
> 생성되는 WPF 프로젝트는 **.NET 8 이상을 대상**으로 설정할 수 있습니다 — 필요시 .NET 10과 함께 해당 버전 SDK를 설치하세요.

### 필수 플러그인 종속성

wpf-dev-pack 에이전트는 다음 Claude Code 플러그인이 별도로 설치되어야 합니다:

| 플러그인 | MCP 서버 | 용도 |
|---------|----------|------|
| **[context7](https://github.com/upstash/context7)** | context7 | 최신 라이브러리/프레임워크 문서 조회 |
| **[microsoft-docs](https://github.com/MicrosoftDocs/mcp)** | microsoft-learn | 공식 Microsoft 문서 및 코드 샘플 조회 |
| **[csharp-lsp](https://github.com/razzmatazz/csharp-language-server)** | csharp | C# Language Server Protocol (정의, 참조, 진단) |

### 필수 MCP

wpf-dev-pack 에이전트가 필요로 하는 다음 MCP 서버는 **Claude Code 플러그인으로 설치하면 안 되며**, `uv`를 통해 MCP 서버로 직접 설치해야 합니다.

| MCP 서버 | 용도 | 설치 방법 |
|---|---|---|
| **[serena](https://github.com/oraios/serena)** | 시맨틱 코드 분석, 심볼 네비게이션 | [Quick Start](https://github.com/oraios/serena#quick-start) 절차에 따라 `uv`로 직접 설치하세요. Claude Code 플러그인 경로를 사용하지 **마세요** — 그 이유는 [Serena Claude Code 문서의 Attention 안내](https://oraios.github.io/serena/02-usage/030_clients.html#claude-code)를 참고하세요 (Claude Code 내장 도구 description이 ~16k 토큰을 차지하면서 플러그인 경로로 등록한 Serena의 도구 사용을 강하게 편향시킵니다). |

> **참고:** wpf-dev-pack은 런타임에 Claude Code 플러그인 가용성을 확인하고 누락된 경우 경고합니다. Serena MCP는 위 절차대로 별도 설정이 필요합니다.

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

## 🧠 지식 제공 방식

wpf-dev-pack은 키워드 감지 훅을 **사용하지 않습니다**. WPF 지식은 **WpfDevPackMcp** MCP 서버가 온디맨드로 제공합니다 — 서버 자체의 instructions가 에이전트에게 WPF/C#/.NET 질문에 **답하기 전 토픽 카탈로그를 검색**하도록 지시합니다.

### 작동 방식

1. **질문**: WPF/C#/.NET 질문을 하거나 커맨드 스킬/에이전트를 호출합니다.
2. **검색**: 에이전트가 `WpfDevPackMcp search_wpf_topics`로 카탈로그를 검색하고 `get_wpf_topic`으로 적합한 토픽을 로드합니다.
3. **에이전트**: 복잡한 다단계 작업은 전문 에이전트가 처리합니다.

> ~50개 지식 토픽은 레포의 `knowledge/<id>/TOPIC.md`에 순수 마크다운으로 존재하며 실시간으로 조회됩니다 — 편집에 플러그인 재빌드나 버전 범프가 필요 없습니다. [`/wpf-dev-pack:set-repo-path`](#-설정)를 한 번 실행해 서버가 읽을 로컬 클론 경로를 지정하세요.

### 토픽 예시 (MCP로 제공)

| 질문 주제 | 토픽 |
|-----------|------|
| CustomControl 작성 | `authoring-wpf-controls` |
| CommunityToolkit MVVM | `implementing-communitytoolkit-mvvm` |
| DrawingContext 렌더링 | `rendering-with-drawingcontext` |
| 고성능 렌더링 | `rendering-wpf-high-performance` |

복잡한 작업에는 전문 에이전트가 추천됩니다 (예: 렌더링은 `wpf-performance-optimizer`, 아키텍처 검토는 `wpf-architect`).

### 커맨드 스킬 vs 지식

- **커맨드 스킬** (`/wpf-dev-pack:<name>`) — 슬래시로 호출하는 생성기/플러그인 운영 도구 (19개 번들; 아래 **스킬 & 지식** 참조).
- **지식 토픽** — WpfDevPackMcp가 제공하는 참조 콘텐츠로, 세션 스킬 목록에 올라가지 않습니다 (세션 컨텍스트 비용 없음).

---

## 🎯 기능

### 🤖 전문 에이전트

> 모든 에이전트는 현재 세션 모델을 그대로 사용합니다. 모델 전환은 `/model` 명령으로 수행하세요 (예: Opus 1M ↔ Sonnet ↔ Haiku).

| 에이전트 | 전문 분야 |
|----------|-----------|
| 🏗️ **wpf-architect** | 전략적 아키텍처 및 설계 결정 |
| 🎨 **wpf-control-designer** | CustomControl 구현 |
| 📐 **wpf-xaml-designer** | XAML 스타일 및 템플릿 |
| 🔄 **wpf-mvvm-expert** | MVVM 패턴 및 CommunityToolkit |
| 🔗 **wpf-data-binding-expert** | 복잡한 바인딩 및 유효성 검사 |
| ⚡ **wpf-performance-optimizer** | 렌더링 및 성능 |
| 🔍 **wpf-code-reviewer** | 코드 품질 분석 |
| 🔎 **wpf-code-auditor** | 전체 코드베이스 패턴 & 일관성 감사 |
| 📝 **code-formatter** | C# 서식 및 스타일 |
| 🔧 **serena-initializer** | 프로젝트 설정 |

### 🔌 MCP 서버

| 플러그인 | MCP 서버 | 용도 |
|---------|----------|------|
| **HandMirrorMcp** | HandMirrorMcp | .NET 어셈블리/NuGet 검사 (내장) |
| **WpfDevPackMcp** | WpfDevPackMcp | WPF 지식 토픽, 로컬 저장소 클론에서 온디맨드 제공 (내장) |
| **context7** | context7 | 라이브러리/프레임워크 문서 |
| _(`uv`로 직접 설치한 MCP)_ | **serena** | 시맨틱 코드 분석 |
| **microsoft-docs** | microsoft-learn | 공식 Microsoft 문서 |
| **csharp-lsp** | csharp | C# LSP 코드 인텔리전스 |

> 설치 방법은 [필수 플러그인 종속성](#필수-플러그인-종속성) 및 [필수-mcp](#필수-mcp) 섹션을 참고하세요. Serena는 Claude Code 플러그인이 **아니며**, `uv`로 MCP 서버로 직접 설치해야 합니다.

### 📚 스킬 & 지식

> **v1.7.0부터**, ~50개의 WPF *지식* 토픽(MVVM, 렌더링, 스레딩, 스타일링,
> 서드파티 라이브러리, .NET 공통, Prism 9 컴패니언, 테스트 등)은 **더 이상
> 플러그인 스킬로 번들되지 않습니다**. 이들은 **WpfDevPackMcp** MCP 서버가
> `get_wpf_topic` / `search_wpf_topics`로 온디맨드 제공하며, 서버 자체의
> instructions가 답변 전 검색하도록 에이전트에게 지시합니다. 덕분에 세션
> 스킬 목록에서 빠져(세션 컨텍스트 비용 없음) 있으면서도 순수 마크다운으로
> 편집 가능합니다.
> [`mcp/README.md`](../mcp/README.md)와 [`/wpf-dev-pack:set-repo-path`](#-설정)를
> 참고하세요.

플러그인은 **19개 커맨드 스킬**(슬래시 호출)과, 에이전트에 preload되는 **9개 내부 `wpf-rule-*` 스킬**(`user-invocable: false`)을 번들합니다 — 총 28개 스킬 폴더:

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

<details>
<summary><b>💬 LLM 채팅 (5개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `make-wpf-chatclient` | 원버튼 스트리밍 LLM 채팅 클라이언트 (전체 구성) |
| `make-wpf-chatclient-factory` | 프로바이더 무관 IChatClient 팩토리 (공유 SocketsHttpHandler) + MockChatClient |
| `make-wpf-chat-orchestrator` | 스트리밍 채팅 오케스트레이터 (UI 비의존 이벤트) + MCP 도구 획득 |
| `make-wpf-chat-bubble-template` | 역할별 채팅 버블 템플릿 |
| `make-wpf-markdown-presenter` | Markdig → 선택 가능한 FlowDocument 마크다운 프리젠터 |

</details>

<details>
<summary><b>🎨 코드 품질 (1개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `formatting-wpf-csharp-code` | C# / XAML 서식 & 스타일 (편집 시 CodeFormatter 훅이 자동 적용) |

</details>

<details>
<summary><b>🔧 플러그인 운영 (6개 스킬)</b></summary>

| 스킬 | 설명 |
|------|------|
| `collecting-wpf-dev-pack-feedback` | 익명화된 피드백 문서 수집 (추후 반영용) |
| `configuring-wpf-dev-pack-language` | 프로젝트별 응답 언어 설정 (`.claude/wpf-dev-pack.local.md`) |
| `set-repo-path` | WpfDevPackMcp가 지식을 읽어올 로컬 저장소 클론 경로 설정 |
| `set-repo-branch` | WpfDevPackMcp가 추적할 git 브랜치 설정 (`config.json`) |
| `set-repo-managed` | 서버 관리 플래그(`state.json`) 설정 — 파괴적/비파괴적 refresh 제어 |
| `show-wpf-dev-pack-config` | WpfDevPackMcp `config.json` / `state.json` 경로·값 표시 |

</details>

---

## 📁 플러그인 구조

```
wpf-dev-pack/
├── 📁 .claude-plugin/
│   └── plugin.json           # 플러그인 매니페스트
├── 📁 agents/                 # 10개 전문 에이전트
│   ├── wpf-architect.md
│   ├── wpf-code-auditor.md
│   ├── wpf-code-reviewer.md
│   ├── wpf-control-designer.md
│   ├── wpf-xaml-designer.md
│   ├── wpf-mvvm-expert.md
│   ├── wpf-data-binding-expert.md
│   ├── wpf-performance-optimizer.md
│   ├── code-formatter.md
│   └── serena-initializer.md
├── 📁 skills/                 # 19개 커맨드 스킬 + 9개 wpf-rule-* preload 스킬
├── 📁 hooks/                  # 이벤트 훅
├── 📁 themes/                 # 컬러 테마 (실험적) — .NET Purple 다크/라이트
├── 📁 monitors/               # 백그라운드 모니터 (실험적) — 빌드 워치
├── 📄 .mcp.json               # MCP 설정 (HandMirrorMcp + WpfDevPackMcp)
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
