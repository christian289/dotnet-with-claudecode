# wpf-dev-pack Feedback — FlaUI cross-process input & DPI coordinate pitfalls

- **Purpose**: FlaUI 기반 WPF UI 자동화 작업 중 손으로 해결한 cross-process 입력/DPI 좌표 함정 3가지가 `flaui-cross-process-input` 스킬에 아직 없거나 불충분하다. 같은 함정을 반복 디버깅하지 않도록 스킬에 반영하기 위함.
- **Scope**: 기존 스킬 보강 4건(신규 스킬 없음). 모두 `skills/flaui-cross-process-input/SKILL.md` 대상. 스킬 본문 변경이므로 패치 버전 범프 + README 스킬 카탈로그 설명 동기화 권장.

---

## 0. Summary (priority)

| ID | Kind | Target | Priority | One-liner |
|----|------|--------|----------|-----------|
| 1 | Augment | skills/flaui-cross-process-input/SKILL.md | High | 앱 내부 논리 단위(DIP) 기반 드롭/클릭 좌표는 DPI-aware 테스트 프로세스에서 effective DPI 배율을 곱해야 물리 픽셀에 정확히 명중한다 |
| 2 | Augment | skills/flaui-cross-process-input/SKILL.md | High | WPF TextBox에 `Keyboard.Type`이 예외 없이 조용히 실패하면 `ValuePattern.SetValue`로 TwoWay 바인딩을 직접 갱신한다 |
| 3 | Augment | skills/flaui-cross-process-input/SKILL.md | High | FlaUI `Mouse.*`/`Keyboard.*`/`Click()`이 access-denied·`COMException`을 던지는 환경에서는 전면 P/Invoke(`SendInput`/`keybd_event`) + 입력 전 `SetForegroundWindow` + 제스처 전 stuck-modifier 해제 |
| 4 | Augment | skills/flaui-cross-process-input/SKILL.md | Medium | `Mouse.MoveTo`는 보간(애니메이션)이라 긴 이동이 느림 — 비드래그 즉시 이동은 `Mouse.Position`(단일 `SetCursorPos`) |

---

## 1. 논리 단위(DIP) 좌표는 effective DPI 배율로 스케일해야 한다

### Phenomenon and causality
- **Phenomenon**: 화면 캡처(`CopyFromScreen`)와 UIA `BoundingRectangle` 좌표를 정렬하려 테스트 프로세스를 Per-Monitor-V2 DPI-aware로 만든 뒤(`SetProcessDpiAwarenessContext`/`SetThreadDpiAwarenessContext`), 앱 내부의 **논리 단위(DIP)** 로 정의된 드롭/클릭 타깃(예: 노드 기반 에디터(Nodify) 캔버스의 grid 셀 크기·셀 피치 같은 레이아웃 상수)으로 스크린 좌표를 계산하면, 고배율(예: 150%) 환경에서 대상 영역 안에는 들어가나 중앙에서 배율만큼 어긋난다.
- **Cause**: UIA `BoundingRectangle`는 물리 픽셀을 돌려주지만, 앱의 grid/레이아웃 상수는 DIP다. DPI-aware 프로세스에서 화면은 물리 픽셀로 렌더되므로, DIP 상수를 그대로 스크린 오프셋에 더하면 `(physical/DIP)` 배율만큼 부족한 좌표가 된다.
- **Effect**: 드롭/클릭이 대상 안에는 떨어지지만 정확한 중앙이 아니어서, 중앙 기준 스냅·정렬·국소 캡처가 어긋난다. 100%/저배율에서는 배율이 1.0이라 재현되지 않아 발견이 늦다.

