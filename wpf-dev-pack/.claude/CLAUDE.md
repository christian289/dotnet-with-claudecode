# WPF Dev Pack - Auto-Trigger System

Automatically activates relevant skills when WPF/C#/.NET keywords are detected.

---

## Required Plugin Dependencies

All agents require these Claude Code plugins to be installed:

| Plugin | MCP Server | Purpose |
|--------|-----------|---------|
| **context7** | context7 | Library/framework documentation |
| **serena** | serena | Semantic code analysis, symbol navigation |
| **microsoft-docs** | microsoft-learn | Official Microsoft documentation |
| **csharp-lsp** | csharp | C# LSP code intelligence |

---

## MVVM Approach: View First

wpf-dev-pack adopts **View First MVVM**.

- View is created first and determines its own ViewModel.
- View-ViewModel wiring strategy depends on the MVVM framework:
  - CommunityToolkit.Mvvm → `rules/view-viewmodel-wiring-communitytoolkit.md`
  - Prism 9 → `rules/view-viewmodel-wiring-prism.md`
- See `rules/prohibitions.md` for banned alternatives (ViewModelLocator, etc.).

---

## Essential (Post-Compact)

These rules MUST survive context compression. If prior context is lost, re-read this section:

1. **No ViewModelLocator** — Use DI + DataTemplate mapping only (`rules/prohibitions.md`)
2. **No System.Windows in ViewModel** — BCL types only (`rules/mvvm-constraints.md`)
3. **Freeze all Freezable objects** — Brush, Pen, Geometry (`rules/freezable-performance.md`)
4. **Generic.xaml = MergedDictionaries hub only** (`rules/resourcedictionary-patterns.md`)
5. **Verify API signatures with HandMirror before writing code**
6. **View First MVVM** — See `rules/` for framework-specific wiring

---

## .NET Version Configuration

### Version Selection Rules

1. **Minimum supported version**: **.NET 8** (C# 12)
2. **User specifies version** → Use that version with corresponding C# version
3. **No specification** → Use **latest stable .NET** (currently .NET 10)

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
RULE 5: wpf-architect MUST conduct Requirements Interview before analysis
```

---

## Requirements Interview System

When `wpf-architect` is invoked, conduct an **adaptive path-based interview** using AskUserQuestion:

| Path | Task Type | Steps | Focus |
|------|-----------|-------|-------|
| **A** | Create new project | 7 | 컨셉 → 아키텍처 → 규모 → 복잡도 → 라이브러리 → 기능 영역 |
| **B** | Analyze/improve | 5 | 분석 목표 → 분석 모드 → 범위 → 출력 형식 |
| **C** | Implement feature | 5 | 기능 설명 → 구현 방식 → 라이브러리 → 기능 영역 |
| **D** | Debug/fix | 4 | 문제 증상 → 문제 유형 → 문제 영역 |

**Keyword Analysis**: 자유 입력 단계(A-2, B-2, C-2, D-2)에서 키워드를 감지하여 후속 단계 기본값 자동 설정.

See `agents/wpf-architect.md` for full interview specification.

## Trigger Priority

1. **Explicit slash command** (`/wpf-dev-pack:skill-name`) → Highest
2. **Keyword-based auto-trigger** → See `skills/.claude/CLAUDE.md`
3. **Context-based inference** → From conversation

## Trigger Behavior

**On Trigger:**
1. Announce: "WPF Dev Pack: Activating `skill-name` skill."
2. Check `.claude/rules/dotnet/wpf/mvvm-framework.md` for active MVVM framework
3. Load appropriate file:
   - **CommunityToolkit.Mvvm** → SKILL.md
   - **Prism 9** → PRISM.md (있을 경우), 없으면 SKILL.md
4. Generate/modify code per guidelines and active framework rules

**Silent Triggers** (no announcement):
- `formatting-wpf-csharp-code`
- `using-xaml-property-element-syntax`
- `managing-literal-strings`

**Multiple Keywords:**
1. Most specific first (e.g., "drawingcontext" > "performance")
2. Related skills can be referenced in parallel
3. Ask user if conflict

---

## Adding a New Skill — Required Co-updates

새 skill을 `skills/<skill-name>/SKILL.md`로 추가할 때 반드시 함께 업데이트할 항목:

1. **`skills/.claude/CLAUDE.md`** — Keyword-Skill Mapping 표에 키워드 행 추가, Skill Category Index의 해당 카테고리에 skill 이름 추가
2. **연관된 기존 SKILL.md** — 토픽이 겹치는 기존 skill에 새 skill로의 cross-link(See [...](../skill-name/SKILL.md)) 추가
3. **Prism 9 분기가 필요한 skill** — `PRISM.md` 컴패니언 파일 작성 (mvvm-framework.md 규칙 참조)
4. **Foundation + Application 쌍 skill** — 두 skill을 별도로 만들고 상호 참조. Foundation skill은 메커니즘·일반 원칙, Application skill은 구체 시나리오 적용 (예: `preventing-dispatcher-deadlock` + `shutting-down-wpf-gracefully`)
