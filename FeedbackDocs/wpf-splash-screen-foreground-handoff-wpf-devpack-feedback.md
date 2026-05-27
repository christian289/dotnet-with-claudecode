# wpf-dev-pack 피드백 — WPF Splash Screen + Cross-Thread Foreground Handoff 패턴

- **작성일**: 2026-05-27
- **출처**: SMVT (Prism 9 + WPF-UI 기반 머신비전 검사 WPF 앱) — splash close 직후 사용자의 다른 앱 창이 MainWindow 위로 튀어나오는 버그를 추적·수정한 세션
- **목적**: 별도 STA thread + 전용 `Dispatcher` 로 호스팅되는 splash window 가 main window 와 owner 관계를 맺을 수 없는 WPF 의 구조적 제약에서 발생하는 foreground 인계 회귀, 그리고 그 fix 와 함께 들어오는 shutdown race 보호 패턴이 현재 wpf-dev-pack 어느 스킬에도 포착되어 있지 않음. 동일 패턴을 다음 프로젝트에서 재발견·재발명하는 비용을 줄이는 것이 목표.
- **범위**: 신규 스킬 1개 (`implementing-wpf-splash-screen`), 기존 스킬 보강 1개 (`shutting-down-wpf-gracefully`). 버전 범프·README 동기화 필요.

---

## 0. 요약 (우선순위)

| ID | 종류 | 대상 | 우선순위 | 한 줄 |
|----|------|------|----------|-------|
| 1  | 신규 스킬 | `skills/implementing-wpf-splash-screen/SKILL.md` | High | STA-thread splash + lock-free sentinel + cross-thread foreground 인계 (`MainWindow.Activate()`) 풀 패턴 |
| 2  | 기존 스킬 보강 | `skills/shutting-down-wpf-gracefully/SKILL.md` | Medium | background callback 이 main dispatcher shutdown 과 race 되는 케이스 + `HasShutdownStarted` 가드 + 좁은 catch (InvalidOperationException/TaskCanceledException) 추가 |

---

## 1. 신규 스킬 — `implementing-wpf-splash-screen`

### 근거 (세션 증거)

SMVT 의 부트스트랩(`OnStartup` 직후 `Initialize()` 에서 모듈 로드 + `Task.WaitAll(FB/Preset/Sensor3DAgent/WorkDir/ModelDir)`) 이 main UI thread 를 수 초 점유함. 일반적인 `App.MainWindow = splash; splash.Show();` 패턴으로 띄우면 splash 자체 애니메이션이 freeze 되므로, splash 를 **dedicated STA thread + 전용 Dispatcher** 위에 띄우는 격리 아키텍처가 강제됨.

이 아키텍처는 다음 함정들을 동반:

1. **Show↔Close race**: `OnStartup` 에서 `Show()`, `OnInitialized` 에서 `Close()`. splash thread 가 아직 `_window` 를 채우기 전에 `Close()` 가 도착 가능. `MRESlim`/`TaskCompletionSource` 가 흔히 쓰이지만 SMVT 에서는 `Interlocked.CompareExchange` 로 `null → close 콜백 → ReadySentinel` 3-state sentinel 을 atomic 으로 전이하는 **lock-free** 구현으로 해결.

2. **Cross-thread Owner 불가**: WPF `Window.Owner` 는 같은 dispatcher (=같은 thread) 의 두 window 만 연결 가능. splash 가 별도 thread 에 있어 `splash.Owner = MainWindow` 가 *원천적으로 불가능*.

3. **Foreground 인계 회귀 (핵심)**: 위 (2) 때문에 splash 가 foreground 인 채로 `Close()` 되면, Win32 가 다음 foreground 를 owner chain 폴백으로 찾지 못하고 글로벌 Z-order 에서 골라잡음. 결과적으로 **직전 foreground 였던 사용자의 다른 앱이 MainWindow 위로 튀어나옴**. 사용자는 "SMVT splash 가 사라지면 갑자기 브라우저가 앞에 떠 있다" 형태로 인식.

   **Fix**: `fade.Completed` (splash thread) 안에서 `Application.Current?.Dispatcher.Invoke(() => Application.Current.MainWindow?.Activate())` 를 splash `Close()` *직전* 동기 실행 → main thread 가 명시적으로 activation 을 가져간 뒤 splash 가 사라지면 인계 대상이 자연스럽게 MainWindow 로 확정.

   **순서 제약**: Close 후 Activate 하면 SMVT 가 이미 foreground process 가 아니라 `SetForegroundWindow` 가 거부되어 taskbar flash 만 발생. 반드시 Activate → Close 순서.

