# dotnet-with-claudecode

ClaudeCode와 함께하는 .NET 개발 튜토리얼

## Overview

이 저장소는 Claude Code를 활용한 .NET/WPF 개발을 위한 스킬, 규칙, 에이전트 설정을 제공합니다.

## Contents

### wpf-dev-pack

WPF 개발을 위한 Claude Code 확장 팩입니다.

- **49개 Skills**: WPF 개발 패턴, MVVM, CustomControl, 성능 최적화 등
- **11개 Agents**: wpf-architect, wpf-code-reviewer, wpf-control-designer 등
- **2개 Commands**: `/make-wpf-custom-control`, `/make-avaloniaui-custom-control`

자세한 내용은 [wpf-dev-pack/README.md](./wpf-dev-pack/README.md)를 참조하세요.

### .claude/skills

AvaloniaUI 전용 스킬들:

- `configuring-avalonia-dependency-injection` - AvaloniaUI DI 설정
- `designing-avalonia-customcontrol-architecture` - CustomControl 구조
- `structuring-avalonia-projects` - 프로젝트 구조 설계
- `using-avalonia-collectionview` - DataGridCollectionView 패턴
- `fixing-avaloniaui-radialgradientbrush` - RadialGradientBrush 호환성 이슈
- `converting-html-css-to-wpf-xaml` - HTML/CSS → WPF XAML 변환

## Requirements

- Claude Code CLI
- .NET 10.0 SDK (wpf-dev-pack 사용 시)

## Installation

### wpf-dev-pack 설치

```bash
claude mcp add-json wpf-dev-pack '{"type":"url","url":"https://raw.githubusercontent.com/anthropics/claude-code-packs/refs/heads/main/packs/url-pack/server.mjs","args":["https://raw.githubusercontent.com/christian289/dotnet-with-claudecode/main/wpf-dev-pack/marketplace.json"]}'
```

## Contributing

기여를 환영합니다! [CONTRIBUTING.md](.github/CONTRIBUTING.md)를 참조해주세요.

## License

이 프로젝트는 [MIT License](LICENSE)를 따릅니다.

## Author

- **christian289** - [GitHub](https://github.com/christian289)
