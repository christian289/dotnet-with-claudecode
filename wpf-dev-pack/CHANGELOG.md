# Changelog

All notable changes to **wpf-dev-pack** are documented in this file.
The format loosely follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to Semantic Versioning.

---

## v1.6.4 — MVVM Terminology Standardization

### Documentation
- `docs/TERMINOLOGY.md` 신규 추가: 4축 분리 모델(Composition Direction ×
  State Management) 정의, Microsoft 공식 정의 인용, wpf-dev-pack의 두 채택
  조합(CommunityToolkit / Prism)을 정확히 명시.
- `docs/TERMINOLOGY.md` added: defines the four-axis separation model
  (Composition Direction × State Management), cites the official Microsoft
  definitions, and documents the two adopted combinations (CommunityToolkit
  / Prism).
- `.claude/rules/prohibitions.md`: 기존 1개 규칙을 P-001~P-004 4개 규칙으로
  확장하며 각 금지 규칙이 어떤 composition style에 해당하는지 명시.
- `.claude/rules/prohibitions.md`: expanded from one rule to four (P-001
  through P-004), classifying each prohibition by its composition style.
- `.claude/CLAUDE.md`, `README.md`, `README.ko.md`, `skills/make-wpf-viewmodel/SKILL.md`:
  "View First MVVM" 단일 라벨을 두 경로별 정확한 라벨로 정정.
- "View First MVVM" single-label corrected to per-path accurate labels in
  the above files.
- 두 wiring 규칙 파일(`view-viewmodel-wiring-communitytoolkit.md`,
  `view-viewmodel-wiring-prism.md`) 상단에 AI agent용 anchor 블록 추가.
- AI-agent anchor blocks added to both wiring rule files.

### Background
v1.5.x 이하까지 본 plugin은 채택한 composition 방식을 일괄적으로
"View First MVVM"으로 표기했으나, Microsoft Learn의 공식 정의 기준
이는 부정확한 라벨이었습니다.

> "With view-first navigation, the page to navigate to refers to the name
> of the view type. ... view-model-first navigation, where the page to
> navigate to refers to the name of the view-model type."
> — Microsoft Learn (MAUI Navigation)

CommunityToolkit.Mvvm 경로의 `Mappings.xaml`은 ViewModel 타입을 lookup key로
사용하므로 **ViewModel First Composition**, Prism 경로의 `RegisterForNavigation`
+ `RequestNavigate`는 View name이 lookup key이므로 **View First Composition**에
해당합니다. v1.6.4는 이 분류를 표준 용어로 정확히 표기합니다.

The pre-v1.6.4 docs uniformly used the "View First MVVM" label, which
conflicts with Microsoft's official definition. The `Mappings.xaml` path
(CommunityToolkit) uses the ViewModel type as the lookup key and is in fact
**ViewModel First Composition**, while the Prism path uses the View name and
is **View First Composition**. v1.6.4 corrects the labels accordingly.

### What Changed
- 라벨: 일괄 "View First MVVM" → 경로별 정확한 라벨 (위 §Background 참조)
- Labels: uniform "View First MVVM" → per-path accurate labels (see Background)
- 금지 규칙 표현: 각 규칙이 어떤 composition style에 해당하는지 명시
- Prohibition wording: each rule now classifies its composition style
- wiring 규칙 파일 상단 AI anchor 블록 신설
- AI-anchor blocks added at the top of wiring rule files

### What Did NOT Change (Breaking Changes: None)
- 매칭 메커니즘: 그대로 유지 (`Mappings.xaml` / `RegisterForNavigation`)
- Matching mechanisms: unchanged (`Mappings.xaml` / `RegisterForNavigation`)
- 금지 패턴 목록: 그대로 유지 (`ViewModelLocator`, code-behind DataContext, 등)
- Prohibited pattern list: unchanged (`ViewModelLocator`, code-behind DataContext, etc.)
- Skills 디렉토리 코드 생성 로직: 그대로 유지
- Skill code-generation logic: unchanged

기존 v1.5.x / v1.6.4 프로젝트는 코드 수정 없이 v1.6.4의 새 라벨로 그대로
호환됩니다.

Existing v1.5.x / v1.6.4 projects are compatible with v1.6.4 labels without
any code changes.

### Deprecated
- "View First MVVM" 단일 라벨은 더 이상 사용되지 않습니다. 검색 호환성을
  위해 `docs/TERMINOLOGY.md` §4에 변경 이력으로만 언급됩니다.
- The "View First MVVM" single label is deprecated. It is referenced only
  as change history in `docs/TERMINOLOGY.md` §4 for searchability.
