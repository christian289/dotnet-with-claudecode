# Claude Rules — Authoring Guidelines

## Core Principles

- **Modularity**: one topic per file.
- **Clarity**: concrete, actionable instructions.
- **Hierarchy**: group related rules under subdirectories.
- **Conditional application**: when appropriate, use the `paths` field
  to restrict the rule's scope.

---

## 1. Memory Hierarchy

| Priority | Location | Purpose | Scope |
|----------|----------|---------|-------|
| 1 (highest) | `./CLAUDE.local.md` | Personal project settings | Personal (gitignored) |
| 2 | `./.claude/rules/*.md` | Modularized rules | Team-shared |
| 3 | `./.claude/CLAUDE.md` | Project main configuration | Team-shared |
| 4 | `./CLAUDE.md` | Project root configuration | Team-shared |
| 5 | `~/.claude/rules/*.md` | Personal global rules | Personal |
| 6 (lowest) | `~/.claude/CLAUDE.md` | Personal global configuration | Personal |

**Rule:** lower-level settings override higher-level ones.

---

## 2. Directory Structure

### 2.1 Recommended Layout

```
your-project/
├── .claude/
│   ├── CLAUDE.md              # project main configuration
│   ├── rules/
│   │   ├── code-style.md      # code style
│   │   ├── testing.md         # testing rules
│   │   ├── security.md        # security rules
│   │   ├── frontend/          # frontend rules
│   │   │   ├── react.md
│   │   │   └── styles.md
│   │   └── backend/           # backend rules
│   │       ├── api.md
│   │       └── database.md
│   └── skills/                # Agent Skills
│       └── my-skill/
│           └── SKILL.md
└── CLAUDE.local.md            # personal settings (gitignore)
```

### 2.2 Automatic File Loading

- All `.md` files under `.claude/rules/` are loaded recursively.
- Subdirectories are traversed automatically.
- Symbolic links are supported (cycles are detected automatically).

---

## 3. Writing a Rule File

### 3.1 Basic Form

```markdown
# [Topic] Guidelines

## 1. Section Name

- Concrete instruction 1
- Concrete instruction 2

## 2. Next Section

- Related instruction
```

### 3.2 Conditional Rules (Path-Specific)

Use YAML frontmatter to scope a rule:

```markdown
---
paths:
  - "src/api/**/*.ts"
  - "src/controllers/**/*.ts"
---

# API Development Rules

- Input validation is required on every endpoint.
- Use the standard error response format.
- Include OpenAPI documentation comments.
```

**Note:** Without a `paths` field the rule applies to all files.

### 3.3 Glob Patterns

| Pattern | Meaning |
|---------|---------|
| `**/*.ts` | All TypeScript files |
| `src/**/*` | Every file under `src` |
| `*.md` | Markdown files in the root |
| `src/components/*.tsx` | Files in a specific directory |
| `**/*.{ts,tsx}` | Multiple extensions |
| `{src,lib}/**/*.ts` | Multiple base directories |

---

## 4. Import Syntax

### 4.1 File References

Reference other files via `@path/to/file`:

```markdown
# Project Configuration

For a project overview, see @README.md.
For available commands, see @package.json.

## Additional Instructions

- Git workflow: @docs/git-instructions.md
- Personal settings: @~/.claude/my-preferences.md
```

### 4.2 Import Rules

- Both relative and absolute paths are supported.
- Imports inside code blocks are NOT evaluated.
- Recursive imports are supported (max depth: 5).
- Use the `/memory` command to inspect currently loaded memory.

---

## 5. Authoring Best Practices

### 5.1 DO

```markdown
# Code Style Guidelines

## 1. Indentation

- Use two spaces.
- Do not use tab characters.

## 2. Naming

- Variables: camelCase
- Constants: UPPER_SNAKE_CASE
- Classes: PascalCase
```

### 5.2 DON'T

```markdown
# Guidelines

Write code well.
Match the formatting.
```

**Why this is bad:** vague, non-actionable.

---

## 6. File Naming Convention

### 6.1 Recommended Pattern

| Filename | Purpose |
|----------|---------|
| `code-style.md` | Code style rules |
| `testing.md` | Testing rules |
| `security.md` | Security rules |
| `api-design.md` | API design rules |
| `preferences.md` | Personal preferences |

### 6.2 Naming Rules

- Use lowercase and hyphens: `code-style.md`.
- Use a name that describes the contents.
- Use the singular form for general rules: `testing.md`, `security.md`.

---

## 7. Using Symbolic Links

### 7.1 Shared Rules

```bash
# Link a shared rules directory
ln -s ~/shared-claude-rules .claude/rules/shared

# Link a single shared file
ln -s ~/company-standards/security.md .claude/rules/security.md
```

### 7.2 Use Cases

- Share organization-wide standard rules.
- Reuse common rules across multiple projects.
- Bring personal rules into a specific project.

---

## 8. Parent-Directory Inheritance

### 8.1 How It Works

```
parent-folder/
├── CLAUDE.md          ← loaded first
└── child-project/
    ├── CLAUDE.md      ← loaded later (overrides)
    └── .claude/
        └── rules/     ← highest priority
```

### 8.2 Example

```
dotnet-with-claudecode/       # contains .claude/ configuration
├── .claude/
│   └── rules/
└── repos/                    # child projects
    ├── project-a/            # inherits the parent .claude/
    └── project-b/            # inherits the parent .claude/
```

---

## 9. Useful Commands

| Command | Description |
|---------|-------------|
| `/init` | Initialize CLAUDE.md |
| `/memory` | Inspect and edit currently loaded memory |

---

## 10. Checklist

### When Writing a Rule File

- [ ] One topic per file.
- [ ] The filename describes the contents.
- [ ] Instructions are concrete and actionable.
- [ ] `paths` frontmatter is used only when appropriate.
- [ ] Related rules are grouped under subdirectories.
- [ ] Structure uses bullet points.
- [ ] Example code is included when useful.

### When Configuring a Project

- [ ] Design the `.claude/rules/` directory layout.
- [ ] Separate shared rules from personal rules.
- [ ] Add `CLAUDE.local.md` to `.gitignore`.
- [ ] Decide how rules are shared with the team.

---

## 11. References

- [Claude Code Memory Documentation](https://code.claude.com/docs/en/memory)
- [Modular Rules Guide](https://code.claude.com/docs/en/memory#modular-rules-with-claude/rules/)