4. **`AllowsTransparency=True` + `WindowStyle="None"` 시 cross-thread Freezable 위험**: splash 가 별도 dispatcher 라 `App.xaml` 의 `ResourceDictionary` (WPF-UI ThemesDictionary 등) 에 의존하면 cross-thread Freezable 이슈가 발생 가능. splash 는 인라인 hex color + 기본 WPF primitive (`Border`/`StackPanel`/`Image`/`TranslateTransform`) 만으로 구성해야 안전.

5. **`BitmapImage` 로드 시점**: splash thread 에서 직접 `BitmapImage` 생성 후 `Freeze()` 권장. `pack://application:,,,/Resources/Images/splash.png` 같은 pack URI resolver 는 프로세스 전역이라 thread 무관.

6. **라이선스/모달 타이밍 정책**: 라이선스 dialog 가 `Initialize()` 완료 후 `OnInitialized()` 에서 표시되는 SMVT 같은 구조는 "처음부터 splash 띄우기" 전략이 맞지만, 사용자 입력 단계 (license dialog 등) 직전엔 `Close()` 호출해 포커스를 양보해야 함. `Close()` 는 idempotent 여야 (`MainWindow.ContentRendered` fallback 과 중복 호출 안전).

### 제안 (구체 변경)

새 스킬 `skills/implementing-wpf-splash-screen/SKILL.md` 신설. 다음 섹션 구성:

- **Use When**: 부트스트랩이 main UI thread 를 길게 점유하는 WPF 앱에 splash 를 추가할 때 / splash close 후 다른 앱이 foreground 를 빼앗는 회귀를 디버깅할 때 / 라이선스/모달이 부트스트랩 중간/끝에 끼는 앱 lifecycle 을 설계할 때
- **Do NOT Use When**: WPF `SplashScreen` 클래스(빌드 액션 = SplashScreen)만으로 충분한 정적 이미지 splash 케이스 / WinUI / MAUI
- **워크플로우**:
  1. ISplashScreenService + SplashScreenService (sealed, IDisposable) 인터페이스 분리
  2. STA thread + Dispatcher.Run() 격리 아키텍처
  3. lock-free sentinel 패턴으로 Show↔Close race 해결 (3-state Interlocked 전이)
  4. `App.OnStartup` 에서 즉시 `Show()`, `OnInitialized` 에서 `MainWindow.ContentRendered` 한 번 fallback `Close()`, license dialog 분기에서는 user-driven 단계 진입 직전 `Close()`
  5. **fade.Completed → main dispatcher 동기 invoke `MainWindow.Activate()` → splash window.Close() → dispatcher.InvokeShutdown()** 순서 강제
  6. SplashWindow.xaml 은 인라인 hex + 기본 primitive 만, App.xaml ResourceDictionary 비의존
- **함정 표**: cross-thread Owner 불가, Activate→Close 순서 제약, ResourceDictionary 의존성 금지, Freezable cross-thread 이슈, dispatcher shutdown race (→ 2번 스킬과 cross-link)
- **샘플 코드**: SMVT 의 `SplashScreenService.cs` 가 거의 그대로 reference 구현으로 쓰일 수 있음 (sentinel 패턴 + foreground 인계 + 로깅 포함)

### 인접 스킬과의 차이/링크

- `managing-wpf-application-lifecycle` 과 인접하지만, lifecycle 스킬은 Startup/Exit/SessionEnding/single-instance 등 application 전반 이벤트가 주제 — splash 의 *별도 thread 호스팅 + foreground 인계* 라는 좁은 패턴은 다루지 않으므로 분리하는 게 맞음. lifecycle 스킬에서 신규 스킬로 cross-link.
- `threading-wpf-dispatcher` 와 인접 — 일반적 Dispatcher 패턴 위에서, splash 의 *전용 dispatcher* 라는 특수 케이스를 다룸. cross-link.
- `shutting-down-wpf-gracefully` 와 인접 — `Dispose()` safety-net 에서 race 발생 시 처리 패턴은 2번 항목으로 분리하고 cross-link.

