# PolyLab 3D Studio (폴리랩 3D 스튜디오)

A beginner-friendly 3D learning studio sample built with WPF (`Viewport3D`),
implemented 1:1 from the Claude Design file `폴리랩 3D 스튜디오.dc.html`
(project `claude.ai/design/p/d395c492-f9fd-41a2-a608-c4e160048d7f`).
It follows this repository's WPF coding rules (CommunityToolkit.Mvvm +
GenericHost, UI-independent ViewModels, `net10.0-windows`).

## Screens & Features

- **시작 화면** — hero copy, "이어서 학습하기" (jumps to the next unfinished
  lesson), course/free-mode entry points, a **참조 자료 · REFERENCE hub**
  (2×2 dark cards linking the four reference screens: 도구 개념 가이드 /
  3D 사전 / WPF 코어 파이프라인 / .NET 3D 도구 지도), overall progress bar,
  and an auto-spinning snowman demo scene.
- **학습 코스** — 6 courses × 30 units: courses 01–04 (레슨/미션/퀴즈/이론)
  plus two advanced tracks — **트랙 A · 순수 WPF 3D 심화** (9 readings +
  comprehensive quiz: hand-built meshes, material layers, all four lights,
  transform order/gimbal lock, scene graph, projection, animation, picking,
  the honest ceiling) and **트랙 B · Windows용 .NET 3D 도구** (4 readings +
  quiz: HelixToolkit.Wpf, Helix.SharpDX/DX11, the DX12 bridge, low-level &
  engines). Per-course progress, **quiz modal** (answer reveal, explanations,
  score) and **reading modal** (paged theory with code blocks teaching WPF 3D
  itself).
- **스튜디오** — dark app bar (undo/redo, XAML export), ribbon toolbar
  (Q/W/E/R tools · 6 solid shapes · 3 point clouds · duplicate/delete),
  scene dock, orbit-camera viewport (presets 앞/위/옆/입체, click select,
  move/rotate/scale drags, selection wireframe, grid + axes), properties
  dock (XYZ, rotation/scale, 10 material swatches, roughness/metalness,
  lighting), tutorial overlay with auto-checked tasks, and a status bar.
- **도구 개념 가이드** — the `도구 개념 가이드.dc.html` design merged in as
  an in-app screen (reference-hub card ↔ guide header buttons navigate
  between the guide, the start screen, and free mode, as the design links
  intended): GIMP/Inkscape/Blender/point-cloud concepts with workflows,
  shortcut chips, and a comparison table.
- **WPF 렌더링 파이프라인** — the `WPF 렌더링 파이프라인.dc.html` design
  merged in as an in-app screen (reachable from the reference hub, back
  button returns to start): an interactive 6-stage diagram of how XAML
  becomes pixels — 3 clickable stages on the UI-thread lane, 3 on the
  render-thread lane, a DUCE-channel boundary chip, a detail panel for the
  selected stage, a "where does 3D fit" card, and a dark DX12 entry-path
  card (D3DImage bridge vs HwndHost airspace). Glossary terms carry the
  design's term-tips as dotted-underlined runs with hover tooltips; copy is
  verbatim (트랙 A-3 / 트랙 B references resolve to the in-app tracks), and
  the CTA navigates to the courses screen.
- **3D 사전** — the `3D 사전.dc.html` design merged in as an in-app screen:
  the shared 60-term glossary (`GlossaryCatalog`, from the design's
  `glossary.js`) with live substring search across word/english/short/detail,
  11 category filter chips (selected chip inverts to dark), a live "N개 용어"
  count, a two-column card grid, and the design's empty-state message.
  The 12 WPF-specific terms (Viewport3D, MeshGeometry3D, …, milcore,
  DUCE 채널, Freeze()) additionally carry a verified "Microsoft Learn 문서 ↗"
  link that opens the official docs in the browser.
- **용어 툴팁 (term-tips)** — the design's `term-tip.js` ported as a reusable
  `TermTip` inline control: dotted accent underline, the design's dark tooltip
  card (word + English + short description + "3D 사전에서 자세히 →"), and
  clicking the term deep-links into the 3D 사전 pre-searched with that word.
  Used across the pipeline, tool-map, and concept-guide screens (the guide
  gets its 11 design term-tips via a `[[word]]` marker extension of the
  `**bold**` text renderer).
