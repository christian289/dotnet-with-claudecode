# Claude Agent Skills — Authoring Guidelines

## 1. Core Principles

- **Conciseness first**: the context window is a shared resource (system
  prompt, conversation history, other skill metadata all share it).
- **Skip what Claude already knows**: ask yourself "does Claude really
  need this explanation?"
- **Set the right freedom level**:
  - High: text-based guidance (when multiple approaches are valid)
  - Medium: pseudocode with parameters (preferred pattern exists,
    variations allowed)
  - Low: specific scripts with minimized parameters (when the task is
    fragile and consistency matters)

---

## 2. SKILL.md Structure

### Full Frontmatter Reference

All fields are optional. Only `description` is recommended.

| Field | Required | Description |
|-------|----------|-------------|
| `name` | No | The skill's display name. Defaults to the directory name if omitted. Lowercase, digits, and hyphens only (max 64 characters). |
| `description` | Recommended | What the skill does and when to use it. Claude uses it to decide whether to auto-invoke. **Claude API**: max 1,024 chars. **Claude Code**: when `description` + `when_to_use` together exceed **1,536 chars**, the listing is truncated (Claude Code 2.1.105, 2026-04-13 raised this from 250 → 1,536; front-load key use cases). |
| `argument-hint` | No | Argument hint shown during autocomplete. Examples: `[issue-number]`, `[filename] [format]`. |
| `disable-model-invocation` | No | When `true`, blocks Claude from auto-invoking → user must run `/name`. Default: `false`. |
| `user-invocable` | No | When `false`, the skill is hidden from the `/` menu → only Claude can invoke. Default: `true`. |
| `allowed-tools` | No | Tools that can be used without prompts while the skill is active. |
| `model` | No | The model to use while the skill is active. |
| `effort` | No | Effort level while the skill is active (`low`, `medium`, `high`, `max`). |
| `context` | No | When set to `fork`, the skill runs in a separate subagent context. |
| `agent` | No | The subagent type to use when `context: fork`. |
| `hooks` | No | Hooks scoped to the skill's lifecycle. |
| `paths` | No | Glob patterns that restrict where the skill activates (YAML list or comma-separated string). |
| `shell` | No | The shell used for `` !`command` `` blocks (`bash` by default; `powershell` supported). |

### Invocation Control (who invokes)

Combine `disable-model-invocation` and `user-invocable` based on who
should be able to invoke the skill:

| Scenario | `disable-model-invocation` | `user-invocable` | Context Loading |
|----------|---------------------------|------------------|-----------------|
| **Both human and Claude** (default) | omit (false) | omit (true) | description always loaded; full content loaded on invocation |
| **Human only** (deploy, commit, etc.) | `true` | omit (true) | description NOT loaded; full content loads only when user runs `/name` |
| **Claude only** (background knowledge) | omit (false) | `false` | description always loaded; full content loads when Claude decides |

```yaml
# Human only: Workflows with side effects (deploy, commit, send message)
---
name: deploy
description: Deploy the application to production
disable-model-invocation: true
---

# Claude only: Background knowledge (not meaningful as a user action)
---
name: legacy-system-context
description: Explains legacy auth middleware architecture. Use when modifying auth-related code.
user-invocable: false
---

# Both: General skill (default)
---
name: explain-code
description: Explains code with visual diagrams and analogies. Use when explaining how code works.
---
```

### Argument Passing

For user-invocable skills that take arguments, set `argument-hint` and
reference them in the body via `$0`, `$1`, `$2`, etc.:

| Variable | Description |
|----------|-------------|
| `$ARGUMENTS` | All arguments (full string) |
| `$ARGUMENTS[N]` | Specific argument by 0-based index |
| `$0`, `$1`, `$2`... | Shortcuts for `$ARGUMENTS[N]` |

> ⚠️ If `$ARGUMENTS` does not appear in the body, the arguments are
> automatically appended at the end as `ARGUMENTS: <value>`.

