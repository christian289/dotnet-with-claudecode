# WpfDevPackMcp 서버 구현 계획 (Plan 1 / 2)

> **Amendment (2026-06-04 follow-up):** 지식 토픽 메인 파일이 `SKILL.md` → `TOPIC.md`로 변경되었으며 YAML frontmatter가 제거되었습니다. 요약은 본문의 첫 번째 `>` 블록인용에 위치합니다. 서버의 카탈로그 리더는 `FrontmatterReader` → `TopicDocReader`로 교체되었고, 제목은 첫 번째 `# H1`에서, 요약은 첫 번째 `>` 블록인용에서 읽습니다. 이 계획 본문의 `SKILL.md` 참조는 역사적 기록으로 보존되며 실제 파일은 `TOPIC.md`입니다.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
>
> **커밋 정책(메인테이너 선호):** 각 Task 끝의 commit 스텝은 **논리적 체크포인트**다. 레포 선호상 실행 중 step별 커밋을 남발하지 말고, 작업 끝에 의미 있는 소수 커밋으로 **squash**하라. **push는 명시 요청 시에만.**

**Goal:** `christian289/dotnet-with-claudecode` 로컬 클론의 `wpf-dev-pack/knowledge/**` 마크다운을 list/search/get 해주는 .NET 10 stdio MCP 서버(`WpfDevPackMcp`)를 만든다. 콘텐츠는 임베드하지 않고 로컬 파일시스템에서 읽으며, 필요 시 `git pull` 로 갱신한다.

**Architecture:** `Host.CreateApplicationBuilder` + `ModelContextProtocol` SDK stdio 서버. 경로 해석(env > config.json) → git 갱신(TTL, best-effort) → 지식 디렉터리 스캔(카탈로그) → 4개 MCP 도구. 단일 프로젝트, EmbeddedResource 없음, dnx/NuGet tool 배포.

