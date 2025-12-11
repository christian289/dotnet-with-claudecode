---
description: WPF CustomControl generation command
allowed-tools: Bash(find:*), Bash(ls:*), Bash(mkdir:*), Bash(mv:*), Bash(cat:*), Bash(rm:*), Bash(dotnet:*), Read, Write, Glob
argument-hint: <ControlName> <description or HTML file path> [CSS file path]
---

# WPF Custom Control Generation

## Argument Information

- Received arguments: $ARGUMENTS
- Argument format: `<ControlName> <description|HTML file> [CSS file]`
  - 1st argument (required): Custom Control file name (e.g., MyButton, CustomTextBox)
  - 2nd argument (required): Control description text or HTML file path (.html extension)
  - 3rd argument (optional): CSS file path (only used when 2nd argument is an HTML file)

## Execution Procedure

### Step 1: Argument Parsing and Validation

- Parse arguments by splitting with spaces
- If 1st or 2nd argument is missing, output error message and abort
- Check if 2nd argument has `.html` extension to determine description mode or HTML mode
- If in HTML mode and 3rd argument exists, verify CSS file existence

### Step 2: Search for Solution and WPF CustomControl Project Library

#### 2.1 Search for Solution File

Search for `.sln` or `.slnx` file in current directory and parent directories.

```bash
# Find solution files
find . -maxdepth 2 -name "*.sln" -o -name "*.slnx" | head -1
```

#### 2.2 Search for WPF CustomControl Project Library

If solution file exists, search for WPF Custom Control Library projects within the solution directory.

**WPF CustomControl Project Library identification criteria (all must be met):**

1. `.csproj` file exists
2. `AssemblyInfo.cs` file exists (under Properties/ or root)
3. `Themes` directory exists
4. `Themes/Generic.xaml` file exists

**Search commands:**

```bash
# Find directories containing .csproj files
find . -name "*.csproj" -type f

# Verify above conditions for each project directory
```

### Step 3: Branch Processing Based on Solution and Project Status

#### Case A: No Solution File Found (Create New Solution with Gallery)

When no `.sln` or `.slnx` file is found in the current directory, create a complete development environment including a Gallery application for testing the custom control.

**Project naming convention:**

- Solution: `{ControlName}.Wpf.slnx`
- Gallery Project: `{ControlName}.Wpf.Gallery` (WPF Application)
- UI Library Project: `{ControlName}.Wpf.UI` (WPF Custom Control Library)

**Creation steps:**

1. **Create WPF Application Project (Gallery):**

```bash
dotnet new wpf -n {ControlName}.Wpf.Gallery -o {ControlName}.Wpf.Gallery
mkdir -p {ControlName}.Wpf.Gallery/Properties
mv {ControlName}.Wpf.Gallery/AssemblyInfo.cs {ControlName}.Wpf.Gallery/Properties/AssemblyInfo.cs
```

2. **Create WPF Custom Control Library Project (UI):**

```bash
dotnet new wpfcustomcontrollib -n {ControlName}.Wpf.UI -o {ControlName}.Wpf.UI
```

3. **Create Solution and Add Projects:**

```bash
dotnet new sln -n {ControlName}.Wpf
dotnet sln {ControlName}.Wpf.sln add {ControlName}.Wpf.Gallery/{ControlName}.Wpf.Gallery.csproj
dotnet sln {ControlName}.Wpf.sln add {ControlName}.Wpf.UI/{ControlName}.Wpf.UI.csproj
```

4. **Migrate to slnx format:**

```bash
dotnet sln {ControlName}.Wpf.sln migrate
rm -f {ControlName}.Wpf.sln
```

5. **Add Project Reference (Gallery → UI):**

```bash
dotnet add {ControlName}.Wpf.Gallery/{ControlName}.Wpf.Gallery.csproj reference {ControlName}.Wpf.UI/{ControlName}.Wpf.UI.csproj
```

6. **Organize UI Project Structure:**

