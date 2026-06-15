# wpf-dev-pack Feedback — Building a streaming LLM ChatClient surface in WPF (code-heavy)

- **Purpose**: A streaming LLM chat panel — provider-agnostic `Microsoft.Extensions.AI` `IChatClient`, Markdig markdown rendered as selectable, role-differentiated, content-hugging bubbles, with an Enter-to-send input — surfaced a cluster of WPF-specific failure modes that no current topic covers. Each cost several wrong attempts that only `dotnet build`/runtime behavior (not the LSP) could disambiguate. This is a **code-pattern-first** capture: every item carries the concrete pattern that worked.
- **Intended use**: this doc is meant to seed `make-wpf-*` **scaffolder** skills as well as knowledge topics, so the code blocks are written as **emit-ready implementation skeletons** (full class / DependencyProperty / behavior shells, not illustrative fragments). The "Scaffolder candidates" table below maps each skeleton to a proposed generator.
- **Scope**: 20 items → ~9 proposed new knowledge topics + 2 new skills + 5 scaffolder candidates, plus augmentations/cross-links to existing topics. Items 1–9 are the WPF rendering/binding/measurement pitfalls; items 10–20 are the surrounding end-to-end plumbing (MCP wiring, DI lifetime, key storage, auto-scroll, security, virtualization, settings) needed to ship a working chat client without re-hitting known bugs. No version bump implied; README/topic-index sync if topics are added.

> Naming note: all type names below are neutral placeholders (`ChatViewModel`, `IChatClientFactory`, `MarkdownPresenter`, `HuggingRichTextBox`, `EnterCommandBehavior`, `Provider`, `ChatSettings`, …). Public framework/library names (`Markdig`, `Microsoft.Extensions.AI`, `RichTextBox`, `FlowDocument`, `FormattedText`, `OpenAI`, `OllamaSharp`, `Anthropic.SDK`, `Google.GenAI`, `SocketsHttpHandler`, `Microsoft.Xaml.Behaviors`, `Wpf.Ui`, `ModelContextProtocol`) are technical context.

---

## 0. Summary (priority)