**Tech Stack:** .NET 10 (C# 14), `ModelContextProtocol`, `Microsoft.Extensions.Hosting`, xUnit. stdout=JSON-RPC 전용, 로그=stderr.

> 본 계획은 스펙 `docs/superpowers/specs/2026-06-04-wpf-dev-pack-knowledge-mcp-design.md` 의 §5.A 를 구현한다. 콘텐츠 마이그레이션·플러그인 재배선·문서/`.claude.ko`·피드백 워크플로우는 **Plan 2** 에서 다룬다.

---

## 파일 구조

소스 루트: `mcp/`

| 파일 | 책임 |
|------|------|
| `WpfDevPackMcp.csproj` | net10.0 Exe, PackAsTool, 패키지 참조 |
| `GlobalUsings.cs` | BCL/NuGet global using 통합(레포 규칙) |
| `Program.cs` | 호스트·DI·MCP 서버 구성, 시작 시 갱신 트리거 |
| `Configuration/RepoConfig.cs` | `record` {RepoPath, Branch}, `ResolvedRepo` {Path, Branch, Managed} |
| `Configuration/ConfigStore.cs` | env>config.json 해석, state.json(lastPullUtc/managed) 읽기·쓰기 |
| `Git/GitRunner.cs` | `git` 프로세스 실행 래퍼(best-effort) |
| `Git/RepoRefresher.cs` | clone/pull 결정·실행(TTL, managed vs working-copy) |
| `Knowledge/Topic.cs` | `record` {Id, Title, Summary, Companions} |
| `Knowledge/FrontmatterReader.cs` | SKILL.md frontmatter `description` 추출 |
| `Knowledge/TopicCatalog.cs` | knowledge 디렉터리 스캔·캐시·get·search |
| `Tools/WpfTopicTools.cs` | `[McpServerToolType]` 4개 도구 |

테스트 루트: `mcp/WpfDevPackMcp.Tests/`

| 파일 | 대상 |
|------|------|
| `WpfDevPackMcp.Tests.csproj` | xUnit 테스트 프로젝트 |
| `FrontmatterReaderTests.cs` | frontmatter 파싱 |
| `TopicCatalogTests.cs` | 스캔·get·search (임시 fixture 디렉터리) |
| `ConfigStoreTests.cs` | 경로 해석 우선순위 |
| `RepoRefresherTests.cs` | TTL 갱신 결정(순수 함수) |

> 코드 스타일(레포 규칙): file-scoped namespace, 1파일 1타입, `sealed`, primary constructor, 최신 C#, `GlobalUsings.cs`. 주석은 영문.

---

## Task 1: 프로젝트 스캐폴딩

**Files:**
- Create: `mcp/WpfDevPackMcp.csproj`
- Create: `mcp/GlobalUsings.cs`
- Create: `mcp/WpfDevPackMcp.Tests/WpfDevPackMcp.Tests.csproj`

- [ ] **Step 1: 서버 csproj 작성**

`mcp/WpfDevPackMcp.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <AssemblyName>WpfDevPackMcp</AssemblyName>
    <RootNamespace>WpfDevPackMcp</RootNamespace>
    <!-- dnx (NuGet tool) 실행 대상 -->
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>WpfDevPackMcp</ToolCommandName>
    <PackageId>WpfDevPackMcp</PackageId>
    <Version>0.1.0</Version>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ModelContextProtocol" Version="0.4.*" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.*" />
  </ItemGroup>

</Project>
```

> `ModelContextProtocol` 버전은 구현 시 `dotnet add package ModelContextProtocol --prerelease` 결과의 최신 안정/프리릴리스로 핀한다. `0.4.*` 는 시작점.

- [ ] **Step 2: GlobalUsings 작성**

`mcp/GlobalUsings.cs`:
```csharp
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
```

- [ ] **Step 3: 테스트 csproj 작성**

`mcp/WpfDevPackMcp.Tests/WpfDevPackMcp.Tests.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\WpfDevPackMcp.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 4: 빌드 검증**

Run: `dotnet build mcp/WpfDevPackMcp.Tests/WpfDevPackMcp.Tests.csproj`
Expected: 빌드 성공(타입 없음 경고만, 에러 0).

- [ ] **Step 5: 커밋(체크포인트)**

```bash
git add mcp/
git commit -m "chore(wpf-dev-pack): scaffold WpfDevPackMcp server + test projects"
```

---

## Task 2: FrontmatterReader (TDD)

frontmatter의 `description:` 한 줄(따옴표/블록 스칼라 포함)을 추출한다. 전체 YAML 파서는 불필요(YAGNI).

**Files:**
- Create: `mcp/Knowledge/FrontmatterReader.cs`
- Test: `mcp/WpfDevPackMcp.Tests/FrontmatterReaderTests.cs`

- [ ] **Step 1: 실패 테스트 작성**

`WpfDevPackMcp.Tests/FrontmatterReaderTests.cs`:
```csharp
using WpfDevPackMcp.Knowledge;
using Xunit;

namespace WpfDevPackMcp.Tests;

public sealed class FrontmatterReaderTests
{
    [Fact]
    public void ReadDescription_DoubleQuoted_ReturnsUnquoted()
    {
        const string md = "---\ndescription: \"Hello world.\"\nuser-invocable: false\n---\n# Body\n";
        Assert.Equal("Hello world.", FrontmatterReader.ReadDescription(md));
    }

    [Fact]
    public void ReadDescription_Unquoted_ReturnsTrimmed()
    {
        const string md = "---\ndescription: Plain text here\n---\n";
        Assert.Equal("Plain text here", FrontmatterReader.ReadDescription(md));
    }

    [Fact]
    public void ReadDescription_NoFrontmatter_ReturnsNull()
    {
        Assert.Null(FrontmatterReader.ReadDescription("# Just a heading\n"));
    }

    [Fact]
    public void ReadFirstHeading_ReturnsH1Text()
    {
        const string md = "---\ndescription: x\n---\n\n# CommunityToolkit.Mvvm Guidelines\n";
        Assert.Equal("CommunityToolkit.Mvvm Guidelines", FrontmatterReader.ReadFirstHeading(md));
    }
}
```

- [ ] **Step 2: 실패 확인**

Run: `dotnet test mcp/WpfDevPackMcp.Tests --filter FrontmatterReaderTests`
Expected: FAIL — `FrontmatterReader` 미정의.

- [ ] **Step 3: 구현**

`mcp/Knowledge/FrontmatterReader.cs`:
```csharp
namespace WpfDevPackMcp.Knowledge;

/// <summary>
/// Minimal reader for the single `description:` line in a SKILL.md YAML
/// frontmatter block and the first markdown H1. Avoids a full YAML parser.
/// </summary>
public static class FrontmatterReader
{
    public static string? ReadDescription(string markdown)
    {
        if (!TryGetFrontmatter(markdown, out var fm))
        {
            return null;
        }

        foreach (var raw in fm.Split('\n'))
        {
            var line = raw.TrimEnd('\r');
            if (!line.StartsWith("description:", StringComparison.Ordinal))
            {
                continue;
            }

            var value = line["description:".Length..].Trim();
            // Block scalar (`>` or `|`): description continues on next lines;
            // for catalog summary purposes the marker line is empty — fall back to heading.
            if (value is ">" or "|")
            {
                return null;
            }

            return Unquote(value);
        }

        return null;
    }

    public static string? ReadFirstHeading(string markdown)
    {
        foreach (var raw in markdown.Split('\n'))
        {
            var line = raw.TrimEnd('\r');
            if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                return line[2..].Trim();
            }
        }

        return null;
    }

    private static bool TryGetFrontmatter(string markdown, out string frontmatter)
    {
        frontmatter = string.Empty;
        if (!markdown.StartsWith("---", StringComparison.Ordinal))
        {
            return false;
        }

        var end = markdown.IndexOf("\n---", 3, StringComparison.Ordinal);
        if (end < 0)
        {
            return false;
        }

        frontmatter = markdown[3..end];
        return true;
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2 &&
            ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
        {
            return value[1..^1];
        }

        return value;
    }
}
```

- [ ] **Step 4: 통과 확인**

Run: `dotnet test mcp/WpfDevPackMcp.Tests --filter FrontmatterReaderTests`
Expected: PASS (4 passed).

- [ ] **Step 5: 커밋(체크포인트)**

```bash
git add mcp/Knowledge/FrontmatterReader.cs mcp/WpfDevPackMcp.Tests/FrontmatterReaderTests.cs
git commit -m "feat(wpf-dev-pack): add FrontmatterReader for SKILL.md description/heading"
```

---

## Task 3: Topic 모델 + TopicCatalog 스캔 (TDD)

**Files:**
- Create: `mcp/Knowledge/Topic.cs`
- Create: `mcp/Knowledge/TopicCatalog.cs`
- Test: `mcp/WpfDevPackMcp.Tests/TopicCatalogTests.cs`

- [ ] **Step 1: Topic 모델 작성**

`mcp/Knowledge/Topic.cs`:
```csharp
namespace WpfDevPackMcp.Knowledge;

/// <summary>One knowledge topic = one directory under wpf-dev-pack/knowledge/.</summary>
public sealed record Topic(
    string Id,
    string Title,
    string Summary,
    IReadOnlyList<string> Companions);
```

- [ ] **Step 2: 실패 테스트 작성**

`WpfDevPackMcp.Tests/TopicCatalogTests.cs`:
```csharp
using WpfDevPackMcp.Knowledge;
using Xunit;

namespace WpfDevPackMcp.Tests;

public sealed class TopicCatalogTests : IDisposable
{
    private readonly string _root;          // repo root
    private readonly string _knowledge;     // <root>/wpf-dev-pack/knowledge

