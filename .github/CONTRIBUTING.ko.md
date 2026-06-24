[🇺🇸 English](./CONTRIBUTING.md)

# dotnet-with-claudecode 기여 가이드

기여에 관심 가져주셔서 감사합니다! 이 문서는 프로젝트 기여 방법을 설명합니다.

## 기여 방법

### 1. Fork & Clone

```bash
# Fork 후 클론
git clone https://github.com/YOUR_USERNAME/dotnet-with-claudecode.git
cd dotnet-with-claudecode
```

### 2. 브랜치 생성

```bash
git checkout -b feature/your-feature-name
# 또는
git checkout -b fix/your-bug-fix
```

### 3. 변경 사항 커밋

```bash
git add .
git commit -m "feat: add new skill for XYZ"
```

**커밋 메시지 컨벤션:**
- `feat:` - 새 기능
- `fix:` - 버그 수정
- `docs:` - 문서 변경
- `refactor:` - 코드 리팩토링
- `chore:` - 기타 변경

**PR을 열기 전에 squash하세요.** 이 저장소는 단계별 다수 커밋보다 의미 있는
소수의 커밋을 선호합니다. 리뷰 요청 전에 히스토리를 정리(squash)해주세요.

### 4. Pull Request 생성

1. 본인 fork에 push
2. 원본 저장소로 Pull Request 생성
3. PR 템플릿에 설명 작성

### 5. 피드백 문서 (`FeedbackDocs/`)

`wpf-dev-pack`은 이 저장소 **밖**에 있는 실제 WPF 프로젝트에서 사용됩니다.
가장 가치 있는 개선은 그 작업 중에 얻은 교훈에서 나옵니다 — 손으로 고쳐야
했던 안티패턴, 누락되었거나 구식인 스킬 안내, 잘못 트리거되거나 트리거되지
않은 경우, 스캐폴더 부족분 등.

이 작업은 `wpf-dev-pack`을 직접 수정하기 어려운 외부 세션에서 일어나므로,
피드백을 문서로 남깁니다:

1. WPF 작업 세션에서 `wpf-dev-pack`을 사용한 뒤, 사용자 호출 스킬
   `/wpf-dev-pack:collecting-wpf-dev-pack-feedback`을 실행합니다.
2. 스킬이 해당 세션을 분석하여 현재 디렉토리에
   `<topic>-wpf-dev-pack-feedback.md`를 생성합니다.
   git을 건드리지 않으며 이 저장소를 찾지도 않습니다.
3. 생성된 md 파일을 이 저장소의 `FeedbackDocs/` 폴더로 옮기고(파일명 유지)
   Pull Request를 엽니다.

이 문서들은 `FeedbackDocs/`에 코퍼스로 누적되며, 메인테이너가 이후 세션에서
구체적인 `wpf-dev-pack` 변경으로 분류·반영합니다.

**FeedbackDocs 규칙:**
- 세션/주제당 파일 1개, `<topic>-wpf-dev-pack-feedback.md` 형식.
- 문서 본문 언어는 제한하지 않습니다 — 한글, 영문, 혼용 모두 무방합니다.
  스킬이 문서 구조를 자동으로 생성합니다.
- 본인 문서만 추가하고 타인의 피드백 문서는 수정·삭제하지 않습니다.

**개인을 특정할 수 있는 정보 금지.** 피드백 문서는 여러 프로젝트에 걸쳐
재사용되는 산출물이므로, 기술적 현상과 그 인과관계만 서술해야 합니다.
다음 정보는 절대 포함하지 마세요:

- 프로젝트, 솔루션, 리포지토리, 제품, 코드네임 이름
- 팀 / 개발자 / 사용자 이름, 이메일, 계정 핸들
- 이슈가 발생한 날짜·시각
- 원 코드베이스의 절대 경로 또는 리포지토리 상대 경로
- 원 프로젝트에 고유한 클래스 / 네임스페이스 / 멤버명
  (`XxxView`, `XxxViewModel`, `IXxxService` 같은 중립 placeholder로 치환)

공개 프레임워크 / 라이브러리 / API 이름(`HelixToolkit`, `ScottPlot`,
`CommunityToolkit.Mvvm`, `Prism`, `DispatcherPriority.ApplicationIdle` 등)은
기술 컨텍스트의 일부이므로 허용됩니다.

**PR 제출 전 셀프 리뷰.** push 직전에 문서를 한 번 더 읽으며 다음 항목을
직접 확인하세요:

- [ ] 프로젝트 / 솔루션 / 리포지토리 / 제품 / 코드네임 이름이 없는가
- [ ] 팀 / 개발자 / 사용자 이름·이메일·핸들이 없는가
- [ ] 원래 이슈가 발생한 날짜·시각이 없는가
- [ ] 원 코드베이스의 절대 경로·리포지토리 상대 경로가 없는가
- [ ] 프로젝트 고유 클래스 / 네임스페이스 / 멤버명이 모두 중립
      placeholder로 치환되었는가
