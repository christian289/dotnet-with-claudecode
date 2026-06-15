---
description: "Generates WPF CustomControl C# class and XAML ControlTemplate style from a control name. Use when creating a new custom control, scaffolding a templated control, or generating CustomControl boilerplate code. Usage: /wpf-dev-pack:make-wpf-custom-control <ControlName>"
argument-hint: [ControlName]
---

# WPF CustomControl Generation

**If `$0` is empty, use the AskUserQuestion tool to ask: "Enter the CustomControl name (e.g., CircularProgress, RangeSlider)". Do NOT proceed until a valid name is provided. Use the response as the ControlName for all subsequent steps.**

Generate a `$0` CustomControl.

- Replace `{BaseClass}` with the appropriate WPF base class (e.g., Control, Button, ContentControl, ItemsControl) based on the control name and context.
- Replace `{Namespace}` with the project's root namespace detected from csproj or existing code.
- If the host project follows a non-default style convention (e.g. block-scoped namespaces, custom usings), conform to it.

## Workflow

### Step 1: Validate Input

- `$0` must be PascalCase
- Determine the best BaseClass based on `$0` name and intended usage

### Step 2: Generate C# Class File

Create `$0.cs`:

```csharp
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace {Namespace}.Controls;

/// <summary>
/// $0 - Custom WPF control based on {BaseClass}.
/// </summary>
[TemplatePart(Name = TemplateParts.Root, Type = typeof(Border))]
[TemplateVisualState(GroupName = VisualStates.CommonStates, Name = VisualStates.Normal)]
[TemplateVisualState(GroupName = VisualStates.CommonStates, Name = VisualStates.MouseOver)]
[TemplateVisualState(GroupName = VisualStates.CommonStates, Name = VisualStates.Pressed)]
[TemplateVisualState(GroupName = VisualStates.CommonStates, Name = VisualStates.Disabled)]
public class $0 : {BaseClass}
{
    // Single source of truth for Template Part names.
    // XAML <Border x:Name="…"> literals MUST match these constants.
    private static class TemplateParts
    {
        public const string Root = "PART_Root";
    }

    // Single source of truth for VSM group/state names.
    // XAML <VisualStateGroup x:Name="…"> / <VisualState x:Name="…"> literals
    // MUST match these constants exactly. The compiler will not catch a mismatch
    // and runtime GoToState will return false silently.
    private static class VisualStates
    {
        public const string CommonStates = "CommonStates";
        public const string Normal       = "Normal";
        public const string MouseOver    = "MouseOver";
        public const string Pressed      = "Pressed";
        public const string Disabled     = "Disabled";
    }

    #region Constructors

    static $0()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof($0),
            new FrameworkPropertyMetadata(typeof($0)));
    }

    public $0()
    {
        // IsEnabledChanged is an EVENT — there is no OnIsEnabledChanged to
        // override (Control/UIElement exposes no such virtual). Subscribe here.
        IsEnabledChanged += (_, _) => UpdateVisualState(true);
    }

    #endregion

    #region Dependency Properties

    /// <summary>
    /// Example dependency property.
    /// </summary>
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof($0),
            new FrameworkPropertyMetadata(
                defaultValue: 0,
                flags: FrameworkPropertyMetadataOptions.AffectsRender,
                propertyChangedCallback: OnValueChanged,
                coerceValueCallback: CoerceValue));

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is $0 control)
        {
            control.OnValueChanged((int)e.OldValue, (int)e.NewValue);
        }
    }

    protected virtual void OnValueChanged(int oldValue, int newValue)
    {
        // Handle property change
    }

    // Multi-constraint coerce: relational constraints first, hard domain LAST.
    // See authoring-wpf-controls §4 "Multi-Constraint Coerce Ordering".
    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        var control = ($0)d;
        var v = (int)baseValue;

        // (Add any relational constraints that depend on other properties first.)

        // Hard domain clamp LAST so transient cross-property states cannot leak
        // a value outside the legal domain.
        v = Math.Clamp(v, 0, 100);

        return v;
    }

    #endregion

    #region Read-only Dependency Property (optional)

    // Read-only DPs expose internal state for binding while preventing external writes.
    // Replace with your real read-only property or remove this region.
    private static readonly DependencyPropertyKey IsBusyPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsBusy),
            typeof(bool),
            typeof($0),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsBusyProperty = IsBusyPropertyKey.DependencyProperty;

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        private set => SetValue(IsBusyPropertyKey, value);
    }

    #endregion

    #region Routed Event (optional)

    // Replace with your real routed event or remove this region.
    public static readonly RoutedEvent ValueChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(ValueChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<int>),
            typeof($0));

    public event RoutedPropertyChangedEventHandler<int> ValueChanged
    {
        add    => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<int> e)
        => RaiseEvent(e);

    #endregion

    #region Template Parts

    private Border? _partRoot;
    private bool _isPressed;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // OnApplyTemplate fires BEFORE Loaded. Avoid duplicating init logic in both —
        // do template-binding setup here, and only do Loaded-time work in Loaded.
        _partRoot = GetTemplateChild(TemplateParts.Root) as Border;

        // Template-Part tolerance: if PART_Root is missing, disable only the
        // feature that depends on it. Do NOT throw — see authoring-wpf-controls §3.1.
        if (_partRoot is null)
        {
            // Optional: log a designer-only warning here if you want.
        }

        UpdateVisualState(false);
    }

    #endregion

    #region Visual States

    private void UpdateVisualState(bool useTransitions)
    {
        // States declared via [TemplateVisualState] MUST be reachable here,
        // otherwise the attribute is a documentation lie and the state never fires.
        string state =
            !IsEnabled  ? VisualStates.Disabled  :
            _isPressed  ? VisualStates.Pressed   :
            IsMouseOver ? VisualStates.MouseOver :
                          VisualStates.Normal;

        VisualStateManager.GoToState(this, state, useTransitions);
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        UpdateVisualState(true);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        UpdateVisualState(true);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        _isPressed = true;
        UpdateVisualState(true);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        _isPressed = false;
        UpdateVisualState(true);
    }

    // Note: IsEnabled changes are handled via the IsEnabledChanged event
    // subscribed in the constructor (there is no OnIsEnabledChanged to override).

    #endregion
}
```

