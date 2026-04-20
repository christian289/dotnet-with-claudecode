# Claude Agent Skills 작성 지침

## 1. 핵심 원칙

- **간결함 우선**: Context window는 공유 자원 (시스템 프롬프트, 대화 기록, 다른 Skills 메타데이터와 공유)
- **Claude가 이미 아는 내용은 생략**: "Claude가 정말 이 설명이 필요한가?" 질문할 것
- **적절한 자유도 설정**:
  - High: 텍스트 기반 지침 (여러 접근법이 유효할 때)
  - Medium: 매개변수가 있는 pseudocode (선호 패턴 존재, 변형 허용)
  - Low: 특정 스크립트, 매개변수 최소화 (작업이 취약하고 일관성 중요할 때)

---

## 2. SKILL.md 구조

### Frontmatter 전체 참조

모든 필드는 선택사항. `description`만 권장.

| 필드 | 필수 | 설명 |
|------|------|------|
| `name` | No | 스킬 표시 이름. 생략 시 디렉토리 이름 사용. 소문자, 숫자, 하이픈만 허용 (최대 64자) |
| `description` | 권장 | 스킬의 용도 + 사용 시점. Claude가 자동 호출 판단에 사용. 250자 초과 시 목록에서 잘림 |
| `argument-hint` | No | 자동완성 시 예상 인자 힌트 표시. 예: `[issue-number]`, `[filename] [format]` |
| `disable-model-invocation` | No | `true` 설정 시 Claude 자동 호출 차단 → 사용자 `/name`으로만 실행. 기본값: `false` |
| `user-invocable` | No | `false` 설정 시 `/` 메뉴에서 숨김 → Claude만 호출 가능. 기본값: `true` |
| `allowed-tools` | No | 스킬 활성 시 승인 없이 사용할 수 있는 도구 목록 |
| `model` | No | 스킬 활성 시 사용할 모델 |
| `effort` | No | 스킬 활성 시 effort 수준 (`low`, `medium`, `high`, `max`) |
| `context` | No | `fork` 설정 시 분리된 subagent 컨텍스트에서 실행 |
| `agent` | No | `context: fork` 시 사용할 subagent 타입 |
| `hooks` | No | 스킬 라이프사이클에 스코핑된 hooks |
| `paths` | No | 스킬 활성화를 제한하는 glob 패턴 (YAML 리스트 또는 쉼표 구분 문자열) |
| `shell` | No | `!`command`` 블록에 사용할 셸 (`bash` 기본, `powershell` 가능) |

### 호출 주체별 설정 (Invocation Control)

스킬을 **누가** 호출하느냐에 따라 `disable-model-invocation`과 `user-invocable`을 조합:

| 사용 시나리오 | `disable-model-invocation` | `user-invocable` | 컨텍스트 로딩 |
|--------------|---------------------------|-------------------|---------------|
| **사람 + Claude 모두** (기본값) | 생략 (false) | 생략 (true) | description 항상 로드, 호출 시 전체 로드 |
| **사람만** (deploy, commit 등) | `true` | 생략 (true) | description 미로드, 사용자 `/name` 호출 시만 로드 |
| **Claude만** (배경 지식) | 생략 (false) | `false` | description 항상 로드, Claude 판단 시 전체 로드 |

```yaml
# 사람만: 부작용이 있는 워크플로우 (배포, 커밋, 메시지 전송)
# Human only: Workflows with side effects (deploy, commit, send message)
---
name: deploy
description: Deploy the application to production
disable-model-invocation: true
---

# Claude만: 배경 지식 (사용자가 직접 실행할 의미 없음)
# Claude only: Background knowledge (not meaningful as a user action)
---
name: legacy-system-context
description: Explains legacy auth middleware architecture. Use when modifying auth-related code.
user-invocable: false
---

# 둘 다: 일반적인 스킬 (기본값)
# Both: General skill (default)
---
name: explain-code
description: Explains code with visual diagrams and analogies. Use when explaining how code works.
---
```

### 인자 전달 (Argument Passing)

사용자가 직접 호출하는 스킬에 인자가 필요하면 `argument-hint`를 설정하고, 본문에서 `$0`, `$1`, `$2`... 로 참조:

| 변수 | 설명 |
|------|------|
| `$ARGUMENTS` | 전달된 모든 인자 (전체 문자열) |
| `$ARGUMENTS[N]` | 0-based 인덱스로 특정 인자 접근 |
| `$0`, `$1`, `$2`... | `$ARGUMENTS[N]`의 축약형 |

> ⚠️ 본문에 `$ARGUMENTS`가 없으면 인자가 자동으로 `ARGUMENTS: <value>`로 끝에 추가됨

```yaml
# 단일 인자 예시
# Single argument example
---
name: fix-issue
description: Fix a GitHub issue
disable-model-invocation: true
argument-hint: <issue-number>
---

Fix GitHub issue $0 following our coding standards.

# 복수 인자 예시
# Multiple arguments example
---
name: migrate-component
description: Migrate a component from one framework to another
disable-model-invocation: true
argument-hint: <ComponentName> <from-framework> <to-framework>
---

Migrate the $0 component from $1 to $2.
Preserve all existing behavior and tests.
```

