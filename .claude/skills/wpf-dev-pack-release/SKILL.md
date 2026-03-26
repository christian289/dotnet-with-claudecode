---
name: wpf-dev-pack-release
description: "Executes the wpf-dev-pack release workflow: runs VersionReleaseChecker, commits changes, pushes, creates a GitHub tag and release note. Owner-only release tool. Use when releasing a new version of wpf-dev-pack plugin."
---

# wpf-dev-pack Release Workflow

Owner-only release tool for the wpf-dev-pack plugin.

## Workflow

Run the following steps in order:

### Step 0: Verify GitHub Account

Ensure `christian289` is the active GitHub account:

```bash
gh auth status
```

If not active, switch:

```bash
gh auth switch --user christian289
```

### Step 1: Run VersionReleaseChecker

Execute the version release checker to verify consistency:

```bash
dotnet ".claude/skills/wpf-dev-pack-release/scripts/VersionReleaseChecker.cs"
```

If issues are found, fix them before proceeding:
- **RELEASE MISSING**: Expected — will be created in Step 5
- **README MISMATCH**: Update README.md and README.ko.md counts to match actual
- **GITHUB_PROFILE MISMATCH**: Update christian289/christian289 README.md via `gh api`

### Step 2: Commit Changes

Stage and commit all pending changes with an English commit message:

```bash
git add -A
git commit -m "feat(wpf-dev-pack): v{version} - {summary of changes}"
```

- Commit message MUST be in English
- Use conventional commit format: `feat`, `fix`, `refactor`, `chore`
- Include `[skip-version]` if version was already manually bumped

### Step 3: Push to Remote

```bash
git push origin main
```

### Step 4: Create GitHub Tag

```bash
git tag wpf-dev-pack-v{version}
git push origin wpf-dev-pack-v{version}
```

Tag format: `wpf-dev-pack-v{version}` (e.g., `wpf-dev-pack-v1.4.7`)

### Step 5: Create GitHub Release Note

```bash
gh release create wpf-dev-pack-v{version} \
  --repo christian289/dotnet-with-claudecode \
  --title "wpf-dev-pack v{version}" \
  --notes "$(cat <<'EOF'
## What's New

- {list changes since last release}

## Stats

- **Skills**: {count}
- **Agents**: {count}
- **MCP Servers**: {count}
EOF
)"
```

- Generate release notes by comparing with the previous tag
- Use `git log` between previous tag and current to summarize changes
- Include skill/agent/MCP server counts in Stats section

### Step 6: Update GitHub Profile README

Update the wpf-dev-pack section in `christian289/christian289` README.md:

```bash
# Fetch current profile README
gh api repos/christian289/christian289/contents/README.md --jq '.content' | base64 -d > /tmp/profile-readme.md

# Edit the file to update version badge and Stats counts
# Then push back:
gh api repos/christian289/christian289/contents/README.md \
  --method PUT \
  --field message="chore: update wpf-dev-pack to v{version}" \
  --field content="$(base64 -w 0 /tmp/profile-readme.md)" \
  --field sha="$(gh api repos/christian289/christian289/contents/README.md --jq '.sha')"
```

- Update version number in the wpf-dev-pack badge/section
- Update Skills, Agents, MCP Servers counts to match actual

## Reading the Version

Read the version from `wpf-dev-pack/.claude-plugin/plugin.json`:

```json
{ "version": "x.y.z" }
```

## Checklist

- [ ] GitHub account switched to `christian289`
- [ ] VersionReleaseChecker passes (no mismatches)
- [ ] All changes committed with English message
- [ ] Pushed to remote
- [ ] Tag `wpf-dev-pack-v{version}` created and pushed
- [ ] GitHub release created with changelog
- [ ] GitHub profile README updated with new version and counts
