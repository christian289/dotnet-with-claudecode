# ResourceDictionary Patterns

Rules for organizing ResourceDictionary files in WPF custom control libraries.

---

## Generic.xaml — MergedDictionaries Hub Only

`Generic.xaml` must function exclusively as a hub that merges other dictionaries.
Never define inline styles, templates, or resources directly in `Generic.xaml`.

```xml
<!-- Correct: Generic.xaml is only a hub -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="/Themes/Colors.xaml" />
        <ResourceDictionary Source="/Themes/MyButton.xaml" />
        <ResourceDictionary Source="/Themes/MyTextBox.xaml" />
        <ResourceDictionary Source="/Themes/MySlider.xaml" />
    </ResourceDictionary.MergedDictionaries>
</ResourceDictionary>
```

```xml
<!-- Prohibited: inline style in Generic.xaml -->
<ResourceDictionary ...>
    <Style TargetType="{x:Type local:MyButton}">
        ...
    </Style>
</ResourceDictionary>
```

---

## Separate Each Control Style into Its Own File

Every custom control gets its own XAML file under `Themes/`:

```
MyControlLibrary/
├── Themes/
│   ├── Generic.xaml          ← hub only, no styles
│   ├── Colors.xaml           ← shared color/brush resources
│   ├── MyButton.xaml         ← ControlTemplate for MyButton
│   ├── MyTextBox.xaml        ← ControlTemplate for MyTextBox
│   └── MySlider.xaml         ← ControlTemplate for MySlider
├── MyButton.cs
├── MyTextBox.cs
└── MySlider.cs
```

---

## Resource Definition Order

Within any single XAML file, define resources in this order:

1. `x:Key` named resources (colors, brushes, geometry, converters)
2. `Style` definitions (which reference the above by `StaticResource`)

```xml
<ResourceDictionary ...>

    <!-- 1. Named resources first -->
    <Color x:Key="PrimaryColor">#FF2196F3</Color>
    <SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}" />

    <!-- 2. Styles second -->
    <Style TargetType="{x:Type local:MyButton}">
        <Setter Property="Background" Value="{StaticResource PrimaryBrush}" />
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="{x:Type local:MyButton}">
                    ...
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

</ResourceDictionary>
```

---

## StaticResource vs DynamicResource

| Scenario | Use |
|---|---|
| Resource defined in same file or merged earlier | `StaticResource` |
| Resource may change at runtime (theme switching) | `DynamicResource` |
| Resource defined in a later-merged dictionary | `DynamicResource` |
| ControlTemplate internal references | `TemplateBinding` (not resource lookup) |

Prefer `StaticResource` by default — it is resolved at load time and has zero runtime overhead.
Use `DynamicResource` only when runtime replacement is explicitly required.
