---
name: implementing-wpf-rtl-support
description: "Implements Right-to-Left (RTL) layout support in WPF using FlowDirection. Use when building applications for Arabic, Hebrew, Persian, or other RTL languages."
---

# WPF RTL (Right-to-Left) Support

Implement bidirectional text and mirrored layouts for RTL languages.

## 1. RTL Languages

| Language | Culture Code | Direction |
|----------|-------------|-----------|
| Arabic | ar-SA, ar-EG | RTL |
| Hebrew | he-IL | RTL |
| Persian (Farsi) | fa-IR | RTL |
| Urdu | ur-PK | RTL |

---

## 2. Setting FlowDirection

### 2.1 Window Level

```xml
<!-- Left-to-Right (default) -->
<Window FlowDirection="LeftToRight"
        Title="My Application">

<!-- Right-to-Left -->
<Window FlowDirection="RightToLeft"
        Title="التطبيق">
```

### 2.2 Element Level

```xml
<Window FlowDirection="RightToLeft">
    <StackPanel>
        <!-- Inherits RTL from parent -->
        <TextBlock Text="مرحبا بالعالم"/>

        <!-- Override to LTR for specific content -->
        <TextBlock FlowDirection="LeftToRight"
                   Text="user@example.com"/>
    </StackPanel>
</Window>
```

### 2.3 Resource-Based

```xml
<Window FlowDirection="{DynamicResource AppFlowDirection}">

<!-- In ResourceDictionary -->
<FlowDirection x:Key="AppFlowDirection">RightToLeft</FlowDirection>
```

---

## 3. Dynamic FlowDirection

### 3.1 Based on Culture

```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SetFlowDirection();
    }

    private void SetFlowDirection()
    {
        var culture = Thread.CurrentThread.CurrentUICulture;
        FlowDirection = culture.TextInfo.IsRightToLeft
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
    }
}
```

### 3.2 Language Switching

```csharp
public void SwitchLanguage(string cultureName)
{
    var culture = new CultureInfo(cultureName);

    Thread.CurrentThread.CurrentCulture = culture;
    Thread.CurrentThread.CurrentUICulture = culture;

    // Update FlowDirection
    Application.Current.MainWindow.FlowDirection =
        culture.TextInfo.IsRightToLeft
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
}
```

---

## 4. Mirroring Behavior

### 4.1 What Gets Mirrored

| Element | Mirrored |
|---------|----------|
| Text alignment | ✅ |
| StackPanel Horizontal | ✅ |
| Grid columns | ✅ |
| Margins/Padding | ✅ |
| ScrollBar position | ✅ |
| Menu items | ✅ |

### 4.2 What Should NOT Be Mirrored

```xml
<!-- Images (logos, icons) -->
<Image Source="logo.png" FlowDirection="LeftToRight"/>

<!-- Phone numbers -->
<TextBlock FlowDirection="LeftToRight" Text="+1-234-567-8900"/>

<!-- Email addresses -->
<TextBlock FlowDirection="LeftToRight" Text="user@example.com"/>

<!-- Numbers in specific format -->
<TextBlock FlowDirection="LeftToRight" Text="12345"/>

<!-- Progress bars -->
<ProgressBar FlowDirection="LeftToRight" Value="75"/>

<!-- Sliders -->
<Slider FlowDirection="LeftToRight" Value="50"/>
```

---

## 5. Bidirectional Text

### 5.1 Mixed Content

```xml
<!-- Arabic text with English -->
<TextBlock>
    <Run Text="مرحبا "/>
    <Run FlowDirection="LeftToRight" Text="John"/>
    <Run Text=" كيف حالك؟"/>
</TextBlock>
```

### 5.2 TextBlock with Mixed Direction

```xml
<TextBlock FlowDirection="RightToLeft">
    السعر: <Run FlowDirection="LeftToRight">$99.99</Run>
</TextBlock>
```

---

## 6. Layout Considerations

### 6.1 Grid Layout

```xml
<!-- RTL: Column 0 appears on right -->
<Grid FlowDirection="RightToLeft">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>  <!-- Right side in RTL -->
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>  <!-- Left side in RTL -->
    </Grid.ColumnDefinitions>

    <TextBlock Grid.Column="0" Text="الاسم:"/>
    <TextBox Grid.Column="1"/>
    <Button Grid.Column="2" Content="حفظ"/>
</Grid>
```

### 6.2 DockPanel

```xml
<!-- RTL: DockPanel.Dock="Left" appears on right -->
<DockPanel FlowDirection="RightToLeft">
    <Button DockPanel.Dock="Left" Content="القائمة"/>  <!-- Right side -->
    <TextBlock Text="المحتوى"/>
</DockPanel>
```

---

## 7. Testing RTL

```csharp
// Force RTL for testing (even on LTR system)
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Test with Arabic culture
        var culture = new CultureInfo("ar-SA");
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        base.OnStartup(e);
    }
}
```

---

## 8. Common Pitfalls

| Issue | Solution |
|-------|----------|
| Icons look wrong | Set `FlowDirection="LeftToRight"` on Image |
| Numbers reversed | Wrap in `Run` with LTR |
| Progress bar wrong | Set `FlowDirection="LeftToRight"` |
| Checkbox text spacing | Check Margin/Padding |

---

## 9. References

- [Bidirectional Features - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/bidirectional-features-in-wpf-overview)
- [FlowDirection - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.windows.flowdirection)
