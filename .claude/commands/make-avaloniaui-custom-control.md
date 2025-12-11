---
description: HTML/CSS to AvaloniaUI AXAML conversion command
allowed-tools: Bash(find:*), Bash(ls:*), Bash(mkdir:*), Bash(mv:*), Bash(cat:*), Bash(rm:*), Bash(dotnet:*), Read, Write, Glob
argument-hint: <ControlName> <description or HTML file path> [CSS file path]
---

# AvaloniaUI Custom Control Generation

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

### Step 2: Search for Solution and AvaloniaUI CustomControl Project Library

#### 2.1 Search for Solution File

Search for `.sln` or `.slnx` file in current directory and parent directories.

```bash
# Find solution files
find . -maxdepth 2 -name "*.sln" -o -name "*.slnx" | head -1
```

#### 2.2 Search for AvaloniaUI CustomControl Project Library

If solution file exists, search for AvaloniaUI Custom Control Library projects within the solution directory.

**AvaloniaUI CustomControl Project Library identification criteria (all must be met):**

1. `.csproj` file exists
2. `Themes` directory exists
3. `Themes/Generic.axaml` file exists
4. `.csproj` contains `Avalonia` package reference

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

- Solution: `{ControlName}.Avalonia.slnx`
- Gallery Project: `{ControlName}.Avalonia.Gallery` (Avalonia Application)
- UI Library Project: `{ControlName}.Avalonia.UI` (Avalonia Class Library)

**Creation steps:**

1. **Create Avalonia Application Project (Gallery):**

```bash
dotnet new avalonia.app -n {ControlName}.Avalonia.Gallery -o {ControlName}.Avalonia.Gallery
```

2. **Create Avalonia Class Library Project (UI):**

```bash
dotnet new avalonia.lib -n {ControlName}.Avalonia.UI -o {ControlName}.Avalonia.UI
```

3. **Create Solution and Add Projects:**

```bash
dotnet new sln -n {ControlName}.Avalonia
dotnet sln {ControlName}.Avalonia.sln add {ControlName}.Avalonia.Gallery/{ControlName}.Avalonia.Gallery.csproj
dotnet sln {ControlName}.Avalonia.sln add {ControlName}.Avalonia.UI/{ControlName}.Avalonia.UI.csproj
```

4. **Migrate to slnx format:**

```bash
dotnet sln {ControlName}.Avalonia.sln migrate
rm -f {ControlName}.Avalonia.sln
```

5. **Add Project Reference (Gallery → UI):**

```bash
dotnet add {ControlName}.Avalonia.Gallery/{ControlName}.Avalonia.Gallery.csproj reference {ControlName}.Avalonia.UI/{ControlName}.Avalonia.UI.csproj
```

6. **Organize UI Project Structure:**

```bash
mkdir -p {ControlName}.Avalonia.UI/Controls
mkdir -p {ControlName}.Avalonia.UI/Themes
```

7. **Create Generic.axaml in Themes folder:**

```xml
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <ResourceDictionary.MergedDictionaries>
        <!-- Style references will be added here -->
    </ResourceDictionary.MergedDictionaries>
</ResourceDictionary>
```

8. **Update Gallery App.axaml to include the theme:**

```xml
<Application.Styles>
    <FluentTheme />
    <StyleInclude Source="avares://{ControlName}.Avalonia.UI/Themes/Generic.axaml"/>
</Application.Styles>
```

9. **Update Gallery MainWindow.axaml to include the custom control:**

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:controls="using:{ControlName}.Avalonia.UI.Controls"
        x:Class="{ControlName}.Avalonia.Gallery.MainWindow"
        Title="Control Gallery" Height="450" Width="800">
    <Grid>
        <controls:{ControlName} />
    </Grid>
