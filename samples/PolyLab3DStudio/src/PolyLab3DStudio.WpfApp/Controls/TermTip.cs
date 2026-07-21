using System.Windows.Documents;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using PolyLab3DStudio.Core;
using PolyLab3DStudio.ViewModels;

namespace PolyLab3DStudio.Controls;

/// <summary>
/// Inline glossary term (design's <c>term-tip</c> element): dotted accent
/// underline, a dark hover card (word + English + short description +
/// dictionary link), and click navigation to the 3D 사전 pre-searched with
/// the term — the WPF port of the design's <c>3D 사전.dc.html?q=</c> link.
/// The card is a Popup anchored directly below the word (not a ToolTip), so
/// the mouse can travel into it and the "3D 사전에서 자세히 →" line is
/// genuinely clickable.
/// </summary>
public sealed class TermTip : Hyperlink
{
    private const int CloseGraceMs = 250; // time allowed to move from the word into the card
    private const double CardGap = 4;     // extra gap between the word's baseline box and the card

    public static readonly DependencyProperty WordProperty = DependencyProperty.Register(
        nameof(Word), typeof(string), typeof(TermTip), new PropertyMetadata(null));

    private GlossaryTerm? _term;
    private Popup? _popup;
    private Border? _card;
    private DispatcherTimer? _closeTimer;
    private Rect _anchor;

    public TermTip()
    {
        OverridesDefaultStyle = true;
        FocusVisualStyle = null;
        Cursor = Cursors.Help;
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
    }

    protected override void OnClick()
    {
        base.OnClick();
        NavigateToDictionary();
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        OpenCard();
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        ScheduleClose();
    }

    /// <summary>Same fallback order as the design's term-tip.js lookup.</summary>
    private static GlossaryTerm? Lookup(string word) =>
        GlossaryCatalog.All.FirstOrDefault(t => t.Word == word) ??
        GlossaryCatalog.All.FirstOrDefault(t => t.Word.Contains(word) || word.Contains(t.Word));

    private void NavigateToDictionary()
    {
        if (_popup is { } popup)
        {
            popup.IsOpen = false;
        }

        if (_term is { } term &&
            Application.Current.MainWindow?.DataContext is ShellViewModel shell)
        {
            shell.GoDictSearch(term.Word);
        }
    }

    private void OpenCard()
    {
        if (_term is not { } term)
        {
            return;
        }

        _closeTimer?.Stop();

        if (_popup is null)
        {
            _card = BuildCard(term);
            _popup = new Popup
            {
                AllowsTransparency = true,
                StaysOpen = true,
                // Custom placement pins the card's LEFT edge under the word.
                // Built-in modes (Mouse/Relative/Bottom) flip horizontally when
                // Windows' right-handed menu alignment (MenuDropAlignment) is
                // on, which detaches the card from the word.
                Placement = PlacementMode.Custom,
                CustomPopupPlacementCallback = PlaceBelowWord,
                Child = _card,
            };

            if (FindHost() is { } host)
            {
                _popup.PlacementTarget = host;
                host.Unloaded += (_, _) => _popup.IsOpen = false;
            }
        }

        if (!_popup.IsOpen)
        {
            // Anchor to the word itself: character rects are relative to the
            // hosting TextBlock, which is also the popup's PlacementTarget.
            Rect start = ContentStart.GetCharacterRect(LogicalDirection.Forward);
            Rect end = ContentEnd.GetCharacterRect(LogicalDirection.Backward);
            _anchor = Rect.Union(start, end);
            _popup.IsOpen = true;
        }
    }

    private CustomPopupPlacement[] PlaceBelowWord(Size popupSize, Size targetSize, Point offset)
    {
        double x = _anchor.X - (_card?.Margin.Left ?? 0);
        double y = _anchor.Bottom + CardGap;
        return
        [
            new CustomPopupPlacement(new Point(x, y), PopupPrimaryAxis.Vertical),
        ];
    }

    private void ScheduleClose()
    {
        if (_popup is not { IsOpen: true })
        {
            return;
        }

        _closeTimer ??= CreateCloseTimer();
        _closeTimer.Stop();
        _closeTimer.Start();
    }

    private DispatcherTimer CreateCloseTimer()
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(CloseGraceMs) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            if (!IsMouseOver && _card is { IsMouseOver: false } && _popup is { } popup)
            {
                popup.IsOpen = false;
            }
        };
        return timer;
    }

    private FrameworkElement? FindHost()
    {
        DependencyObject? current = this;
        while (current is TextElement element)
        {
            current = element.Parent;
        }

        return current as FrameworkElement;
    }

    private Border BuildCard(GlossaryTerm term)
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

        var card = new Border
        {
            Background = (Brush)FindResource("DarkCardBrush"),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(15, 13, 15, 13),
            MaxWidth = 320,
            Margin = new Thickness(4, 4, 18, 22),
            Cursor = Cursors.Hand,
            Child = panel,
            Effect = new DropShadowEffect
            {
                Color = (Color)FindResource("ShadowColor"),
                Opacity = 0.45,
                BlurRadius = 36,
                ShadowDepth = 14,
                Direction = 270,
            },
        };

        card.MouseEnter += (_, _) => _closeTimer?.Stop();
        card.MouseLeave += (_, _) => ScheduleClose();
        card.MouseLeftButtonUp += (_, _) => NavigateToDictionary();
        return card;
    }
}