    public TopicCatalogTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "wdpmcp-" + Guid.NewGuid().ToString("N"));
        _knowledge = Path.Combine(_root, "wpf-dev-pack", "knowledge");
        WriteTopic("implementing-communitytoolkit-mvvm",
            "---\ndescription: \"Implements MVVM using CommunityToolkit.\"\n---\n# CommunityToolkit MVVM\nbody",
            prism: "# Prism variant\n");
        WriteTopic("virtualizing-wpf-ui",
            "---\ndescription: Implements WPF UI virtualization for large data sets.\n---\n# Virtualizing\nbody");
    }

    private void WriteTopic(string id, string skill, string? prism = null)
    {
        var dir = Path.Combine(_knowledge, id);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "SKILL.md"), skill);
        if (prism is not null)
        {
            File.WriteAllText(Path.Combine(dir, "PRISM.md"), prism);
        }
    }

    [Fact]
    public void List_ReturnsAllTopics_SortedById()
    {
        var catalog = new TopicCatalog(_root);
        var ids = catalog.List().Select(t => t.Id).ToArray();
        Assert.Equal(["implementing-communitytoolkit-mvvm", "virtualizing-wpf-ui"], ids);
    }

    [Fact]
    public void List_PopulatesSummaryFromDescription_AndCompanions()
    {
        var catalog = new TopicCatalog(_root);
        var mvvm = catalog.List().Single(t => t.Id == "implementing-communitytoolkit-mvvm");
        Assert.Equal("Implements MVVM using CommunityToolkit.", mvvm.Summary);
        Assert.Contains("PRISM.md", mvvm.Companions);
    }

    [Fact]
    public void GetContent_Default_ReturnsSkillMd()
    {
        var catalog = new TopicCatalog(_root);
        var content = catalog.GetContent("virtualizing-wpf-ui", "default");
        Assert.Contains("# Virtualizing", content);
    }

    [Fact]
    public void GetContent_Prism_ReturnsPrismMd()
    {
        var catalog = new TopicCatalog(_root);
        var content = catalog.GetContent("implementing-communitytoolkit-mvvm", "prism");
        Assert.Contains("# Prism variant", content);
    }

    [Fact]
    public void GetContent_UnknownId_Throws()
        => Assert.Throws<TopicNotFoundException>(() => new TopicCatalog(_root).GetContent("nope", "default"));

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
```

- [ ] **Step 3: 실패 확인**

Run: `dotnet test mcp/WpfDevPackMcp.Tests --filter TopicCatalogTests`
Expected: FAIL — `TopicCatalog`/`TopicNotFoundException` 미정의.

- [ ] **Step 4: 구현**

`mcp/Knowledge/TopicCatalog.cs`:
```csharp
namespace WpfDevPackMcp.Knowledge;

public sealed class TopicNotFoundException(string id)
    : Exception($"Unknown WPF topic id: '{id}'.")
{
    public string Id { get; } = id;
}

/// <summary>
/// Scans &lt;repoRoot&gt;/wpf-dev-pack/knowledge/*/SKILL.md into a topic
/// catalog. Reads directly from the local filesystem (a git clone), so a
/// rescan reflects the latest pulled content. Results are cached until
/// <see cref="Invalidate"/> is called.
/// </summary>
public sealed class TopicCatalog(string repoRoot)
{
    private const string DefaultVariant = "default";

    private readonly string _knowledgeRoot =
        Path.Combine(repoRoot, "wpf-dev-pack", "knowledge");

    private readonly Lock _gate = new();
    private IReadOnlyList<Topic>? _cache;

    public void Invalidate()
    {
        lock (_gate)
        {
            _cache = null;
        }
    }

    public IReadOnlyList<Topic> List()
    {
        lock (_gate)
        {
            return _cache ??= Scan();
        }
    }

    public string GetContent(string id, string variant)
    {
        var file = ResolveVariantFile(RequireDir(id), variant);
        if (!File.Exists(file))
        {
            throw new TopicNotFoundException($"{id} (variant: {variant})");
        }

        return File.ReadAllText(file);
    }

    public IReadOnlyList<string> Variants(string id)
    {
        var dir = RequireDir(id);
        var list = new List<string> { DefaultVariant };
        if (File.Exists(Path.Combine(dir, "PRISM.md")))
        {
            list.Add("prism");
        }

        if (File.Exists(Path.Combine(dir, "ADVANCED.md")))
        {
            list.Add("advanced");
        }

        return list;
    }

    private string RequireDir(string id)
    {
        var dir = Path.Combine(_knowledgeRoot, id);
        if (!Directory.Exists(dir) || !File.Exists(Path.Combine(dir, "SKILL.md")))
        {
            throw new TopicNotFoundException(id);
        }

        return dir;
    }

    private static string ResolveVariantFile(string dir, string variant) => variant switch
    {
        "prism" => Path.Combine(dir, "PRISM.md"),
        "advanced" => Path.Combine(dir, "ADVANCED.md"),
        _ => Path.Combine(dir, "SKILL.md"),
    };

