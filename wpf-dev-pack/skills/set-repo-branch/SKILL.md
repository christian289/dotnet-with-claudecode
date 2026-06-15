---
description: "Sets the git branch the WpfDevPackMcp server tracks for WPF knowledge, in ~/.wpf-dev-pack-mcp/config.json (preserving repoPath). Use when the knowledge topics you need live on a branch other than main, or to point the MCP back at main. Companion to /wpf-dev-pack:set-repo-path."
argument-hint: "<branch>"
disable-model-invocation: true
---

# Set WpfDevPackMcp Branch

Sets the `branch` field in `~/.wpf-dev-pack-mcp/config.json` — the branch the MCP
server fetches/serves WPF knowledge from — leaving `repoPath` unchanged.

**If `$0` is empty, use AskUserQuestion to ask which branch to track (e.g.,
`main`). Do NOT proceed until a branch is provided.**

```bash
dotnet "${CLAUDE_PLUGIN_ROOT}/scripts/SetWpfDevPackBranch.cs" "$0"
```

Surface the output verbatim, including any `!` warning (e.g. repoPath not set
yet → run `/wpf-dev-pack:set-repo-path` first). The new branch takes effect on
the next WpfDevPackMcp refresh.

> When `Managed=true` (see `/wpf-dev-pack:set-repo-managed`), refresh
> hard-resets the clone to `origin/<branch>`, so the branch must exist on the
> remote. View the current branch with `/wpf-dev-pack:show-wpf-dev-pack-config`.