</Window>
```

**Target project for control generation:** `{ControlName}.Avalonia.UI`

#### Case B: Solution Exists but No AvaloniaUI CustomControl Library (0 projects)

1. Get solution filename and remove extension, then append `.UI` suffix to determine project name
   - Example: `MyApp.sln` → `MyApp.UI`
2. Create Avalonia Class Library project:

```bash
dotnet new avalonia.lib -n {ProjectName}.UI -o {ProjectName}.UI
```

3. Add project to solution:

```bash
dotnet sln add {ProjectName}.UI/{ProjectName}.UI.csproj
```

4. **Organize new project structure:**

```bash
mkdir -p {ProjectName}.UI/Controls
mkdir -p {ProjectName}.UI/Themes
```

5. **Create Generic.axaml:**

Create `{ProjectName}.UI/Themes/Generic.axaml` with empty MergedDictionaries.

#### Case C: 1 AvaloniaUI CustomControl Library Found (Single project)

- Select that project path as target project
- Create `Controls` directory if it doesn't exist
- Proceed to Step 4

#### Case D: 2 or More AvaloniaUI CustomControl Libraries Found (Multiple projects)

- Display list of discovered AvaloniaUI CustomControl Library projects to user
- Request user to select which project to generate in
- Proceed to Step 4 with selected project path

### Step 4: Create Custom Control Files

Let the target project path be `{TargetProject}` and the 1st argument (control name) be `{ControlName}`.

#### 4.1 Create C# Code File

Create `{TargetProject}/Controls/{ControlName}.cs` file:

```csharp
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace {ProjectNamespace}.Controls;

public sealed class {ControlName} : TemplatedControl
{
    // Add dependency properties as needed
}
```

- `{ProjectNamespace}` is determined based on `.csproj` file's `<RootNamespace>` or project folder name

#### 4.2 Create Theme Resource File (colors, sizes, Path data, etc.)

Analyze the 2nd argument (description or HTML/CSS) to extract Theme-related values and separate them into a dedicated resource dictionary.

Create `{TargetProject}/Themes/{ControlName}Resources.axaml` file:

- Color values → `SolidColorBrush` resources
- Size values → `x:Double` resources
- Path data → `Geometry` or `StreamGeometry` resources
- Alignment/margins → `Thickness`, `CornerRadius`, etc. resources

Resource key naming convention:

- Format: `{ControlName}.{Purpose}.{Property}`
- Examples: `MyButton.Background.Normal`, `MyButton.Border.Radius`

#### 4.3 Create Style ResourceDictionary AXAML File

Create `{TargetProject}/Themes/{ControlName}.axaml` file:

**Structure:**

```xml
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:controls="using:{ProjectNamespace}.Controls">
    <ResourceDictionary.MergedDictionaries>
        <ResourceInclude Source="avares://{ProjectNamespace}/Themes/{ControlName}Resources.axaml" />
    </ResourceDictionary.MergedDictionaries>

    <ControlTheme x:Key="{x:Type controls:{ControlName}}" TargetType="controls:{ControlName}">
        <!-- Setter definitions based on 2nd argument analysis results -->
        <Setter Property="Template">
            <ControlTemplate TargetType="controls:{ControlName}">
                <!-- Generate appropriate AXAML structure by analyzing 2nd argument (description/HTML) -->
            </ControlTemplate>
        </Setter>
    </ControlTheme>
</ResourceDictionary>
```

**When 2nd argument is description text:**

- Analyze description to generate appropriate ControlTemplate structure
- Add necessary property Setters (Width, Height, Cursor, etc.)

**When 2nd argument is HTML file:**

- Read and analyze HTML file content
- Analyze together with 3rd argument (CSS) if provided
- Convert HTML structure to AXAML ControlTemplate
- Separate CSS style values into Theme resource file

#### 4.4 Merge Style ResourceDictionary into Generic.axaml

Modify `{TargetProject}/Themes/Generic.axaml` file to merge new Style ResourceDictionary:

```xml
<ResourceDictionary.MergedDictionaries>
    <!-- Keep existing entries -->
    <ResourceInclude Source="avares://{ProjectNamespace}/Themes/{ControlName}.axaml" />