    private List<Topic> Scan()
    {
        var topics = new List<Topic>();
        if (!Directory.Exists(_knowledgeRoot))
        {
            return topics;
        }

        foreach (var dir in Directory.EnumerateDirectories(_knowledgeRoot).OrderBy(d => d, StringComparer.Ordinal))
        {
            var skill = Path.Combine(dir, "SKILL.md");
            if (!File.Exists(skill))
            {
                continue;
            }

            var id = Path.GetFileName(dir);
            var text = File.ReadAllText(skill);
            var summary = FrontmatterReader.ReadDescription(text) ?? string.Empty;
            var title = FrontmatterReader.ReadFirstHeading(text) ?? id;
            var companions = Directory.EnumerateFiles(dir, "*.md")
                .Select(Path.GetFileName)
                .Where(n => n is not null && !string.Equals(n, "SKILL.md", StringComparison.Ordinal))
                .Select(n => n!)
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToList();

            topics.Add(new Topic(id, title, summary, companions));
        }

        return topics;
    }
}
```

- [ ] **Step 5: 통과 확인**

Run: `dotnet test mcp/WpfDevPackMcp.Tests --filter TopicCatalogTests`
Expected: PASS (5 passed).

- [ ] **Step 6: 커밋(체크포인트)**

```bash
git add mcp/Knowledge/Topic.cs mcp/Knowledge/TopicCatalog.cs mcp/WpfDevPackMcp.Tests/TopicCatalogTests.cs
git commit -m "feat(wpf-dev-pack): add TopicCatalog scan/get over knowledge directory"
```

---

## Task 4: 검색 (TDD)

`{id, title, summary, body}` 부분일치 가중 점수로 상위 N개를 반환.

**Files:**
- Modify: `mcp/Knowledge/TopicCatalog.cs` (Search 추가)
- Test: `mcp/WpfDevPackMcp.Tests/TopicCatalogTests.cs` (검색 테스트 추가)

- [ ] **Step 1: 실패 테스트 추가**

`TopicCatalogTests.cs` 에 추가:
```csharp
    [Fact]
    public void Search_MatchesTitleAndSummary_RanksHigher()
    {
        var catalog = new TopicCatalog(_root);
        var hits = catalog.Search("mvvm", maxResults: 5);
        Assert.NotEmpty(hits);
        Assert.Equal("implementing-communitytoolkit-mvvm", hits[0].Id);
    }

    [Fact]
    public void Search_NoMatch_ReturnsEmpty()
        => Assert.Empty(new TopicCatalog(_root).Search("zzz-nonexistent", maxResults: 5));
```

- [ ] **Step 2: 실패 확인**

Run: `dotnet test mcp/WpfDevPackMcp.Tests --filter TopicCatalogTests`
Expected: FAIL — `Search`/`SearchHit` 미정의.

- [ ] **Step 3: 구현 — TopicCatalog 에 추가**

`TopicCatalog.cs` 상단(네임스페이스 내)에 레코드 추가:
```csharp
public sealed record SearchHit(string Id, string Title, string Snippet, int Score);
```

`TopicCatalog` 클래스에 메서드 추가:
```csharp
    public IReadOnlyList<SearchHit> Search(string query, int maxResults)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var terms = query.ToLowerInvariant()
            .Split([' ', '\t', ',', ';'], StringSplitOptions.RemoveEmptyEntries);
        if (terms.Length == 0)
        {
            return [];
        }

        var hits = new List<SearchHit>();
        foreach (var topic in List())
        {
            var dir = Path.Combine(_knowledgeRoot, topic.Id);
            var body = File.ReadAllText(Path.Combine(dir, "SKILL.md")).ToLowerInvariant();
            var id = topic.Id.ToLowerInvariant();
            var title = topic.Title.ToLowerInvariant();
            var summary = topic.Summary.ToLowerInvariant();

            var score = 0;
            foreach (var term in terms)
            {
                if (id.Contains(term)) score += 10;
                if (title.Contains(term)) score += 6;
                if (summary.Contains(term)) score += 4;
                if (body.Contains(term)) score += 1;
            }

            if (score > 0)
            {
                hits.Add(new SearchHit(topic.Id, topic.Title, Snippet(topic.Summary), score));
            }
        }

        return hits
            .OrderByDescending(h => h.Score)
            .ThenBy(h => h.Id, StringComparer.Ordinal)
            .Take(maxResults)
            .ToList();
    }

    private static string Snippet(string summary)
        => summary.Length <= 160 ? summary : summary[..160] + "…";
```

- [ ] **Step 4: 통과 확인**

Run: `dotnet test mcp/WpfDevPackMcp.Tests --filter TopicCatalogTests`
Expected: PASS (7 passed).

- [ ] **Step 5: 커밋(체크포인트)**

```bash
git add mcp/Knowledge/TopicCatalog.cs mcp/WpfDevPackMcp.Tests/TopicCatalogTests.cs
git commit -m "feat(wpf-dev-pack): add weighted Search to TopicCatalog"
```

---

## Task 5: 경로 해석 + 상태 저장 (TDD)

env `WPFDEVPACK_REPO_PATH` > `~/.wpf-dev-pack-mcp/config.json` 의 `repoPath`. 상태(`lastPullUtc`, `managed`)는 `state.json`.

**Files:**
- Create: `mcp/Configuration/RepoConfig.cs`
- Create: `mcp/Configuration/ConfigStore.cs`
- Test: `mcp/WpfDevPackMcp.Tests/ConfigStoreTests.cs`

- [ ] **Step 1: 모델 작성**

`mcp/Configuration/RepoConfig.cs`:
```csharp
namespace WpfDevPackMcp.Configuration;

/// <summary>User-written config (~/.wpf-dev-pack-mcp/config.json).</summary>
public sealed record RepoConfig(string? RepoPath, string Branch = "main");

/// <summary>Server-managed pull state (~/.wpf-dev-pack-mcp/state.json).</summary>
public sealed record RepoState(DateTimeOffset? LastPullUtc, bool Managed);

/// <summary>Fully resolved target the server will read from.</summary>
public sealed record ResolvedRepo(string Path, string Branch);
```

- [ ] **Step 2: 실패 테스트 작성**

`WpfDevPackMcp.Tests/ConfigStoreTests.cs`:
```csharp
using WpfDevPackMcp.Configuration;
using Xunit;

namespace WpfDevPackMcp.Tests;

public sealed class ConfigStoreTests : IDisposable
{
    private readonly string _home;

