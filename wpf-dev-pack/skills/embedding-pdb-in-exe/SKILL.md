---
name: embedding-pdb-in-exe
description: "Embeds PDB debugging symbols into EXE/DLL files. Use when configuring embedded debug symbols, single-file deployment, Source Link integration, or dotnet publish settings."
---

# PDB Embedded Debugging Symbols

## 1. Overview

PDB (Program Database) 파일을 EXE/DLL에 직접 임베드하면 별도의 심볼 파일 없이 스택 트레이스에서 소스 코드 위치 정보를 확인할 수 있습니다.
Embedding PDB files directly into EXE/DLL allows viewing source code locations in stack traces without separate symbol files.

### 1.1 Use Cases

- **단일 파일 배포**: 별도 PDB 파일 없이 디버깅 정보 포함
  Single-file deployment with debugging info without separate PDB
- **크래시 분석**: 프로덕션 환경에서 상세 스택 트레이스 확보
  Detailed stack traces in production for crash analysis
- **Source Link 연동**: GitHub/Azure DevOps 소스 연결
  Source Link integration with GitHub/Azure DevOps

---

## 2. csproj Configuration

### 2.1 Basic Embedded PDB

```xml
<PropertyGroup>
  <DebugType>embedded</DebugType>
</PropertyGroup>
```

### 2.2 Full Configuration (Recommended)

```xml
<PropertyGroup>
  <!-- 디버그 심볼을 EXE/DLL에 임베드 -->
  <!-- Embed debug symbols into EXE/DLL -->
  <DebugType>embedded</DebugType>

  <!-- 전체 디버그 정보 생성 (Release 빌드에서도) -->
  <!-- Generate full debug info (even in Release builds) -->
  <DebugSymbols>true</DebugSymbols>

  <!-- 결정론적 빌드 활성화 (재현 가능한 빌드) -->
  <!-- Enable deterministic builds (reproducible builds) -->
  <Deterministic>true</Deterministic>
</PropertyGroup>
```

### 2.3 Single-File Deployment with Embedded PDB

```xml
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFramework>net10.0-windows</TargetFramework>
  <UseWPF>true</UseWPF>

  <!-- 단일 파일 배포 설정 -->
  <!-- Single-file deployment settings -->
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>

  <!-- PDB 임베드 -->
  <!-- Embed PDB -->
  <DebugType>embedded</DebugType>
  <DebugSymbols>true</DebugSymbols>

  <!-- 네이티브 라이브러리도 단일 파일에 포함 -->
  <!-- Include native libraries in single file -->
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
</PropertyGroup>
```

---

## 3. Command Line Options

### 3.1 Build with Embedded PDB

```bash
# Debug build with embedded PDB
# 임베디드 PDB로 디버그 빌드
dotnet build -p:DebugType=embedded

# Release build with embedded PDB
# 임베디드 PDB로 릴리스 빌드
dotnet build -c Release -p:DebugType=embedded
```

### 3.2 Publish with Embedded PDB

```bash
# Single-file publish with embedded PDB
# 임베디드 PDB로 단일 파일 배포
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=embedded

# Framework-dependent single file
# 프레임워크 종속 단일 파일
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:DebugType=embedded
```

---

## 4. DebugType Options Comparison

| Option | PDB Location | File Size | Stack Trace | Use Case |
|--------|--------------|-----------|-------------|----------|
| `full` | 별도 .pdb 파일 / Separate .pdb file | Small EXE | 상세 / Detailed | 개발 / Development |
| `pdbonly` | 별도 .pdb 파일 / Separate .pdb file | Small EXE | 기본 / Basic | Release (기본) |
| `portable` | 별도 .pdb 파일 / Separate .pdb file | Small EXE | 상세 / Detailed | 크로스 플랫폼 / Cross-platform |
| `embedded` | EXE 내부 / Inside EXE | Large EXE | 상세 / Detailed | 배포 / Distribution |
| `none` | 없음 / None | Small EXE | 최소 / Minimal | 보안 중요 / Security critical |

### 4.1 File Size Impact

```
MyApp.exe (DebugType=pdbonly):  150 KB
MyApp.pdb:                      200 KB

MyApp.exe (DebugType=embedded): 350 KB
(PDB 파일 없음 / No PDB file)
```

---

## 5. Source Link Integration

### 5.1 GitHub Source Link

```xml
<ItemGroup>
  <!-- GitHub Source Link -->
  <PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.*" PrivateAssets="All"/>
</ItemGroup>

<PropertyGroup>
  <DebugType>embedded</DebugType>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
</PropertyGroup>
```

