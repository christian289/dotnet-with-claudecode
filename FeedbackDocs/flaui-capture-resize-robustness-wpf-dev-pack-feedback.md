# wpf-dev-pack Feedback — FlaUI 캡처/자동화의 WPF 창 리사이즈 강건성

- **Purpose**: WPF UI 자동화(FlaUI)로 스크린샷 캡처·요소 조작을 작성할 때, 대상 창/다이얼로그의 크기·레이아웃이 실행 시점마다 달라져도 깨지지 않게 하는 원칙을 명문화하기 위함.
- **Scope**: 기존 FlaUI 기반 UI 테스트/캡처 스킬 1건 보강(또는 rule 1건 추가). 적용 시 버전 범프·README 동기화 필요.

---

## 0. Summary (priority)

| ID | Kind | Target | Priority | One-liner |
|----|------|--------|----------|-----------|
| 1 | Augment / Rule | skills/&lt;flaui-ui-test-capture&gt;/SKILL.md | Medium | UIA 식별자(Name/ControlType/ClassName/AutomationId) 기반 캡처·조작은 창 리사이즈에 강건; 좌표·고정 crop 기반은 stale 해진다 |

---

## 1. FlaUI 캡처·자동화는 UIA 식별자 기반일 때 창 리사이즈에 강건하다

### Phenomenon and causality

**Phenomenon** — 동일한 WPF 다이얼로그라도 표시 내용이 늘거나 줄어 창 크기·내부 레이아웃이 실행마다 달라질 수 있다(동적 콘텐츠 추가, `SizeToContent`, 가변 패널 등). 이때 UIA 식별자(요소의 `Name` / `ControlType` / `ClassName` / `AutomationProperties.AutomationId`)로 요소를 찾아 캡처·클릭하는 자동화 코드는 그대로 동작하고, `Capture.Element(element, …)` 로 찍은 스크린샷도 변경된 크기에 자동으로 맞춰진다. 반면 화면 절대 좌표나 고정 offset/crop 사각형으로 작성한 코드는 창 크기가 바뀌면 어긋난다.

**Cause** — FlaUI 의 `Capture.Element` 는 캡처 시점에 요소의 현재 `BoundingRectangle`(라이브 visual tree 에서 계산)을 읽어 그 영역만 저장하므로 크기 변화를 추종한다. UIA 요소 조회(`ByName` / `ByControlType` / `ByClassName` / `ByAutomationId`)도 픽셀 위치와 무관하게 라이브 트리에서 해석된다. 그러나 좌표 기반 입력(`Mouse.MoveTo` 또는 SendInput 의 고정 좌표)과 고정 crop 사각형은 특정 레이아웃 스냅샷을 코드에 박아 넣은 것이라, 창이 리사이즈되면 그 스냅샷이 stale 해진다.

**Effect** — 좌표·고정 crop 기반 캡처/조작은 창 리사이즈 후 스크린샷이 밀리거나 잘리고, 클릭이 엉뚱한 지점에 떨어진다. 식별자 기반 코드는 영향이 없고 변경된 크기로 정상 재캡처된다. 결과적으로 대상 창의 크기를 바꾼 뒤 전체 UI 캡처 스위트를 재실행하면, 식별자 기반 단계는 **코드 수정 없이** 통과하고 캡처 산출물만 새 크기로 갱신된다.

### Proposal (concrete change)

FlaUI 기반 UI 테스트/캡처 스킬(또는 신규 rule)에 다음 원칙을 추가:

- 캡처·조작 대상은 **항상 UIA 식별자**(`ByName` / `ByControlType` / `ByClassName` / `ByAutomationId`)로 찾는다. 화면 절대 좌표·고정 crop 사각형·고정 offset 상수는 피한다.
- 부분 영역 캡처가 필요하면 요소의 **런타임 `BoundingRectangle`** 에서 사각형을 계산한다(고정 픽셀 상수로 crop 금지). 필요한 여백·확장도 bounds 기준 상대값으로 둔다.
- 이 원칙을 지키면 대상 창/다이얼로그의 크기·레이아웃이 바뀌어도(콘텐츠 추가, `SizeToContent`, DPI 스케일 변화) 캡처/자동화가 깨지지 않고 새 상태로 재생성된다. 회귀 검증 시 "창 크기 변경 → 캡처 스위트 전체 재실행 → 코드 수정 없이 통과 + 캡처만 갱신"을 기대치로 명시한다.

### Adjacent skill boundaries / cross-links

- cross-process 입력 주입(SendInput vs FlaUI `Mouse`/`Keyboard`)·DPI 인지(Per-Monitor-V2) 관련 피드백과 인접하나 경계가 다르다 — 그 문서는 "입력을 어떻게 주입하는가", 본 항목은 "요소를 어떻게 앵커링하면 리사이즈에 강건한가"이다. 두 문서를 cross-link 한다.
- DPI 좌표 정렬(가상화 좌표 ↔ `CopyFromScreen` 정렬) 항목과도 인접 — DPI 항목은 "좌표계가 맞는가", 본 항목은 "좌표 자체에 의존하지 않는가"로 보완 관계.