    public ConfigStoreTests()
    {
        _home = Path.Combine(Path.GetTempPath(), "wdpmcp-home-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_home);
        Environment.SetEnvironmentVariable("WPFDEVPACK_REPO_PATH", null);
    }

    [Fact]
    public void Resolve_EnvVarWins_OverConfigFile()
    {
        File.WriteAllText(Path.Combine(_home, "config.json"),
            """{ "repoPath": "C:/from-file", "branch": "main" }""");
        Environment.SetEnvironmentVariable("WPFDEVPACK_REPO_PATH", "C:/from-env");

        var store = new ConfigStore(_home);
        var resolved = store.Resolve();

        Assert.NotNull(resolved);
        Assert.Equal("C:/from-env", resolved!.Path);
    }

    [Fact]
    public void Resolve_FromConfigFile_WhenNoEnv()
    {
        File.WriteAllText(Path.Combine(_home, "config.json"),
            """{ "repoPath": "C:/from-file", "branch": "dev" }""");

        var resolved = new ConfigStore(_home).Resolve();

        Assert.Equal("C:/from-file", resolved!.Path);
        Assert.Equal("dev", resolved.Branch);
    }

    [Fact]
    public void Resolve_ReturnsNull_WhenUnconfigured()
        => Assert.Null(new ConfigStore(_home).Resolve());

    [Fact]
    public void State_RoundTrips()
    {
        var store = new ConfigStore(_home);
        var stamp = DateTimeOffset.UtcNow;
        store.SaveState(new RepoState(stamp, Managed: true));

        var loaded = store.LoadState();
        Assert.True(loaded.Managed);
        Assert.Equal(stamp.ToUnixTimeSeconds(), loaded.LastPullUtc!.Value.ToUnixTimeSeconds());
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("WPFDEVPACK_REPO_PATH", null);
        if (Directory.Exists(_home))
        {
            Directory.Delete(_home, recursive: true);
        }
    }
}
```

- [ ] **Step 3: 실패 확인**

Run: `dotnet test mcp/WpfDevPackMcp.Tests --filter ConfigStoreTests`
Expected: FAIL — `ConfigStore` 미정의.

- [ ] **Step 4: 구현**

`mcp/Configuration/ConfigStore.cs`:
```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WpfDevPackMcp.Configuration;

/// <summary>
/// Reads the user-written repo config and persists server pull state under
/// a base directory (default: ~/.wpf-dev-pack-mcp). The env var
/// WPFDEVPACK_REPO_PATH overrides the config file's repoPath.
/// </summary>
public sealed class ConfigStore
{
    public const string RepoPathEnvVar = "WPFDEVPACK_REPO_PATH";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly string _baseDir;

    public ConfigStore(string? baseDir = null)
    {
        _baseDir = baseDir ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".wpf-dev-pack-mcp");
        Directory.CreateDirectory(_baseDir);
    }

    public string ConfigPath => Path.Combine(_baseDir, "config.json");
    public string StatePath => Path.Combine(_baseDir, "state.json");

    public ResolvedRepo? Resolve()
    {
        var config = LoadConfig();
        var branch = string.IsNullOrWhiteSpace(config?.Branch) ? "main" : config!.Branch;

        var fromEnv = Environment.GetEnvironmentVariable(RepoPathEnvVar);
        var path = !string.IsNullOrWhiteSpace(fromEnv) ? fromEnv : config?.RepoPath;

        return string.IsNullOrWhiteSpace(path) ? null : new ResolvedRepo(path!, branch);
    }

    public RepoConfig? LoadConfig()
        => File.Exists(ConfigPath)
            ? JsonSerializer.Deserialize<RepoConfig>(File.ReadAllText(ConfigPath), JsonOptions)
            : null;

    public RepoState LoadState()
        => File.Exists(StatePath)
            ? JsonSerializer.Deserialize<RepoState>(File.ReadAllText(StatePath), JsonOptions)
              ?? new RepoState(null, false)
            : new RepoState(null, false);

    public void SaveState(RepoState state)
        => File.WriteAllText(StatePath, JsonSerializer.Serialize(state, JsonOptions));
}
```

- [ ] **Step 5: 통과 확인**

Run: `dotnet test mcp/WpfDevPackMcp.Tests --filter ConfigStoreTests`
Expected: PASS (4 passed).

> 주의: 테스트는 환경변수를 변경하므로 `ConfigStoreTests` 컬렉션을 병렬 비활성화한다(아래 Step 6).

- [ ] **Step 6: 환경변수 테스트 직렬화**

`ConfigStoreTests` 클래스 선언 위에 추가:
```csharp
[Collection("env-serial")]
```
그리고 `WpfDevPackMcp.Tests/EnvSerialCollection.cs` 생성:
```csharp
using Xunit;

namespace WpfDevPackMcp.Tests;

[CollectionDefinition("env-serial", DisableParallelization = true)]
public sealed class EnvSerialCollection;
```

- [ ] **Step 7: 통과 재확인 + 커밋**

Run: `dotnet test mcp/WpfDevPackMcp.Tests --filter ConfigStoreTests`
Expected: PASS.
```bash
git add mcp/Configuration/ mcp/WpfDevPackMcp.Tests/ConfigStoreTests.cs mcp/WpfDevPackMcp.Tests/EnvSerialCollection.cs
git commit -m "feat(wpf-dev-pack): add ConfigStore path resolution + pull state"
```

---

## Task 6: 갱신 결정 (TTL 순수 함수, TDD)

**Files:**
- Create: `mcp/Git/RepoRefresher.cs` (결정 함수 우선)
- Test: `mcp/WpfDevPackMcp.Tests/RepoRefresherTests.cs`

- [ ] **Step 1: 실패 테스트 작성**

`WpfDevPackMcp.Tests/RepoRefresherTests.cs`:
```csharp
using WpfDevPackMcp.Git;
using Xunit;

namespace WpfDevPackMcp.Tests;

public sealed class RepoRefresherTests
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(60);

    [Fact]
    public void ShouldPull_True_WhenNeverPulled()
        => Assert.True(RepoRefresher.ShouldPull(lastPull: null, now: DateTimeOffset.UtcNow, Ttl, force: false));

    [Fact]
    public void ShouldPull_True_WhenTtlElapsed()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.True(RepoRefresher.ShouldPull(now.AddMinutes(-61), now, Ttl, force: false));
    }

    [Fact]
    public void ShouldPull_False_WithinTtl()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.False(RepoRefresher.ShouldPull(now.AddMinutes(-10), now, Ttl, force: false));
    }

    [Fact]
    public void ShouldPull_True_WhenForced_EvenWithinTtl()
    {
        var now = DateTimeOffset.UtcNow;
        Assert.True(RepoRefresher.ShouldPull(now.AddMinutes(-1), now, Ttl, force: true));
    }
}
```

- [ ] **Step 2: 실패 확인**

Run: `dotnet test mcp/WpfDevPackMcp.Tests --filter RepoRefresherTests`
Expected: FAIL — `RepoRefresher` 미정의.

- [ ] **Step 3: 구현 (결정 함수만 먼저)**

`mcp/Git/RepoRefresher.cs`:
```csharp
namespace WpfDevPackMcp.Git;

