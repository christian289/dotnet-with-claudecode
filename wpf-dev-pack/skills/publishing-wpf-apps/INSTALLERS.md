# WPF Installer Technologies

---

## Velopack (Recommended)

Modern deployment framework with automatic updates.

```bash
# Install CLI
dotnet tool install -g vpk

# Pack application
vpk pack -u MyApp -v 1.0.0 -p publish -e MyApp.exe

# Create delta updates automatically
vpk pack -u MyApp -v 1.0.1 -p publish -e MyApp.exe --delta
```

**Features:**
- Fast delta updates (only changed files)
- No admin rights required
- Cross-platform (Windows, macOS, Linux)
- Simple integration

**NuGet:**
```xml
<PackageReference Include="Velopack" Version="0.*" />
```

**Resources:** [velopack.io](https://velopack.io)

---

## MSIX

Windows native packaging format.

```xml
<!-- Add Windows Application Packaging Project -->
<PropertyGroup>
  <WindowsPackageType>MSIX</WindowsPackageType>
</PropertyGroup>
```

**Features:**
- Windows Store distribution
- Enterprise deployment (Intune, SCCM)
- Clean install/uninstall
- Auto-updates via Store or sideload

**Best for:** Enterprise, Store distribution

**Resources:** [MSIX Documentation](https://learn.microsoft.com/en-us/windows/msix/)

---

## NSIS

Traditional installer with full control.

```nsis
!include "MUI2.nsh"

Name "MyApp"
OutFile "MyAppSetup.exe"
InstallDir "$PROGRAMFILES\MyApp"

Section "Install"
  SetOutPath $INSTDIR
  File /r "publish\*.*"
  CreateShortcut "$DESKTOP\MyApp.lnk" "$INSTDIR\MyApp.exe"
SectionEnd
```

**Features:**
- Full customization
- Small installer size
- No dependencies
- Widely supported

**Best for:** Traditional enterprise deployment

**Resources:** [nsis.sourceforge.io](https://nsis.sourceforge.io)

---

## Inno Setup

Simple and widely used installer creator.

```iss
[Setup]
AppName=MyApp
AppVersion=1.0.0
DefaultDirName={autopf}\MyApp
OutputBaseFilename=MyAppSetup

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{autoprograms}\MyApp"; Filename: "{app}\MyApp.exe"
```

**Features:**
- Easy script syntax
- Good documentation
- Widely used
- Free and open source

**Best for:** Simple installers

**Resources:** [jrsoftware.org/isinfo.php](https://jrsoftware.org/isinfo.php)

---

## Comparison

| Feature | Velopack | MSIX | NSIS | Inno Setup |
|---------|----------|------|------|------------|
| Auto-Update | ✅ Built-in | ✅ Store/Sideload | ❌ Manual | ❌ Manual |
| Delta Updates | ✅ | ✅ | ❌ | ❌ |
| Admin Rights | Optional | Optional | Required | Required |
| Windows Store | ❌ | ✅ | ❌ | ❌ |
| Learning Curve | Low | Medium | High | Medium |
| Customization | Medium | Low | High | Medium |

---

## Recommendation

| Scenario | Recommended |
|----------|-------------|
| New project with auto-update | **Velopack** |
| Windows Store / Enterprise | **MSIX** |
| Traditional enterprise | **NSIS** or **Inno Setup** |
| Portable app (no installer) | xcopy / zip |
