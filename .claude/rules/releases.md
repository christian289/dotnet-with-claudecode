# GitHub Releases Convention

## Core Principle

- **`wpf-dev-pack` is the flagship product of this repository.** Its
  latest release must always be the one shown on the repo's main-page
  "Releases" sidebar widget.
- GitHub picks that widget's release by the **"Latest" flag**, which
  defaults to the most recently published non-prerelease, non-draft
  release. It is **not** determined by tag naming or release title —
  a differently-prefixed tag (e.g. `polylab3dstudio-v0.1.0` vs.
  `wpf-dev-pack-v1.8.1`) does not protect `wpf-dev-pack` from being
  displaced.

## Rule

- **Any release for anything other than `wpf-dev-pack`** (samples
  under `samples/`, the `mcp/` server if ever GitHub-released instead
  of only NuGet-published, or any future project in this repo) **must
  be created with `--prerelease`**:

  ```bash
  gh release create <tag> <assets...> --prerelease \
    --title "..." --notes "..."
  ```

  A pre-release is excluded from "Latest" eligibility, so it cannot
  displace `wpf-dev-pack` from the repo's main page.

- `wpf-dev-pack` releases are created normally (no `--prerelease`) via
  the `/wpf-dev-pack-release` skill, and each new one automatically
  becomes "Latest" again since it is newer than any pre-release.

## Recovery (if a non-wpf-dev-pack release was already published without `--prerelease`)

```bash
gh release edit <wpf-dev-pack-tag> --latest
gh release edit <the-other-tag> --prerelease
```

Verify with `gh release list` — the `wpf-dev-pack` row must show
`Latest`.

## `gh` CLI Account Caution

- This repo may have multiple `gh` accounts logged in, and the
  **active** account is not necessarily the repo owner
  (`christian289`). `gh release create`/`edit` can fail with a
  misleading error (e.g. "workflow scope may be required") when the
  wrong account is active.
- If a release operation fails unexpectedly: run `gh auth status`,
  then `gh auth switch --user christian289` before retrying.

## Background

On 2026-07-21, publishing `samples/PolyLab3DStudio` as
`polylab3dstudio-v0.1.0` (a normal release) silently demoted
`wpf-dev-pack-v1.8.1` from the "Latest" badge. Fixed via the recovery
steps above; this rule exists so it does not recur.
