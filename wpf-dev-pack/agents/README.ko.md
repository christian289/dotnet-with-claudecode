[🇺🇸 English](./README.md)

# Agents

WPF 개발 작업을 위한 전문 AI 에이전트입니다.

## 에이전트 목록

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

## 사용법

에이전트는 작업 복잡도에 따라 자동으로 위임되거나 명시적으로 호출할 수 있습니다:

```
Task(subagent_type="wpf-dev-pack:wpf-architect", ...)
```