---

## 2. 기존 스킬 보강 — `shutting-down-wpf-gracefully` 에 background-callback / main-dispatcher race 절 추가

### 근거 (세션 증거)

splash foreground 인계 fix 를 PR (`#356`) 로 올린 직후 Gemini code review 가 다음 race 를 지적:

> 앱 종료 시점(예: `OnExit`에서 `Dispose`가 호출될 때)에 메인 스레드의 디스패처가 이미 종료 중이거나 종료된 상태일 수 있습니다. 이 경우 `Dispatcher.Invoke`는 `InvalidOperationException`을 발생시키며, 이는 스플래시 스레드(STA 스레드)에서 예외가 처리되지 않아 애플리케이션이 비정상 종료되는 원인이 될 수 있습니다.

실제 시나리오:
- `App.OnExit` → `_splashScreenService.Dispose()` safety-net 경로
- 만약 splash 가 아직 살아 있다면 `Close()` 가 220ms fade 를 시작
- 그 220ms 사이 main dispatcher 가 shutdown 진입
- `fade.Completed` 가 main dispatcher 로 `Invoke(...)` 시도 → `InvalidOperationException` 또는 `TaskCanceledException`
- splash STA thread 에 unhandled exception → 비정상 종료 / 오류 dialog

이 패턴은 splash 뿐 아니라 "별도 thread/timer/background callback 에서 main dispatcher 로 Invoke 하는 모든 코드" 에 일반적으로 적용됨.

SMVT 의 fix:

```csharp
var mainDispatcher = Application.Current?.Dispatcher;
if (mainDispatcher is { HasShutdownStarted: false })
{
    try
    {
        mainDispatcher.Invoke(() =>
        {
            Application.Current?.MainWindow?.Activate();
        });
    }
    catch (InvalidOperationException ex)
    {
        // main dispatcher 가 check 직후 shutdown 진입한 race
        _logger.Debug(ex, "Main dispatcher invoke failed during splash close (shutdown race)");
    }
    catch (TaskCanceledException ex)
    {
        // Invoke 가 shutdown 진행 중 cancel 된 케이스
        _logger.Debug(ex, "Main dispatcher invoke cancelled during splash close (shutdown race)");
    }
}
```

핵심 포인트:
- **`HasShutdownStarted` 가드 + 좁은 catch** 조합 (단순 `catch (Exception)` 광역 catch 는 real bug 도 삼킴)
- `Dispatcher.Invoke` 가 shutdown race 에서 실제로 던지는 두 예외 타입만 swallow (`InvalidOperationException`, `TaskCanceledException`)
- swallow 한 예외는 Debug 레벨로 진단 로그 (silent swallow 금지)

### 제안 (구체 변경)

`skills/shutting-down-wpf-gracefully/SKILL.md` 에 새 절 추가:

**"Background callback 이 main dispatcher shutdown 과 race 되는 경우"** (또는 "Cross-thread Invoke during shutdown" 등)

구성:
1. 문제 패턴: 별도 thread (timer, splash, NATS subscriber callback 등) 에서 `Application.Current?.Dispatcher.Invoke(...)` 호출 → 앱 종료 진행 중이면 `InvalidOperationException` / `TaskCanceledException`
2. anti-pattern: 단순 try/catch (Exception) → real bug 도 swallow
3. 권장 패턴: `HasShutdownStarted` 가드 + 좁은 catch (위 코드 예시)
4. catch 한 예외는 silent 가 아니라 Debug 레벨 로그 (Serilog 이든 ILogger<T> 이든) 로 진단 가능하게
5. 주의: shutdown race 라 logger 자체도 죽어가는 중일 수 있어 best-effort. `?.` null 가드 권장

### 인접 스킬과의 차이/링크

- `preventing-dispatcher-deadlock` 와 인접하지만 그쪽은 *sync-over-async* 데드락이 주제. 이 항목은 *shutdown 시점의 Invoke race* 라 독립 절로 분리하는 게 명확.
- `threading-wpf-dispatcher` 에 짧은 mention 추가하고, full 패턴은 `shutting-down-wpf-gracefully` 로 cross-link.
- 1번 신규 스킬 (`implementing-wpf-splash-screen`) 의 "함정 표" 에서 이 절로 link.
