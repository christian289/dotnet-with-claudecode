[🇰🇷 한국어](./README.ko.md)

<div align="center">

# 🎨 wpf-dev-pack

### The Ultimate WPF Development Toolkit for Claude Code

[![Version](https://img.shields.io/badge/version-1.7.2-blue.svg)](https://github.com/christian289/dotnet-with-claudecode)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET_SDK-10.0.300+-purple.svg)](https://dotnet.microsoft.com/)
[![Claude Code](https://img.shields.io/badge/Claude%20Code-Plugin-orange.svg)](https://claude.ai)

**11 Skills** · **10 Specialized Agents** · **2 MCP Servers**

[Installation](#-installation) · [Quick Start](#-quick-start) · [Features](#-features) · [Documentation](#-documentation)

---

</div>

## ✨ Highlights

> **MVVM Composition Style**: wpf-dev-pack enforces a single matching path per MVVM framework, both with **Stateful ViewModel**:
> - **CommunityToolkit.Mvvm** (default) → **ViewModel First Composition** via `Mappings.xaml` + implicit DataTemplate.
> - **Prism 9** (alternative) → **View First Composition** via `RegisterForNavigation` + `IRegionManager.RequestNavigate`.
>
> Prism `ViewModelLocator.AutoWireViewModel`, code-behind `DataContext = new VM()`, inline XAML `DataContext`, and Stateless-VM patterns are prohibited (see [`.claude/rules/prohibitions.md`](./.claude/rules/prohibitions.md) and [`docs/TERMINOLOGY.md`](./docs/TERMINOLOGY.md)).
>
> Pre-v1.6.4 docs labeled this uniformly as "View First MVVM" — that label conflicted with Microsoft's official definition (lookup key for `Mappings.xaml` is the ViewModel type → ViewModel First). v1.6.4 corrects the labels per path; the enforced code rules are unchanged.

<table>
<tr>
<td width="50%">

### 🤖 AI-Powered Development
- **10 Specialized Agents** for different WPF tasks
- **Session-model agnostic** — agents inherit your current model
- **Auto-detection** of WPF keywords
- **Prism 9** companion files for dual-framework support

</td>
<td width="50%">

### 🛠️ Complete Toolkit
- **11 command Skills** + on-demand WPF knowledge via MCP
- **Best practices** built-in

</td>
</tr>
<tr>
<td width="50%">

### 📚 Smart Documentation
- **Microsoft Learn** plugin (install from marketplace)
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
| .NET SDK | **10.0.300+** | Required for file-based app hooks |
| Claude Code | Latest | - |
| uv | Latest | For Serena MCP |

> **Target Framework vs SDK**: .NET SDK 10.0.300+ is required to **run wpf-dev-pack** (hooks use file-based apps).
> Generated WPF projects can **target .NET 8+** — install the corresponding SDK alongside .NET 10 if needed.

### Required Plugin Dependencies

wpf-dev-pack agents require the following Claude Code plugins to be installed separately:

| Plugin | MCP Server | Purpose |
|--------|-----------|---------|
| **[context7](https://github.com/upstash/context7)** | context7 | Up-to-date library/framework documentation |
| **[microsoft-docs](https://github.com/MicrosoftDocs/mcp)** | microsoft-learn | Official Microsoft documentation and code samples |
| **[csharp-lsp](https://github.com/razzmatazz/csharp-language-server)** | csharp | C# Language Server Protocol (definition, references, diagnostics) |

### Required MCPs

The following MCP server is required by wpf-dev-pack agents but **must NOT be installed as a Claude Code plugin** — install it directly as an MCP server via `uv` instead.

| MCP Server | Purpose | Installation |
|---|---|---|
| **[serena](https://github.com/oraios/serena)** | Semantic code analysis, symbol navigation | Install directly via `uv` per the [Quick Start](https://github.com/oraios/serena#quick-start). Do **not** use a Claude Code plugin path — see the [Attention note in the Serena Claude Code docs](https://oraios.github.io/serena/02-usage/030_clients.html#claude-code) for the rationale (Claude Code's built-in tool descriptions strongly bias the model away from invoking Serena's tools when registered through the plugin path). |

> **Note:** wpf-dev-pack checks Claude Code plugin availability at runtime and warns if missing. The Serena MCP must be set up separately as described above.

Install via Claude Code marketplace or `/install-plugin` command.

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
| "Create a CustomControl" | `authoring-wpf-controls` |
| "Apply MVVM pattern" | `implementing-communitytoolkit-mvvm` |
| "Render with DrawingContext" | `rendering-with-drawingcontext` |
| "Need performance optimization" | `rendering-wpf-high-performance` + `wpf-performance-optimizer` agent |
| "Review architecture" | `wpf-architect` agent recommended |

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

> All agents inherit the current session model. Run `/model` to switch (e.g. Opus 1M ↔ Sonnet ↔ Haiku).

| Agent | Specialty |
|-------|-----------|
| 🏗️ **wpf-architect** | Strategic architecture & design decisions |
| 🎨 **wpf-control-designer** | CustomControl implementation |
| 📐 **wpf-xaml-designer** | XAML styles & templates |
| 🔄 **wpf-mvvm-expert** | MVVM pattern & CommunityToolkit |
| 🔗 **wpf-data-binding-expert** | Complex bindings & validation |
| ⚡ **wpf-performance-optimizer** | Rendering & performance |
| 🔍 **wpf-code-reviewer** | Code quality analysis |
| 📝 **code-formatter** | C# formatting & style |
| 🔧 **serena-initializer** | Project setup |

### 🔌 MCP Servers

| Plugin | MCP Server | Purpose |
|--------|-----------|---------|
| **HandMirrorMcp** | HandMirrorMcp | .NET assembly/NuGet inspection (bundled) |
| **WpfDevPackMcp** | WpfDevPackMcp | WPF knowledge topics, served on demand from a local repo clone (bundled) |
| **context7** | context7 | Library/framework documentation |
| **sequential-thinking** | sequential-thinking | Step-by-step analysis |
| _(direct MCP via `uv`)_ | **serena** | Semantic code analysis |
| **microsoft-docs** | microsoft-learn | Official Microsoft documentation |
| **csharp-lsp** | csharp | C# LSP code intelligence |

> See [Required Plugin Dependencies](#required-plugin-dependencies) and [Required MCPs](#required-mcps) for installation. Serena is **not** a Claude Code plugin — install it directly as an MCP server via `uv`.

### 📚 Skills & Knowledge

> **As of v1.7.0**, the ~50 WPF *knowledge* topics (MVVM, rendering, threading,
> styling, 3rd-party libraries, .NET common, Prism 9 companions, testing, …) are
> **no longer bundled as plugin skills**. They are served on demand by the
> **WpfDevPackMcp** MCP server via `get_wpf_topic` / `search_wpf_topics`; the
> server's own instructions tell the agent to search before answering. This keeps them out of the
> session's skill listing (no per-session context cost) while remaining editable
> as plain Markdown. See [`mcp/README.md`](../mcp/README.md) and
> [`/wpf-dev-pack:set-repo-path`](#-configuration).

The plugin bundles **11 command skills** (slash-invocable):

<details>
<summary><b>🏗️ Scaffolding (7 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `make-wpf-project` | WPF project scaffolding with MVVM/DI |
| `make-wpf-custom-control` | CustomControl generation |
| `make-wpf-usercontrol` | UserControl generation |
| `make-wpf-converter` | IValueConverter generation |
| `make-wpf-behavior` | Behavior<T> generation |
| `make-wpf-viewmodel` | ViewModel + View + DI + DataTemplate mapping generation |
| `make-wpf-service` | Service interface + implementation + DI registration |

</details>

<details>
<summary><b>🎨 Code Quality (1 skill)</b></summary>

| Skill | Description |
|-------|-------------|
| `formatting-wpf-csharp-code` | C# / XAML formatting & style (silent trigger) |

</details>

<details>
<summary><b>🔧 Plugin Operations (3 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `collecting-wpf-dev-pack-feedback` | Capture anonymized feedback docs for later application |
| `configuring-wpf-dev-pack-language` | Set the per-project response language (`.claude/wpf-dev-pack.local.md`) |
| `set-repo-path` | Configure the local repo-clone path WpfDevPackMcp reads knowledge from |

</details>

---

## 📁 Plugin Structure

```
wpf-dev-pack/
├── 📁 .claude-plugin/
│   └── plugin.json           # Plugin manifest
├── 📁 agents/                 # 10 Specialized agents
│   ├── wpf-architect.md
│   ├── wpf-code-reviewer.md
│   ├── wpf-control-designer.md
│   ├── wpf-xaml-designer.md
│   ├── wpf-mvvm-expert.md
│   ├── wpf-data-binding-expert.md
│   ├── wpf-performance-optimizer.md
│   ├── code-formatter.md
│   └── serena-initializer.md
├── 📁 skills/                 # 11 command skills
├── 📁 hooks/                  # Event hooks
├── 📄 .mcp.json               # MCP config (HandMirrorMcp + WpfDevPackMcp)
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
