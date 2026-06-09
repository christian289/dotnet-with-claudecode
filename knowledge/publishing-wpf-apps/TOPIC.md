# WPF Application Publishing Guide

> Guides WPF application publishing and installer options, plus Windows App SDK (WASDK) self-contained + MSIX packaging gotchas. Use when user mentions publish, deploy, release, packaging, or installer to choose a deployment method and installer technology; also when a WASDK/MSIX WPF app hits resource Content globbing breakage (pack:// BitmapImage XamlParseException / DirectoryNotFoundException, MSB3030 on a referencing project), multi-exe MSIX packaging ambiguity, or a packaged app failing native DLL load (DllNotFoundException 0x8007007E because MSIX does not search PATH).

When publishing WPF applications, ask the user about deployment and installer preferences.

---

## Ask User: Deployment Method

> **Which deployment method do you need?**
> 1. **Framework-Dependent** - Small size (~1MB), requires .NET runtime
> 2. **Self-Contained** - Includes runtime (150-200MB), no dependencies
> 3. **Single-File** - One executable (50-80MB compressed)

---

## Ask User: Installer Technology

> **Which installer/update technology do you prefer?**
> 1. **Velopack** (Recommended) - Modern, fast updates, delta updates
> 2. **MSIX** - Windows Store, enterprise deployment
> 3. **NSIS** - Traditional installer, full control
> 4. **Inno Setup** - Simple, widely used
> 5. **None** - Portable/xcopy deployment

---

## Quick Reference

### Deployment Methods

| Method | Size | Startup | Requirements |
|--------|------|---------|--------------|
| Framework-Dependent | ~1MB | Fast | .NET runtime |
| Self-Contained | 150-200MB | Fast | None |
| Single-File | 150-200MB | Medium | None |
| Single-File + Compressed | 50-80MB | Slower | None |

### Installer Technologies

| Technology | Auto-Update | Delta Updates | Store | Complexity |
|------------|-------------|---------------|-------|------------|
| Velopack | ✅ | ✅ | ❌ | Low |
| MSIX | ✅ | ✅ | ✅ | Medium |
| NSIS | Manual | ❌ | ❌ | High |
| Inno Setup | Manual | ❌ | ❌ | Medium |

---

## WPF Limitations

⚠️ **PublishTrimmed**: Not supported (reflection-heavy)
⚠️ **PublishAot**: Not supported (WPF incompatible)

---

## Basic Commands

```bash
# Framework-Dependent
dotnet publish -c Release

# Self-Contained
dotnet publish -c Release -r win-x64 --self-contained true

# Single-File (WPF)
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true
```

---

## Windows App SDK (self-contained) + MSIX gotchas

Adding `Microsoft.WindowsAppSDK` (especially self-contained) to a WPF app, and packaging it as MSIX, introduces several non-obvious build/runtime failures. Programmatic restart is the usual motivation for adopting self-contained WASDK — for that, see `managing-wpf-application-lifecycle` (Programmatic Restart: `AppInstance.Restart`).

### Default Content globbing breaks `pack://` image/icon resources

**Symptoms:**
- A `BitmapImage` with `UriSource="pack://application:,,,/Resources/Images/<image>.png"` throws `System.Windows.Markup.XamlParseException` during `InitializeComponent()` (inner `System.IO.DirectoryNotFoundException`; stack includes `MS.Internal.AppModel.ContentFilePart.GetStreamCore`).
- A project that `ProjectReference`s the WPF app fails to build with `MSB3030` ("could not copy `...\Resources\<icon>.ico` — file not found"). An IDE-only build may hide it (different paths); it surfaces in a CLI/solution build.

**Cause:** Windows App SDK enables default Content globbing (`EnableDefaultContentItems`), so `*.png` / `*.ico` you intended only as `<Resource>` (embedded) or `<ApplicationIcon>` get **additionally** classified as `Content`. As `Content`, WPF emits an `AssemblyAssociatedContentFileAttribute` and resolves `pack://application:,,,` to a **loose file in the output folder** instead of an embedded resource — and if it is not copied to output, it is not found. That same `Content` also becomes a transitive copy target for referencing projects, which then fails because the source is not in output. Reproduces in both packaged and unpackaged runs.

**Fix — exclude the resources from default Content globbing and keep the intended item type:**

```xml
<ItemGroup>
  <!-- Remove from default Content globbing so they are not treated as loose files. -->
  <Content Remove="Resources\Images\**\*.png" />
  <Content Remove="Resources\**\*.ico" />

  <!-- Images / splash: embed as Resource so pack:// resolves to the embedded stream. -->
  <Resource Include="Resources\Images\**\*.png" />
</ItemGroup>

<PropertyGroup>
  <!-- App icon: embed into the exe; do not ship it as Content. -->
  <ApplicationIcon>Resources\app.ico</ApplicationIcon>
</PropertyGroup>
```

The app-icon `.ico` Content entry is usually vestigial (MSIX logo assets are separate PNGs, so the ICO need not be Content). After adding WASDK, watch for the same globbing trap each time you add a new image/icon.

### Self-contained WASDK + custom-manifest MSIX: multi-exe ambiguity

**Symptom:** A custom-manifest CLI MSIX pipeline (one that regenerates/overwrites the manifest each build) fails because the input folder contains **multiple `.exe` files**, so the manifest's `$targetnametoken$.exe` placeholder cannot be resolved to a single exe.

**Cause:** Self-contained Windows App SDK bundles the runtime into the **`dotnet build` output** (not publish-only) and also drops auxiliary executables (e.g. a restart helper) there. The extra exe makes a placeholder-based tool unable to pick the main app exe.

**Fixes / notes:**
- Self-contained bundles the runtime into the **build output**, so a "copy the whole output folder" packaging step is compatible (no separate publish needed).
- Resolve the multi-exe ambiguity with the packaging tool's explicit main-exe option (e.g. `--executable <App>.exe`).
- Framework-dependent alternative: declare a framework `PackageDependency` in the manifest and pre-install the runtime on the target machine. In a pipeline that overwrites the manifest every build, self-contained has less friction (no dependency injection into the manifest).

### Packaged apps do not search PATH for native DLLs

**Symptom:** A native DLL (P/Invoke target) that loads fine in development fails once the same code runs from an installed MSIX package: `System.DllNotFoundException: Unable to load DLL '<native>.dll' or one of its dependencies (0x8007007E)`. The native SDK is installed system-wide (Program Files) and exposed **only on PATH**, with its dependent DLLs spread across several PATH directories.

**Cause:** Packaged (MSIX) apps do **not** use the `PATH` environment variable for native DLL resolution (the loader searches a restricted set — app directory, System32, etc.). Unpackaged apps do search PATH, so the same code loads in development.

**Fix — register the PATH directories with the loader at startup, gated to packaged runs:**

```csharp
using System.Runtime.InteropServices;

internal static partial class NativeDllSearch
{
    private const uint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetDefaultDllDirectories(uint directoryFlags);

    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr AddDllDirectory(string newDirectory);

    /// <summary>
    /// Adds every existing PATH directory to the native DLL search path.
    /// Call once at startup, only when running packaged
    /// (unpackaged apps already search PATH).
    /// </summary>
    public static void AddPathDirectories()
    {
        SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);

        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var dir in path.Split(
            Path.PathSeparator,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            // Dependencies may be spread across several PATH dirs — add them all.
            if (Directory.Exists(dir))
            {
                AddDllDirectory(dir);
            }
        }
    }
}
```

- Add **every** existing PATH directory — adding only one is not enough when dependencies are scattered.
- Gate to packaged runs (check package identity, e.g. `GetCurrentPackageFullName` returns `APPMODEL_ERROR_NO_PACKAGE` when unpackaged); unpackaged loaders search PATH already.
- For a child console process that P/Invokes, have the packaged parent pass a flag (e.g. `--use-path-dll-search`) so the child applies the same fix.

---

## Additional Resources

- **WPF Single-File**: See [WPF-SINGLE-FILE.md](WPF-SINGLE-FILE.md)
- **Size Optimization**: See [SIZE-OPTIMIZATION.md](SIZE-OPTIMIZATION.md)
- **Installer Options**: See [INSTALLERS.md](INSTALLERS.md)