| ID | Kind | Target | Priority | One-liner |
|----|------|--------|----------|-----------|
| 1 | New knowledge | knowledge/rendering-markdown-in-wpf/TOPIC.md | High | Convert Markdig AST → WPF; never `ToString()` an unhandled node (it emits the type name) |
| 2 | New knowledge | knowledge/displaying-selectable-rich-text-in-wpf/TOPIC.md | High | Selectable rich text needs `FlowDocument` + read-only `RichTextBox` (`TextBlock` can't select) |
| 3 | Augment (1) | knowledge/rendering-markdown-in-wpf/TOPIC.md | Medium | `Block`/`Inline` collide (Markdig vs `System.Windows.Documents`) → alias both |
| 4 | Augment (2) | knowledge/displaying-selectable-rich-text-in-wpf/TOPIC.md | High | Read-only `RichTextBox` is width-greedy & can't self-measure → compute natural width with `FormattedText`, force via `MeasureOverride` |
| 5 | Augment (2) | knowledge/displaying-selectable-rich-text-in-wpf/TOPIC.md | Medium | `RichTextBox` swallows `MouseWheel` in an outer `ScrollViewer` → re-raise on parent |
| 6 | New knowledge | knowledge/styling-chat-bubbles-in-wpf/TOPIC.md | High | Role-differentiated bubbles (color/alignment/margin) — base values MUST be Style Setters or the role `DataTrigger` can't override (local-value precedence) |
| 7 | New knowledge | knowledge/hosting-extensions-ai-chatclient-in-wpf-mvvm/TOPIC.md | High | Marshal streamed `IChatClient` updates to the Dispatcher; let `FunctionInvokingChatClient` run the tool loop; `yield` is illegal in `catch` |
| 8 | New knowledge | knowledge/sharing-httpclient-across-llm-sdks/TOPIC.md | Medium | One shared `SocketsHttpHandler`; each LLM SDK injects `HttpClient` differently |
| 9 | New skill + scaffolder | skills/submitting-on-enter-in-textbox/SKILL.md + make-wpf-behavior | Medium | Enter=submit / Shift+Enter=newline via attached `Behavior<TextBox>` (IME-safe), not code-behind |
| 10 | New knowledge | knowledge/consuming-mcp-tools-in-extensions-ai/TOPIC.md | High | Get tools via `McpClient.CreateAsync` (there is **no** `McpClientFactory`) + `StdioClientTransport`; dispose the stdio client off-Dispatcher on exit |
| 11 | Augment (8) | knowledge/sharing-httpclient-across-llm-sdks/TOPIC.md | High | The shared `SocketsHttpHandler` only shares if the factory is a DI **singleton** |
| 12 | Augment (7) | knowledge/hosting-extensions-ai-chatclient-in-wpf-mvvm/TOPIC.md | High | A mock `IChatClient` (scripted updates) lets you build/test the whole chat UI with no provider/key |
| 13 | New knowledge | knowledge/storing-api-keys-and-binding-passwordbox-in-wpf/TOPIC.md | High | Keys → OS credential store, not config; `PasswordBox.Password` is **not** a DP → attached behavior / kit control |
| 14 | Augment (1,2) | knowledge/rendering-markdown-in-wpf/TOPIC.md (security note) | Med-High | Validate Hyperlink scheme (http/https only) before launching untrusted model links |
| 15 | Augment (7) | knowledge/hosting-extensions-ai-chatclient-in-wpf-mvvm/TOPIC.md | Medium | Stop = cancel CTS; re-entrancy guard; treat `OperationCanceledException` as a normal stop, not an error |
| 16 | New skill + scaffolder | skills/auto-scrolling-to-latest-in-wpf/SKILL.md + make-wpf-behavior | High | Pin-to-bottom-unless-scrolled-up `ScrollViewer` behavior for a streaming chat/log |
| 17 | Augment | virtualizing-wpf-ui | Medium | Variable-height selectable `RichTextBox` bubbles + virtualization recycling caveats |
| 18 | Augment (1) | knowledge/rendering-markdown-in-wpf/TOPIC.md | Medium | State the supported markdown subset; tables flatten unless you add a `Table` mapping |
| 19 | New knowledge | knowledge/building-a-provider-settings-panel/TOPIC.md | Medium | Provider/model/BaseURL/key settings with per-provider conditional fields & placeholders |
| 20 | Augment (2) | knowledge/displaying-selectable-rich-text-in-wpf/TOPIC.md | Low-Med | Throttle/debounce the full `FlowDocument` rebuild during fast streaming |

### Scaffolder candidates (`make-wpf-*`)

Because this doc seeds generators, the items below ship **complete, emit-ready skeletons** — the natural seeds:

| Generator | Emits | From items |
|-----------|-------|------------|
| `make-wpf-markdown-presenter <Name>` | a `ContentControl` with a bindable `Markdown` DP that parses Markdig → `FlowDocument` → read-only `RichTextBox`, including the `HuggingRichTextBox` subclass and the block/inline builders | 1, 2, 3, 4, 5 |
| `make-wpf-behavior <Name>` | a `Behavior<T>` shell with a `Command`/`CommandParameter` DP pair + `OnAttached`/`OnDetaching` wiring (basis for Enter-to-send, stick-to-bottom, PasswordBox-bind) | 9, 16, 13 |
| `make-wpf-chat-bubble-template` | the role `DataTemplate` + `Style`/`DataTrigger` (color / alignment / margin) for an `ItemsControl` of chat turns | 6 |
| `make-wpf-chatclient-factory` | the singleton `IChatClientFactory` with a shared `SocketsHttpHandler` and per-SDK construction, plus a `MockChatClient` | 8, 11, 12 |
| `make-wpf-chat-orchestrator` | the `IAsyncEnumerable<ChatEvent>` stream decomposition (Dispatcher-marshaled, `UseFunctionInvocation`, cancellation) and the MCP tool acquisition | 7, 10, 15 |

---

## 1. Convert Markdig AST → WPF without leaking type names

### Phenomenon and causality
- **Phenomenon**: A markdown viewer converting a Markdig `MarkdownDocument` to WPF shows literal strings like `Markdig.Syntax.QuoteBlock` or `Markdig.Syntax.Inlines.LineBreak` instead of content, for any construct the converter doesn't explicitly handle (quotes, line breaks, tables, thematic breaks…).
- **Cause**: Markdig AST nodes don't override `ToString()`; the default returns the type name. A catch-all arm `_ => new TextBlock { Text = node.ToString() }` (block) or `target.Add(new Run(inline.ToString()))` (inline) renders that type name.
- **Effect**: Correct only for explicitly-handled types; every other construct leaks an internal type name. Recurs each time real model output uses a new markdown feature.

The robust dispatcher recurses containers, degrades leaves, and skips the rest — it never `ToString()`s a node:

```csharp
// `FlowBlock` = System.Windows.Documents.Block (see item 3 for the alias).
private FlowBlock? RenderBlock(MarkdigBlock block) => block switch
{
    FencedCodeBlock c   => BuildCodeBlock(c.Lines.ToString()),
    CodeBlock c         => BuildCodeBlock(c.Lines.ToString()),
    HeadingBlock h      => BuildHeading(h),
    ListBlock l         => BuildList(l),
    QuoteBlock q        => BuildQuote(q),
    ThematicBreakBlock  => BuildThematicBreak(),
    ParagraphBlock p    => BuildParagraph(p),
    ContainerBlock c    => BuildContainerFallback(c), // recurse children into a Section
    LeafBlock leaf      => BuildLeafFallback(leaf),   // render its inlines/lines
    _ => null,                                        // never emit a type name
};
```

Leaf/heading/quote/list builders (FlowDocument blocks):

```csharp
private Paragraph BuildParagraph(ParagraphBlock p)
{
    var para = new Paragraph { Margin = new Thickness(0, 0, 0, 6) };
    AppendInlines(para.Inlines, p.Inline);
    return para;
}

private Paragraph BuildHeading(HeadingBlock h)
{
    var para = new Paragraph
    {
        FontWeight = FontWeights.Bold,
        FontSize = h.Level switch { 1 => 20, 2 => 17, 3 => 15, _ => 14 },
        Margin = new Thickness(0, 0, 0, 6)
    };
    AppendInlines(para.Inlines, h.Inline);
    return para;
}

private Section BuildQuote(QuoteBlock quote)
{
    var section = new Section
    {
        BorderThickness = new Thickness(3, 0, 0, 0),   // left accent rule
        Padding = new Thickness(8, 0, 0, 0),
        Margin = new Thickness(0, 0, 0, 6)
    };
    section.SetResourceReference(TextElement.ForegroundProperty, "TextFillColorSecondaryBrush");
    section.SetResourceReference(FlowBlock.BorderBrushProperty, "TextFillColorSecondaryBrush");
    foreach (MarkdigBlock child in quote)
    {
        FlowBlock? r = RenderBlock(child);
        if (r is not null) { section.Blocks.Add(r); }
    }
    return section;
}

private List BuildList(ListBlock list)
{
    var result = new List
    {
        MarkerStyle = list.IsOrdered ? TextMarkerStyle.Decimal : TextMarkerStyle.Disc,
        Margin = new Thickness(0, 0, 0, 6),
        Padding = new Thickness(16, 0, 0, 0)
    };
    foreach (MarkdigBlock item in list)
    {
        var li = new ListItem();
        if (item is ListItemBlock ib)
        {
            foreach (MarkdigBlock child in ib)
            {
                FlowBlock? r = RenderBlock(child);
                if (r is not null) { li.Blocks.Add(r); }
            }
        }
        if (li.Blocks.Count == 0) { li.Blocks.Add(new Paragraph(new Run(item.ToString() ?? string.Empty))); }
        result.ListItems.Add(li);
    }
    return result;
}

// Generic fallbacks — the anti-type-name-leak core:
private Section BuildContainerFallback(ContainerBlock container)
{
    var s = new Section();
    foreach (MarkdigBlock child in container)
    {
        FlowBlock? r = RenderBlock(child);
        if (r is not null) { s.Blocks.Add(r); }
    }
    return s;
}
private Paragraph BuildLeafFallback(LeafBlock leaf)
{
    var p = new Paragraph { Margin = new Thickness(0, 0, 0, 6) };
    if (leaf.Inline is not null) { AppendInlines(p.Inlines, leaf.Inline); }
    else { p.Inlines.Add(new Run(leaf.Lines.ToString())); }
    return p;
}
```

Inline mapping (note the same anti-leak rule on the inline catch-all — recurse a `ContainerInline`, otherwise drop):

```csharp
private void AppendInlines(InlineCollection target, ContainerInline? container)
{
    if (container is null) { return; }
    foreach (MarkdigInline inline in container)
    {
        switch (inline)
        {
            case LiteralInline lit:
                target.Add(new Run(lit.Content.ToString()));
                break;
            case EmphasisInline em:
                Span span;
                if (em.DelimiterCount >= 2) { span = new Bold(); } else { span = new Italic(); }
                AppendInlines(span.Inlines, em);   // nested formatting preserved
                target.Add(span);
                break;
            case CodeInline code:
                target.Add(new Run(code.Content) { FontFamily = new FontFamily("Consolas") });
                break;
            case LinkInline link:
                var h = new Hyperlink { NavigateUri = SafeUri(link.Url) };
                AppendInlines(h.Inlines, link);
                h.RequestNavigate += (s, e) => { OpenInBrowser(e.Uri); e.Handled = true; };
                target.Add(h);
                break;
            case LineBreakInline:
                target.Add(new LineBreak());
                break;
            default:
                if (inline is ContainerInline nested) { AppendInlines(target, nested); }
                break; // never add inline.ToString()
        }
    }
}
```

### Proposal (concrete change)
Add knowledge topic `rendering-markdown-in-wpf` with the dispatcher + the builder helpers above, stating the rule: *unhandled Markdig node → recurse (Container), degrade to inlines/lines (Leaf), or skip; never `ToString()`*.

### Adjacent skill boundaries / cross-links
Cross-link item 2 (the FlowDocument host) and item 3 (the `Block` alias used in the same file).

---

## 2. Selectable rich text = `FlowDocument` hosted in a read-only `RichTextBox`

### Phenomenon and causality
- **Phenomenon**: Chat content rendered as a `TextBlock`/`Run` tree shows rich formatting but cannot be drag-selected or copied. There is no `IsTextSelectionEnabled` to flip.
- **Cause**: WPF `TextBlock` exposes no text-selection API (unlike the WinUI control of the same name). Drag-select + `Ctrl+C` over mixed formatted content is only available from a `FlowDocument` in a read-only `RichTextBox` (or `FlowDocumentScrollViewer` with `IsSelectionEnabled`).
- **Effect**: A "let the user select/copy the answer" requirement forces the renderer from a `UIElement` tree to a `FlowDocument` (`Paragraph`/`Run`/`Bold`/`Italic`/`Hyperlink`/`List`/`Section`), hosted in a read-only `RichTextBox`.

The control shell — a `ContentControl` with a bindable `Markdown` dependency property whose change callback re-renders by swapping `Content`. The streaming ViewModel just appends to the bound `Markdown` string; each change re-runs `Render`. (Rebuilding the whole `FlowDocument` per update is simple and fine for an MVP — a known optimization point for very long, fast-streaming answers.)

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

    var host = new HuggingRichTextBox   // see item 4 for the subclass
    {
        Document = doc,
        IsReadOnly = true,          // selection + Ctrl+C; no editing, no caret
        IsDocumentEnabled = true,   // Hyperlink inlines stay clickable
        IsTabStop = false,
        BorderThickness = new Thickness(0),
        Background = Brushes.Transparent,
        Padding = new Thickness(0),
        MinHeight = 0,
        NaturalWidth = ComputeNaturalWidth(markdown) // item 4
    };
    host.SetResourceReference(Control.ForegroundProperty, "TextFillColorPrimaryBrush"); // see note A
    ScrollViewer.SetHorizontalScrollBarVisibility(host, ScrollBarVisibility.Disabled);  // wrap at forced width
    host.PreviewMouseWheel += ForwardWheelToParent;                                     // item 5
    Content = host;
}
```

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
- **Note B — an invalid `{ui:ThemeResource ...}` key throws at *runtime*, not build.** Referencing a non-existent theme brush (a guessed accent-brush key) compiles cleanly and then throws `XamlParseException` the instant the element is realized (first message render). Only use keys verified to exist in the kit (e.g. `SystemAccentColorPrimaryBrush`, `CardBackgroundFillColorDefaultBrush`).

### Proposal (concrete change)
Add knowledge topic `displaying-selectable-rich-text-in-wpf`: the read-only `RichTextBox`/`FlowDocument` recipe, `BlockUIContainer` for interactive sub-content (code block + Copy), and Notes A/B. Items 4 and 5 are sections of this topic.

### Adjacent skill boundaries / cross-links
Cross-link `integrating-wpfui-fluent` (theme brush keys / foreground inheritance) and item 1 (markdown source). Boundary: *display* of rich text, not editable input.

---

## 3. `Block`/`Inline` name collision between Markdig and `System.Windows.Documents`

### Phenomenon and causality
- **Phenomenon**: A file that both parses markdown and builds a `FlowDocument` fails with CS0104 ("`Block` is an ambiguous reference") on bare `Block` (and `Inline`).
- **Cause**: `Markdig.Syntax.Block` and `System.Windows.Documents.Block` are both in scope (`using Markdig.Syntax;` + `using System.Windows.Documents;`); same for `Inline`.
- **Effect**: The bare identifier is unusable; both must be aliased.

```csharp
using MarkdigBlock  = Markdig.Syntax.Block;            // markdown AST node
using MarkdigInline = Markdig.Syntax.Inlines.Inline;   // markdown AST inline
using FlowBlock     = System.Windows.Documents.Block;  // FlowDocument node
// usage: FlowBlock.BorderBrushProperty for a SetResourceReference on a quote Section
```

### Proposal (concrete change)
A short "two-`Block` aliasing" note inside `rendering-markdown-in-wpf`.

### Adjacent skill boundaries / cross-links
Belongs with items 1 and 2.

---

## 4. Content-hugging read-only `RichTextBox` (the customization that makes bubble size follow content)

### Phenomenon and causality
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

How it composes with the bubble (item 6): the bubble's asymmetric outer margin (e.g. `48` on the gutter side) caps the *available* width to `parent − 48`; `Math.Min(NaturalWidth, available)` therefore hugs short messages and wraps long ones at the gutter. `HorizontalScrollBarVisibility=Disabled` on the host makes the FlowDocument wrap at exactly the forced width rather than scroll.

### Proposal (concrete change)
Add to `displaying-selectable-rich-text-in-wpf` a "content-hugging read-only RichTextBox" section: state the two-way self-measure failure (infinite→0, finite→greedy) explicitly, then give the `HuggingRichTextBox` + external `FormattedText` recipe and the `HorizontalScrollBarVisibility=Disabled` requirement.

### Adjacent skill boundaries / cross-links
Cross-link `rendering-wpf-architecture` (Measure/Arrange — why `MeasureOverride` is the hook) and `authoring-wpf-controls` (subclass vs new control). Boundary: text-control auto-sizing, not decorative-overflow clipping.

---

## 5. Read-only `RichTextBox` swallows `MouseWheel` inside an outer `ScrollViewer`

### Phenomenon and causality
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

### Proposal (concrete change)
A "forward MouseWheel to the parent scroller" note in `displaying-selectable-rich-text-in-wpf`.

### Adjacent skill boundaries / cross-links
Cross-link `routing-wpf-events` (Preview/bubbling re-raise). Applies to any inner text control nested in a scroller.

---

## 6. Role-differentiated chat bubbles — color + alignment + margin, sized to content (local-value precedence gotcha)

### Phenomenon and causality
- **Phenomenon**: Requests (user) and responses (assistant) must be visually distinguished: opposite horizontal alignment, different background color, and an asymmetric margin so each side has a gutter; and each bubble should size to its content rather than stretch full-width. A first attempt where the bubble's `Background`/`HorizontalAlignment` were set as **inline attributes** on the `Border` produced **identical colors for user and assistant** — the role `DataTrigger` appeared to do nothing.
- **Cause**: WPF dependency-property value precedence ranks a **local value (inline attribute)** *above* a **Style trigger Setter**. So `<Border Background="..."/>` (local) cannot be overridden by a `DataTrigger` Setter. Also, a bubble hugs content only when its `HorizontalAlignment` is `Left`/`Right` (not `Stretch`) **and** its child reports a hug desired width (item 4); a stretching child or `Stretch` alignment forces full width.
- **Effect**: Role styling silently fails unless the base values are declared as Style **Setters** and the role override as a **DataTrigger Setter** (no inline attributes for the trigger-controlled properties). The asymmetric margin doubles as the role indicator *and* the max-width cap.

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

    <!-- content hugs (item 4); the 48px gutter caps max width = parent - 48 -->
    <md:MarkdownPresenter Markdown="{Binding Markdown}"/>
</Border>
```

