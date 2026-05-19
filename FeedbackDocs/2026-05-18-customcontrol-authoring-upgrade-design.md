# wpf-dev-pack 업그레이드 제안 — CustomControl 작성 교훈 반영

- **작성일**: 2026-05-18
- **출처**: SMVT 프로젝트에서 `ColormapRangeSlider` CustomControl(OpenCV LUT 듀얼-썸 레인지 슬라이더)을 처음부터 손으로 구현하면서 부딪힌 실제 문제들
- **목적**: 이번 세션에서 손으로 고친 안티패턴/함정들이 `wpf-dev-pack` 스킬·스캐폴더에 미반영되어 있어, 동일 실수가 재생산되지 않도록 반영
- **범위**: 신규 스킬 1개 + 기존 스킬 4개 보강 + 스캐폴더 1개 현대화. 별도 레포이므로 반영 시 버전 범프(`v1.6.x`)·README/agents 동기화·release 워크플로 준수 필요

---

## 0. 요약 (우선순위)

| ID | 종류 | 대상 | 우선순위 | 한 줄 |
|----|------|------|----------|-------|
| U1 | 신규 스킬 | `skills/containing-control-decorative-overflow/` | High | FocusRing/HoverGlow 가 조상 `ClipToBounds`·과소 레이아웃 박스에 잘리는 문제 |
| U2 | 보강 | `skills/authoring-wpf-controls/SKILL.md` §4 | High | 다중 제약 coerce 순서 — 도메인 클램프를 **마지막**에 |
| U3 | 현대화 | `skills/make-wpf-custom-control/SKILL.md` | High | 스캐폴더가 raw VSM 문자열·coerce 누락·step 번호 깨짐 등 안티패턴 생성 |
| U4 | 보강 | `skills/authoring-wpf-controls/SKILL.md` (VSM 섹션 신설) + `managing-literal-strings` 상호링크 | Medium | VSM 이름 const/enum 단일화 + XAML `x:Name` const 불가 + 어트리뷰트≠GoToState 무음버그 |
| U5 | 보강 | `skills/managing-wpf-popup-focus/SKILL.md` | Medium | 항목 선택 시 Popup 닫힘 와이어링 + 불투명/반투명 surface 브러시 선택 |
| U6 | 경미 | `skills/optimizing-wpf-memory/SKILL.md` | Low | 변환 후 복사되는 네이티브 리소스(`Mat` 등)는 필드 대신 지역 `using` |

---

## U1. 신규 스킬 — `containing-control-decorative-overflow`

### 근거 (세션 증거)
`ColormapRangeSlider` 의 thumb 는 `Width/Height=28` 인데 `FocusRing`(~34) / `HoverGlow`(43) 가 더 커서:
1. thumb 자체 레이아웃 박스(28)가 비주얼을 못 담아 mis-center → 사용자가 `Margin="-2.5"` 로 임시 보정.
2. thumb 이 `PART_Track`(Border `ClipToBounds=True` + `CornerRadius=6`) **안에** 있어 트랙 경계에서 ring 이 깎임. HoverGlow 를 키워도 소용없음(클리퍼가 조상이라).
3. min/max 끝단에서 z-order(다음 형제 입력박스가 위에 그려짐)로 한쪽만 잘리는 비대칭.

해결에 쓴 핵심 원리(현재 어느 스킬에도 없음):
- **클리퍼는 보통 요소 자신이 아니라 조상의 `ClipToBounds`/`CornerRadius`** 다. 요소 크기를 키워도 안 풀린다.
- 레이아웃 박스를 **최대 비주얼이 동심으로 담기는 크기로 확대**(보이는 원 28, 박스 44). 코드의 위치 계산 상수(`ThumbBoxSize`)와 Style `Width/Height` 가 **반드시 일치**.
- **클립 대상 분리**: 콘텐츠(스펙트럼)는 둥근 클립 유지, 장식/썸 레이어는 별도 형제로 분리.
- 끝단 "가둠"이 필요하면 장식 레이어에 **직사각 `ClipToBounds`**(둥근 클립은 곡선 절단). 의도가 "끝단에서 깔끔히 잘림"이면 사각 클립이 정답.
- 형제 z-order/`Panel.ZIndex` 로 인접 불투명 컨트롤이 장식 오버플로를 덮지 않게.

