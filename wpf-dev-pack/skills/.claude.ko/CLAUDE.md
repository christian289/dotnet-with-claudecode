# WPF Dev Pack Skills - 키워드 매핑 & 인덱스

> 본 파일은 `wpf-dev-pack/skills/.claude/CLAUDE.md`의 한국어 미러입니다.
> AI는 영문 원본을 읽으며, 본 파일은 사람을 위한 참고용입니다.

> **📦 보관 처리된 skill**: doc-mirror 계열 skill은 `../../archive-skills/`로
> 이동되었습니다. `microsoft-docs` MCP 플러그인으로 대체된 주제
> (DependencyProperty, ControlTemplate, Storyboard, DragDrop,
> async/await, Span<T> 등)는 `../../archive-skills/` 참조.

---

## 한글 키워드는 의도된 설계

일부 skill 매핑에는 한글 키워드(`차트`, `대시보드`, `뷰모델 테스트`,
`속성 요소 구문` 등)가 포함되어 있습니다. 이는 **번역 누락이 아니라
기능적 트리거**입니다 — 한국어 사용자가 해당 표현을 입력할 때 Claude가
대응 skill을 활성화하도록 합니다. "번역"으로 제거하지 마세요. 데이터
역할이지 문서 역할이 아니므로 저장소 이중 언어 컨벤션에서 제외됩니다.

---

## Keyword-Skill Mapping

### WPF Keywords

| Keyword | Skill |
|---------|-------|
| `customcontrol` | `authoring-wpf-controls` |
| `mvvm`, `viewmodel` | `implementing-communitytoolkit-mvvm` |
| `drawingcontext`, `onrender` | `rendering-with-drawingcontext` |
| `drawingvisual` | `rendering-with-drawingvisual` |
| `resourcedictionary` | `managing-styles-resourcedictionary` |
| `generic.xaml` | `designing-wpf-customcontrol-architecture`, `configuring-wpf-themeinfo` |
| `collectionview` | `managing-wpf-collectionview-mvvm` |
| `binding`, `multibinding`, `prioritybinding`, `relativesource` | `advanced-data-binding` |
| `converter`, `ivalueconverter`, `markupextension` | `using-converter-markup-extension` |
| `property element`, `속성 요소 구문` | `using-xaml-property-element-syntax` |
| `routed event` | `routing-wpf-events` |
| `dispatcher` | `threading-wpf-dispatcher` |
| `virtualizing` | `virtualizing-wpf-ui` |
| `freeze`, `freezable` | `optimizing-wpf-memory` |
| `startup`, `sessionending`, `lifecycle`, `dispatcherunhandledexception`, `unhandled exception` | `managing-wpf-application-lifecycle` |
| `popup focus`, `popupfocus`, `preview mouse down popup` | `managing-wpf-popup-focus` |
| `repository`, `repository pattern`, `service layer` | `implementing-repository-pattern` |
| `console app di`, `generichost console` | `configuring-console-app-di` |
| `rendering architecture`, `wpf rendering pipeline` | `rendering-wpf-architecture` |
| `ui automation discovery`, `flaui element`, `automationelement discovery` | `flaui-wpf-element-discovery` |
| `prism dialog discovery`, `flaui prism dialog` | `flaui-prism-dialog-discovery` |
| `hit test`, `hittest`, `ishittestvisible` | `implementing-hit-testing` |
| `getawaiter`, `getresult`, `.result`, `.wait()`, `sync over async`, `dispatcher deadlock`, `async event handler`, `async void handler` | `preventing-dispatcher-deadlock` |
| `graceful shutdown`, `onexit`, `async cleanup`, `onmainwindowclose`, `onexplicitshutdown`, `window.closing async`, `ihost.stopasync shutdown` | `shutting-down-wpf-gracefully` |
| `performance` | `rendering-wpf-high-performance` |
| `visualtree`, `logicaltree` | `navigating-visual-logical-tree` |
| `validation` | `implementing-wpf-validation` |
| `themeinfo`, `assemblyinfo` | `configuring-wpf-themeinfo` |
| `wpf-ui`, `wpfui`, `fluentwindow`, `navigationview` | `integrating-wpfui-fluent` |
| `livecharts`, `cartesianchart`, `piechart`, `iseries` | `integrating-livecharts2` |
| `fluentvalidation`, `abstractvalidator`, `rulefor` | `validating-with-fluentvalidation` |
| `erroror`, `result pattern`, `error.validation` | `handling-errors-with-erroror` |
| `nodify`, `nodifyeditor`, `node graph`, `node editor` | `integrating-nodify` |
| `flaui`, `cross-process`, `sendinput`, `keybd_event`, `stuck key`, `xunit.runner.json`, `parallelizeTestCollections` | `flaui-cross-process-input` |
| `scottplot`, `mousewheel`, `focus`, `modifier` | `scottplot-syncing-modifier-keys-for-mousewheel` |
| `scottplot margins`, `autoscaler`, `invertedy`, `fractionalautoscaler`, `axis flip`, `scottplot reactive`, `scottplot converter`, `imagerect overlay flip` | `scottplot-axes-margins-destructive` |
| `focus ring clipped`, `hover glow cut`, `decorative overflow`, `clip to bounds ancestor`, `thumb cut at edge` | `containing-control-decorative-overflow` |

