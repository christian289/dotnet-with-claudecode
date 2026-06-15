# Animating WPF ControlTemplates and Themes

> Authors smooth Storyboard animations inside WPF ControlTemplates — hover/press/check transitions, ambient looping signatures, and runtime theme switching — for both restyled stock controls (ControlTemplate.Triggers) and custom controls (VisualStateManager VisualState/VisualTransition Storyboards). Use when a template needs animation, when restyling Button/CheckBox/Slider/ComboBox/ProgressBar/Expander/etc. into a theme, when MC4111 "Trigger target not found" fires, when a StaticResource resolves to {DependencyProperty.UnsetValue} or throws StaticResourceHolder only at runtime, or when switching a theme crashes even though the dictionary compiled and loaded fine.

This topic supplies the animated *template body*. It deliberately does not repeat the surrounding plumbing, which already lives elsewhere: dictionary layout / `BasedOn` / the MergedDictionaries theme-swap in `managing-styles-resourcedictionary`; the CustomControl + `Generic.xaml` structure in `designing-wpf-customcontrol-architecture`; `Generic.xaml` auto-load in `configuring-wpf-themeinfo`; the VisualStateManager **name** contract (`[TemplateVisualState]` ↔ `GoToState`) in `authoring-wpf-controls` §3.4. See **Related topics** at the end.

---

## Essential (these compile clean and fail only at runtime)

Re-read this section first if context was compressed. Every rule here was verified against a 10-theme WPF sample that builds 0/0 yet crashed at runtime until each was applied (`samples/ThemeGallery`).

1. **A `Setter TargetName` must resolve to a `FrameworkElement`** in the template. Pointing it at a named Freezable (a `SolidColorBrush`/`Transform`/`DropShadowEffect`) is one way it fails to resolve and raises the build-time markup error **MC4111** *"Cannot find the Trigger target X."* Animate the Freezable with a `Storyboard` instead, or `Setter` a *fresh whole* brush onto the element's brush property.
2. **A brush whose `Color` you animate must be declared inline on the element**, not pulled from a shared `{StaticResource}`. Animating a shared brush mutates that one instance so every other consumer changes too (and a *frozen* shared brush forces WPF to clone-and-animate it every frame). An inline brush gives the animation a private, unfrozen instance.
3. **No `StaticResource` forward references inside a dictionary.** A `{StaticResource X}` used before `X` is defined compiles, then throws `StaticResourceHolder` (or yields `{DependencyProperty.UnsetValue}`) when the template is *instantiated*. Define resources above their first use, or reuse a top-level brush key.
4. **Never `Storyboard.TargetProperty="(UIElement.Children)[n]…"`.** Indexing a panel's child collection fails at runtime. Name the child (`x:Name`) and target it directly; indexing a `TransformGroup` — `(UIElement.RenderTransform).(TransformGroup.Children)[n].(…)` — is fine.
5. **Every trigger that animates *in* must animate *out*.** Pair `EnterActions` with `ExitActions` (and `StopStoryboard` any looping enter storyboard) so state never sticks when the condition clears.
6. **Neither `dotnet build` nor loading the dictionary in isolation instantiates a `ControlTemplate`** — so rules 1–4 can pass both and still crash when a template is first applied to a live control. Verify by applying templates to real controls and switching themes (see §8).

---

## 1. Two ways to drive template animation — pick by ownership

| You are… | Use | Why |
|----------|-----|-----|
| **Restyling a stock/3rd-party control** (plain `Button`, `ListBoxItem`, `Slider`…) whose template you don't own | `ControlTemplate.Triggers` with `Storyboard` `EnterActions`/`ExitActions` (§2) | No C# class to call `GoToState` from; the state is a bindable property (`IsMouseOver`, `IsPressed`, `IsChecked`, `IsEnabled`, `IsExpanded`). |
| **Authoring your own `CustomControl`** with a Parts & States contract | `VisualStateManager` (`VisualStateGroups` + `VisualTransition` + per-state `Storyboard`) (§3) | Your control already calls `VisualStateManager.GoToState`; states are mutually exclusive within a group; transitions are named; state can be computed in C#. |

