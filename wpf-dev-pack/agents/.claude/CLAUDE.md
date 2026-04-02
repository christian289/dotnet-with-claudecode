# WPF Dev Pack Agents - Delegation Guide

Delegate complex tasks to specialized agents.

---

## Requirements Interview System

`wpf-architect` uses an **adaptive interview system** where Step 1 determines the entire interview path.

### Path Overview

| Path | Task Type | Steps | Focus |
|------|-----------|-------|-------|
| **A** | Create new project | 7 | 컨셉 → 아키텍처 → 규모 → 복잡도 → 라이브러리 → 기능 영역 |
| **B** | Analyze/improve | 5 | 분석 목표 → 분석 모드 → 범위 → 출력 형식 |
| **C** | Implement feature | 5 | 기능 설명 → 구현 방식 → 라이브러리 → 기능 영역 |
| **D** | Debug/fix | 4 | 문제 증상 → 문제 유형 → 문제 영역 |

### Interview Flow

```
Step 1: Task Type
   ├─→ A: Create new project (7 steps)
   │   ├─ A-2: 프로그램 컨셉 (자유 입력 → 키워드 분석)
   │   ├─ A-3: 아키텍처 패턴
   │   ├─ A-4: 프로젝트 규모
   │   ├─ A-5: 복잡도
   │   ├─ A-6: 오픈소스 라이브러리 (키워드 기반 추천)
   │   └─ A-7: 기능 영역
   │
   ├─→ B: Analyze/improve (5 steps)
   │   ├─ B-2: 분석 목표 (자유 입력 → 키워드 분석)
   │   ├─ B-3: 분석 모드 (코드 품질/성능/아키텍처/오픈소스)
   │   ├─ B-4: 분석 범위 (전체/특정 프로젝트/특정 파일)
   │   └─ B-5: 출력 형식 (수정 코드/요약/패턴 추출)
   │
   ├─→ C: Implement feature (5 steps)
   │   ├─ C-2: 기능 설명 (자유 입력 → 키워드 분석)
   │   ├─ C-3: 구현 방식 (통합/독립 모듈/프로토타입)
   │   ├─ C-4: 라이브러리 (키워드 기반 추천)
   │   └─ C-5: 기능 영역
   │
   └─→ D: Debug/fix (4 steps)
       ├─ D-2: 문제 증상 (자유 입력 → 키워드 분석)
       ├─ D-3: 문제 유형 (UI/데이터/성능/크래시/빌드)
       └─ D-4: 문제 영역 (XAML/ViewModel/CustomControl/Rendering/Threading/3rd Party)
```

### Keyword Analysis

All free-input steps (A-2, B-2, C-2, D-2) analyze user input for keywords and auto-set defaults for subsequent steps. This reduces interview friction while maintaining precision.

### Usage

The interview starts automatically when `wpf-architect` is invoked. Users can:
- Click preset options for common choices
- Type custom responses in free-input steps
- Follow the recommended path or override defaults

### Path-Specific Agent Routing

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

