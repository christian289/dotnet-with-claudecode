# CMDS Pattern Refactoring — wpf-dev-pack

## Summary

CMDS system files repository에서 검증된 3가지 아키텍처 패턴을 wpf-dev-pack 플러그인에 적용한다:

1. **@include / shared rules 확장** — 에이전트 간 중복 규칙을 `rules/`로 추출
2. **에이전트 본문 리팩토링** — 중복 제거 후 `@rules/` 참조로 교체
3. **Essential (Post-Compact)** — 컨텍스트 압축 후에도 살아남는 핵심 규칙 명시

## Motivation

7개 에이전트 정의 파일에 동일한 규칙이 2~4회 중복되어 있다. 규칙 변경 시 최대 4곳을 수동 수정해야 하며, 불일치 위험이 있다. CMDS의 `@include` 패턴으로 Single Source of Truth를 확보한다.

---

## Change 1: New Shared Rule Files (6 files)

All files created in `.claude/rules/`.

### 1-1. `mvvm-constraints.md`

**Source**: architect(38-45), mvvm-expert(25-28), code-reviewer(56-68), data-binding-expert(28-30)

Content:
- No System.Windows references in ViewModel
- Prohibited DLLs: WindowsBase.dll, PresentationFramework.dll, PresentationCore.dll
- ViewModel can only use BCL types (IEnumerable, ObservableCollection, etc.)
- Code examples: allowed vs prohibited `using` statements

### 1-2. `resourcedictionary-patterns.md`

**Source**: architect(56-58), control-designer(103-113), xaml-designer(25-38)

Content:
- Generic.xaml serves only as MergedDictionaries hub
- Separate each control style into individual XAML files
- x:Key resources defined before referencing Style
- Code example: Generic.xaml hub pattern

### 1-3. `freezable-performance.md`

**Source**: code-reviewer(100-109), performance-optimizer(92-108)

Content:
- ALL Freezable objects (Brush, Pen, Geometry) must be frozen
- Code examples: correct vs incorrect usage
- Why: unfrozen resources consume more memory and prevent cross-thread access

### 1-4. `rendering-antipatterns.md`

**Source**: code-reviewer(111-123), performance-optimizer(56-60)

Content:
- InvalidateVisual() in loops = ANTI-PATTERN
- Call InvalidateVisual() ONCE after all data changes
- Code example: batch update pattern

### 1-5. `virtualization-patterns.md`

**Source**: code-reviewer(125-144), performance-optimizer(110-127)

Content:
- ItemsControl with VirtualizingStackPanel for large collections
- VirtualizationMode="Recycling"
- ListView attached property pattern
- Code examples: XAML patterns

### 1-6. `converter-patterns.md`

**Source**: code-reviewer(161-168), data-binding-expert(35-61)

Content:
- Singleton converter pattern with `static Instance`
- TemplateBinding vs Binding (TemplateBinding faster in ControlTemplate)
- Pure function requirement (no side-effects in converters)

---

## Change 2: Agent Refactoring (7 agents)

For each agent, replace duplicated rule sections with `@rules/` references. Keep all unique content intact.

### wpf-architect.md
- **Remove**: "MVVM Constraints" section (lines 38-45), "CustomControl Rules" (lines 56-58)
- **Add**: `@rules/mvvm-constraints.md`, `@rules/resourcedictionary-patterns.md`
- **Keep**: Project Structure (unique), Requirements Interview (unique)

### wpf-mvvm-expert.md
- **Remove**: "Critical Constraints" section (lines 25-28)
- **Add**: `@rules/mvvm-constraints.md`
- **Keep**: ViewModel Base Pattern, CollectionView Encapsulation, Navigation (all unique)

### wpf-code-reviewer.md
- **Remove**: MVVM violation examples (lines 56-68), Unfrozen Freezable (100-109), InvalidateVisual (111-123), Missing Virtualization (125-144), TemplateBinding/ResourceDictionary (161-178)
- **Add**: `@rules/mvvm-constraints.md`, `@rules/freezable-performance.md`, `@rules/rendering-antipatterns.md`, `@rules/virtualization-patterns.md`, `@rules/converter-patterns.md`, `@rules/resourcedictionary-patterns.md`
- **Keep**: C# LSP Integration (unique), Direct UI Manipulation check (unique), Code-Behind check (unique), DependencyProperty callback pattern (unique), Review Output Format (unique)

### wpf-control-designer.md
- **Remove**: file structure Generic.xaml reference (lines 103-113)
- **Add**: `@rules/resourcedictionary-patterns.md`
- **Keep**: DependencyProperty Pattern, Parts and States Model, DefaultStyleKey (all unique)

### wpf-data-binding-expert.md
- **Remove**: Critical Constraints MVVM reference (lines 28-30)
- **Add**: `@rules/mvvm-constraints.md`, `@rules/converter-patterns.md`
- **Keep**: MultiBinding, Validation, Debugging, Common Issues (all unique)

### wpf-performance-optimizer.md
- **Remove**: Freezable Pattern section (lines 92-108), VirtualizingStackPanel section (lines 110-127)
- **Add**: `@rules/freezable-performance.md`, `@rules/virtualization-patterns.md`, `@rules/rendering-antipatterns.md`
- **Keep**: DrawingContext, DrawingVisual, BitmapCache, Dispatcher Priority (all unique)

### wpf-xaml-designer.md
- **Remove**: ResourceDictionary Structure (lines 25-27), Generic.xaml Pattern (lines 30-38)
- **Add**: `@rules/resourcedictionary-patterns.md`
- **Keep**: Individual Control Style, Resource Reference Rules, ControlTemplate, Animation (all unique)

---

## Change 3: Essential (Post-Compact) in CLAUDE.md

Add section to `.claude/CLAUDE.md` after "MVVM Approach: View First":

```markdown
## Essential (Post-Compact)

These rules MUST survive context compression. If prior context is lost, re-read this section:

1. **ViewModelLocator 금지** — DI + DataTemplate 매핑만 사용 (`rules/prohibitions.md`)
2. **ViewModel에 System.Windows 참조 금지** — BCL 타입만 사용 (`rules/mvvm-constraints.md`)
3. **Freezable 객체는 반드시 Freeze()** — Brush, Pen, Geometry (`rules/freezable-performance.md`)
4. **Generic.xaml = MergedDictionaries hub 전용** (`rules/resourcedictionary-patterns.md`)
5. **HandMirror로 API 시그니처 검증 후 코드 작성**
6. **View First MVVM** — framework별 wiring은 `rules/` 참조
```

---

## Out of Scope

- token-estimate frontmatter (Claude Code 공식 필드 아님)
- STATIC/DYNAMIC markers (스킬 콘텐츠는 대부분 정적)
- precedence (키워드 라우팅이 이미 담당)
- memory-type (플러그인 맥락에 부적합)

## Success Criteria

- 에이전트 파일 간 규칙 중복 0건
- 기존 규칙 3개 + 신규 규칙 6개 = rules/ 디렉토리 총 9개 파일
- 에이전트 고유 콘텐츠 손실 없음
- CLAUDE.md에 Essential 섹션 존재
