# WPF Dev Pack Agents - Delegation Guide

Delegate complex tasks to specialized agents.

> **Full Requirements Interview spec**: see `wpf-dev-pack/.claude/CLAUDE.md` §Requirements Interview System and `agents/wpf-architect.md`.

---

## Path-Specific Agent Routing

| Path | Selection | Primary Agent |
|------|-----------|---------------|
| B | Code quality review | `wpf-code-reviewer` |
| B | Performance analysis | `wpf-performance-optimizer` |
| B | Architecture diagnosis | `wpf-architect` (self) |
| B | Open-source code analysis | `wpf-architect` (self) |
| B | Codebase-wide audit | `wpf-code-auditor` |
| D | UI display problem | `wpf-xaml-designer` |
| D | Data problem | `wpf-data-binding-expert` |
| D | Performance problem | `wpf-performance-optimizer` |
| D | Crash / exception | `wpf-code-reviewer` |
| D | Build / configuration error | `wpf-code-reviewer` |

---

## Agent Mapping

| Task Type | Agent | Trigger |
|-----------|-------|---------|
| Architecture analysis | `wpf-architect` | "architecture", "best practice" |
| Code review | `wpf-code-reviewer` | "review", "MVVM violation" |
| Full-codebase audit | `wpf-code-auditor` | "audit", "codebase-wide consistency", pattern sweep |
| CustomControl development | `wpf-control-designer` | CustomControl creation |
| XAML styles/themes | `wpf-xaml-designer` | ControlTemplate, Style |
| MVVM implementation | `wpf-mvvm-expert` | ViewModel, Command |
| Data binding | `wpf-data-binding-expert` | Complex bindings |
| Performance optimization | `wpf-performance-optimizer` | Performance issues |

