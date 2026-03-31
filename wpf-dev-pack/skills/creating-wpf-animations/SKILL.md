---
description: "Creates WPF animations using Storyboard, Timeline, and EasingFunction patterns. Use when implementing UI transitions, state change visualizations, or interactive feedback effects."
---

# WPF Animation Patterns

WPF animations create visual effects by changing property values over time.

## 1. Animation Components

```
Storyboard (container)
├── Timeline (time control)
│   ├── Animation (value change)
│   │   ├── DoubleAnimation
│   │   ├── ColorAnimation
│   │   └── ...
│   └── AnimationUsingKeyFrames (keyframes)
│       ├── DoubleAnimationUsingKeyFrames
│       └── ...
└── EasingFunction (acceleration/deceleration)
```

---

## 2. Basic Animation (XAML)

### 2.1 DoubleAnimation

```xml
<Button Content="Hover Me" Width="100">
    <Button.Style>
        <Style TargetType="Button">
            <Style.Triggers>
                <Trigger Property="IsMouseOver" Value="True">
                    <Trigger.EnterActions>
                        <BeginStoryboard>
                            <Storyboard>
                                <!-- Width animation -->
                                <DoubleAnimation
                                    Storyboard.TargetProperty="Width"
                                    To="150"
                                    Duration="0:0:0.3"/>
                            </Storyboard>
                        </BeginStoryboard>
                    </Trigger.EnterActions>
                    <Trigger.ExitActions>
                        <BeginStoryboard>
                            <Storyboard>
                                <DoubleAnimation
                                    Storyboard.TargetProperty="Width"
                                    To="100"
                                    Duration="0:0:0.3"/>
                            </Storyboard>
                        </BeginStoryboard>
                    </Trigger.ExitActions>
                </Trigger>
            </Style.Triggers>
        </Style>
    </Button.Style>
</Button>
```

### 2.2 ColorAnimation

```xml
<Border x:Name="AnimatedBorder" Background="Blue" Width="100" Height="100">
    <Border.Triggers>
        <EventTrigger RoutedEvent="MouseEnter">
            <BeginStoryboard>
                <Storyboard>
                    <!-- Background color animation -->
                    <ColorAnimation
                        Storyboard.TargetProperty="(Border.Background).(SolidColorBrush.Color)"
                        To="Red"
                        Duration="0:0:0.5"/>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
        <EventTrigger RoutedEvent="MouseLeave">
            <BeginStoryboard>
                <Storyboard>
                    <ColorAnimation
                        Storyboard.TargetProperty="(Border.Background).(SolidColorBrush.Color)"
                        To="Blue"
                        Duration="0:0:0.5"/>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Border.Triggers>
</Border>
```

### 2.3 ThicknessAnimation (Margin, Padding)

```xml
<Border x:Name="SlidingBorder" Margin="0,0,0,0">
    <Border.Triggers>
        <EventTrigger RoutedEvent="Loaded">
            <BeginStoryboard>
                <Storyboard>
                    <!-- Slide-in effect -->
                    <ThicknessAnimation
                        Storyboard.TargetProperty="Margin"
                        From="-100,0,0,0"
                        To="0,0,0,0"
                        Duration="0:0:0.5"/>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Border.Triggers>
</Border>
```

---

## 3. EasingFunction

### 3.1 Main Easing Types

```xml
<Storyboard>
    <DoubleAnimation Storyboard.TargetProperty="Width" To="200" Duration="0:0:0.5">
        <DoubleAnimation.EasingFunction>
            <!-- Various easing functions -->

            <!-- Smooth deceleration -->
            <QuadraticEase EasingMode="EaseOut"/>

            <!-- Elastic effect -->
            <!--<ElasticEase Oscillations="3" Springiness="5"/>-->

            <!-- Bounce effect -->
            <!--<BounceEase Bounces="3" Bounciness="2"/>-->

            <!-- Back effect (overshoot) -->
            <!--<BackEase Amplitude="0.3"/>-->

            <!-- Circular easing -->
            <!--<CircleEase EasingMode="EaseInOut"/>-->
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
</Storyboard>
```

### 3.2 EasingMode

| Mode | Description |
|------|-------------|
| **EaseIn** | Slow at start, fast at end |
| **EaseOut** | Fast at start, slow at end |
| **EaseInOut** | Slow at both ends, fast in middle |

---

## 4. KeyFrame Animation

### 4.1 DoubleAnimationUsingKeyFrames

```xml
<Storyboard>
    <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty="Opacity">
        <!-- Sequential keyframes -->
        <LinearDoubleKeyFrame Value="0.3" KeyTime="0:0:0.2"/>
        <LinearDoubleKeyFrame Value="1.0" KeyTime="0:0:0.4"/>
        <SplineDoubleKeyFrame Value="0.5" KeyTime="0:0:0.8">
            <SplineDoubleKeyFrame.KeySpline>
                <KeySpline ControlPoint1="0.5,0" ControlPoint2="0.5,1"/>
            </SplineDoubleKeyFrame.KeySpline>
        </SplineDoubleKeyFrame>
        <EasingDoubleKeyFrame Value="1.0" KeyTime="0:0:1">
            <EasingDoubleKeyFrame.EasingFunction>
                <BounceEase/>
            </EasingDoubleKeyFrame.EasingFunction>
        </EasingDoubleKeyFrame>
    </DoubleAnimationUsingKeyFrames>
</Storyboard>
```

