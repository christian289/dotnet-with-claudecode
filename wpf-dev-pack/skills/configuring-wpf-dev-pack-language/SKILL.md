---
description: "Configures the user-facing response language for the wpf-dev-pack plugin by writing a per-project preference file at .claude/wpf-dev-pack.local.md. Use when starting work with wpf-dev-pack in a new project and the default language does not match the user's expectation, or when switching language mid-project. The setting is read by the LanguagePreferenceLoader SessionStart hook on the next conversation start and takes effect automatically from then on."
argument-hint: "[language-code]"
disable-model-invocation: true
---

# Configuring wpf-dev-pack Language

This skill writes (or updates) a per-project preference file that tells
the wpf-dev-pack plugin which language to respond in. A SessionStart
hook (`hooks/LanguagePreferenceLoader.cs`) reads the file on every new
conversation and emits a directive into the system context, so the
preference takes effect automatically.

## When to use

- The default language of a response does not match what the user wants
  (typical case: an English-speaking contributor in a project where
  wpf-dev-pack was originally tuned for Korean responses, or vice versa).
- The user wants to switch language mid-project.

## When NOT to use

- The user only wants a one-off language switch for the next response —
  just have them phrase the next message in the target language. Do not
  write a file for a one-shot.
- The wpf-dev-pack plugin is not installed in the current project — the
  SessionStart hook will not run without the plugin.

## Workflow

Copy this checklist and track progress:

```
Configure Progress:
- [ ] Step 1: Detect current state of .claude/wpf-dev-pack.local.md
- [ ] Step 2: Resolve target language (argument or AskUserQuestion)
- [ ] Step 3: Write or update .claude/wpf-dev-pack.local.md
- [ ] Step 4: Confirm and explain when the change takes effect
```

### Step 1 — Detect current state

Check whether `.claude/wpf-dev-pack.local.md` exists in the current
project root. If it does, read its YAML frontmatter and find any
existing `language:` value so the user can see what they are replacing.

If the file does not exist, that is fine — Step 3 will create it.

### Step 2 — Resolve target language

If `$0` is provided, treat it as a [BCP-47](https://www.rfc-editor.org/info/bcp47)
language code (the IETF standard built on ISO 639 language codes):

| `$0` value | Meaning |
|---|---|
| `ko`, `ko-KR` | Korean |
| `en`, `en-US`, `en-GB` | English |
| `ja`, `ja-JP` | Japanese |
| `zh`, `zh-CN`, `zh-TW` | Chinese |
| (other) | Pass through — the hook will display the raw code |

The table above is illustrative, not exhaustive. Claude understands any
valid BCP-47 / ISO 639-1 code (Claude Code does not maintain a closed
allowlist). Authoritative references for code samples:

- BCP-47 specification (the IETF standard):
  <https://www.rfc-editor.org/info/bcp47>
- IANA Language Subtag Registry (the canonical list of subtags):
  <https://www.iana.org/assignments/language-subtag-registry/language-subtag-registry>
- ISO 639-1 two-letter codes (quick reference):
  <https://en.wikipedia.org/wiki/List_of_ISO_639_language_codes>
- W3C i18n introduction to BCP-47 (gentler reading):
  <https://www.w3.org/International/articles/language-tags/>

Accept the value if it is a non-empty short token (letters, digits,
hyphen). Reject and ask for clarification otherwise.

If `$0` is missing, use `AskUserQuestion` with up to 4 options
(typically Korean / English / Japanese / Chinese) plus an `Other`
escape for free-form input. Surface the *currently configured* value
from Step 1 so the user can see what they are replacing.

### Step 3 — Write or update the file

If the file does not exist, create `.claude/wpf-dev-pack.local.md`
with this minimal structure:

```markdown
---
language: <code>
---

# wpf-dev-pack — Per-Project Local Settings

This file stores personal, per-project preferences for the wpf-dev-pack
plugin. It is read by the LanguagePreferenceLoader SessionStart hook on
every new conversation in this project.

This file is personal and should not be committed. The repo's
.gitignore covers `.claude/*.local.md`.
```

If the file already exists, only modify the `language:` field in its
YAML frontmatter — preserve all other settings and the body verbatim.
If the frontmatter is missing entirely, prepend one with just the
`language:` field; do not touch the existing body.

### Step 4 — Confirm

Tell the user:

1. Which file was written and which `language:` value it now contains.
2. **The change does not affect the current conversation.** The
   SessionStart hook runs only at the start of a new session, so the
   user needs to begin a new conversation for the directive to be
   injected into the system context.
3. To revert to the default behavior, delete the file or remove its
   `language:` field — the hook will then emit nothing and the
   plugin's default language behavior applies.

## Notes

- The SessionStart hook adds a *recommendation* to the system context;
  it does not hard-switch the model. Claude follows the recommendation
  by default, and the user can still override at any time
  ("respond in English please" / "한글로 답해줘").
- This skill is intentionally NOT auto-triggered. Changing the
  conversation language is a meaningful side effect, so it must be
  invoked explicitly.
- The mechanism is wpf-dev-pack scoped. Other plugins or workflows in
  the same project are unaffected.
