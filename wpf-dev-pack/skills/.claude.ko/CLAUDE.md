# WPF Dev Pack Skills — 라우팅

> 본 파일은 `wpf-dev-pack/skills/.claude/CLAUDE.md`의 한국어 미러입니다.
> AI는 영문 원본을 읽으며, 본 파일은 사람을 위한 참고용입니다.

---

## 개요

지식 토픽은 **더 이상 플러그인 스킬이 아닙니다**. `knowledge/<id>/`(레포 루트)에
위치하며 `WpfDevPackMcp` MCP 서버가 제공합니다. 키워드 라우터 훅은 없습니다:
MCP 서버 자체의 instructions가 에이전트에게 WPF 질문에 답하기 전
`search_wpf_topics`(라이브 카탈로그 랭킹 검색)로 찾아 `get_wpf_topic`으로
로드하도록 지시합니다. 새 토픽은 자동 발견되어 등록할 것이 없습니다.

`skills/` 아래에는 커맨드 스킬만 남으며 슬래시 명령으로 호출합니다:

| 키워드 (의도) | 커맨드 스킬 |
|---|---|
| `create viewmodel`, `뷰모델 생성` | `make-wpf-viewmodel` |
| `create service`, `서비스 생성` | `make-wpf-service` |
| (명시적) | `make-wpf-project`, `make-wpf-custom-control`, `make-wpf-usercontrol`, `make-wpf-converter`, `make-wpf-behavior` |
| (자동) C#/XAML 서식 | `formatting-wpf-csharp-code` |
| `feedback` (메인테이너) | `collecting-wpf-dev-pack-feedback` |
| `language` | `configuring-wpf-dev-pack-language` |
| `repo path`, MCP 미설정 | `set-repo-path` |

지식 토픽 추가 방법: `knowledge/<id>/TOPIC.md`를 생성합니다.
**YAML frontmatter 없음** — 첫 번째 `# H1`이 제목이며, H1 바로 아래에
한 줄짜리 `> 요약` 블록인용을 작성합니다(MCP 카탈로그가 본문에서
제목·요약을 읽음). 라우터 수정 불필요, 플러그인 스킬 등록 불필요,
버전 범프 불필요, MCP 재빌드 불필요 — 카탈로그가 자동 발견하고
서버는 다음 pull 시 반영합니다.

---

## HandMirror MCP - .NET API 검증

.NET API/NuGet 패키지 정보를 조회할 때, **HandMirrorMcp 도구도 함께
사용**하여 환각을 줄입니다.

**트리거 조건**: .NET/NuGet 관련 조회로 context7 또는 Microsoft Learn
MCP를 사용할 때

**공동 사용 규칙:**

```
WHEN context7 또는 Microsoft Learn으로 .NET/NuGet 정보 조회:
  ALSO HandMirrorMcp 사용해 검증:
    - inspect_nuget_package: NuGet 패키지 내 namespace/type 목록
    - inspect_nuget_package_type: 정확한 메서드 시그니처 조회
    - search_nuget_packages: 키워드로 패키지 검색
    - get_type_info: 로컬 assembly (.dll/.exe) 타입 조사
    - explain_build_error: CS/NU 빌드 에러 진단
    - analyze_csproj: 프로젝트 파일 이슈 분석
```

**사용 시나리오:**
- NuGet 패키지의 API 이름 케이싱 정확성 검증 (예: SQLite vs Sqlite)
- 확장 메서드의 정확한 namespace 식별
- 패키지 버전 간 breaking change 점검
- 빌드 에러(CS0246, NU1605 등) 진단 및 필수 패키지 권고
