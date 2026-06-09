# WpfDevPackMcp — WPF Dev Pack 지식 MCP 서버

`WpfDevPackMcp` 는 `wpf-dev-pack` 플러그인의 WPF 지식 토픽을 제공하는 작은
**.NET 10 stdio MCP 서버**입니다. 지식 콘텐츠는 `christian289/dotnet-with-claudecode`
의 **로컬 클론** 안 `knowledge/` (레포 루트)에 순수 마크다운으로 존재하며,
서버가 이를 디스크에서 읽고 필요 시 `git pull` 로 갱신합니다. 덕분에 지식이
플러그인의 `skills/` 로더에서 빠져(세션 컨텍스트를 더는 점유하지 않음) 있으면서,
마크다운만 수정하면 갱신됩니다 — **서버 재빌드나 재배포가 필요 없습니다.**

이 프로젝트는 **플러그인 밖**(레포 루트의 `mcp/`)에 있습니다. 플러그인은
`wpf-dev-pack/.mcp.json` 을 통해서만 이 서버를 참조합니다(HandMirrorMcp와 동일한
방식).

## 사전 요건

- .NET SDK **10.0.300+**
- `PATH` 의 `git` (필요 시 갱신용; 저장소는 공개라 인증 불필요)

## 빌드 & 테스트

```
dotnet build mcp/WpfDevPackMcp.csproj
dotnet test  mcp/WpfDevPackMcp.Tests
```

## 서버 빌드/배포

프로젝트는 **framework-dependent·platform-agnostic** .NET tool 입니다
(`PackAsTool=true`). 단일 `dotnet pack`이 모든 OS에서
.NET 10 런타임으로 실행되는 작은 크로스플랫폼 패키지 1개(~1.3 MB, 관리 IL)를 만듭니다:

```
dotnet pack mcp/WpfDevPackMcp.csproj -c Release -o mcp/nupkg
dotnet nuget push mcp/nupkg/WpfDevPackMcp.<ver>.nupkg \
  --source https://api.nuget.org/v3/index.json --api-key <NUGET_KEY>
```

`wpf-dev-pack/.mcp.json` 이 핀 버전으로 실행합니다:

```json
"WpfDevPackMcp": { "type": "stdio", "command": "dnx", "args": ["WpfDevPackMcp@0.1.3", "--yes"] }
```

빌드 산출물(`bin/`, `obj/`, `nupkg/`)은 git-ignore 됩니다.

## NuGet에 새 버전 배포 (메인테이너)

NuGet 버전은 **불변**이라 릴리스마다 새 `<Version>`이 필요합니다. 마지막 단계에서
핀을 갱신하기 전까지 플러그인은 기존 핀 버전으로 계속 동작하므로, **먼저 배포하고
나중에 핀을 갱신**합니다.

**최초 1회 설정**

1. NuGet API 키 발급: nuget.org → 계정 → **API Keys** → **Create**
   (권한: **Push**, glob `WpfDevPackMcp` 또는 `*`). 키 복사(한 번만 표시).
2. nuget.org에서 `WpfDevPackMcp` 패키지 id 사용 가능 여부 확인(첫 push 시 소유권
   등록). 이미 점유돼 있으면 여기 `<PackageId>` **와** `wpf-dev-pack/.mcp.json`의
   `dnx` 인자를 함께 변경.

**릴리스마다**

1. **검증** — `dotnet test mcp/WpfDevPackMcp.Tests` 후, MCP Inspector로 도구 실행
   확인(아래 "MCP Inspector 로 점검" 참조).
2. **버전 올리기** — `mcp/WpfDevPackMcp.csproj`의 `<Version>`(예: `0.1.2` → `0.1.3`).
3. **팩** — `dotnet pack mcp/WpfDevPackMcp.csproj -c Release -o mcp/nupkg`
4. **푸시** —
   ```
   dotnet nuget push mcp/nupkg/WpfDevPackMcp.<ver>.nupkg \
     --source https://api.nuget.org/v3/index.json --api-key <NUGET_KEY>
   ```
