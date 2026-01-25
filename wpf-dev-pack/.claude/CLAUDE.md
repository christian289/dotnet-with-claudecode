# WPF Dev Pack - Auto-Trigger System

Automatically activates relevant skills when WPF/C#/.NET keywords are detected.

---

## Core Rules

```
RULE 1: Detect WPF/C#/.NET keywords → Activate relevant skills
RULE 2: Delegate complex tasks to specialized agents
RULE 3: Announce skill activation (except silent triggers)
RULE 4: Select most specific skill when multiple match
```

## Trigger Priority

1. **Explicit slash command** (`/wpf-dev-pack:skill-name`) → Highest
2. **Keyword-based auto-trigger** → See `skills/.claude/CLAUDE.md`
3. **Context-based inference** → From conversation

## Trigger Behavior

**On Trigger:**
1. Announce: "WPF Dev Pack: Activating `skill-name` skill."
2. Load SKILL.md content
3. Generate/modify code per guidelines

**Silent Triggers** (no announcement):
- `formatting-wpf-csharp-code`
- `using-xaml-property-element-syntax`
- `managing-literal-strings`

**Multiple Keywords:**
1. Most specific first (e.g., "drawingcontext" > "performance")
2. Related skills can be referenced in parallel
3. Ask user if conflict
