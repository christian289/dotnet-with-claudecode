# WPF Dev Pack Skills - Keyword Mapping & Index

> **📦 Archived skills**: doc-mirror skills have been moved to `../../archive-skills/`.
> See `../../archive-skills/` for topics covered by the `microsoft-docs` MCP plugin
> (e.g., DependencyProperty, ControlTemplate, Storyboard, DragDrop, async/await, Span<T>).

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

| Keyword | Skill (see PRISM.md) |
|---------|----------------------|
| `prism`, `bindablebase`, `delegatecommand` | `implementing-communitytoolkit-mvvm` |
| `prismapplication`, `icontainerregistry` | `configuring-dependency-injection` |
| `regionmanager`, `iregionmanager` | (see `rules/view-viewmodel-wiring-prism.md`) |
| `imodule`, `modulecatalog` | `structuring-wpf-projects` |
| `validatablebindablebase` | `implementing-wpf-validation` |

### .NET Keywords

| Keyword | Skill |
|---------|-------|
| `dependency injection` | `configuring-dependency-injection` |

### Build & Deployment

| Keyword | Skill |
|---------|-------|
| `pdb`, `debugtype`, `source link` | `embedding-pdb-in-exe` |
| `publish`, `deploy`, `release` | `publishing-wpf-apps` |
| `self-contained`, `single-file` | `publishing-wpf-apps` |
| `installer`, `velopack`, `msix`, `nsis` | `publishing-wpf-apps` |

### HandMirror MCP - .NET API Verification

When querying .NET API/NuGet package information, **also use HandMirrorMcp tools** to reduce hallucinations.

**Trigger condition**: When using context7 or Microsoft Learn MCP for .NET/NuGet related queries

**Co-usage rules:**

```
WHEN using context7 or Microsoft Learn for .NET/NuGet info:
  ALSO use HandMirrorMcp to verify:
    - inspect_nuget_package: List namespaces/types in a NuGet package
    - inspect_nuget_package_type: Get exact method signatures
    - search_nuget_packages: Search packages by keyword
    - get_type_info: Inspect local assembly (.dll/.exe) types
    - explain_build_error: Diagnose CS/NU build errors
    - analyze_csproj: Analyze project file for issues
```

**Usage scenarios:**
- Verify API name casing accuracy in NuGet packages (e.g., SQLite vs Sqlite)
- Identify correct namespaces for extension methods
- Check API breaking changes across package versions
- Diagnose build errors (CS0246, NU1605, etc.) and recommend required packages

---

## Skill Category Index

| Category | Skills |
|----------|--------|
| **UI & Controls** | `authoring-wpf-controls`, `configuring-wpf-themeinfo` |
| **Data Binding & MVVM** | `implementing-communitytoolkit-mvvm`, `advanced-data-binding`, `implementing-wpf-validation`, `managing-wpf-collectionview-mvvm`, `using-converter-markup-extension`, `configuring-dependency-injection` |
| **Performance & Rendering** | `rendering-with-drawingcontext`, `rendering-with-drawingvisual`, `rendering-wpf-high-performance`, `virtualizing-wpf-ui`, `optimizing-wpf-memory`, `implementing-hit-testing` |
| **Input & Events** | `routing-wpf-events` |
| **Styling & Resources** | `managing-styles-resourcedictionary`, `using-xaml-property-element-syntax` |
| **Application** | `managing-wpf-application-lifecycle`, `threading-wpf-dispatcher` |
| **Build & Deployment** | `embedding-pdb-in-exe`, `publishing-wpf-apps` |
| **3rd Party Libraries** | `integrating-wpfui-fluent`, `integrating-livecharts2`, `validating-with-fluentvalidation`, `handling-errors-with-erroror`, `integrating-nodify`, `flaui-cross-process-input`, `scottplot-syncing-modifier-keys-for-mousewheel` |
| **Testing** | `testing-wpf-viewmodels`, `managing-unit-tests` |
| **Scaffolding** | `make-wpf-project`, `make-wpf-custom-control`, `make-wpf-usercontrol`, `make-wpf-converter`, `make-wpf-behavior`, `make-wpf-viewmodel`, `make-wpf-service` |