Key rules distilled:
- **Never set a trigger-controlled property inline.** Base = Style Setter, override = Trigger Setter. (`Background`, `HorizontalAlignment`, `Margin` here.)
- **`HorizontalAlignment=Left/Right`, not `Stretch`** — this is half of what lets the bubble hug; the other half is the child's hug desired width (item 4).
- **One asymmetric margin serves two purposes**: a left/right gutter that reads as "who sent it", and a max-width cap so long messages wrap with breathing room on the opposite side.

### Proposal (concrete change)
Add knowledge topic `styling-chat-bubbles-in-wpf` (or fold into a broader chat-UI topic): the role `Style` + `DataTrigger` template above, the **local-value-precedence** rule (base-as-Setter, override-as-Trigger), and the alignment+child-hug interplay. This is a concrete, recurring instance of a precedence rule that `managing-styles-resourcedictionary` states only abstractly.

### Adjacent skill boundaries / cross-links
Cross-link `managing-styles-resourcedictionary` (value precedence, `BasedOn`) and item 4 (the child hug that the bubble depends on). Boundary: this is per-item role styling, not theme/resource organization.

---

## 7. Streaming `IChatClient`: Dispatcher-marshal updates, let the middleware run the tool loop, and don't `yield` in `catch`

### Phenomenon and causality
- **Phenomenon (threading)**: Consuming `IChatClient.GetStreamingResponseAsync(...)` with `await foreach` and writing each chunk into a bound `ObservableCollection`/property throws cross-thread or yields binding errors — the async stream can resume on a thread-pool thread, not the UI Dispatcher.
- **Phenomenon (tool loop)**: Hand-rolling model→tool-call→tool-result→model is error-prone and unnecessary.
- **Phenomenon (compiler)**: A stream-decomposition helper that tries to `yield return` a failure event from inside `try/catch` fails with CS1631 ("Cannot yield a value in the body of a catch clause").
- **Cause**: (a) async continuations aren't Dispatcher-affine; (b) `Microsoft.Extensions.AI` ships `FunctionInvokingChatClient` via `ChatClientBuilder.UseFunctionInvocation()` that runs the tool loop automatically when tools are supplied; (c) C# forbids `yield` inside `catch`.
- **Effect**: Without marshaling, the UI throws on the first chunk; without the middleware, you reimplement an agent loop; with `yield` in `catch`, it won't build.