```yaml
# Single argument example
---
name: fix-issue
description: Fix a GitHub issue
disable-model-invocation: true
argument-hint: <issue-number>
---

Fix GitHub issue $0 following our coding standards.

# Multiple arguments example
---
name: migrate-component
description: Migrate a component from one framework to another
disable-model-invocation: true
argument-hint: <ComponentName> <from-framework> <to-framework>
---

Migrate the $0 component from $1 to $2.
Preserve all existing behavior and tests.
```

**Usage examples:**

- `/fix-issue 123` → `$0` = `123`
- `/migrate-component SearchBar React Vue` → `$0` = `SearchBar`, `$1` = `React`, `$2` = `Vue`

### Description Authoring Rules

- **Third person is mandatory**: the description is injected into the
  system prompt.
  - ✅ "Processes Excel files and generates reports"
  - ❌ "I can help you process Excel files"
  - ❌ "You can use this to process Excel files"
- **Be specific**: include both what it does and when to use it.

```yaml
# Good example
description: Extract text and tables from PDF files, fill forms, merge documents. Use when working with PDF files or when the user mentions PDFs, forms, or document extraction.

# Bad example
description: Helps with documents
```

### Skill Content Language Policy

- **SKILL.md body, description frontmatter, and code-example comments
  are English-only.**
- Reason: the existing wpf-dev-pack and Avalonia skills are all
  English. International reusability and consistency matter.
- The Korean-response rule in `.claude/rules/preferences.md` applies
  **only to user-facing conversational responses**, not to skill
  content.
- The Korean + inline-English rule in
  `.claude/rules/dotnet/preferences.md` §2.5 is **conditional** —
  applies only when Korean is the chosen primary language. In an
  English-first skill code example, keep comments English-only.

---

## 3. Naming Conventions

- **Prefer gerund form** (verb + ing):
  - ✅ `processing-pdfs`, `analyzing-spreadsheets`, `managing-databases`
- **Alternatives allowed**:
  - Noun phrases: `pdf-processing`, `spreadsheet-analysis`
  - Action-oriented: `process-pdfs`, `analyze-spreadsheets`
- **Avoid**:
  - Ambiguous names: `helper`, `utils`, `tools`
  - Overly generic: `documents`, `data`, `files`
  - Reserved-feeling names: `anthropic-helper`, `claude-tools`

---

## 4. Progressive Disclosure

- SKILL.md acts as a **table of contents** (≤ 500 lines recommended).
- Move detail into separate files.
- **References must be one level deep** (SKILL.md → reference.md ✅,
  SKILL.md → advanced.md → details.md ❌).

### Example Directory Layout

```
pdf/
├── SKILL.md              # main guidance (loaded on trigger)
├── FORMS.md              # form-filling guide (loaded on demand)
├── reference.md          # API reference (loaded on demand)
├── examples.md           # usage examples (loaded on demand)
├── evals/
│   └── evals.json        # trigger evaluation data (required)
└── scripts/
    ├── analyze_form.py   # utility script (executed, not loaded)
    └── validate.py       # validation script
```

### Reference Pattern

```markdown
# SKILL.md

**Basic usage**: [inline guidance in SKILL.md]
**Advanced features**: See [advanced.md](advanced.md)
**API reference**: See [reference.md](reference.md)
**Examples**: See [examples.md](examples.md)
```

---

## 5. Workflow Patterns

### Break Complex Work Into a Checklist

```markdown
## PDF Form Filling Workflow

Copy this checklist and track your progress:

```
Task Progress:
- [ ] Step 1: Analyze the form (run analyze_form.py)
- [ ] Step 2: Build the field mapping (edit fields.json)
- [ ] Step 3: Validate the mapping (run validate_fields.py)
- [ ] Step 4: Fill the form (run fill_form.py)
- [ ] Step 5: Verify the output (run verify_output.py)
```
```

### Implement Feedback Loops

- **General pattern**: run validator → fix errors → repeat.
- State explicitly that on validation failure, the workflow goes back
  to the previous step.

---

## 6. Common Patterns

### Template Pattern