### Scaffolding Keywords

| Keyword | Skill |
|---------|-------|
| `create project`, `scaffold`, `new project` | `make-wpf-project` |
| `create customcontrol`, `generate control` | `make-wpf-custom-control` |
| `create usercontrol`, `generate usercontrol` | `make-wpf-usercontrol` |
| `create converter`, `generate converter` | `make-wpf-converter` |
| `create behavior`, `generate behavior` | `make-wpf-behavior` |
| `create viewmodel`, `generate viewmodel`, `viewmodel 생성` | `make-wpf-viewmodel` |
| `create service`, `generate service`, `서비스 생성` | `make-wpf-service` |

### Testing Keywords

| Keyword | Skill |
|---------|-------|
| `viewmodel test`, `unit test`, `xunit`, `nsubstitute` | `testing-wpf-viewmodels` |
| `뷰모델 테스트`, `단위 테스트` | `testing-wpf-viewmodels` |

### Feature Intent Keywords

| Keyword (Intent) | Skill |
|-------------------|-------|
| `차트`, `chart`, `그래프`, `대시보드` | `integrating-livecharts2` |
| `입력 검증`, `폼 검증`, `form validation` | `validating-with-fluentvalidation`, `implementing-wpf-validation` |
| `모던 ui`, `플루언트 디자인`, `fluent design` | `integrating-wpfui-fluent` |
| `노드 편집기`, `워크플로우 편집기`, `visual scripting` | `integrating-nodify` |
| `대용량 데이터`, `느려`, `렉`, `slow`, `lag` | `rendering-wpf-high-performance`, `virtualizing-wpf-ui` |
| `ui 테스트`, `자동화 테스트`, `automation test` | `flaui-cross-process-input`, `flaui-wpf-element-discovery` |

### Prism 9 Keywords

| Keyword | Skill (PRISM.md 참조) |
|---------|----------------------|
| `prism`, `bindablebase`, `delegatecommand` | `implementing-communitytoolkit-mvvm` |
| `prismapplication`, `icontainerregistry` | `configuring-dependency-injection` |
| `regionmanager`, `iregionmanager` | (`rules/view-viewmodel-wiring-prism.md` 참조) |
| `imodule`, `modulecatalog` | `structuring-wpf-projects` |
| `validatablebindablebase` | `implementing-wpf-validation` |

### .NET Keywords

| Keyword | Skill |
|---------|-------|
| `dependency injection` | `configuring-dependency-injection` |

### Configuration & Setup Keywords

| Keyword | Skill |
|---------|-------|
| `language`, `응답 언어`, `set language`, `configure language`, `switch language` | `configuring-wpf-dev-pack-language` |

### Build & Deployment

