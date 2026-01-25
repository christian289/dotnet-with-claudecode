---
name: embedding-pdb-in-exe
description: "Embeds PDB debugging symbols into EXE/DLL files. Use when configuring embedded debug symbols, single-file deployment, Source Link integration, or dotnet publish settings."
---

# PDB Embedded Debugging Symbols

Embed PDB files into EXE/DLL for stack traces with source locations without separate symbol files.

---

## Quick Start

```xml
<PropertyGroup>
  <DebugType>embedded</DebugType>
</PropertyGroup>
```

---

## Recommended Configuration

```xml
<PropertyGroup>
  <DebugType>embedded</DebugType>
  <DebugSymbols>true</DebugSymbols>
  <Deterministic>true</Deterministic>
  <PathMap>$(MSBuildProjectDirectory)=.</PathMap>
</PropertyGroup>
```

---

## Command Line

```bash
# Build
dotnet build -c Release -p:DebugType=embedded

# Publish (single-file)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=embedded
```

---

## DebugType Options

| Option | PDB Location | Use Case |
|--------|--------------|----------|
| `full` | Separate file | Development |
| `pdbonly` | Separate file | Release (default) |
| `portable` | Separate file | Cross-platform |
| `embedded` | Inside EXE | Distribution |
| `none` | None | Security critical |

---

## Additional Resources

- **Source Link Integration**: See [SOURCE-LINK.md](SOURCE-LINK.md)
- **Advanced Configuration**: See [ADVANCED.md](ADVANCED.md)
- [MSBuild DebugType Reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/code-generation#debugtype)