```markdown
## Report Structure

ALWAYS use this exact template structure:

```markdown
# [Analysis Title]

## Executive summary
[One-paragraph overview of key findings]

## Key findings
- Finding 1 with supporting data
```
```

### Example Pattern (input/output pairs)

```markdown
## Commit Message Format

**Example 1:**
Input: Added user authentication with JWT tokens
Output:
```
feat(auth): implement JWT-based authentication

Add login endpoint and token validation middleware
```
```

### Conditional Workflow

```markdown
## Document Modification Workflow

1. Determine the modification type:

   **Creating new content?** → Follow "Creation workflow" below
   **Editing existing content?** → Follow "Editing workflow" below
```

---

## 7. Patterns to Avoid

| Pattern | Problem | Fix |
|---------|---------|-----|
| Windows paths (`\`) | Breaks on Unix | Always use `/` |
| Time-dependent info | Becomes outdated quickly | Use an "old patterns" section |
| Too many options | Causes confusion | Give a default + one alternative |
| Deep reference nesting | Risk of partial reads | One level deep only |
| Tool-installation assumption | Execution fails | Include install steps explicitly |

### Option Presentation Example

```markdown
# Bad example (too many choices)
"You can use pypdf, or pdfplumber, or PyMuPDF, or pdf2image..."

# Good example (default + alternative)
"Use pdfplumber for text extraction:
```python
import pdfplumber
```
For scanned PDFs requiring OCR, use pdf2image with pytesseract instead."
```

---

## 8. When Embedding Executable Code

### Explicit Error Handling

```python
def process_file(path):
    """Process a file, creating it if it doesn't exist."""
    try:
        with open(path) as f:
            return f.read()
    except FileNotFoundError:
        # Create file with default content instead of failing
        print(f"File {path} not found, creating default")
        with open(path, 'w') as f:
            f.write('')
        return ''
```

### No Magic Numbers

```python
# Good example: Self-documenting
REQUEST_TIMEOUT = 30  # HTTP requests typically complete within 30 seconds
MAX_RETRIES = 3       # Three retries balances reliability vs speed

# Bad example: Magic numbers
TIMEOUT = 47  # Why 47?
```

### Verifiable Intermediate Output

- **Pattern**: produce a plan → validate the plan → execute → confirm.
- Validation scripts should return specific error messages.

---

## 9. MCP Tool References

- **Use the full tool name**: `ServerName:tool_name`.

```markdown
Use the BigQuery:bigquery_schema tool to retrieve table schemas.
Use the GitHub:create_issue tool to create issues.
```

---

## 10. Checklist

### Core Quality

- [ ] description is specific and includes key terms.
- [ ] description states what it does AND when to use it (within Claude
      API 1,024 chars / Claude Code listing 1,536 chars — CC 2.1.105+).
- [ ] SKILL.md body is within 500 lines.
- [ ] Additional details live in separate files.
- [ ] No time-dependent information (or it lives under an "old
      patterns" section).
- [ ] Terminology is consistent.
- [ ] Examples are concrete (not abstract).
- [ ] File references are one level deep.
- [ ] Progressive disclosure is used appropriately.
- [ ] The workflow has clear steps.

### Invocation Control

- [ ] Decided who invokes: human only / Claude only / both.
- [ ] Side-effect skills set `disable-model-invocation: true`.
- [ ] Background-knowledge skills set `user-invocable: false`.
- [ ] User-invocable skills with arguments set `argument-hint`.
- [ ] Argument references use the shortcut form `$0`, `$1`, `$2`...

### Code and Scripts

- [ ] Scripts solve the problem instead of offloading it to Claude.
- [ ] Error handling is explicit and helpful.
- [ ] No magic numbers (every value is justified).
- [ ] Required packages are listed in the guidance.
- [ ] No Windows paths (everything uses `/`).
- [ ] Critical operations include validation / confirmation steps.
- [ ] Quality-critical operations include a feedback loop.

---

## 11. Compaction Survival — Critical Content Goes First

### 11.1 Background: What Survives Compaction

When a Claude Code conversation grows long, `/compact` runs (manually or
automatically) and replaces the history with a summary. What does and
does not survive compaction is documented officially at
<https://code.claude.com/docs/en/context-window#what-survives-compaction>.
The key facts for skill authoring:

| Item | After compaction |
|------|------------------|
| System prompt | Re-injected |
| Auto memory (`MEMORY.md`, first 200 lines / 25KB) | Re-injected |
| `~/.claude/CLAUDE.md` and the project `CLAUDE.md` hierarchy | Re-injected |
| Environment info | Re-injected |
| MCP tool names (deferred) | Re-injected |
| **Skill description listing** | **NOT re-injected.** Only skills you actually invoked are preserved (`noSurviveCompact: true`). |
| Verbatim conversation body | Replaced by a structured summary (preserves: user requests/intent, key technical concepts, files examined/modified with important code snippets, errors and fixes, pending tasks, current work). |
| Tool outputs and intermediate reasoning | Lost |

### 11.2 Implications for Skill Authors

- Skill descriptions disappear after compaction unless the user
  explicitly invoked that skill → design descriptions as **bait for
  the first invocation**, not as ongoing reminders.
- SKILL.md body is in context when the skill is invoked, but after
  compaction it becomes part of the conversation summary. The summary
  retains **specific code snippets, file paths, and decisions
  verbatim**; long prose guidance, bottom-of-file examples, and
  verbose table bodies are dropped.

### 11.3 Critical-First Authoring Rule

When you write a skill, **put the most important content near the
top**. Higher placement gives content a better chance of surviving
verbatim in the summary or of being captured as a "key technical
concept".

#### Recommended Section Order

```
1. Frontmatter
2. (Optional) Essential / Post-Compact section ← at the very top, for
   non-negotiable rules
3. Core workflow (numbered steps)
4. Per-step detail (concise tables + direct imperative sentences)
5. Examples / case-specific patterns
6. Notes / footnotes
7. References (See [reference.md])
```

#### The `Essential (Post-Compact)` Pattern

State non-negotiable rules near the top (recommended: within the first
100 lines) in this explicit form:

```markdown
## Essential (Post-Compact)

These rules MUST survive context compression. If prior context is lost,
re-read this section.

1. <hard rule one — one line, numbered>
2. <hard rule two>
...
```

This pattern is modeled after the "Essential (Post-Compact)" section in
`wpf-dev-pack/.claude/CLAUDE.md`. The header text itself signals to the
summary that the contents should be preserved.

#### Forms to Avoid

- Putting critical rules only inside a bottom "Notes" section — high
  chance of being dropped from the summary.
- Expressing rules *only* inside example code (examples are weakly
  retained).
- Phrasing rules as flowing paragraphs — numbered/table/imperative
  forms survive better.
- Replacing rules with references to other files ("See
  `reference.md`") without restating the rule in the body — references
  are for progressive disclosure, not for delegating the rule itself.

### 11.4 Checklist (compaction-aware authoring)

- [ ] The most important hard rules appear within the first 100 lines.
- [ ] If there are non-negotiable rules, they are stated under an
      `Essential (Post-Compact)` header.
- [ ] No rule is expressed *only* in example code (each rule has its
      own imperative sentence first).
- [ ] Workflows are written as numbered steps (not prose).
- [ ] Tables do not carry the rule alone (tables are auxiliary; the
      rule is stated above the table).
- [ ] External-file references *supplement* rules instead of replacing
      them.

### 11.5 References

- Official docs: <https://code.claude.com/docs/en/context-window#what-survives-compaction>
- Reference pattern: the `Essential (Post-Compact)` section of
  `wpf-dev-pack/.claude/CLAUDE.md`.

---

## 12. Official Documentation

- [Skills Overview](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/overview)
- [Skills Quickstart](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/quickstart)
- [Skills in Claude Code](https://code.claude.com/docs/en/skills)
- [Claude Code Context Window — What survives compaction](https://code.claude.com/docs/en/context-window#what-survives-compaction)
