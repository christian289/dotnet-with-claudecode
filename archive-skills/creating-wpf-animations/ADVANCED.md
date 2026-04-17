# WPF Animation Patterns — Advanced Patterns

> Core concepts: See [SKILL.md](SKILL.md)

---

## Composite Animation with Storyboard (C#)

```csharp
namespace MyApp.Animations;

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

public static class StoryboardHelper
{
    /// <summary>
    /// Scale up + fade in animation
    /// </summary>
    public static void ScaleFadeIn(FrameworkElement element, double durationSeconds = 0.3)
    {
        // Setup transform
        var scaleTransform = new ScaleTransform(0.8, 0.8);
        element.RenderTransform = scaleTransform;
        element.RenderTransformOrigin = new Point(0.5, 0.5);
        element.Opacity = 0;

        var storyboard = new Storyboard();

        // Opacity animation
        var opacityAnimation = new DoubleAnimation
        {
            To = 1,
            Duration = TimeSpan.FromSeconds(durationSeconds),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(opacityAnimation, element);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(opacityAnimation);

        // ScaleX animation
        var scaleXAnimation = new DoubleAnimation
        {
            To = 1,
            Duration = TimeSpan.FromSeconds(durationSeconds),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleXAnimation, element);
        Storyboard.SetTargetProperty(scaleXAnimation,
            new PropertyPath("RenderTransform.ScaleX"));
        storyboard.Children.Add(scaleXAnimation);

        // ScaleY animation
        var scaleYAnimation = new DoubleAnimation
        {
            To = 1,
            Duration = TimeSpan.FromSeconds(durationSeconds),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleYAnimation, element);
        Storyboard.SetTargetProperty(scaleYAnimation,
            new PropertyPath("RenderTransform.ScaleY"));
        storyboard.Children.Add(scaleYAnimation);

        storyboard.Begin();
    }
}
```

---

## AnimationController (Stop/Resume)

```csharp
namespace MyApp.Animations;

using System.Windows;
using System.Windows.Media.Animation;

public sealed class AnimationController
{
    private readonly Storyboard _storyboard;
    private readonly FrameworkElement _target;

    public AnimationController(Storyboard storyboard, FrameworkElement target)
    {
        _storyboard = storyboard;
        _target = target;
    }

    public void Start()
    {
        _storyboard.Begin(_target, isControllable: true);
    }

    public void Pause()
    {
        _storyboard.Pause(_target);
    }

    public void Resume()
    {
        _storyboard.Resume(_target);
    }

    public void Stop()
    {
        _storyboard.Stop(_target);
    }

    public void Seek(TimeSpan offset)
    {
        _storyboard.Seek(_target, offset, TimeSeekOrigin.BeginTime);
    }
}
```

---

## VisualStateManager Integration

### State-Based Animation

```xml
<Style TargetType="{x:Type Button}">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type Button}">
                <Border x:Name="Border" Background="{TemplateBinding Background}">
                    <VisualStateManager.VisualStateGroups>
                        <VisualStateGroup x:Name="CommonStates">
                            <VisualState x:Name="Normal">
                                <Storyboard>
                                    <ColorAnimation
                                        Storyboard.TargetName="Border"
                                        Storyboard.TargetProperty="(Border.Background).(SolidColorBrush.Color)"
                                        To="#2196F3" Duration="0:0:0.2"/>
                                </Storyboard>
                            </VisualState>
                            <VisualState x:Name="MouseOver">
                                <Storyboard>
                                    <ColorAnimation
                                        Storyboard.TargetName="Border"
                                        Storyboard.TargetProperty="(Border.Background).(SolidColorBrush.Color)"
                                        To="#1976D2" Duration="0:0:0.2"/>
                                </Storyboard>
                            </VisualState>
                            <VisualState x:Name="Pressed">
                                <Storyboard>
                                    <ColorAnimation
                                        Storyboard.TargetName="Border"
                                        Storyboard.TargetProperty="(Border.Background).(SolidColorBrush.Color)"
                                        To="#0D47A1" Duration="0:0:0.1"/>
                                </Storyboard>
                            </VisualState>
                        </VisualStateGroup>
                    </VisualStateManager.VisualStateGroups>
                    <ContentPresenter/>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```
