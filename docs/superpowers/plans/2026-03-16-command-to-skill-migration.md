# Command-to-Skill Migration Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Claude Code 2.1.3에서 command가 agent skill 2.0으로 통합됨에 따라, wpf-dev-pack의 5개 command를 skills로 마이그레이션한다.

**Architecture:** `commands/` 디렉토리의 5개 command(make-wpf-project, make-wpf-custom-control, make-wpf-usercontrol, make-wpf-converter, make-wpf-behavior)를 `skills/` 디렉토리로 이동. 각 SKILL.md의 frontmatter는 이미 skill 호환 형식(name, description)이므로 내용 변경 없이 이동만 수행. commands/ 디렉토리는 삭제하고, README 및 CLAUDE.md의 "5 Commands" 카운트와 관련 참조를 업데이트한다.

**Tech Stack:** Claude Code Plugin (skills, agents, hooks)

---

## File Structure

### 이동 대상 (commands/ → skills/)

| Source | Destination |
|--------|-------------|
| `commands/make-wpf-project/SKILL.md` | `skills/make-wpf-project/SKILL.md` |
| `commands/make-wpf-project/PRISM.md` | `skills/make-wpf-project/PRISM.md` |
| `commands/make-wpf-custom-control/SKILL.md` | `skills/make-wpf-custom-control/SKILL.md` |
| `commands/make-wpf-usercontrol/SKILL.md` | `skills/make-wpf-usercontrol/SKILL.md` |
| `commands/make-wpf-converter/SKILL.md` | `skills/make-wpf-converter/SKILL.md` |
| `commands/make-wpf-behavior/SKILL.md` | `skills/make-wpf-behavior/SKILL.md` |

### 삭제 대상

| Path | Reason |
|------|--------|
| `commands/` 디렉토리 전체 | skills로 마이그레이션 완료 |

### 수정 대상

| File | Change |
|------|--------|
| `wpf-dev-pack/README.md` | "5 Commands" 제거, skill 카운트 62→67, 구조도 업데이트 |
| `wpf-dev-pack/README.ko.md` | 동일 한국어 변경 |
| `wpf-dev-pack/.claude/CLAUDE.md` | command 관련 참조 정리 |
| `wpf-dev-pack/skills/.claude/CLAUDE.md` | 신규 skill 5개 키워드 매핑 추가 |
| `.claude/CLAUDE.md` | 변경 이력 추가 |

---

## Chunk 1: Command 파일 이동 및 commands/ 디렉토리 삭제

### Task 1: commands/ → skills/ 파일 이동

**Files:**
- Move: `wpf-dev-pack/commands/make-wpf-project/` → `wpf-dev-pack/skills/make-wpf-project/`
- Move: `wpf-dev-pack/commands/make-wpf-custom-control/` → `wpf-dev-pack/skills/make-wpf-custom-control/`
- Move: `wpf-dev-pack/commands/make-wpf-usercontrol/` → `wpf-dev-pack/skills/make-wpf-usercontrol/`
- Move: `wpf-dev-pack/commands/make-wpf-converter/` → `wpf-dev-pack/skills/make-wpf-converter/`
- Move: `wpf-dev-pack/commands/make-wpf-behavior/` → `wpf-dev-pack/skills/make-wpf-behavior/`
- Delete: `wpf-dev-pack/commands/` (전체 디렉토리 — README.md, README.ko.md, .claude/ 포함)

- [ ] **Step 1: git mv로 5개 command 디렉토리를 skills/로 이동**

```bash
cd wpf-dev-pack
git mv commands/make-wpf-project skills/make-wpf-project
git mv commands/make-wpf-custom-control skills/make-wpf-custom-control
git mv commands/make-wpf-usercontrol skills/make-wpf-usercontrol
git mv commands/make-wpf-converter skills/make-wpf-converter
git mv commands/make-wpf-behavior skills/make-wpf-behavior
```

- [ ] **Step 2: commands/ 디렉토리의 남은 파일 삭제**

`commands/README.md`, `commands/README.ko.md`, `commands/.claude/CLAUDE.md`는 더 이상 필요 없다.

```bash
git rm commands/README.md
git rm commands/README.ko.md
git rm commands/.claude/CLAUDE.md
# commands/ 디렉토리는 비어있으므로 git이 자동 삭제
```

- [ ] **Step 3: 이동된 파일 확인**

