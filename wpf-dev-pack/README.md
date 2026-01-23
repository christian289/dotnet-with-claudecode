# wpf-dev-pack

WPF development toolkit for Claude Code with specialized agents and skills.

## Features

- **9 Specialized Agents**: architect, control-designer, xaml-designer, mvvm-expert, data-binding-expert, performance-optimizer, code-reviewer, code-formatter, serena-initializer
- **57 Skills**: CustomControl, MVVM, XAML styling, data binding, validation, behaviors, high-performance rendering, code formatting, .NET advanced APIs
- **5 Commands**: make-wpf-custom-control, make-wpf-project, make-wpf-converter, make-wpf-behavior, make-wpf-usercontrol
- **4 MCP Servers**: Context7, MicrosoftDocs, Sequential-thinking, Serena
- **C# LSP Server**: Requires `csharp-lsp` from Claude Code marketplace
- **Auto-detection**: WPF keywords trigger relevant skills automatically

## Requirements

- **.NET 10 SDK**: This plugin requires [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) or later

## Installation

```bash
claude --plugin-dir ./wpf-dev-pack
```

## Usage

```bash
# Generate CustomControl
/wpf-dev-pack:make-wpf-custom-control MyButton Button

# Scaffold new WPF project (CommunityToolkit.Mvvm)
/wpf-dev-pack:make-wpf-project MyApp

# Scaffold new WPF project with Prism framework
/wpf-dev-pack:make-wpf-project MyApp --prism

# Generate IValueConverter
/wpf-dev-pack:make-wpf-converter BoolToVisibility

# Generate Behavior
/wpf-dev-pack:make-wpf-behavior SelectAllOnFocus TextBox

# Generate UserControl
/wpf-dev-pack:make-wpf-usercontrol SearchBox

# Request code review
"Review this ViewModel code from MVVM perspective"
```

## Agents

| Agent | Model | Role |
|-------|-------|------|
| wpf-architect | Opus | Strategic architecture analysis (READ-ONLY) |
| wpf-control-designer | Sonnet | CustomControl design and implementation |
| wpf-xaml-designer | Sonnet | XAML Style/ControlTemplate design |
| wpf-mvvm-expert | Sonnet | MVVM pattern implementation |
| wpf-data-binding-expert | Sonnet | Complex bindings, converters, validation |
| wpf-performance-optimizer | Sonnet | Rendering and performance optimization |
| wpf-code-reviewer | Opus | Code quality review (READ-ONLY) |
| code-formatter | Sonnet | C# code formatting and style enforcement |
| serena-initializer | Haiku | Serena project activation and onboarding |

## MCP Servers

| Server | Description |
|--------|-------------|
| Context7 | Up-to-date library documentation and code examples |
| MicrosoftDocs | Official Microsoft technical documentation |
| Sequential-thinking | Step-by-step problem solving and analysis |
| Serena | Semantic code analysis and manipulation tools |

### Serena MCP Setup

