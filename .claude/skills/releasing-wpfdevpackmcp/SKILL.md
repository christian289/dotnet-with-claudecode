---
name: releasing-wpfdevpackmcp
description: "Releases a new version of the WpfDevPackMcp NuGet MCP server (the server under mcp/), which is versioned independently of the wpf-dev-pack plugin and of knowledge content. Bumps the four version strings that must stay in lockstep (csproj <Version>, the dnx pin in wpf-dev-pack/.mcp.json, and BOTH version fields in mcp/.mcp/server.json), then builds, packs, pushes to NuGet, re-pins after the package goes live, and optionally re-publishes to the Official MCP Registry. Use when code under mcp/ or its packaging changed and must be republished. Do NOT use for knowledge-topic edits (served live, no version bump) or plugin-only changes (use /wpf-dev-pack-release). Usage: /releasing-wpfdevpackmcp [version e.g. 0.1.4]"
disable-model-invocation: true
argument-hint: [version e.g. 0.1.4]
---

# Releasing WpfDevPackMcp (NuGet MCP server)

Owner-only release tool for the **WpfDevPackMcp** NuGet package (the MCP server
under `mcp/`). Its version is independent of the wpf-dev-pack **plugin** version
(`/wpf-dev-pack-release`) and of **knowledge** content (served live, never versioned).

## Essential (Post-Compact)

These rules MUST survive context compression. If prior context is lost, re-read this.

1. A WpfDevPackMcp release bumps **4 version strings across 3 files**, all to the SAME value:
   - `mcp/WpfDevPackMcp.csproj` → `<Version>`
   - `wpf-dev-pack/.mcp.json` → the dnx pin `WpfDevPackMcp@<ver>`
   - `mcp/.mcp/server.json` → top-level `"version"`
   - `mcp/.mcp/server.json` → `packages[0].version`
2. **Order matters**: bump csproj + server.json BEFORE `dotnet pack`; update the
   `.mcp.json` pin ONLY AFTER the new version is live on nuget.org (the plugin keeps
   running on the old pin until the new one is available).
3. **Do NOT bump** for: edits under `knowledge/` (served live — just `git push`) or
   plugin-only changes (skills/hooks/rules → `/wpf-dev-pack-release`).
4. NuGet versions are immutable — never reuse a number; always increase.
5. In `server.json`, the nuget `registryBaseUrl` MUST be
   `https://api.nuget.org/v3/index.json` (the bare `https://api.nuget.org` fails
   Official MCP Registry validation with HTTP 400).

## Arguments

- `$0` = target version (e.g. `0.1.4`). If omitted, increment the **patch** of the
  current `<Version>` in `mcp/WpfDevPackMcp.csproj`.

## Workflow

Copy this checklist and track progress:

```
Release Progress:
- [ ] 0  Verify identity (GitHub + NuGet API key)
- [ ] 1  Determine {new-version}
- [ ] 2  Bump the 4 version strings
- [ ] 3  Verify all 4 match
- [ ] 4  Build + test
- [ ] 5  Pack
- [ ] 6  Push to NuGet (.nupkg → .snupkg pushed automatically)
- [ ] 7  Wait for nuget.org indexing
- [ ] 8  Re-pin wpf-dev-pack/.mcp.json
- [ ] 9  Commit + push
- [ ] 10 (optional) Re-publish to the Official MCP Registry
```

### Step 0 — identity

```bash
gh auth status        # switch to christian289 if needed: gh auth switch --user christian289
```
Have the NuGet API key ready (nuget.org → Account → API Keys, scope Push). Never commit it.

### Step 1 — determine {new-version}

```bash
grep "<Version>" mcp/WpfDevPackMcp.csproj
```
Use `$0` if provided, otherwise patch+1 (e.g. `0.1.3` → `0.1.4`).

### Step 2 — bump the 4 version strings (Essential rule 1)

Set every one of these to `{new-version}`:

| File | What to change |
|------|----------------|
| `mcp/WpfDevPackMcp.csproj` | `<Version>{new-version}</Version>` |
| `wpf-dev-pack/.mcp.json` | `"args": ["WpfDevPackMcp@{new-version}", "--yes"]` — **do this in Step 8, not now** |
| `mcp/.mcp/server.json` | top-level `"version": "{new-version}"` |
| `mcp/.mcp/server.json` | `packages[0].version: "{new-version}"` |

> The `.mcp.json` pin is intentionally deferred to Step 8 (publish-then-pin). In this
> step bump only the csproj `<Version>` and the two `server.json` fields.

### Step 3 — verify all match (do this before packing)

```bash
grep -H "<Version>" mcp/WpfDevPackMcp.csproj
grep -nE '"version"' mcp/.mcp/server.json   # expect TWO lines, both {new-version}
```
If any differ, fix and re-run before continuing.

### Step 4 — build + test

```bash
dotnet test mcp/WpfDevPackMcp.Tests/WpfDevPackMcp.Tests.csproj
```
Must be green. (Optional: exercise tools with the MCP Inspector — see `mcp/README.md`.)

### Step 5 — pack

```bash
dotnet pack mcp/WpfDevPackMcp.csproj -c Release -o mcp/nupkg
```
Produces `WpfDevPackMcp.{new-version}.nupkg` and `.snupkg` (both git-ignored).

### Step 6 — push to NuGet

```bash
dotnet nuget push "mcp/nupkg/WpfDevPackMcp.{new-version}.nupkg" \
  --api-key <NUGET_KEY> --source https://api.nuget.org/v3/index.json
```
The matching `.snupkg` symbol package is pushed automatically.

### Step 7 — wait for indexing

Poll until the new version resolves (a few minutes):

```bash
dnx WpfDevPackMcp@{new-version} --yes
```

### Step 8 — re-pin (only after Step 7 succeeds)

Update `wpf-dev-pack/.mcp.json`: `WpfDevPackMcp@{old}` → `WpfDevPackMcp@{new-version}`.
Also update the example pins in `mcp/README.md` / `mcp/README.ko.md` if they show a version.

### Step 9 — commit + push

```bash
git add -A
git commit -m "chore(wpf-dev-pack): WpfDevPackMcp {old} -> {new-version} - {summary}"
git push origin main
```

### Step 10 — Official MCP Registry (optional)

Only if the server is (or should be) listed at `registry.modelcontextprotocol.io`.
Requires the package from Step 6 to already be live. `mcp-publisher` is a standalone
binary from the registry's GitHub releases (not a NuGet/dnx tool):

```bash
# download mcp-publisher for your OS from
#   https://github.com/modelcontextprotocol/registry/releases/latest
mcp-publisher login github                 # auth as christian289 (io.github.christian289/*)
mcp-publisher publish mcp/.mcp/server.json
```

## When NOT to use

- **Knowledge edits only** (`knowledge/<id>/TOPIC.md`, companions): served live from
  the clone on the server's next pull — no pack, no push, no version bump. Just `git push`.
- **Plugin-only release** (command skills, hooks, rules in `wpf-dev-pack/`): use
  `/wpf-dev-pack-release` instead; it bumps `plugin.json` + README badges + changelog.
- **No MCP code/packaging change**: if nothing under `mcp/` changed, there is nothing
  to republish.

## Related

- `/wpf-dev-pack-release` — the **plugin** version release (different version stream).
- `mcp/README.md` — full publish reference + MCP Inspector usage.
- Repo `.claude/CLAUDE.md` → "WpfDevPackMcp NuGet package versioning".
