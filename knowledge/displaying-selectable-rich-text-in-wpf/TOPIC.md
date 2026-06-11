# Displaying Selectable Rich Text in WPF (read-only RichTextBox + FlowDocument)

> Renders user-selectable, copyable rich text in WPF by hosting a `FlowDocument` inside a read-only `RichTextBox`, because a `TextBlock`/`Run` tree shows formatting but exposes no text-selection API and cannot be drag-selected or `Ctrl+C`-copied. Use when a chat answer, markdown view, or formatted message must support drag-select and copy, must size its bubble to its content rather than greedily filling the available width, must clickable hyperlinks and per-code-block Copy buttons, must keep scrolling when nested in an outer `ScrollViewer`, and must not flicker while text streams in. Covers the `MarkdownPresenter` `ContentControl` shell and `Render`, `BlockUIContainer` interactive sub-content, the content-hugging `HuggingRichTextBox` self-measure recipe, forwarding `MouseWheel` to the parent scroller, and throttling the full document rebuild during fast streaming.

Selectable, copyable rich text in WPF is only available from a `FlowDocument` hosted in a read-only `RichTextBox` (or a `FlowDocumentScrollViewer` with `IsSelectionEnabled`) — a `TextBlock` tree renders the same formatting but cannot select.

---

## 1. Selectable rich text = `FlowDocument` hosted in a read-only `RichTextBox`

- **Phenomenon**: Chat content rendered as a `TextBlock`/`Run` tree shows rich formatting but cannot be drag-selected or copied. There is no `IsTextSelectionEnabled` to flip.
- **Cause**: WPF `TextBlock` exposes no text-selection API (unlike the WinUI control of the same name). Drag-select + `Ctrl+C` over mixed formatted content is only available from a `FlowDocument` in a read-only `RichTextBox` (or `FlowDocumentScrollViewer` with `IsSelectionEnabled`).
- **Effect**: A "let the user select/copy the answer" requirement forces the renderer from a `UIElement` tree to a `FlowDocument` (`Paragraph`/`Run`/`Bold`/`Italic`/`Hyperlink`/`List`/`Section`), hosted in a read-only `RichTextBox`.

The control shell — a `ContentControl` with a bindable `Markdown` dependency property whose change callback re-renders by swapping `Content`. The streaming ViewModel just appends to the bound `Markdown` string; each change re-runs `Render`. (Rebuilding the whole `FlowDocument` per update is simple and fine for an MVP — a known optimization point for very long, fast-streaming answers; see section 5.)

```csharp
public sealed class MarkdownPresenter : ContentControl
{
    private static readonly MarkdownPipeline _pipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    public static readonly DependencyProperty MarkdownProperty =
        DependencyProperty.Register(nameof(Markdown), typeof(string), typeof(MarkdownPresenter),
            new PropertyMetadata(null, OnMarkdownChanged));

    public string? Markdown
    {
        get => (string?)GetValue(MarkdownProperty);
        set => SetValue(MarkdownProperty, value);
    }

    private static void OnMarkdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((MarkdownPresenter)d).Render((string?)e.NewValue);

    // Render: parse markdown -> build a FlowDocument -> host it in a read-only RichTextBox.
    // (full body below)
}
```

`Render` parses with Markdig, builds a `FlowDocument`, and swaps it into a read-only `RichTextBox` (borderless, transparent, content-sized, hyperlinks live):

```csharp
private void Render(string? markdown)
{
    if (string.IsNullOrEmpty(markdown)) { Content = null; return; }

    var doc = new FlowDocument { PagePadding = new Thickness(0), FontSize = 14 };
    foreach (MarkdigBlock block in Markdig.Markdown.Parse(markdown, _pipeline))
    {
        FlowBlock? b = RenderBlock(block);
        if (b is not null) { doc.Blocks.Add(b); }
    }

    var host = new HuggingRichTextBox   // see section 3 for the subclass
    {
        Document = doc,
        IsReadOnly = true,          // selection + Ctrl+C; no editing, no caret
        IsDocumentEnabled = true,   // Hyperlink inlines stay clickable
        IsTabStop = false,
        BorderThickness = new Thickness(0),
        Background = Brushes.Transparent,
        Padding = new Thickness(0),
        MinHeight = 0,
        NaturalWidth = ComputeNaturalWidth(markdown) // section 3
    };
    host.SetResourceReference(Control.ForegroundProperty, "TextFillColorPrimaryBrush"); // see Note A
    ScrollViewer.SetHorizontalScrollBarVisibility(host, ScrollBarVisibility.Disabled);  // wrap at forced width
    host.PreviewMouseWheel += ForwardWheelToParent;                                     // section 4
    Content = host;
}
```

---

## 2. `BlockUIContainer` for interactive sub-content (code block + Copy)

