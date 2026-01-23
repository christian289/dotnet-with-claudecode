---
name: migrating-wpf-to-dotnet
description: "Migrates WPF applications from .NET Framework to .NET 6+. Use when upgrading legacy WPF apps, resolving compatibility issues, or modernizing project files."
---

# WPF .NET Framework → .NET Migration

## 1. Migration Checklist

### 1.1 Pre-Migration

- [ ] Verify .NET Framework version (4.6.1+ recommended)
- [ ] Check NuGet package .NET compatibility
- [ ] Verify third-party library .NET versions
- [ ] Check Windows Forms interop usage
- [ ] Check WCF client usage
- [ ] Check ClickOnce deployment usage

### 1.2 Migration

- [ ] Convert project file to SDK style
- [ ] NuGet packages.config → PackageReference
- [ ] Clean up AssemblyInfo.cs
- [ ] app.config → appsettings.json (optional)
- [ ] Build and fix errors
- [ ] Run tests

### 1.3 Post-Migration

- [ ] Test self-contained deployment
- [ ] Test single-file deployment
- [ ] Ready to Run (R2R) optimization
- [ ] Performance benchmark

---

## 2. Project File Conversion

### 2.1 Old Format (.NET Framework)

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" />
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProjectGuid>{GUID}</ProjectGuid>
    <OutputType>WinExe</OutputType>
    <RootNamespace>MyApp</RootNamespace>
    <AssemblyName>MyApp</AssemblyName>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
    <!-- ... hundreds of lines ... -->
  </PropertyGroup>
  <!-- ... -->
</Project>
```

### 2.2 New SDK Format (.NET 10)

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.*" />
  </ItemGroup>

</Project>
```

---

## 3. Step-by-Step Migration

### 3.1 Try-Convert Tool (Automated)

```bash
# Install
dotnet tool install -g try-convert

# Run
try-convert -p MyApp.csproj
```

### 3.2 Manual Conversion

**Step 1: Create new project file**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <RootNamespace>MyApp</RootNamespace>
    <AssemblyName>MyApp</AssemblyName>
  </PropertyGroup>
</Project>
```

**Step 2: Migrate NuGet references**

```xml
<!-- packages.config → PackageReference -->
<ItemGroup>
  <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />
</ItemGroup>
```

**Step 3: Clean up AssemblyInfo.cs**

SDK style auto-generates most attributes. Keep only custom attributes:

```csharp
// Properties/AssemblyInfo.cs (keep only necessary items)
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: Guid("your-guid-here")]
```

Disable generation in project file (if needed):

```xml
<PropertyGroup>
  <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
</PropertyGroup>
```

---

## 4. Breaking Changes

### 4.1 Removed APIs

| API | .NET Framework | .NET Alternative |
|-----|---------------|-----------------|
| `BinaryFormatter` | ✅ | ⚠️ Removed for security. Use `System.Text.Json` |
| `AppDomain.CreateDomain` | ✅ | ❌ Not supported |
| `Remoting` | ✅ | ❌ Use gRPC, SignalR |
| `Code Access Security` | ✅ | ❌ Removed |

### 4.2 WCF Client

```xml
<!-- WCF client (server not supported) -->
<ItemGroup>
  <PackageReference Include="System.ServiceModel.Http" Version="8.0.*" />
  <PackageReference Include="System.ServiceModel.Primitives" Version="8.0.*" />
</ItemGroup>
```

### 4.3 Windows Forms Interop

```xml
<PropertyGroup>
  <UseWPF>true</UseWPF>
  <UseWindowsForms>true</UseWindowsForms>
</PropertyGroup>
```

---

## 5. Dual Targeting (Gradual Migration)

### 5.1 Multi-Target

```xml
<PropertyGroup>
  <TargetFrameworks>net48;net10.0-windows</TargetFrameworks>
  <UseWPF>true</UseWPF>
</PropertyGroup>

<ItemGroup Condition="'$(TargetFramework)' == 'net48'">
  <Reference Include="System.Windows.Forms" />
</ItemGroup>

<ItemGroup Condition="'$(TargetFramework)' == 'net10.0-windows'">
  <PackageReference Include="Microsoft.Windows.Compatibility" Version="9.0.*" />
</ItemGroup>
```

### 5.2 Conditional Compilation

```csharp
#if NET48
    // .NET Framework specific code
    System.Configuration.ConfigurationManager.AppSettings["Key"];
#else
    // .NET specific code
    Microsoft.Extensions.Configuration.IConfiguration config;
#endif
```

---

## 6. Modern .NET Features

### 6.1 Nullable Reference Types

```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string? _optionalValue;
    [ObservableProperty] private string _requiredValue = string.Empty;
}
```

### 6.2 File-Scoped Namespaces

```csharp
// Old
namespace MyApp.ViewModels
{
    public class MainViewModel { }
}

// New
namespace MyApp.ViewModels;

public class MainViewModel { }
```

### 6.3 Global Usings

```csharp
// GlobalUsings.cs
global using System;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.Linq;
global using System.Threading.Tasks;
global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Input;
```

---

## 7. Deployment

### 7.1 Self-Contained

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

### 7.2 Single File

```xml
<PropertyGroup>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
</PropertyGroup>
```

### 7.3 Ready to Run (AOT)

```xml
<PropertyGroup>
  <PublishReadyToRun>true</PublishReadyToRun>
</PropertyGroup>
```

---

## 8. Common Issues

| Issue | Solution |
|-------|----------|
| `System.Drawing` error | Add `System.Drawing.Common` NuGet |
| WCF server code | Migrate to CoreWCF or gRPC |
| `app.config` reading | Use `Microsoft.Extensions.Configuration` |
| XAML designer error | Requires Visual Studio 2022 17.8+ |
| ClickOnce | Replace with MSIX or custom updater |

---

## 9. Resources

- [.NET Upgrade Assistant](https://learn.microsoft.com/en-us/dotnet/core/porting/upgrade-assistant-overview)
- [Porting from .NET Framework](https://learn.microsoft.com/en-us/dotnet/core/porting/)
- [WPF Migration Guide](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/migration/)
- [Breaking Changes in .NET](https://learn.microsoft.com/en-us/dotnet/core/compatibility/)
