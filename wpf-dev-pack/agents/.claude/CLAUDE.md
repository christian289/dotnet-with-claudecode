# WPF Dev Pack Agents - Delegation Guide

Delegate complex tasks to specialized agents.

---

## Requirements Interview System

`wpf-architect` and `wpf-architect-low` now include an **intelligent interview system** that:

1. **Understands user intent** via 4-step AskUserQuestion flow
2. **Routes to appropriate skills/agents** based on responses
3. **Adapts complexity level** to user preferences

### Interview Flow

```
Step 1: Task Type
   ├─→ 새 프로젝트 생성 → Step 2 → make-wpf-project
   ├─→ 기존 프로젝트 분석 → Step 2 → Analysis Process
   ├─→ 특정 기능 구현 → Step 3 (skip architecture)
   └─→ 문제 해결 → Step 4 (direct to features)

Step 2: Architecture Pattern
   ├─→ MVVM + CommunityToolkit → wpf-mvvm-expert
   ├─→ Code-behind → Basic patterns only
   ├─→ Prism Framework → make-wpf-project --prism
   └─→ No preference → Auto-recommend

Step 3: Complexity Level
   ├─→ Simple → Basic WPF patterns
   ├─→ Balanced → wpf-control-designer
   └─→ Advanced → wpf-performance-optimizer

Step 4: Feature Areas (Multi-Select)
   ├─→ UI/컨트롤 → wpf-control-designer
   ├─→ 데이터 바인딩 → wpf-data-binding-expert
   ├─→ 렌더링/그래픽 → wpf-performance-optimizer
   └─→ 애니메이션 → wpf-xaml-designer
```

### Usage

The interview starts automatically when `wpf-architect` is invoked. Users can:
- Click preset options for common choices
- Type "Other" for custom requirements
- Skip steps when context is clear

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

---

## Claude Pro Alternatives

| Default (Opus) | Alternative (Sonnet) |
|----------------|---------------------|
| `wpf-architect` | `wpf-architect-low` |
| `wpf-code-reviewer` | `wpf-code-reviewer-low` |
