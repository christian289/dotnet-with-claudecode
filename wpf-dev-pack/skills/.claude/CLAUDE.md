# WPF Dev Pack Skills — Routing

Knowledge topics are NO LONGER plugin skills. They live in
`wpf-dev-pack/knowledge/<id>/` and are served by the `WpfDevPackMcp` MCP
server. The `WpfKeywordDetector` UserPromptSubmit hook owns the
keyword → topic-id routing table (single source of truth, zero
always-loaded context). On a WPF knowledge keyword it emits
`WpfDevPackMcp get_wpf_topic("<id>")`.

Only command skills remain under `skills/` and are slash-invocable:

| Keyword (intent) | Command skill |
|---|---|
| `create viewmodel`, `뷰모델 생성` | `make-wpf-viewmodel` |
| `create service`, `서비스 생성` | `make-wpf-service` |
| (explicit) | `make-wpf-project`, `make-wpf-custom-control`, `make-wpf-usercontrol`, `make-wpf-converter`, `make-wpf-behavior` |
| `feedback` (maintainer) | `collecting-wpf-dev-pack-feedback` |
| `language` | `configuring-wpf-dev-pack-language` |
| `repo path`, MCP unconfigured | `set-repo-path` |

To add a knowledge topic: create `wpf-dev-pack/knowledge/<id>/TOPIC.md`
(NO frontmatter — first `# H1` is the title; put a one-line `> summary`
blockquote directly under the H1; the MCP catalog reads both from the body)
and add its keyword(s) to `hooks/WpfKeywordDetector.cs`. No plugin skill,
no version bump, no MCP rebuild — the server picks it up on next pull.

---

### HandMirror MCP - .NET API Verification

When querying .NET API/NuGet package information, **also use HandMirrorMcp tools** to reduce hallucinations.

**Trigger condition**: When using context7 or Microsoft Learn MCP for .NET/NuGet related queries

**Co-usage rules:**

```
WHEN using context7 or Microsoft Learn for .NET/NuGet info:
  ALSO use HandMirrorMcp to verify:
    - inspect_nuget_package: List namespaces/types in a NuGet package
    - inspect_nuget_package_type: Get exact method signatures
    - search_nuget_packages: Search packages by keyword
    - get_type_info: Inspect local assembly (.dll/.exe) types
    - explain_build_error: Diagnose CS/NU build errors
    - analyze_csproj: Analyze project file for issues
```

**Usage scenarios:**
- Verify API name casing accuracy in NuGet packages (e.g., SQLite vs Sqlite)
- Identify correct namespaces for extension methods
- Check API breaking changes across package versions
- Diagnose build errors (CS0246, NU1605, etc.) and recommend required packages