</ResourceDictionary.MergedDictionaries>
```

### Step 5: Report Results

- Output list of generated files:
  - `Controls/{ControlName}.cs`
  - `Themes/{ControlName}Resources.axaml`
  - `Themes/{ControlName}.axaml`
  - `Themes/Generic.axaml` (modified)
- Include information if new solution/projects were created
- **If Case A (new solution created):**
  - Inform user that Gallery project is ready for testing
  - Provide run command: `dotnet run --project {ControlName}.Avalonia.Gallery`
- Provide guidance for next steps (build verification, adding StyledProperty, etc.)

## HTML to AXAML Conversion Rules

### Element Mapping Rules

| HTML Element | AvaloniaUI Control |
|--------------|-------------------|
| `<div>` | `<Border>` or `<Panel>` or `<StackPanel>` or `<Grid>` |
| `<span>` | `<TextBlock>` |
| `<p>` | `<TextBlock>` |
| `<h1>`~`<h6>` | `<TextBlock>` with FontSize |
| `<button>` | `<Button>` |
| `<input type="text">` | `<TextBox>` |
| `<input type="checkbox">` | `<CheckBox>` |
| `<input type="radio">` | `<RadioButton>` |
| `<select>` | `<ComboBox>` |
| `<textarea>` | `<TextBox AcceptsReturn="True">` |
| `<img>` | `<Image>` |
| `<a>` | `<Button>` or `<HyperlinkButton>` |
| `<ul>`, `<ol>` | `<ItemsControl>` or `<ListBox>` |
| `<li>` | `<ListBoxItem>` or item template |
| `<table>` | `<DataGrid>` or `<Grid>` |
| `<label>` | `<TextBlock>` or `<Label>` |
| `<form>` | `<StackPanel>` or `<Grid>` |
| `<header>`, `<footer>`, `<section>` | `<Border>` or `<DockPanel>` |
| `<nav>` | `<StackPanel Orientation="Horizontal">` |
| `<svg>` | `<Path>` or `<DrawingImage>` |

### Layout Conversion Rules

| CSS Layout | AvaloniaUI Layout |
|------------|-------------------|
| `display: flex; flex-direction: row` | `<StackPanel Orientation="Horizontal">` |
| `display: flex; flex-direction: column` | `<StackPanel Orientation="Vertical">` |
| `display: grid` | `<Grid>` with RowDefinitions/ColumnDefinitions |
| `position: absolute` | `<Canvas>` with Canvas.Left/Top |
| `position: relative` | `<Panel>` or `<Grid>` |

### CSS to AXAML Property Mapping

| CSS Property | AvaloniaUI Property |
|--------------|---------------------|
| `color` | `Foreground` |
| `background-color` | `Background` |
| `background: linear-gradient(...)` | `<LinearGradientBrush>` |
| `background: radial-gradient(...)` | `<RadialGradientBrush>` (Note: GradientOrigin must equal Center) |
| `border` | `BorderBrush`, `BorderThickness` |
| `border-radius` | `CornerRadius` |
| `padding` | `Padding` |
| `margin` | `Margin` |
| `width` | `Width` |
| `height` | `Height` |
| `min-width` | `MinWidth` |
| `max-width` | `MaxWidth` |
| `font-size` | `FontSize` |
| `font-weight` | `FontWeight` |
| `font-family` | `FontFamily` |
| `text-align` | `TextAlignment` or `HorizontalContentAlignment` |
| `opacity` | `Opacity` |
| `cursor: pointer` | `Cursor="Hand"` |
| `overflow: hidden` | `ClipToBounds="True"` |
| `box-shadow` | `BoxShadow` |

### CSS Pseudo-class to AvaloniaUI Pseudo-class Mapping

| CSS Pseudo-class | AvaloniaUI Selector |
|------------------|---------------------|
| `:hover` | `:pointerover` |
| `:active` | `:pressed` |
| `:focus` | `:focus` |
| `:disabled` | `:disabled` |
| `:checked` | `:checked` |
| `:first-child` | `:nth-child(1)` |
| `:last-child` | `:nth-last-child(1)` |

### CSS Unit Conversion

| CSS Unit | AvaloniaUI Value |
|----------|------------------|
| `px` | Direct numeric value (device-independent pixels) |
| `em`, `rem` | Calculate relative to base font size (default 12) |
| `%` | Use `*` in Grid or binding |
| `vw`, `vh` | Use binding to window size |
| `auto` | `Auto` in Grid, `NaN` for Width/Height |

### CSS Animation to AvaloniaUI Animation

**CSS Animation:**
```css
.button:hover {
    transform: scale(1.05);
    transition: transform 0.2s ease;
}
```

**AvaloniaUI Equivalent:**
```xml
<Style Selector="Button:pointerover">
    <Style.Animations>
        <Animation Duration="0:0:0.2" Easing="QuadraticEaseOut">
            <KeyFrame Cue="100%">
                <Setter Property="RenderTransform">
                    <ScaleTransform ScaleX="1.05" ScaleY="1.05"/>
                </Setter>
            </KeyFrame>
        </Animation>
    </Style.Animations>
