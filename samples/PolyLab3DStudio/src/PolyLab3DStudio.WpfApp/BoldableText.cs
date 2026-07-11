using System.Windows.Documents;
using PolyLab3DStudio.Controls;

namespace PolyLab3DStudio;

/// <summary>
/// Attached property that renders lightweight <c>**bold**</c> markup and
/// <c>[[word]]</c> glossary term-tips (used by the guide content models)
/// into <see cref="TextBlock"/> inlines.
/// </summary>
public static class BoldableText
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
        "Text",
        typeof(string),
        typeof(BoldableText),
        new PropertyMetadata(null, OnTextChanged));

    public static string? GetText(TextBlock element) => (string?)element.GetValue(TextProperty);

    public static void SetText(TextBlock element, string? value) => element.SetValue(TextProperty, value);

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock textBlock)
        {
            return;
        }

        textBlock.Inlines.Clear();

        if (e.NewValue is not string markup || markup.Length == 0)
        {
            return;
        }

        string[] segments = markup.Split("**");

        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i].Length == 0)
            {
                continue;
            }

            // Odd segments sit between ** delimiters and render bold.
            AddWithTermTips(textBlock, segments[i], bold: i % 2 == 1);
        }
    }

    /// <summary>Splits a segment on [[word]] markers, emitting TermTip inlines.
    /// Non-bold inlines keep the inherited font weight.</summary>
    private static void AddWithTermTips(TextBlock textBlock, string segment, bool bold)
    {
        int pos = 0;

        while (pos < segment.Length)
        {
            int open = segment.IndexOf("[[", pos, StringComparison.Ordinal);
            int close = open >= 0 ? segment.IndexOf("]]", open + 2, StringComparison.Ordinal) : -1;
            if (open < 0 || close < 0)
            {
                textBlock.Inlines.Add(MakeRun(segment[pos..], bold));
                return;
            }

            if (open > pos)
            {
                textBlock.Inlines.Add(MakeRun(segment[pos..open], bold));
            }

            var tip = new TermTip { Word = segment[(open + 2)..close] };
            if (bold)
            {
                tip.FontWeight = FontWeights.Bold;
            }

            textBlock.Inlines.Add(tip);
            pos = close + 2;
        }
    }

    private static Run MakeRun(string text, bool bold) =>
        bold ? new Run(text) { FontWeight = FontWeights.Bold } : new Run(text);
}
