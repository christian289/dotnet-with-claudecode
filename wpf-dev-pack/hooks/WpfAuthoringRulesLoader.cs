#!/usr/bin/env dotnet

// WPF Authoring Rules Loader Hook (SessionStart)
//
// At the start of every Claude Code session where the wpf-dev-pack plugin is
// installed, this hook injects a compact, ENFORCED rule set for authoring WPF
// ControlTemplates, Styles, and animations into the session context. Plugin
// bundled rule files are NOT auto-loaded for installed users (plugins deliver
// context only through skills, agents, and hooks), so these always-on rules
// are shipped as a SessionStart hook.
//
// The rules are general WPF control-template / styling / animation authoring
// constraints (not theme-specific). Full guidance + examples live in the
// WpfDevPackMcp knowledge topic "animating-wpf-controltemplates"; the
// samples/ThemeGallery project is the reference implementation.
//
// Input:  stdin JSON (SessionStart payload). Consumed but unused.
// Output: a system-context rule block on stdout.

// Consume stdin for protocol compatibility, even though we don't read it.
_ = Console.In.ReadToEnd();

Console.Write(
    """
    [wpf-dev-pack] WPF ControlTemplate / Style / animation authoring rules — ENFORCED this session.

    Apply these whenever you write or restyle a WPF ControlTemplate, Style, or animation (any control, themed or not). They compile clean and fail only at runtime, so follow them up front:

    1. Required named parts — when re-templating a stock control, reproduce WPF's expected parts or its layout/behavior silently breaks:
       - TextBox/PasswordBox: ScrollViewer x:Name="PART_ContentHost"
       - ProgressBar: root x:Name="PART_Track" + fill x:Name="PART_Indicator" (put the indeterminate loop in a separate element toggled by the IsIndeterminate trigger)
       - Slider/ScrollBar: Track x:Name="PART_Track" with Track.DecreaseRepeatButton, Track.IncreaseRepeatButton, Track.Thumb (template ScrollBar for both orientations)
       - ComboBox: Popup x:Name="PART_Popup" (IsOpen TemplateBound) + a dropdown ToggleButton IsChecked TwoWay-bound to IsDropDownOpen + ItemsPresenter
       - Expander: header ToggleButton IsChecked TwoWay-bound to IsExpanded
       - TabControl: TabPanel IsItemsHost="True" + ContentPresenter ContentSource="SelectedContent"
    2. Animation safety — animate ONLY Opacity, RenderTransform sub-properties, brush Color/stops, and Effect Opacity/BlurRadius, on small elements; never a container's Width/Height/Margin. A brush whose Color you animate must be declared INLINE on the element (not a shared {StaticResource}). Brush color paths are parenthesized: (Border.Background).(SolidColorBrush.Color).
    3. A Setter TargetName must resolve to a FrameworkElement — never a named Freezable (brush/transform/effect). Targeting a Freezable raises build error MC4111. Animate it with a Storyboard, or Setter a fresh whole brush onto the element.
    4. No StaticResource forward references in a ResourceDictionary — a {StaticResource X} used before X is defined compiles, then throws StaticResourceHolder (or yields {DependencyProperty.UnsetValue}) when the template is instantiated. Define resources above first use, or reuse a top-level brush.
    5. Storyboard.TargetProperty — never index a panel's children: (UIElement.Children)[n] fails at runtime. Name the child and target it directly; indexing a TransformGroup, (UIElement.RenderTransform).(TransformGroup.Children)[n], is valid.
    6. Pair every trigger's EnterActions with ExitActions (StopStoryboard any looping enter) so state never sticks; handle IsEnabled=False visually and keep a keyboard focus cue. (VisualStateManager states auto-revert, so this enter/exit pairing applies to ControlTemplate.Triggers, not VSM.)
    7. Verify at runtime — neither `dotnet build` nor loading a ResourceDictionary in isolation instantiates a ControlTemplate, so rules 1–5 can pass both and still crash when a template is applied to a live control. Apply templates to real controls (and switch themes) to surface failures.

    Full guidance + examples: WpfDevPackMcp get_wpf_topic("animating-wpf-controltemplates"). Reference sample: samples/ThemeGallery.

    """);
