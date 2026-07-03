# PolyLab 3D Studio (폴리랩 3D 스튜디오)

A low-poly 3D scene editor sample built with WPF (`Viewport3D`), following
this repository's WPF coding rules (CommunityToolkit.Mvvm + GenericHost,
UI-independent ViewModels, `net10.0-windows`).

## Features

- **Primitive palette** — add cube / sphere (icosphere) / cylinder / cone /
  torus / plane, generated as flat-shaded low-poly meshes by
  `PrimitiveMeshFactory` (UI-framework-independent, `System.Numerics`).
- **Scene object list** — select, duplicate, remove, clear.
- **Properties panel** — position XYZ, rotation XYZ, uniform scale, and a
  six-color studio palette applied per object.
- **Viewport** — orbit camera (left-drag), zoom (wheel), click-to-select via
  3D hit testing, selection glow (emissive tint).

## Project Structure

| Project | Type | Purpose |
|---------|------|---------|
| `PolyLab3DStudio.Core` | .NET Class Library | Mesh data + primitive generators (no UI deps) |
| `PolyLab3DStudio.ViewModels` | .NET Class Library | MVVM ViewModels (CommunityToolkit.Mvvm only) |
| `PolyLab3DStudio.WpfApp` | WPF Application | Entry point, XAML UI, `SceneRenderer` service |

## Run

```
dotnet run --project src/PolyLab3DStudio.WpfApp
```

## Design Source Note

This sample was requested as an implementation of the Claude Design file
`폴리랩 3D 스튜디오.dc.html`
(`claude.ai/design/p/d395c492-f9fd-41a2-a608-c4e160048d7f`). At build time
that project was **not accessible** from the authenticated account (the
design API returned `project not found`), so the UI here is a best-effort
interpretation of the design's title and the Claude Design visual language
(dark studio layout, coral accent). Once the design project becomes
accessible, reconcile this UI against the actual `.dc.html`.