### Step 3: Generate XAML Style File

Create `Themes/$0.xaml`:

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:local="clr-namespace:{Namespace}.Controls">

    <!--
        Color/brush resources.
        Prefer host theme tokens (ThemeResource / implicit Style) for colors that
        should follow the host app's theme; use signature-only x:Key brushes
        for colors that uniquely identify THIS control's visual identity.
        Hard-coded hex values like below are placeholders.
    -->
    <SolidColorBrush x:Key="$0DefaultBackground"      Color="#FFFFFF" />
    <SolidColorBrush x:Key="$0DefaultForeground"     Color="#000000" />
    <SolidColorBrush x:Key="$0DefaultBorderBrush"    Color="#CCCCCC" />
    <SolidColorBrush x:Key="$0MouseOverOverlayBrush" Color="#1A2196F3" />
    <SolidColorBrush x:Key="$0PressedOverlayBrush"   Color="#332196F3" />

    <Style TargetType="{x:Type local:$0}">
        <Setter Property="Background"   Value="{StaticResource $0DefaultBackground}" />
        <Setter Property="Foreground"   Value="{StaticResource $0DefaultForeground}" />
        <Setter Property="BorderBrush"  Value="{StaticResource $0DefaultBorderBrush}" />
        <Setter Property="BorderThickness" Value="1" />
        <Setter Property="Padding"      Value="8,4" />
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type local:$0}">

                    <!--
                        x:Name literals MUST match the const TemplateParts/VisualStates
                        on the C# side ($0.cs). VSM is a name-based contract: a
                        mismatch is a silent runtime no-op.
                    -->
                    <Border x:Name="PART_Root"
                            Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            Padding="{TemplateBinding Padding}"
                            CornerRadius="4">

                        <VisualStateManager.VisualStateGroups>
                            <VisualStateGroup x:Name="CommonStates">
                                <VisualState x:Name="Normal" />

                                <!--
                                    Animate Opacity of a dedicated overlay layer instead of
                                    (Border.Background).(SolidColorBrush.Color). The latter
                                    explodes silently if Background is a shared/frozen
                                    brush (e.g. from a theme dictionary) or a DynamicResource.
                                -->
                                <VisualState x:Name="MouseOver">
                                    <Storyboard>
                                        <DoubleAnimation
                                            Storyboard.TargetName="PART_MouseOverOverlay"
                                            Storyboard.TargetProperty="Opacity"
                                            To="1.0"
                                            Duration="0:0:0.15" />
                                    </Storyboard>
                                </VisualState>

                                <VisualState x:Name="Pressed">
                                    <Storyboard>
                                        <DoubleAnimation
                                            Storyboard.TargetName="PART_PressedOverlay"
                                            Storyboard.TargetProperty="Opacity"
                                            To="1.0"
                                            Duration="0:0:0.10" />
                                    </Storyboard>
                                </VisualState>

                                <VisualState x:Name="Disabled">
                                    <Storyboard>
                                        <DoubleAnimation
                                            Storyboard.TargetName="PART_Root"
                                            Storyboard.TargetProperty="Opacity"
                                            To="0.5"
                                            Duration="0:0:0" />
                                    </Storyboard>
                                </VisualState>
                            </VisualStateGroup>
                        </VisualStateManager.VisualStateGroups>

                        <Grid>
                            <!-- MouseOver / Pressed overlays: each is an opaque-controllable
                                 layer that animates Opacity, not the underlying brush color. -->
                            <Border x:Name="PART_MouseOverOverlay"
                                    Background="{StaticResource $0MouseOverOverlayBrush}"
                                    CornerRadius="4"
                                    Opacity="0"
                                    IsHitTestVisible="False" />
                            <Border x:Name="PART_PressedOverlay"
                                    Background="{StaticResource $0PressedOverlayBrush}"
                                    CornerRadius="4"
                                    Opacity="0"
                                    IsHitTestVisible="False" />

                            <ContentPresenter
                                HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                                VerticalAlignment="{TemplateBinding VerticalContentAlignment}" />
                        </Grid>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>
