# Default Behavior

## 1. Response Language

- **Default response language = English**
- When responding in English: use a friendly, polite tone.
- When asked to translate into Korean: produce accurate, unambiguous,
  clear Korean.
- When asked to write in Korean: use a polite, courteous tone.

### 1.1 Priority (highest first)

1. **Current user-message language** — if the user writes in Korean,
   Japanese, Chinese, or any other language, respond in the same
   language and keep using it for the rest of the session (sticky).
2. **The `language:` field in `.claude/wpf-dev-pack.local.md`** — in
   projects where the wpf-dev-pack plugin is installed, the
   `LanguagePreferenceLoader` SessionStart hook injects this preference
   into the system context at session start. See the
   "Per-Project Language Preference" section of
   `wpf-dev-pack/.claude/CLAUDE.md` and the
   `/wpf-dev-pack:configuring-wpf-dev-pack-language` skill for details.
3. The default value defined in this rule (English).

If the user explicitly requests a switch ("respond in English" /
"한글로 답해줘"), switch immediately and keep the new language for the
rest of the session.

### 1.2 Artifact Language Policy (separate)

This rule applies to **user-facing conversational responses only**.
The following artifacts follow their own policies, which take
precedence:

- **Skill body (SKILL.md), description frontmatter, code-example
  comments**: English only. See the "Skill content language policy"
  subsection of `.claude/rules/claude-skills/best-practices.md` for
  details.
- **Korean + English bilingual comments**: conditional — applies only
  when Korean is the chosen primary language. See
  `.claude/rules/dotnet/preferences.md` §2.5 for details.

## 2. Tool Recommendations

- When you notice the user could benefit from a tool they may not be
  aware of, recommend it proactively and include the basic usage.

## 3. Knowledge Verification

- When enumerating facts, verify their accuracy as part of wrapping up
  the response. Do not assert what you have not confirmed.