Rule of thumb: **restyling someone else's control → triggers; authoring your own → VSM.** Do not drive the same visual on the same control from both. The `Storyboard` body and the **animation-safety rules (§5)** are identical regardless of which mechanism hosts it.

---

## 2. ControlTemplate.Triggers + Storyboards (restyling stock controls)

Hover/press with a matching exit (the enter loop is explicitly stopped, and every animated property is returned, so nothing sticks):

```xml
<Style TargetType="{x:Type Button}">
  <Setter Property="Template"><Setter.Value>
    <ControlTemplate TargetType="{x:Type Button}">
      <Grid RenderTransformOrigin="0.5,0.5">
        <Grid.RenderTransform><ScaleTransform x:Name="SunScale" /></Grid.RenderTransform>
        <Border x:Name="Core" CornerRadius="16">
          <Border.Effect>
            <DropShadowEffect x:Name="Corona" BlurRadius="14" Opacity="0.35" ShadowDepth="0" Color="#FFC94D" />
          </Border.Effect>
          <ContentPresenter Margin="{TemplateBinding Padding}" HorizontalAlignment="Center" VerticalAlignment="Center" />
        </Border>
      </Grid>
      <ControlTemplate.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
          <Trigger.EnterActions><BeginStoryboard x:Name="HoverBSB"><Storyboard>
            <DoubleAnimation Storyboard.TargetName="Corona" Storyboard.TargetProperty="(DropShadowEffect.BlurRadius)" To="28" Duration="0:0:0.25" />
          </Storyboard></BeginStoryboard></Trigger.EnterActions>
          <Trigger.ExitActions>
            <StopStoryboard BeginStoryboardName="HoverBSB" />
            <BeginStoryboard><Storyboard>
              <DoubleAnimation Storyboard.TargetName="Corona" Storyboard.TargetProperty="(DropShadowEffect.BlurRadius)" To="14" Duration="0:0:0.3" />
            </Storyboard></BeginStoryboard>
          </Trigger.ExitActions>
        </Trigger>
        <Trigger Property="IsPressed" Value="True">
          <Trigger.EnterActions><BeginStoryboard><Storyboard>
            <DoubleAnimation Storyboard.TargetName="SunScale" Storyboard.TargetProperty="ScaleX" To="0.93" Duration="0:0:0.12">
              <DoubleAnimation.EasingFunction><CubicEase EasingMode="EaseOut" /></DoubleAnimation.EasingFunction>
            </DoubleAnimation>
          </Storyboard></BeginStoryboard></Trigger.EnterActions>
          <Trigger.ExitActions><BeginStoryboard><Storyboard>
            <DoubleAnimation Storyboard.TargetName="SunScale" Storyboard.TargetProperty="ScaleX" To="1" Duration="0:0:0.18">
              <DoubleAnimation.EasingFunction><BackEase Amplitude="0.4" EasingMode="EaseOut" /></DoubleAnimation.EasingFunction>
            </DoubleAnimation>
          </Storyboard></BeginStoryboard></Trigger.ExitActions>
        </Trigger>
        <Trigger Property="IsEnabled" Value="False">
          <Setter TargetName="Core" Property="Opacity" Value="0.4" />
        </Trigger>
      </ControlTemplate.Triggers>
    </ControlTemplate>
  </Setter.Value></Setter>
</Style>
```

A check/select transition (`IsChecked`) typically scales a hidden glyph in with a `BackEase` overshoot, then starts a delayed (`BeginTime`) looping motion; the exit `StopStoryboard`s the loop and scales the glyph back to 0. Toggle/select states (`IsChecked`, `IsSelected`, `IsHighlighted`, `IsDropDownOpen`, `IsExpanded`) follow the same enter/exit shape.

**Ambient / signature loop** — a self-starting perpetual animation with no user interaction. Begin it from an `EventTrigger` on `Loaded` and loop `Forever` on a small decorative element:

```xml
<ControlTemplate.Triggers>
  <EventTrigger RoutedEvent="Loaded">
    <BeginStoryboard><Storyboard>
      <DoubleAnimation AutoReverse="True" RepeatBehavior="Forever"
        Storyboard.TargetName="NodeGlow" Storyboard.TargetProperty="(DropShadowEffect.BlurRadius)"
        To="18" Duration="0:0:1.1">
        <DoubleAnimation.EasingFunction><SineEase EasingMode="EaseInOut" /></DoubleAnimation.EasingFunction>
      </DoubleAnimation>
    </Storyboard></BeginStoryboard>
  </EventTrigger>
</ControlTemplate.Triggers>
```

Use `RoutedEvent="FrameworkElement.Loaded"` when the templated element type does not surface `Loaded` directly. A perpetual rotation is `DoubleAnimation … TargetProperty="Angle" From="0" To="360" RepeatBehavior="Forever"` against a named `RotateTransform`.

---

## 3. VisualStateManager states + Storyboards (your own custom controls)

`authoring-wpf-controls` §3.4 owns the **naming contract** — the XAML `x:Name` literals here MUST match the C# `[TemplateVisualState]`/`GoToState` constants or the transition is a silent no-op. This section supplies what that topic leaves empty: the actual `<Storyboard>` bodies and the **transition** definitions.

```xml
<ControlTemplate TargetType="{x:Type local:MyControl}">
  <Border x:Name="PART_Root" Background="{TemplateBinding Background}">
    <VisualStateManager.VisualStateGroups>
      <VisualStateGroup x:Name="CommonStates">
        <!-- Auto-interpolates any state change that has no explicit Storyboard, and
             eases the generated tween. Per-state Storyboards below override it. -->
        <VisualStateGroup.Transitions>
          <VisualTransition GeneratedDuration="0:0:0.15">
            <VisualTransition.GeneratedEasingFunction>
              <CubicEase EasingMode="EaseOut" />
            </VisualTransition.GeneratedEasingFunction>
          </VisualTransition>
        </VisualStateGroup.Transitions>

        <VisualState x:Name="Normal" />
        <VisualState x:Name="MouseOver">
          <Storyboard>
            <DoubleAnimation Storyboard.TargetName="HoverOverlay" Storyboard.TargetProperty="Opacity" To="1" Duration="0:0:0.15" />
          </Storyboard>
        </VisualState>
        <VisualState x:Name="Pressed">
          <Storyboard>
            <DoubleAnimation Storyboard.TargetName="PressOverlay" Storyboard.TargetProperty="Opacity" To="1" Duration="0:0:0.1" />
          </Storyboard>
        </VisualState>
        <VisualState x:Name="Disabled">
          <Storyboard>
            <DoubleAnimation Storyboard.TargetName="PART_Root" Storyboard.TargetProperty="Opacity" To="0.5" Duration="0:0:0.1" />
          </Storyboard>
        </VisualState>
      </VisualStateGroup>
    </VisualStateManager.VisualStateGroups>

    <Grid>
      <ContentPresenter />
      <!-- State overlays: hit-transparent, start invisible, animate Opacity only. -->
      <Border x:Name="HoverOverlay" Background="#1FFFFFFF" Opacity="0" IsHitTestVisible="False" />
      <Border x:Name="PressOverlay" Background="#33000000" Opacity="0" IsHitTestVisible="False" />
    </Grid>
  </Border>
</ControlTemplate>
```

Why overlay `Opacity` and not `(Border.Background).(SolidColorBrush.Color)`: a custom control's `Background` is usually a shared, frozen, or `DynamicResource` brush — animating its color directly would mutate the shared instance, clone-and-animate a frozen one each frame, or (if the indirect path's real brush type doesn't match — e.g. a `LinearGradientBrush`, or an unresolved `DynamicResource`) silently no-op. Animating a dedicated overlay's `Opacity` sidesteps all of it. `VisualStateManager.GoToState(this, VisualStates.MouseOver, useTransitions: true)` plays the matching `Storyboard`; passing `false` snaps without animating (use on first `OnApplyTemplate`). Unlike `ControlTemplate.Triggers` (Essential §5), a VSM state needs **no** explicit animate-out — leaving the state auto-reverts its `Storyboard` — so the enter/exit-pairing rule applies to triggers, not to VSM states.

