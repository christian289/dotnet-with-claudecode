# dotnet-with-claudecode

.NET Development Tutorial with Claude Code

## Overview

This repository provides skills, rules, and agent configurations for .NET/WPF development using Claude Code.

## Contents

### wpf-dev-pack

A Claude Code extension pack for WPF development.

- **57 Skills**: WPF development patterns, MVVM, CustomControl, performance optimization, etc.
- **11 Agents**: wpf-architect, wpf-code-reviewer, wpf-control-designer, etc.
- **5 Commands**: `/make-wpf-custom-control`, `/make-wpf-project`, `/make-wpf-converter`, etc.

See [wpf-dev-pack/README.md](./wpf-dev-pack/README.md) for details.

### .claude/skills

AvaloniaUI-specific skills:

- `configuring-avalonia-dependency-injection` - AvaloniaUI DI configuration
- `designing-avalonia-customcontrol-architecture` - CustomControl architecture
- `structuring-avalonia-projects` - Project structure design
- `using-avalonia-collectionview` - DataGridCollectionView patterns
- `fixing-avaloniaui-radialgradientbrush` - RadialGradientBrush compatibility fix
- `converting-html-css-to-wpf-xaml` - HTML/CSS to WPF XAML conversion

## Requirements

- Claude Code CLI
- .NET 10.0 SDK (for wpf-dev-pack)

## Installation

### Installing wpf-dev-pack

```bash
# Step 1: Add the marketplace (one-time)
/plugin marketplace add christian289/dotnet-with-claudecode

# Step 2: Install the plugin
/plugin install wpf-dev-pack@dotnet-claude-plugins
```

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](.github/CONTRIBUTING.md).

## License

This project is licensed under the [MIT License](LICENSE).

## Author

- **christian289** - [GitHub](https://github.com/christian289)