</Style>
```

### Box Shadow Conversion

**CSS:**
```css
.card {
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}
```

**AvaloniaUI Equivalent:**
```xml
<Border BoxShadow="0 4 6 #1A000000">
    <!-- Content -->
</Border>
```

## Generated File Structure Summary

### Case A: New Solution with Gallery (No existing solution)

```
{ControlName}.Avalonia.slnx
{ControlName}.Avalonia.Gallery/
├── {ControlName}.Avalonia.Gallery.csproj
├── App.axaml
├── App.axaml.cs
├── MainWindow.axaml                   # Pre-configured with control reference
└── MainWindow.axaml.cs
{ControlName}.Avalonia.UI/
├── {ControlName}.Avalonia.UI.csproj
├── Controls/
│   └── {ControlName}.cs              # Control class definition
└── Themes/
    ├── Generic.axaml                 # Style reference added to MergedDictionaries
    ├── {ControlName}.axaml           # ControlTheme definition
    └── {ControlName}Resources.axaml  # Theme resources: colors, sizes, Path, etc.
```

### Case B/C/D: Existing Solution

```
{TargetProject}/
├── Controls/
│   └── {ControlName}.cs              # Control class definition
└── Themes/
    ├── Generic.axaml                 # Style reference added to MergedDictionaries
    ├── {ControlName}.axaml           # ControlTheme definition
    └── {ControlName}Resources.axaml  # Theme resources: colors, sizes, Path, etc.
```

## Common Conversion Mistakes to Avoid

1. **RadialGradientBrush Differences**: AvaloniaUI Issue #19888 - GradientOrigin must equal Center value
2. **Pseudo-class Syntax**: Use `:pointerover` instead of `:hover`
3. **Namespace**: Always use `https://github.com/avaloniaui` as default namespace
4. **Style Selector**: AvaloniaUI uses CSS-like selectors, not `TargetType` like WPF
5. **CornerRadius**: Can be specified as single value or `TopLeft,TopRight,BottomRight,BottomLeft`
6. **Margin/Padding**: Order is `Left,Top,Right,Bottom` (differs from CSS `Top,Right,Bottom,Left`)
7. **ControlTheme**: Use `ControlTheme` instead of WPF's `Style` for templated controls
8. **ResourceInclude**: Use `ResourceInclude` instead of WPF's `ResourceDictionary` with `Source`
9. **avares URI**: Use `avares://AssemblyName/Path` instead of WPF's `pack://application:,,,`

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
/html-css-to-avaloniaui-control MyButton "Custom button with rounded corners and hover effect"

# With HTML file
/html-css-to-avaloniaui-control CardPanel ./designs/card.html

# With HTML and CSS files
/html-css-to-avaloniaui-control CardPanel ./designs/card.html ./designs/card.css
```

## Quick Start After Generation (Case A)

When a new solution with Gallery is created, immediately test the control:

```bash
# Build and run the Gallery application
dotnet run --project {ControlName}.Avalonia.Gallery

# Or build only
dotnet build
```
