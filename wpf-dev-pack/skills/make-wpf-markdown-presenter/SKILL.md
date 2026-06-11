---
description: "Generates a WPF MarkdownPresenter: a ContentControl with a bindable Markdown dependency property that parses Markdig into a FlowDocument and hosts it in a selectable, content-hugging read-only RichTextBox. Use when rendering LLM/chat markdown output as selectable rich text in WPF, or when building a chat/markdown view. Usage: /wpf-dev-pack:make-wpf-markdown-presenter <Name>"
argument-hint: [Name]
---

# WPF Markdown Presenter Generator

**If `$0` is empty, use the AskUserQuestion tool to ask: "Enter the presenter control name (e.g., MarkdownPresenter, ChatMarkdownView)". Do NOT proceed until a valid name is provided. Use the response as the control name for all subsequent steps.**

Generate a `$0` control: a `ContentControl` with a bindable `Markdown` dependency property that parses markdown with Markdig, builds a WPF `FlowDocument`, and hosts it in a selectable, content-hugging read-only `RichTextBox`.

- Replace `{Namespace}` with the project's root namespace detected from csproj or existing code.
- Replace `{Project}` with the target project path (a WPF Application or WPF control library — must have `UseWPF`).

> **The full rationale** for every choice below is in two knowledge topics —
> fetch with `WpfDevPackMcp get_wpf_topic`: `rendering-markdown-in-wpf` (the
> AST→FlowDocument dispatcher, the "never `ToString()` a node" rule, the
> `Block`/`Inline` aliasing, Hyperlink scheme validation) and
> `displaying-selectable-rich-text-in-wpf` (the read-only RichTextBox host, the
> content-hugging `HuggingRichTextBox`, `MouseWheel` forwarding, streaming throttle).

## Usage

```bash
/wpf-dev-pack:make-wpf-markdown-presenter MarkdownPresenter
```

## Required Packages

```xml
<PackageReference Include="Markdig" Version="0.37.*" />
```

- **Markdig** is required (markdown parsing). Verify the current version and the
  exact API surface with HandMirror before emitting (`inspect_nuget_package`
  / `inspect_nuget_package_type` on `Markdig`), since the AST node type names
  drive the `switch`.
- The code uses the WPF-UI (`Wpf.Ui`) theme-brush keys `TextFillColorPrimaryBrush`
  / `TextFillColorSecondaryBrush` via `SetResourceReference`, and a
  `Wpf.Ui.Controls.Button` for the code-block Copy button. If WPF-UI is not
  installed, swap those for stock WPF (`SystemColors.ControlTextBrushKey`
  resource reference, and a plain `System.Windows.Controls.Button`). See
  `integrating-wpfui-fluent` for the brush keys.

## Generated Code

### $0.cs

The three `using`-aliases at the top are **mandatory** — `Markdig.Syntax.Block`
and `System.Windows.Documents.Block` (and the two `Inline` types) collide
(CS0104), so both must be aliased in any file that both parses markdown and
builds a `FlowDocument`.

