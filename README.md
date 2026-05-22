[🇰🇷 한국어](./README.ko.md)

# dotnet-with-claudecode

.NET Development Tutorial with Claude Code

## Overview

This repository provides skills, rules, and agent configurations for .NET/WPF development using Claude Code.

## Contents

### [wpf-dev-pack](./wpf-dev-pack)

Claude Code plugin for WPF development.

## Requirements

- Claude Code CLI
- .NET SDK 10.0.300+ (for wpf-dev-pack hooks)
- Required Claude Code plugins for wpf-dev-pack:
  - [context7](https://github.com/upstash/context7)
  - [microsoft-docs](https://github.com/MicrosoftDocs/mcp)
  - [csharp-lsp](https://github.com/razzmatazz/csharp-language-server)
- Required MCPs for wpf-dev-pack:
  - [serena](https://github.com/oraios/serena) — install **directly as an MCP server via `uv`, not as a Claude Code plugin**. Claude Code's built-in tool descriptions strongly bias the model away from using Serena's tools when Serena is registered via the plugin path; see the [Attention note in the Serena Claude Code docs](https://oraios.github.io/serena/02-usage/030_clients.html#claude-code) for the rationale, and follow the [Quick Start](https://github.com/oraios/serena#quick-start) for installation.

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
