---
name: publishing-wpf-apps
description: "Guides WPF application publishing and installer options. Use when user mentions publish, deploy, release, packaging, or installer to help choose deployment method and installer technology."
---

# WPF Application Publishing Guide

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

## Additional Resources

- **WPF Single-File**: See [WPF-SINGLE-FILE.md](WPF-SINGLE-FILE.md)
- **Size Optimization**: See [SIZE-OPTIMIZATION.md](SIZE-OPTIMIZATION.md)
- **Installer Options**: See [INSTALLERS.md](INSTALLERS.md)
