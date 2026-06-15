# ThemeGallery — Theme Dictionary Contract

Every theme is ONE self-contained `ResourceDictionary` XAML file in
`src/ThemeGallery.Themes/` named `<ThemeName>.xaml`. The gallery swaps the
whole dictionary at runtime, so a theme must not depend on any other file.

## 1. Hard Requirements

1. **Self-contained**: no `MergedDictionaries`, no external images/fonts.
   All visuals are drawn with XAML primitives (gradients, `Geometry`,
   `Path`, shapes inside templates).
2. **Root element**: `<ResourceDictionary>` with only the two standard
   namespaces (`presentation` + `x`). No code-behind, no converters,
   no custom markup extensions, no `clr-namespace` imports except
   `System` from `mscorlib` if a constant is truly needed.
3. **Shell brush keys** — the gallery window consumes these via
   `DynamicResource`; every theme MUST define all of them:
   - `ThemeWindowBackgroundBrush` (any `Brush`, gradients encouraged)
   - `ThemeTextBrush`
   - `ThemeSubtleTextBrush`
   - `ThemeAccentBrush`
   - `ThemeCardBrush` (panel/surface background)
   - `ThemeBorderBrush`
4. **Implicit styles** (no `x:Key`, applied by `TargetType`) for ALL of:
   `Button`, `ToggleButton`, `CheckBox`, `RadioButton`, `TextBox`,
   `ComboBox`, `ComboBoxItem`, `Slider`, `ProgressBar`, `ScrollBar`,
   `TabControl`, `TabItem`, `ListBox`, `ListBoxItem`, `Expander`.
   Each interactive control gets a full `ControlTemplate` (not just
   property setters) so the theme identity is unmistakable.
5. **Smooth animation is mandatory**: every interactive control animates
   at least its hover AND its press/check/select/focus transition with
   `Storyboard`s (Trigger Enter/ExitActions or VSM), `Duration` between
   `0:0:0.12` and `0:0:0.6`, with an easing function. Each theme also has
   at least one **signature ambient animation** (a looping motion that
   sells the concept, e.g. an orbiting planet or a flickering neon tube).
6. **Disabled state**: every templated control visually handles
   `IsEnabled=False` (reduced opacity is acceptable).

## 2. Template Part Names (WPF looks these up — get them exactly right)

| Control | Required named parts inside the ControlTemplate |
|---|---|
| `TextBox` | `ScrollViewer x:Name="PART_ContentHost"` |
| `ProgressBar` | root track element `x:Name="PART_Track"`, fill element `x:Name="PART_Indicator"` (WPF sizes the indicator for determinate values) |
| `Slider` | `Track x:Name="PART_Track"` with `Track.DecreaseRepeatButton`, `Track.IncreaseRepeatButton`, `Track.Thumb` |
| `ScrollBar` | `Track x:Name="PART_Track"` (same structure; style must template BOTH orientations via an `Orientation` trigger swapping templates) |
| `ComboBox` | `Popup x:Name="PART_Popup"`; dropdown toggle = `ToggleButton` with `IsChecked="{Binding IsDropDownOpen, Mode=TwoWay, RelativeSource={RelativeSource TemplatedParent}}"`; items host = `ItemsPresenter`; `ScrollViewer` around it |
| `TabControl` | `ContentPresenter ContentSource="SelectedContent"` |
| `Expander` | header `ToggleButton` with `IsChecked="{Binding IsExpanded, Mode=TwoWay, RelativeSource={RelativeSource TemplatedParent}}"`; content collapses via trigger on `IsExpanded` |

`ProgressBar.IsIndeterminate=True`: hide `PART_Indicator` and show a
looping element animated with `RepeatBehavior="Forever"` (e.g. a sweeping
`TranslateTransform`).

## 3. Animation Safety Rules (performance + correctness)

1. Animate ONLY `Opacity`, `RenderTransform` sub-properties, brush
   `Color`/`Offset`/gradient stops, and `Effect.Opacity`/`BlurRadius` on
   SMALL elements. Never animate `Width`/`Height`/`Margin` of containers.
2. **A brush whose `Color` is animated must be declared inline on the
   element** (`<Border.Background><SolidColorBrush .../></Border.Background>`),
   NOT pulled from a shared `{StaticResource}` — animating a shared/frozen
   brush throws at runtime.
3. `DropShadowEffect` is allowed for glow but only on small elements,
   `BlurRadius <= 25`, and never stacked.
4. Ambient `RepeatBehavior="Forever"` storyboards run on small decorative
   elements only (a 10–30 px shape, a gradient offset), never on the
   window background as a layout-affecting animation.
5. Storyboard property paths for brush colors:
   `(Border.Background).(SolidColorBrush.Color)` (parenthesized).
6. Every storyboard that enters on a trigger has a matching exit
   (`ExitActions` with a return animation or a short `To`-less fade-back)
   so state never sticks.

## 4. Other Conventions

- Foreground inside templates: `TemplateBinding Foreground` (set the
  themed default with a `Setter`); content text then inherits correctly.
- `SnapsToDevicePixels=True` and `UseLayoutRounding=True` on template
  roots with hard edges.
- RepeatButtons inside Slider/ScrollBar tracks: transparent template,
  `Focusable=False`, `IsTabStop=False`.
- Keyboard focus: templated controls keep a visible focus cue
  (`IsKeyboardFocused` trigger or `FocusVisualStyle`).
- The file must be well-formed XML (validate with
  `[xml](Get-Content <file> -Raw)` in PowerShell before finishing).

## 5. Theme Roster

| File | Concept |
|---|---|
| `SolarSystem.xaml` | Deep space, orbit rings, planets as thumbs/glyphs, sun-glow hover |
| `Diablo.xaml` | Gothic dark fantasy, ember red, carved stone, pulsing demonic glow |
| `Metal.xaml` | Brushed steel, machined edges, chrome highlight sweep, stamped press |
| `Korea.xaml` | Taegeuk red/blue, dancheong palette, hanji paper, seal-stamp check |
| `Flame.xaml` | Charcoal + fire gradient, licking flames, burning-fuse progress |
| `Spring.xaml` | Cherry blossom, petal pinks + fresh green, blooming check, gentle sway |
| `Summer.xaml` | Ocean & beach, cyan + sand, wave-fill progress, sun-glare hover |
| `Autumn.xaml` | Maple orange/brown, falling leaf, leaf-flip expander |
| `Winter.xaml` | Ice blue/white, frost, crystallizing snowflake check, aurora accent |
| `Neon.xaml` | Dark club wall, neon-tube glow, ignition flicker on hover, halogen warm white |