| Keyword | Skill |
|---------|-------|
| `pdb`, `debugtype`, `source link` | `embedding-pdb-in-exe` |
| `publish`, `deploy`, `release` | `publishing-wpf-apps` |
| `self-contained`, `single-file` | `publishing-wpf-apps` |
| `installer`, `velopack`, `msix`, `nsis` | `publishing-wpf-apps` |

### HandMirror MCP - .NET API 검증

.NET API/NuGet 패키지 정보를 조회할 때, **HandMirrorMcp 도구도 함께
사용**하여 환각을 줄입니다.

**트리거 조건**: .NET/NuGet 관련 조회로 context7 또는 Microsoft Learn
MCP를 사용할 때

**공동 사용 규칙:**

```
WHEN context7 또는 Microsoft Learn으로 .NET/NuGet 정보 조회:
  ALSO HandMirrorMcp 사용해 검증:
    - inspect_nuget_package: NuGet 패키지 내 namespace/type 목록
    - inspect_nuget_package_type: 정확한 메서드 시그니처 조회
    - search_nuget_packages: 키워드로 패키지 검색
    - get_type_info: 로컬 assembly (.dll/.exe) 타입 조사
    - explain_build_error: CS/NU 빌드 에러 진단
    - analyze_csproj: 프로젝트 파일 이슈 분석
```

**사용 시나리오:**
- NuGet 패키지의 API 이름 케이싱 정확성 검증 (예: SQLite vs Sqlite)
- 확장 메서드의 정확한 namespace 식별
- 패키지 버전 간 breaking change 점검
- 빌드 에러(CS0246, NU1605 등) 진단 및 필수 패키지 권고

---

## Skill 카테고리 인덱스

| 카테고리 | Skills |
|----------|--------|
| **UI & Controls** | `authoring-wpf-controls`, `configuring-wpf-themeinfo`, `containing-control-decorative-overflow` |
| **Data Binding & MVVM** | `implementing-communitytoolkit-mvvm`, `advanced-data-binding`, `implementing-wpf-validation`, `managing-wpf-collectionview-mvvm`, `using-converter-markup-extension`, `configuring-dependency-injection` |
| **Performance & Rendering** | `rendering-with-drawingcontext`, `rendering-with-drawingvisual`, `rendering-wpf-high-performance`, `virtualizing-wpf-ui`, `optimizing-wpf-memory`, `implementing-hit-testing` |
| **Input & Events** | `routing-wpf-events` |
| **Styling & Resources** | `managing-styles-resourcedictionary`, `using-xaml-property-element-syntax` |
| **Application** | `managing-wpf-application-lifecycle`, `threading-wpf-dispatcher`, `preventing-dispatcher-deadlock`, `shutting-down-wpf-gracefully` |
| **UI Interaction** | `managing-wpf-popup-focus`, `binding-enum-command-parameters`, `checking-image-bounds-transform`, `displaying-slider-index`, `highlighting-nodify-connections`, `resolving-icon-font-inheritance` |
| **Data Access** | `implementing-repository-pattern`, `configuring-console-app-di` |
| **Architecture & Tree Navigation** | `navigating-visual-logical-tree`, `rendering-wpf-architecture`, `structuring-wpf-projects`, `designing-wpf-customcontrol-architecture` |
| **Build & Deployment** | `embedding-pdb-in-exe`, `publishing-wpf-apps` |
| **3rd Party Libraries** | `integrating-wpfui-fluent`, `integrating-livecharts2`, `validating-with-fluentvalidation`, `handling-errors-with-erroror`, `integrating-nodify`, `flaui-cross-process-input`, `flaui-wpf-element-discovery`, `flaui-prism-dialog-discovery`, `scottplot-syncing-modifier-keys-for-mousewheel`, `scottplot-axes-margins-destructive` |
| **Testing** | `testing-wpf-viewmodels`, `managing-unit-tests` |
| **Scaffolding** | `make-wpf-project`, `make-wpf-custom-control`, `make-wpf-usercontrol`, `make-wpf-converter`, `make-wpf-behavior`, `make-wpf-viewmodel`, `make-wpf-service` |
| **Meta / Maintenance** | `collecting-wpf-dev-pack-feedback`, `configuring-wpf-dev-pack-language` |