```bash
ls wpf-dev-pack/skills/make-wpf-project/
# Expected: SKILL.md PRISM.md
ls wpf-dev-pack/skills/make-wpf-custom-control/
# Expected: SKILL.md
ls wpf-dev-pack/skills/make-wpf-converter/
# Expected: SKILL.md
ls wpf-dev-pack/skills/make-wpf-behavior/
# Expected: SKILL.md
ls wpf-dev-pack/skills/make-wpf-usercontrol/
# Expected: SKILL.md
```

- [ ] **Step 4: make-wpf-custom-control SKILL.md에서 command 전용 frontmatter 정리**

`make-wpf-custom-control/SKILL.md`에만 있는 `disable-model-invocation: true`와 `argument-hint:` 필드를 제거한다. 이는 command 전용 필드이며 skill에서는 사용하지 않는다.

현재:
```yaml
---
name: make-wpf-custom-control
description: WPF CustomControl generation wizard. Specify control name and base class to auto-generate C# class and XAML style.
disable-model-invocation: true
argument-hint: [ControlName] [BaseClass]
---
```

변경:
```yaml
---
name: make-wpf-custom-control
description: WPF CustomControl generation wizard. Specify control name and base class to auto-generate C# class and XAML style.
---
```

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "refactor(wpf-dev-pack): command를 skill로 마이그레이션 (Claude Code 2.1.3 agent skill 2.0)"
```

---

## Chunk 2: README.md / README.ko.md 업데이트

### Task 2: README.md 업데이트

**Files:**
- Modify: `wpf-dev-pack/README.md`

변경 목록:
1. **헤더 카운트**: `**62 Skills** · **11 Specialized Agents** · **5 Commands** · **1 MCP Server**` → `**67 Skills** · **11 Specialized Agents** · **1 MCP Server**` (5 Commands 제거, skill에 5개 추가)
2. **Quick Start 섹션**: `/wpf-dev-pack:make-wpf-*` 슬래시 커맨드 사용법은 동일하게 유지 (skill도 `/plugin:skill-name` 으로 호출 가능)
3. **Plugin Structure 섹션**: `commands/` 디렉토리 항목 제거, skills 카운트 62→67 업데이트
4. **Skills by Category**: "Scaffolding" 카테고리 추가 (5개 make-wpf-* skills)

- [ ] **Step 1: 헤더 카운트 업데이트**

`**62 Skills** · **11 Specialized Agents** · **5 Commands** · **1 MCP Server**`
→ `**67 Skills** · **11 Specialized Agents** · **1 MCP Server**`

- [ ] **Step 2: Plugin Structure에서 commands/ 항목 제거, skills 카운트 업데이트**

```
# Before
├── 📁 commands/               # 5 User commands
│   ├── make-wpf-custom-control/
│   ├── make-wpf-project/
│   ├── make-wpf-converter/
│   ├── make-wpf-behavior/
│   └── make-wpf-usercontrol/
├── 📁 skills/                 # 62 Skills

# After
├── 📁 skills/                 # 67 Skills
```

- [ ] **Step 3: Skills by Category에 Scaffolding 카테고리 추가**

기존 카테고리 목록 끝(`.NET Common` 다음)에 추가:

```markdown
<details>
<summary><b>🏗️ Scaffolding (5 skills) — migrated from Commands</b></summary>

| Skill | Description |
|-------|-------------|
| `make-wpf-project` | WPF project scaffolding with MVVM/DI |
| `make-wpf-custom-control` | CustomControl generation |
| `make-wpf-usercontrol` | UserControl generation |
| `make-wpf-converter` | IValueConverter generation |
| `make-wpf-behavior` | Behavior<T> generation |

</details>
```

### Task 3: README.ko.md 업데이트

**Files:**
- Modify: `wpf-dev-pack/README.ko.md`

README.md와 동일한 변경사항을 한국어로 적용:

- [ ] **Step 1: 헤더 카운트 업데이트**

`**62개 스킬** · **11개 전문 에이전트** · **5개 명령어** · **1개 MCP 서버**`
→ `**67개 스킬** · **11개 전문 에이전트** · **1개 MCP 서버**`

- [ ] **Step 2: Plugin Structure 업데이트 (한국어)**

```
# Before
├── 📁 commands/               # 5개 사용자 명령어
├── 📁 skills/                 # 62개 스킬

