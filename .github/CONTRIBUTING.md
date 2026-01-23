# Contributing to dotnet-with-claudecode

기여해 주셔서 감사합니다! 이 문서는 프로젝트에 기여하는 방법을 안내합니다.

## How to Contribute

### 1. Fork & Clone

```bash
# Fork 후 클론
git clone https://github.com/YOUR_USERNAME/dotnet-with-claudecode.git
cd dotnet-with-claudecode
```

### 2. Branch 생성

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

**커밋 메시지 규칙:**
- `feat:` - 새로운 기능
- `fix:` - 버그 수정
- `docs:` - 문서 변경
- `refactor:` - 리팩토링
- `chore:` - 기타 변경

### 4. Pull Request 생성

1. 본인의 fork에 push
2. 원본 저장소로 Pull Request 생성
3. PR 템플릿에 따라 설명 작성

## Guidelines

### Skill 작성 시

- `SKILL.md`는 500줄 이내로 유지
- description은 3인칭으로 작성
- 예시 코드는 실제 동작하는 코드로 작성
- 한글/영문 병기 (코멘트, 예외 메시지 등)

### Agent 작성 시

- 명확한 역할 정의
- 적절한 모델 티어 선택 (haiku/sonnet/opus)
- 다른 에이전트와의 중복 최소화

## Code of Conduct

[CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md)를 준수해주세요.

## Questions?

질문이 있으시면 [Issues](https://github.com/christian289/dotnet-with-claudecode/issues)에 등록해주세요.