### `implementing-hit-testing` 와의 차이
그 스킬은 `OnRender` 투명배경으로 **마우스 이벤트 수신**만 다룸. 본 주제는 **시각적 클리핑/오버플로 가둠**으로 무관.

### SKILL.md 골격(작성 규칙 준수)
```
---
description: >
  Fixes WPF custom-control decorative visuals (focus ring, hover glow,
  selection adorner) being clipped or mis-centered. Root cause is usually an
  ancestor ClipToBounds/CornerRadius or a control layout box smaller than its
  largest visual — not the visual's own size.

  Use When:
    - FocusRing / HoverGlow / glow adorner gets shaved at a control or track edge
    - Enlarging the glow element does not stop the clipping
    - Decorative ellipse/border bigger than the control's Width/Height is off-center
    - Thumb / handle at a range slider extreme is cut asymmetrically (min vs max)
    - Need a clean straight-edge "contained at boundary" look vs full overflow

  Do NOT Use When:
    - The clipping is desired and already correct
    - Issue is mouse hit-testing on OnRender visuals (use implementing-hit-testing)
    - Pure DrawingContext/DrawingVisual render perf (use rendering-* skills)
---
```
본문 섹션(한글): ① 근본원인 진단(조상 ClipToBounds vs 과소 박스), ② 레이아웃 박스 확대 + 코드상수 동기화 패턴, ③ 클립/장식 레이어 분리, ④ 사각 vs 둥근 클립 선택(끝단 가둠), ⑤ z-order/ZIndex, ⑥ 흔한 함정(음수 Margin 보정은 증상 가림), ⑦ 검증(끝단/중앙/포커스/호버 시 육안 + dotnet build).

---

## U2. `authoring-wpf-controls` §4 보강 — 다중 제약 Coerce 순서

### 근거 (세션 증거)
`CoerceLowValue` 가 `Max(Minimum,v)` → `Min(v, HighValue-MinimumGap)` 순서였는데, 도메인 재구성 과도기(ColorMapWindow 의 8U↔16U dtype 전환으로 `Minimum/Maximum` 변경)에 `HighValue-MinimumGap < Minimum` 이면 결과가 `Minimum` 미만으로 떨어짐. Gemini 코드리뷰도 동일 지적. 수정: **관계(no-cross/gap) 클램프 먼저 → 도메인(min/max) 하드 클램프 마지막**.

### 현재
§4 의 coerce 예시는 `=> Math.Clamp((int)value, 0, 100);` 한 줄 — 단일 제약뿐, 다중 제약 순서 함정 없음.

### 제안 (해당 섹션에 추가할 내용)
- 원칙: 속성에 **관계 제약 + 도메인 제약**이 함께 있으면 도메인(하드 경계) 클램프를 **반드시 마지막**에. 그래야 어떤 과도기에도 도메인 밖 값이 안 나옴.
- 예시: 듀얼-썸 `RangeSlider` 의 `CoerceLow`/`CoerceHigh` (no-cross + min/max).
```csharp
// 관계 제약 먼저, 도메인 클램프 마지막
v = Math.Min(v, c.HighValue - c.MinimumGap); // no-cross
v = Math.Max(c.Minimum, v);                  // hard domain (LAST)
```
- 노트: WPF `RangeBase` 도 도메인 클램프를 마지막에 둔다. **핸드오프/디자인 spec 의사코드가 이 순서를 자주 틀리게 적는다** — 그대로 옮기지 말 것.
- `OnXxxChanged` 에서 상호 의존 속성은 `CoerceValue(OtherProperty)` 재호출 필요(no-cross 양방향 일관성).

