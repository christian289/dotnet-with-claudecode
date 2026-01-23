<div align="center">

# 🎨 wpf-dev-pack

### The Ultimate WPF Development Toolkit for Claude Code

[![Version](https://img.shields.io/badge/version-1.1.0-blue.svg)](https://github.com/christian289/dotnet-with-claudecode)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0+-purple.svg)](https://dotnet.microsoft.com/)
[![Claude Code](https://img.shields.io/badge/Claude%20Code-Plugin-orange.svg)](https://claude.ai)

**57 Skills** · **9 Specialized Agents** · **5 Commands** · **4 MCP Servers**

[Installation](#-installation) · [Quick Start](#-quick-start) · [Features](#-features) · [Documentation](#-documentation)

---

</div>

## ✨ Highlights

<table>
<tr>
<td width="50%">

### 🤖 AI-Powered Development
- **9 Specialized Agents** for different WPF tasks
- **Opus-level** architects for strategic decisions
- **Auto-detection** of WPF keywords

</td>
<td width="50%">

### 🛠️ Complete Toolkit
- **57 Skills** covering all WPF aspects
- **5 Commands** for instant scaffolding
- **Best practices** built-in

</td>
</tr>
<tr>
<td width="50%">

### 📚 Smart Documentation
- **Context7** for up-to-date docs
- **MicrosoftDocs** integration
- **Semantic code analysis** with Serena

</td>
<td width="50%">

### ⚡ High Performance
- **DrawingContext** rendering patterns
- **Virtualization** strategies
- **Memory optimization** techniques

</td>
</tr>
</table>

---

## 📦 Installation

### From Marketplace (Recommended)

```bash
# Step 1: Add the marketplace (one-time)
/plugin marketplace add christian289/dotnet-with-claudecode

# Step 2: Install the plugin
/plugin install wpf-dev-pack@dotnet-claude-plugins
```

### Local Installation

```bash
claude --plugin-dir ./wpf-dev-pack
```

### Requirements

| Requirement | Version |
|-------------|---------|
| .NET SDK | 9.0 or later |
| Claude Code | Latest |
| uv (for Serena) | Latest |

---

## 🚀 Quick Start

### Create a New WPF Project

```bash
# With CommunityToolkit.Mvvm (Recommended)
/wpf-dev-pack:make-wpf-project MyApp

# With Prism Framework
/wpf-dev-pack:make-wpf-project MyApp --prism
```

### Generate Components

```bash
# CustomControl
/wpf-dev-pack:make-wpf-custom-control MyButton Button

# UserControl
/wpf-dev-pack:make-wpf-usercontrol SearchBox

# Converter
/wpf-dev-pack:make-wpf-converter BoolToVisibility

# Behavior
/wpf-dev-pack:make-wpf-behavior SelectAllOnFocus TextBox
```

### Ask for Help

```
"How do I create a high-performance chart control?"
"Review this ViewModel from MVVM perspective"
"Optimize this rendering code for large datasets"
```

---

## 🎯 Features

### 🤖 Specialized Agents

| Agent | Model | Specialty |
|-------|:-----:|-----------|
| 🏗️ **wpf-architect** | Opus | Strategic architecture & design decisions |
| 🎨 **wpf-control-designer** | Sonnet | CustomControl implementation |
| 📐 **wpf-xaml-designer** | Sonnet | XAML styles & templates |
| 🔄 **wpf-mvvm-expert** | Sonnet | MVVM pattern & CommunityToolkit |
| 🔗 **wpf-data-binding-expert** | Sonnet | Complex bindings & validation |
| ⚡ **wpf-performance-optimizer** | Sonnet | Rendering & performance |
| 🔍 **wpf-code-reviewer** | Opus | Code quality analysis |
| 📝 **code-formatter** | Sonnet | C# formatting & style |
| 🔧 **serena-initializer** | Haiku | Project setup |

### 🔌 MCP Servers

| Server | Purpose |
|--------|---------|
| **Context7** | Up-to-date library documentation |
| **MicrosoftDocs** | Official Microsoft documentation |
| **Sequential-thinking** | Step-by-step problem solving |
| **Serena** | Semantic code analysis |

### 📚 Skills by Category

<details>
<summary><b>🎨 UI & Controls (15 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `authoring-wpf-controls` | Control authoring patterns |
| `customizing-controltemplate` | ControlTemplate customization |
| `designing-wpf-customcontrol-architecture` | CustomControl architecture |
| `developing-wpf-customcontrols` | CustomControl development |
| `implementing-wpf-adorners` | Adorner implementation |
| `understanding-wpf-content-model` | Content model patterns |
| `creating-wpf-dialogs` | Dialog windows |
| `creating-wpf-flowdocument` | FlowDocument creation |
| `using-wpf-behaviors-triggers` | Behaviors & triggers |
| `using-xaml-property-element-syntax` | XAML syntax patterns |
| `using-converter-markup-extension` | Converter patterns |
| `displaying-slider-index` | Slider UI patterns |
| `binding-enum-command-parameters` | Enum binding patterns |
| `localizing-wpf-applications` | Localization |
| `implementing-wpf-automation` | UI Automation |

</details>

<details>
<summary><b>🔗 Data Binding & MVVM (8 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `implementing-communitytoolkit-mvvm` | CommunityToolkit.Mvvm |
| `advanced-data-binding` | Advanced binding patterns |
| `implementing-wpf-validation` | Validation strategies |
| `managing-wpf-collectionview-mvvm` | CollectionView in MVVM |
| `mapping-viewmodel-view-datatemplate` | View-ViewModel mapping |
| `configuring-dependency-injection` | DI configuration |
| `defining-wpf-dependencyproperty` | DependencyProperty |
| `structuring-wpf-projects` | Project structure |

</details>

<details>
<summary><b>⚡ Performance & Rendering (10 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `rendering-with-drawingcontext` | DrawingContext rendering |
| `rendering-with-drawingvisual` | DrawingVisual rendering |
| `rendering-wpf-architecture` | Rendering architecture |
| `rendering-wpf-high-performance` | High-performance rendering |
| `implementing-2d-graphics` | 2D graphics |
| `implementing-hit-testing` | Hit testing |
| `virtualizing-wpf-ui` | UI virtualization |
| `optimizing-wpf-memory` | Memory optimization |
| `checking-image-bounds-transform` | Image transforms |
| `navigating-visual-logical-tree` | Tree navigation |

</details>

<details>
<summary><b>🎬 Animation & Media (3 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `creating-wpf-animations` | Animation creation |
| `integrating-wpf-media` | Media integration |
| `using-wpf-clipboard` | Clipboard operations |

</details>

<details>
<summary><b>⌨️ Input & Events (4 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `handling-wpf-input-commands` | Input & commands |
| `routing-wpf-events` | Routed events |
| `implementing-wpf-dragdrop` | Drag & drop |
| `managing-wpf-popup-focus` | Popup focus management |

</details>

<details>
<summary><b>🎨 Styling & Resources (3 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `managing-styles-resourcedictionary` | Styles & resources |
| `resolving-icon-font-inheritance` | Icon fonts |
| `formatting-wpf-csharp-code` | Code formatting |

</details>

<details>
<summary><b>🔧 Application & Threading (3 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `managing-wpf-application-lifecycle` | App lifecycle |
| `threading-wpf-dispatcher` | Dispatcher & threading |
| `migrating-wpf-to-dotnet` | .NET migration |

</details>

<details>
<summary><b>🔷 .NET Common (12 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `configuring-console-app-di` | Console app DI |
| `handling-async-operations` | Async patterns |
| `implementing-io-pipelines` | I/O pipelines |
| `implementing-pubsub-pattern` | Pub/Sub pattern |
| `implementing-repository-pattern` | Repository pattern |
| `managing-literal-strings` | String management |
| `optimizing-fast-lookup` | Fast lookup |
| `optimizing-io-operations` | I/O optimization |
| `optimizing-memory-allocation` | Memory allocation |
| `processing-parallel-tasks` | Parallel processing |
| `using-generated-regex` | Source-generated regex |

</details>

---

## 📁 Plugin Structure

```
wpf-dev-pack/
├── 📁 .claude-plugin/
│   └── plugin.json           # Plugin manifest
├── 📁 agents/                 # 9 Specialized agents
│   ├── wpf-architect.md
│   ├── wpf-control-designer.md
│   ├── wpf-xaml-designer.md
│   ├── wpf-mvvm-expert.md
│   ├── wpf-data-binding-expert.md
│   ├── wpf-performance-optimizer.md
│   ├── wpf-code-reviewer.md
│   ├── code-formatter.md
│   └── serena-initializer.md
├── 📁 commands/               # 5 User commands
│   ├── make-wpf-custom-control/
│   ├── make-wpf-project/
│   ├── make-wpf-converter/
│   ├── make-wpf-behavior/
│   └── make-wpf-usercontrol/
├── 📁 skills/                 # 57 Skills
├── 📁 hooks/                  # Event hooks
├── 📄 .mcp.json               # MCP server config
├── 📄 README.md
└── 📄 LICENSE
```

---

## 🔧 Configuration

### Serena MCP Setup

> ⚠️ **Required**: Install [uv](https://docs.astral.sh/uv/) to use Serena.

```bash
# Test Serena locally
uvx --from git+https://github.com/oraios/serena serena start-mcp-server
```

### C# LSP (Required for IntelliSense)

```bash
claude /install-plugin csharp-lsp
```

---

## 📖 Documentation

### Official References

- 📘 [WPF Samples (Microsoft)](https://github.com/microsoft/WPF-Samples)
- 📗 [WPF Graphics & Multimedia](https://learn.microsoft.com/dotnet/desktop/wpf/graphics-multimedia/)
- 📙 [Claude Code Plugin Spec](https://code.claude.com/docs/en/plugins-reference)

### Architecture Reference

- [oh-my-claudecode](https://github.com/Yeachan-Heo/oh-my-claudecode) - Agent-based orchestration pattern

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

---

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.

---

<div align="center">

**Made with ❤️ by vincent lee**

[⬆ Back to Top](#-wpf-dev-pack)

</div>
