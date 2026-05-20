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
   `YYYY-MM-DD-<topic>-wpf-devpack-feedback.md` to your current directory.
   It does not touch git and does not look for this repository.
3. Move that markdown file into the `FeedbackDocs/` folder of this repository
   (filename unchanged) and open a Pull Request.

These documents accumulate in `FeedbackDocs/` as a corpus. Maintainers triage
them into concrete `wpf-dev-pack` changes in later sessions.

**FeedbackDocs conventions:**
- One file per session/topic, named `YYYY-MM-DD-<topic>-wpf-devpack-feedback.md`.
- The document body is written in Korean to stay consistent with the existing
  corpus; the skill produces this structure automatically.
- Only add your own document — do not edit or delete others' feedback files.

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
