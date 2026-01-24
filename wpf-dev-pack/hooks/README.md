[🇰🇷 한국어](./README.ko.md)

# Hooks

Event hooks that run automatically during Claude Code operations.

## Hook List

| Hook | Trigger | Description |
|------|---------|-------------|
| **WpfKeywordDetector** | PreToolUse | Detects WPF/C#/.NET keywords and auto-activates relevant skills |
| **CodeFormatter** | PostToolUse | Formats C# and XAML code after file modifications |
| **McpDependencyChecker** | PreToolUse | Checks for required MCP server availability |
| **XamlValidator** | PostToolUse | Validates XAML syntax after edits |

## Files

| File | Description |
|------|-------------|
| `hooks.json` | Hook configuration and triggers |
| `WpfKeywordDetector.cs` | Keyword detection logic |
| `CodeFormatter.cs` | Code formatting with XamlStyler and dotnet format |
| `McpDependencyChecker.cs` | MCP dependency verification |
| `XamlValidator.cs` | XAML syntax validation |

## How It Works

1. **PreToolUse hooks** run before Claude executes a tool
2. **PostToolUse hooks** run after a tool completes
3. Hooks can modify behavior or provide feedback

## Requirements

- .NET 10.0 SDK for hook execution
- XamlStyler for XAML formatting (optional)
