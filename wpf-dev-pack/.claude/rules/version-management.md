# wpf-dev-pack 버전 관리 규칙

## 자동 버전 업데이트

Claude Code가 `git push`를 실행하기 전에 다음 규칙을 따릅니다.

---

## 1. 버전 업데이트 조건

wpf-dev-pack 디렉토리 내 파일이 변경되었고, 다음 조건 중 하나라도 해당되면 버전 업데이트:

- plugin.json의 version이 마지막 태그/릴리스와 동일
- 커밋 메시지에 `[skip-version]`이 없음

---

## 2. 버전 업데이트 규칙

| 커밋 메시지 지시자 | 동작 |
|-------------------|------|
| `[major]` | MAJOR 증가 (1.2.1 → 2.0.0) |
| `[minor]` | MINOR 증가 (1.2.1 → 1.3.0) |
| `[patch]` 또는 지시자 없음 | PATCH 증가 (1.2.1 → 1.2.2) |
| `[skip-version]` | 버전 업데이트 건너뜀 |

---

## 3. 버전 동기화 파일

다음 파일들의 버전을 동시에 업데이트:

1. `.claude-plugin/plugin.json` - `"version": "X.Y.Z"`
2. `README.md` - `version-X.Y.Z-blue.svg` 배지

---

## 4. 자동 커밋

버전 업데이트 후 자동으로 커밋 생성:

```
chore(wpf-dev-pack): bump version to X.Y.Z
```

---

## 5. 실행 순서

1. `git push` 명령 감지
2. wpf-dev-pack 변경사항 확인 (`git diff --name-only origin/main...HEAD`)
3. 버전 업데이트 필요 여부 판단
4. plugin.json, README.md 버전 수정
5. 버전 커밋 생성
6. push 실행

---

## 6. Hook 구현

자동 버전 업데이트는 다음 파일들로 구현됨:

| 파일 | 역할 |
|------|------|
| `dotnet-with-claudecode/.claude/settings.json` | PreToolUse hook 설정 |
| `dotnet-with-claudecode/.claude/hooks/pre-git-push.ps1` | git push 감지 wrapper |
| `dotnet-with-claudecode/.claude/hooks/bump-wpf-dev-pack-version.ps1` | 버전 업데이트 로직 |

---

## 7. 예외 상황

버전 업데이트를 건너뛰는 경우:

- 커밋 메시지에 `[skip-version]` 포함
- wpf-dev-pack 외부 파일만 변경된 경우
- 버전 파일만 변경된 경우 (버전 충돌 방지)