public static partial class RepoRefresher
{
    /// <summary>Pure decision: pull if forced, never pulled, or TTL elapsed.</summary>
    public static bool ShouldPull(DateTimeOffset? lastPull, DateTimeOffset now, TimeSpan ttl, bool force)
        => force || lastPull is null || (now - lastPull.Value) >= ttl;
}
```

- [ ] **Step 4: 통과 확인 + 커밋**

Run: `dotnet test mcp/WpfDevPackMcp.Tests --filter RepoRefresherTests`
Expected: PASS (4 passed).
```bash
git add mcp/Git/RepoRefresher.cs mcp/WpfDevPackMcp.Tests/RepoRefresherTests.cs
git commit -m "feat(wpf-dev-pack): add TTL pull decision (RepoRefresher.ShouldPull)"
```

---

## Task 7: Git 실행기 + 갱신 오케스트레이션 (통합, 수동 검증)

`git` 프로세스 호출은 외부 의존이라 단위테스트 대신 실제 임시 저장소로 수동 검증한다.

**Files:**
- Create: `mcp/Git/GitRunner.cs`
- Modify: `mcp/Git/RepoRefresher.cs` (`EnsureFresh` 추가)

- [ ] **Step 1: GitRunner 작성**

`mcp/Git/GitRunner.cs`:
```csharp
using System.Diagnostics;

namespace WpfDevPackMcp.Git;

public sealed record GitResult(bool Success, string StdOut, string StdErr);

/// <summary>Thin best-effort wrapper around the `git` executable.</summary>
public sealed class GitRunner
{
    public bool IsGitAvailable()
    {
        try
        {
            return Run(Environment.CurrentDirectory, "--version").Success;
        }
        catch
        {
            return false;
        }
    }

    public GitResult Run(string workingDir, params string[] args)
    {
        var psi = new ProcessStartInfo("git")
        {
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var a in args)
        {
            psi.ArgumentList.Add(a);
        }

        using var proc = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start git process.");
        var stdout = proc.StandardOutput.ReadToEnd();
        var stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit();
        return new GitResult(proc.ExitCode == 0, stdout, stderr);
    }
}
```

- [ ] **Step 2: RepoRefresher.EnsureFresh 작성**

`mcp/Git/RepoRefresher.cs` 에 추가(같은 `partial` 클래스):
```csharp
using Microsoft.Extensions.Logging;
using WpfDevPackMcp.Configuration;

namespace WpfDevPackMcp.Git;

public static partial class RepoRefresher
{
    private const string RepoUrl = "https://github.com/christian289/dotnet-with-claudecode";

    /// <summary>
    /// Best-effort: clone if missing, else pull when due. Never throws — logs
    /// failures and leaves the local clone usable. Returns the (possibly
    /// updated) RepoState to persist.
    /// </summary>
    public static RepoState EnsureFresh(
        ResolvedRepo repo, RepoState state, TimeSpan ttl, bool force,
        GitRunner git, ConfigStore store, ILogger logger)
    {
        var now = DateTimeOffset.UtcNow;
        if (!git.IsGitAvailable())
        {
            logger.LogWarning("git not found on PATH; serving local content as-is.");
            return state;
        }

        var isRepo = Directory.Exists(Path.Combine(repo.Path, ".git"));
        var managed = state.Managed;

        try
        {
            if (!isRepo)
            {
                Directory.CreateDirectory(repo.Path);
                logger.LogInformation("Cloning {Url} into {Path}…", RepoUrl, repo.Path);
                var clone = git.Run(repo.Path, "clone", "--branch", repo.Branch, RepoUrl, ".");
                if (!clone.Success)
                {
                    logger.LogWarning("git clone failed: {Err}", clone.StdErr);
                    return state;
                }

                managed = true;
                return Save(store, new RepoState(now, managed));
            }

            if (!ShouldPull(state.LastPullUtc, now, ttl, force))
            {
                return state;
            }

            if (managed)
            {
                git.Run(repo.Path, "fetch", "origin", repo.Branch);
                var reset = git.Run(repo.Path, "reset", "--hard", $"origin/{repo.Branch}");
                if (!reset.Success)
                {
                    logger.LogWarning("git reset failed: {Err}", reset.StdErr);
                }
            }
            else
            {
                var status = git.Run(repo.Path, "status", "--porcelain");
                if (status.Success && string.IsNullOrWhiteSpace(status.StdOut))
                {
                    var pull = git.Run(repo.Path, "pull", "--ff-only");
                    if (!pull.Success)
                    {
                        logger.LogWarning("git pull failed: {Err}", pull.StdErr);
                    }
                }
                else
                {
                    logger.LogInformation("Working tree dirty; skipping pull to protect local edits.");
                }
            }

            return Save(store, new RepoState(now, managed));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Repo refresh failed; serving local content as-is.");
            return state;
        }
    }

