---
description: "Sets the local clone path of the christian289/dotnet-with-claudecode repository so the WpfDevPackMcp server can read WPF knowledge topics from disk. Use when the WpfDevPackMcp server reports the repo path is not configured, when first setting up wpf-dev-pack on a machine, or when moving the local clone. Writes ~/.wpf-dev-pack-mcp/config.json."
argument-hint: "<repo-path> [branch]"
disable-model-invocation: true
---

# Set WpfDevPackMcp Repository Path

Records the on-disk location of a local clone of
`christian289/dotnet-with-claudecode` so the `WpfDevPackMcp` MCP server can
serve WPF knowledge topics from `knowledge/` (at the repo root).

**If `$0` is empty, use AskUserQuestion to ask for the absolute path to the
local clone (e.g., `C:/Users/<you>/src/dotnet-with-claudecode`). Do NOT
proceed until a path is provided.** Optionally accept a branch as `$1`
(default `main`).

Run the file-based app to validate and persist the path:

```bash
dotnet "${CLAUDE_PLUGIN_ROOT}/scripts/SetWpfDevPackRepoPath.cs" "$0" "$1"
```

Surface the app's output verbatim, including any `!` warnings (path missing,
no `.git`, etc.). After it succeeds, tell the user the WpfDevPackMcp tools
are now usable and that the server will clone/pull the repo as needed.

## Companion skills

- `/wpf-dev-pack:show-wpf-dev-pack-config` — view the current config.json /
  state.json paths and values (and any `WPFDEVPACK_REPO_PATH` env override).
- `/wpf-dev-pack:set-repo-branch <branch>` — change the tracked branch
  (preserves repoPath).
- `/wpf-dev-pack:set-repo-managed <true|false>` — control whether refresh
  hard-resets the clone. Set `false` if you ever point `repoPath` at your own
  working repo, so a refresh cannot `git reset --hard` your working tree.

