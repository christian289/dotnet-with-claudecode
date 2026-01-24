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

## 파일

| 파일 | 설명 |
|------|------|
| `hooks.json` | 훅 설정 및 트리거 |
| `WpfKeywordDetector.cs` | 키워드 감지 로직 |
| `CodeFormatter.cs` | XamlStyler 및 dotnet format을 사용한 코드 서식 |
| `McpDependencyChecker.cs` | MCP 종속성 확인 |
| `XamlValidator.cs` | XAML 구문 유효성 검사 |

## 작동 방식

1. **PreToolUse 훅**은 Claude가 도구를 실행하기 전에 실행
2. **PostToolUse 훅**은 도구가 완료된 후 실행
3. 훅은 동작을 수정하거나 피드백을 제공할 수 있음

## 요구사항

- 훅 실행을 위한 .NET 10.0 SDK
- XAML 서식을 위한 XamlStyler (선택사항)