---

## U3. `make-wpf-custom-control` 스캐폴더 현대화 (레버리지 최대)

스캐폴더가 이번에 손으로 제거한 안티패턴을 그대로 생성함. 수정 항목:

| # | 현재 | 변경 |
|---|------|------|
| 1 | "Step 1" 다음 바로 "Step 3" (Step 2 없음) | 번호 정정(Step 1→2→3…) |
| 2 | `[TemplateVisualState(Name="Normal")]` + `GoToState(this,"Disabled"…)` raw string 중복 | 중첩 `private static class VisualStates { public const string Normal="Normal"; … }` 생성, 어트리뷰트·`GoToState` 모두 const 사용. (선택) `enum` 으로 호출부 문자열 제거 |
| 3 | 어트리뷰트에 `Pressed` 선언했으나 `UpdateVisualState` 는 `Pressed` 로 전이 안 함 | 선언한 상태는 코드에서 실제 도달하도록(또는 미사용 상태 제거). 어트리뷰트 Name = 실제 GoToState 이름과 일치 강제 주석 |
| 4 | `CoerceValueCallback` 예시 없음 | U2 의 순서 주석 포함한 coerce 예시 1개 추가 |
| 5 | 하드코딩 색상(`#FFFFFF` 등) | host 테마 상속(`ui:ThemeResource`/암시 스타일) vs 시그니처 전용 토큰 구분 노트 추가 |
| 6 | `ColorAnimation` 대상 `(Border.Background).(SolidColorBrush.Color)` | Background 가 공유/Dynamic/Frozen 브러시면 깨짐 — 안전 패턴(개별 Border 레이어 opacity, 또는 명시적 SolidColorBrush 인스턴스) 주석 |
| 7 | 없음 | read-only DP(`RegisterReadOnly`) + `RoutedEvent` 스텁, part-missing 관용(예외 대신 기능 비활성, `authoring-wpf-controls §3.1` 링크), `OnApplyTemplate` 가 `Loaded` 보다 먼저 실행 → 중복 init 회피 노트 |

> 주의: 스캐폴더는 범용이라 file-scoped namespace 등은 그대로 둬도 무방하나, "프로젝트 컨벤션(block-scoped 등)이 있으면 따르라"는 한 줄 안내 권장.

---

## U4. VisualState 이름 관리 (신규 섹션 + 상호 링크)

### 근거 (세션 증거)
`[TemplateVisualState(GroupName="LowThumbStates", Name="Normal")]` 인데 실제 코드는 `GoToState(this,"LowNormal")` 로 전이 — 어트리뷰트는 메타데이터일 뿐이라 **컴파일·런타임 무음**, 디자이너 힌트만 틀림. 해결: `VisualStates` const + `ThumbVisual` enum 으로 단일화.

### 핵심(어디에도 없는 WPF 특수 사실)
- `[TemplateVisualState]` Name/GroupName 인자, `VisualStateManager.GoToState(...)` 의 string 인자는 **`const string` 사용 가능**(컴파일타임 상수/일반 string).
- 그러나 XAML `<VisualStateGroup x:Name>` / `<VisualState x:Name>` 는 **C# const 참조 불가** — VSM 은 이름 기반 계약이라 이 XAML↔C# 결합점은 **불가피하게 리터럴**. const 값이 그 `x:Name` 과 정확히 같아야 한다(주석으로 명시).
- 어트리뷰트 Name ≠ 실제 GoToState 이름이면 **무음 버그**(상태 전이 무효, 예외 없음). 그룹 간 동일 단순명("Normal")이 있으면 thumb 은 `LowNormal`/`HighNormal` 처럼 결합명으로.

### 반영
- `authoring-wpf-controls` 에 "VisualState 이름 관리" 섹션 신설(현재 §3.3 은 상태 표현 우선순위만, 이름 계약 없음).
- `managing-literal-strings` 끝에 "WPF VSM 특수 케이스 → authoring-wpf-controls 참조" 한 줄 상호링크.