Decompose the stream into UI-agnostic events; catch into a flag, `yield` the failure **after** the catch:

```csharp
public async IAsyncEnumerable<ChatEvent> SendAsync(
    IList<ChatMessage> history, [EnumeratorCancellation] CancellationToken ct)
{
    var options = new ChatOptions { Tools = _tools, ModelId = _settings.ModelId };
    await using var e = _client.GetStreamingResponseAsync(history, options, ct).GetAsyncEnumerator(ct);

    while (true)
    {
        ChatResponseUpdate? update = null;
        bool faulted = false; Exception? error = null;
        try
        {
            if (!await e.MoveNextAsync()) { break; }
            update = e.Current;
        }
        catch (OperationCanceledException) { yield break; }
        catch (Exception ex) { faulted = true; error = ex; }   // CS1631: cannot yield here

        if (faulted) { yield return new ChatFailed(error!); yield break; }

        if (!string.IsNullOrEmpty(update!.Text)) { yield return new ChatText(update.Text); }
        foreach (AIContent c in update.Contents)
        {
            if (c is FunctionCallContent call)  { yield return new ToolStarted(call.Name, call.Arguments); }
            if (c is FunctionResultContent res) { yield return new ToolCompleted(res.CallId, res.Result); }
        }
    }
}
```

The factory wraps the provider client once so the loop is automatic; MCP client tools implement `AIFunction` and drop straight into `ChatOptions.Tools`:

```csharp
IChatClient client = new ChatClientBuilder(inner)
    .UseFunctionInvocation(loggerFactory: null, configure: null)
    .Build();
// tools: IList<McpClientTool> from ModelContextProtocol's McpClient.ListToolsAsync() — McpClientTool : AIFunction
```

The ViewModel marshals every event onto the Dispatcher before touching bound state:

```csharp
var cts = new CancellationTokenSource(); // Stop button cancels
await foreach (ChatEvent ev in _orchestrator.SendAsync(_history, cts.Token))
{
    Application.Current.Dispatcher.Invoke(() =>
    {
        switch (ev)
        {
            case ChatText t:        _current.AppendMarkdown(t.Text); _current.IsWaiting = false; break;
            case ToolStarted s:     _current.ToolCalls.Add(new ToolCallVm(s)); break;
            case ToolCompleted d:   _current.CompleteTool(d); break;
            case ChatFailed f:      _current.AppendMarkdown($"\n\n> {f.Error.Message}"); break;
        }
    });
}
```

Notes that saved time: emit text via `update.Text` (robust across stream/SDK shapes) rather than walking `TextContent`; a "waiting …" indicator bound to `IsWaiting` (flipped false on the first `ChatText`) covers the gap before the first token.

### Proposal (concrete change)
Add knowledge topic `hosting-extensions-ai-chatclient-in-wpf-mvvm`: Dispatcher marshaling, the `UseFunctionInvocation()` "don't hand-roll the loop" rule, `ChatResponseUpdate` decomposition (`Text` + `Contents`), the `yield`-outside-`catch` restructuring, and the cancellation/`IsWaiting` patterns.

### Adjacent skill boundaries / cross-links
Cross-link `threading-wpf-dispatcher` (marshaling) and `preventing-dispatcher-deadlock` (the related shutdown trap: disposing a long-lived async transport — e.g. a child-process stdio client — during `OnExit` must not `.GetAwaiter().GetResult()` on the UI thread; use a bounded `Task.Run(() => DisposeAsync()).Wait(timeout)` off the Dispatcher).

---

## 8. One shared `SocketsHttpHandler` across LLM SDKs — each SDK injects `HttpClient` differently

### Phenomenon and causality
- **Phenomenon**: A provider-agnostic chat client that `new`s an `HttpClient` per request (per provider, per send) risks socket exhaustion under sustained use.
- **Cause**: `HttpClient`-per-operation is the classic socket-exhaustion anti-pattern; the fix is a single shared `SocketsHttpHandler` (one connection pool, `PooledConnectionLifetime` against stale DNS) with lightweight per-use `HttpClient(handler, disposeHandler: false)` wrappers. The friction: each LLM SDK exposes a *different* `HttpClient` injection point, and a chat client's `Dispose` (e.g. an orchestrator `using`) must not tear down the shared pool — hence `disposeHandler:false`.
- **Effect**: A factory that owns the shared handler and a per-SDK map of where it goes.

