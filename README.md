[🇰🇷 한국어](./README.ko.md)

# dotnet-with-claudecode

.NET Development Tutorial with Claude Code

## Overview

This repository provides skills, rules, and agent configurations for .NET/WPF development using Claude Code.

## Contents

### [wpf-dev-pack](./wpf-dev-pack)

Claude Code plugin for WPF development.

### .claude/skills

#### AvaloniaUI Skills

- `configuring-avalonia-dependency-injection` - AvaloniaUI DI configuration
- `designing-avalonia-customcontrol-architecture` - CustomControl architecture
- `structuring-avalonia-projects` - Project structure design
- `using-avalonia-collectionview` - DataGridCollectionView patterns
- `fixing-avaloniaui-radialgradientbrush` - RadialGradientBrush compatibility fix
- `converting-html-css-to-wpf-xaml` - HTML/CSS to WPF XAML conversion

#### MewUI Framework Skills

- `building-mewui-apps` - MewUI application setup
- `creating-mewui-controls` - MewUI control creation
- `binding-mewui-data` - MewUI data binding
- `using-mewui-layout` - MewUI layout system
- `rendering-mewui-elements` - MewUI element rendering
- `navigating-mewui-tree` - MewUI tree navigation

#### Project Management Skills

- `wpf-dev-pack-release` - wpf-dev-pack release workflow

## Requirements

- Claude Code CLI
- .NET 10.0 SDK (for wpf-dev-pack hooks)
- Required Claude Code plugins for wpf-dev-pack:
  - [context7](https://github.com/nicobailey/context7-mcp)
  - [serena](https://github.com/oraios/serena)
  - [microsoft-docs](https://github.com/nicobailey/microsoft-docs-mcp)
  - [csharp-lsp](https://github.com/nicobailey/csharp-lsp)

## Installation

### Installing wpf-dev-pack

```bash
# Step 1: Add the marketplace (one-time)
/plugin marketplace add christian289/dotnet-with-claudecode

# Step 2: Install the plugin
/plugin install wpf-dev-pack@dotnet-claude-plugins
```

## Git Hooks Setup

This repository includes shared git hooks for automated version bumping of `wpf-dev-pack`.

### Installing Git Hooks

After cloning the repository, run one of the following:

```bash
# Option 1: Direct configuration
# 방법 1: 직접 설정
git config core.hooksPath .githooks

# Option 2: Use install script (Windows PowerShell)
# 방법 2: 설치 스크립트 사용 (Windows PowerShell)
.\.githooks\install.ps1

# Option 2: Use install script (Linux/Mac)
# 방법 2: 설치 스크립트 사용 (Linux/Mac)
./.githooks/install.sh
```

### What the Hook Does

- **pre-push**: Automatically bumps `wpf-dev-pack` patch version when pushing changes to `wpf-dev-pack/` directory (excluding `plugin.json` and `README.md`)

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](.github/CONTRIBUTING.md).

## License

This project is licensed under the [MIT License](LICENSE).

## Author

- **christian289** - [GitHub](https://github.com/christian289)
