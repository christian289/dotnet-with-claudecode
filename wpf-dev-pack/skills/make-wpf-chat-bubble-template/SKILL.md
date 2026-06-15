---
description: "Generates a role-differentiated chat bubble DataTemplate (color, alignment, asymmetric margin, content-sized) plus the turn ViewModel for an ItemsControl of chat turns. Use when laying out a chat/conversation list where user and assistant turns must look distinct. Usage: /wpf-dev-pack:make-wpf-chat-bubble-template <TurnName>"
argument-hint: [TurnName]
---

# WPF Chat Bubble Template Generator

**If `$0` is empty, use the AskUserQuestion tool to ask: "Enter the chat turn ViewModel name (e.g., ChatTurnViewModel, MessageViewModel)". Do NOT proceed until a valid name is provided. Use the response as the turn ViewModel name for all subsequent steps.**

Generate the role-differentiated chat bubble `DataTemplate` (opposite alignment, distinct background, asymmetric gutter, sized to content) and the `$0` turn ViewModel that drives it, for an `ItemsControl` of chat turns.

- Replace `{Namespace}` with the project's root namespace.
- The bubble hosts a `MarkdownPresenter` (generate it with
  `/wpf-dev-pack:make-wpf-markdown-presenter`) so each turn renders selectable
  markdown.

> **The governing rule** (full rationale in the `styling-chat-bubbles-in-wpf`
> knowledge topic — fetch via `WpfDevPackMcp get_wpf_topic`): declare the base
> (assistant) values as **Style Setters** and the per-role override as a
> **DataTrigger Setter** — NEVER set a trigger-controlled property
> (`Background`, `HorizontalAlignment`, `Margin`) as an inline attribute on the
> `Border`. In WPF dependency-property value precedence a local value (an inline
> attribute) outranks a Style trigger Setter, so an inline attribute silently
> defeats the role `DataTrigger` and both roles render identically.

## Usage

```bash
/wpf-dev-pack:make-wpf-chat-bubble-template ChatTurnViewModel
```

## Generated Code

### $0.cs (turn ViewModel)

`IsUser` discriminates the role; `Markdown` is the content the streaming
orchestrator appends to. Lives in the ViewModels project (no `System.Windows`).

> **Skip this file if `/wpf-dev-pack:make-wpf-chatclient` already emitted a
> turn ViewModel** — emitting both produces a duplicate-type collision. The
> chatclient's version is this class plus an `IsWaiting` flag.

```csharp
namespace {Namespace}.ViewModels;

public sealed partial class $0 : ObservableObject
{
    [ObservableProperty] private bool _isUser;
    [ObservableProperty] private bool _isWaiting;          // "waiting…" until the first streamed token
    [ObservableProperty] private string _markdown = string.Empty;

    // The streaming orchestrator appends tokens here; the bound MarkdownPresenter re-renders.
    public void AppendMarkdown(string delta) => Markdown += delta;
}
```

### Chat list XAML (ItemsControl + bubble DataTemplate)

The default uses **stock-WPF brushes** so it builds with no UI-kit package:

```xml
<ItemsControl xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
              xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
              xmlns:vm="clr-namespace:{Namespace}.ViewModels;assembly={Namespace}.ViewModels"
              xmlns:controls="clr-namespace:{Namespace}.Controls"
              ItemsSource="{Binding Turns}">
    <ItemsControl.ItemTemplate>
        <DataTemplate DataType="{x:Type vm:$0}">
            <Border Padding="10" CornerRadius="6">
                <Border.Style>
                    <Style TargetType="Border">
                        <!-- DEFAULT = ASSISTANT (response): left, neutral background, right gutter -->
                        <Setter Property="HorizontalAlignment" Value="Left"/>
                        <Setter Property="Margin" Value="0,4,48,0"/>
                        <Setter Property="Background" Value="#FFF0F0F0"/>
                        <Style.Triggers>
                            <!-- USER (request): right, tinted background, left gutter -->
                            <DataTrigger Binding="{Binding IsUser}" Value="True">
                                <Setter Property="HorizontalAlignment" Value="Right"/>
                                <Setter Property="Margin" Value="48,4,0,0"/>
                                <Setter Property="Background" Value="#FFD6E8FF"/>
                            </DataTrigger>
                        </Style.Triggers>
                    </Style>
                </Border.Style>

                <!-- content hugs (MarkdownPresenter reports a hug desired width);
                     the 48px gutter caps max width = parent - 48 -->
                <controls:MarkdownPresenter Markdown="{Binding Markdown}"/>
            </Border>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

## Key Rules

| Rule | Why |
|------|-----|
| Base values = Style **Setter**; role override = **DataTrigger** Setter | A local (inline) value outranks a Style trigger Setter, so inline attributes defeat the trigger. |
| `HorizontalAlignment` = `Left`/`Right`, never `Stretch` | `Stretch` forces the bubble to full width; hugging needs `Left`/`Right` **and** a child that reports a hug width. |
| One asymmetric `Margin` per role | Doubles as the "who sent it" gutter and the max-width cap (`parent − gutter`); no separate `MaxWidth` needed. |
| Properties constant across roles (`Padding`, `CornerRadius`) | Safe to leave as inline attributes — never under trigger control. |

> **WPF-UI variant** — if (and only if) the project references `Wpf.Ui`, swap
> the two `Background` Setters for theme-aware kit brushes and add
> `xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml"`:
> `{ui:ThemeResource CardBackgroundFillColorDefaultBrush}` (assistant) /
> `{ui:ThemeResource SystemAccentColorPrimaryBrush}` (user). Declaring
> `xmlns:ui` without the package installed is an MC3074 **build** error, and an
> invalid theme-resource key throws `XamlParseException` at **render** time —
> use only keys verified to exist in the kit.

## File Location

```
{Project}.ViewModels/
└── $0.cs
{Project}.WpfApp/
└── Views/  (place the ItemsControl XAML in the chat view)
```

## Related

- Knowledge (fetch via `WpfDevPackMcp get_wpf_topic`): `styling-chat-bubbles-in-wpf`,
  `displaying-selectable-rich-text-in-wpf`, `managing-styles-resourcedictionary`,
  `virtualizing-wpf-ui` (for long histories of these bubbles).
- Sibling scaffolders: `/wpf-dev-pack:make-wpf-markdown-presenter` (the bubble's
  content control). This template is one of the pieces emitted by the one-button
  `/wpf-dev-pack:make-wpf-chatclient`.