```csharp
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

using MarkdigBlock  = Markdig.Syntax.Block;            // markdown AST node
using MarkdigInline = Markdig.Syntax.Inlines.Inline;   // markdown AST inline
using FlowBlock     = System.Windows.Documents.Block;  // FlowDocument node

namespace {Namespace}.Controls;

/// <summary>
/// A ContentControl that renders a bound Markdown string as a selectable WPF
/// FlowDocument hosted in a read-only RichTextBox. Unhandled Markdig AST nodes
/// are recursed (Container), degraded to inlines/lines (Leaf), or skipped —
/// never ToString()'d (the default ToString returns the type name).
/// </summary>
public sealed class $0 : ContentControl
{
    private static readonly MarkdownPipeline _pipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    public static readonly DependencyProperty MarkdownProperty =
        DependencyProperty.Register(nameof(Markdown), typeof(string), typeof($0),
            new PropertyMetadata(null, OnMarkdownChanged));

    public string? Markdown
    {
        get => (string?)GetValue(MarkdownProperty);
        set => SetValue(MarkdownProperty, value);
    }

    // Coalesce re-renders during fast streaming instead of rebuilding per token.
    private readonly DispatcherTimer _renderTimer =
        new() { Interval = TimeSpan.FromMilliseconds(50) }; // ~20 Hz

    public $0()
    {
        _renderTimer.Tick += (_, _) => { _renderTimer.Stop(); Render(Markdown); };
    }

    private static void OnMarkdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var self = ($0)d;
        self._renderTimer.Stop();
        self._renderTimer.Start(); // debounce: render once the burst settles
    }

    // Render: parse markdown -> build a FlowDocument -> host it in a read-only RichTextBox.
    private void Render(string? markdown)
    {
        if (string.IsNullOrEmpty(markdown)) { Content = null; return; }

        var doc = new FlowDocument { PagePadding = new Thickness(0), FontSize = 14 };
        foreach (MarkdigBlock block in Markdig.Markdown.Parse(markdown, _pipeline))
        {
            FlowBlock? b = RenderBlock(block);
            if (b is not null) { doc.Blocks.Add(b); }
        }

        var host = new HuggingRichTextBox
        {
            Document = doc,
            IsReadOnly = true,          // selection + Ctrl+C; no editing, no caret
            IsDocumentEnabled = true,   // Hyperlink inlines stay clickable
            IsTabStop = false,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Padding = new Thickness(0),
            MinHeight = 0,
            NaturalWidth = ComputeNaturalWidth(markdown)
        };
        // Code-built text elements do NOT inherit a theme foreground — set it once.
        host.SetResourceReference(Control.ForegroundProperty, "TextFillColorPrimaryBrush");
        ScrollViewer.SetHorizontalScrollBarVisibility(host, ScrollBarVisibility.Disabled); // wrap at forced width
        host.PreviewMouseWheel += ForwardWheelToParent;
        Content = host;
    }

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

    // A horizontal rule. Minimal helper — adjust styling as needed.
    private static FlowBlock BuildThematicBreak()
        => new BlockUIContainer(new Separator { Margin = new Thickness(0, 4, 0, 4) });

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
        var copy = new Button { Content = "Copy", VerticalAlignment = VerticalAlignment.Top };
        copy.Click += (_, _) => { try { Clipboard.SetText(text); } catch { /* clipboard busy */ } };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        Grid.SetColumn(box, 0); Grid.SetColumn(copy, 1);
        grid.Children.Add(box); grid.Children.Add(copy);

        var border = new Border { Padding = new Thickness(8), Margin = new Thickness(0, 4, 0, 6), Child = grid };
        return new BlockUIContainer(border) { Margin = new Thickness(0) };
    }

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

    // Natural width is measured from the source markdown lines (reliable at any constraint).
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

    // Read-only RichTextBox swallows MouseWheel; re-raise a bubbling MouseWheel on the parent.
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

    // Reject non-absolute URLs. Minimal helper.
    private static Uri? SafeUri(string? url)
        => Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ? uri : null;

    // Untrusted model output: allow http/https only before shell-executing a link.
    private static void OpenInBrowser(Uri? uri)
    {
        if (uri is null) { return; }
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) { return; } // allow-list
        try { Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true }); }
        catch { /* a navigation failure must not crash the chat */ }
    }

    // A content-hugging read-only RichTextBox: a read-only RichTextBox cannot self-measure its
    // natural width (it collapses to ~0 at an infinite constraint and is greedy at a finite one),
    // so the natural width is computed externally (ComputeNaturalWidth) and forced here.
    private sealed class HuggingRichTextBox : RichTextBox
    {
        public double NaturalWidth { get; set; }

        protected override Size MeasureOverride(Size constraint)
        {
            double width = double.IsInfinity(constraint.Width)
                ? NaturalWidth
                : Math.Min(NaturalWidth, constraint.Width); // hug short / wrap long
            Size measured = base.MeasureOverride(new Size(width, constraint.Height));
            return new Size(width, measured.Height);
        }
    }
}
```

### Optional: real table mapping

`UseAdvancedExtensions()` parses pipe tables, but an unhandled `Table` falls
through to the generic container fallback and renders as flat cell text. To
promote it to a grid, add an arm to `RenderBlock`:

```csharp
case Markdig.Extensions.Tables.Table mdTable:
    return BuildTable(mdTable); // Markdig TableRow/TableCell -> System.Windows.Documents.Table/TableRow/TableCell
// BuildTable creates a WPF Table with one TableRowGroup; each TableCell hosts Paragraph(s) via AppendInlines.
```

## XAML Usage

```xml
<Window xmlns:controls="clr-namespace:{Namespace}.Controls">
    <!-- Markdown is a string the ViewModel appends to as tokens stream in. -->
    <controls:$0 Markdown="{Binding CurrentMarkdown}" />
</Window>
```

## File Location

```
{Project}/
└── Controls/
    └── $0.cs
```

## Notes

- **Theme foreground**: a `FlowDocument`/`TextBlock` built in code does not
  inherit a theme foreground (renders black on a dark theme). The single
  `SetResourceReference(Control.ForegroundProperty, ...)` on the host fixes it;
  children inherit it.
- **Invalid theme keys throw at runtime, not build.** Referencing a non-existent
  `{ui:ThemeResource ...}`/resource key compiles and then throws
  `XamlParseException` on first render. Only use keys verified to exist in the kit.
- `SafeUri`, `BuildThematicBreak` are minimal helpers — adjust to taste.

## Related

- Knowledge (fetch via `WpfDevPackMcp get_wpf_topic`): `rendering-markdown-in-wpf`,
  `displaying-selectable-rich-text-in-wpf`, `integrating-wpfui-fluent`,
  `authoring-wpf-controls`.
- Sibling scaffolders: this presenter is one of the pieces emitted by
  `/wpf-dev-pack:make-wpf-chatclient` (the one-button full chat client).
  Pair it with `/wpf-dev-pack:make-wpf-chat-bubble-template` to place it inside
  role-differentiated bubbles.