    private static RepoState Save(ConfigStore store, RepoState state)
    {
        store.SaveState(state);
        return state;
    }
}
```

- [ ] **Step 3: 빌드 확인**

Run: `dotnet build mcp/WpfDevPackMcp.csproj`
Expected: 에러 0.

- [ ] **Step 4: 수동 통합 검증 (실제 임시 클론)**

PowerShell:
```powershell
$tmp = Join-Path $env:TEMP ("wdp-" + [guid]::NewGuid())
git clone --depth 1 https://github.com/christian289/dotnet-with-claudecode $tmp
Test-Path (Join-Path $tmp "wpf-dev-pack")   # Expected: True
```
Expected: clone 성공, `wpf-dev-pack` 디렉터리 존재. (이 경로는 Task 10 스모크 테스트의 `WPFDEVPACK_REPO_PATH` 로 재사용)

- [ ] **Step 5: 커밋(체크포인트)**

```bash
git add mcp/Git/
git commit -m "feat(wpf-dev-pack): add GitRunner + best-effort EnsureFresh (clone/pull)"
```

---

## Task 8: MCP 도구 클래스

검증된 SDK 패턴(`[McpServerToolType]` / `[McpServerTool, Description]`, DI 생성자 주입).

**Files:**
- Create: `mcp/Tools/WpfTopicTools.cs`
- Create: `mcp/Knowledge/KnowledgeService.cs` (도구가 의존하는 파사드)

- [ ] **Step 1: KnowledgeService 파사드 작성**

도구가 카탈로그 + 갱신을 한 곳에서 쓰도록 묶는다.

`mcp/Knowledge/KnowledgeService.cs`:
```csharp
using Microsoft.Extensions.Logging;
using WpfDevPackMcp.Configuration;
using WpfDevPackMcp.Git;

namespace WpfDevPackMcp.Knowledge;

public sealed class RepoNotConfiguredException()
    : Exception("WPF knowledge repo path is not configured. Run /wpf-dev-pack:set-repo-path <path>.");

/// <summary>
/// Facade the MCP tools depend on: resolves the repo, refreshes (TTL/forced),
/// and exposes catalog operations. Throws RepoNotConfiguredException when no
/// path is set (the RepoPathGuard hook normally prevents reaching here).
/// </summary>
public sealed class KnowledgeService(ConfigStore store, GitRunner git, ILogger<KnowledgeService> logger)
{
    private static readonly TimeSpan Ttl = ReadTtl();
    private readonly Lock _gate = new();
    private TopicCatalog? _catalog;
    private string? _catalogRoot;

    public void EnsureReady(bool force = false)
    {
        var repo = store.Resolve() ?? throw new RepoNotConfiguredException();
        var state = RepoRefresher.EnsureFresh(repo, store.LoadState(), Ttl, force, git, store, logger);
        _ = state;

        lock (_gate)
        {
            if (_catalog is null || !string.Equals(_catalogRoot, repo.Path, StringComparison.Ordinal))
            {
                _catalog = new TopicCatalog(repo.Path);
                _catalogRoot = repo.Path;
            }
            else if (force)
            {
                _catalog.Invalidate();
            }
        }
    }

    public TopicCatalog Catalog => _catalog ?? throw new RepoNotConfiguredException();

    private static TimeSpan ReadTtl()
    {
        var raw = Environment.GetEnvironmentVariable("WPFDEVPACK_PULL_TTL_MINUTES");
        return int.TryParse(raw, out var m) && m >= 0 ? TimeSpan.FromMinutes(m) : TimeSpan.FromMinutes(60);
    }
}
```

- [ ] **Step 2: 도구 클래스 작성**

`mcp/Tools/WpfTopicTools.cs`:
```csharp
using System.ComponentModel;
using ModelContextProtocol.Server;
using WpfDevPackMcp.Knowledge;

namespace WpfDevPackMcp.Tools;

[McpServerToolType]
public sealed class WpfTopicTools(KnowledgeService knowledge)
{
    [McpServerTool, Description("Lists all WPF knowledge topics with a one-line summary and available companion files (PRISM.md, ADVANCED.md, references).")]
    public IReadOnlyList<Topic> list_wpf_topics()
    {
        knowledge.EnsureReady();
        return knowledge.Catalog.List();
    }

    [McpServerTool, Description("Returns the full markdown for one WPF knowledge topic. variant: 'default' (SKILL.md), 'prism' (PRISM.md), or 'advanced' (ADVANCED.md).")]
    public string get_wpf_topic(
        [Description("Topic id, e.g. 'implementing-communitytoolkit-mvvm' (a knowledge/ directory name).")] string id,
        [Description("Which variant to return: default | prism | advanced.")] string variant = "default")
    {
        knowledge.EnsureReady();
        try
        {
            return knowledge.Catalog.GetContent(id, variant);
        }
        catch (TopicNotFoundException)
        {
            var ids = string.Join(", ", knowledge.Catalog.List().Select(t => t.Id));
            return $"Topic or variant not found: id='{id}', variant='{variant}'.\nAvailable ids: {ids}";
        }
    }

    [McpServerTool, Description("Searches WPF knowledge topics by keyword over id, title, summary, and body. Returns ranked matches.")]
    public IReadOnlyList<SearchHit> search_wpf_topics(
        [Description("Search query (one or more keywords).")] string query,
        [Description("Maximum number of results (default 8).")] int maxResults = 8)
    {
        knowledge.EnsureReady();
        return knowledge.Catalog.Search(query, maxResults);
    }

    [McpServerTool, Description("Forces an immediate git pull of the knowledge repo and rescans the catalog. Returns the topic count after refresh.")]
    public string refresh_wpf_knowledge()
    {
        knowledge.EnsureReady(force: true);
        return $"Refreshed. {knowledge.Catalog.List().Count} topics available.";
    }
}
```

- [ ] **Step 3: 빌드 확인**

Run: `dotnet build mcp/WpfDevPackMcp.csproj`
Expected: 에러 0.

- [ ] **Step 4: 커밋(체크포인트)**

```bash
git add mcp/Tools/ mcp/Knowledge/KnowledgeService.cs
git commit -m "feat(wpf-dev-pack): add KnowledgeService facade + 4 MCP tools"
```

---

## Task 9: Program.cs 호스트 구성 + 스모크 테스트

**Files:**
- Create: `mcp/Program.cs`

- [ ] **Step 1: Program.cs 작성**

`mcp/Program.cs`:
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WpfDevPackMcp.Configuration;
using WpfDevPackMcp.Git;
using WpfDevPackMcp.Knowledge;
using WpfDevPackMcp.Tools;

var builder = Host.CreateApplicationBuilder(args);

// CRITICAL: stdout is reserved for MCP JSON-RPC. All logs go to stderr.
builder.Logging.ClearProviders();
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddSingleton<ConfigStore>(_ => new ConfigStore());
builder.Services.AddSingleton<GitRunner>();
builder.Services.AddSingleton<KnowledgeService>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<WpfTopicTools>();

await builder.Build().RunAsync();
```

