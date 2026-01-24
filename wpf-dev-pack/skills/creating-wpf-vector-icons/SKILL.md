---
name: creating-wpf-vector-icons
description: "Creates scalable vector icons in WPF using PathGeometry and GeometryGroup. Use when building resolution-independent icons, icon buttons, or symbol libraries."
---

# WPF Vector Icons

Create scalable, resolution-independent icons using WPF geometry.

## 1. Icon Definition Patterns

### 1.1 PathGeometry Resources

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Check mark -->
    <PathGeometry x:Key="CheckIconGeometry">
        M 2,7 L 5,10 L 10,3
    </PathGeometry>

    <!-- Close (X) -->
    <PathGeometry x:Key="CloseIconGeometry">
        M 2,2 L 10,10 M 10,2 L 2,10
    </PathGeometry>

    <!-- Plus -->
    <PathGeometry x:Key="PlusIconGeometry">
        M 6,2 L 6,10 M 2,6 L 10,6
    </PathGeometry>

    <!-- Minus -->
    <PathGeometry x:Key="MinusIconGeometry">
        M 2,6 L 10,6
    </PathGeometry>

    <!-- Arrow right -->
    <PathGeometry x:Key="ArrowRightIconGeometry">
        M 2,6 L 10,6 M 7,3 L 10,6 L 7,9
    </PathGeometry>

    <!-- Search (magnifier) -->
    <PathGeometry x:Key="SearchIconGeometry">
        M 7,7 A 4,4 0 1 1 7,6.99 M 10,10 L 14,14
    </PathGeometry>

</ResourceDictionary>
```

### 1.2 GeometryGroup for Complex Icons

```xml
<!-- Menu (hamburger) icon -->
<GeometryGroup x:Key="MenuIconGeometry">
    <RectangleGeometry Rect="0,0,16,2"/>
    <RectangleGeometry Rect="0,6,16,2"/>
    <RectangleGeometry Rect="0,12,16,2"/>
</GeometryGroup>

<!-- Settings (gear) icon -->
<GeometryGroup x:Key="SettingsIconGeometry">
    <EllipseGeometry Center="8,8" RadiusX="3" RadiusY="3"/>
    <PathGeometry>
        M 8,0 L 9,3 L 7,3 Z
        M 8,16 L 9,13 L 7,13 Z
        M 0,8 L 3,9 L 3,7 Z
        M 16,8 L 13,9 L 13,7 Z
    </PathGeometry>
</GeometryGroup>

<!-- Home icon -->
<GeometryGroup x:Key="HomeIconGeometry">
    <PathGeometry>M 8,1 L 1,7 L 3,7 L 3,14 L 6,14 L 6,10 L 10,10 L 10,14 L 13,14 L 13,7 L 15,7 Z</PathGeometry>
</GeometryGroup>
```

---

## 2. Icon Usage

### 2.1 Direct Path Usage

```xml
<Path Data="{StaticResource CheckIconGeometry}"
      Fill="Green"
      Width="16" Height="16"
      Stretch="Uniform"/>
```

### 2.2 Icon in Button

```xml
<Button Width="32" Height="32">
    <Path Data="{StaticResource CloseIconGeometry}"
          Fill="{Binding Foreground, RelativeSource={RelativeSource AncestorType=Button}}"
          Width="12" Height="12"
          Stretch="Uniform"/>
</Button>
```

---

## 3. IconButton Style

```xml
<Style x:Key="IconButtonStyle" TargetType="{x:Type Button}">
    <Setter Property="Width" Value="32"/>
    <Setter Property="Height" Value="32"/>
    <Setter Property="Background" Value="Transparent"/>
    <Setter Property="BorderThickness" Value="0"/>
    <Setter Property="Cursor" Value="Hand"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type Button}">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="4">
                    <Path x:Name="IconPath"
                          Data="{TemplateBinding Content}"
                          Fill="{TemplateBinding Foreground}"
                          Stretch="Uniform"
                          HorizontalAlignment="Center"
                          VerticalAlignment="Center"
                          Width="16" Height="16"/>
                </Border>
                <ControlTemplate.Triggers>
                    <Trigger Property="IsMouseOver" Value="True">
                        <Setter Property="Background" Value="#E3F2FD"/>
                        <Setter TargetName="IconPath" Property="Fill" Value="#2196F3"/>
                    </Trigger>
                    <Trigger Property="IsPressed" Value="True">
                        <Setter Property="Background" Value="#BBDEFB"/>
                    </Trigger>
                    <Trigger Property="IsEnabled" Value="False">
                        <Setter TargetName="IconPath" Property="Fill" Value="#BDBDBD"/>
                    </Trigger>
                </ControlTemplate.Triggers>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>

<!-- Usage -->
<Button Style="{StaticResource IconButtonStyle}"
        Content="{StaticResource CloseIconGeometry}"
        ToolTip="Close"/>

<Button Style="{StaticResource IconButtonStyle}"
        Content="{StaticResource SearchIconGeometry}"
        ToolTip="Search"/>
```

---

## 4. Icon with Text

```xml
<Style x:Key="IconTextButtonStyle" TargetType="{x:Type Button}">
    <Setter Property="Padding" Value="12,8"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type Button}">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        Padding="{TemplateBinding Padding}"
                        CornerRadius="4">
                    <StackPanel Orientation="Horizontal">
                        <Path Data="{TemplateBinding Tag}"
                              Fill="{TemplateBinding Foreground}"
                              Width="16" Height="16"
                              Stretch="Uniform"
                              Margin="0,0,8,0"/>
                        <ContentPresenter VerticalAlignment="Center"/>
                    </StackPanel>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>

<!-- Usage -->
<Button Style="{StaticResource IconTextButtonStyle}"
        Tag="{StaticResource PlusIconGeometry}"
        Content="Add Item"/>
```

---

## 5. Dynamic Icon Color

```xml
<!-- Icon that inherits foreground color -->
<Path Data="{StaticResource CheckIconGeometry}"
      Fill="{Binding Foreground, RelativeSource={RelativeSource AncestorType=ContentControl}}"
      Width="16" Height="16"
      Stretch="Uniform"/>

<!-- Icon with binding -->
<Path Data="{StaticResource CheckIconGeometry}"
      Fill="{Binding IconColor}"
      Width="16" Height="16"
      Stretch="Uniform"/>
```

---

## 6. Path Mini-Language Reference

| Command | Description | Example |
|---------|-------------|---------|
| **M** | MoveTo | M 10,10 |
| **L** | LineTo | L 100,100 |
| **H** | Horizontal line | H 50 |
| **V** | Vertical line | V 50 |
| **A** | Arc | A 50,50 0 0 1 100,100 |
| **Z** | Close path | Z |

Lowercase = relative, Uppercase = absolute

---

## 7. References

- [Path Markup Syntax - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/path-markup-syntax)
- [Geometry Overview - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/geometry-overview)