### Proposal (concrete change)
`skills/flaui-cross-process-input/SKILL.md`의 "UIA BoundingRectangle 기반 좌표 계산" 섹션에 하위 항목 추가:
- UIA `BoundingRectangle`는 물리 픽셀이므로 그대로 쓰면 되지만, **앱 내부 논리 단위(DIP)** 로 좌표를 계산할 때(캔버스 셀 크기/피치, ItemsControl 항목 간격 등)는 테스트 프로세스가 DPI-aware인 경우 **effective DPI 배율을 곱해야** 한다.
- effective scale 산출 스니펫: `GetThreadDpiAwarenessContext` → `GetAwarenessFromDpiAwarenessContext`(unaware면 1.0) → aware면 `GetDpiForWindow(hwnd) / 96.0`.
- 증상 단서: "대상 안엔 들어가는데 중앙에서 배율만큼 밀린다", "150%에서만 어긋난다".

### Adjacent skill boundaries / cross-links
`flaui-cross-process-input`의 좌표 계산 섹션에 붙는 것이 자연스럽다(이미 BoundingRectangle 기반 좌표를 다룸). "요소를 못 찾는" 문제는 `flaui-wpf-element-discovery` 소관이라 별개다. cross-link 한 줄 권장: "UIA 좌표=물리 픽셀, 앱 논리 좌표=DIP — 혼용 시 DPI 배율 보정".

---

## 2. `Keyboard.Type`이 조용히 실패하는 TextBox는 `ValuePattern.SetValue`로 채운다

### Phenomenon and causality
- **Phenomenon**: cross-process FlaUI에서 WPF TextBox(예: 검색/필터 입력 필드)에 `element.Click()` 후 `Keyboard.Type(text)`로 입력하면, 예외 없이 텍스트가 들어가지 않는 경우가 잦다.
- **Cause**: cross-process 키 주입이 포커스/타이밍에 민감해 키스트로크가 WPF 입력 큐에 도달하지 못하고 silent drop된다. 대상 WPF TextBox는 UIA `ValuePattern`을 지원한다.
- **Effect**: 입력 필드가 비어 후속 단계(필터 결과 0건 등)가 먼저 실패하고, 정작 근본 원인(입력 미반영)은 예외가 없어 추적이 어렵다.

### Proposal (concrete change)
`skills/flaui-cross-process-input/SKILL.md`의 키보드 입력 섹션(`Keyboard.Press` vs `Type` 표 근처)에 추가:
- "cross-process에서 `Keyboard.Type`이 WPF TextBox에 조용히 실패(텍스트 미입력)하면, 키 주입에 매달리지 말고 `ValuePattern`(`element.Patterns.Value`)으로 `SetValue(text)`하라. WPF TwoWay 바인딩이 즉시 갱신된다. `Click`+`Ctrl+A`+`Type`은 last-resort 폴백."
- 함정 명시: 이 실패는 "예외 없이 그냥 텍스트가 안 들어감"이라 조용해서, 후속 증상이 먼저 터져 근본 원인 추적이 어렵다.

### Adjacent skill boundaries / cross-links
`flaui-cross-process-input`(입력 신뢰성)에 둔다. 검색/필터 입력값 설정은 `flaui-wpf-element-discovery`와도 맞닿으니, 후자에서 "값 설정은 ValuePattern" cross-link 한 줄을 둔다.

---

## 3. FlaUI 입력 API가 access-denied·COMException을 던지면 전면 P/Invoke로 전환한다

### Phenomenon and causality
- **Phenomenon**: 특정 세션/보안 컨텍스트에서 FlaUI `Mouse.*`, `Keyboard.*`, `element.Click()` 호출이 Win32 access-denied, 또는 `NoClickablePoint`/`COMException`을 던진다.
- **Cause**: 해당 환경에서 FlaUI 입력 추상 레이어가 권한 또는 clickable-point 확보에 실패한다.
- **Effect**: 클릭·드래그·키 입력 전반이 불안정해 시나리오가 비결정적으로 깨진다.

