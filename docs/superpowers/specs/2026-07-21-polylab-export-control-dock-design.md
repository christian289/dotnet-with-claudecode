# PolyLab3DStudio — Export Popup Control Dock (design)

- **Date**: 2026-07-21
- **Status**: Approved (design), pending implementation plan
- **Scope**: `samples/PolyLab3DStudio` — the **export popup only**. The main
  studio screen (SCENE / viewport / properties docks) and the window size are
  NOT touched.

## Context / current state

Clicking the dark app-bar button "WPF XAML로 내보내기" (`OpenXamlCommand`) shows
the export surface. It is an in-view **overlay popup**: a dimmed full-screen
`Grid` (`StudioView.xaml` ~line 912, `Visibility` bound to `Export`) holding a
centered card with `DataContext = Export` (`XamlExportViewModel`).

The card is currently **2 columns**:

- **Left sidebar** (with a right border): the copy-paste 4-step guide + tip,
  AND the controls — ".NET version selector" (`net8/9/10-windows` segmented
  buttons), "프로젝트로 내보내기", exported-path / error status.
- **Right**: the code display — MainWindow.xaml / MainWindow.xaml.cs tabs
  (code-behind vs declarative variant), a read-only `CodeText` box, copy button.

`WpfProjectExporter.Export(baseDir, tfm, xaml, cs)` writes a `PolyLabScene/`
directory (`PolyLabScene.csproj`, `App.xaml(.cs)`, `MainWindow.xaml(.cs)`) and
returns the directory. There is **no solution file** and **no way to launch
Visual Studio**.

## Goals

1. Keep the export popup exactly as it is invoked (the "WPF XAML" button →
   overlay). Do NOT add export UI to the main studio screen; do NOT resize the
   OS window.
2. Restructure the popup card from **2 columns to 3**: `가이드 | 코드 | 속성 dock`.
3. Move the **.NET version selector + export actions** out of the current left
   sidebar into a new right-side **properties dock** column (styled like the
   studio's right properties dock). The left column keeps the guide only; the
   center column keeps the code display.
4. Add **"Visual Studio에서 열기"** in the dock: generate a `.slnx` solution file
   and open it with the default handler (Visual Studio if installed).

## Non-goals

- No change to the main studio 3-dock layout or the window size.
- No separate OS window; the popup stays an in-view overlay.
- No change to the generated scene XAML/C# beyond adding a solution file.
- No multi-editor launcher (VS Code / Rider); just the OS default handler for
  `.slnx`.

## Design

### 1. Popup card layout (`Views/StudioView.xaml`, the `Export` overlay)

- The centered card's inner grid gains a **3rd `ColumnDefinition`**. Final
  columns:
  - **Col 1 — 가이드**: the existing copy-paste 4-step guide + tip (the controls
    are removed from here).
  - **Col 2 — 코드**: the existing code area (tabs + `CodeText` + copy),
    unchanged.
  - **Col 3 — 속성 dock (NEW)**: the relocated controls, styled like the studio
    right dock (`HeaderBrush` background, panel-label headers, left border).
- The card widens (increase its `Width`/`MaxWidth`) to fit the 3rd column. This
  is card sizing inside the existing overlay — **no OS window resize**.

### 2. Properties dock content (Col 3)

Top-to-bottom:

1. Header label (e.g. "내보내기 · EXPORT").
2. **.NET version selector**: `net8/9/10-windows` segmented buttons
   (`PickTfmCommand`, `IsNet8/9/10` — unchanged).
3. `프로젝트로 내보내기` (`ExportProjectCommand`).
4. On success: `Visual Studio에서 열기` (new) + `폴더 열기`
   (`OpenExportedFolderCommand`), plus the exported-path readout.
5. `ExportError` message surface (reused for both export and VS-launch errors).

### 3. Core changes

- `WpfSceneCodeGenerator.GenerateSolutionFile()` → returns `.slnx` content:
  ```xml
  <Solution>
    <Project Path="PolyLabScene.csproj" />
  </Solution>
  ```
- `WpfProjectExporter.Export(...)` also writes `PolyLabScene.slnx` into the
  export directory. Return value stays the directory path (the `.slnx` path is
  `Path.Combine(dir, "PolyLabScene.slnx")`).

### 4. Open in Visual Studio (MVVM-respecting)

- `XamlExportViewModel`: add an `Action<string>` `openInVisualStudio` callback +
  `OpenInVisualStudioCommand`, enabled only when `ExportedPath` is set. The
  command calls `openInVisualStudio(Path.Combine(ExportedPath, "PolyLabScene.slnx"))`.
- `StudioViewModel`: add `event Action<string>? OpenInVisualStudioRequested`;
  pass a delegate into the `XamlExportViewModel` ctor (same pattern as
  `PickFolderRequested` / `OpenFolderRequested`).
- `StudioView.xaml.cs`: handle the request with
  `Process.Start(new ProcessStartInfo(slnxPath) { UseShellExecute = true })`.
  ViewModels stay UI-framework-free; process launch lives in the View layer,
  consistent with the existing folder-open callback.

### 5. Error handling

- Export failure (`IOException` / `UnauthorizedAccessException`): existing
  `ExportError` surface (now shown in the dock).
- VS launch failure (`Win32Exception` / `InvalidOperationException` — e.g. no
  handler for `.slnx`, VS not installed): caught in the View-layer handler,
  reported back to the VM so the dock **reuses the existing `ExportError`
  property** ("Visual Studio를 열 수 없어요 — 설치 여부를 확인하세요"). No new
  error property is introduced.

## File-by-file change list

| File | Change |
|------|--------|
| `Core/WpfSceneCodeGenerator.cs` | add `GenerateSolutionFile()` (.slnx) |
| `Core/WpfProjectExporter.cs` | write `PolyLabScene.slnx` alongside the csproj |
| `ViewModels/XamlExportViewModel.cs` | `openInVisualStudio` callback + `OpenInVisualStudioCommand` (enable after export) + VS-launch error via `ExportError` |
| `ViewModels/StudioViewModel.cs` | `OpenInVisualStudioRequested` event; wire the new callback into the `XamlExportViewModel` ctor |
| `Views/StudioView.xaml` | inside the `Export` overlay only: split the card into 3 columns (`가이드 | 코드 | 속성 dock`); move the version/export controls into the new dock; add the "Visual Studio에서 열기" button; widen the card |
| `Views/StudioView.xaml.cs` | handle `OpenInVisualStudioRequested` via `Process.Start` with error handling. No window resize. |

## Testing

This sample has no test project. Verification is **manual, via running the app
and screenshotting**:

1. Open the export popup → card is 3 columns (`가이드 | 코드 | 속성 dock`); the
   main studio screen and window size are unchanged.
2. In the dock: version selector, "프로젝트로 내보내기" work; the code column and
   its tabs/variant/copy still work.
3. Export writes the project **and** `PolyLabScene.slnx`.
4. "Visual Studio에서 열기" opens the `.slnx` (VS if installed); a graceful error
   (via `ExportError`) otherwise.

Optionally (deferred, only if requested): a small Core unit-test project could
assert `GenerateSolutionFile()` emits a `.slnx` referencing `PolyLabScene.csproj`
and that `WpfProjectExporter.Export` writes it — no UI dependency.
