# Advanced PDB Configuration

---

## Single-File Deployment

```xml
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFramework>net10.0-windows</TargetFramework>
  <UseWPF>true</UseWPF>

  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>

  <DebugType>embedded</DebugType>
  <DebugSymbols>true</DebugSymbols>
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
</PropertyGroup>
```

---

## Conditional Configuration

### Release Only

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <DebugType>embedded</DebugType>
</PropertyGroup>

<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <DebugType>full</DebugType>
</PropertyGroup>
```

### CI/CD Only

```xml
<PropertyGroup Condition="'$(CI)' == 'true'">
  <DebugType>embedded</DebugType>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
</PropertyGroup>
```

---

## Security

⚠️ Embedded PDB contains source code path information.

### Sanitize Paths

```xml
<PropertyGroup>
  <DebugType>embedded</DebugType>
  <PathMap>$(MSBuildProjectDirectory)=.</PathMap>
  <ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>
</PropertyGroup>
```

### Strip Symbols

```xml
<PropertyGroup Condition="'$(StripSymbols)' == 'true'">
  <DebugType>none</DebugType>
  <DebugSymbols>false</DebugSymbols>
</PropertyGroup>
```

---

## Verification

### Check Embedded PDB

```bash
dotnet tool install -g dotnet-symbol
dotnet-symbol --symbols MyApp.exe

# Or with dumpbin (Visual Studio)
dumpbin /headers MyApp.exe | findstr "Debug"
```

### Test Stack Trace

```csharp
try
{
    throw new Exception("Test");
}
catch (Exception ex)
{
    // Should show filename and line number
    Console.WriteLine(ex.StackTrace);
}
```

---

## File Size Impact

```
MyApp.exe (pdbonly):  150 KB + MyApp.pdb: 200 KB
MyApp.exe (embedded): 350 KB (no separate PDB)
```

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| No line numbers in stack trace | Verify `<DebugType>embedded</DebugType>` |
| EXE too large | Use `pdbonly` and distribute PDB separately |
| Path info exposed | Add `<PathMap>` |
| Build fails | Update SDK, check disk space |

---

## Resources

- [Single-file deployment](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file)
- [Deterministic builds](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/code-generation#deterministic)