```

### Step 4: Ensure a CustomControl home, then register the theme

CustomControls require a `Themes/Generic.xaml` **and** the `ThemeInfo` assembly
attribute. Prefer a dedicated `.UI` CustomControl library; if the solution has
none, target the WPF app project and create the infrastructure there.

**4a. If `Themes/Generic.xaml` does not exist** in the target project, create it
as a hub-only dictionary (see `resourcedictionary-patterns.md` — hub only, no
inline styles):

```xml
<!-- Themes/Generic.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="/Themes/$0.xaml" />
    </ResourceDictionary.MergedDictionaries>
</ResourceDictionary>
```

**4b. If the `ThemeInfo` attribute is missing**, add it once (e.g. in
`Properties/AssemblyInfo.cs` or any `.cs`) so WPF locates `Generic.xaml`:

```csharp
using System.Windows;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,
    ResourceDictionaryLocation.SourceAssembly)]
```

**4c. If `Generic.xaml` already exists**, just add this control's dictionary to
its `MergedDictionaries`:

```xml
<ResourceDictionary.MergedDictionaries>
    <!-- Existing entries -->
    <ResourceDictionary Source="/Themes/$0.xaml" />
</ResourceDictionary.MergedDictionaries>
```

## Output Summary

After generation, provide:
1. Created files list
2. Next steps for customization (which DPs/events to keep, which to remove)
3. Usage example in XAML

```xml
<!-- Usage Example -->
<local:$0 Value="42" />
```

## Related knowledge topics (via WpfDevPackMcp)

Fetch with `WpfDevPackMcp get_wpf_topic`:

- `authoring-wpf-controls` — §3.4 Visual State Naming Contract, §4 Multi-Constraint Coerce Ordering, §3.1 Template-Part tolerance
- `animating-wpf-controltemplates` — the `<Storyboard>` bodies + `VisualStateGroup.Transitions` that fill the VisualStates above; ambient loops; the Setter-on-Freezable (MC4111), inline-animated-brush, and StaticResource-forward-reference pitfalls; verify-at-runtime harness
- `containing-control-decorative-overflow` — when focus ring / hover glow gets clipped at an ancestor boundary
- `managing-styles-resourcedictionary` — Generic.xaml hub pattern
- `managing-literal-strings` — consolidating literal strings (VSM names are a notable WPF-specific case)
