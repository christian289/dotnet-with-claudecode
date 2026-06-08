# wpf-dev-pack Feedback — MSIX + Windows App SDK packaging/runtime gotchas for WPF

- **Purpose**: WPF 앱을 MSIX 로 패키징하고 Windows App SDK(self-contained)를 도입할 때 발생하는 비자명한 빌드/런타임 실패들이 현재 publishing/lifecycle 지식 토픽에 없어, 매번 시행착오로 재발견된다. 이를 토픽 보강으로 encode 한다.
- **Scope**: publishing-wpf-apps augment 3건(리소스 Content 글로빙, 패키징 mechanics, 네이티브 DLL PATH 검색), managing-wpf-application-lifecycle augment 1건(프로그래밍 재시작). 지식 토픽 변경이므로 플러그인 version bump + README 의 토픽 목록 동기화 필요.

---

## 0. Summary (priority)

| ID | Kind | Target | Priority | One-liner |
|----|------|--------|----------|-----------|
| 1 | Augment / Rule | publishing-wpf-apps | High | WASDK self-contained 가 켜는 기본 Content 글로빙이 pack:// 이미지/아이콘 리소스를 깨뜨림 |
| 2 | Augment | publishing-wpf-apps | Medium | WASDK self-contained + custom-manifest MSIX 패키징 mechanics(다중 exe, build-time 번들) |
| 3 | Augment | managing-wpf-application-lifecycle | High | packaged WPF 프로그래밍 재시작은 CoreApplication.RequestRestartAsync 가 아니라 AppInstance.Restart |
| 4 | Augment / Rule | publishing-wpf-apps | Medium | packaged 앱은 네이티브 DLL 검색에 PATH 환경변수를 쓰지 않음 |

---

## 1. WASDK self-contained 가 켜는 기본 Content 글로빙이 pack:// 이미지/아이콘 리소스를 깨뜨림

### Phenomenon and causality
- **Phenomenon**: WPF 앱에 `Microsoft.WindowsAppSDK`(self-contained)를 추가한 뒤 두 가지 증상이 나타난다.
  - (a) 이미지/스플래시용 `BitmapImage`(`UriSource="pack://application:,,,/Resources/Images/<image>.png"`)가 `InitializeComponent()` 단계에서 `System.Windows.Markup.XamlParseException`(inner `System.IO.DirectoryNotFoundException`, 스택에 `MS.Internal.AppModel.ContentFilePart.GetStreamCore`)으로 크래시한다.
  - (b) 그 WPF 앱을 `ProjectReference` 하는 하위/테스트 프로젝트의 빌드가 `MSB3030`("`...\Resources\<icon>.ico` 파일을 찾을 수 없으므로 복사할 수 없습니다")으로 실패한다.
- **Cause**: Windows App SDK 도입이 `EnableDefaultContentItems` 류 기본 Content 글로빙을 활성화하여, 원래 `<Resource>`(임베드) 또는 `<ApplicationIcon>`로만 두던 `*.png`/`*.ico` 가 **추가로 `Content` 로도 분류**된다. `Content` 로 분류되면
  - (a) WPF 가 `AssemblyAssociatedContentFileAttribute` 를 생성해 `pack://application:,,,` URI 를 임베드 리소스가 아니라 **출력 폴더의 느슨한 파일**로 해석한다. 그 파일이 출력에 없으면(`CopyToOutputDirectory` 미설정) 못 찾는다.
  - (b) 그 `Content` 가 `ProjectReference` 를 통해 referencing 프로젝트의 transitive copy 대상이 되는데, 원본이 출력에 없어 복사가 실패한다.
- **Effect**: 앱 시작 시 스플래시/이미지 `BitmapImage` 크래시(프로세스 종료). 솔루션 CLI 빌드가 `MSB3030` 으로 실패(IDE 단독 빌드에선 경로가 달라 안 보일 수 있어 늦게 발견). 패키지/비-패키지 양쪽에서 동일 발생.