# After
├── 📁 skills/                 # 67개 스킬
```

- [ ] **Step 3: Skills by Category에 스캐폴딩 카테고리 추가 (한국어)**

```markdown
<details>
<summary><b>🏗️ 스캐폴딩 (5개 스킬) — Commands에서 마이그레이션</b></summary>

| 스킬 | 설명 |
|------|------|
| `make-wpf-project` | MVVM/DI 포함 WPF 프로젝트 스캐폴딩 |
| `make-wpf-custom-control` | CustomControl 생성 |
| `make-wpf-usercontrol` | UserControl 생성 |
| `make-wpf-converter` | IValueConverter 생성 |
| `make-wpf-behavior` | Behavior<T> 생성 |

</details>
```

- [ ] **Step 4: Commit**

```bash
git commit -m "docs(wpf-dev-pack): README command→skill 마이그레이션 반영, skill 카운트 67개로 업데이트"
```

---

## Chunk 3: CLAUDE.md 파일들 업데이트

### Task 4: wpf-dev-pack/.claude/CLAUDE.md 업데이트

**Files:**
- Modify: `wpf-dev-pack/.claude/CLAUDE.md`

- [ ] **Step 1: "Trigger Priority" 섹션에서 slash command 설명 업데이트**

현재:
```
1. **Explicit slash command** (`/wpf-dev-pack:skill-name`) → Highest
```

이 부분은 slash command가 아닌 skill 호출이므로 그대로 유지 가능. 변경 불필요.

### Task 5: skills/.claude/CLAUDE.md에 새 skill 키워드 매핑 추가

**Files:**
- Modify: `wpf-dev-pack/skills/.claude/CLAUDE.md`

- [ ] **Step 1: Skill Category Index에 Scaffolding 카테고리 추가**

기존 카테고리 테이블 끝에 추가:

```markdown
| **Scaffolding** | `make-wpf-project`, `make-wpf-custom-control`, `make-wpf-usercontrol`, `make-wpf-converter`, `make-wpf-behavior` |
```

- [ ] **Step 2: Keyword-Skill Mapping의 WPF Keywords 테이블에 스캐폴딩 키워드 추가**

```markdown
| `프로젝트 생성`, `scaffold`, `새 프로젝트` | `make-wpf-project` |
| `customcontrol 생성`, `컨트롤 생성` | `make-wpf-custom-control` |
| `usercontrol 생성`, `유저컨트롤` | `make-wpf-usercontrol` |
| `converter 생성`, `컨버터` | `make-wpf-converter` |
| `behavior 생성`, `비헤이비어` | `make-wpf-behavior` |
```

### Task 6: 프로젝트 루트 .claude/CLAUDE.md 변경 이력 추가

**Files:**
- Modify: `.claude/CLAUDE.md`

- [ ] **Step 1: Skills 업데이트 이력에 마이그레이션 기록 추가**

최상단 이력 항목으로 추가:

```markdown
### 2026-03-16: wpf-dev-pack - Command를 Skill로 마이그레이션

**목적:**
- Claude Code 2.1.3에서 command가 agent skill 2.0으로 통합됨에 따른 마이그레이션

**변경 사항:**
- 5개 command를 skills/로 이동: make-wpf-project, make-wpf-custom-control, make-wpf-usercontrol, make-wpf-converter, make-wpf-behavior
- commands/ 디렉토리 삭제
- skill 카운트: 62 → 67

**호출 방식 변경 없음:**
- 기존: `/wpf-dev-pack:make-wpf-project MyApp`
- 이후: `/wpf-dev-pack:make-wpf-project MyApp` (동일)
```

- [ ] **Step 2: Commit**

```bash
git commit -m "docs(wpf-dev-pack): CLAUDE.md 파일들에 command→skill 마이그레이션 반영"
```

---

## 검증

- [ ] `wpf-dev-pack/commands/` 디렉토리가 존재하지 않는지 확인
- [ ] `wpf-dev-pack/skills/make-wpf-project/SKILL.md` 등 5개 skill이 정상 존재하는지 확인
- [ ] `grep -r "commands/" wpf-dev-pack/` 으로 남은 commands 참조 확인 (README의 설명 텍스트 제외)
- [ ] skill 총 카운트가 67개인지 확인: `ls wpf-dev-pack/skills/ | grep -v README | grep -v ".claude" | wc -l`