5. **대기** — nuget.org 인덱싱(수 분) 후 확인: `dnx WpfDevPackMcp@<ver> --yes`
6. **핀 갱신** — `wpf-dev-pack/.mcp.json`의 `WpfDevPackMcp@<old>` → `WpfDevPackMcp@<ver>`
   (새 버전이 nuget.org에 올라간 뒤에만).

API 키는 비밀 — 절대 커밋 금지. 3~6단계는 메인테이너 작업이고, 지식 콘텐츠 수정은
재배포가 필요 없습니다(저장소에서 실시간 제공).

## 런타임 설정 (필수)

서버는 로컬 클론 위치를 알아야 합니다. 해석 순서:

1. `WPFDEVPACK_REPO_PATH` 환경변수
2. `~/.wpf-dev-pack-mcp/config.json` — `{ "repoPath": "...", "branch": "main" }`

Claude Code 세션에서 설정:

```
/wpf-dev-pack:set-repo-path <로컬-클론-경로>
```

- 둘 다 미설정이면 도구가 "미설정" 오류를 반환합니다 — 플러그인의
  `RepoPathGuard` PreToolUse 훅이 먼저 호출을 차단하고 안내합니다.
- 지정 경로가 비었거나 git 저장소가 아니면, 서버가 최초 사용 시 공개 저장소를
  그 위치로 clone 합니다.
- 선택: `WPFDEVPACK_PULL_TTL_MINUTES`(기본 `60`) — 서비스 전 pull 주기.

## 도구

| 도구 | 설명 |
|------|------|
| `list_wpf_topics()` | 전체 토픽 + 한 줄 요약 + 동반 파일 |
| `get_wpf_topic(id, variant?)` | 전문 마크다운; `variant`: `default` (TOPIC.md) \| `prism` (PRISM.md) \| `advanced` (ADVANCED.md) |
| `search_wpf_topics(query, maxResults?)` | id / title / summary / body 기반 랭킹 검색 |
| `refresh_wpf_knowledge()` | `git pull` + 재스캔 강제 |

토픽 파일: `knowledge/<id>/TOPIC.md` — **YAML frontmatter 없음.**
제목 = 첫 `# H1`, 요약 = 첫 `>` 블록인용. 변형은 형제 `PRISM.md` / `ADVANCED.md`.

## MCP Inspector 로 점검

```
# 빌드 후 빌드 산출물 exe 대상으로 도구 목록 조회
dotnet build mcp/WpfDevPackMcp.csproj -c Release
npx @modelcontextprotocol/inspector --cli \
  "mcp/bin/Release/net10.0/WpfDevPackMcp.exe" --method tools/list

# 도구 호출 (먼저 경로 설정, 또는 ~/.wpf-dev-pack-mcp/config.json 의존)
npx @modelcontextprotocol/inspector --cli "<exe>" \
  --method tools/call --tool-name list_wpf_topics

npx @modelcontextprotocol/inspector --cli "<exe>" \
  --method tools/call --tool-name get_wpf_topic \
  --tool-arg id=implementing-communitytoolkit-mvvm --tool-arg variant=prism
```

`stdout` 에는 MCP JSON-RPC 만 흐르고 모든 로그는 `stderr` 로 갑니다. 서버는 `git`
자식 프로세스에 닫힌 `stdin` 을 주어, 자식이 서버의 JSON-RPC 파이프를 상속해
블록되지 않도록 합니다.

## 지식 콘텐츠 갱신

해당 `knowledge/<id>/TOPIC.md` 를 수정(또는 새 토픽 디렉터리 추가)하고 push 하세요. 서버가
다음 pull 때 반영합니다 — **재빌드·재배포·플러그인 버전업 불필요.**

> 영문 원본: [README.md](https://github.com/christian289/dotnet-with-claudecode/blob/main/mcp/README.md)
