# WpfDevPackMcp — WPF Dev Pack knowledge MCP server

`WpfDevPackMcp` is a small **.NET 10 stdio MCP server** that serves the WPF
knowledge topics of the `wpf-dev-pack` plugin. The knowledge content lives as
plain Markdown under `wpf-dev-pack/knowledge/` in a **local clone** of
`christian289/dotnet-with-claudecode`; the server reads it from disk and
refreshes it with `git pull` on demand. This keeps the knowledge out of the
plugin's `skills/` loader (so it no longer consumes session context) while
letting you update it by editing Markdown — **no server rebuild or republish
needed**.

This project lives **outside the plugin** (`mcp/`, at the repo root). The
plugin references it only through `wpf-dev-pack/.mcp.json` (the same way it
references HandMirrorMcp).

## Prerequisites

- .NET SDK **10.0.300+**
- `git` on `PATH` (used for the on-demand refresh; the repo is public, no auth)

## Build & test

```
dotnet build mcp/WpfDevPackMcp.csproj
dotnet test  mcp/WpfDevPackMcp.Tests
```

## Two ways to produce the server

### 1. dnx + NuGet tool — how `.mcp.json` runs it

The project is a **framework-dependent, platform-agnostic** .NET tool
(`PackAsTool=true`, no `RuntimeIdentifiers`). A single `dotnet pack` produces
one small cross-platform package (~1.3 MB, managed IL) that runs on Windows,
Linux, and macOS with the .NET 10 runtime:

```
dotnet pack mcp/WpfDevPackMcp.csproj -c Release -o mcp/nupkg
dotnet nuget push mcp/nupkg/WpfDevPackMcp.<ver>.nupkg \
  --source https://api.nuget.org/v3/index.json --api-key <NUGET_KEY>
```

`wpf-dev-pack/.mcp.json` runs it with a pinned version:

```json
"WpfDevPackMcp": { "type": "stdio", "command": "dnx", "args": ["WpfDevPackMcp@0.1.0", "--yes"] }
```

**Why not self-contained / RID-specific?** The .NET runtime is free here: the
wpf-dev-pack plugin already mandates the .NET 10 SDK (its hooks are `dotnet`
file-based apps and `.mcp.json` launches via `dnx`), so every machine that runs
this server already has .NET 10. A framework-dependent tool is therefore one
tiny cross-platform package and the simplest possible publish. (.NET 10 *can*
emit self-contained per-RID tool packages — `<RuntimeIdentifiers>win-x64;…;any</RuntimeIdentifiers>`
plus a RID-conditional `<SelfContained>` — and `dnx` auto-selects them; that's
only worth it for running where .NET isn't installed, which isn't the case
here.) **Native AOT** is likewise possible in principle (the build reaches
native codegen), but the JSON config code would need System.Text.Json source
generation to be AOT-safe, AOT needs a per-OS C++ toolchain, and it brings no
benefit when the runtime is already present — so it is not used.

### 2. Single-file, self-contained executable — publish profile (optional)

A standalone `.exe` artifact (not a NuGet tool). Useful if you want a bare
executable without NuGet/dnx. Profile:
`mcp/Properties/PublishProfiles/win-x64.pubxml`.

```
dotnet publish mcp/WpfDevPackMcp.csproj -p:PublishProfile=win-x64
```

Output: `mcp/bin/Release/net10.0/publish/win-x64/WpfDevPackMcp.exe` — one
standalone `.exe` (~37 MB), runtime bundled. For other OSes, add a sibling
profile with the matching RID (`linux-x64`, `osx-arm64`, …). Build output
(`bin/`, `obj/`, `nupkg/`) is git-ignored.

## Runtime configuration (required)

The server must know where the local clone is. Resolution order:

1. `WPFDEVPACK_REPO_PATH` environment variable
2. `~/.wpf-dev-pack-mcp/config.json` — `{ "repoPath": "...", "branch": "main" }`

Configure it from a Claude Code session:

```
/wpf-dev-pack:set-repo-path <path-to-local-clone>
```

- If neither is set, the tools return a "not configured" error — the plugin's
  `RepoPathGuard` PreToolUse hook blocks the call first with guidance.
- If the configured path is empty / not a git repo, the server clones the
  public repo into it on first use.
- Optional: `WPFDEVPACK_PULL_TTL_MINUTES` (default `60`) controls how often the
  server pulls before serving.

## Tools

| Tool | Description |
|------|-------------|
| `list_wpf_topics()` | All topics with a one-line summary + companion files |
| `get_wpf_topic(id, variant?)` | Full Markdown; `variant`: `default` (TOPIC.md) \| `prism` (PRISM.md) \| `advanced` (ADVANCED.md) |
| `search_wpf_topics(query, maxResults?)` | Ranked matches over id / title / summary / body |
| `refresh_wpf_knowledge()` | Force a `git pull` + rescan |

Topic files: `wpf-dev-pack/knowledge/<id>/TOPIC.md` — **no YAML frontmatter**.
Title = the first `# H1`; summary = the first `>` blockquote. Variants are the
sibling `PRISM.md` / `ADVANCED.md` files.

## Inspect with the MCP Inspector

```
# List tools (against the single-file exe)
npx @modelcontextprotocol/inspector --cli \
  "mcp/bin/Release/net10.0/publish/win-x64/WpfDevPackMcp.exe" --method tools/list

# Call a tool (set the repo path first, or rely on ~/.wpf-dev-pack-mcp/config.json)
npx @modelcontextprotocol/inspector --cli "<exe>" \
  --method tools/call --tool-name list_wpf_topics

npx @modelcontextprotocol/inspector --cli "<exe>" \
  --method tools/call --tool-name get_wpf_topic \
  --tool-arg id=implementing-communitytoolkit-mvvm --tool-arg variant=prism
```

`stdout` carries only MCP JSON-RPC; all logs go to `stderr`. The server gives
its `git` child processes a closed `stdin`, so they never inherit/block on the
server's JSON-RPC pipe.

## Updating knowledge content

Edit the relevant `wpf-dev-pack/knowledge/<id>/TOPIC.md` (or add a new topic
directory plus a keyword in `wpf-dev-pack/hooks/WpfKeywordDetector.cs`) and
push. The server picks it up on its next pull — **no rebuild, no republish, no
plugin version bump**.

> Korean mirror: [README.ko.md](README.ko.md)
