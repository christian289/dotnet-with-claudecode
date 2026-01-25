# WPF Single-File Publishing

WPF requires additional flags for true single-file deployment.

---

## Required Configuration

```xml
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFramework>net10.0-windows</TargetFramework>
  <UseWPF>true</UseWPF>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <SelfContained>true</SelfContained>

  <!-- Single-File Settings -->
  <PublishSingleFile>true</PublishSingleFile>

  <!-- REQUIRED for WPF: Bundle native libraries -->
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>

  <!-- REQUIRED for .NET 5+: Include all content -->
  <IncludeAllContentForSelfExtract>true</IncludeAllContentForSelfExtract>
</PropertyGroup>
```

---

## Why These Flags?

Without these flags, native WPF libraries appear as separate files:
- `D3DCompiler_47_cor3.dll`
- `PresentationNative_cor3.dll`
- `wpfgfx_cor3.dll`

---

## Command Line

```bash
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:IncludeAllContentForSelfExtract=true
```

---

## WPF Limitations

| Option | WPF Support |
|--------|-------------|
| PublishSingleFile | ✅ (with flags) |
| PublishTrimmed | ❌ (reflection) |
| PublishAot | ❌ (not supported) |
| PublishReadyToRun | ✅ |

---

## Combine with PDB Embedding

```xml
<PropertyGroup>
  <PublishSingleFile>true</PublishSingleFile>
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  <IncludeAllContentForSelfExtract>true</IncludeAllContentForSelfExtract>

  <!-- Embed debug symbols -->
  <DebugType>embedded</DebugType>
</PropertyGroup>
```

See also: [embedding-pdb-in-exe](../embedding-pdb-in-exe/SKILL.md)
