# WPF Content Model — Advanced Patterns

> Core concepts: See [SKILL.md](SKILL.md)

---

## 1. Creating ContentControl-Derived Controls

```csharp
namespace MyApp.Controls;

using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Card control that displays single content
/// </summary>
public sealed class CardControl : ContentControl
{
    static CardControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CardControl),
            new FrameworkPropertyMetadata(typeof(CardControl)));
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(CardControl),
            new PropertyMetadata(new CornerRadius(8)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
}
```

---

## 2. Customizing ItemsPanel

```xml
<!-- Horizontal list layout -->
<ListBox ItemsSource="{Binding Items}">
    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <StackPanel Orientation="Horizontal"/>
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>
</ListBox>

<!-- Grid layout -->
<ItemsControl ItemsSource="{Binding Items}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <WrapPanel/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
</ItemsControl>

<!-- Virtualized panel (large data) -->
<ListBox ItemsSource="{Binding LargeCollection}"
         VirtualizingPanel.IsVirtualizing="True"
         VirtualizingPanel.VirtualizationMode="Recycling">
    <ListBox.ItemsPanel>
        <ItemsPanelTemplate>
            <VirtualizingStackPanel/>
        </ItemsPanelTemplate>
    </ListBox.ItemsPanel>
</ListBox>
```

---

## 3. Creating ItemsControl-Derived Controls

```csharp
namespace MyApp.Controls;

using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Control that displays a list of tags
/// </summary>
public sealed class TagListControl : ItemsControl
{
    static TagListControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TagListControl),
            new FrameworkPropertyMetadata(typeof(TagListControl)));
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        // Specify container that wraps each item
        return new TagItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is TagItem;
    }
}

public sealed class TagItem : ContentControl
{
    static TagItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TagItem),
            new FrameworkPropertyMetadata(typeof(TagItem)));
    }
}
```

---

## 4. Creating HeaderedContentControl-Derived Controls

```csharp
namespace MyApp.Controls;

using System.Windows;
using System.Windows.Controls;

/// <summary>
/// Collapsible section control
/// </summary>
public sealed class CollapsibleSection : HeaderedContentControl
{
    static CollapsibleSection()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CollapsibleSection),
            new FrameworkPropertyMetadata(typeof(CollapsibleSection)));
    }

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(
            nameof(IsExpanded),
            typeof(bool),
            typeof(CollapsibleSection),
            new PropertyMetadata(true));

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }
}
```

---

## 5. HierarchicalDataTemplate (Hierarchical Data Binding)

```xml
<TreeView ItemsSource="{Binding RootNodes}">
    <TreeView.ItemTemplate>
        <HierarchicalDataTemplate ItemsSource="{Binding Children}">
            <StackPanel Orientation="Horizontal">
                <Image Source="{Binding Icon}" Width="16"/>
                <TextBlock Text="{Binding Name}" Margin="5,0,0,0"/>
            </StackPanel>
        </HierarchicalDataTemplate>
    </TreeView.ItemTemplate>
</TreeView>
```