### 4.2 ObjectAnimationUsingKeyFrames (Discrete Values)

```xml
<Storyboard>
    <!-- Visibility toggle (non-continuous values) -->
    <ObjectAnimationUsingKeyFrames Storyboard.TargetProperty="Visibility">
        <DiscreteObjectKeyFrame KeyTime="0:0:0" Value="{x:Static Visibility.Visible}"/>
        <DiscreteObjectKeyFrame KeyTime="0:0:1" Value="{x:Static Visibility.Hidden}"/>
        <DiscreteObjectKeyFrame KeyTime="0:0:2" Value="{x:Static Visibility.Visible}"/>
    </ObjectAnimationUsingKeyFrames>
</Storyboard>
```

---

## 5. Transform Animation

### 5.1 RenderTransform Animation

```xml
<Button Content="Rotate" RenderTransformOrigin="0.5,0.5">
    <Button.RenderTransform>
        <RotateTransform x:Name="RotateTransform" Angle="0"/>
    </Button.RenderTransform>
    <Button.Triggers>
        <EventTrigger RoutedEvent="Click">
            <BeginStoryboard>
                <Storyboard>
                    <!-- Rotation animation -->
                    <DoubleAnimation
                        Storyboard.TargetName="RotateTransform"
                        Storyboard.TargetProperty="Angle"
                        By="360"
                        Duration="0:0:0.5"/>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Button.Triggers>
</Button>
```

### 5.2 Composite Transform Animation

```xml
<Border Width="100" Height="100" Background="Blue" RenderTransformOrigin="0.5,0.5">
    <Border.RenderTransform>
        <TransformGroup>
            <ScaleTransform x:Name="Scale" ScaleX="1" ScaleY="1"/>
            <RotateTransform x:Name="Rotate" Angle="0"/>
            <TranslateTransform x:Name="Translate" X="0" Y="0"/>
        </TransformGroup>
    </Border.RenderTransform>
    <Border.Triggers>
        <EventTrigger RoutedEvent="MouseEnter">
            <BeginStoryboard>
                <Storyboard>
                    <!-- Simultaneous animations -->
                    <DoubleAnimation Storyboard.TargetName="Scale"
                                     Storyboard.TargetProperty="ScaleX"
                                     To="1.2" Duration="0:0:0.2"/>
                    <DoubleAnimation Storyboard.TargetName="Scale"
                                     Storyboard.TargetProperty="ScaleY"
                                     To="1.2" Duration="0:0:0.2"/>
                    <DoubleAnimation Storyboard.TargetName="Rotate"
                                     Storyboard.TargetProperty="Angle"
                                     To="10" Duration="0:0:0.2"/>
                </Storyboard>
            </BeginStoryboard>
        </EventTrigger>
    </Border.Triggers>
</Border>
```

---

## 6. Animation in Code

### 6.1 Basic Animation

```csharp
namespace MyApp.Animations;

using System;
using System.Windows;
using System.Windows.Media.Animation;

public static class AnimationHelper
{
    /// <summary>
    /// Opacity fade animation
    /// </summary>
    public static void FadeIn(UIElement element, double durationSeconds = 0.3)
    {
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(durationSeconds),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        element.BeginAnimation(UIElement.OpacityProperty, animation);
    }

    public static void FadeOut(UIElement element, double durationSeconds = 0.3, Action? onCompleted = null)
    {
        var animation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(durationSeconds),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        if (onCompleted is not null)
        {
            animation.Completed += (s, e) => onCompleted();
        }

        element.BeginAnimation(UIElement.OpacityProperty, animation);
    }
}
```

> **Advanced**: See [ADVANCED.md](ADVANCED.md) for composite Storyboard animation, AnimationController (stop/resume), and VisualStateManager integration.

---

## 7. Performance Considerations

| Property | Performance | Description |
|----------|-------------|-------------|
| **Opacity** | ⭐⭐⭐ | Most efficient |
| **RenderTransform** | ⭐⭐⭐ | No layout recalculation |
| **Clip** | ⭐⭐ | Medium performance |
| **Width/Height** | ⭐ | Causes layout recalculation |
| **Margin** | ⭐ | Causes layout recalculation |

```csharp
// Performance optimization hints
RenderOptions.SetBitmapScalingMode(element, BitmapScalingMode.LowQuality);
Timeline.SetDesiredFrameRate(storyboard, 30); // Default 60fps → 30fps
```

---

## 8. References

- [Animation Overview - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/animation-overview)
- [Storyboards Overview - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/storyboards-overview)
