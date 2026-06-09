[🇰🇷 한국어](./README.ko.md)

# Agents

Specialized AI agents for WPF development tasks.

## Agent List

> All agents inherit the current session model. Run `/model` to switch.

| Agent | Specialty |
|-------|-----------|
| 🏗️ **wpf-architect** | Strategic architecture & design decisions |
| 🎨 **wpf-control-designer** | CustomControl implementation |
| 📐 **wpf-xaml-designer** | XAML styles & templates |
| 🔄 **wpf-mvvm-expert** | MVVM pattern & CommunityToolkit |
| 🔗 **wpf-data-binding-expert** | Complex bindings & validation |
| ⚡ **wpf-performance-optimizer** | Rendering & performance |
| 🔍 **wpf-code-reviewer** | Code quality analysis |
| 🔎 **wpf-code-auditor** | Full-codebase pattern & consistency audit |
| 📝 **code-formatter** | C# formatting & style |
| 🔧 **serena-initializer** | Project setup |

## Usage

Agents are automatically delegated based on task complexity, or can be invoked explicitly:

```
Task(subagent_type="wpf-dev-pack:wpf-architect", ...)
```
