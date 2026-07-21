# PolyLab3DStudio Export Popup Control Dock — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restructure the PolyLab3DStudio XAML-export popup into 3 columns (`가이드 | 코드 | 속성 dock`), moving the version/export controls into a dedicated right dock, and add "Open in Visual Studio" via a generated `.slnx`.

**Architecture:** Core gains a `.slnx` generator and writes it during export. `XamlExportViewModel` gains an `OpenInVisualStudioCommand` that calls a view-supplied `Func<string,string?>` (returns an error string or null). `StudioView` code-behind launches the `.slnx` via `Process.Start(UseShellExecute)`. Only the export overlay's inner layout changes — the main studio screen and OS window are untouched.

**Tech Stack:** .NET 10 WPF, CommunityToolkit.Mvvm (`[ObservableProperty]` / `[RelayCommand]` source generators), MVVM with view-layer callbacks for UI/process operations.

## Global Constraints

- ViewModels must not reference the UI framework (`System.Windows.*`); process launch and dialogs live in the View layer via callbacks (existing pattern: `PickFolderRequested` `Func`, `OpenFolderRequested` event).
- The generated exported scene XAML/C# is unchanged; only a solution file is added.
- No change to the main studio 3-dock layout or the OS window size; changes are confined to the `Export` overlay in `StudioView.xaml`.
- Spec: `docs/superpowers/specs/2026-07-21-polylab-export-popup-control-dock-design.md`.
- Paths are Windows; build with `dotnet build`. Solve absolute path root:
  `samples/PolyLab3DStudio/src/`.

---

### Task 1: Core — generate and write `PolyLabScene.slnx`

**Files:**
- Modify: `samples/PolyLab3DStudio/src/PolyLab3DStudio.Core/WpfSceneCodeGenerator.cs` (after `GenerateAppCs()`, ~line 49)
- Modify: `samples/PolyLab3DStudio/src/PolyLab3DStudio.Core/WpfProjectExporter.cs:22-28`

**Interfaces:**
- Produces: `WpfSceneCodeGenerator.GenerateSolutionFile() -> string` (the `.slnx` XML); `WpfProjectExporter.Export(...)` now also writes `PolyLabScene.slnx` into the returned directory.

- [ ] **Step 1: Add `GenerateSolutionFile()`**

In `WpfSceneCodeGenerator.cs`, immediately after the `GenerateAppCs()` method (before the `// ==================== code-behind variant ====================` comment), add:

```csharp
    /// <summary>
    /// Minimal .slnx solution referencing the exported csproj so the folder
    /// opens as a solution in Visual Studio (VS 2022 17.10+ / dotnet CLI).
    /// </summary>
    public static string GenerateSolutionFile() =>
        """
        <Solution>
          <Project Path="PolyLabScene.csproj" />
        </Solution>
        """;
```

- [ ] **Step 2: Write the `.slnx` during export**

In `WpfProjectExporter.cs`, add the `.slnx` write just before `return directory;` (after the `MainWindow.xaml.cs` write on line 27):

```csharp
        File.WriteAllText(Path.Combine(directory, "PolyLabScene.slnx"), WpfSceneCodeGenerator.GenerateSolutionFile());
        return directory;
```

- [ ] **Step 3: Build the Core project**

Run: `dotnet build samples/PolyLab3DStudio/src/PolyLab3DStudio.Core/PolyLab3DStudio.Core.csproj -c Debug`
Expected: `오류 0개` / `0 Error(s)`.

- [ ] **Step 4: Commit**

```bash
git add samples/PolyLab3DStudio/src/PolyLab3DStudio.Core/WpfSceneCodeGenerator.cs samples/PolyLab3DStudio/src/PolyLab3DStudio.Core/WpfProjectExporter.cs
git commit -m "feat(polylab): generate PolyLabScene.slnx on project export"
```

---

### Task 2: ViewModels — Open-in-VS command + StudioViewModel wiring