---

## U5. `managing-wpf-popup-focus` 보강

### 근거 (세션 증거)
컬러맵 picker `Popup StaysOpen=False` 는 **바깥 클릭만** 닫힘 — ListBox 항목을 골라도 안 닫혀, `PART_ColormapList` 명명 후 `SelectionChanged` 에서 owning `ToggleButton.IsChecked=false` 로 코드 와이어링. 또한 popup 배경에 `CardBackgroundFillColorDefaultBrush`(반투명 acrylic)를 써서 뒤가 비쳐, `SolidBackgroundFillColorSecondaryBrush`(불투명)로 교체.

### 현재
이 스킬은 cross-app **포커스 획득**(`PreviewMouseDown`/`Activate`)만 다룸. 선택-닫힘·surface 불투명은 없음.

### 제안 (섹션 추가, 또는 신규 `closing-wpf-popup-on-selection`)
- 패턴: 템플릿 안 선택형 컨트롤(ListBox/Selector)은 PART 명명 → `OnApplyTemplate` 에서 `SelectionChanged` 구독(detach 포함) → owning `ToggleButton.IsChecked=false`(= `Popup.IsOpen` TwoWay) 로 닫기.
- 노트: `StaysOpen=False` 의 닫힘 범위(바깥 클릭 O, 내부 선택 X) 명확화.
- surface 브러시: WPF-UI `Card*` 계열은 반투명 → 뒤 컨트롤 비침. flyout/popup 불투명이 필요하면 `SolidBackgroundFillColor*` 사용. (`integrating-wpfui-fluent` 와 상호링크)

---

## U6. `optimizing-wpf-memory` 경미 보강

### 근거
`RebuildColormapBrush` 가 `_rampMat` 을 필드로 잡고 `Unloaded` 에서 dispose 했으나, `Mat.ToBitmapSource()` 가 픽셀을 복사하므로 `source.Freeze()` 직후 `Mat` 즉시 해제 가능 → 지역 `using Mat` 으로 단순화(필드/Unloaded/생성자 구독 제거). Gemini 도 동일 권고.

### 제안
"네이티브/대용량 리소스를 다른 표현으로 **복사 변환**한 경우(예: `Mat`→`BitmapSource`, freeze 후), 원본은 컨트롤 수명 동안 보유하지 말고 메서드 지역 `using` 으로 즉시 해제" 한 단락 + 안티패턴(필드+Unloaded) 대비표.

---

## 부수 메모 (참고용, 별도 액션 아님)

- LSP 가 `OpenCvSharp`/`Mat`/`ColormapTypes` 등 패키지 타입에 광범위한 CS0246/CS0103 false positive 를 냄 → 스킬에 "LSP 단독 신뢰 금지, `dotnet build` 로 검증" 한 줄(일부 스킬엔 이미 있음, 일관화 권장).
- 실행 중 WPF 앱/VS 가 출력 DLL 잠금 → 전체 솔루션 빌드의 copy 단계(MSB3021/MSB3027) 실패는 컴파일 실패와 구분해야 함(앱 종료 후 재빌드 안내).

---

## 반영 절차 제안

1. U3(스캐폴더) → U2(coerce) → U1(신규 스킬) → U4 → U5 → U6 순(레버리지/정확성 우선).
2. 신규/수정 스킬은 이 레포 `writing-skills` 규칙 준수: 폴더=`name`=kebab-case+동명사, `description` 영어 + `Use When`/`Do NOT Use When`, 본문 한글, 코드/경로 원문.
3. `make-wpf-custom-control` 수정 시 생성 결과를 실제 1회 스캐폴딩 후 `dotnet build` 통과 확인.
4. 완료 시 `v1.6.x` 버전 범프 + README(.md/.ko.md)·agents 동기화 + release 워크플로.
