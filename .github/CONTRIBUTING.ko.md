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

## 행동 강령

[CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md)를 준수해주세요.

## 문의

질문이 있으면 [Issue](https://github.com/christian289/dotnet-with-claudecode/issues)를 열어주세요.