The full reuse-oriented factory (placeholders for project types):

```csharp
public sealed class ChatClientFactory : IChatClientFactory
{
    // One handler == one connection pool shared by every provider for the app's lifetime.
    private readonly SocketsHttpHandler _shared = new()
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(15)
    };

    // Lightweight wrapper; disposeHandler:false keeps the shared pool alive when the
    // wrapper (or a chat client that owns it) is disposed.
    private HttpClient Pooled() => new(_shared, disposeHandler: false);

    public IChatClient Create(ChatSettings s, string? apiKey)
    {
        IChatClient inner = s.Provider switch
        {
            Provider.Mock              => new MockChatClient(),                 // no HttpClient
            Provider.Ollama            => CreateOllama(s),
            Provider.OpenAI
              or Provider.AzureOpenAI
              or Provider.OpenAICompatible => CreateOpenAiFamily(s, apiKey),
            Provider.Anthropic         => CreateAnthropic(s, apiKey),
            Provider.Gemini            => CreateGemini(s, apiKey),
            _ => throw new InvalidOperationException($"Unsupported provider: {s.Provider}")
        };
        return new ChatClientBuilder(inner)
            .UseFunctionInvocation(loggerFactory: null, configure: null)
            .Build();
    }

    // OpenAI family (incl. Azure & OpenAI-compatible): handler via Transport on the options.
    private IChatClient CreateOpenAiFamily(ChatSettings s, string? apiKey)
    {
        var options = new OpenAIClientOptions
        {
            Transport = new HttpClientPipelineTransport(Pooled()) // System.ClientModel.Primitives
        };
        if (!string.IsNullOrWhiteSpace(s.BaseUrl)) { options.Endpoint = new Uri(s.BaseUrl); }
        return new OpenAIClient(new ApiKeyCredential(apiKey!), options)
            .GetChatClient(s.ModelId)
            .AsIChatClient();
    }

    // OllamaSharp: HttpClient is a direct ctor arg; it implements IChatClient explicitly.
    private IChatClient CreateOllama(ChatSettings s)
    {
        HttpClient http = Pooled();
        http.BaseAddress = new Uri(string.IsNullOrWhiteSpace(s.BaseUrl) ? "http://localhost:11434" : s.BaseUrl);
        return new OllamaApiClient(http, s.ModelId, null!); // JsonSerializerContext may be null
    }

    // Anthropic.SDK: HttpClient is the 2nd ctor arg; endpoint override via ApiUrlFormat template.
    private IChatClient CreateAnthropic(ChatSettings s, string? apiKey)
    {
        var client = new AnthropicClient(new APIAuthentication(apiKey!), Pooled(), requestInterceptor: null);
        if (!string.IsNullOrWhiteSpace(s.BaseUrl))
        {
            client.ApiUrlFormat = $"{s.BaseUrl.TrimEnd('/')}/{{0}}/{{1}}"; // "{base}/{version}/{endpoint}"
        }
        return client.Messages; // .Messages is the IChatClient
    }

    // Google.GenAI (official): HttpClient via a FACTORY on ClientOptions, not a direct param.
    private IChatClient CreateGemini(ChatSettings s, string? apiKey)
    {
        var clientOptions = new ClientOptions { HttpClientFactory = Pooled }; // Func<HttpClient>
        HttpOptions? http = string.IsNullOrWhiteSpace(s.BaseUrl) ? null : new HttpOptions { BaseUrl = s.BaseUrl };
        var client = new Client(
            enterprise: null, vertexAI: null, apiKey: apiKey,
            credential: null, project: null, location: null,
            httpOptions: http, clientOptions: clientOptions);
        return client.AsIChatClient(s.ModelId); // GoogleGenAIExtensions, in the Microsoft.Extensions.AI namespace
    }
}
```

**Every provider path reuses the same `_shared` handler.** Connection pooling lives in `SocketsHttpHandler` (the pool), not in the per-call `HttpClient` wrapper — so calling `Pooled()` fresh on each `Create()` still reuses sockets, because every wrapper wraps the one `_shared` handler. The only difference is the *injection shape*: OpenAI / Ollama / Anthropic.SDK accept an `HttpClient` **instance** (`Pooled()` invoked), while Google.GenAI accepts a `Func<HttpClient>` **factory** (`HttpClientFactory = Pooled`, a method group). The literal name `HttpClientFactory` appears only for Gemini for that reason — it is **not** a sign that the other SDKs skip reuse.

Per-SDK injection-point cheat sheet:

| SDK | Where the shared `HttpClient` goes | Endpoint override |
|-----|-----------------------------------|-------------------|
| OpenAI / Azure / compatible | `OpenAIClientOptions.Transport = new HttpClientPipelineTransport(Pooled())` | `OpenAIClientOptions.Endpoint` |
| OllamaSharp | `new OllamaApiClient(Pooled(), model, jsonCtx)` (direct ctor arg) | `HttpClient.BaseAddress` |
| Anthropic.SDK | `new AnthropicClient(auth, Pooled(), interceptor)` (2nd arg) | `client.ApiUrlFormat` template |
| Google.GenAI | `new ClientOptions { HttpClientFactory = Pooled }` (a `Func<HttpClient>`) | `HttpOptions.BaseUrl` |

Version-floor caveat: a native SDK may pin `Microsoft.Extensions.AI.Abstractions` to a higher version than other consumers (e.g. an MCP client library). The bump is safe only when those other consumers depend on a `[x, )` **floor** that accepts the higher version — verify the dependency is a floor, not an exact pin, before bumping.

