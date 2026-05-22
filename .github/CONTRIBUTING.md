[🇰🇷 한국어](./CONTRIBUTING.ko.md)

# Contributing to dotnet-with-claudecode

Thank you for your interest in contributing! This document explains how to contribute to this project.

## How to Contribute

### 1. Fork & Clone

```bash
# Clone after forking
git clone https://github.com/YOUR_USERNAME/dotnet-with-claudecode.git
cd dotnet-with-claudecode
```

### 2. Create a Branch

```bash
git checkout -b feature/your-feature-name
# or
git checkout -b fix/your-bug-fix
```

### 3. Commit Your Changes

```bash
git add .
git commit -m "feat: add new skill for XYZ"
```

**Commit Message Convention:**
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `refactor:` - Code refactoring
- `chore:` - Other changes

**Squash before opening the PR.** This repo prefers a small number of
meaningful commits over many step-by-step commits. Clean up your history
(squash) before requesting review.

### 4. Create a Pull Request

1. Push to your fork
2. Create a Pull Request to the original repository
3. Fill out the PR template with description

### 5. Feedback Documents (`FeedbackDocs/`)

`wpf-dev-pack` is used in real WPF projects that live **outside** this
repository. The most valuable improvements come from lessons learned during
that work — anti-patterns you had to hand-fix, skill guidance that was missing
or outdated, triggers that fired wrongly or not at all, scaffolder gaps.

Because that work happens in a foreign session where editing `wpf-dev-pack`
directly is impractical, capture the feedback as a document:

1. In your WPF working session, after using `wpf-dev-pack`, run the
   user-invocable skill `/wpf-dev-pack:collecting-wpf-dev-pack-feedback`.
2. The skill analyzes that session and writes
   `<topic>-wpf-dev-pack-feedback.md` to your current directory.
   It does not touch git and does not look for this repository.
3. Move that markdown file into the `FeedbackDocs/` folder of this repository
   (filename unchanged) and open a Pull Request.

These documents accumulate in `FeedbackDocs/` as a corpus. Maintainers triage
them into concrete `wpf-dev-pack` changes in later sessions.

**FeedbackDocs conventions:**
- One file per session/topic, named `<topic>-wpf-dev-pack-feedback.md`.
- The document body language is not constrained — Korean, English, or mixed
  is all fine. The skill produces the document structure automatically.
- Only add your own document — do not edit or delete others' feedback files.

**No personally identifying information.** A feedback document is a reusable
artifact contributed across projects, so it must describe the technical
phenomenon and its causal chain only. Do NOT include:

- Project, solution, repository, product, or codename
- Team / developer / user name, email, or account handle
- Date or time of when the issue was encountered
- Absolute or repo-relative paths from the originating codebase
- Class / namespace / member names that are unique to the originating project
  (use neutral placeholders such as `XxxView`, `XxxViewModel`, `IXxxService`)

Public framework / library / API names (`HelixToolkit`, `ScottPlot`,
`CommunityToolkit.Mvvm`, `Prism`, `DispatcherPriority.ApplicationIdle`, etc.)
are part of the technical context and are allowed.

**Self-review before opening the PR.** Re-read the document one more time
right before pushing, and verify:

- [ ] No project / solution / repository / product / codename appears
- [ ] No team / developer / user name, email, or handle appears
- [ ] No date or time of the original incident appears
- [ ] No absolute or repo-relative paths from the originating codebase appear
- [ ] All project-specific class / namespace / member names have been
      replaced with neutral placeholders
- [ ] Each item describes Phenomenon → Cause → Effect as a generic technical
      chain, not as a project incident

If any of the above fails, fix the document first; only then open the PR.

**What happens after the PR is merged.** Maintainers reflect the feedback
via the `/applying-wpf-dev-pack-feedback` skill in this repository. That
skill walks through each item, makes the plugin changes, parks the
feedback document inside `FeedbackDocs/`, and appends a row to
`FeedbackDocs/APPLIED-LOG.md` describing what was applied (or partially
applied / rejected) and in which commit. Contributors do not write to
the Applied Log directly.

## Guidelines

### When Writing Skills

- Keep `SKILL.md` under 500 lines
- Write description in third person
- Include working example code
- Follow project coding conventions

### When Writing Agents

- Define clear responsibilities
- Choose appropriate model tier (haiku/sonnet/opus)
- Minimize overlap with other agents

## Code of Conduct

Please follow our [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).

## Questions?

If you have questions, please open an [Issue](https://github.com/christian289/dotnet-with-claudecode/issues).
