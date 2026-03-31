---
description: "Executes the wpf-dev-pack release workflow: bumps version, updates all version references, commits, pushes, creates GitHub tag/release, and updates GitHub profile. Use when releasing a new version of wpf-dev-pack plugin. Usage: /wpf-dev-pack-release [version]"
---

# wpf-dev-pack Release Workflow

Owner-only release tool for the wpf-dev-pack plugin.
Execute all steps sequentially without asking for confirmation at each step.

## Arguments

- `$ARGUMENTS` contains the optional version argument.
- **Version specified** (e.g., `1.6.0`): Use that exact version.
- **No version specified**: Auto-increment **patch** from current version (e.g., `1.5.0` → `1.5.1`).

## Step 0: Verify GitHub Account

```bash
gh auth status
```

If `christian289` is not the active account:

```bash
gh auth switch --user christian289
```

## Step 1: Determine Version

Read current version:

```bash
cat wpf-dev-pack/.claude-plugin/plugin.json
```

Determine `{new-version}`:
- If `$ARGUMENTS` contains a semver string → use it as `{new-version}`
- Otherwise → increment patch of current version (e.g., `1.5.0` → `1.5.1`)

## Step 2: Update Version in All Files

Update version in these 3 files:

1. **`wpf-dev-pack/.claude-plugin/plugin.json`** — `"version": "{new-version}"`
2. **`wpf-dev-pack/README.md`** — badge `version-{new-version}-blue`
3. **`wpf-dev-pack/README.ko.md`** — badge `version-{new-version}-blue`

## Step 3: Run VersionReleaseChecker

```bash
dotnet ".claude/skills/wpf-dev-pack-release/scripts/VersionReleaseChecker.cs" -- --standalone
```

Fix any issues before proceeding:
- **README MISMATCH**: Update README.md and README.ko.md counts, then re-run checker.
- **GITHUB_PROFILE MISMATCH**: Will be fixed in Step 8.
- **RELEASE MISSING**: Expected — will be created in Step 7.

## Step 4: Commit Changes

```bash
git add wpf-dev-pack/.claude-plugin/plugin.json wpf-dev-pack/README.md wpf-dev-pack/README.ko.md
```

Include any other staged changes, then commit:

```bash
git add -A
git commit -m "feat(wpf-dev-pack): v{new-version} - {summary of changes since last tag}"
```

## Step 5: Push to Remote

```bash
git push origin main
```

## Step 6: Create GitHub Tag

```bash
git tag wpf-dev-pack-v{new-version}
git push origin wpf-dev-pack-v{new-version}
```

## Step 7: Create GitHub Release Note

Get previous tag and changelog:

```bash
git tag --sort=-v:refname | grep wpf-dev-pack | head -2
git log {previous-tag}..wpf-dev-pack-v{new-version} --oneline
```

Create release:

```bash
gh release create wpf-dev-pack-v{new-version} \
  --repo christian289/dotnet-with-claudecode \
  --title "wpf-dev-pack v{new-version}" \
  --notes "$(cat <<'EOF'
## What's New

- {changes from git log}

## Stats

- **Skills**: {actual count}
- **Agents**: {actual count}
- **MCP Servers**: {actual count}
EOF
)"
```

## Step 8: Update GitHub Profile README

```bash
gh api repos/christian289/christian289/contents/README.md --jq '.content' | base64 -d > /tmp/profile-readme.md
```

Update version badge and Skills/Agents/MCP Servers counts using `sed`, then push:

```bash
gh api repos/christian289/christian289/contents/README.md \
  --method PUT \
  --field message="chore: update wpf-dev-pack to v{new-version}" \
  --field content="$(base64 -w 0 /tmp/profile-readme.md)" \
  --field sha="$(gh api repos/christian289/christian289/contents/README.md --jq '.sha')"
```

## Step 9: Final Verification

```bash
dotnet ".claude/skills/wpf-dev-pack-release/scripts/VersionReleaseChecker.cs" -- --standalone
```

All checks must pass. Report the final result.
