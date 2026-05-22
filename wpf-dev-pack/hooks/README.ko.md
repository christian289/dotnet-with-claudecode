[🇺🇸 English](./README.md)

# Hooks

Claude Code 작업 중 자동으로 실행되는 이벤트 훅입니다.

## 훅 목록

| 훅 | 트리거 | 설명 |
|----|--------|------|
| **WpfKeywordDetector** | PreToolUse | WPF/C#/.NET 키워드를 감지하고 관련 스킬 자동 활성화 |
| **CodeFormatter** | PostToolUse | 파일 수정 후 C# 및 XAML 코드 서식 지정 |
| **McpDependencyChecker** | PreToolUse | 필수 MCP 서버 가용성 확인 |
| **XamlValidator** | PostToolUse | 편집 후 XAML 구문 유효성 검사 |
| **FeedbackDocAuditor** | PostToolUse (스킬 스코프) | `*-wpf-dev-pack-feedback.md` 파일을 개인 식별 정보에 대해 감사하고 위반 시 종료 코드 2로 진단을 모델에 전달. `hooks.json`이 아닌 `skills/collecting-wpf-dev-pack-feedback/SKILL.md` frontmatter에 등록되어, 해당 스킬이 활성화된 동안에만 발화함. |
| **LanguagePreferenceLoader** | SessionStart | 세션 시작 시 `.claude/wpf-dev-pack.local.md`를 읽어 `language:` 필드가 있으면 시스템 컨텍스트에 응답 언어 지시문을 주입. 해당 세션 동안 Claude가 그 언어로 응답하도록 유도. |
| **DotnetVersionChecker** | SessionStart | 모든 wpf-dev-pack 훅이 C# file-based app으로 작동하기 때문에 필수인 .NET SDK 10.0.300 이상 설치 여부를 검증. 누락 혹은 미달 시 빨간색 경고와 설치/업데이트 URL을 출력. 하루 1회만 실행되도록 캐싱. |

## 파일

| 파일 | 설명 |
|------|------|
| `hooks.json` | 훅 설정 및 트리거 |
| `WpfKeywordDetector.cs` | 키워드 감지 로직 |
| `CodeFormatter.cs` | XamlStyler 및 dotnet format을 사용한 코드 서식 |
| `McpDependencyChecker.cs` | MCP 종속성 확인 |
| `XamlValidator.cs` | XAML 구문 유효성 검사 |
| `FeedbackDocAuditor.cs` | 새로 작성된 피드백 문서의 익명성 감사 |
| `LanguagePreferenceLoader.cs` | 프로젝트별 응답 언어 환경설정 로더 (SessionStart) |
| `DotnetVersionChecker.cs` | .NET SDK 10.0.300 이상 설치/버전 검증 (SessionStart) |

## 작동 방식

1. **PreToolUse 훅**은 Claude가 도구를 실행하기 전에 실행
2. **PostToolUse 훅**은 도구가 완료된 후 실행
3. 훅은 동작을 수정하거나 피드백을 제공할 수 있음

## 요구사항

- 훅 실행을 위한 .NET SDK 10.0.300 이상
- XAML 서식을 위한 XamlStyler (선택사항)
