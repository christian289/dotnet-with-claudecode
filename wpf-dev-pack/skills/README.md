[🇰🇷 한국어](./README.ko.md)

# Skills

Command skills for WPF/.NET development. All are slash-invocable except
`formatting-wpf-csharp-code`, which Claude applies automatically (it is
`user-invocable: false`; the CodeFormatter PostToolUse hook also runs on edits).

> **Knowledge topics are not skills.** The ~50 WPF knowledge topics (MVVM,
> rendering, threading, styling, 3rd-party libraries, Prism 9 companions,
> testing, …) are **not** bundled under `skills/`. They live as plain Markdown
> at `knowledge/<id>/TOPIC.md` in the repo and are served on demand by the
> **WpfDevPackMcp** MCP server (`search_wpf_topics` / `get_wpf_topic`). Run
> [`set-repo-path`](./set-repo-path/SKILL.md) once to point the server at your
> local clone. See the plugin [README](../README.md) → "Skills & Knowledge".

## Command Skills (11)

### 🛠️ Scaffolding (7)

| Skill | Description |
|-------|-------------|
| `make-wpf-project` | Scaffold a WPF project (MVVM/DI) |
| `make-wpf-custom-control` | Scaffold a CustomControl |
| `make-wpf-usercontrol` | Scaffold a UserControl |
| `make-wpf-converter` | Scaffold an IValueConverter |
| `make-wpf-behavior` | Scaffold a Behavior<T> |
| `make-wpf-viewmodel` | Scaffold ViewModel + View + DataTemplate mapping |
| `make-wpf-service` | Scaffold service interface + impl + DI registration |

### 🎨 Code Quality (1)

| Skill | Description |
|-------|-------------|
| `formatting-wpf-csharp-code` | C# / XAML formatting & style (auto-applied on edits by the CodeFormatter hook) |

### 🔧 Plugin Operations (3)

| Skill | Description |
|-------|-------------|
| `collecting-wpf-dev-pack-feedback` | Capture anonymized feedback docs for later application |
| `configuring-wpf-dev-pack-language` | Set the per-project response language (`.claude/wpf-dev-pack.local.md`) |
| `set-repo-path` | Configure the local repo-clone path WpfDevPackMcp reads knowledge from |

## Usage

Invoke a command skill directly:

```
/wpf-dev-pack:make-wpf-project MyApp
```

For WPF knowledge, just ask your question — the WpfDevPackMcp MCP server's
instructions guide the agent to search the topic catalog (`search_wpf_topics`)
and load the relevant topic (`get_wpf_topic`) before answering.