**Files:**
- Modify: `samples/PolyLab3DStudio/src/PolyLab3DStudio.ViewModels/XamlExportViewModel.cs` (ctor 19-37; add field, command)
- Modify: `samples/PolyLab3DStudio/src/PolyLab3DStudio.ViewModels/StudioViewModel.cs` (add property ~line 72; OpenXaml call 246-254)

**Interfaces:**
- Consumes: `WpfProjectExporter.Export` writing `PolyLabScene.slnx` (Task 1).
- Produces: `XamlExportViewModel` ctor takes a new 6th argument `Func<string, string?> openInVisualStudio` (inserted after `openFolder`); `XamlExportViewModel.OpenInVisualStudioCommand`; `StudioViewModel.OpenInVisualStudioRequested` settable `Func<string, string?>?` property (view sets it; returns an error message or `null`).

- [ ] **Step 1: Add the callback field + ctor parameter in `XamlExportViewModel`**

In `XamlExportViewModel.cs`, add the field next to the other callbacks (after `_openFolder` on line 15):

```csharp
    private readonly Func<string, string?> _openInVisualStudio;
```

Then add the ctor parameter after `Action<string> openFolder,` (line 24) and assign it. The ctor parameter list becomes:

```csharp
    public XamlExportViewModel(
        int objectCount,
        Func<IReadOnlyList<SceneObjectState>> snapshot,
        Func<(double Intensity, double Angle)> light,
        Action<string> copyToClipboard,
        Func<string?> pickFolder,
        Action<string> openFolder,
        Func<string, string?> openInVisualStudio,
        Action<TaskEvent> notify,
        Action close)
```

And add the assignment inside the ctor body (after `_openFolder = openFolder;` on line 34):

```csharp
        _openInVisualStudio = openInVisualStudio;
```

- [ ] **Step 2: Add the `OpenInVisualStudioCommand`**

In `XamlExportViewModel.cs`, add this command right after `OpenExportedFolderCommand`'s method (`OpenExportedFolder`, ends ~line 180):

```csharp
    [RelayCommand]
    private void OpenInVisualStudio()
    {
        if (ExportedPath is not { Length: > 0 } directory)
        {
            return;
        }

        string solutionPath = Path.Combine(directory, "PolyLabScene.slnx");
        ExportError = _openInVisualStudio(solutionPath); // null on success
    }
```

- [ ] **Step 3: Add the settable `Func` property in `StudioViewModel`**

In `StudioViewModel.cs`, right after the `PickFolderRequested` property (line 72), add:

```csharp
    /// <summary>Set by the view: opens the exported .slnx in Visual Studio; returns an error message or null on success.</summary>
    public Func<string, string?>? OpenInVisualStudioRequested { get; set; }
```

- [ ] **Step 4: Pass the callback into the export VM**

In `StudioViewModel.cs`, in `OpenXaml()` (246-254), insert the new argument after the `openFolder` lambda (line 252). The call becomes:

```csharp
        Export = new XamlExportViewModel(
            Objects.Count,
            () => [.. Objects.Select(o => o.ToState())],
            () => (LightIntensity, LightAngle),
            text => CopyToClipboardRequested?.Invoke(text),
            () => PickFolderRequested?.Invoke(),
            path => OpenFolderRequested?.Invoke(path),
            slnx => OpenInVisualStudioRequested?.Invoke(slnx),
            CheckTask,
            () => Export = null);
```

- [ ] **Step 5: Build the ViewModels project**

Run: `dotnet build samples/PolyLab3DStudio/src/PolyLab3DStudio.ViewModels/PolyLab3DStudio.ViewModels.csproj -c Debug`
Expected: `오류 0개` / `0 Error(s)`.

- [ ] **Step 6: Commit**

```bash
git add samples/PolyLab3DStudio/src/PolyLab3DStudio.ViewModels/XamlExportViewModel.cs samples/PolyLab3DStudio/src/PolyLab3DStudio.ViewModels/StudioViewModel.cs
git commit -m "feat(polylab): add OpenInVisualStudio command wired through StudioViewModel"
```

---

### Task 3: View code-behind — launch the `.slnx`

