---
name: configuring-wpf-themeinfo
description: "Configures ThemeInfo attribute in AssemblyInfo.cs for WPF CustomControl Generic.xaml auto-loading. Use when creating WPF Custom Control Library projects or troubleshooting missing control styles."
---

# WPF ThemeInfo Configuration

Configuring the `ThemeInfo` attribute for automatic Generic.xaml loading in WPF Custom Control Library projects.

## Overview

WPF uses the `ThemeInfo` attribute to locate theme-specific and generic resource dictionaries at runtime. Without this attribute, `Themes/Generic.xaml` will **not** be loaded automatically, and CustomControl styles will not be applied.

---

## 1. AssemblyInfo.cs Configuration

### Required Code

```csharp
// Properties/AssemblyInfo.cs
using System.Windows;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,            // Theme-specific resources
    ResourceDictionaryLocation.SourceAssembly)]  // Generic resources (Themes/Generic.xaml)
```

### File Location

```
YourProject/
├── Properties/
│   └── AssemblyInfo.cs    ← ThemeInfo attribute here
├── Themes/
│   └── Generic.xaml       ← Auto-loaded by ThemeInfo
└── CustomButton.cs
```

---

## 2. ResourceDictionaryLocation Options

| Value | Description | Use Case |
|-------|-------------|----------|
| `None` | No resource dictionary for this category | Theme-specific: when no OS theme customization needed |
| `SourceAssembly` | Resources in the current assembly | Generic: load from `Themes/Generic.xaml` in this assembly |
| `ExternalAssembly` | Resources in a separate assembly | When themes are packaged in `YourAssembly.ThemeName.dll` |

### Parameter Meanings

```csharp
[assembly: ThemeInfo(
    themeDictionaryLocation,    // 1st: Theme-specific (e.g., Aero, Luna)
    genericDictionaryLocation)] // 2nd: Generic fallback (Themes/Generic.xaml)
```

- **1st parameter** (`themeDictionaryLocation`): OS theme-specific styles (rarely used)
- **2nd parameter** (`genericDictionaryLocation`): Generic.xaml location (**must be `SourceAssembly`**)

---

## 3. Anti-Pattern: App.xaml MergedDictionaries

### Bad Example

```xml
<!-- App.xaml - DO NOT DO THIS for CustomControl styles -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="/MyControls;component/Themes/Generic.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### Why This Is Wrong

| Issue | Description |
|-------|-------------|
| **Not standard pattern** | CustomControl styles should be loaded via ThemeInfo, not App.xaml |
| **Cross-assembly failure** | When another assembly references your control library, styles won't load (the consuming app doesn't have your MergedDictionary) |
| **Breaks encapsulation** | The control library should be self-contained; consumers shouldn't need to configure resource loading |

### Correct Approach

1. Add `ThemeInfo` attribute to AssemblyInfo.cs (see Section 1)
2. Place styles in `Themes/Generic.xaml`
3. Control styles load automatically in any consuming application

---

## 4. DefaultStyleKeyProperty Connection

Every CustomControl must set its default style key in the static constructor. This works together with ThemeInfo to locate the correct style:

```csharp
public class CustomButton : Button
{
    static CustomButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CustomButton),
            new FrameworkPropertyMetadata(typeof(CustomButton)));
    }
}
```

**How it works:**
1. `ThemeInfo` tells WPF where to find `Generic.xaml`
2. `DefaultStyleKeyProperty` tells WPF which `TargetType` style to apply
3. WPF matches the style in Generic.xaml by `TargetType="{x:Type local:CustomButton}"`

---

## 5. Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Style not applied | Missing ThemeInfo attribute | Add ThemeInfo to AssemblyInfo.cs |
| Style not applied | Wrong generic parameter | Set 2nd parameter to `SourceAssembly` |
| Style not applied | Missing DefaultStyleKeyProperty | Add `OverrideMetadata` in static constructor |
| Style not applied in other assembly | Using App.xaml MergedDictionaries instead of ThemeInfo | Switch to ThemeInfo attribute |
| Generic.xaml not found | Wrong file location | Must be in `Themes/Generic.xaml` (exact path) |