```bash
mkdir -p {ControlName}.Wpf.UI/Properties
mkdir -p {ControlName}.Wpf.UI/Controls
mv {ControlName}.Wpf.UI/AssemblyInfo.cs {ControlName}.Wpf.UI/Properties/AssemblyInfo.cs
rm {ControlName}.Wpf.UI/CustomControl1.cs
```

7. **Update Gallery MainWindow.xaml to include the custom control:**

Add namespace declaration and sample usage of the control for immediate testing.

```xml
<Window x:Class="{ControlName}.Wpf.Gallery.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:controls="clr-namespace:{ControlName}.Wpf.UI.Controls;assembly={ControlName}.Wpf.UI"
        Title="Control Gallery" Height="450" Width="800">
    <Grid>
        <controls:{ControlName} />
    </Grid>
</Window>
```

**Target project for control generation:** `{ControlName}.Wpf.UI`

#### Case B: Solution Exists but No WPF CustomControl Library (0 projects)

1. Get solution filename and remove extension, then append `.UI` suffix to determine project name
   - Example: `MyApp.sln` → `MyApp.UI`
2. Create WPF Custom Control Library project:

```bash
dotnet new wpfcustomcontrollib -n {ProjectName}.UI -o {ProjectName}.UI
```

3. Add project to solution:

```bash
dotnet sln add {ProjectName}.UI/{ProjectName}.UI.csproj
```

4. **Organize new project structure:**
   - Create `Properties` directory
   - Move `AssemblyInfo.cs` to `Properties/AssemblyInfo.cs`
   - Create `Controls` directory
   - Delete `CustomControl1.cs` file

```bash
mkdir -p {ProjectName}.UI/Properties
mkdir -p {ProjectName}.UI/Controls
mv {ProjectName}.UI/AssemblyInfo.cs {ProjectName}.UI/Properties/AssemblyInfo.cs
rm -f ProjectName}.UI/CustomControl1.cs
```

#### Case C: 1 WPF CustomControl Library Found (Single project)

- Select that project path as target project
- Create `Controls` directory if it doesn't exist
- Proceed to Step 4

#### Case D: 2 or More WPF CustomControl Libraries Found (Multiple projects)

- Display list of discovered WPF CustomControl Library projects to user
- Request user to select which project to generate in
- Proceed to Step 4 with selected project path

### Step 4: Create Custom Control Files

Let the target project path be `{TargetProject}` and the 1st argument (control name) be `{ControlName}`.

#### 4.1 Create C# Code File

Create `{TargetProject}/Controls/{ControlName}.cs` file:

```csharp
using System.Windows;
using System.Windows.Controls;

namespace {ProjectNamespace}.Controls;

public sealed class {ControlName} : Control
{
    static {ControlName}()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof({ControlName}),
            new FrameworkPropertyMetadata(typeof({ControlName})));
    }
}
```

- `{ProjectNamespace}` is determined based on `.csproj` file's `<RootNamespace>` or project folder name

#### 4.2 Create Theme Resource File (colors, sizes, Path data, etc.)

Analyze the 2nd argument (description or HTML/CSS) to extract Theme-related values and separate them into a dedicated resource dictionary.

Create `{TargetProject}/Themes/{ControlName}Resources.xaml` file:

- Color values → `SolidColorBrush` resources
- Size values → `sys:Double` resources
- Path data → `Geometry` or `StreamGeometry` resources
- Alignment/margins → `Thickness`, `CornerRadius`, etc. resources

Resource key naming convention:

- Format: `{ControlName}.{Purpose}.{Property}`
- Examples: `MyButton.Background.Normal`, `MyButton.Border.Radius`

#### 4.3 Create Style ResourceDictionary XAML File

Create `{TargetProject}/Themes/{ControlName}.xaml` file:

**Structure:**

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="clr-namespace:{ProjectNamespace}.Controls">
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="/{ProjectNamespace};component/Themes/{ControlName}Resources.xaml" />
    </ResourceDictionary.MergedDictionaries>

    <Style TargetType="{x:Type controls:{ControlName}}">
        <!-- Setter definitions based on 2nd argument analysis results -->
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type controls:{ControlName}}">
                    <!-- Generate appropriate XAML structure by analyzing 2nd argument (description/HTML) -->
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>
```

**When 2nd argument is description text:**

- Analyze description to generate appropriate ControlTemplate structure
- Add necessary property Setters (Width, Height, Cursor, etc.)

**When 2nd argument is HTML file:**

- Read and analyze HTML file content
- Analyze together with 3rd argument (CSS) if provided
- Convert HTML structure to XAML ControlTemplate
- Separate CSS style values into Theme resource file

#### 4.4 Merge Style ResourceDictionary into Generic.xaml

Modify `{TargetProject}/Themes/Generic.xaml` file to merge new Style ResourceDictionary:

```xml
<ResourceDictionary.MergedDictionaries>
    <!-- Keep existing entries -->
    <ResourceDictionary Source="/{ProjectNamespace};component/Themes/{ControlName}.xaml" />
</ResourceDictionary.MergedDictionaries>
```

### Step 5: Report Results

- Output list of generated files:
  - `Controls/{ControlName}.cs`
  - `Themes/{ControlName}Resources.xaml`
  - `Themes/{ControlName}.xaml`
  - `Themes/Generic.xaml` (modified)
- Include information if new solution/projects were created
- **If Case A (new solution created):**
  - Inform user that Gallery project is ready for testing
  - Provide run command: `dotnet run --project {ControlName}.Wpf.Gallery`
- Provide guidance for next steps (build verification, adding DependencyProperty, etc.)

## Generated File Structure Summary

### Case A: New Solution with Gallery (No existing solution)

```
{ControlName}.Wpf.slnx
{ControlName}.Wpf.Gallery/
├── {ControlName}.Wpf.Gallery.csproj
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml                   # Pre-configured with control reference
└── MainWindow.xaml.cs
{ControlName}.Wpf.UI/
├── {ControlName}.Wpf.UI.csproj
├── Controls/
│   └── {ControlName}.cs              # Control class definition
├── Themes/
│   ├── Generic.xaml                  # Style reference added to MergedDictionaries
│   ├── {ControlName}.xaml            # Style + ControlTemplate definition
│   └── {ControlName}Resources.xaml   # Theme resources: colors, sizes, Path, etc.
└── Properties/
    └── AssemblyInfo.cs
```

### Case B/C/D: Existing Solution

```
{TargetProject}/
├── Controls/
│   └── {ControlName}.cs              # Control class definition
├── Themes/
│   ├── Generic.xaml                  # Style reference added to MergedDictionaries
│   ├── {ControlName}.xaml            # Style + ControlTemplate definition
│   └── {ControlName}Resources.xaml   # Theme resources: colors, sizes, Path, etc.
└── Properties/
    └── AssemblyInfo.cs
```

## Error Handling

- Missing required arguments: Output usage instructions message
- HTML file not found: Request file path verification
- CSS file not found: Warn and proceed without CSS
- dotnet CLI error: Provide error details and resolution guidance
- `dotnet sln migrate` failure: Continue with .sln format and notify user
- Multiple solution files exist: After migration, delete the original `.sln` file to prevent MSB1011 error
- Platform-specific commands: Use cross-platform compatible commands (`mv`, `rm -f`, `mkdir -p`) instead of Windows-specific commands (`move`, `del`, `mkdir`)

## Usage Examples

```
# Basic usage with description (creates Gallery if no solution exists)
/project:wpf-custom-control MyButton "Custom button with rounded corners and hover effect"

# With HTML file
/project:wpf-custom-control CardPanel ./designs/card.html

# With HTML and CSS files
/project:wpf-custom-control CardPanel ./designs/card.html ./designs/card.css
```

## Quick Start After Generation (Case A)

When a new solution with Gallery is created, immediately test the control:

```bash
# Build and run the Gallery application
dotnet run --project {ControlName}.Wpf.Gallery

# Or build only
dotnet build
```
