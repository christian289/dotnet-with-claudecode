# 기본 지침

## 1. 응답 언어

- **기본 응답 언어 = 영어 (English)**
- 영문 응답 시: 친절하고 정중한 문체로 작성
- 한글 번역 요청 시: 최대한 정확하고 오해가 없는 명료한 한글로 번역
- 한글 작문 요청 시: 최대한 정중하고 상냥한 문체로 작성

### 1.1 우선순위 (높은 순)

1. **현재 대화의 사용자 메시지 언어** — 사용자가 한글/일문/중문 등 다른
   언어로 질문하면 같은 언어로 응답하고, 동일 세션에서는 그 언어 유지
2. **`.claude/wpf-dev-pack.local.md`의 `language:` 필드** — wpf-dev-pack
   플러그인이 설치된 환경에서 `LanguagePreferenceLoader` SessionStart
   훅이 세션 시작 시 주입하는 지시문. 자세한 내용은
   `wpf-dev-pack/.claude/CLAUDE.md`의 "Per-Project Language Preference"
   섹션 및 `/wpf-dev-pack:configuring-wpf-dev-pack-language` 스킬 참조
3. 본 규칙의 기본값 (영어)

사용자가 명시적으로 전환을 요청하면("respond in English" / "한글로
답해줘") 즉시 전환하고 이후 같은 세션은 새 언어 유지.

### 1.2 산출물 언어 정책 (별도)

본 규칙은 **사용자와의 대화 응답**에만 적용됨. 다음 산출물에는 별도
정책이 우선함:

- **Skill 본문(SKILL.md), description frontmatter, 코드 예시 주석**:
  영문 단일. 자세한 내용은
  `.claude/rules/claude-skills/best-practices.md`의
  "Skill 콘텐츠 언어 정책" 서브섹션 참조
- **한글+영문 병기**: 한글을 먼저 작성한 경우에만 조건부 적용. 자세한
  내용은 `.claude/rules/dotnet/preferences.md` §2.5 참조

## 2. 도구 추천

- 프로그래밍을 할 때 질문자가 필요해 보이는 도구가 있다고 판단하면 도구를 적극 추천하고 기본적인 사용법을 남겨줄 것

## 3. 지식 검증

- 지식을 열거할 때 정확한 지식인지 답변을 마무리할 때 검증과정을 반드시 거칠 것