### Proposal (concrete change)
- publishing-wpf-apps 토픽(또는 신규 rule)에 "Windows App SDK 도입 시 리소스 Content 글로빙 주의" 섹션 추가:
  - 증상 2종(`pack://` `BitmapImage` 크래시, referencing 프로젝트 `MSB3030`)과 식별 단서(`ContentFilePart`, `MSB3030`, inner `DirectoryNotFoundException`)를 명시.
  - 해법: 해당 리소스를 `<Content Remove="...png" />` / `<Content Remove="...ico" />` 로 기본 Content 글로빙에서 제외하고, 이미지/스플래시는 `<Resource>`(임베드)로, 앱 아이콘은 `<ApplicationIcon>`(exe 임베드)로만 유지. 앱 아이콘 ICO 의 Content 항목은 보통 vestigial(MSIX 로고 자산은 별도 PNG 경로로 생성되므로 불필요).
  - "WASDK 추가 후 새 이미지/아이콘 리소스를 넣을 때 같은 글로빙 함정 재발 주의" 메모.

### Adjacent skill boundaries / cross-links
- publishing-wpf-apps(배포/패키징)와 일반 WPF 리소스(pack URI/임베드) 지식의 경계. Item 2(WASDK 패키징 mechanics)와 형제 — 둘 다 "WASDK self-contained 도입 부작용".

---

## 2. WASDK self-contained + custom-manifest MSIX 패키징 mechanics

### Phenomenon and causality
- **Phenomenon**: WPF 앱을 custom manifest 기반 CLI MSIX 파이프라인(매니페스트를 매 빌드 재생성/덮어쓰는 류)으로 패키징하면서 WASDK self-contained 를 쓰면, 패키징 단계가 "입력 폴더에 `.exe` 가 여러 개라 매니페스트의 `$targetnametoken$.exe` placeholder 가 어느 exe 인지 결정할 수 없다"는 취지로 실패한다.
- **Cause**: self-contained Windows App SDK 는 런타임을 `dotnet build` 출력에 번들하면서 보조 실행파일(예: 재시작을 수행하는 helper exe)도 함께 출력에 둔다. 그 결과 앱 exe 외 추가 exe 가 패키지 입력 폴더에 존재해, placeholder 기반 도구가 메인 exe 를 특정하지 못한다.
- **Effect**: 다중 exe 모호성으로 MSIX 패키지 생성 실패.

### Proposal (concrete change)
- publishing-wpf-apps 토픽에 "WASDK self-contained + custom-manifest MSIX" 메모:
  - self-contained 는 런타임을 **build 출력**에 번들(publish 전용 아님) → 출력 폴더를 통째 복사하는 패키징 방식과 호환.
  - 다중 exe 모호성은 패키징 도구의 "메인 exe 명시" 옵션(예: `--executable <app.exe>`)으로 해소.
  - framework-dependent 대안 비교: 매니페스트에 framework `PackageDependency` 선언 + 타깃 머신에 런타임 사전설치 필요. 매니페스트를 매번 덮어쓰는 custom-manifest 파이프라인에선 self-contained 가 마찰이 더 적다(매니페스트에 의존성 주입 불필요).

### Adjacent skill boundaries / cross-links
- Item 1과 형제. lifecycle Item 3(AppInstance.Restart 가 self-contained 도입의 주된 동기)과 연결.

---

## 3. packaged WPF 프로그래밍 재시작: CoreApplication.RequestRestartAsync 가 아니라 AppInstance.Restart

### Phenomenon and causality
- **Phenomenon**: 패키지(MSIX, FullTrust mediumIL) WPF 앱에서 `Windows.ApplicationModel.Core.CoreApplication.RequestRestartAsync` 를 UI 스레드에서 호출하면 **반환조차 하지 않고 데드락**한다(호출의 동기 구간이 블록되어, 이를 감싼 `Task.WhenAny(call.AsTask(), Task.Delay(...))` 타임아웃조차 발화하지 않음). 같은 호출을 백그라운드 스레드로 옮기면 데드락은 사라지지만 **프로세스 종료만 되고 relaunch 가 일어나지 않는다**(foreground 컨텍스트 상실).
- **Cause**: `RequestRestartAsync` 는 UWP CoreApplication 뷰(CoreDispatcher/CoreWindow)를 전제로 한 async API 인데, FullTrust WPF 패키지 앱(`Windows.FullTrustApplication`)에는 그 코어 뷰가 없다. relaunch 는 호출 컨텍스트의 foreground 상태에 의존한다.
- **Effect**: 재시작이 hang 하거나(UI 스레드) 종료-후-미기동(백그라운드)이 되어, 어느 쪽도 정상 재시작이 안 된다. 반환값을 버리면 사용자에겐 "버튼 눌렀는데 아무 일도 없음"으로 보인다.

