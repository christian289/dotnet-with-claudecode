[🇰🇷 한국어](./README.ko.md)

<div align="center">

# 🎨 wpf-dev-pack

### The Ultimate WPF Development Toolkit for Claude Code

[![Version](https://img.shields.io/badge/version-1.4.0-blue.svg)](https://github.com/christian289/dotnet-with-claudecode)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0+-purple.svg)](https://dotnet.microsoft.com/)
[![Claude Code](https://img.shields.io/badge/Claude%20Code-Plugin-orange.svg)](https://claude.ai)

**62 Skills** · **11 Specialized Agents** · **5 Commands** · **1 MCP Server**

[Installation](#-installation) · [Quick Start](#-quick-start) · [Features](#-features) · [Documentation](#-documentation)

---

</div>

## ✨ Highlights

<table>
<tr>
<td width="50%">

### 🤖 AI-Powered Development
- **11 Specialized Agents** for different WPF tasks
- **Opus-level** architects for strategic decisions
- **Auto-detection** of WPF keywords

</td>
<td width="50%">

### 🛠️ Complete Toolkit
- **62 Skills** covering all WPF aspects
- **5 Commands** for instant scaffolding
- **Best practices** built-in

</td>
</tr>
<tr>
<td width="50%">

### 📚 Smart Documentation
- **MicrosoftDocs** integration (included)
- **Context7** for up-to-date docs (external)
- **Semantic code analysis** with Serena (external)

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

### Updating

```bash
# Manual update
claude plugin update wpf-dev-pack@dotnet-claude-plugins

# Or enable auto-updates for this marketplace
/plugin → Marketplaces → dotnet-claude-plugins → Enable auto-update
```

> **Note:** Third-party marketplaces have auto-update disabled by default.

### Requirements

| Requirement | Version | Notes |
|-------------|---------|-------|
| .NET SDK | 8.0+ (10.0+ recommended) | Minimum .NET 8; .NET 10 for latest features |
| Claude Code | Latest | - |
| uv | Latest | For Serena MCP |

### Required MCP Dependencies

wpf-dev-pack requires the following MCP servers for full functionality:

| MCP Server | Purpose | Required By |
|------------|---------|-------------|
| **Context7** | Up-to-date library docs | Most agents |
| **Sequential-thinking** | Step-by-step analysis | Opus-level agents |
| **Serena** | Semantic code analysis | All agents |

> **Note:** These are commonly used MCPs that you may already have installed.
> wpf-dev-pack will check for their availability at runtime and warn if missing.

**If not installed, add to `~/.claude/.mcp.json`:**

```json
{
  "mcpServers": {
    "context7": {
      "command": "npx",
      "args": ["-y", "@upstash/context7-mcp@latest"],
      "windows": {
        "command": "cmd",
        "args": ["/c", "npx", "-y", "@upstash/context7-mcp@latest"]
      }
    },
    "sequential-thinking": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-sequential-thinking"],
      "windows": {
        "command": "cmd",
        "args": ["/c", "npx", "-y", "@modelcontextprotocol/server-sequential-thinking"]
      }
    },
    "serena": {
      "command": "uvx",
      "args": ["--from", "git+https://github.com/oraios/serena", "serena", "start-mcp-server"]
    }
  }
}
```

> **Why external?** These MCPs are commonly used across many plugins. Including them in wpf-dev-pack would cause duplication for users who already have them configured.

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

## 🎯 Requirements Interview System

When you invoke `wpf-architect`, an **adaptive path-based interview** identifies your exact needs:

### How It Works

```
Step 1: Task Type Selection
   ├─→ Path A: Create new project (7 steps)
   ├─→ Path B: Analyze/improve existing (5 steps)
   ├─→ Path C: Implement feature (5 steps)
   └─→ Path D: Debug/fix (4 steps)
```

Each path asks targeted questions with **keyword analysis** on free-input steps to auto-configure subsequent defaults.

### Interview Paths

| Path | Task Type | Steps | Focus |
|------|-----------|:-----:|-------|
| **A** | Create new project | 7 | Concept → Architecture → Scale → Complexity → Libraries → Feature areas |
| **B** | Analyze/improve | 5 | Goal → Analysis mode → Scope → Output format |
| **C** | Implement feature | 5 | Description → Approach → Libraries → Feature areas |
| **D** | Debug/fix | 4 | Symptoms → Problem type → Problem area |

### Example Flow (Path A)

```
User: "I want to build a chart app with WPF"

wpf-architect: [A-1] What kind of app? Describe the concept.
   → User: "Real-time stock chart dashboard"
   (Keywords detected: "chart", "real-time" → LiveCharts2, performance defaults)

wpf-architect: [A-2] Architecture pattern?
   → User selects: "MVVM + CommunityToolkit"

wpf-architect: [A-3] Project scale?
   → User selects: "Medium (5-15 Views)"

wpf-architect: [A-5] 3rd-party libraries?
   → Auto-suggested: LiveCharts2 ✓, WPF-UI (optional)

Result: Activates LiveCharts2 + DrawingContext skills + wpf-performance-optimizer
```

---

## 🧠 Auto-Trigger System

wpf-dev-pack uses an intelligent keyword detection system inspired by [oh-my-claudecode](https://github.com/Yeachan-Heo/oh-my-claudecode). When you mention WPF, C#, or .NET keywords, relevant skills are **automatically activated**.

### How It Works

1. **Keyword Detection**: Your prompt is scanned for WPF/.NET keywords
2. **Skill Activation**: Matching skills are automatically loaded
3. **Agent Recommendation**: Complex tasks suggest specialized agents

### Example Triggers

| You Say | Auto-Activates |
|---------|----------------|
| "CustomControl 만들어줘" | `authoring-wpf-controls`, `developing-wpf-customcontrols` |
| "MVVM 패턴 적용" | `implementing-communitytoolkit-mvvm` |
| "DrawingContext로 렌더링" | `rendering-with-drawingcontext` |
| "성능 최적화 필요" | `rendering-wpf-high-performance` + `wpf-performance-optimizer` agent |
| "아키텍처 검토" | `wpf-architect` agent recommended |

### Silent Triggers

Some skills activate without notification:
- `formatting-wpf-csharp-code` - Code formatting
- `using-xaml-property-element-syntax` - XAML syntax
- `managing-literal-strings` - String management

### Keyword Categories

<details>
<summary><b>📌 Primary WPF Keywords (Click to expand)</b></summary>

| Category | Keywords |
|----------|----------|
| **Controls** | `customcontrol`, `dependencyproperty`, `templatepart`, `controltemplate` |
| **MVVM** | `mvvm`, `viewmodel`, `relaycommand`, `observableproperty` |
| **Rendering** | `drawingcontext`, `drawingvisual`, `onrender`, `invalidatevisual` |
| **Performance** | `virtualizingstackpanel`, `freeze`, `freezable`, `bitmapcache` |
| **Events** | `routedevent`, `command`, `inputbinding`, `dragdrop` |
| **Styling** | `resourcedictionary`, `generic.xaml`, `storyboard`, `animation` |
| **Threading** | `dispatcher`, `invoke`, `begininvoke` |

</details>

<details>
<summary><b>🔷 .NET Keywords (Click to expand)</b></summary>

| Category | Keywords |
|----------|----------|
| **Async** | `async`, `await`, `task`, `valuetask`, `configureawait` |
| **Parallel** | `parallel`, `plinq`, `concurrentdictionary` |
| **Memory** | `span`, `memory<`, `arraypool`, `stackalloc` |
| **I/O** | `pipeline`, `pipereader`, `pipewriter` |
| **Patterns** | `repository pattern`, `pubsub`, `channel` |

</details>

---

## 🎯 Features

### 🤖 Specialized Agents

> **Claude Pro users**: Use `-low` versions for `wpf-architect` and `wpf-code-reviewer` (Opus → Sonnet)

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

### 🔌 MCP Servers

**Included:**

| Server | Purpose |
|--------|---------|
| **MicrosoftDocs** | Official Microsoft documentation |

**Required (External):**

| Server | Purpose | Notes |
|--------|---------|-------|
| **Context7** | Up-to-date library docs | Install separately |
| **Sequential-thinking** | Step-by-step analysis | Install separately |
| **Serena** | Semantic code analysis | Install separately |

> See [Required MCP Dependencies](#required-mcp-dependencies) for installation instructions.

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
<summary><b>📦 3rd Party Libraries (5 skills) — NEW in v1.4.0</b></summary>

| Skill | Description |
|-------|-------------|
| `integrating-wpfui-fluent` | WPF-UI (Wpf.Ui) Fluent Design integration |
| `integrating-livecharts2` | LiveCharts2 charting library |
| `validating-with-fluentvalidation` | FluentValidation + INotifyDataErrorInfo bridge |
| `handling-errors-with-erroror` | ErrorOr result pattern for service layer |
| `integrating-nodify` | Nodify node-based editor control |

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
├── 📁 agents/                 # 11 Specialized agents
│   ├── wpf-architect.md           # Opus
│   ├── wpf-architect-low.md       # Sonnet (Claude Pro)
│   ├── wpf-code-reviewer.md       # Opus
│   ├── wpf-code-reviewer-low.md   # Sonnet (Claude Pro)
│   ├── wpf-control-designer.md    # Sonnet
│   ├── wpf-xaml-designer.md       # Sonnet
│   ├── wpf-mvvm-expert.md         # Sonnet
│   ├── wpf-data-binding-expert.md # Sonnet
│   ├── wpf-performance-optimizer.md # Sonnet
│   ├── code-formatter.md          # Haiku
│   └── serena-initializer.md      # Haiku
├── 📁 commands/               # 5 User commands
│   ├── make-wpf-custom-control/
│   ├── make-wpf-project/
│   ├── make-wpf-converter/
│   ├── make-wpf-behavior/
│   └── make-wpf-usercontrol/
├── 📁 skills/                 # 62 Skills
├── 📁 hooks/                  # Event hooks
├── 📄 .mcp.json               # MCP config (MicrosoftDocs only)
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
