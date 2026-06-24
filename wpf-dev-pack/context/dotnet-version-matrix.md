# .NET Version Configuration

> Part of wpf-dev-pack's session core policy (injected by `CorePolicyLoader`). The
> `DotnetVersionChecker` SessionStart hook separately verifies that the installed
> .NET SDK meets the hooks' minimum (10.0.300+); this document governs which
> TargetFramework / C# version to generate for project and code output.

## Version Selection Rules

1. **Minimum supported version**: **.NET 8** (C# 12).
2. **User specifies a version** → use that version with its corresponding C# version.
3. **No specification** → use the **latest stable .NET** (currently .NET 10).

## .NET ↔ C# Version Mapping

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

> **Update policy**: when a new .NET version releases, add a new row to this table.
> Last updated: 2026-01 (latest stable: .NET 10).

## Code Generation Rules

When generating WPF projects or code:

```
IF user specifies ".NET X":
    Use netX.0-windows + C# version from the mapping table
ELSE:
    Use the latest stable .NET from the mapping table (top row)
```

- Always use the **maximum C# features** available for the target .NET version.
- Use `Microsoft.Extensions.Hosting` matching the .NET major version.
- Example: .NET 10 → `Microsoft.Extensions.Hosting` 10.x.