### Proposal (concrete change)
- managing-wpf-application-lifecycle 토픽에 "패키지 앱 프로그래밍 재시작" 섹션 추가:
  - 패키지 WPF/Win32 는 `Microsoft.Windows.AppLifecycle.AppInstance.Restart(args)`(Windows App SDK, **동기**, `Windows.ApplicationModel.Core.AppRestartFailureReason` 반환) 사용. 동기라 UI 스레드(=foreground)에서 호출해도 데드락 없고 foreground 가 유지되어 종료+재기동이 정상 동작.
  - 반환된 `AppRestartFailureReason`(`NotInForeground`/`Other` 등)을 반드시 확인해 silent failure 방지(성공 시 프로세스가 종료되어 다음 줄에 도달하지 않으므로, 도달 = 실패).
  - 재시작 커맨드는 보통 `async void` → 메서드 전체를 try-catch 로 감싸 `Process.Start`/`AppInstance.Restart` 예외 시 사용자 안내. `async Task` 로 바꾸려면 `DelegateCommand(Action)` 와 시그니처가 안 맞아(`Func<Task>` 미수용) `AsyncDelegateCommand` 가 필요하고, 그래도 try-catch(또는 커맨드 자체 예외 핸들링)는 별도로 필요 — 전환만으론 견고성 미해결.
  - 비-패키지(개발/unpackaged)는 `Process.Start(Environment.ProcessPath)` + `Application.Current.Shutdown()`(단 `Environment.ProcessPath` null 가드). 패키지 exe 를 직접 `Process.Start` 하면 package identity 가 유실되므로 금지.

### Adjacent skill boundaries / cross-links
- managing-wpf-application-lifecycle(시작/종료/SessionEnding)와 경계. Item 1/2(WASDK 도입)와 연결 — self-contained WASDK 가 이 API 를 쓰는 직접 동기.

---

## 4. packaged 앱은 네이티브 DLL 검색에 PATH 환경변수를 쓰지 않음

### Phenomenon and causality
- **Phenomenon**: 비-패키지(개발) 실행에선 정상 로드되던 네이티브 DLL(P/Invoke 대상)이, 동일 코드를 MSIX 패키지로 설치해 실행하면 `System.DllNotFoundException: Unable to load DLL '<native>.dll' or one of its dependencies (0x8007007E)` 로 실패한다. 해당 네이티브 SDK 는 시스템(Program Files)에 설치되어 **PATH 에만 노출**되고(앱 폴더에 미번들), 의존 DLL 들이 여러 PATH 디렉터리에 분산돼 있다.
- **Cause**: 패키지(MSIX) 앱은 네이티브 DLL 검색에 **PATH 환경변수를 사용하지 않는다**(앱 디렉터리 / System32 등 제한된 검색). 비-패키지 앱은 PATH 를 검색하므로 개발 환경에선 로드된다.
- **Effect**: PATH 에만 설치된 서드파티 네이티브 SDK(및 분산된 의존성)를 패키지 앱이 못 찾아 `DllNotFoundException`. 자식 console 프로세스(별도 exe)에서 P/Invoke 하는 경우에도 동일.

### Proposal (concrete change)
- publishing-wpf-apps 토픽(또는 신규 rule)에 "패키지 앱의 네이티브 DLL 검색" 메모:
  - 패키지 앱은 PATH 를 네이티브 DLL 검색에 쓰지 않음 → PATH 에만 있는 네이티브 의존성은 명시적으로 검색 경로에 추가해야 함.
  - 복구: 시작 시 `SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS)` 호출 후 PATH 의 각 디렉터리를 `AddDllDirectory` 로 등록(`LibraryImport`/`DllImport` P/Invoke). 의존성이 여러 PATH 디렉터리에 분산된 경우 단일 디렉터리만 추가하면 부족하므로, PATH 의 모든(존재하는) 디렉터리 추가가 견고.
  - 패키지일 때만 적용하도록 게이트 권장(비-패키지는 loader 가 PATH 를 정상 검색하므로 불필요). 자식 프로세스에 적용 시, 부모가 packaged 판정을 커맨드 인자(예: `--use-path-dll-search`)로 전달하는 패턴.

### Adjacent skill boundaries / cross-links
- publishing-wpf-apps(배포/런타임 의존성)와 경계. Item 3(packaged 자식 프로세스 제어)과 자식 exe 인자 전달 패턴에서 연결.
