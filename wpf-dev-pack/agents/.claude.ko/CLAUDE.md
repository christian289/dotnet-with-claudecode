# WPF Dev Pack Agents - Delegation Guide

복잡한 작업은 전문 agent에 위임합니다.

> 본 파일은 `wpf-dev-pack/agents/.claude/CLAUDE.md`의 한국어 미러입니다.
> AI는 영문 원본을 읽으며, 본 파일은 사람을 위한 참고용입니다.

> **전체 Requirements Interview 사양**: `wpf-dev-pack/.claude/CLAUDE.md`의
> §Requirements Interview System 및 `agents/wpf-architect.md` 참조.

---

## 경로별 Agent 라우팅

| Path | Selection | Primary Agent |
|------|-----------|---------------|
| B | 코드 품질 리뷰 | `wpf-code-reviewer` |
| B | 성능 분석 | `wpf-performance-optimizer` |
| B | 아키텍처 진단 | `wpf-architect` (self) |
| B | 오픈소스 코드 분석 | `wpf-architect` (self) |
| B | 코드베이스 전수 감사 | `wpf-code-auditor` |
| D | UI 표시 문제 | `wpf-xaml-designer` |
| D | 데이터 문제 | `wpf-data-binding-expert` |
| D | 성능 문제 | `wpf-performance-optimizer` |
| D | 크래시/예외 | `wpf-code-reviewer` |
| D | 빌드/설정 오류 | `wpf-code-reviewer` |

---

## Agent 매핑

| 작업 유형 | Agent | 트리거 |
|-----------|-------|--------|
| Architecture analysis | `wpf-architect` | "architecture", "best practice" |
| Code review | `wpf-code-reviewer` | "review", "MVVM violation" |
| Full-codebase audit | `wpf-code-auditor` | "audit", "codebase-wide consistency", pattern sweep |
| CustomControl development | `wpf-control-designer` | CustomControl 생성 |
| XAML styles/themes | `wpf-xaml-designer` | ControlTemplate, Style |
| MVVM implementation | `wpf-mvvm-expert` | ViewModel, Command |
| Data binding | `wpf-data-binding-expert` | 복잡한 binding |
| Performance optimization | `wpf-performance-optimizer` | 성능 이슈 |