**Files:**
- Modify: `samples/PolyLab3DStudio/src/PolyLab3DStudio.WpfApp/Views/StudioView.xaml.cs`

**Interfaces:**
- Consumes: `StudioViewModel.OpenInVisualStudioRequested` (Task 2) — assigns the handler; the handler returns `string?` (error or null).

- [ ] **Step 1: Add `System.ComponentModel` for `Win32Exception`**

At the top of `StudioView.xaml.cs`, add after `using System.Diagnostics;` (line 1):

```csharp
using System.ComponentModel;
```

- [ ] **Step 2: Subscribe/unsubscribe the callback in `OnDataContextChanged`**

In the unsubscribe block (inside `if (_vm is not null)`, lines 25-31) add:

```csharp
            _vm.OpenInVisualStudioRequested = null;
```

In the subscribe block (inside the second `if (_vm is not null)`, lines 35-41) add:

```csharp
            _vm.OpenInVisualStudioRequested = OnOpenInVisualStudioRequested;
```

- [ ] **Step 3: Add the launch handler**

At the end of the class (after `OnOpenFolderRequested`, line 57), add:

```csharp
    private static string? OnOpenInVisualStudioRequested(string solutionPath)
    {
        try
        {
            Process.Start(new ProcessStartInfo(solutionPath) { UseShellExecute = true });
            return null;
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or FileNotFoundException)
        {
            return $"Visual Studio를 열 수 없어요: {ex.Message}";
        }
    }
```

- [ ] **Step 4: Build the WpfApp project**

Run: `dotnet build samples/PolyLab3DStudio/src/PolyLab3DStudio.WpfApp/PolyLab3DStudio.WpfApp.csproj -c Debug`
Expected: `오류 0개` / `0 Error(s)`. (The UI button is added in Task 4; the handler compiles now.)

- [ ] **Step 5: Commit**

```bash
git add samples/PolyLab3DStudio/src/PolyLab3DStudio.WpfApp/Views/StudioView.xaml.cs
git commit -m "feat(polylab): launch exported .slnx via Process.Start in StudioView"
```

---

### Task 4: XAML — 3-column popup (`가이드 | 코드 | 속성 dock`)

**Files:**
- Modify: `samples/PolyLab3DStudio/src/PolyLab3DStudio.WpfApp/Views/StudioView.xaml` (the `Export` overlay only: card 919; body grid 949-953; left sidebar 954-1132)

**Interfaces:**
- Consumes: `PickTfmCommand`/`IsNet8/9/10`, `ExportProjectCommand`, `OpenExportedFolderCommand`, `ExportedPath`, `ExportError` (existing), and `OpenInVisualStudioCommand` (Task 2).

- [ ] **Step 1: Widen the popup card**

On line 919, change the card width from `880` to `1180`:

```xml
            <Border Grid.Row="1" Width="1180" Background="{StaticResource SurfaceBrush}" CornerRadius="16"
```

- [ ] **Step 2: Make the body a 3-column grid**

Replace the body grid column definitions (lines 950-953) with three columns:

```xml
                    <Grid Grid.Row="1">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="262" />   <!-- 가이드 -->
                            <ColumnDefinition Width="*" />     <!-- 코드 -->
                            <ColumnDefinition Width="320" />   <!-- 속성 dock -->
                        </Grid.ColumnDefinitions>
```

(The left-sidebar `Border` remains `Grid.Column` unset = column 0; the existing code container remains `Grid.Column="1"` — unchanged.)

- [ ] **Step 3: Remove the controls block from the left sidebar (col 0)**

In the left-sidebar `StackPanel` (starts line 957), CUT the "Runnable project export" block — from the `<Rectangle Style="{StaticResource CardDivider}" />` (line 1035) through the `ExportError` `TextBlock` that ends at line 1125 (the block beginning `<!-- Runnable project export -->` on line 1034 and everything up to and including the `ExportError` TextBlock). The 4-step guide + TIP box above it stays. Keep this cut text on the clipboard for Step 4.

Left sidebar now ends after the TIP `Border` (the guide only).

