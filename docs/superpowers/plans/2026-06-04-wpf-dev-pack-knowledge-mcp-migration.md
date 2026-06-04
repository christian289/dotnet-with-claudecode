# wpf-dev-pack 마이그레이션 & 플러그인 재배선 구현 계획 (Plan 2 / 2)

> **Amendment (2026-06-04 follow-up):** 지식 토픽 메인 파일이 `SKILL.md` → `TOPIC.md`로 변경되었으며 YAML frontmatter가 제거되었습니다. 요약은 본문의 첫 번째 `>` 블록인용에 위치합니다. 서버의 카탈로그 리더는 `FrontmatterReader` → `TopicDocReader`로 교체되었고, 제목은 첫 번째 `# H1`에서, 요약은 첫 번째 `>` 블록인용에서 읽습니다. 이 계획 본문의 `SKILL.md` 참조(지식 토픽용)는 역사적 기록으로 보존되며 실제 파일은 `TOPIC.md`입니다.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
>
> **커밋 정책(메인테이너 선호):** 각 Task 끝의 commit 스텝은 **논리적 체크포인트**다. 실행 중 step별 커밋을 남발하지 말고 끝에 의미 있는 소수 커밋으로 **squash**. **push는 명시 요청 시에만.**
>
> **선행:** Plan 1(`WpfDevPackMcp` 서버)이 빌드/패키징 가능해야 한다. 이 계획은 서버 도구명(`list_wpf_topics`/`get_wpf_topic`/`search_wpf_topics`/`refresh_wpf_knowledge`)과 NuGet 패키지명(`WpfDevPackMcp`)을 참조한다.

**Goal:** 지식 스킬 50개를 `skills/` → `knowledge/` 로 이전하고, 라우터·가드 훅·설정 스킬·매니페스트·문서·피드백 워크플로우를 MCP 기반으로 재배선한다.