### 5.2 Azure DevOps Source Link

```xml
<ItemGroup>
  <!-- Azure DevOps Source Link -->
  <PackageReference Include="Microsoft.SourceLink.AzureRepos.Git" Version="8.0.*" PrivateAssets="All"/>
</ItemGroup>

<PropertyGroup>
  <DebugType>embedded</DebugType>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
</PropertyGroup>
```

### 5.3 GitLab Source Link

```xml
<ItemGroup>
  <!-- GitLab Source Link -->
  <PackageReference Include="Microsoft.SourceLink.GitLab" Version="8.0.*" PrivateAssets="All"/>
</ItemGroup>

<PropertyGroup>
  <DebugType>embedded</DebugType>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
</PropertyGroup>
```

---

## 6. Conditional Configuration

### 6.1 Release Only Embedded

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <DebugType>embedded</DebugType>
</PropertyGroup>

<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <DebugType>full</DebugType>
</PropertyGroup>
```

### 6.2 CI/CD Pipeline Configuration

```xml
<!-- CI 환경에서만 임베드 -->
<!-- Embed only in CI environment -->
<PropertyGroup Condition="'$(CI)' == 'true'">
  <DebugType>embedded</DebugType>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
</PropertyGroup>
```

---

## 7. Security Considerations

### 7.1 Production Deployment

⚠️ **주의**: 임베디드 PDB는 소스 코드 경로 정보를 포함합니다.
⚠️ **Warning**: Embedded PDB contains source code path information.

```xml
<!-- 민감한 경로 정보 제거 -->
<!-- Remove sensitive path information -->
<PropertyGroup>
  <DebugType>embedded</DebugType>
  <PathMap>$(MSBuildProjectDirectory)=.</PathMap>
  <ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>
</PropertyGroup>
```

### 7.2 Strip PDB for Security-Critical Apps

```xml
<!-- 보안 중요 앱에서는 PDB 제거 -->
<!-- Remove PDB for security-critical apps -->
<PropertyGroup Condition="'$(StripSymbols)' == 'true'">
  <DebugType>none</DebugType>
  <DebugSymbols>false</DebugSymbols>
</PropertyGroup>
```

---

## 8. Verification

### 8.1 Check if PDB is Embedded

```bash
# Using dotnet-symbol tool
# dotnet-symbol 도구 사용
dotnet tool install -g dotnet-symbol
dotnet-symbol --symbols MyApp.exe

# Using dumpbin (Visual Studio)
# dumpbin 사용 (Visual Studio)
dumpbin /headers MyApp.exe | findstr "Debug"
```

### 8.2 Verify Stack Trace Quality

```csharp
try
{
    throw new Exception("Test exception");
}
catch (Exception ex)
{
    // 임베디드 PDB 포함 시 파일명과 라인 번호 표시
    // With embedded PDB, shows filename and line number
    Console.WriteLine(ex.StackTrace);
    // at MyApp.Program.Main(String[] args) in D:\MyApp\Program.cs:line 15
}
```

---

## 9. Troubleshooting

| Issue | Solution |
|-------|----------|
| Stack trace shows no line numbers | Verify `<DebugType>embedded</DebugType>` is set |
| EXE size too large | Use `<DebugType>pdbonly</DebugType>` and distribute PDB separately |
| Source Link not working | Check `<PublishRepositoryUrl>true</PublishRepositoryUrl>` |
| Path information exposed | Add `<PathMap>` to sanitize paths |
| Build fails with embedded PDB | Update to latest SDK, check disk space |

---

## 10. Complete WPF Example

### 10.1 Production-Ready csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>

    <!-- 디버그 심볼 설정 -->
    <!-- Debug symbol settings -->
    <DebugType>embedded</DebugType>
    <DebugSymbols>true</DebugSymbols>
    <Deterministic>true</Deterministic>

    <!-- 경로 정보 난독화 -->
    <!-- Obfuscate path information -->
    <PathMap>$(MSBuildProjectDirectory)=.</PathMap>

    <!-- 단일 파일 배포 (선택) -->
    <!-- Single-file deployment (optional) -->
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  </PropertyGroup>

  <ItemGroup>
    <!-- Source Link (GitHub) -->
    <PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.*" PrivateAssets="All"/>
  </ItemGroup>

</Project>
```

---

## 11. Resources

- [MSBuild - DebugType](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/code-generation#debugtype)
- [Source Link](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink)
- [Single-file deployment](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file)
- [Deterministic builds](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/code-generation#deterministic)
