# WPF Dev Pack - Auto-Trigger System

Automatically activates relevant skills when WPF/C#/.NET keywords are detected.

---

## .NET Version Configuration

### Version Selection Rules

1. **User specifies version** → Use that version with corresponding C# version
2. **No specification** → Use **latest stable .NET** (currently .NET 10)

### .NET ↔ C# Version Mapping

| .NET Version | C# Version | TargetFramework | Key Features |
|--------------|------------|-----------------|--------------|
| .NET 10 | C# 14 | `net10.0-windows` | Extensions, field keyword |
| .NET 9 | C# 13 | `net9.0-windows` | params collections, lock object |
| .NET 8 | C# 12 | `net8.0-windows` | Primary constructors, collection expressions |
| .NET 7 | C# 11 | `net7.0-windows` | Raw string literals, list patterns |
| .NET 6 | C# 10 | `net6.0-windows` | Global using, file-scoped namespace |
| .NET 5 | C# 9 | `net5.0-windows` | Records, init-only, top-level statements |
| .NET Core 3.1 | C# 8 | `netcoreapp3.1` | Nullable reference types, async streams |
| .NET Framework 4.8 | C# 7.3 | `net48` | Tuples, pattern matching, local functions |

> **Update Policy**: When new .NET version releases, add new row to this table.
> Last updated: 2026-01 (Latest stable: .NET 10)

### Code Generation Rules

When generating WPF projects or code:

```
IF user specifies ".NET X":
    Use netX.0-windows + C# version from mapping table
ELSE:
    Use latest stable .NET from mapping table (top row)
```

- Always use **maximum C# features** available for the target .NET version
- Use `Microsoft.Extensions.Hosting` matching the .NET major version
- Example: .NET 10 → `Microsoft.Extensions.Hosting` 10.x

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
