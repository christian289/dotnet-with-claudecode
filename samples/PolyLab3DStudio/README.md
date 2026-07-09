# PolyLab 3D Studio (폴리랩 3D 스튜디오)

A beginner-friendly 3D learning studio sample built with WPF (`Viewport3D`),
implemented 1:1 from the Claude Design file `폴리랩 3D 스튜디오.dc.html`
(project `claude.ai/design/p/d395c492-f9fd-41a2-a608-c4e160048d7f`).
It follows this repository's WPF coding rules (CommunityToolkit.Mvvm +
GenericHost, UI-independent ViewModels, `net10.0-windows`).

## Screens & Features

- **시작 화면** — hero copy, "이어서 학습하기" (jumps to the next unfinished
  lesson), course/free-mode entry points, overall progress bar, and an
  auto-spinning snowman demo scene.
- **학습 코스** — 4 courses × 15 lessons (레슨/미션/퀴즈/이론), per-course
  progress, **quiz modal** (answer reveal, explanations, score) and
  **reading modal** (paged theory with code blocks teaching WPF 3D itself).
- **스튜디오** — dark app bar (undo/redo, XAML export), ribbon toolbar
  (Q/W/E/R tools · 6 solid shapes · 3 point clouds · duplicate/delete),
  scene dock, orbit-camera viewport (presets 앞/위/옆/입체, click select,
  move/rotate/scale drags, selection wireframe, grid + axes), properties
  dock (XYZ, rotation/scale, 10 material swatches, roughness/metalness,
  lighting), tutorial overlay with auto-checked tasks, and a status bar.
- **도구 개념 가이드** — the `도구 개념 가이드.dc.html` design merged in as
  an in-app screen (start-screen dark card ↔ guide header buttons navigate
  between the guide, the start screen, and free mode, as the design links
  intended): GIMP/Inkscape/Blender/point-cloud concepts with workflows,
  shortcut chips, and a comparison table.
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