> **⚠️ Important**: [uv](https://docs.astral.sh/uv/) must be installed on your machine to use Serena.

If Serena is not installed, you can test it locally with:

```bash
uvx --from git+https://github.com/oraios/serena serena start-mcp-server
```

For more details, see the [Serena Quick Start Guide](https://github.com/oraios/serena?tab=readme-ov-file#quick-start).

**Note**: The above command is for local project testing only. For global configuration, the `.mcp.json` file in this plugin already contains the Serena configuration. The plugin will automatically use Serena when loaded.

## LSP Server (Required)

This plugin requires the official **csharp-lsp** plugin from Claude Code marketplace for C# code intelligence.

### Installation

```bash
claude /install-plugin csharp-lsp
```

Or install via Claude Code settings:
1. Open Claude Code settings
2. Go to Plugins → Marketplace
3. Search for `csharp-lsp`
4. Click Install

> **Note**: The `csharp-lsp` plugin provides IntelliSense, go-to-definition, and other language features for `.cs` and `.csx` files.

## Skills (57)

### WPF Related (45)
- authoring-wpf-controls
- binding-enum-command-parameters
- checking-image-bounds-transform
- configuring-dependency-injection
- creating-wpf-animations
- creating-wpf-dialogs
- creating-wpf-flowdocument
- customizing-controltemplate
- defining-wpf-dependencyproperty
- designing-wpf-customcontrol-architecture
- developing-wpf-customcontrols
- displaying-slider-index
- handling-wpf-input-commands
- implementing-2d-graphics
- implementing-hit-testing
- implementing-wpf-adorners
- implementing-wpf-automation
- implementing-wpf-dragdrop
- integrating-wpf-media
- localizing-wpf-applications
- managing-styles-resourcedictionary
- managing-wpf-application-lifecycle
- managing-wpf-collectionview-mvvm
- managing-wpf-popup-focus
- mapping-viewmodel-view-datatemplate
- navigating-visual-logical-tree
- optimizing-wpf-memory
- rendering-with-drawingcontext
- rendering-with-drawingvisual
- rendering-wpf-architecture
- rendering-wpf-high-performance
- resolving-icon-font-inheritance
- routing-wpf-events
- structuring-wpf-projects
- threading-wpf-dispatcher
- understanding-wpf-content-model
- using-wpf-clipboard
- virtualizing-wpf-ui
- advanced-data-binding
- implementing-wpf-validation
- using-wpf-behaviors-triggers
- migrating-wpf-to-dotnet
- formatting-wpf-csharp-code
- using-xaml-property-element-syntax
- using-converter-markup-extension

### .NET Common (12)
- configuring-console-app-di
- handling-async-operations
- implementing-communitytoolkit-mvvm
- implementing-io-pipelines
- implementing-pubsub-pattern
- implementing-repository-pattern
- managing-literal-strings
- optimizing-fast-lookup
- optimizing-io-operations
- optimizing-memory-allocation
- processing-parallel-tasks
- using-generated-regex

## Plugin Structure

```
wpf-dev-pack/
├── .claude-plugin/
│   └── plugin.json          # Plugin manifest (required)
├── agents/                   # Specialized subagents (9)
│   ├── wpf-architect.md
│   ├── wpf-control-designer.md
│   ├── wpf-xaml-designer.md
│   ├── wpf-mvvm-expert.md
│   ├── wpf-data-binding-expert.md
│   ├── wpf-performance-optimizer.md
│   ├── wpf-code-reviewer.md
│   ├── code-formatter.md
│   └── serena-initializer.md
├── commands/                 # User-invocable skills
│   └── make-wpf-custom-control/
│       └── SKILL.md
├── skills/                   # Agent skills (57)
│   ├── authoring-wpf-controls/
│   │   └── SKILL.md
│   └── ...
├── hooks/                    # Event hooks
│   ├── hooks.json
│   ├── wpf-keyword-detector.js
│   └── xaml-validator.js
├── .mcp.json                 # MCP server configuration
├── README.md
└── LICENSE
```

### Configuration Files

| File | Purpose |
|------|---------|
| `.claude-plugin/plugin.json` | Plugin metadata and manifest |
| `.mcp.json` | MCP servers (Context7, MicrosoftDocs, Sequential-thinking, Serena) |
| `hooks/hooks.json` | Event hooks for auto-detection |

### Required Marketplace Plugin

| Plugin | Purpose |
|--------|---------|
| `csharp-lsp` | C# Language Server for code intelligence |

### Plugin Reference

This plugin follows the [Claude Code Plugin Specification](https://code.claude.com/docs/en/plugins-reference).

## References

This plugin was created based on the following resources:

- **WPF Samples**: https://github.com/microsoft/WPF-Samples
  - Official Microsoft WPF sample repository
  - Patterns for CustomControl, DependencyProperty, Animations, etc.

- **WPF Graphics & Multimedia**: https://learn.microsoft.com/ko-kr/dotnet/desktop/wpf/graphics-multimedia/
  - Official Microsoft documentation
  - DrawingContext, DrawingVisual, 2D Graphics, Animation

- **Plugin Architecture Reference**: https://github.com/Yeachan-Heo/oh-my-claudecode
  - Agent-based orchestration pattern

## Author

vincent lee

## License

MIT
