---
description: "Sets the server-managed 'Managed' flag in ~/.wpf-dev-pack-mcp/state.json. Managed=true lets WpfDevPackMcp git reset --hard its clone to origin/<branch> on refresh (only safe for a dedicated clone the server owns); Managed=false makes refresh non-destructive (ff-only when clean, skip when dirty) — required when repoPath is your active working repo. Use to stop the MCP from resetting your working tree, or to hand a dedicated clone back to server management."
argument-hint: "<true|false>"
disable-model-invocation: true
---

# Set WpfDevPackMcp Managed Flag

Sets `Managed` in `~/.wpf-dev-pack-mcp/state.json` (preserving `LastPullUtc`).
This flag decides how the server refreshes the clone at `repoPath`.

**If `$0` is empty, use AskUserQuestion to ask for `true` or `false`. Do NOT
proceed until a value is provided.**

```bash
dotnet "${CLAUDE_PLUGIN_ROOT}/scripts/SetWpfDevPackManaged.cs" "$0"
```

What it controls (`mcp/Git/RepoRefresher.cs`):

- **`true`** — on refresh the server runs `git fetch` + `git reset --hard origin/<branch>` on `repoPath`. Use ONLY for a dedicated clone the server created/owns. **Never set `true` when `repoPath` is your active working repo** — a refresh will discard uncommitted changes and reset the branch to `origin/<branch>`, destroying local work.
- **`false`** — refresh is non-destructive: `git pull --ff-only` when the tree is clean, skip when dirty. Use when `repoPath` points at your own working clone.

Surface the output verbatim. Inspect the current flag with
`/wpf-dev-pack:show-wpf-dev-pack-config`.