### Proposal (concrete change)
`skills/flaui-cross-process-input/SKILL.md`에 "FlaUI 입력 API가 예외를 던지는 환경" 처방을 추가(기존 `SetCursorPos` vs `SendInput` hit-test, `ReleaseAllKeys` 항목에 덧대기 — 중복 신규 항목 만들지 말 것):
- 증상: `Mouse.*`/`Keyboard.*`/`element.Click()`이 access-denied, `NoClickablePoint`, `COMException`을 던짐.
- 처방: 포인터 입력은 `SendInput`, 키 입력은 `keybd_event`로 down/up을 직접 전송해 FlaUI 입력 레이어를 우회한다.
- **필수 전제 2가지**:
  - (a) 입력 전 `SetForegroundWindow` — `SendInput`은 전역 입력이라 대상 창이 비포그라운드면 입력이 다른 활성 창으로 떨어져 오동작 + 사용자 작업 방해.
  - (b) 제스처 전 stuck modifier 해제 — key-down만 보내는 hold API가 modifier를 잔류시키면(`Press` 후 `Release` 누락) 다음 제스처가 오염된다(예: Ctrl 잔류 → 좌클릭이 캔버스 pan으로 처리). 매 제스처 전 잔류 키를 release한다.

### Adjacent skill boundaries / cross-links
정확히 `flaui-cross-process-input` 범주(입력 주입 신뢰성). `ReleaseAllKeys` vs `ReleaseModifierKeys`, `SetCursorPos` vs `SendInput`는 이미 스킬에 있으니, 그 항목들에 "FlaUI 입력 API 자체가 throw하면 전면 P/Invoke 전환 + Foreground 전제"를 덧대는 형태가 적절하다.

---

## 4. `Mouse.MoveTo`는 보간(애니메이션)이라 느리다 — 비드래그 즉시 이동은 `Mouse.Position`

### Phenomenon and causality
- **Phenomenon**: 여러 항목을 순회하며 매 반복마다 커서를 한 지점에서 멀리 떨어진 다른 지점으로 옮기는 루프에서, 드래그 시작 전 단순 위치 이동인데도 커서 이동이 눈에 띄게 느리다(부드러운 애니메이션).
- **Cause**: FlaUI `Mouse.MoveTo(pt)`는 단일 `SetCursorPos`가 아니라 `Mouse.MovePixelsPerMillisecond` 기반 다단계 보간(애니메이션)으로 이동한다. "MoveTo가 SetCursorPos를 쓴다"는 단일 호출식 서술은 이 보간/지연을 가린다.
- **Effect**: 긴 거리 reposition이 반복되면 테스트 전체가 느려진다. 100%·짧은 이동에서는 체감되지 않아 발견이 늦다.

### Proposal (concrete change)
`skills/flaui-cross-process-input/SKILL.md`의 hit-test 섹션(Problem 2)에 하위 절 추가:
- `Mouse.MoveTo`는 `MovePixelsPerMillisecond` 기반 보간이라 긴 이동이 느리다는 점 명시.
- 비드래그 단순 reposition은 `Mouse.Position = pt`(단일 `SetCursorPos`, 즉시) 사용. WPF hit-test 갱신이 필요하면 여전히 `SendInput(MOUSEEVENTF_MOVE|ABSOLUTE)`. 드래그 *중*에는 보간 이동 유지(연속 `DragOver` 필요).
- 용도 구분 표(`MoveTo` 보간 / `Mouse.Position` 즉시 / `SendInput` hit-test) 권장.

### Adjacent skill boundaries / cross-links
Problem 2(`SetCursorPos`가 WPF hit-test를 갱신 안 함 → `SendInput`)와 보완 관계. hit-test 갱신 필요=`SendInput`, 클릭/드래그 없는 즉시 위치이동=`Mouse.Position`, 드래그 중=보간. 세 용도를 표로 분리.

---

> 추가 후보(이번 문서엔 미포함): 노드 기반 에디터(Nodify)의 노드/소켓 UIA 탐색은 `flaui-wpf-element-discovery` 소관 — 노드는 ItemContainer에 부여된 AutomationId로 식별되나, 소켓 커넥터는 AutomationId 없는 ItemsControlItem이라 개수로 식별해야 하고, WPF `ContextMenu`는 별도 popup이라 메인 윈도우 트리에 안 잡혀 desktop top-level 폴백이 필요하다.