**Architecture:** 콘텐츠 디렉터리 이동(`git mv`) + 신규 file-based app/훅(C#) + 기존 훅/매니페스트/CLAUDE 편집. 자동 트리거는 기존 `WpfKeywordDetector`(UserPromptSubmit) 출력 분기로 보존, 미설정 가드는 신규 `RepoPathGuard`(PreToolUse).

**Tech Stack:** C# file-based apps(`dotnet` 실행 훅), Claude Code 플러그인 매니페스트(`.mcp.json`/`hooks.json`/`SKILL.md`), git.

> 스펙: `docs/superpowers/specs/2026-06-04-wpf-dev-pack-knowledge-mcp-design.md` §B,C,D,E,F,§7,§8,§10. 서버는 **Plan 1**.

---

## 파일 구조 (생성/수정)

| 파일 | 동작 | 책임 |
|------|------|------|
| `wpf-dev-pack/knowledge/<id>/…` | 생성(이동) | 지식 50개 토픽 |
| `wpf-dev-pack/skills/set-repo-path/SKILL.md` | 생성 | 경로 설정 커맨드 스킬 |
| `wpf-dev-pack/scripts/SetWpfDevPackRepoPath.cs` | 생성 | 경로 검증·기록 file-based app |
| `wpf-dev-pack/hooks/RepoPathGuard.cs` | 생성 | PreToolUse 미설정 차단 |
| `wpf-dev-pack/hooks/WpfKeywordDetector.cs` | 수정 | 라우터 출력 분기(지식→MCP) |
| `wpf-dev-pack/hooks/hooks.json` | 수정 | RepoPathGuard 등록 |
| `wpf-dev-pack/.mcp.json` | 수정 | WpfDevPackMcp 등록 |
| `wpf-dev-pack/skills/.claude/CLAUDE.md` | 수정 | 키워드 테이블 슬림 |
| `wpf-dev-pack/.claude/CLAUDE.md` | 수정 | Trigger Behavior/Adding-skill 갱신 |
| `.claude/CLAUDE.md`(루트) | 수정 | 레이아웃·메인테이너 표·체크리스트 |
| `.claude.ko/CLAUDE.md`, `wpf-dev-pack/.claude.ko/CLAUDE.md`, `wpf-dev-pack/skills/.claude.ko/CLAUDE.md` | 수정 | 한국어 미러 |
| `.claude/skills/applying-wpf-dev-pack-feedback/SKILL.md` | 수정 | Step3 표·version 함의 |
| `.claude/skills/applying-wpf-dev-pack-feedback/hooks/MicrosoftSkillCreatorReminder.cs` | 수정 | `knowledge/` 경로 인식 |
| 잔류 커맨드 스킬들의 "Related Skills" | 수정 | 이동된 토픽 교차링크 보정 |
| `docs/changelogs/wpf-dev-pack.md` | 수정(릴리스) | 변경 엔트리 |

---

## Task 1: 지식 50개 디렉터리 이전 (`skills/` → `knowledge/`)

**Files:** 50개 디렉터리 이동.

- [ ] **Step 1: 이동 전 개수 확인**

Run (PowerShell):
```powershell
(Get-ChildItem wpf-dev-pack/skills -Directory).Count   # 이동 전 스킬 디렉터리 수 기록
```
Expected: 현재 스킬 디렉터리 수(참고용 기준선).

- [ ] **Step 2: `git mv` 로 50개 이동**

Run (PowerShell):
```powershell
$ids = @(
 'advanced-data-binding','authoring-wpf-controls','binding-enum-command-parameters',
 'checking-image-bounds-transform','configuring-console-app-di','configuring-dependency-injection',
 'configuring-wpf-themeinfo','containing-control-decorative-overflow','designing-wpf-customcontrol-architecture',
 'displaying-slider-index','embedding-pdb-in-exe','flaui-cross-process-input','flaui-prism-dialog-discovery',
 'flaui-wpf-element-discovery','handling-errors-with-erroror','highlighting-nodify-connections',
 'implementing-communitytoolkit-mvvm','implementing-hit-testing','implementing-repository-pattern',
 'implementing-wpf-splash-screen','implementing-wpf-validation','integrating-livecharts2','integrating-nodify',
 'integrating-wpfui-fluent','managing-literal-strings','managing-styles-resourcedictionary','managing-unit-tests',
 'managing-wpf-application-lifecycle','managing-wpf-collectionview-mvvm','managing-wpf-popup-focus',
 'navigating-visual-logical-tree','optimizing-wpf-memory','preventing-dispatcher-deadlock','publishing-wpf-apps',
 'rendering-with-drawingcontext','rendering-with-drawingvisual','rendering-wpf-architecture',
 'rendering-wpf-high-performance','resolving-icon-font-inheritance','routing-wpf-events',
 'scottplot-axes-margins-destructive','scottplot-syncing-modifier-keys-for-mousewheel',
 'shutting-down-wpf-gracefully','structuring-wpf-projects','testing-wpf-viewmodels',
 'threading-wpf-dispatcher','using-converter-markup-extension','using-xaml-property-element-syntax',
 'validating-with-fluentvalidation','virtualizing-wpf-ui'
)
New-Item -ItemType Directory -Force wpf-dev-pack/knowledge | Out-Null
foreach ($id in $ids) { git mv "wpf-dev-pack/skills/$id" "wpf-dev-pack/knowledge/$id" }
```
Expected: 50개 디렉터리 이동, 에러 0.

- [ ] **Step 3: 이동 결과 검증**

Run (PowerShell):
```powershell
(Get-ChildItem wpf-dev-pack/knowledge -Directory).Count                 # Expected: 50
(Get-ChildItem wpf-dev-pack/knowledge -Recurse -Filter SKILL.md).Count  # Expected: 50
(Get-ChildItem wpf-dev-pack/skills -Directory).Count                    # Expected: 10 (잔류)
```
Expected: knowledge 50 / SKILL.md 50 / skills 10.

- [ ] **Step 4: 잔류 10개가 정확한지 확인**

Run (PowerShell):
```powershell
(Get-ChildItem wpf-dev-pack/skills -Directory).Name | Sort-Object
```
Expected(정렬): `collecting-wpf-dev-pack-feedback`, `configuring-wpf-dev-pack-language`, `formatting-wpf-csharp-code`, `make-wpf-behavior`, `make-wpf-converter`, `make-wpf-custom-control`, `make-wpf-project`, `make-wpf-service`, `make-wpf-usercontrol`, `make-wpf-viewmodel`. (set-repo-path 은 Task 3에서 추가 → 11개)

- [ ] **Step 5: 커밋(체크포인트)**

```bash
git add -A wpf-dev-pack/skills wpf-dev-pack/knowledge
git commit -m "refactor(wpf-dev-pack): move 50 knowledge skills to knowledge/ (out of plugin skill loader)"
```

---

## Task 2: 잔류 커맨드 스킬의 깨진 교차링크 보정

이동된 토픽을 가리키던 잔류 스킬의 상대 링크(`../<id>/SKILL.md`)가 깨진다.

**Files:** 잔류 스킬들의 "Related Skills" 등 교차링크(예: `wpf-dev-pack/skills/make-wpf-converter/SKILL.md:308-312`).

- [ ] **Step 1: 깨진 링크 탐색**

Run:
```powershell
Select-String -Path wpf-dev-pack/skills/*/SKILL.md,wpf-dev-pack/skills/*/PRISM.md -Pattern '\.\./([a-z0-9-]+)/SKILL\.md' -AllMatches |
  ForEach-Object { $_.Matches } | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique
```
Expected: 참조되는 토픽 id 목록. 이 중 `knowledge/` 로 이동된 것이 깨진 대상.

- [ ] **Step 2: 링크를 MCP 토픽 참조로 치환**

이동된 토픽을 가리키는 `[...](../<id>/SKILL.md)` 링크를, 파일 경로가 아니라 **MCP 토픽 안내**로 바꾼다. 예 — `make-wpf-converter/SKILL.md` 의 Related Skills:
```markdown
## Related Skills

- `using-converter-markup-extension` — MCP knowledge topic (use `WpfDevPackMcp get_wpf_topic`).
- `advanced-data-binding` — MCP knowledge topic (use `WpfDevPackMcp get_wpf_topic`).
- Plugin rule: `.claude/rules/converter-patterns.md` — Singleton `Instance`, null/UnsetValue handling, TemplateBinding guidance
```
다른 잔류 스킬도 동일 패턴으로, 깨진 `../<id>/SKILL.md` 경로 링크를 "MCP knowledge topic" 안내(id 명시)로 치환. 잔류 스킬끼리의 링크는 그대로 둔다.

- [ ] **Step 3: 깨진 경로 링크 부재 검증**

Run:
```powershell
$ids = (Get-ChildItem wpf-dev-pack/knowledge -Directory).Name
$hits = Select-String -Path wpf-dev-pack/skills/*/SKILL.md,wpf-dev-pack/skills/*/PRISM.md -Pattern '\.\./([a-z0-9-]+)/SKILL\.md' -AllMatches |
  ForEach-Object { $_.Matches } | ForEach-Object { $_.Groups[1].Value } | Where-Object { $ids -contains $_ }
$hits   # Expected: 출력 없음(이동된 토픽으로의 경로 링크 0건)
```
Expected: 출력 없음.

- [ ] **Step 4: 커밋(체크포인트)**

```bash
git add wpf-dev-pack/skills
git commit -m "docs(wpf-dev-pack): repoint retained skills' cross-links to MCP knowledge topics"
```

---

## Task 3: `set-repo-path` 스킬 + file-based app

**Files:**
- Create: `wpf-dev-pack/skills/set-repo-path/SKILL.md`
- Create: `wpf-dev-pack/scripts/SetWpfDevPackRepoPath.cs`

- [ ] **Step 1: file-based app 작성**

`wpf-dev-pack/scripts/SetWpfDevPackRepoPath.cs`:
```csharp
#!/usr/bin/env dotnet

// Validates a local clone path of christian289/dotnet-with-claudecode and
// records it to ~/.wpf-dev-pack-mcp/config.json so WpfDevPackMcp can read it.
// Usage: dotnet SetWpfDevPackRepoPath.cs <repo-path> [branch]

using System.Text.Json;

var path = args.Length > 0 ? args[0]?.Trim().Trim('"') : null;
var branch = args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]) ? args[1].Trim() : "main";

if (string.IsNullOrWhiteSpace(path))
{
    Console.Error.WriteLine("ERROR: repo path argument is required.");
    Console.Error.WriteLine("Usage: /wpf-dev-pack:set-repo-path <path-to-local-clone> [branch]");
    return 1;
}

var full = Path.GetFullPath(path);
var warnings = new List<string>();

if (!Directory.Exists(full))
{
    warnings.Add($"Path does not exist yet: {full}. WpfDevPackMcp will clone the repo there on first use.");
}
else
{
    if (!Directory.Exists(Path.Combine(full, ".git")))
    {
        warnings.Add($"No .git found at {full}. If it is empty, the server will clone into it; otherwise point at a real clone.");
    }

    if (!Directory.Exists(Path.Combine(full, "wpf-dev-pack")))
    {
        warnings.Add("No 'wpf-dev-pack' folder found at the path — confirm this is the dotnet-with-claudecode repo root.");
    }
}

var baseDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wpf-dev-pack-mcp");
Directory.CreateDirectory(baseDir);

var configPath = Path.Combine(baseDir, "config.json");
var json = JsonSerializer.Serialize(
    new { repoPath = full.Replace('\\', '/'), branch },
    new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(configPath, json);

Console.WriteLine($"WpfDevPackMcp repo path set:");
Console.WriteLine($"  repoPath = {full}");
Console.WriteLine($"  branch   = {branch}");
Console.WriteLine($"  written  = {configPath}");
foreach (var w in warnings)
{
    Console.WriteLine($"  ! {w}");
}

return 0;
```

- [ ] **Step 2: 스킬 작성**

`wpf-dev-pack/skills/set-repo-path/SKILL.md`:
```markdown
---
description: "Sets the local clone path of the christian289/dotnet-with-claudecode repository so the WpfDevPackMcp server can read WPF knowledge topics from disk. Use when the WpfDevPackMcp server reports the repo path is not configured, when first setting up wpf-dev-pack on a machine, or when moving the local clone. Writes ~/.wpf-dev-pack-mcp/config.json."
argument-hint: "<repo-path> [branch]"
disable-model-invocation: true
---

# Set WpfDevPackMcp Repository Path

Records the on-disk location of a local clone of
`christian289/dotnet-with-claudecode` so the `WpfDevPackMcp` MCP server can
serve WPF knowledge topics from `wpf-dev-pack/knowledge/`.

**If `$0` is empty, use AskUserQuestion to ask for the absolute path to the
local clone (e.g., `C:/Users/<you>/src/dotnet-with-claudecode`). Do NOT
proceed until a path is provided.** Optionally accept a branch as `$1`
(default `main`).

Run the file-based app to validate and persist the path:

```bash
dotnet "${CLAUDE_PLUGIN_ROOT}/scripts/SetWpfDevPackRepoPath.cs" "$0" "$1"
```

Surface the app's output verbatim, including any `!` warnings (path missing,
no `.git`, etc.). After it succeeds, tell the user the WpfDevPackMcp tools
are now usable and that the server will clone/pull the repo as needed.
```

- [ ] **Step 3: file-based app 단독 실행 검증**

Run (PowerShell, Task 7 Plan1 의 `$tmp` 또는 실제 레포 경로):
```powershell
dotnet "wpf-dev-pack/scripts/SetWpfDevPackRepoPath.cs" "C:/Users/chris/personal/dotnet-with-claudecode"
Get-Content "$env:USERPROFILE/.wpf-dev-pack-mcp/config.json"
```
Expected: config.json 에 `repoPath`/`branch` 기록, 출력에 경로 확인 메시지.

- [ ] **Step 4: 커밋(체크포인트)**

```bash
git add wpf-dev-pack/skills/set-repo-path wpf-dev-pack/scripts/SetWpfDevPackRepoPath.cs
git commit -m "feat(wpf-dev-pack): add set-repo-path skill + SetWpfDevPackRepoPath file-based app"
```

---

## Task 4: `RepoPathGuard` PreToolUse 훅

**Files:**
- Create: `wpf-dev-pack/hooks/RepoPathGuard.cs`
- Modify: `wpf-dev-pack/hooks/hooks.json`

- [ ] **Step 1: 가드 훅 작성**

`wpf-dev-pack/hooks/RepoPathGuard.cs`:
```csharp
#!/usr/bin/env dotnet

// PreToolUse guard: blocks WpfDevPackMcp tool calls when the knowledge repo
// path is not configured, with a message pointing to /wpf-dev-pack:set-repo-path.
// Input: stdin JSON with "tool_name". Output: PreToolUse permission decision.

using System.Text.Json;

var input = Console.In.ReadToEnd();
if (string.IsNullOrWhiteSpace(input))
{
    return;
}

string? toolName = null;
try
{
    using var doc = JsonDocument.Parse(input);
    if (doc.RootElement.TryGetProperty("tool_name", out var t))
    {
        toolName = t.GetString();
    }
}
catch
{
    return;
}

// Only guard our own MCP server's tools.
if (string.IsNullOrEmpty(toolName) ||
    toolName.IndexOf("WpfDevPackMcp", StringComparison.OrdinalIgnoreCase) < 0)
{
    return;
}

if (IsConfigured())
{
    return; // allow
}

var reason =
    "WpfDevPackMcp is not configured: the knowledge repo path is unset. " +
    "Run /wpf-dev-pack:set-repo-path <path-to-local-clone-of-christian289/dotnet-with-claudecode> first.";

var output = new
{
    hookSpecificOutput = new
    {
        hookEventName = "PreToolUse",
        permissionDecision = "deny",
        permissionDecisionReason = reason,
    },
};
Console.WriteLine(JsonSerializer.Serialize(output));

static bool IsConfigured()
{
    var fromEnv = Environment.GetEnvironmentVariable("WPFDEVPACK_REPO_PATH");
    if (!string.IsNullOrWhiteSpace(fromEnv))
    {
        return true;
    }

    var configPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".wpf-dev-pack-mcp", "config.json");
    if (!File.Exists(configPath))
    {
        return false;
    }

    try
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(configPath));
        return doc.RootElement.TryGetProperty("repoPath", out var p)
            && !string.IsNullOrWhiteSpace(p.GetString());
    }
    catch
    {
        return false;
    }
}
```

> R5: `permissionDecision: "deny"` JSON 포맷은 현행 Claude Code PreToolUse API 기준. 실행 시 실제 차단 동작을 Step 3에서 확인하고, 미동작 시 `exit 2 + stderr` 대안으로 전환.

- [ ] **Step 2: hooks.json 에 등록**

`wpf-dev-pack/hooks/hooks.json` 의 `PreToolUse` 배열에 항목 추가(기존 HandMirror 항목과 병기):
```json
    "PreToolUse": [
      {
        "matcher": "mcp__plugin_context7|mcp__claude_ai_Microsoft_Learn",
        "hooks": [
          {
            "type": "command",
            "command": "dotnet \"${CLAUDE_PLUGIN_ROOT}/hooks/HandMirrorReminder.cs\"",
            "timeout": 5
          }
        ]
      },
      {
        "matcher": "mcp__.*WpfDevPackMcp.*",
        "hooks": [
          {
            "type": "command",
            "command": "dotnet \"${CLAUDE_PLUGIN_ROOT}/hooks/RepoPathGuard.cs\"",
            "timeout": 5
          }
        ]
      }
    ],
```

> matcher 정규식은 실제 도구 prefix(`mcp__WpfDevPackMcp__*` 또는 플러그인 네임스페이스 포함)에 맞춘다. 훅 본문도 `tool_name` 에 `WpfDevPackMcp` 포함 여부를 재확인하므로 이중 안전.

- [ ] **Step 3: 가드 동작 검증(미설정/설정 두 경우)**

Run (PowerShell):
```powershell
# config 제거하여 미설정 상태로
Remove-Item "$env:USERPROFILE/.wpf-dev-pack-mcp/config.json" -ErrorAction SilentlyContinue
$env:WPFDEVPACK_REPO_PATH = $null
'{"tool_name":"mcp__WpfDevPackMcp__list_wpf_topics"}' | dotnet "wpf-dev-pack/hooks/RepoPathGuard.cs"
```
Expected: `permissionDecision":"deny"` JSON 출력(차단 + set-repo-path 안내).
```powershell
$env:WPFDEVPACK_REPO_PATH = "C:/Users/chris/personal/dotnet-with-claudecode"
'{"tool_name":"mcp__WpfDevPackMcp__list_wpf_topics"}' | dotnet "wpf-dev-pack/hooks/RepoPathGuard.cs"
'{"tool_name":"Read"}' | dotnet "wpf-dev-pack/hooks/RepoPathGuard.cs"
```
Expected: 두 경우 모두 출력 없음(허용 / 비대상 도구).

- [ ] **Step 4: 커밋(체크포인트)**

```bash
git add wpf-dev-pack/hooks/RepoPathGuard.cs wpf-dev-pack/hooks/hooks.json
git commit -m "feat(wpf-dev-pack): add RepoPathGuard PreToolUse hook for unconfigured MCP repo path"
```

---

## Task 5: `WpfKeywordDetector` 라우터 분기 (지식→MCP)

**Files:**
- Modify: `wpf-dev-pack/hooks/WpfKeywordDetector.cs` (출력부 + 커맨드 집합)

- [ ] **Step 1: 커맨드 스킬 집합 + 출력 분기로 교체**

`WpfKeywordDetector.cs` 의 출력 블록(현 라인 32–55: `if (skills.Count > 0 || agents.Count > 0)` … 닫는 `}`)을 아래로 교체:
```csharp
// Skill ids that remain real plugin skills (slash-invocable). Everything
// else detected is now an MCP knowledge topic served by WpfDevPackMcp.
var commandSkills = new HashSet<string>(StringComparer.Ordinal)
{
    "make-wpf-project", "make-wpf-custom-control", "make-wpf-usercontrol",
    "make-wpf-converter", "make-wpf-behavior", "make-wpf-viewmodel", "make-wpf-service",
    "collecting-wpf-dev-pack-feedback", "configuring-wpf-dev-pack-language",
    "formatting-wpf-csharp-code", "set-repo-path",
};

if (skills.Count > 0 || agents.Count > 0)
{
    Console.WriteLine("========================================");
    Console.WriteLine("[WPF Dev Pack] Hook Triggered");

    var knowledge = skills.Where(s => !commandSkills.Contains(s)).Take(5).ToList();
    var commands = skills.Where(commandSkills.Contains).ToList();

    foreach (var skill in commands)
    {
        Console.WriteLine($"  -> /wpf-dev-pack:{skill}");
    }

    if (knowledge.Count > 0)
    {
        Console.WriteLine("  WPF knowledge topics (load via MCP before answering):");
        foreach (var id in knowledge)
        {
            Console.WriteLine($"    -> WpfDevPackMcp get_wpf_topic(\"{id}\")");
        }
        Console.WriteLine("    (If WpfDevPackMcp is unconfigured, run /wpf-dev-pack:set-repo-path <path>.)");
    }

    if (agents.Count > 0)
    {
        Console.WriteLine($"  Recommended agents: {string.Join(", ", agents)}");
    }

    Console.WriteLine("========================================");
}
```

> 키워드→id 맵(`DetectKeywordsAndAgents`)은 그대로 둔다 — 라우팅 원천. 이동된 id는 그대로 MCP 토픽 id 와 일치한다.

- [ ] **Step 2: 라우터 출력 검증(지식/커맨드 둘 다)**

Run (PowerShell):
```powershell
'{"prompt":"how do I implement mvvm with observableproperty"}' | dotnet "wpf-dev-pack/hooks/WpfKeywordDetector.cs"
```
Expected: `WpfDevPackMcp get_wpf_topic("implementing-communitytoolkit-mvvm")` 안내(지식 분기).
```powershell
'{"prompt":"서비스 생성 해줘"}' | dotnet "wpf-dev-pack/hooks/WpfKeywordDetector.cs"
```
Expected: `-> /wpf-dev-pack:make-wpf-service` (커맨드 분기 유지).

- [ ] **Step 3: 커밋(체크포인트)**

```bash
git add wpf-dev-pack/hooks/WpfKeywordDetector.cs
git commit -m "feat(wpf-dev-pack): route knowledge keywords to WpfDevPackMcp get_wpf_topic"
```

---

## Task 6: `.mcp.json` 에 WpfDevPackMcp 등록

**Files:**
- Modify: `wpf-dev-pack/.mcp.json`

- [ ] **Step 1: 서버 엔트리 추가**

`wpf-dev-pack/.mcp.json` 전체를 교체:
```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "description": "MCP servers for WPF Dev Pack plugin",
  "mcpServers": {
    "HandMirrorMcp": {
      "type": "stdio",
      "command": "dnx",
      "args": ["HandMirrorMcp@0.1.1", "--yes"]
    },
    "WpfDevPackMcp": {
      "type": "stdio",
      "command": "dnx",
      "args": ["WpfDevPackMcp@0.1.0", "--yes"]
    }
  }
}
```

> 버전 `0.1.0` 은 Plan 1 의 초기 publish 버전과 일치시킨다. 실제 publish 후 핀 버전 갱신(Task 10).

- [ ] **Step 2: JSON 유효성 확인**

Run (PowerShell):
```powershell
Get-Content wpf-dev-pack/.mcp.json -Raw | ConvertFrom-Json | Out-Null; "valid json"
```
Expected: `valid json`.

- [ ] **Step 3: 커밋(체크포인트)**

```bash
git add wpf-dev-pack/.mcp.json
git commit -m "feat(wpf-dev-pack): register WpfDevPackMcp server in .mcp.json"
```

---

## Task 7: `skills/.claude/CLAUDE.md` 슬림 + 트리거 문서 갱신

**Files:**
- Modify: `wpf-dev-pack/skills/.claude/CLAUDE.md`
- Modify: `wpf-dev-pack/.claude/CLAUDE.md`

- [ ] **Step 1: 키워드 테이블을 라우팅 설명으로 대체**

`wpf-dev-pack/skills/.claude/CLAUDE.md` 의 거대한 "Keyword-Skill Mapping" 표를 제거하고, 상단에 다음 설명으로 대체(잔류 커맨드 행만 짧게 유지):
```markdown
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

To add a knowledge topic: create `wpf-dev-pack/knowledge/<id>/SKILL.md`
and add its keyword(s) to `hooks/WpfKeywordDetector.cs`. No plugin skill,
no version bump, no MCP rebuild — the server picks it up on next pull.
```

- [ ] **Step 2: 플러그인 CLAUDE.md Trigger Behavior 갱신**

`wpf-dev-pack/.claude/CLAUDE.md` 의 "Trigger Behavior" / "Adding a New Skill" 절을 갱신:
- "On Trigger … Load appropriate file: SKILL.md/PRISM.md" → 지식은 `WpfDevPackMcp get_wpf_topic(id[, variant])` 로 로드, 커맨드는 슬래시.
- "Adding a New Skill — Required Co-updates" 에 "지식 토픽은 `knowledge/`에 추가 + `WpfKeywordDetector.cs` 키워드 추가(플러그인 스킬 아님)" 항목 추가.
- "Essential (Post-Compact)" 5번 항목 옆에 "WPF 지식은 WpfDevPackMcp 도구로 조회" 한 줄 추가.

- [ ] **Step 3: 검증(스킬 로더가 knowledge 미로드)**

Run (PowerShell):
```powershell
# knowledge 하위에는 SKILL.md 가 있어도 plugin skills 경로가 아니므로 로더 비대상.
Test-Path wpf-dev-pack/skills/implementing-communitytoolkit-mvvm   # Expected: False
Test-Path wpf-dev-pack/knowledge/implementing-communitytoolkit-mvvm/SKILL.md  # Expected: True
```
Expected: False / True.

- [ ] **Step 4: 커밋(체크포인트)**

```bash
git add wpf-dev-pack/skills/.claude/CLAUDE.md wpf-dev-pack/.claude/CLAUDE.md
git commit -m "docs(wpf-dev-pack): slim keyword table, document MCP knowledge routing"
```

---

## Task 8: 루트 CLAUDE.md + `.claude.ko/` 미러

**Files:**
- Modify: `.claude/CLAUDE.md`
- Modify: `.claude.ko/CLAUDE.md`, `wpf-dev-pack/.claude.ko/CLAUDE.md`, `wpf-dev-pack/skills/.claude.ko/CLAUDE.md`

- [ ] **Step 1: 루트 CLAUDE.md 갱신**

`.claude/CLAUDE.md`:
- Directory Layout 코드블록에 `mcp/`(MCP 서버 소스), `wpf-dev-pack/knowledge/`(MCP 지식 토픽) 추가.
- Maintainer Workflow 표에 `/wpf-dev-pack:set-repo-path <path>` 행 추가(plugin scope, "Configure local clone path for WpfDevPackMcp").
- "Plugin Version Update Checklist" 에 "WpfDevPackMcp NuGet 패키지 버전 / `.mcp.json` 핀(지식-only 변경은 bump 불필요)" 명시.

- [ ] **Step 2: 한국어 미러 3종 동기화**

각 `.claude.ko/CLAUDE.md` 미러에 Step 1·Task 7 변경을 동일 의미로 반영(레포 바이링궐 규약: `.claude/` 가 원천, `.claude.ko/` 추종).

- [ ] **Step 3: 미러 존재 확인**

Run (PowerShell):
```powershell
Test-Path .claude.ko/CLAUDE.md, wpf-dev-pack/.claude.ko/CLAUDE.md, wpf-dev-pack/skills/.claude.ko/CLAUDE.md
```
Expected: 모두 True(없으면 생성 후 동기화).

- [ ] **Step 4: 커밋(체크포인트)**

```bash
git add .claude/CLAUDE.md .claude.ko wpf-dev-pack/.claude.ko wpf-dev-pack/skills/.claude.ko
git commit -m "docs: reflect MCP knowledge layout in root CLAUDE.md + Korean mirrors"
```

---

## Task 9: 피드백 적용 워크플로우 갱신

**Files:**
- Modify: `.claude/skills/applying-wpf-dev-pack-feedback/SKILL.md`
- Modify: `.claude/skills/applying-wpf-dev-pack-feedback/hooks/MicrosoftSkillCreatorReminder.cs`

- [ ] **Step 1: Step 3 "변경 종류 → 적용 위치" 표 교체**

`applying-wpf-dev-pack-feedback/SKILL.md` 의 라인 93–99 표를 스펙 §8 의 신표로 교체:
```markdown
| Change kind | Where to apply |
|------|-------|
| New knowledge topic | `wpf-dev-pack/knowledge/<id>/SKILL.md` + add keyword(s) to `wpf-dev-pack/hooks/WpfKeywordDetector.cs` + cross-link adjacent topics |
| Knowledge augmentation | Edit `wpf-dev-pack/knowledge/<id>/SKILL.md` |
| Prism 9 companion (knowledge) | Add `PRISM.md` next to `knowledge/<id>/SKILL.md` |
| New command skill | `wpf-dev-pack/skills/<name>/SKILL.md` + slimmed `skills/.claude/CLAUDE.md` |
| Scaffolder modernization | Update the relevant `make-wpf-*` skill template |
| Rule addition | Add to `wpf-dev-pack/.claude/rules/<rule>.md` |
```

- [ ] **Step 2: Step 4(version bump) 함의 추가**

`applying-wpf-dev-pack-feedback/SKILL.md` Step 4 에 한 단락 추가:
```markdown
> Knowledge-only changes (edits under `wpf-dev-pack/knowledge/`) require
> NO plugin version bump and NO MCP republish — content is served from the
> repo by WpfDevPackMcp and reflected on its next `git pull`. In that case
> skip Step 4 and write `(knowledge only, no plugin/MCP bump)` in the
> APPLIED-LOG `Plugin version` column. Version bumps apply only to changes
> shipped in the plugin (command skills, hooks, rules).
```

- [ ] **Step 3: MicrosoftSkillCreatorReminder 경로 확장**

`MicrosoftSkillCreatorReminder.cs` 의 "새 SKILL.md 작성 감지" 경로 검사가 `wpf-dev-pack/skills/` 뿐 아니라 `wpf-dev-pack/knowledge/` 도 포함하도록 수정. (현 파일에서 경로 문자열 비교/`Contains("wpf-dev-pack/skills")` 부분을 `skills` 또는 `knowledge` 둘 다 매칭하도록 확장.)

- [ ] **Step 4: 훅 동작 검증**

Run (PowerShell): 새 knowledge SKILL.md 작성 시나리오의 stdin(PreToolUse Write payload, `file_path` = `.../wpf-dev-pack/knowledge/foo/SKILL.md`)을 훅에 파이프해 리마인더가 발동하는지 확인.
Expected: 리마인더 출력(또는 비대상 시 무출력).

- [ ] **Step 5: 커밋(체크포인트)**

```bash
git add .claude/skills/applying-wpf-dev-pack-feedback
git commit -m "docs: retarget feedback apply workflow to knowledge base (md, not skills)"
```

---

## Task 10: 배포 — NuGet publish + 버전 핀 + changelog

**Files:**
- Modify: `wpf-dev-pack/.mcp.json` (실제 publish 버전 핀)
- Modify: `docs/changelogs/wpf-dev-pack.md` (via `/wpf-dev-pack-release`)

- [ ] **Step 1: WpfDevPackMcp NuGet publish**

Run (메인테이너 NuGet 자격으로):
```powershell
dotnet pack mcp/WpfDevPackMcp.csproj -c Release -o mcp/nupkg
dotnet nuget push mcp/nupkg/WpfDevPackMcp.0.1.0.nupkg --source https://api.nuget.org/v3/index.json --api-key $env:NUGET_API_KEY
```
Expected: publish 성공.

- [ ] **Step 2: `.mcp.json` 버전 핀 확정**

`WpfDevPackMcp@0.1.0` 가 publish 버전과 일치하는지 확인(불일치 시 수정).

- [ ] **Step 3: 릴리스 스킬로 버전/체인지로그 갱신**

`/wpf-dev-pack-release` 실행 — `plugin.json` version, README 배지 ×2, `docs/changelogs/wpf-dev-pack.md` 엔트리("knowledge skills → WpfDevPackMcp 이전, set-repo-path/RepoPathGuard 추가")를 lockstep 갱신.

- [ ] **Step 4: 로컬 플러그인 재설치 E2E**

Run (CLAUDE 세션):
```
/plugin marketplace remove dotnet-claude-plugins
/plugin marketplace add <이 레포 절대경로>
/plugin install wpf-dev-pack@dotnet-claude-plugins
```
그 후 `/wpf-dev-pack:set-repo-path <레포경로>` → WPF 키워드 프롬프트로 `get_wpf_topic` 자동 안내 → 도구 호출이 토픽을 반환하는지 확인. 미설정 상태에서 도구 호출 시 RepoPathGuard 차단 확인.
Expected: 설정 후 지식 조회 정상, 미설정 시 차단.

- [ ] **Step 5: 커밋(체크포인트)**

```bash
git add wpf-dev-pack/.mcp.json wpf-dev-pack/.claude-plugin/plugin.json wpf-dev-pack/README.md wpf-dev-pack/README.ko.md docs/changelogs/wpf-dev-pack.md
git commit -m "release(wpf-dev-pack): ship knowledge-MCP migration + WpfDevPackMcp registration"
```

---

## Self-Review (작성자 체크)

**Spec 커버리지:**
- §B 콘텐츠 이전(50개) → Task 1 ✅ / 깨진 링크 보정 → Task 2 ✅
- §D set-repo-path + file-based app → Task 3 ✅
- §E RepoPathGuard PreToolUse → Task 4 ✅
- §C 라우터 분기 → Task 5 ✅
- §8 `.mcp.json` → Task 6 ✅ / skills·plugin CLAUDE.md → Task 7 ✅ / 루트 CLAUDE.md + `.claude.ko` → Task 8 ✅
- §8 피드백 워크플로우 + MicrosoftSkillCreatorReminder → Task 9 ✅
- §9 배포/버전/changelog → Task 10 ✅

**플레이스홀더 스캔:** 버전 `0.1.0`/`@0.1.0`·matcher 정규식·NUGET_API_KEY 는 의도된 실값/환경값. 문서 편집 Task(7·8·9)는 "어디를 무엇으로" 를 구체 텍스트로 명시(대규모 CLAUDE.md는 전체 재작성 대신 지정 블록 교체).

**타입/명칭 일관성:** 도구명(`get_wpf_topic` 등)·패키지명(`WpfDevPackMcp`)·경로(`wpf-dev-pack/knowledge/`, `~/.wpf-dev-pack-mcp/config.json`)·커맨드 집합(11개)·env(`WPFDEVPACK_REPO_PATH`)이 Plan 1 및 스펙과 일치.

**미해결(구현 시 확정):** 실제 MCP 도구 prefix(hooks.json matcher), PreToolUse deny 포맷(R5), MicrosoftSkillCreatorReminder 현 구현의 경로 비교 방식 — 해당 Task에서 실물 확인.