---

## 4. Required named template parts per stock control

When you re-template a stock control you must reproduce WPF's expected named parts, or its layout/behavior silently breaks (a `Slider` with no `PART_Track` won't move; a `TextBox` with no `PART_ContentHost` shows no text). Catalog (verified against working templates):

| Control | Mandatory parts inside the `ControlTemplate` |
|---------|----------------------------------------------|
| `TextBox` / `PasswordBox` | `ScrollViewer x:Name="PART_ContentHost"` (the editable host) |
| `ProgressBar` | root `x:Name="PART_Track"` + fill `x:Name="PART_Indicator"` (WPF sizes the indicator for determinate values); put the indeterminate loop in a *separate* element toggled by the `IsIndeterminate` trigger |
| `Slider` | `Track x:Name="PART_Track"` containing `Track.DecreaseRepeatButton`, `Track.IncreaseRepeatButton`, `Track.Thumb` |
| `ScrollBar` | same `Track x:Name="PART_Track"` structure, templated for **both** orientations (vertical `IsDirectionReversed="True"`, horizontal `False`); swap templates via a `Style.Trigger` on `Orientation` |
| `ComboBox` | `Popup x:Name="PART_Popup"` (`IsOpen="{TemplateBinding IsDropDownOpen}"`); a dropdown `ToggleButton` `IsChecked="{Binding IsDropDownOpen, Mode=TwoWay, RelativeSource={RelativeSource TemplatedParent}}"`; an `ItemsPresenter` (usually in a `ScrollViewer`) |
| `TabControl` | `TabPanel IsItemsHost="True"` for headers + `ContentPresenter ContentSource="SelectedContent"` for the body |
| `Expander` | header `ToggleButton` `IsChecked="{Binding IsExpanded, Mode=TwoWay, RelativeSource={RelativeSource TemplatedParent}}"`; collapse the content via an `IsExpanded` trigger |

Track/scrollbar `RepeatButton`s use a transparent template with `OverridesDefaultStyle="True"`, `Focusable="False"`, `IsTabStop="False"`. Factor the `Thumb` and `RepeatButton` into keyed helper styles and reference them from both `Slider` and `ScrollBar`.

```xml
<!-- Keyed helpers, defined ABOVE the Slider/ScrollBar styles that use them (rule §Essential.3) -->
<Style x:Key="TrackRepeat" TargetType="{x:Type RepeatButton}">
  <Setter Property="OverridesDefaultStyle" Value="True" />
  <Setter Property="Focusable" Value="False" />
  <Setter Property="IsTabStop" Value="False" />
  <Setter Property="Template"><Setter.Value>
    <ControlTemplate TargetType="{x:Type RepeatButton}"><Border Background="Transparent" /></ControlTemplate>
  </Setter.Value></Setter>
</Style>

<ControlTemplate TargetType="{x:Type Slider}">
  <Grid VerticalAlignment="Center" Background="Transparent">
    <Rectangle Height="2" Fill="#33314F" RadiusX="1" RadiusY="1" />
    <Track x:Name="PART_Track">
      <Track.DecreaseRepeatButton><RepeatButton Command="{x:Static Slider.DecreaseLarge}" Style="{StaticResource TrackRepeat}" /></Track.DecreaseRepeatButton>
      <Track.IncreaseRepeatButton><RepeatButton Command="{x:Static Slider.IncreaseLarge}" Style="{StaticResource TrackRepeat}" /></Track.IncreaseRepeatButton>
      <Track.Thumb><Thumb Style="{StaticResource MyThumbStyle}" /></Track.Thumb>
    </Track>
  </Grid>
</ControlTemplate>
```

A `ProgressBar` indeterminate loop (note `PART_Indicator` is hidden, a separate element animates):

