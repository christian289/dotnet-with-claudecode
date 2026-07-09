using System.Windows.Documents;

namespace PolyLab3DStudio;

/// <summary>
/// Attached property that renders lightweight <c>**bold**</c> markup
/// (used by the guide content models) into <see cref="TextBlock"/> inlines.
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
            textBlock.Inlines.Add(i % 2 == 1
                ? new Run(segments[i]) { FontWeight = FontWeights.Bold }
                : new Run(segments[i]));
        }
    }
}
