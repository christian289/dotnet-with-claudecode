# Rendering Markdown as a WPF FlowDocument (Markdig → WPF)

> Converts a Markdig `MarkdownDocument` AST into a WPF `FlowDocument` tree (Paragraph, Section, List, Run, Bold, Italic, Hyperlink) for displaying LLM or chat output as rich text. Use when rendering markdown produced by a language model, a chat assistant, or any untrusted source inside a WPF view. The governing rule: an unhandled Markdig node must recurse (Container), degrade to its inlines/lines (Leaf), or be skipped — it must NEVER be `ToString()`ed, because Markdig AST nodes return their internal type name (e.g. `Markdig.Syntax.QuoteBlock`) from the default `ToString()`. Also covers the `Block`/`Inline` name collision between Markdig and `System.Windows.Documents` (CS0104), validating Hyperlink schemes before launching untrusted links, and the supported markdown subset (tables flatten unless explicitly mapped).

> **Related foundation topics**:
> [`displaying-selectable-rich-text-in-wpf`](../displaying-selectable-rich-text-in-wpf/TOPIC.md) hosts the produced `FlowDocument` in a selectable, read-only viewer.
> [`styling-chat-bubbles-in-wpf`](../styling-chat-bubbles-in-wpf/TOPIC.md) places the rendered document inside chat bubble containers.

---

## 1. The AST → FlowDocument Dispatcher (never `ToString()` a node)

A markdown viewer that converts a Markdig `MarkdownDocument` to WPF shows literal strings like `Markdig.Syntax.QuoteBlock` or `Markdig.Syntax.Inlines.LineBreak` instead of content, for any construct the converter doesn't explicitly handle (quotes, line breaks, tables, thematic breaks…). The cause: Markdig AST nodes don't override `ToString()`; the default returns the type name. A catch-all arm `_ => new TextBlock { Text = node.ToString() }` (block) or `target.Add(new Run(inline.ToString()))` (inline) renders that type name. The result is correct only for explicitly-handled types; every other construct leaks an internal type name, and it recurs each time real model output uses a new markdown feature.

The robust dispatcher recurses containers, degrades leaves, and skips the rest — it never `ToString()`s a node:

```csharp
// `FlowBlock` = System.Windows.Documents.Block (see §4 for the alias).
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

The rule, stated plainly: an unhandled Markdig node is **recursed** (when it is a `ContainerBlock`), **degraded** to its inlines or lines (when it is a `LeafBlock`), or **skipped** (`null`) — never converted to text via `ToString()`.

---

## 2. Block / Heading / Quote / List Builders

Each explicitly-handled block type maps to a `FlowDocument` block (`Paragraph`, `Section`, or `List`). Headings become bold paragraphs sized by level; quotes become a `Section` with a left accent rule; lists recurse their items back through `RenderBlock`.

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
```

The two generic fallbacks below are the anti-type-name-leak core: a `ContainerBlock` recurses its children into a `Section`, and a `LeafBlock` degrades to its inlines (or, failing that, its raw lines).