**사용 예시:**
- `/fix-issue 123` → `$0` = `123`
- `/migrate-component SearchBar React Vue` → `$0` = `SearchBar`, `$1` = `React`, `$2` = `Vue`

### description 작성 규칙

- **3인칭 필수**: description은 시스템 프롬프트에 주입됨
  - ✅ "Processes Excel files and generates reports"
  - ❌ "I can help you process Excel files"
  - ❌ "You can use this to process Excel files"
- **구체적으로 작성**: 무엇을 하는지 + 언제 사용하는지 포함

```yaml
# 좋은 예시
# Good example
description: Extract text and tables from PDF files, fill forms, merge documents. Use when working with PDF files or when the user mentions PDFs, forms, or document extraction.

# 나쁜 예시
# Bad example
description: Helps with documents
```

### Skill 콘텐츠 언어 정책

- **SKILL.md 본문, description frontmatter, 코드 예시 주석: 영문 단일**
- 이유: 기존 wpf-dev-pack, avalonia 스킬이 모두 영문으로 통일. 국제 재사용성 및 일관성 유지
- `.claude/rules/preferences.md`의 한글 답변 규칙은 **사용자와의 대화 응답**에만 적용되며, skill 콘텐츠에는 적용되지 않음
- `.claude/rules/dotnet/preferences.md` §2.5의 한글+영문 병기 규칙은 **조건부** (한글을 먼저 쓰는 경우에만 영문 병기). 영문이 기본인 skill 코드 예시에는 영문 단일 주석

---

## 3. 명명 규칙

- **gerund 형식 권장** (동사 + ing):
  - ✅ `processing-pdfs`, `analyzing-spreadsheets`, `managing-databases`
- **대안 허용**:
  - 명사구: `pdf-processing`, `spreadsheet-analysis`
  - 동작 지향: `process-pdfs`, `analyze-spreadsheets`
- **피할 것**:
  - 모호한 이름: `helper`, `utils`, `tools`
  - 과도하게 일반적: `documents`, `data`, `files`
  - 예약어: `anthropic-helper`, `claude-tools`

---

## 4. 점진적 공개 (Progressive Disclosure)

- SKILL.md는 **목차 역할** (500줄 이내 권장)
- 상세 내용은 별도 파일로 분리
- **참조는 1단계 깊이만** (SKILL.md → reference.md ✅, SKILL.md → advanced.md → details.md ❌)

### 디렉토리 구조 예시

```
pdf/
├── SKILL.md              # 메인 지침 (트리거 시 로드)
├── FORMS.md              # 폼 작성 가이드 (필요 시 로드)
├── reference.md          # API 참조 (필요 시 로드)
├── examples.md           # 사용 예시 (필요 시 로드)
├── evals/
│   └── evals.json        # 트리거 평가 데이터 (필수)
└── scripts/
    ├── analyze_form.py   # 유틸리티 스크립트 (실행, 로드 X)
    └── validate.py       # 검증 스크립트
```

### 참조 패턴

```markdown
# SKILL.md

**기본 사용**: [SKILL.md 내 지침]
**고급 기능**: See [advanced.md](advanced.md)
**API 참조**: See [reference.md](reference.md)
**예시**: See [examples.md](examples.md)
```

---

## 5. 워크플로우 패턴

### 복잡한 작업은 체크리스트로 분리

```markdown
## PDF 폼 작성 워크플로우
## PDF Form Filling Workflow

다음 체크리스트를 복사하여 진행 상황을 추적하세요:
Copy this checklist and track your progress:

```
Task Progress:
- [ ] Step 1: 폼 분석 (analyze_form.py 실행)
- [ ] Step 2: 필드 매핑 생성 (fields.json 편집)
- [ ] Step 3: 매핑 검증 (validate_fields.py 실행)
- [ ] Step 4: 폼 작성 (fill_form.py 실행)
- [ ] Step 5: 출력 검증 (verify_output.py 실행)
```
```

### 피드백 루프 구현

- **일반 패턴**: 검증기 실행 → 오류 수정 → 반복
- 검증이 실패하면 이전 단계로 돌아가도록 명시

---

## 6. 공통 패턴

### 템플릿 패턴

```markdown
## 보고서 구조
## Report Structure

ALWAYS use this exact template structure:

```markdown
# [분석 제목]
# [Analysis Title]

## Executive summary
[핵심 발견 사항 한 문단 요약]
[One-paragraph overview of key findings]

## Key findings
- 발견 1 (지원 데이터 포함)
- Finding 1 with supporting data
```
```

### 예시 패턴 (input/output 쌍)

```markdown
## 커밋 메시지 형식
## Commit Message Format

**Example 1:**
Input: Added user authentication with JWT tokens
Output:
```
feat(auth): implement JWT-based authentication

Add login endpoint and token validation middleware
```
```

### 조건부 워크플로우

