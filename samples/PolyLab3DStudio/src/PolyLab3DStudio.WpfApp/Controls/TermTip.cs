using System.Windows.Documents;
using PolyLab3DStudio.Core;
using PolyLab3DStudio.ViewModels;

namespace PolyLab3DStudio.Controls;

/// <summary>
/// Inline glossary term (design's <c>term-tip</c> element): dotted accent
/// underline, a dark styled tooltip (word + English + short description +
/// dictionary hint), and click navigation to the 3D 사전 pre-searched with
/// the term — the WPF port of the design's <c>3D 사전.dc.html?q=</c> link.
/// </summary>
public sealed class TermTip : Hyperlink
{
    public static readonly DependencyProperty WordProperty = DependencyProperty.Register(
        nameof(Word), typeof(string), typeof(TermTip), new PropertyMetadata(null));

    private GlossaryTerm? _term;

    public TermTip()
    {
        OverridesDefaultStyle = true;
        FocusVisualStyle = null;
        Cursor = Cursors.Help;
        ToolTipService.SetInitialShowDelay(this, 200);
        ToolTipService.SetShowDuration(this, 20000);
    }

    public string? Word
    {
        get => (string?)GetValue(WordProperty);
        set => SetValue(WordProperty, value);
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        string word = Word ?? "";
        if (Inlines.Count == 0 && word.Length > 0)
        {
            Inlines.Add(new Run(word));
        }

        TextDecorations = (TextDecorationCollection)FindResource("TermTipUnderline");

        _term = Lookup(word);
        if (_term is { } term)
        {
            ToolTip = BuildToolTip(term);
        }
    }

    protected override void OnClick()
    {
        base.OnClick();
        if (_term is { } term &&
            Application.Current.MainWindow?.DataContext is ShellViewModel shell)
        {
            shell.GoDictSearch(term.Word);
        }
    }

    /// <summary>Same fallback order as the design's term-tip.js lookup.</summary>
    private static GlossaryTerm? Lookup(string word) =>
        GlossaryCatalog.All.FirstOrDefault(t => t.Word == word) ??
        GlossaryCatalog.All.FirstOrDefault(t => t.Word.Contains(word) || word.Contains(t.Word));

    private ToolTip BuildToolTip(GlossaryTerm term)
    {
        var header = new TextBlock();
        header.Inlines.Add(new Run(term.Word)
        {
            FontSize = 14,
            FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource("SurfaceBrush"),
        });
        header.Inlines.Add(new Run("  "));
        header.Inlines.Add(new Run(term.English)
        {
            FontFamily = (FontFamily)FindResource("MonoFont"),
            FontSize = 10,
            Foreground = (Brush)FindResource("DarkMutedBrush"),
        });

        var body = new TextBlock
        {
            Text = term.Short,
            FontSize = 12.5,
            Foreground = (Brush)FindResource("DarkBodyBrush"),
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 20.6,
            LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
            Margin = new Thickness(0, 5, 0, 0),
        };

        var hint = new TextBlock
        {
            Text = "3D 사전에서 자세히 →",
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Foreground = (Brush)FindResource("YellowBrush"),
            Margin = new Thickness(0, 9, 0, 0),
        };

        var panel = new StackPanel();
        panel.Children.Add(header);
        panel.Children.Add(body);
        panel.Children.Add(hint);

        return new ToolTip
        {
            Style = (Style)FindResource("TermTipToolTip"),
            Content = panel,
        };
    }
}
