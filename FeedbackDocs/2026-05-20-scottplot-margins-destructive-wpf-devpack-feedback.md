# wpf-dev-pack 피드백 — ScottPlot 5 `Axes.Margins(x, y)` 의 destructive 동작 경고

- **작성일**: 2026-05-20
- **출처**: SMVT 프로젝트 (`fix/smvt-200` 브랜치, PR #347). Prism + ScottPlot 5 + reactive MVVM 컨버터 (`ConveyToPlotConverter`) 가 콜러맵 슬라이더 / 컬러맵 토글 시점에 ImageRect in-place swap 으로 매 tick 재실행되는 hot path 에서 ShapeDrawingBehavior 가 그린 Rectangle overlay 가 Y 축 플립되는 회귀를 디버깅하다 발견.
- **목적**: ScottPlot 5 의 `plot.Axes.Margins(double, double)` 가 이름과 달리 단순 setter 가 아니라 **AutoScaler 인스턴스 교체 + 즉시 AutoScale 실행** 을 동반하는 destructive 호출이며, reactive MVVM 컨버터처럼 같은 코드 경로가 반복 실행되는 시나리오에서 `InvertedY` flag 의 효과가 무력화되어 axis 상태가 발산하는 footgun 임을 wpf-dev-pack 사용자에게 명시.
- **범위**: 신규 스킬 1건 (`scottplot-axes-margins-destructive`) 또는 기존 ScottPlot 관련 스킬 (`scottplot-syncing-modifier-keys-for-mousewheel` 등) 에 한 섹션 추가. 버전 범프 + README 동기화 필요.

---

## 0. 요약 (우선순위)

| ID | 종류 | 대상 | 우선순위 | 한 줄 |
|----|------|------|----------|-------|
| 1  | 신규 스킬 (또는 기존 ScottPlot 스킬 보강) | `skills/scottplot-axes-margins-destructive/SKILL.md` (또는 인접 스킬에 섹션 추가) | **High** | `plot.Axes.Margins(x, y)` 는 AutoScaler 교체 + 즉시 AutoScale 호출이므로 reactive 컨버터 hot path 에서는 `FractionalAutoScaler.SetMarginsX/Y` 로 우회 |

---

## 1. ScottPlot 5 `Axes.Margins(x, y)` destructive 동작 — reactive 컨버터에서 InvertedY 무력화

### 근거 (세션 증거)

세션에서 다음 시나리오를 디버깅:

- **증상**: Conversion3DProfile FB 의 Parameter Edit 다이얼로그 (`ShapeRectangleDrawingToolView`) 에서 사용자가 ROI Rectangle 을 그린 뒤 ColorMap Filter 의 "Apply Colormap" 체크박스를 toggle 하면, X/Y/Width/Height 값은 유지되는데 ScottPlot 의 Rectangle plottable 만 시각적으로 Y 축 방향이 뒤집힘 (image-style inverted → cartesian +Y).
- **재현 조건**: WPF binding `Plot="{Binding DisplaySourceContainer, Converter=ConveyToPlotConverter}"` 가 wrapper 의 `ImageSource` PropertyChanged 시 재평가되어 컨버터의 `ConfigureImagePlot` 이 매번 동일 코드 경로를 다시 실행하는 reactive hot path.

3차 fix 시도 끝에 발견한 root cause:

```csharp
private Plot ConfigureImagePlot(Plot plot, IImageSourceContainer container)
{
    ConfigurePlot(plot);                              // 내부에서 AutoScale() 호출 (기존 코드)
    plot.Axes.Margins(0, 0);                          // ← 이 호출이 destructive
    plot.Axes.AutoScaler.InvertedY = true;            // ← flag 만 set, range 재계산 안 함
    ...
}
```

ScottPlot 5.1.57 의 `AxisManager.Margins(double horizontal, double vertical)` 내부 동작:

1. `plot.Axes.AutoScaler` 를 **새 `FractionalAutoScaler(x, y)` 인스턴스로 교체**. 새 AutoScaler 의 `InvertedY` 기본값은 `false`.
2. 즉시 `AutoScale()` 호출 → 새 AutoScaler (InvertedY=false) 기준으로 Y 축 range 를 `(Min=0, Max=H)` 비-반전 상태로 재계산.

따라서 직후 `plot.Axes.AutoScaler.InvertedY = true` 는 **새 AutoScaler 의 flag 만** 갱신할 뿐, 이미 비-반전 상태가 된 YAxis.Range 는 재계산되지 않음. 다음 명시적 `AutoScale()` 호출 전까지 axis 가 비-반전 상태로 잠재.

원래 코드는 `ConfigurePlot()` 안에 또 다른 `AutoScale()` 호출이 있어 매 호출마다 우연히 axis 를 재-반전시켜 마스킹되었으나, 그 AutoScale 을 다른 위치로 옮기는 fix 를 적용하자 잠재된 destructiveness 가 노출됨. ShapeDrawingBehavior 의 Rectangle plottable 은 ImageRect 와 달리 자체적인 Y-flip 보정이 없어 axis range 부호를 그대로 따르므로 시각적으로 플립이 가시화.

mainview 에서는 PlottableList 에 ImageRect 만 있고 Rectangle overlay 가 없어 동일 결함이 시각화되지 않았음 (ImageRect 자체가 자기 CoordinateRect 의 TOP-LEFT 에 픽셀 (0,0) 을 맞추는 내부 로직을 가져 axis 방향과 무관하게 자연스럽게 보임).

### 손으로 적용한 fix

```csharp
// 변경 전 (destructive):
plot.Axes.Margins(0, 0);
plot.Axes.AutoScaler.InvertedY = true;

// 변경 후 (non-destructive):
if (plot.Axes.AutoScaler is ScottPlot.AutoScalers.FractionalAutoScaler fas)
{
    fas.SetMarginsX(0);
    fas.SetMarginsY(0);
}
plot.Axes.AutoScaler.InvertedY = true;
```

`FractionalAutoScaler.SetMarginsX(double)` / `SetMarginsY(double)` 는 기존 AutoScaler 인스턴스의 fraction 필드만 in-place 갱신. AutoScale 호출 없음. 이전 AutoScale 이 만들어 둔 inverted Y range + InvertedY=true flag 모두 보존됨.

### 제안 (구체 변경)

신규 스킬 1건 추가 또는 기존 ScottPlot 관련 스킬 (`scottplot-syncing-modifier-keys-for-mousewheel` 등) 의 인접 위치에 별도 섹션 추가. 추천 위치:

- 신규: `wpf-dev-pack/skills/scottplot-axes-margins-destructive/SKILL.md`
- 또는 기존: `scottplot-syncing-modifier-keys-for-mousewheel/SKILL.md` 의 마지막에 별도 `## reactive 컨버터에서의 Margins/InvertedY 함정` 섹션 추가.

스킬 본문에 포함되어야 할 내용:

1. **API 동작 명시**: `plot.Axes.Margins(double, double)` 는 단순 setter 가 아니라 다음 두 가지 부작용을 동반:
   - `plot.Axes.AutoScaler` 를 새 `FractionalAutoScaler` 인스턴스로 교체 (이전 인스턴스의 `InvertedY` 등 상태 손실)
   - 즉시 `AutoScale()` 실행 (현 데이터에 맞춰 axis range 재계산)

2. **flag setter 의 lazy 성격**: `plot.Axes.AutoScaler.InvertedY = true` 는 flag 만 set 하며 즉시 axis range 를 뒤집지 않음. 다음 AutoScale 호출 시점에서야 효과 발생.

3. **트리거 시나리오**: 다음 모두 해당될 때 발현 위험:
   - WPF binding + ScottPlot Plot DP 가 같은 코드 경로를 반복 실행 (slider drag, PropertyChanged 등)
   - 같은 컨버터/setup 함수가 매 호출마다 `Margins(x, y)` + `InvertedY = true` 를 함께 수행
   - PlottableList 에 `ImageRect` 외의 plottable (Rectangle, Polygon 등 axis 부호를 그대로 따르는 overlay) 가 추가됨

4. **권장 패턴**: cookbook-once 가정의 setup 코드를 reactive hot path 에서 재사용할 때:

```csharp
// 안전한 패턴 — 기존 AutoScaler in-place 갱신
if (plot.Axes.AutoScaler is FractionalAutoScaler fas)
{
    fas.SetMarginsX(horizontal);
    fas.SetMarginsY(vertical);
}
plot.Axes.AutoScaler.InvertedY = true;
```

또는 setup 코드를 **최초 1회만 실행** 하도록 분리 (예: ImageRect 신규 삽입 시점에만 호출, in-place swap 시점에는 호출 안 함).

5. **검증 방법**: PlotControl 에 `ShapeDrawingBehavior` 같이 axis 부호에 민감한 overlay 를 추가한 뒤, 컨버터를 반복 호출 (예: colormap slider drag) 하며 Rectangle 의 시각적 위치가 유지되는지 확인.

### 인접 스킬과의 차이/링크

- `scottplot-syncing-modifier-keys-for-mousewheel` — modifier 키 sync 와 별도 주제이나 같은 "ScottPlot 5 API 가 cookbook-once 가정으로 설계되어 reactive WPF 와 충돌하는 경향" 을 공유. cross-link 권장.
- 기존 wpf-dev-pack 의 일반 binding/converter 스킬과 중복되지 않음 — ScottPlot 5 의 특정 API 동작에 집중.

---

## 부록 — 향후 우선순위 낮은 후보 (이번 문서에서는 본문화하지 않음)

세션에서 동일하게 부딪혔으나 본 PR 의 ScottPlot Margins 만큼 비용이 크지 않아 본문 작성은 보류한 항목들. 향후 별도 피드백 문서 후보로 메모:

1. **Modal popup 의 부모 Window InputBindings 미상속** — modal 다이얼로그 focus 시 부모 Window 의 `KeyBinding` 이 발화하지 않으므로 popup view 의 root `<View.InputBindings>` 에 명시적으로 복제 필요. WPF 일반 함정.
2. **`{Binding Dict[Key].SubProp}` 의 KeyNotFoundException race** — VM 의 `OnNavigatedTo` 에서 채워지는 Dictionary 가 binding 초기 평가 시점에 비어있어 `Dictionary` indexer 가 throw → `TargetInvocationException` 로그. 해결: `TryGetValue` 기반 null-safe wrapper 클래스로 노출.
3. **reactive 컨버터에서 wrapper PropertyChanged propagation 패턴** — 컨테이너 내부 (예: `wrapper.ImageSource`) PC 가 발화해도 외부 `{Binding wrapper, Converter=...}` binding 은 재평가되지 않음. wrapper 보관자가 inner PC 를 listen 해서 자기 PC 를 재발화해야 함 (`DisplaySourceSlotItem` 패턴).
