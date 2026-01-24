[🇰🇷 한국어](./README.ko.md)

# Agents

Specialized AI agents for WPF development tasks.

## Agent List

| Agent | Model | Specialty |
|-------|:-----:|-----------|
| 🏗️ **wpf-architect** | Opus | Strategic architecture & design decisions |
| 🏗️ **wpf-architect-low** | Sonnet | Architecture analysis (Claude Pro) |
| 🎨 **wpf-control-designer** | Sonnet | CustomControl implementation |
| 📐 **wpf-xaml-designer** | Sonnet | XAML styles & templates |
| 🔄 **wpf-mvvm-expert** | Sonnet | MVVM pattern & CommunityToolkit |
| 🔗 **wpf-data-binding-expert** | Sonnet | Complex bindings & validation |
| ⚡ **wpf-performance-optimizer** | Sonnet | Rendering & performance |
| 🔍 **wpf-code-reviewer** | Opus | Code quality analysis |
| 🔍 **wpf-code-reviewer-low** | Sonnet | Code review (Claude Pro) |
| 📝 **code-formatter** | Haiku | C# formatting & style |
| 🔧 **serena-initializer** | Haiku | Project setup |

## Usage

Agents are automatically delegated based on task complexity, or can be invoked explicitly:

```
Task(subagent_type="wpf-dev-pack:wpf-architect", ...)
```

## Claude Pro Users

Use `-low` versions for Opus-level agents:
- `wpf-architect` → `wpf-architect-low`
- `wpf-code-reviewer` → `wpf-code-reviewer-low`