```markdown
## 문서 수정 워크플로우
## Document Modification Workflow

1. 수정 유형 결정:
1. Determine the modification type:

   **새 콘텐츠 생성?** → "생성 워크플로우" 따르기
   **Creating new content?** → Follow "Creation workflow" below
   **기존 콘텐츠 편집?** → "편집 워크플로우" 따르기
   **Editing existing content?** → Follow "Editing workflow" below
```

---

## 7. 피해야 할 패턴

| 패턴 | 문제 | 해결책 |
|------|------|--------|
| Windows 경로 (`\`) | Unix에서 오류 발생 | 항상 `/` 사용 |
| 시간 의존적 정보 | 곧 outdated 됨 | "old patterns" 섹션 사용 |
| 너무 많은 옵션 | 혼란 유발 | 기본값 제공 + 대안 하나만 |
| 깊은 참조 중첩 | 부분 읽기 위험 | 1단계 깊이만 |
| 도구 설치 가정 | 실행 실패 | 명시적으로 설치 지침 포함 |

### 옵션 제시 예시

```markdown
# 나쁜 예시 (너무 많은 선택지)
# Bad example (too many choices)
"pypdf, pdfplumber, PyMuPDF, pdf2image 중 하나 사용..."
"You can use pypdf, or pdfplumber, or PyMuPDF, or pdf2image..."

# 좋은 예시 (기본값 + 대안)
# Good example (default + alternative)
"텍스트 추출에는 pdfplumber 사용:
"Use pdfplumber for text extraction:
```python
import pdfplumber
```
OCR이 필요한 스캔된 PDF는 pdf2image와 pytesseract 사용."
For scanned PDFs requiring OCR, use pdf2image with pytesseract instead."
```

---

## 8. 실행 가능한 코드 포함 시

### 에러 처리 명시

```python
def process_file(path):
    """파일 처리, 없으면 생성"""
    """Process a file, creating it if it doesn't exist."""
    try:
        with open(path) as f:
            return f.read()
    except FileNotFoundError:
        # 실패하지 않고 기본 콘텐츠로 파일 생성
        # Create file with default content instead of failing
        print(f"File {path} not found, creating default")
        with open(path, 'w') as f:
            f.write('')
        return ''
```

### 매직 넘버 금지

```python
# 좋은 예시: 자체 문서화
# Good example: Self-documenting
REQUEST_TIMEOUT = 30  # HTTP 요청은 보통 30초 내 완료
                      # HTTP requests typically complete within 30 seconds
MAX_RETRIES = 3       # 3회 재시도로 안정성과 속도 균형
                      # Three retries balances reliability vs speed

# 나쁜 예시: 매직 넘버
# Bad example: Magic numbers
TIMEOUT = 47  # 왜 47? / Why 47?
```

### 검증 가능한 중간 출력

- **패턴**: 계획 생성 → 계획 검증 → 실행 → 확인
- 검증 스크립트는 구체적인 오류 메시지 제공

---

## 9. MCP 도구 참조

- **완전한 도구 이름 사용**: `ServerName:tool_name`

```markdown
Use the BigQuery:bigquery_schema tool to retrieve table schemas.
Use the GitHub:create_issue tool to create issues.
```

---

## 10. 체크리스트

### 핵심 품질

- [ ] description이 구체적이고 키 용어 포함
- [ ] description에 무엇을 하는지 + 언제 사용하는지 포함 (250자 이내 권장)
- [ ] SKILL.md 본문 500줄 이내
- [ ] 추가 세부사항은 별도 파일로 분리
- [ ] 시간 의존적 정보 없음 (또는 "old patterns" 섹션에)
- [ ] 일관된 용어 사용
- [ ] 예시가 구체적 (추상적 X)
- [ ] 파일 참조 1단계 깊이
- [ ] 점진적 공개 적절히 사용
- [ ] 워크플로우에 명확한 단계

### 호출 제어 (Invocation Control)

- [ ] 호출 주체 결정: 사람만 / Claude만 / 둘 다
- [ ] 부작용 있는 스킬에 `disable-model-invocation: true` 설정
- [ ] 배경 지식 스킬에 `user-invocable: false` 설정
- [ ] 사용자 호출 스킬에 인자 필요 시 `argument-hint` 설정
- [ ] 인자 참조 시 `$0`, `$1`, `$2`... 축약형 사용

### 코드 및 스크립트

- [ ] 스크립트가 Claude에 떠넘기지 않고 문제 해결
- [ ] 에러 처리 명시적이고 도움됨
- [ ] 매직 넘버 없음 (모든 값 정당화)
- [ ] 필수 패키지 지침에 나열
- [ ] Windows 경로 없음 (모두 `/`)
- [ ] 중요 작업에 검증/확인 단계
- [ ] 품질 중요 작업에 피드백 루프

---

## 11. 공식 문서

- [Skills Overview](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/overview)
- [Skills Quickstart](https://platform.claude.com/docs/en/agents-and-tools/agent-skills/quickstart)
- [Skills in Claude Code](https://code.claude.com/docs/en/skills)