```xml
<ControlTemplate TargetType="{x:Type ProgressBar}">
  <Border x:Name="PART_Track" ClipToBounds="True" CornerRadius="9">
    <Grid>
      <Border x:Name="PART_Indicator" HorizontalAlignment="Left" CornerRadius="9" />
      <Grid x:Name="IndetGrid" Visibility="Collapsed">
        <!-- give the comet a Background (gradient/solid) or it animates invisibly -->
        <Border x:Name="Comet" Width="70" HorizontalAlignment="Left" Background="#CCFFC94D" RenderTransformOrigin="0,0">
          <Border.RenderTransform><TranslateTransform x:Name="CometMove" X="-70" /></Border.RenderTransform>
        </Border>
      </Grid>
    </Grid>
  </Border>
  <ControlTemplate.Triggers>
    <Trigger Property="IsIndeterminate" Value="True">
      <Setter TargetName="PART_Indicator" Property="Visibility" Value="Collapsed" />
      <Setter TargetName="IndetGrid" Property="Visibility" Value="Visible" />
      <Trigger.EnterActions><BeginStoryboard><Storyboard>
        <DoubleAnimation RepeatBehavior="Forever" Storyboard.TargetName="CometMove" Storyboard.TargetProperty="X" From="-70" To="360" Duration="0:0:1.4" />
      </Storyboard></BeginStoryboard></Trigger.EnterActions>
    </Trigger>
  </ControlTemplate.Triggers>
</ControlTemplate>
```

---

## 5. Animation-safety rules (apply to triggers and VSM alike)

- **Animate only cheap, composited properties on small elements**: `Opacity`, `RenderTransform` sub-properties (`ScaleX/Y`, `Angle`, `X/Y`), brush `Color`/`Offset`/gradient stops, and `Effect` `Opacity`/`BlurRadius`. **Never animate `Width`/`Height`/`Margin` of containers** — it re-runs layout every frame and fights the layout system (see `containing-control-decorative-overflow`; for cheap looping transforms cache with `BitmapCache`, see `rendering-wpf-high-performance`).
- **Animated brushes are inline** (Essential §2). Brush color paths are parenthesized: `(Border.Background).(SolidColorBrush.Color)`, `(Border.BorderBrush).(SolidColorBrush.Color)`.
- **`DropShadowEffect` glow**: keep `BlurRadius` ≤ ~25, on small elements, and don't stack effects.
- **Precedence traps** (same value-precedence ladder as `styling-chat-bubbles-in-wpf`): a local/inline property value outranks a `Style` trigger `Setter` (the trigger silently loses — set trigger-controlled properties via `Setter`, not inline), and a running animation outranks almost everything, so a held animation (`FillBehavior="HoldEnd"`) pins a property and a later `DynamicResource` theme swap appears to do nothing (§7).
- **Disabled + focus**: every templated control handles `IsEnabled="False"` visually (reduced opacity is fine) and keeps a visible keyboard focus cue (`IsKeyboardFocused` trigger or a `FocusVisualStyle`).
- **The one place `Freeze()` does NOT apply**: a brush you intend to animate must stay unfrozen. This is the deliberate exception to the "freeze every `Brush`/`Pen`/`Geometry`" performance guidance.

---

## 6. Pitfalls that compile clean but fail only at runtime

Each was hit and fixed in a sample that built with 0 warnings/0 errors.