- [ ] 각 항목이 특정 프로젝트의 사건이 아니라 Phenomenon → Cause → Effect
      형태의 일반적인 기술 인과 체인으로 기술되어 있는가

위 중 하나라도 실패하면 먼저 문서를 수정한 뒤 PR을 제출하세요.

**PR이 머지된 이후.** 메인테이너는 이 저장소의
`/applying-wpf-dev-pack-feedback` 스킬을 통해 피드백을 반영합니다. 이
스킬은 각 항목을 순차적으로 플러그인에 적용한 뒤, 피드백 문서를
`FeedbackDocs/` 안으로 이동시키고, `FeedbackDocs/APPLIED-LOG.md`에
적용 내역(전부 적용 / 부분 적용 / 거부)과 반영 커밋을 기록하는 한 줄을
추가합니다. 기여자는 Applied Log를 직접 작성하지 않습니다.

## 가이드라인

### 스킬 작성 시

- `SKILL.md`는 500줄 이내로 유지
- description은 3인칭으로 작성
- 동작하는 예제 코드 포함
- 프로젝트 코딩 컨벤션 준수

### 에이전트 작성 시

- 명확한 책임 정의
- 적절한 모델 등급 선택 (haiku/sonnet/opus)
- 다른 에이전트와의 중복 최소화

### wpf-dev-pack 구성요소 추가

> 이 레시피들은 이전에 플러그인 내부 `CLAUDE.md` 파일에 있었습니다. 해당 파일들은 설치 사용자에게
> 로드되지 않아 제거되었고, 이 레포 메인테이너용 지침이므로 여기로 옮겼습니다.

**지식 토픽 추가** (WpfDevPackMcp MCP로 제공되는 WPF 지식 — 플러그인 스킬이 *아님*):

1. 레포 루트의 `knowledge/<id>/TOPIC.md`를 생성합니다(플러그인 외부이므로 번들되지 않음).
   **YAML frontmatter 없음.** 첫 번째 `# H1`이 제목이고, H1 바로 아래에 한 줄짜리 `> summary`
   blockquote를 둡니다 — MCP 카탈로그(`TopicDocReader`)가 첫 H1에서 제목을, 첫 `>` blockquote에서
   요약을 읽습니다.
2. 라우터 수정·플러그인 스킬 등록·플러그인 버전 범프·MCP 재배포 모두 불필요 — MCP 카탈로그가 새
   디렉터리를 자동 발견하고 `search_wpf_topics`가 다음 `git pull` 시 노출합니다(서버가 지식 콘텐츠를
   실시간으로 읽음).

**커맨드 스킬 추가** (`skills/` 아래 슬래시 호출 플러그인 스킬):

`skills/<skill-name>/SKILL.md`를 추가할 때 함께:

1. **인접 SKILL.md 상호 링크** — 토픽이 겹치면 새 스킬로의 상호 링크를 추가합니다
   (`See [...](../skill-name/SKILL.md)`). (스킬은 `description`으로 자동 발견되므로 수동 스킬
   레지스트리를 갱신할 필요가 없습니다.)
2. Prism 9 분기가 필요한 스킬은 **`PRISM.md` 컴패니언**을 작성합니다(MVVM 프레임워크 지식 토픽 참고).
3. **Foundation + Application 스킬 쌍** — 두 스킬을 별도로 작성하고 상호 참조합니다. Foundation
   스킬은 메커니즘/일반 원칙을, Application 스킬은 특정 시나리오 적용을 설명합니다(예:
   `preventing-dispatcher-deadlock` + `shutting-down-wpf-gracefully`).

**WPF 룰 추가.** 상세 WPF 작성 룰은 `skills/wpf-rule-<name>/SKILL.md`에 preload용
스킬로 배포합니다(`user-invocable: false` — `/` 메뉴에선 숨기되 Claude가 호출 가능해야
preload되므로 `disable-model-invocation`은 설정하지 마세요; 설정하면 preload가 막힙니다).
에이전트는 `skills:` frontmatter로 필요한 룰을 가져오며, 이는 서브에이전트 시작 시 스킬
전체 내용을 결정적으로 주입합니다(플러그인 에이전트는 `hooks`를 무시하고 `.claude/rules`는
자동 로드되지 않음). 룰 추가 절차: `wpf-rule-<name>` 스킬을 만들고, 이를 적용해야 하는 각
에이전트의 `skills:` 목록에 이름을 추가합니다.

> 새 커맨드 스킬이나 훅을 추가하면 번들 플러그인 콘텐츠가 바뀌므로, 작업을 마친 뒤
> `/wpf-dev-pack-release`로 버전을 범프하세요. `knowledge/` 아래 지식 전용 편집은 버전 범프가
> 필요 없습니다. 루트 `.claude/CLAUDE.md`의 "Plugin Version Update Checklist"를 참고하세요.

## 행동 강령

[CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md)를 준수해주세요.

## 문의

질문이 있으면 [Issue](https://github.com/christian289/dotnet-with-claudecode/issues)를 열어주세요.