- **.NET 3D 도구 지도** — the `닷넷 3D 도구 지도.dc.html` design merged in
  as an in-app screen: the 5-step learning ladder (순정 Media3D →
  HelixToolkit.Wpf → Helix.SharpDX → Vortice/Silk.NET → Stride, with the
  "DX11 천장 돌파" marker), an 8-row × 5-tool comparison table, the "판단"
  callout, term-tips, and a dark CTA ("트랙 B 시작하기 →" → courses screen).
- **XAML 내보내기** — generates a complete, standalone
  `MainWindow.xaml` / `MainWindow.xaml.cs` for the current scene
  (mesh + point cloud factories, orbit camera included) with a 4-step
  copy-paste guide, in **two flavors** switchable in the modal:
  - **코드 비하인드형** — the scene is built in C# (`BuildScene()` with
    one `AddObject(...)` call per object), as in the original design.
  - **XAML 중심형** — the scene structure, per-object materials,
    transforms, and lights are declared in XAML; procedural meshes are
    supplied by a `{local:SceneMesh ...}` markup extension, and C# keeps
    only the orbit camera and mesh factories.
  - **프로젝트로 바로 내보내기** — beyond copy-paste, the modal can write a
    complete, immediately runnable `PolyLabScene` WPF project (csproj +
    App.xaml/.cs + the current flavor's MainWindow pair) into a chosen
    folder, with a .NET target picker (.NET 8 / 9 / 10) — `dotnet run`
    or F5 works out of the box.
- **포인트 클라우드 옵션** — cloud objects get two extra properties-panel
  controls (both carried into the exported code of both flavors):
  - **높이 컬러맵** — colors each point by its height (Y) via
    TextureCoordinates + a gradient brush (blue → teal → green → yellow →
    orange, built from the studio palette).
  - **점 개수** — 100 to 100,000 points per cloud (terrain snaps to the
    nearest (n+1)² grid). Rebuilds are debounced while the slider drags;
    at 100k points frame rate and click hit-testing get noticeably heavier
    (expected for ~800k triangles in `Viewport3D`).
- **설정** — English label toggle, grid toggle, wheel invert, mouse
  sensitivity, progress reset. Settings and lesson progress persist to
  `%APPDATA%/PolyLab3DStudio/*.json`.
- Keyboard: `Q/W/E/R` tools, `Delete` remove, `Esc` deselect,
  `Ctrl+Z` / `Ctrl+Shift+Z` undo/redo (40-snapshot history).

## Project Structure

| Project | Type | Purpose |
|---------|------|---------|
| `PolyLab3DStudio.Core` | .NET Class Library | Mesh/point-cloud factories, shape & course catalogs, task checker, WPF code generator, JSON stores (no UI deps) |
| `PolyLab3DStudio.ViewModels` | .NET Class Library | Shell/Start/Courses/Studio/Settings + quiz/reading/tutorial/export VMs (CommunityToolkit.Mvvm only) |
| `PolyLab3DStudio.WpfApp` | WPF Application | Views (incl. the merged concept guide screen), `PolyViewport` (Viewport3D orbit camera, picking, tool drags), resources, GenericHost DI |

## Run

```
dotnet run --project src/PolyLab3DStudio.WpfApp
```

## Design Fidelity Notes

- Colors, typography scale, copy, layouts, seeds (demo scene, lesson
  seeds, point cloud seeds 7/13/42), camera math (goal interpolation
  ×0.22, orbit 0.007, zoom ×1±0.09, clamps), and the exported code are
  transcribed from the design and its `Viewport.jsx`.
- WPF `Viewport3D` has no shadow mapping, so the ground shadows from the
  Three.js original are omitted; lighting follows the exported WPF code
  (DirectionalLight + AmbientLight) instead.
- Buttons carry `AutomationProperties.Name` for UI Automation.
- Both export flavors were verified end-to-end: the generated code was
  dropped into fresh `net10.0-windows` WPF projects, built with zero
  errors, and rendered pixel-identical scenes.
