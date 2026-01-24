---
name: localizing-wpf-with-baml
description: "Localizes WPF applications using BAML localization with x:Uid attributes and LocBaml tool. Use when building enterprise multi-language applications requiring satellite assemblies."
---

# WPF BAML Localization

Localize WPF applications using x:Uid attributes and LocBaml tool for satellite assembly generation.

## 1. BAML Localization Overview

```
BAML Localization Workflow
├── 1. Add x:Uid to XAML elements
├── 2. Set UICulture in project
├── 3. Build to generate default resources
├── 4. Extract with LocBaml /parse
├── 5. Translate CSV files
└── 6. Generate satellite assemblies with LocBaml /generate
```

**When to use BAML:**
- Enterprise applications with professional translation workflow
- Need to localize without recompiling
- Complex UI with many localizable properties

---

## 2. Adding x:Uid Attributes

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        x:Uid="MainWindow"
        Title="My Application">
    <Grid x:Uid="MainGrid">
        <StackPanel x:Uid="ContentPanel">
            <TextBlock x:Uid="TitleText" Text="Welcome to My Application"/>
            <TextBlock x:Uid="DescriptionText" Text="Please select an option below."/>
            <Button x:Uid="SaveButton" Content="Save"/>
            <Button x:Uid="CancelButton" Content="Cancel"/>
            <Button x:Uid="HelpButton" Content="Help" ToolTip="Click for help"/>
        </StackPanel>
    </Grid>
</Window>
```

**x:Uid naming conventions:**
- Use descriptive names: `SaveButton`, `TitleText`
- Unique within the XAML file
- Applied to elements with localizable properties

---

## 3. Project Configuration

### 3.1 Set UICulture

```xml
<!-- .csproj -->
<PropertyGroup>
    <UICulture>en-US</UICulture>
</PropertyGroup>
```

### 3.2 Build Output Structure

```
bin/Release/
├── MyApp.exe
├── MyApp.resources.dll
└── ko-KR/
    └── MyApp.resources.dll    ← Korean satellite assembly
```

---

## 4. LocBaml Tool Usage

### 4.1 Extract Resources

```bash
# Parse and extract to CSV
LocBaml /parse MyApp.resources.dll /out:en-US.csv
```

**CSV output format:**
```csv
MyApp.g.en-US.resources:mainwindow.baml,MainWindow:Window.Title,Title,True,True,None,My Application
MyApp.g.en-US.resources:mainwindow.baml,TitleText:TextBlock.Text,Text,True,True,None,Welcome to My Application
MyApp.g.en-US.resources:mainwindow.baml,SaveButton:Button.Content,Content,True,True,None,Save
```

### 4.2 Translate CSV

Create `ko-KR.csv` with translations:
```csv
MyApp.g.en-US.resources:mainwindow.baml,MainWindow:Window.Title,Title,True,True,None,내 애플리케이션
MyApp.g.en-US.resources:mainwindow.baml,TitleText:TextBlock.Text,Text,True,True,None,내 애플리케이션에 오신 것을 환영합니다
MyApp.g.en-US.resources:mainwindow.baml,SaveButton:Button.Content,Content,True,True,None,저장
```

### 4.3 Generate Satellite Assembly

```bash
# Create Korean satellite assembly
LocBaml /generate MyApp.resources.dll /trans:ko-KR.csv /cul:ko-KR /out:.\ko-KR

# Create Japanese satellite assembly
LocBaml /generate MyApp.resources.dll /trans:ja-JP.csv /cul:ja-JP /out:.\ja-JP
```

---

## 5. Automation Script

```powershell
# build-localization.ps1
param(
    [string]$ProjectPath = ".",
    [string[]]$Cultures = @("ko-KR", "ja-JP", "de-DE")
)

$OutputPath = "$ProjectPath\bin\Release"
$ResourceDll = "$OutputPath\MyApp.resources.dll"

# Extract base resources
LocBaml /parse $ResourceDll /out:"$OutputPath\en-US.csv"

# Generate satellite assemblies for each culture
foreach ($culture in $Cultures) {
    $csvPath = "$ProjectPath\Localization\$culture.csv"
    $outPath = "$OutputPath\$culture"

    if (Test-Path $csvPath) {
        New-Item -ItemType Directory -Force -Path $outPath | Out-Null
        LocBaml /generate $ResourceDll /trans:$csvPath /cul:$culture /out:$outPath
        Write-Host "Generated: $culture"
    }
}
```

---

## 6. Runtime Culture Selection

```csharp
// App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    // Set culture before any UI is created
    var culture = new CultureInfo("ko-KR");
    Thread.CurrentThread.CurrentCulture = culture;
    Thread.CurrentThread.CurrentUICulture = culture;

    base.OnStartup(e);
}
```

---

## 7. Localizable Properties

| Element | Localizable Properties |
|---------|----------------------|
| Window | Title |
| TextBlock | Text |
| Button | Content, ToolTip |
| Label | Content |
| MenuItem | Header |
| ToolTip | Content |

---

## 8. References

- [WPF Globalization - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/wpf-globalization-and-localization-overview)
- [LocBaml Tool - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/how-to-localize-an-application)
