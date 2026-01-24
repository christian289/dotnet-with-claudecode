---
name: creating-wpf-brushes
description: "Creates WPF Brush patterns including SolidColorBrush, LinearGradientBrush, RadialGradientBrush, ImageBrush, and VisualBrush. Use when filling shapes with colors, gradients, images, or tile patterns."
---

# WPF Brush Patterns

Brush types for filling shapes and backgrounds in WPF.

## 1. Brush Hierarchy

```
Brush (abstract)
├── SolidColorBrush      ← Single color
├── GradientBrush
│   ├── LinearGradientBrush  ← Linear gradient
│   └── RadialGradientBrush  ← Radial gradient
├── TileBrush
│   ├── ImageBrush       ← Image fill
│   ├── DrawingBrush     ← Drawing fill
│   └── VisualBrush      ← Visual element fill
└── BitmapCacheBrush     ← Cached visual
```

---

## 2. SolidColorBrush

```xml
<!-- Inline color name -->
<Rectangle Fill="Blue"/>

<!-- Hex color -->
<Rectangle Fill="#FF2196F3"/>

<!-- With opacity -->
<Rectangle>
    <Rectangle.Fill>
        <SolidColorBrush Color="Blue" Opacity="0.5"/>
    </Rectangle.Fill>
</Rectangle>
```

```csharp
// Code creation
var brush = new SolidColorBrush(Colors.Blue);
brush.Opacity = 0.5;
brush.Freeze(); // Performance optimization
```

---

## 3. LinearGradientBrush

```xml
<!-- Horizontal gradient (default) -->
<Rectangle Width="200" Height="100">
    <Rectangle.Fill>
        <LinearGradientBrush StartPoint="0,0" EndPoint="1,0">
            <GradientStop Color="#2196F3" Offset="0"/>
            <GradientStop Color="#FF9800" Offset="1"/>
        </LinearGradientBrush>
    </Rectangle.Fill>
</Rectangle>

<!-- Diagonal gradient -->
<Rectangle.Fill>
    <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
        <GradientStop Color="#2196F3" Offset="0"/>
        <GradientStop Color="#4CAF50" Offset="0.5"/>
        <GradientStop Color="#FF9800" Offset="1"/>
    </LinearGradientBrush>
</Rectangle.Fill>

<!-- Vertical gradient -->
<Rectangle.Fill>
    <LinearGradientBrush StartPoint="0,0" EndPoint="0,1">
        <GradientStop Color="White" Offset="0"/>
        <GradientStop Color="Black" Offset="1"/>
    </LinearGradientBrush>
</Rectangle.Fill>
```

**StartPoint/EndPoint**: Relative coordinates (0-1)

---

## 4. RadialGradientBrush

```xml
<!-- Basic radial -->
<Ellipse Width="200" Height="200">
    <Ellipse.Fill>
        <RadialGradientBrush>
            <GradientStop Color="White" Offset="0"/>
            <GradientStop Color="Blue" Offset="1"/>
        </RadialGradientBrush>
    </Ellipse.Fill>
</Ellipse>

<!-- Off-center highlight (3D sphere effect) -->
<Ellipse.Fill>
    <RadialGradientBrush GradientOrigin="0.3,0.3"
                         Center="0.5,0.5"
                         RadiusX="0.5" RadiusY="0.5">
        <GradientStop Color="White" Offset="0"/>
        <GradientStop Color="Blue" Offset="1"/>
    </RadialGradientBrush>
</Ellipse.Fill>
```

**GradientOrigin**: Light source position (0-1)

---

## 5. ImageBrush

```xml
<!-- Basic image fill -->
<Rectangle Width="200" Height="200">
    <Rectangle.Fill>
        <ImageBrush ImageSource="/Assets/texture.png"/>
    </Rectangle.Fill>
</Rectangle>

<!-- Tiled pattern -->
<Rectangle.Fill>
    <ImageBrush ImageSource="/Assets/pattern.png"
                TileMode="Tile"
                Viewport="0,0,0.25,0.25"
                ViewportUnits="RelativeToBoundingBox"/>
</Rectangle.Fill>

<!-- Stretched (default) -->
<Rectangle.Fill>
    <ImageBrush ImageSource="/Assets/background.jpg"
                Stretch="UniformToFill"/>
</Rectangle.Fill>
```

**TileMode**: None, Tile, FlipX, FlipY, FlipXY
**Stretch**: None, Fill, Uniform, UniformToFill

---

## 6. VisualBrush

```xml
<!-- Tile visual elements -->
<Rectangle Width="200" Height="200">
    <Rectangle.Fill>
        <VisualBrush TileMode="Tile"
                     Viewport="0,0,0.25,0.25"
                     ViewportUnits="RelativeToBoundingBox">
            <VisualBrush.Visual>
                <Grid Width="50" Height="50">
                    <Ellipse Width="20" Height="20" Fill="Red"
                             HorizontalAlignment="Left" VerticalAlignment="Top"/>
                    <Ellipse Width="20" Height="20" Fill="Blue"
                             HorizontalAlignment="Right" VerticalAlignment="Bottom"/>
                </Grid>
            </VisualBrush.Visual>
        </VisualBrush>
    </Rectangle.Fill>
</Rectangle>

<!-- Reflection effect -->
<VisualBrush Visual="{Binding ElementName=SourceElement}">
    <VisualBrush.RelativeTransform>
        <ScaleTransform ScaleY="-1" CenterY="0.5"/>
    </VisualBrush.RelativeTransform>
</VisualBrush>
```

---

## 7. DrawingBrush

```xml
<!-- Checkered pattern -->
<Rectangle.Fill>
    <DrawingBrush TileMode="Tile"
                  Viewport="0,0,20,20"
                  ViewportUnits="Absolute">
        <DrawingBrush.Drawing>
            <GeometryDrawing Brush="Gray">
                <GeometryDrawing.Geometry>
                    <GeometryGroup>
                        <RectangleGeometry Rect="0,0,10,10"/>
                        <RectangleGeometry Rect="10,10,10,10"/>
                    </GeometryGroup>
                </GeometryDrawing.Geometry>
            </GeometryDrawing>
        </DrawingBrush.Drawing>
    </DrawingBrush>
</Rectangle.Fill>
```

---

## 8. Performance Tips

```csharp
// Always Freeze brushes for performance
var brush = new SolidColorBrush(Colors.Blue);
brush.Freeze();

var gradient = new LinearGradientBrush(Colors.Red, Colors.Blue, 0);
gradient.Freeze();
```

**Freeze()**: Makes brush immutable, enables cross-thread sharing, improves rendering performance.

---

## 9. References

- [Painting with Brushes - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/painting-with-solid-colors-and-gradients-overview)
- [TileBrush Overview - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/tilebrush-overview)