### Proposal (concrete change)
Add knowledge topic `sharing-httpclient-across-llm-sdks` (or a section in item 7's topic): the shared-handler pattern, `disposeHandler:false`, the per-SDK table, and the version-floor caveat.

### Adjacent skill boundaries / cross-links
Cross-link item 7 (these clients feed the same `ChatClientBuilder`). More .NET-API than WPF, but it is the data source of the WPF chat surface; scope it to "constructing the `IChatClient` for a WPF chat app."

---

## 9. Enter=submit / Shift+Enter=newline via attached `Behavior<TextBox>` (IME-safe), not code-behind

### Phenomenon and causality
- **Phenomenon**: A multiline chat input (`AcceptsReturn=True`) needs Enter to *send* and Shift+Enter to insert a newline. Implemented in code-behind it couples to the view; a naive `KeyDown` handler also fires "send" when an IME composition is confirmed with Enter, sending a half-finished message.
- **Cause**: With `AcceptsReturn=True` the TextBox inserts a newline on Enter by default; intercepting requires `PreviewKeyDown`, and the IME-confirmation key arrives as `Key.ImeProcessed` and must be excluded. Code-behind also makes the gesture non-reusable and hard to test.
- **Effect**: Without an IME guard the composing-language user's Enter sends mid-composition; without `e.Handled = true` both send *and* newline occur. An attached `Behavior<TextBox>` (Microsoft.Xaml.Behaviors) keeps it declarative and reusable.

```csharp
public sealed class EnterCommandBehavior : Behavior<TextBox>
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(EnterCommandBehavior), new(null));
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(EnterCommandBehavior), new(null));
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    protected override void OnAttached()  => AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
    protected override void OnDetaching() => AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) { return; }
        if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) { return; } // Shift+Enter -> newline (default)
        if (e.Key == Key.ImeProcessed) { return; }                     // ignore IME candidate confirm
        if (Command?.CanExecute(CommandParameter) == true) { Command.Execute(CommandParameter); }
        e.Handled = true;                                              // suppress the newline insert
    }
}
```

```xml
<TextBox AcceptsReturn="True" MaxLines="4"
         Text="{Binding Input, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}">
    <i:Interaction.Behaviors>
        <local:EnterCommandBehavior Command="{Binding SendCommand}"/>
    </i:Interaction.Behaviors>
</TextBox>
```

### Proposal (concrete change)
Add a skill `submitting-on-enter-in-textbox` with the behavior above, **and** a `make-wpf-behavior <Name>` scaffolder that emits a `Microsoft.Xaml.Behaviors` `Behavior<T>` skeleton with a `Command`/`CommandParameter` DP pair and `OnAttached`/`OnDetaching` wiring — this Enter-to-submit case is a ready template for it.

### Adjacent skill boundaries / cross-links
Cross-link `routing-wpf-events` (`PreviewKeyDown` tunneling, `e.Handled` semantics). Boundary: input-gesture-to-command, distinct from validation or CanExecute wiring.

---

## 10. Acquiring MCP tools for `ChatOptions.Tools` (there is no `McpClientFactory`) + stdio lifecycle

### Phenomenon and causality
- **Phenomenon**: Items 7–8 assume an `IList<McpClientTool>` exists, but a new implementer doesn't know where tools come from — and naturally reaches for a non-existent `McpClientFactory`, which won't compile.
- **Cause**: The MCP .NET client entry point is the **static** `McpClient.CreateAsync(IClientTransport, McpClientOptions?, ILoggerFactory?, CancellationToken)`. `McpClient` is **abstract** (no public ctor); there is no `McpClientFactory`. A local server process is reached via `StdioClientTransport`. `ListToolsAsync()` returns `McpClientTool`s, and `McpClientTool : AIFunction`, so they drop straight into `ChatOptions.Tools`.
- **Effect**: Without the exact shape, tool wiring stalls or compiles against an imaginary API; and the stdio child process leaks (or its disposal deadlocks the UI thread) if lifecycle isn't handled.

```csharp
// Connect to a local stdio MCP server process. For a framework-dependent dll use
// Command="dotnet", Arguments=[dllPath]; for a self-contained exe, Command=exePath.
var transport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "tools",
    Command = serverCommand,
    Arguments = serverArgs,
}, loggerFactory);

// NOTE: there is NO McpClientFactory. McpClient is abstract — use the static CreateAsync.
McpClient mcp = await McpClient.CreateAsync(transport, clientOptions: null, loggerFactory: null, ct);
IList<McpClientTool> tools = await mcp.ListToolsAsync(cancellationToken: ct); // McpClientTool : AIFunction

var options = new ChatOptions { Tools = [.. tools], ModelId = modelId }; // straight into the chat call
```

Lifecycle: connect once and cache (re-connecting per send re-spawns the process); aggregate tools if several servers are connected; dispose on app exit. Disposing the stdio client ends its child process — and from `App.OnExit` (UI thread) you must **not** block sync-over-async, which deadlocks the Dispatcher:

```csharp
// App.OnExit — bounded, OFF the Dispatcher; never .GetAwaiter().GetResult()/.Wait() the async dispose on the UI thread.
Task.Run(async () => await _mcpService.DisposeAsync()).Wait(TimeSpan.FromSeconds(3));
```

### Proposal (concrete change)
Add knowledge `consuming-mcp-tools-in-extensions-ai`: the `CreateAsync` (not factory) shape, `StdioClientTransport` (dll-vs-exe command), `ListToolsAsync` → `ChatOptions.Tools`, connect-once caching, and the off-Dispatcher exit disposal. **Scaffolder**: feeds `make-wpf-chat-orchestrator`.

### Adjacent skill boundaries / cross-links
Cross-link item 7 (the tools are consumed there) and `preventing-dispatcher-deadlock` (the exit-disposal trap). Boundary: this is MCP *client* acquisition, not authoring an MCP server.

---

## 11. The shared `SocketsHttpHandler` only shares if the factory is a DI singleton

### Phenomenon and causality
- **Phenomenon**: After adopting the shared-handler pattern (item 8), connection reuse still doesn't happen — sockets churn/exhaust under sustained use — with no compile or runtime error.
- **Cause**: The handler is an instance field of the factory. Reuse depends on the **same factory instance** living for the app. Registering the factory transient/scoped creates a fresh factory (and a fresh `SocketsHttpHandler`, i.e. a fresh pool) on every resolve, silently defeating the pattern.
- **Effect**: The optimization is invisibly nullified. The fix is a lifetime contract, not code: register the factory as a **singleton**.

```csharp
// REQUIRED: one factory instance for the app's lifetime, so _shared is truly shared.
container.RegisterSingleton<IChatClientFactory, ChatClientFactory>();
// Transient/Scoped => new SocketsHttpHandler per resolve => no shared pool.

// Per-send you may still Create() a fresh IChatClient wrapper and dispose it (cheap);
// disposeHandler:false (item 8) keeps the singleton-owned pool alive across sends.
```

### Proposal (concrete change)
A prominent "register the factory as a singleton" callout in `sharing-httpclient-across-llm-sdks`, stating that the lifetime is part of the correctness of the pattern.

### Adjacent skill boundaries / cross-links
Cross-link `configuring-dependency-injection` (lifetimes) and item 8.

---

## 12. A mock `IChatClient` to build/test the chat UI with no provider or key

### Phenomenon and causality
- **Phenomenon**: The chat UI (streaming text, bubbles, markdown, tool display) can't be developed or smoke-tested until a real provider + API key are available.
- **Cause**: `IChatClient` is a small interface; a fake that yields scripted `ChatResponseUpdate`s drives the entire orchestrator + UI path with no network and no key.
- **Effect**: Without it, all UI work is blocked on account procurement; with it, the whole surface (including streaming cadence) is exercisable offline.

```csharp
internal sealed class MockChatClient : IChatClient
{
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        const string reply = "This is a **mock** streamed reply.\n\n```csharp\nvar x = 1;\n```";
        foreach (string token in reply.Split(' '))
        {
            ct.ThrowIfCancellationRequested();
            yield return new ChatResponseUpdate(ChatRole.Assistant, token + " ");
            await Task.Delay(40, ct); // simulate token cadence
        }
    }

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages,
        ChatOptions? options, CancellationToken ct = default)
        => Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "mock")));

    public object? GetService(Type serviceType, object? serviceKey = null) => null;
    public void Dispose() { }
}
```

Pair it with a `Provider.Mock` that **skips MCP connection** (empty tools) so the UI runs with no server at all.

### Proposal (concrete change)
A "mock `IChatClient` for account-less development" section in `hosting-extensions-ai-chatclient-in-wpf-mvvm`, plus the mock branch in the factory. **Scaffolder**: `make-wpf-chatclient-factory` emits this mock alongside the real providers.

### Adjacent skill boundaries / cross-links
Cross-link item 7 (it satisfies the same streaming contract) and `testing-wpf-viewmodels` (the same fake feeds ViewModel tests).

---

## 13. API keys → OS credential store, not config; and `PasswordBox.Password` is not a DependencyProperty

### Phenomenon and causality
- **Phenomenon (storage)**: Putting the API key in app settings/config writes it to disk in plaintext.
- **Phenomenon (binding)**: Binding a `PasswordBox` for key entry with `{Binding}` silently does nothing.
- **Cause**: (storage) config files are plaintext; secrets belong in the OS credential store (e.g. Windows Credential Manager). (binding) `PasswordBox.Password` is deliberately **not** a `DependencyProperty` (to avoid the secret living in the binding/DP system), so it can't be a binding target — you need an attached behavior or a kit control that exposes a bindable password.
- **Effect**: Plaintext key leakage; or a non-functional key field.

```csharp
// Storage: keyed per provider, never settings.json.
public interface IApiKeyStore
{
    void Save(string key, string secret);  // e.g. Win32 CredWrite
    string? TryGet(string key);             // CredRead
    void Delete(string key);                // CredDelete
}

// Binding: PasswordBox.Password is NOT a DP. Attached behavior to surface it (one-way out):
public static class PasswordBoxAssistant
{
    public static readonly DependencyProperty BoundPasswordProperty = DependencyProperty.RegisterAttached(
        "BoundPassword", typeof(string), typeof(PasswordBoxAssistant),
        new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged));
    public static string GetBoundPassword(DependencyObject o) => (string)o.GetValue(BoundPasswordProperty);
    public static void SetBoundPassword(DependencyObject o, string v) => o.SetValue(BoundPasswordProperty, v);

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox pb) { return; }
        pb.PasswordChanged -= Handler; // idempotent re-attach (guards re-entrancy)
        pb.PasswordChanged += Handler;
        static void Handler(object s, RoutedEventArgs _)
        {
            var box = (PasswordBox)s;
            SetBoundPassword(box, box.Password); // push entered value back to the bound property
        }
    }
}
```
> If the UI kit provides a `PasswordBox` whose `Password` *is* bindable, prefer it and skip the behavior — but never round-trip the secret through `settings.json`.

### Proposal (concrete change)
Add knowledge `storing-api-keys-and-binding-passwordbox-in-wpf`: the OS-credential-store contract + the `PasswordBox`-is-not-a-DP fact and the attached-behavior (or kit-control) options. **Scaffolder**: the behavior is a `make-wpf-behavior` instance.

### Adjacent skill boundaries / cross-links
Cross-link `implementing-wpf-validation` (form input) and item 19 (the settings panel that hosts the key field).

---

## 14. Validate Hyperlink scheme before launching links from untrusted model output

### Phenomenon and causality
- **Phenomenon**: Markdown links produced by the model are opened on click via `Process.Start(... UseShellExecute = true)`.
- **Cause**: LLM output is untrusted; shell-execute honors any registered URI scheme, so a crafted link could launch an unintended handler.
- **Effect**: Potential launch of arbitrary URI schemes from generated content. Restrict to an http/https allow-list.

```csharp
private static void OpenInBrowser(Uri? uri)
{
    if (uri is null) { return; }
    if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) { return; } // allow-list
    try { Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true }); }
    catch { /* a navigation failure must not crash the chat */ }
}
// SafeUri (item 1) already rejects non-absolute URLs; combine both: absolute + http/https only.
```

### Proposal (concrete change)
A security note in `rendering-markdown-in-wpf` (and the selectable-text topic): validate Hyperlink scheme (http/https only) before launching, because the link text is untrusted model output.

### Adjacent skill boundaries / cross-links
Cross-link items 1 and 2 (where Hyperlinks are built).

---

## 15. Stop = cancel the stream; guard re-entrancy; don't render cancellation as an error

### Phenomenon and causality
- **Phenomenon**: A Stop button must cancel an in-flight response; pressing Enter again mid-stream starts an overlapping send and corrupts the turn list; and cancelling shouldn't paint a scary error bubble.
- **Cause**: Streaming needs a per-send `CancellationTokenSource`, a guard on Send `CanExecute` while streaming, and `OperationCanceledException` treated as a normal stop.
- **Effect**: Without guards: overlapping streams, duplicated/garbled turns, or an error message on a user-initiated Stop.

```csharp
private CancellationTokenSource? _cts;
public bool IsStreaming { get; private set; }

private async Task SendAsync()
{
    if (IsStreaming || string.IsNullOrWhiteSpace(Input)) { return; } // re-entrancy + empty guard
    IsStreaming = true;
    _cts = new CancellationTokenSource();
    RaiseCommandStates(); // Send disabled, Stop enabled
    try
    {
        await foreach (ChatEvent ev in _orchestrator.SendAsync(_history, _cts.Token))
        {
            Application.Current.Dispatcher.Invoke(() => Apply(ev)); // item 7
        }
    }
    catch (OperationCanceledException) { /* user pressed Stop — normal, not an error */ }
    finally
    {
        IsStreaming = false;
        _cts.Dispose(); _cts = null;
        RaiseCommandStates();
    }
}

private void Stop() => _cts?.Cancel();
// SendCommand.CanExecute => !IsStreaming && !string.IsNullOrWhiteSpace(Input)
// StopCommand.CanExecute => IsStreaming
```

### Proposal (concrete change)
A "cancellation, Stop, and re-entrancy" section in `hosting-extensions-ai-chatclient-in-wpf-mvvm`.

### Adjacent skill boundaries / cross-links
Cross-link item 7 (the stream it cancels) and item 9 (the Enter gesture whose `CanExecute` it gates).

---

## 16. Pin-to-bottom (unless the user scrolled up) for a streaming chat/log `ScrollViewer`

### Phenomenon and causality
- **Phenomenon**: As tokens stream and turns are appended, the view must follow the newest content; but a naive "always `ScrollToEnd`" yanks the view back down while the user is scrolling up to read history.
- **Cause**: An `ItemsControl`/`ScrollViewer` does not auto-follow growing content, and unconditional auto-scroll fights manual scrolling. The fix distinguishes *content grew* (extent change) from *user moved* (offset change) and only follows when the user was already pinned to the bottom.
- **Effect**: New content lands off-screen, or the view fights the reader. The behavior below keeps both correct.

```csharp
public sealed class StickToBottomBehavior : Behavior<ScrollViewer>
{
    private bool _pinned = true;

    protected override void OnAttached()  => AssociatedObject.ScrollChanged += OnScrollChanged;
    protected override void OnDetaching() => AssociatedObject.ScrollChanged -= OnScrollChanged;

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var sv = AssociatedObject;
        if (e.ExtentHeightChange == 0)
        {
            // user moved (no content growth): recompute whether we're at the bottom
            _pinned = sv.VerticalOffset >= sv.ScrollableHeight - 1.0;
        }
        else if (_pinned)
        {
            // content grew while pinned: follow it
            sv.ScrollToVerticalOffset(sv.ScrollableHeight);
        }
    }
}
```
```xml
<ScrollViewer VerticalScrollBarVisibility="Auto">
    <i:Interaction.Behaviors><local:StickToBottomBehavior/></i:Interaction.Behaviors>
    <ItemsControl ItemsSource="{Binding Turns}"/>
</ScrollViewer>
```

### Proposal (concrete change)
Add skill `auto-scrolling-to-latest-in-wpf` (chat/log lists) with the behavior above. **Scaffolder**: another `make-wpf-behavior` instance.

### Adjacent skill boundaries / cross-links
Cross-link `routing-wpf-events` and item 7 (the streaming that drives content growth).

---

## 17. Virtualization vs variable-height, selectable `RichTextBox` bubbles

### Phenomenon and causality
- **Phenomenon**: A long conversation (hundreds of turns), each a `RichTextBox` bubble, grows memory and slows layout; turning on virtualization then interacts badly with variable item height and per-bubble selection/scroll state.
- **Cause**: A plain `ItemsControl` does not virtualize; a virtualizing host recycles containers (`VirtualizationMode=Recycling`), so a bubble's transient visual state is reused for a different turn; and a `RichTextBox` is a comparatively heavy element per item.
- **Effect**: Either no virtualization (memory/perf) or recycled containers that drop per-bubble state.

Guidance:
- Use a virtualizing host for long histories (`VirtualizingStackPanel.IsVirtualizing="True"`, `VirtualizationMode="Recycling"`, `ScrollViewer.CanContentScroll="True"`).
- Keep all per-turn state in the turn ViewModel (never in the visual), since containers are recycled and re-bound.
- Because `RichTextBox` is heavy, consider capping retained turns, or rendering off-screen turns with a lighter read-only representation and promoting to `RichTextBox` only when realized.
- Note the tension with item 16: `CanContentScroll="True"` (item-based scrolling) changes how "at the bottom" is measured.

### Proposal (concrete change)
Augment `virtualizing-wpf-ui` with a "variable-height, selectable chat bubbles" caveat section.

### Adjacent skill boundaries / cross-links
Cross-link items 2/4 (the heavy bubble) and 16 (scroll interaction).

---

## 18. State the supported markdown subset; tables flatten unless mapped

### Phenomenon and causality
- **Phenomenon**: `UseAdvancedExtensions()` parses pipe tables (and footnotes, task lists, etc.), but the renderer in items 1–2 maps an unhandled `Table` to the generic `ContainerBlock` fallback → it renders as flat cell text, not a grid. Implementers may report this as a bug.
- **Cause**: The renderer maps only the block types it explicitly handles; advanced constructs hit the safe (no-type-name) fallback, which flattens to text.
- **Effect**: Tables/footnotes render as plain text — acceptable if documented, surprising if not.

```csharp
// Document the supported set, and optionally add a real table mapping:
case Markdig.Extensions.Tables.Table mdTable:
    return BuildTable(mdTable); // Markdig TableRow/TableCell -> System.Windows.Documents.Table/TableRow/TableCell
// BuildTable creates a WPF Table with one TableRowGroup; each TableCell hosts Paragraph(s) via AppendInlines.
```

### Proposal (concrete change)
A "supported markdown subset + how to extend (Table → WPF `Table` mapping)" note in `rendering-markdown-in-wpf`, so the flattening is a documented boundary, not a perceived defect.

### Adjacent skill boundaries / cross-links
Cross-link item 1.

---

## 19. A provider settings panel — per-provider conditional fields and placeholders

### Phenomenon and causality
- **Phenomenon**: A real app needs a settings surface to choose provider, model id, optional base URL, system prompt, and the API key — with fields that differ per provider (base URL only meaningful for some; example model ids differ).
- **Cause**: Live config via `IOptionsMonitor<T>`; per-provider conditional visibility and per-provider placeholder examples computed from the selected provider; the key handled by the credential store (item 13). The same value-precedence rule as item 6 applies to derived UI: raise `PropertyChanged` for derived properties (e.g. `ShowBaseUrl`) inside the provider setter so the UI re-evaluates on switch.
- **Effect**: Without structure, settings sprawl and provider-specific fields confuse or mislead.

```csharp
// VM: derived visibility + per-provider placeholder, re-raised on provider change.
public Provider Provider
{
    get => _provider;
    set { if (SetProperty(ref _provider, value)) { OnPropertyChanged(nameof(ShowBaseUrl)); } }
}
public bool ShowBaseUrl => Provider is not Provider.Mock; // or a per-provider rule
```
```xml
<!-- Base URL section only where meaningful; placeholder is a per-provider example via a converter -->
<StackPanel Visibility="{Binding ShowBaseUrl, Converter={StaticResource BoolToVisibility}}">
    <ui:TextBox Text="{Binding BaseUrl, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                PlaceholderText="{Binding Provider, Converter={StaticResource ProviderBaseUrlExampleConverter}}"/>
</StackPanel>
<!-- Model id field uses an analogous ProviderModelExampleConverter for its placeholder. -->
```

### Proposal (concrete change)
Add knowledge `building-a-provider-settings-panel`: `IOptionsMonitor<T>` consumption, per-provider conditional visibility, the converter-driven per-provider placeholders, and the credential-store key field (item 13).

### Adjacent skill boundaries / cross-links
Cross-link `using-converter-markup-extension` (the placeholder converters), `implementing-wpf-validation` (field validation), and item 13 (key storage).

---

## 20. Throttle the full `FlowDocument` rebuild during fast streaming

### Phenomenon and causality
- **Phenomenon**: Rebuilding the entire `FlowDocument` on every streamed token (item 2's `OnMarkdownChanged → Render`) causes flicker and high CPU on long, fast responses, and resets any in-progress selection.
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
Alternatives: render plain text while streaming and do a single markdown pass on completion; or keep the document and append only the delta (harder when markdown re-flows). Throttling is the lowest-risk default.

### Proposal (concrete change)
A "throttle streaming re-render" note in `displaying-selectable-rich-text-in-wpf` / the markdown-presenter scaffolder, with the debounce above as the default.

### Adjacent skill boundaries / cross-links
Cross-link `threading-wpf-dispatcher` (`DispatcherTimer`) and item 2.
