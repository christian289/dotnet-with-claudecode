[🇺🇸 English](./README.md)

# Hooks

Claude Code 작업 중 자동으로 실행되는 이벤트 훅입니다.

## 훅 목록

| 훅 | 트리거 | 설명 |
|----|--------|------|
| **DotnetVersionChecker** | SessionStart | 모든 훅이 C# file-based app으로 작동하기 때문에 필수인 .NET SDK 10.0.300 이상 설치 여부를 검증. 누락/미달 시 설치·업데이트 URL과 함께 빨간색 경고 출력. 하루 1회 캐싱. |
| **LanguagePreferenceLoader** | SessionStart | 세션 시작 시 `.claude/wpf-dev-pack.local.md`를 읽어 `language:` 필드가 있으면 시스템 컨텍스트에 응답 언어 지시문을 주입. 해당 세션 동안 Claude가 그 언어로 응답하도록 유도. |
| **WpfAuthoringRulesLoader** | SessionStart | WPF ControlTemplate / Style / 애니메이션 작성에 항상 적용되는 강제 규칙(스톡 컨트롤별 필수 `PART_` 이름, 애니메이션 안전 규칙, Setter의 Freezable 대상 → MC4111, `StaticResource` 전방 참조, `(UIElement.Children)[n]` 경로 함정, 런타임 검증)을 세션 컨텍스트에 주입. 플러그인 번들 룰은 설치 사용자에게 자동 로드되지 않으므로 훅으로 제공. 상세는 `animating-wpf-controltemplates` MCP 토픽. |
| **HandMirrorReminder** | PreToolUse (context7 / Microsoft Learn) | .NET/NuGet 문서 조회 시, 코드 작성 전 HandMirrorMcp로 정확한 namespace/시그니처를 검증하도록 에이전트에게 상기. |
| **RepoPathGuard** | PreToolUse (WpfDevPackMcp) | 지식 레포 경로가 미설정이면 `WpfDevPackMcp` 도구 호출을 차단하고 `/wpf-dev-pack:set-repo-path` 실행을 안내. |
| **McpDependencyChecker** | UserPromptSubmit | 필수 MCP 서버(context7, serena, microsoft-learn)를 세션당 1회 확인하고 누락 시 경고. |
| **XamlValidator** | PostToolUse (Edit/Write `*.xaml`) | 편집 후 XAML 구문 유효성 검사. |
| **MvvmViolationDetector** | PostToolUse (Edit/Write `*.cs`) | C# 편집 후 MVVM 계층 위반(예: ViewModel의 `System.Windows` UI 타입) 감지. |
| **CodeFormatter** | PostToolUse (Edit/Write `*.cs` / `*.xaml`) | 파일 수정 후 C#(`dotnet format`) 및 XAML(XamlStyler) 서식 지정. |
| **BuildErrorDiagnoser** | PostToolUse (Bash) | Bash 명령 후 빌드 출력을 파싱해 CS/NU/MSB 오류를 HandMirrorMcp 후속 단계와 함께 설명. |
| **FeedbackDocAuditor** | PostToolUse (스킬 스코프) | `*-wpf-dev-pack-feedback.md` 파일을 개인 식별 정보에 대해 감사하고 위반 시 종료 코드 2로 진단을 모델에 전달. `hooks.json`이 아닌 `skills/collecting-wpf-dev-pack-feedback/SKILL.md` frontmatter에 등록되어, 해당 스킬이 활성화된 동안에만 발화함. |

## 파일

| 파일 | 설명 |
|------|------|
| `hooks.json` | 훅 설정 및 트리거 |
| `DotnetVersionChecker.cs` | .NET SDK 10.0.300 이상 설치/버전 검증 (SessionStart) |
| `LanguagePreferenceLoader.cs` | 프로젝트별 응답 언어 환경설정 로더 (SessionStart) |
| `WpfAuthoringRulesLoader.cs` | WPF ControlTemplate/Style/애니메이션 작성 강제 규칙 주입 (SessionStart) |
| `HandMirrorReminder.cs` | .NET API를 HandMirrorMcp로 검증하도록 상기 (PreToolUse) |
| `RepoPathGuard.cs` | 지식 레포 경로 설정 전까지 `WpfDevPackMcp` 호출 차단 (PreToolUse) |
| `McpDependencyChecker.cs` | 필수 MCP 가용성 확인 (UserPromptSubmit) |
| `XamlValidator.cs` | XAML 구문 유효성 검사 |
| `MvvmViolationDetector.cs` | C# 편집의 MVVM 위반 감지 |
| `CodeFormatter.cs` | XamlStyler 및 dotnet format을 사용한 코드 서식 |
| `BuildErrorDiagnoser.cs` | Bash 후 빌드 오류(CS/NU/MSB) 진단 |
| `FeedbackDocAuditor.cs` | 새로 작성된 피드백 문서의 익명성 감사 |

## 작동 방식

1. **SessionStart 훅**은 세션 시작 시 실행
2. **UserPromptSubmit 훅**은 프롬프트 제출 시 실행
3. **PreToolUse 훅**은 Claude가 도구를 실행하기 전에 실행
4. **PostToolUse 훅**은 도구가 완료된 후 실행
5. 훅은 동작을 수정하거나 피드백을 제공할 수 있음

## 요구사항

- 훅 실행을 위한 .NET SDK 10.0.300 이상
- XAML 서식을 위한 XamlStyler (선택사항)
