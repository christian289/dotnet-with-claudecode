# WpfDevPackMcp — WPF Dev Pack knowledge MCP server

`WpfDevPackMcp` is a small **.NET 10 stdio MCP server** that serves the WPF
knowledge topics of the `wpf-dev-pack` plugin. The knowledge content lives as
plain Markdown under `knowledge/` (at the repo root) in a **local clone** of
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

## Producing the server

The project is a **framework-dependent, platform-agnostic** .NET tool
(`PackAsTool=true`). A single `dotnet pack` produces
one small cross-platform package (~1.3 MB, managed IL) that runs on Windows,
Linux, and macOS with the .NET 10 runtime:

```
dotnet pack mcp/WpfDevPackMcp.csproj -c Release -o mcp/nupkg
dotnet nuget push mcp/nupkg/WpfDevPackMcp.<ver>.nupkg \
  --source https://api.nuget.org/v3/index.json --api-key <NUGET_KEY>
```

`wpf-dev-pack/.mcp.json` runs it with a pinned version:

```json
"WpfDevPackMcp": { "type": "stdio", "command": "dnx", "args": ["WpfDevPackMcp@0.1.3", "--yes"] }
```

Build output (`bin/`, `obj/`, `nupkg/`) is git-ignored.

## Publishing a new version to NuGet (maintainer)

NuGet versions are **immutable** — every release needs a new `<Version>`. The
plugin keeps running on the currently pinned version until you update the pin
in the last step, so publish first and re-pin after.

**One-time setup**

1. Create a NuGet API key: nuget.org → Account → **API Keys** → **Create**
   (scope: **Push**, glob pattern `WpfDevPackMcp` or `*`). Copy it (shown once).
2. Ensure the `WpfDevPackMcp` package id is available on nuget.org (the first
   push registers ownership). If taken, change `<PackageId>` here **and** the
   `dnx` arg in `wpf-dev-pack/.mcp.json`.

**Each release**

1. **Verify** — `dotnet test mcp/WpfDevPackMcp.Tests`, then exercise the tools
   with the MCP Inspector (see "Inspect with the MCP Inspector" below).
2. **Bump** `<Version>` in `mcp/WpfDevPackMcp.csproj` (e.g. `0.1.2` → `0.1.3`).
3. **Pack** — `dotnet pack mcp/WpfDevPackMcp.csproj -c Release -o mcp/nupkg`
4. **Push** —
   ```
   dotnet nuget push mcp/nupkg/WpfDevPackMcp.<ver>.nupkg \
     --source https://api.nuget.org/v3/index.json --api-key <NUGET_KEY>
   ```
5. **Wait** for nuget.org indexing (a few minutes), then verify:
   `dnx WpfDevPackMcp@<ver> --yes`
6. **Re-pin** — update `wpf-dev-pack/.mcp.json`: `WpfDevPackMcp@<old>` →
   `WpfDevPackMcp@<ver>` (only after the new version is live on nuget.org).

Keep the API key secret — never commit it. Steps 3–6 are the maintainer's;
knowledge-content edits do **not** need a republish (they are served live from
the repo).

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

Topic files: `knowledge/<id>/TOPIC.md` — **no YAML frontmatter**.
Title = the first `# H1`; summary = the first `>` blockquote. Variants are the
sibling `PRISM.md` / `ADVANCED.md` files.

## Inspect with the MCP Inspector

```
# Build first, then list tools against the built exe
dotnet build mcp/WpfDevPackMcp.csproj -c Release
npx @modelcontextprotocol/inspector --cli \
  "mcp/bin/Release/net10.0/WpfDevPackMcp.exe" --method tools/list

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

Edit the relevant `knowledge/<id>/TOPIC.md` (or add a new topic
directory plus a keyword in `wpf-dev-pack/hooks/WpfKeywordDetector.cs`) and
push. The server picks it up on its next pull — **no rebuild, no republish, no
plugin version bump**.

> Korean mirror: [README.ko.md](https://github.com/christian289/dotnet-with-claudecode/blob/main/mcp/README.ko.md)