1. **`Setter TargetName` on a Freezable → MC4111.** `<Setter TargetName="MyBrush" Property="Color" …>` where `MyBrush` is a named `SolidColorBrush` fails: *"Cannot find the Trigger target 'MyBrush'."* `Setter.TargetName` only resolves `FrameworkElement`s. **Fix**: animate the color with a `Storyboard` (`ColorAnimation` on `(Border.BorderBrush).(SolidColorBrush.Color)`), or `Setter` a *fresh whole brush* onto the element: `<Setter TargetName="Bd" Property="BorderBrush"><Setter.Value><SolidColorBrush Color="#55538C"/></Setter.Value></Setter>`.
2. **Animating a brush from a shared `{StaticResource}` mutates that one instance** (every other consumer changes too), or clones-and-animates it each frame if it is frozen. Declare the brush inline on the element so the animation owns a private, unfrozen instance. A *silent* no-op is a distinct case: an indirect color path whose actual brush is the wrong type — e.g. animating `(Background).(SolidColorBrush.Color)` when `Background` is a `LinearGradientBrush`.
3. **`StaticResource` forward reference → `StaticResourceHolder` / `UnsetValue` at instantiation.** Compiles fine; throws when the *template* is realized — e.g. `XamlParseException` providing a value for `StaticResourceHolder`, or *"'{DependencyProperty.UnsetValue}' is not a valid value for property 'Foreground'."* Worst case it only appears when one specific control is rendered or when switching to that theme. **Fix**: order the dictionary top-down (palette `Color`s → shared brushes → keyed helper styles → control styles); or hoist the shared value to a top-level brush key (e.g. reuse the theme's `ThemeCardBrush`/`ThemeSubtleTextBrush`) and reference that. A reference *inside a `ControlTemplate`* is the most dangerous because it resolves latest.
4. **`Storyboard.TargetProperty="(UIElement.Children)[0]…"` → runtime path-resolution failure.** Indexing a panel's child collection in a property path throws when the storyboard runs. **Fix**: give the child an `x:Name` and target it directly (`Storyboard.TargetName="ThatChild" TargetProperty="(UIElement.RenderTransform).(TranslateTransform.X)"`). Indexing a `TransformGroup` — `(UIElement.RenderTransform).(TransformGroup.Children)[1].(RotateTransform.Angle)` — IS supported and is the correct way to animate one transform inside a group.

---

## 7. Runtime theme switching

`managing-styles-resourcedictionary` §4.3 owns the swap mechanism — remove the previous theme `ResourceDictionary` and add the new one to `Application.Current.Resources.MergedDictionaries`. Keep a P-002-clean abstraction (`IThemeSwitcher`) so a WPF-free ViewModel can request a switch:

```csharp
public void Apply(string themeName)
{
    var dict = new ResourceDictionary
    {
        Source = new Uri($"pack://application:,,,/MyApp.Themes;component/{themeName}.xaml", UriKind.Absolute),
    };
    var merged = Application.Current.Resources.MergedDictionaries;
    if (_current is not null) { merged.Remove(_current); }
    merged.Add(dict);
    _current = dict;
}
```

For the swap to take visible effect:

- **Consume theme brushes via `{DynamicResource}`, never `{StaticResource}`.** `StaticResource` resolves once at load and freezes the first theme; `DynamicResource` re-resolves live when the dictionary is swapped. This applies to shell chrome (`Background="{DynamicResource ThemeWindowBackgroundBrush}"`) and to any brush a template references that should follow the theme.
- **Implicit (keyless, `TargetType`-only) styles re-apply automatically** on swap — the showcased controls need no explicit bindings.
- **Define the full shell-brush surface in every theme** so chrome can bind it uniformly. A practical six-key contract: `ThemeWindowBackgroundBrush`, `ThemeTextBrush`, `ThemeSubtleTextBrush`, `ThemeAccentBrush`, `ThemeCardBrush`, `ThemeBorderBrush`.
- **Animated cross-fade (optional)**: instead of an instant swap, animate a `ColorAnimation` between palette brushes, or fade an overlay; but a held animation pins its property (§5 precedence) so a subsequent `DynamicResource` swap of the same property appears to do nothing — clear running storyboards before/with the swap.

---

## 8. Verify at runtime — compile and isolated load are not enough

The hard-won lesson behind §6: a theme/template can pass **both** of these and still crash a user:

- `dotnet build` (BAML compile) — catches malformed XAML, unknown members, and `Storyboard` target errors like MC4111, but does **not** resolve `StaticResource` keys across a dictionary or instantiate templates.
- Loading the `ResourceDictionary` in isolation (`new ResourceDictionary { Source = packUri }` and realizing `rd[typeof(Button)]`) — exercises the `Style` objects but still does **not** instantiate the `ControlTemplate`s, so pitfalls §6.1–§6.4 stay hidden.

The errors surface only when a **template is applied to a live control** and (for theme work) when **switching** themes. Add a throwaway STA harness that builds a `Window` containing one of every templated control, then cycles each theme through the real switch path, forcing a layout pass and pumping the dispatcher so trigger/ambient storyboards actually run; capture both synchronous and `Application.DispatcherUnhandledException` errors:

```csharp
var sta = new Thread(() =>
{
    var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
    app.DispatcherUnhandledException += (_, e) => { Log($"{current}: {e.Exception.Message}"); e.Handled = true; };
    var panel = new StackPanel();
    foreach (var c in new Control[] { new Button{Content="x"}, new CheckBox{IsChecked=true},
        new ComboBox(), new Slider{Value=40}, new ProgressBar{IsIndeterminate=true}, /* …all 15… */ })
        panel.Children.Add(c);
    var w = new Window { Left = -2000, Content = new ScrollViewer { Content = panel } };
    Apply(themes[0]); w.Show();                 // initial theme on a live tree
    foreach (var t in themes.Skip(1)) { current = t; Apply(t); w.UpdateLayout(); }  // switch each
    w.Close(); app.Shutdown();
});
sta.SetApartmentState(ApartmentState.STA); sta.Start(); sta.Join();
```

Green means every template instantiated and every theme switched without a `StaticResourceHolder` / `UnsetValue` / MC4111 / path-resolution failure. For full UI-automation of an animated themed app (and why discovery-by-`AutomationId` survives restyling), see `setting-up-flaui-ui-tests`.

---

## References

- [Storyboards Overview — Microsoft Learn](https://learn.microsoft.com/dotnet/desktop/wpf/graphics-multimedia/storyboards-overview)
- [Animation Overview — Microsoft Learn](https://learn.microsoft.com/dotnet/desktop/wpf/graphics-multimedia/animation-overview)
- [VisualStateManager — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.visualstatemanager)
- [Control templates — Microsoft Learn](https://learn.microsoft.com/dotnet/desktop/wpf/controls/control-styles-and-templates)

### Related topics

- [`managing-styles-resourcedictionary`](../managing-styles-resourcedictionary/TOPIC.md) — dictionary layout, `BasedOn`, `StaticResource` vs `DynamicResource`, and the canonical MergedDictionaries theme-swap (§4.3) this topic animates.
- [`designing-wpf-customcontrol-architecture`](../designing-wpf-customcontrol-architecture/TOPIC.md) — where a custom control's template lives (`Generic.xaml` hub, per-control file); this topic supplies its animated body.
- [`authoring-wpf-controls`](../authoring-wpf-controls/TOPIC.md) — the `PART_` and VisualStateManager **naming** contracts (§3.2, §3.4) whose states this topic fills with Storyboards.
- [`configuring-wpf-themeinfo`](../configuring-wpf-themeinfo/TOPIC.md) — `ThemeInfo`/`Generic.xaml` auto-load plumbing an animated custom-control template still ships through.
- [`styling-chat-bubbles-in-wpf`](../styling-chat-bubbles-in-wpf/TOPIC.md) — the dependency-property value-precedence rule (local value beats a Style trigger Setter) that also explains why a held animation defeats a later theme swap.
- [`containing-control-decorative-overflow`](../containing-control-decorative-overflow/TOPIC.md) — where to put an animated hover-glow/focus-ring so it isn't clipped, and why to animate `RenderTransform`/`Opacity` not `Margin`.
- [`rendering-wpf-high-performance`](../rendering-wpf-high-performance/TOPIC.md) — `BitmapCache` for cheap looping transform animations and the layout-vs-render-property cost rule.
- [`setting-up-flaui-ui-tests`](../setting-up-flaui-ui-tests/TOPIC.md) — UI-automating an animated themed app; AutomationId discovery survives restyling.