- [ ] **Step 4: Add the 속성 dock column (col 2) and paste the controls into it**

After the code container's closing tag (the sibling that is `Grid.Column="1"`, i.e., just before the body grid's closing `</Grid>` on line ~1230), add a new dock `Border` at `Grid.Column="2"`, and paste the block cut in Step 3 inside its `StackPanel` (drop the leading `<Rectangle CardDivider/>` since it is now the first item):

```xml
                        <Border Grid.Column="2" Background="{StaticResource HeaderBrush}"
                                BorderBrush="{StaticResource BorderSoftBrush}" BorderThickness="1,0,0,0">
                            <ScrollViewer VerticalScrollBarVisibility="Auto" Padding="18,16">
                                <StackPanel>
                                    <TextBlock Text="내보내기 · EXPORT" Style="{StaticResource PanelLabelText}" />
                                    <!-- PASTE the cut block here (version buttons + 프로젝트로 내보내기 +
                                         ExportedPath readout + ExportError), minus its leading CardDivider. -->
                                </StackPanel>
                            </ScrollViewer>
                        </Border>
```

- [ ] **Step 5: Add the "Visual Studio에서 열기" button**

Inside the pasted block, immediately AFTER the "프로젝트로 내보내기" (`ExportProjectCommand`) button and BEFORE the "폴더 열기" button, insert:

```xml
                                    <Button Command="{Binding OpenInVisualStudioCommand}" Margin="0,8,0,0"
                                            Visibility="{Binding ExportedPath, Converter={StaticResource NotNullToVisibility}}"
                                            Cursor="Hand" Focusable="False" AutomationProperties.Name="Visual Studio에서 열기">
                                        <Button.Template>
                                            <ControlTemplate TargetType="Button">
                                                <Border Background="{StaticResource InkBrush}" CornerRadius="8" Padding="0,9">
                                                    <TextBlock Text="Visual Studio에서 열기" FontSize="12" FontWeight="SemiBold"
                                                               Foreground="{StaticResource SurfaceBrush}" HorizontalAlignment="Center" />
                                                </Border>
                                            </ControlTemplate>
                                        </Button.Template>
                                    </Button>
```

- [ ] **Step 6: Build the WpfApp project**

Run: `dotnet build samples/PolyLab3DStudio/src/PolyLab3DStudio.WpfApp/PolyLab3DStudio.WpfApp.csproj -c Debug`
Expected: `오류 0개` / `0 Error(s)`.

- [ ] **Step 7: Run the app and verify the popup (screenshot)**

Launch the built exe, enter free studio mode, add a shape, click the app-bar "WPF XAML로 내보내기" button, and screenshot. Verify:
- The popup card is 3 columns: `가이드 (좌) | 코드 (중앙) | 속성 dock (우)`.
- The main studio screen and window size are unchanged.
- The dock holds the .NET version buttons + "프로젝트로 내보내기"; the code column still shows tabs/code/copy.
- After picking a folder and exporting, "Visual Studio에서 열기" and "폴더 열기" appear; the exported folder contains `PolyLabScene.slnx` next to `PolyLabScene.csproj`.

Launch:
```powershell
Start-Process "samples/PolyLab3DStudio/src/PolyLab3DStudio.WpfApp/bin/Debug/net10.0-windows/PolyLab3DStudio.WpfApp.exe"
```

- [ ] **Step 8: Commit**

```bash
git add samples/PolyLab3DStudio/src/PolyLab3DStudio.WpfApp/Views/StudioView.xaml
git commit -m "feat(polylab): 3-column export popup with dedicated control dock"
```

---

## Notes for the implementer

- The exact line numbers above are current-state anchors; re-read the file around them before editing (the file is edited across steps).
- `Path` and `IOException` are available via implicit usings (`System.IO`); no extra `using` needed in the ViewModels.
- `.slnx` is the XML solution format; opening it relies on Visual Studio 2022 17.10+ (or a `.slnx` file association). If no handler exists, `Process.Start` throws `Win32Exception`, which the handler turns into the `ExportError` message — this is expected on machines without VS.
