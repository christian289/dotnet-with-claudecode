---
name: building-mewui-apps
description: "Creates MewUI applications with proper setup, windows, theming, and controls. Use when starting a new MewUI project, understanding application lifecycle, using built-in controls, or styling with themes."
---

## Minimal App

```csharp
using MewUI;
using MewUI.Controls;

var window = new Window()
    .Title("My App")
    .Width(800).Height(600)
    .Content(new Label().Text("Hello, MewUI!"));

Application.Run(window);  // Static method
```

---

## Application Setup

```csharp
// Set defaults BEFORE Run()
Application.DefaultGraphicsBackend = GraphicsBackend.Direct2D;  // or Gdi, OpenGL

var window = new Window().Title("My App").Content(mainContent);
Application.Run(window);

// Access current app AFTER Run() starts
// Application.Current.SetTheme(ThemeVariant.Light);
```

---

## Common Controls

```csharp
new Label().Text("Display text").BindText(observable)
new TextBox().BindText(observable).Placeholder("Hint")
new Button().Content("Click").OnClick(() => DoAction())
new CheckBox().Text("Option").BindIsChecked(observable)  // Note: .Text() not .Content()
new ComboBox().Items("A", "B", "C").BindSelectedIndex(observable)
new ListBox().Items("X", "Y", "Z").BindSelectedIndex(observable)
new Slider().Minimum(0).Maximum(100).BindValue(observable)
new ProgressBar().Minimum(0).Maximum(100).BindValue(observable)
new Image().SourceFile("path.png").StretchMode(ImageStretch.Uniform)  // Note: SourceFile, StretchMode
```

---

## Theming

```csharp
// Access theme in controls
var bg = Theme.Palette.ControlBackground;
var accent = Theme.Palette.Accent;  // Note: Accent, not AccentColor
var radius = Theme.Metrics.ControlCornerRadius;

// Visual states
var color = _isPressed ? Theme.Palette.ButtonPressedBackground
    : IsMouseOver ? Theme.Palette.ButtonHoverBackground
    : Theme.Palette.ButtonFace;

// Theme change notification - note two parameters
protected override void OnThemeChanged(Theme oldTheme, Theme newTheme)
{
    base.OnThemeChanged(oldTheme, newTheme);
    InvalidateVisual();
}
```

---

## Fluent Styling

```csharp
element
    .Width(200).Height(100)
    .Margin(8).Padding(4)
    .Background(Colors.White).Foreground(Colors.Black)
    .HorizontalAlignment(HorizontalAlignment.Center)
    .VerticalAlignment(VerticalAlignment.Stretch)
```

---

## App Layout Pattern

```csharp
new DockPanel().Children(
    CreateMenu().DockTo(Dock.Top),      // Note: .DockTo() not .Dock()
    CreateToolbar().DockTop(),           // Or convenience methods
    CreateStatusBar().DockBottom(),
    CreateSidebar().DockLeft().Width(200),
    CreateMainContent()  // Fills remaining
)
```

---

**Dialogs & Popups**: See [dialogs.md](dialogs.md)
**Window management**: See [windows.md](windows.md)
