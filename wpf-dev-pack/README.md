[🇰🇷 한국어](./README.ko.md)

<div align="center">

# 🎨 wpf-dev-pack

### The Ultimate WPF Development Toolkit for Claude Code

[![Version](https://img.shields.io/badge/version-1.6.4-blue.svg)](https://github.com/christian289/dotnet-with-claudecode)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET_SDK-10.0+-purple.svg)](https://dotnet.microsoft.com/)
[![Claude Code](https://img.shields.io/badge/Claude%20Code-Plugin-orange.svg)](https://claude.ai)

**57 Skills** · **10 Specialized Agents** · **1 MCP Server**

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
- **57 Skills** covering all WPF aspects
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
| .NET SDK | **10.0+** | Required for file-based app hooks |
| Claude Code | Latest | - |
| uv | Latest | For Serena MCP |

> **Target Framework vs SDK**: .NET 10 SDK is required to **run wpf-dev-pack** (hooks use file-based apps).
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
| **context7** | context7 | Library/framework documentation |
| **sequential-thinking** | sequential-thinking | Step-by-step analysis |
| _(direct MCP via `uv`)_ | **serena** | Semantic code analysis |
| **microsoft-docs** | microsoft-learn | Official Microsoft documentation |
| **csharp-lsp** | csharp | C# LSP code intelligence |

> See [Required Plugin Dependencies](#required-plugin-dependencies) and [Required MCPs](#required-mcps) for installation. Serena is **not** a Claude Code plugin — install it directly as an MCP server via `uv`.

### 📚 Skills by Category

<details>
<summary><b>🎨 UI & Controls (5 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `authoring-wpf-controls` | Control authoring patterns |
| `designing-wpf-customcontrol-architecture` | CustomControl architecture |
| `displaying-slider-index` | Slider UI patterns |
| `binding-enum-command-parameters` | Enum binding patterns |
| `configuring-wpf-themeinfo` | ThemeInfo configuration |

</details>

<details>
<summary><b>🔗 Data Binding & MVVM (7 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `implementing-communitytoolkit-mvvm` | CommunityToolkit.Mvvm |
| `advanced-data-binding` | Advanced binding patterns (MultiBinding, PriorityBinding) |
| `using-converter-markup-extension` | Converter MarkupExtension pattern |
| `implementing-wpf-validation` | Validation strategies |
| `managing-wpf-collectionview-mvvm` | CollectionView in MVVM |
| `configuring-dependency-injection` | DI configuration |
| `structuring-wpf-projects` | Project structure |

</details>

<details>
<summary><b>⚡ Performance & Rendering (9 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `rendering-with-drawingcontext` | DrawingContext rendering |
| `rendering-with-drawingvisual` | DrawingVisual rendering |
| `rendering-wpf-architecture` | Rendering architecture |
| `rendering-wpf-high-performance` | High-performance rendering |
| `implementing-hit-testing` | Hit testing |
| `virtualizing-wpf-ui` | UI virtualization |
| `optimizing-wpf-memory` | Memory optimization |
| `checking-image-bounds-transform` | Image transforms |
| `navigating-visual-logical-tree` | Tree navigation |

</details>

<details>
<summary><b>⌨️ Input & Events (2 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `routing-wpf-events` | Routed events |
| `managing-wpf-popup-focus` | Popup focus management |

</details>

<details>
<summary><b>🎨 Styling & Resources (4 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `managing-styles-resourcedictionary` | Styles & resources |
| `resolving-icon-font-inheritance` | Icon fonts |
| `using-xaml-property-element-syntax` | XAML property element syntax |
| `formatting-wpf-csharp-code` | Code formatting |

</details>

<details>
<summary><b>🔧 Application & Threading (6 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `managing-wpf-application-lifecycle` | App lifecycle |
| `threading-wpf-dispatcher` | Dispatcher & threading |
| `preventing-dispatcher-deadlock` | Avoid sync-over-async deadlocks in event handlers |
| `shutting-down-wpf-gracefully` | OnMainWindowClose / OnExplicitShutdown with async cleanup |
| `embedding-pdb-in-exe` | PDB embedding |
| `publishing-wpf-apps` | Publishing & installers |

</details>

<details>
<summary><b>📦 3rd Party Libraries (8 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `integrating-wpfui-fluent` | WPF-UI (Wpf.Ui) Fluent Design integration |
| `integrating-livecharts2` | LiveCharts2 charting library |
| `validating-with-fluentvalidation` | FluentValidation + INotifyDataErrorInfo bridge |
| `handling-errors-with-erroror` | ErrorOr result pattern for service layer |
| `integrating-nodify` | Nodify node-based editor control |
| `scottplot-syncing-modifier-keys-for-mousewheel` | ScottPlot modifier key sync for mouse wheel zoom |
| `flaui-cross-process-input` | FlaUI cross-process input calibration |
| `flaui-wpf-element-discovery` | FlaUI element discovery in WPF automation |

</details>

<details>
<summary><b>🔷 .NET Common (3 skills)</b></summary>

| Skill | Description |
|-------|-------------|
| `configuring-console-app-di` | Console app DI |
| `implementing-repository-pattern` | Repository pattern |
| `managing-literal-strings` | String management |

</details>

<details>
<summary><b>🔄 Prism 9 Companion (13 PRISM.md files)</b></summary>

12 skills provide `PRISM.md` companion files with Prism 9 (Community License) equivalents:

| Skill | PRISM.md Focus |
|-------|----------------|
| `implementing-communitytoolkit-mvvm` | BindableBase, SetProperty, DelegateCommand |
| `configuring-dependency-injection` | PrismApplication, IContainerRegistry |
| `structuring-wpf-projects` | IModule-based module architecture |
| `managing-wpf-application-lifecycle` | PrismApplication lifecycle |
| `binding-enum-command-parameters` | DelegateCommand\<T\> |
| `implementing-wpf-validation` | ValidatableBindableBase |
| `managing-wpf-collectionview-mvvm` | BindableBase + IContainerRegistry |
| `validating-with-fluentvalidation` | ValidatableBindableBase\<T\> bridge |
| `implementing-repository-pattern` | IContainerRegistry DI |
| `displaying-slider-index` | SetProperty + RaisePropertyChanged |

> Each skill's SKILL.md (CommunityToolkit.Mvvm) and PRISM.md (Prism 9) cross-reference each other.

</details>

<details>
<summary><b>🧪 Testing (1 skill)</b></summary>

| Skill | Description |
|-------|-------------|
| `testing-wpf-viewmodels` | ViewModel unit testing with xUnit + NSubstitute |

</details>

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
├── 📁 skills/                 # 57 Skills
├── 📁 hooks/                  # Event hooks
├── 📄 .mcp.json               # MCP config (HandMirrorMcp only)
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