Code blocks keep a Copy button by hosting a `UIElement` inside the flow via `BlockUIContainer` — and the inner read-only `TextBox` is itself selectable + horizontally scrollable:

```csharp
private static BlockUIContainer BuildCodeBlock(string code)
{
    string text = code.TrimEnd('\n', '\r');
    var box = new TextBox
    {
        Text = text, IsReadOnly = true,
        FontFamily = new FontFamily("Consolas"),
        TextWrapping = TextWrapping.NoWrap,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
        BorderThickness = new Thickness(0), Background = Brushes.Transparent
    };
    box.SetResourceReference(TextBox.ForegroundProperty, "TextFillColorPrimaryBrush");
    var copy = new Wpf.Ui.Controls.Button { Content = "Copy", VerticalAlignment = VerticalAlignment.Top };
    copy.Click += (_, _) => { try { Clipboard.SetText(text); } catch { /* clipboard busy */ } };

    var grid = new Grid();
    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
    Grid.SetColumn(box, 0); Grid.SetColumn(copy, 1);
    grid.Children.Add(box); grid.Children.Add(copy);

    var border = new Border { Padding = new Thickness(8), Margin = new Thickness(0, 4, 0, 6), Child = grid };
    return new BlockUIContainer(border) { Margin = new Thickness(0) };
}
```

Two adjacent gotchas hit while building the document in code:

- **Note A — code-built text elements do not inherit a theme foreground.** A `FlowDocument`/`TextBlock` created in code renders black on a dark theme until a brush is supplied. Set it once on the host via a resource reference to the kit brush (`host.SetResourceReference(Control.ForegroundProperty, "TextFillColorPrimaryBrush")`); children inherit it. The same applies to the inner code `TextBox`.
- **Note B — an invalid `{ui:ThemeResource ...}` key throws at *runtime*, not build.** Referencing a non-existent theme brush (a guessed accent-brush key) compiles cleanly and then throws `XamlParseException` the instant the element is realized (first message render). Only use keys verified to exist in the kit (e.g. `SystemAccentColorPrimaryBrush`, `CardBackgroundFillColorDefaultBrush`). See [`integrating-wpfui-fluent`](../integrating-wpfui-fluent/TOPIC.md).

---

## 3. Content-hugging read-only `RichTextBox` (size follows content)

- **Phenomenon**: A read-only `RichTextBox` in a content-hugging container always expands to the full available width, so a one-word answer fills the whole bubble. Subclassing to fix it fails two opposite ways: measuring at `PositiveInfinity` width collapses the desired width to ~0 (**text renders one glyph per line, vertically**); measuring at a finite width returns that finite (available) width unchanged (**greedy, always max**).
- **Cause**: `TextBoxBase` reports a desired width that fills the available width, and degenerates to a minimum (~0) at an infinite constraint. So the control's own `MeasureOverride` can never yield the *natural* content width.
- **Effect**: The natural width must be computed **outside** the control — `FormattedText`/`TextBlock` *do* report natural width at an infinite constraint — and then forced. The bubble then hugs short content and wraps long content.

The subclass forces an externally-computed width and re-measures height at that width:

```csharp
private sealed class HuggingRichTextBox : RichTextBox
{
    public double NaturalWidth { get; set; } // set by the presenter (below)

    protected override Size MeasureOverride(Size constraint)
    {
        double width = double.IsInfinity(constraint.Width)
            ? NaturalWidth
            : Math.Min(NaturalWidth, constraint.Width); // hug short / wrap long
        Size measured = base.MeasureOverride(new Size(width, constraint.Height));
        return new Size(width, measured.Height);
    }
}
```

Natural width is measured from the **source markdown lines** (reliable at any constraint), with code fences in a monospace face and headings at a larger size to avoid under-measuring:

```csharp
private static double ComputeNaturalWidth(string markdown)
{
    var prose = new Typeface("Segoe UI");
    var code  = new Typeface("Consolas");
    double max = 0; bool inFence = false;
    foreach (string line in markdown.Replace("\r\n", "\n").Split('\n'))
    {
        string t = line.TrimStart();
        if (t.StartsWith("```")) { inFence = !inFence; continue; }
        if (line.Length == 0) { continue; }

        double size = 14; Typeface tf = prose;
        if (inFence) { tf = code; }
        else if (t.StartsWith("#")) { size = 20; } // heading rendered larger -> measure larger

        var ft = new FormattedText(line, CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight, tf, size, Brushes.Black, 1.0);
        max = Math.Max(max, ft.WidthIncludingTrailingWhitespace);
    }
    return max + 12; // padding / caret / bold-and-inline-formatting buffer
}
```

How it composes with the bubble: the bubble's asymmetric outer margin (e.g. `48` on the gutter side) caps the *available* width to `parent − 48`; `Math.Min(NaturalWidth, available)` therefore hugs short messages and wraps long ones at the gutter. `HorizontalScrollBarVisibility=Disabled` on the host makes the FlowDocument wrap at exactly the forced width rather than scroll.

---

## 4. Forward `MouseWheel` to the parent scroller

- **Phenomenon**: In a scrolling conversation list, scrolling while the pointer is over a message bubble does nothing — the outer list won't move, even when the bubble has no internal scroll.
- **Cause**: `TextBoxBase` handles `MouseWheel` and marks it handled regardless of whether its content can scroll, so the event never bubbles to the ancestor `ScrollViewer`.
- **Effect**: Scrolling stalls over every message. Fix: intercept `PreviewMouseWheel` and re-raise an equivalent bubbling `MouseWheel` on the parent.

```csharp
private static void ForwardWheelToParent(object sender, MouseWheelEventArgs e)
{
    if (e.Handled) { return; }
    e.Handled = true;
    var args = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
    {
        RoutedEvent = UIElement.MouseWheelEvent,
        Source = sender
    };
    if (sender is RichTextBox host && host.Parent is UIElement parent) { parent.RaiseEvent(args); }
}
```

This applies to any inner text control nested in a scroller. The Preview/bubbling re-raise mechanics are covered in [`routing-wpf-events`](../routing-wpf-events/TOPIC.md).

---

## 5. Throttle the full `FlowDocument` rebuild during fast streaming

- **Phenomenon**: Rebuilding the entire `FlowDocument` on every streamed token (section 1's `OnMarkdownChanged → Render`) causes flicker and high CPU on long, fast responses, and resets any in-progress selection.
- **Cause**: Each `Markdown` change re-parses and rebuilds the whole document; at token cadence that is many full rebuilds per second.
- **Effect**: Visible flicker, wasted CPU, selection churn mid-stream.

```csharp
// Coalesce re-renders on a DispatcherTimer instead of rebuilding per token.
private readonly DispatcherTimer _renderTimer =
    new() { Interval = TimeSpan.FromMilliseconds(50) }; // ~20 Hz
// ctor: _renderTimer.Tick += (_, _) => { _renderTimer.Stop(); Render(Markdown); };
private static void OnMarkdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
    var self = (MarkdownPresenter)d;
    self._renderTimer.Stop();
    self._renderTimer.Start(); // debounce: render once the burst settles
}
```

Alternatives: render plain text while streaming and do a single markdown pass on completion; or keep the document and append only the delta (harder when markdown re-flows). Throttling is the lowest-risk default. The `DispatcherTimer` and the WPF UI-thread model are covered in [`threading-wpf-dispatcher`](../threading-wpf-dispatcher/TOPIC.md).

---

## Related topics

- [`rendering-markdown-in-wpf`](../rendering-markdown-in-wpf/TOPIC.md) — the markdown source / Markdig parse that feeds the `FlowDocument`.
- [`integrating-wpfui-fluent`](../integrating-wpfui-fluent/TOPIC.md) — theme brush keys and foreground inheritance (Notes A/B).
- [`routing-wpf-events`](../routing-wpf-events/TOPIC.md) — Preview/bubbling re-raise for `ForwardWheelToParent`.
- [`rendering-wpf-architecture`](../rendering-wpf-architecture/TOPIC.md) — Measure/Arrange pass, why `MeasureOverride` is the hook for content-hugging.
- [`authoring-wpf-controls`](../authoring-wpf-controls/TOPIC.md) — subclass vs new control for `HuggingRichTextBox`.
- [`threading-wpf-dispatcher`](../threading-wpf-dispatcher/TOPIC.md) — `DispatcherTimer` for streaming debounce.
- [`styling-chat-bubbles-in-wpf`](../styling-chat-bubbles-in-wpf/TOPIC.md) — the bubble container whose asymmetric margin caps the available width.

---

## References

- [FlowDocument Class — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.documents.flowdocument)
- [RichTextBox Class — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.controls.richtextbox)
- [TextBoxBase.IsReadOnly Property — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.controls.primitives.textboxbase.isreadonly)
- [BlockUIContainer Class — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.documents.blockuicontainer)
- [FormattedText Class — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.media.formattedtext)
- [FrameworkElement.MeasureOverride Method — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.frameworkelement.measureoverride)
- [ScrollViewer.HorizontalScrollBarVisibility Attached Property — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.controls.scrollviewer.horizontalscrollbarvisibility)
- [UIElement.MouseWheel Event — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.uielement.mousewheel)
- [DispatcherTimer Class — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.threading.dispatchertimer)
- [Flow Document Overview — Microsoft Learn](https://learn.microsoft.com/dotnet/desktop/wpf/advanced/flow-document-overview)
