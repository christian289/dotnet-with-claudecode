# Styling Role-Differentiated Chat Bubbles in WPF

> Styles role-differentiated chat bubbles (user request vs. assistant response) with opposite horizontal alignment, distinct background color, and an asymmetric margin, each bubble sized to hug its content rather than stretch full-width. The non-negotiable rule: declare the base (assistant/default) values as Style Setters and the per-role override as a DataTrigger Setter — never set a trigger-controlled property (`Background`, `HorizontalAlignment`, `Margin`) as an inline attribute on the `Border`, because in WPF dependency-property value precedence a local value outranks a Style trigger Setter, so an inline attribute silently defeats the role DataTrigger and both roles render identically. Content-hug additionally requires `HorizontalAlignment` of `Left`/`Right` (not `Stretch`) plus a child that reports a hug desired width; the asymmetric margin doubles as the role gutter and the max-width cap.

## 1. The Role Style + DataTrigger Template

One `DataTemplate` per chat turn; `IsUser` is a `bool` on the turn ViewModel. The default Style targets the assistant (response); the `DataTrigger` flips alignment, margin, and background for the user (request).

```xml
<!-- One DataTemplate per chat turn; IsUser is a bool on the turn VM. -->
<Border Padding="10" CornerRadius="6">
    <Border.Style>
        <Style TargetType="Border">
            <!-- DEFAULT = ASSISTANT (response): left, card background, right gutter -->
            <Setter Property="HorizontalAlignment" Value="Left"/>
            <Setter Property="Margin" Value="0,4,48,0"/>
            <Setter Property="Background" Value="{ui:ThemeResource CardBackgroundFillColorDefaultBrush}"/>
            <Style.Triggers>
                <!-- USER (request): right, accent background, left gutter -->
                <DataTrigger Binding="{Binding IsUser}" Value="True">
                    <Setter Property="HorizontalAlignment" Value="Right"/>
                    <Setter Property="Margin" Value="48,4,0,0"/>
                    <Setter Property="Background" Value="{ui:ThemeResource SystemAccentColorPrimaryBrush}"/>
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Border.Style>

    <!-- content hugs (see §3); the 48px gutter caps max width = parent - 48 -->
    <md:MarkdownPresenter Markdown="{Binding Markdown}"/>
</Border>
```

---

## 2. The Local-Value-Precedence Rule (Base-as-Setter / Override-as-Trigger)

WPF dependency-property value precedence ranks a **local value (an inline attribute)** *above* a **Style trigger Setter**. So `<Border Background="..."/>` set inline can never be overridden by a `DataTrigger` Setter — the trigger fires but its Setter loses the precedence contest, and the property keeps the inline value. The symptom is a role DataTrigger that "appears to do nothing": user and assistant bubbles render with identical colors (and alignment, and margin).

- **Never set a trigger-controlled property inline.** Base = Style Setter, override = Trigger Setter. The trigger-controlled properties here are `Background`, `HorizontalAlignment`, and `Margin`.
- Anything that is **constant across both roles** (here `Padding` and `CornerRadius`) is safe to leave as an inline attribute — it is never under trigger control, so no precedence conflict arises.
- This is a concrete, recurring instance of a precedence rule that [`managing-styles-resourcedictionary`](../managing-styles-resourcedictionary/TOPIC.md) states only abstractly.

---

## 3. Alignment (Left/Right, not Stretch) and Child-Hug Interplay

A bubble hugs its content — rather than stretching to the full width of the list item — only when **both** of these hold:

1. The bubble's `HorizontalAlignment` is `Left` or `Right`, **not** `Stretch`. A `Stretch` alignment forces the `Border` to fill its parent's width regardless of content.
2. The child reports a hug desired width. A stretching child forces full width even when the bubble's own alignment is `Left`/`Right`.

So alignment is only *half* of what produces the hug; the other half is the child's own desired-width behavior. The `md:MarkdownPresenter` recipe that makes the child report a hug desired width lives in [`displaying-selectable-rich-text-in-wpf`](../displaying-selectable-rich-text-in-wpf/TOPIC.md) — the bubble's content-hug depends on it.

---

## 4. The Asymmetric Margin as Gutter and Max-Width Cap

The asymmetric margin serves two purposes at once:

- **Role gutter** — a left or right gutter that reads visually as "who sent it". The assistant's `Margin="0,4,48,0"` opens a 48px gutter on the right; the user's `Margin="48,4,0,0"` opens it on the left. The bubbles lean to opposite sides of the conversation.
- **Max-width cap** — because the margin is reserved space the `Border` cannot occupy, the 48px gutter caps the bubble's maximum width at `parent - 48`. Long messages wrap inside that cap instead of running edge to edge, leaving breathing room on the opposite side.

One asymmetric margin therefore encodes both the role indicator and the max-width cap; no separate `MaxWidth` is needed.

---

## References

- [Dependency property value precedence — Microsoft Learn](https://learn.microsoft.com/dotnet/desktop/wpf/properties/dependency-property-value-precedence)

### Related topics

- [`managing-styles-resourcedictionary`](../managing-styles-resourcedictionary/TOPIC.md) — value precedence and `BasedOn`; this topic is a concrete instance of the precedence rule it states abstractly.
- [`displaying-selectable-rich-text-in-wpf`](../displaying-selectable-rich-text-in-wpf/TOPIC.md) — the child recipe whose hug desired width the bubble's content-hug depends on.
