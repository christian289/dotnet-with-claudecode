---
description: "Executes the wpf-dev-pack release workflow: runs VersionReleaseChecker, commits changes, pushes, creates a GitHub tag and release note. Owner-only release tool. Use when releasing a new version of wpf-dev-pack plugin."
---

# wpf-dev-pack Release Workflow

Owner-only release tool for the wpf-dev-pack plugin.
Execute all steps sequentially without asking for confirmation at each step.

## Step 0: Verify GitHub Account

```bash
gh auth status
```

If `christian289` is not the active account:

```bash
gh auth switch --user christian289
```

## Step 1: Read Version

```bash
cat wpf-dev-pack/.claude-plugin/plugin.json
```

Extract the `version` field. Use this as `{version}` throughout.

## Step 2: Run VersionReleaseChecker

```bash
dotnet ".claude/skills/wpf-dev-pack-release/scripts/VersionReleaseChecker.cs" -- --standalone
```

Fix any issues before proceeding:
- **README MISMATCH**: Update README.md and README.ko.md counts
- **GITHUB_PROFILE MISMATCH**: Will be fixed in Step 7
- **RELEASE MISSING**: Expected — will be created in Step 6

If README fixes are needed, apply them now and re-run the checker until README checks pass.

## Step 3: Commit Changes

If there are unstaged changes:

```bash
git add -A
git commit -m "feat(wpf-dev-pack): v{version} - {summary} [skip-version]"
```

If no changes exist, skip to Step 4.

## Step 4: Push to Remote

```bash
git push origin main
```

## Step 5: Create GitHub Tag

```bash
git tag wpf-dev-pack-v{version}
git push origin wpf-dev-pack-v{version}
```

## Step 6: Create GitHub Release Note

Generate changelog from previous tag:

```bash
git tag --sort=-v:refname | grep wpf-dev-pack | head -2
```

Use the second tag as the previous version, then:

```bash
git log {previous-tag}..wpf-dev-pack-v{version} --oneline
```

Create the release:

```bash
gh release create wpf-dev-pack-v{version} \
  --repo christian289/dotnet-with-claudecode \
  --title "wpf-dev-pack v{version}" \
  --notes "$(cat <<'EOF'
## What's New

- {changes from git log}

## Stats

- **Skills**: {actual count from checker}
- **Agents**: {actual count from checker}
- **MCP Servers**: {actual count from checker}
EOF
)"
```

## Step 7: Update GitHub Profile README

```bash
gh api repos/christian289/christian289/contents/README.md --jq '.content' | base64 -d > /tmp/profile-readme.md
```

Update Skills, Agents, MCP Servers counts to match actual values using `sed`, then push:

```bash
gh api repos/christian289/christian289/contents/README.md \
  --method PUT \
  --field message="chore: update wpf-dev-pack to v{version}" \
  --field content="$(base64 -w 0 /tmp/profile-readme.md)" \
  --field sha="$(gh api repos/christian289/christian289/contents/README.md --jq '.sha')"
```

## Step 8: Final Verification

Re-run the checker to confirm all issues are resolved:

```bash
dotnet ".claude/skills/wpf-dev-pack-release/scripts/VersionReleaseChecker.cs" -- --standalone
```

All checks must pass. Report the final result.