> `KnowledgeService.EnsureReady()` 가 각 도구 호출 시 갱신을 트리거하므로 별도 시작-시 IHostedService 는 생략(YAGNI). 첫 도구 호출이 곧 시작-시 갱신.

- [ ] **Step 2: 빌드 확인**

Run: `dotnet build mcp/WpfDevPackMcp.csproj`
Expected: 에러 0.

- [ ] **Step 3: 스모크 테스트 — tools/list (JSON-RPC over stdio)**

Task 7 Step 4의 임시 클론 경로를 `$tmp` 로 재사용. PowerShell:
```powershell
$env:WPFDEVPACK_REPO_PATH = $tmp
$init = '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"smoke","version":"1.0"}}}'
$listed = '{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}'
@($init, $listed) | dotnet run --project mcp/WpfDevPackMcp.csproj
```
Expected(stdout): `initialize` 결과 + `tools/list` 응답에 `list_wpf_topics`, `get_wpf_topic`, `search_wpf_topics`, `refresh_wpf_knowledge` 4개 도구가 보인다. 로그는 stderr 로만 출력(stdout 오염 없음).

> 참고: 단, 마이그레이션(Plan 2) 전이면 `wpf-dev-pack/knowledge/` 가 비어 `list` 가 0개일 수 있다. 도구 노출 자체가 검증 대상.

- [ ] **Step 4: 커밋(체크포인트)**

```bash
git add mcp/Program.cs
git commit -m "feat(wpf-dev-pack): wire stdio MCP host (Program.cs)"
```

---

## Task 10: 전체 테스트 + dnx 패키징 검증

**Files:** 없음(검증·패키징).

- [ ] **Step 1: 전체 단위 테스트**

Run: `dotnet test mcp/WpfDevPackMcp.Tests`
Expected: 모든 테스트 PASS.

- [ ] **Step 2: tool 패키지 pack**

Run: `dotnet pack mcp/WpfDevPackMcp.csproj -c Release -o mcp/nupkg`
Expected: `mcp/nupkg/WpfDevPackMcp.0.1.0.nupkg` 생성. 경고에 `PackAsTool` 관련 에러 없음.

- [ ] **Step 3: 로컬 dnx 실행 검증(선택)**

Run: `dnx --source mcp/nupkg WpfDevPackMcp@0.1.0 --yes` 에 Step 9-3의 stdin 파이프를 적용해 tools/list 가 동일하게 나오는지 확인.
Expected: 글로벌 설치 없이 dnx로 동일 동작.

- [ ] **Step 4: nupkg 산출물 gitignore**

`mcp/.gitignore` 생성:
```gitignore
bin/
obj/
nupkg/
```

- [ ] **Step 5: 커밋(체크포인트)**

```bash
git add mcp/.gitignore
git commit -m "chore(wpf-dev-pack): ignore build/pack artifacts for WpfDevPackMcp"
```

---

## Self-Review (작성자 체크)

**Spec 커버리지(§5.A):**
- 경로 해석 env>config.json → Task 5 ✅
- 로컬 클론 clone/pull, managed vs working-copy, dirty skip, best-effort → Task 7 ✅
- TTL 갱신(기본 60m, env 조정) → Task 6 + KnowledgeService.ReadTtl ✅
- 카탈로그 스캔(= 인덱스, manifest 불필요) → Task 3 ✅
- 도구 4종(list/get/search/refresh) + variant 매핑 → Task 8 ✅
- stdout=JSON-RPC, stderr 로그 → Task 9 ✅
- dnx/NuGet tool 배포 → Task 1(PackAsTool) + Task 10 ✅
- 단일 프로젝트, EmbeddedResource 없음 → Task 1 ✅

**플레이스홀더 스캔:** `ModelContextProtocol` 버전 `0.4.*`·`Version 0.1.0` 은 의도된 시작값(구현 시 핀). TODO/미정 코드 없음.

**타입 일관성:** `ConfigStore`/`ResolvedRepo`/`RepoState`/`Topic`/`SearchHit`/`TopicCatalog`/`GitRunner`/`RepoRefresher`/`KnowledgeService`/`WpfTopicTools` 시그니처가 Task 간 일치. `EnsureReady`/`Catalog`/`Invalidate`/`ShouldPull`/`EnsureFresh` 명칭 일관.

> **미해결(구현 시 확정):** `ModelContextProtocol` 실제 최신 버전, `WithTools<T>` 의 인스턴스 도구 DI 동작(생성자 주입) 재확인, dnx `--source` 로컬 피드 플래그 정확 표기. Task 1·8·10에서 패키지 실물로 검증.

---

## 다음 단계

이 Plan 1 은 **MCP 서버 자체**만 다룬다. 다음을 **Plan 2** 에서 다룬다:
- 지식 50개 디렉터리 `git mv` 마이그레이션(`skills/` → `knowledge/`)
- `set-repo-path` 스킬 + `SetWpfDevPackRepoPath.cs`
- `RepoPathGuard.cs`(PreToolUse) + `hooks.json`
- `WpfKeywordDetector.cs` 라우터 분기 + `skills/.claude/CLAUDE.md` 슬림
- `.mcp.json`, 플러그인·루트 CLAUDE.md, `.claude.ko/` 미러, 훅 README
- `applying-wpf-dev-pack-feedback` 워크플로우 갱신
- NuGet publish + `.mcp.json` 버전 핀