```csharp
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

---

## 3. Inline Mapping

The inline mapping carries the same anti-leak rule on its catch-all: recurse a `ContainerInline`, otherwise drop. Literal text becomes a `Run`; emphasis becomes `Bold` (two delimiters) or `Italic` (one), with nested formatting preserved by recursing into the span; inline code uses Consolas; links become a `Hyperlink` whose navigation is routed through the scheme-validating opener in §5.

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

---

## 4. The Two-`Block` Aliasing (Markdig vs `System.Windows.Documents`)

A file that both parses markdown and builds a `FlowDocument` fails with CS0104 ("`Block` is an ambiguous reference") on bare `Block` (and `Inline`). The cause is that `Markdig.Syntax.Block` and `System.Windows.Documents.Block` are both in scope (`using Markdig.Syntax;` + `using System.Windows.Documents;`); the same applies to `Inline`. The bare identifier is unusable, so both must be aliased:

```csharp
using MarkdigBlock  = Markdig.Syntax.Block;            // markdown AST node
using MarkdigInline = Markdig.Syntax.Inlines.Inline;   // markdown AST inline
using FlowBlock     = System.Windows.Documents.Block;  // FlowDocument node
// usage: FlowBlock.BorderBrushProperty for a SetResourceReference on a quote Section
```

These aliases are the `MarkdigBlock`, `MarkdigInline`, and `FlowBlock` names used throughout the dispatcher and builders in §§1–3.

---

## 5. Hyperlink Scheme Security (untrusted model output)

Markdown links produced by the model are opened on click via `Process.Start(... UseShellExecute = true)`. Because LLM output is untrusted and shell-execute honors any registered URI scheme, a crafted link could launch an unintended handler. The fix is an http/https allow-list checked before launching:

```csharp
private static void OpenInBrowser(Uri? uri)
{
    if (uri is null) { return; }
    if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) { return; } // allow-list
    try { Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true }); }
    catch { /* a navigation failure must not crash the chat */ }
}
// SafeUri (§3) already rejects non-absolute URLs; combine both: absolute + http/https only.
```

`SafeUri` (used when constructing each `Hyperlink.NavigateUri` in §3) rejects non-absolute URLs; `OpenInBrowser` rejects non-http(s) schemes. Together they enforce: the link must be **absolute and http/https only** before it is ever launched. The same validation applies anywhere a `Hyperlink` is built from generated text — see [`displaying-selectable-rich-text-in-wpf`](../displaying-selectable-rich-text-in-wpf/TOPIC.md).

---

## 6. Supported Markdown Subset (tables flatten unless mapped)

`UseAdvancedExtensions()` parses pipe tables (and footnotes, task lists, etc.), but the renderer in §§1–3 maps only the block types it explicitly handles. An unhandled `Table` falls through to the generic `ContainerBlock` fallback, so it renders as flat cell text rather than a grid. This is acceptable **if documented** and surprising **if not** — implementers may otherwise report the flattening as a bug.

To promote a table to a real WPF grid, add an explicit arm to `RenderBlock` that maps the Markdig table to `System.Windows.Documents.Table`:

```csharp
// Document the supported set, and optionally add a real table mapping:
case Markdig.Extensions.Tables.Table mdTable:
    return BuildTable(mdTable); // Markdig TableRow/TableCell -> System.Windows.Documents.Table/TableRow/TableCell
// BuildTable creates a WPF Table with one TableRowGroup; each TableCell hosts Paragraph(s) via AppendInlines.
```

The takeaway: state the supported subset explicitly so the flattening of advanced constructs (tables, footnotes) is a documented boundary, not a perceived defect — and extend it with a `Table` mapping only when a grid layout is actually required.

---

## 7. References

- [Markdig — GitHub repository](https://github.com/xoofx/markdig)
- [FlowDocument Class — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.documents.flowdocument)
- [Flow Document Overview (WPF) — Microsoft Learn](https://learn.microsoft.com/dotnet/desktop/wpf/advanced/flow-document-overview)
- [Block Class — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.documents.block)
- [Hyperlink Class — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.windows.documents.hyperlink)
- [Uri.UriSchemeHttps Field — Microsoft Learn](https://learn.microsoft.com/dotnet/api/system.uri.urischemehttps)

---

## 8. Related Topics

- [`displaying-selectable-rich-text-in-wpf`](../displaying-selectable-rich-text-in-wpf/TOPIC.md) — hosts the produced `FlowDocument` in a selectable, read-only viewer; shares the Hyperlink scheme-validation rule.
- [`styling-chat-bubbles-in-wpf`](../styling-chat-bubbles-in-wpf/TOPIC.md) — places the rendered markdown inside chat bubble containers.
- [`integrating-wpfui-fluent`](../integrating-wpfui-fluent/TOPIC.md) — supplies the `TextFillColorSecondaryBrush` and similar dynamic resource keys referenced by the quote `Section`.
