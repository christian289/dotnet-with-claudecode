---
description: "Shows the current WpfDevPackMcp configuration and server state: the absolute paths and values of config.json (repoPath, branch) and state.json (LastPullUtc, Managed), plus the WPFDEVPACK_REPO_PATH / WPFDEVPACK_PULL_TTL_MINUTES env overrides. Use to inspect or troubleshoot how the MCP resolves the knowledge repo — wrong path or branch, an unexpected Managed flag, or an env var overriding config.json."
---

# Show WpfDevPackMcp Configuration

Print the on-disk WpfDevPackMcp configuration and server state so the user can see exactly what the MCP server reads and from where.

Run the file-based app and surface its output verbatim:

```bash
dotnet "${CLAUDE_PLUGIN_ROOT}/scripts/ShowWpfDevPackConfig.cs"
```

It prints the absolute paths and current contents of:

- `~/.wpf-dev-pack-mcp/config.json` — user-written `repoPath` + `branch`
- `~/.wpf-dev-pack-mcp/state.json` — server-managed `LastPullUtc` + `Managed`

plus the `WPFDEVPACK_REPO_PATH` and `WPFDEVPACK_PULL_TTL_MINUTES` environment
overrides. **When `WPFDEVPACK_REPO_PATH` is set it overrides config.json's
`repoPath`** — a common cause of "the MCP isn't reading the repo I configured."

To change values: `/wpf-dev-pack:set-repo-path` (repoPath), `/wpf-dev-pack:set-repo-branch` (branch), `/wpf-dev-pack:set-repo-managed` (Managed flag).
