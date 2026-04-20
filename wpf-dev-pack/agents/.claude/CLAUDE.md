# WPF Dev Pack Agents - Delegation Guide

Delegate complex tasks to specialized agents.

> **Full Requirements Interview spec**: see `wpf-dev-pack/.claude/CLAUDE.md` §Requirements Interview System and `agents/wpf-architect.md`.
> 전체 인터뷰 사양은 부모 CLAUDE.md §Requirements Interview System과 `agents/wpf-architect.md` 참조.

---

## Path-Specific Agent Routing

| Path | Selection | Primary Agent |
|------|-----------|---------------|
| B | 코드 품질 리뷰 | `wpf-code-reviewer` |
| B | 성능 분석 | `wpf-performance-optimizer` |
| B | 아키텍처 진단 | `wpf-architect` (self) |
| B | 오픈소스 코드 분석 | `wpf-architect` (self) |
| D | UI 표시 문제 | `wpf-xaml-designer` |
| D | 데이터 문제 | `wpf-data-binding-expert` |
| D | 성능 문제 | `wpf-performance-optimizer` |
| D | 크래시/예외 | `wpf-code-reviewer` |
| D | 빌드/설정 오류 | `wpf-code-reviewer` |

---

## Agent Mapping

| Task Type | Agent | Trigger |
|-----------|-------|---------|
| Architecture analysis | `wpf-architect` | "architecture", "best practice" |
| Code review | `wpf-code-reviewer` | "review", "MVVM violation" |
| CustomControl development | `wpf-control-designer` | CustomControl creation |
| XAML styles/themes | `wpf-xaml-designer` | ControlTemplate, Style |
| MVVM implementation | `wpf-mvvm-expert` | ViewModel, Command |
| Data binding | `wpf-data-binding-expert` | Complex bindings |
| Performance optimization | `wpf-performance-optimizer` | Performance issues |

