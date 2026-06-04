namespace WpfDevPackMcp.Knowledge;

/// <summary>
/// Reads the title and summary from a plain-markdown TOPIC.md file
/// (no YAML frontmatter). The file format is:
///
///   # &lt;Title&gt;
///
///   &gt; Summary line one.
///   &gt; Summary line two.
///
///   ...body...
/// </summary>
public static class TopicDocReader
{
    /// <summary>
    /// Returns the text of the first line that starts with "# " (trimmed),
    /// or null if no such line exists.
    /// </summary>
    public static string? ReadTitle(string markdown)
    {
        foreach (var raw in markdown.Split('\n'))
        {
            var line = raw.TrimEnd('\r');
            if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                return line[2..].Trim();
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the first contiguous block of lines that start with "&gt;" in
    /// the document. Each line has its leading "&gt;" stripped, along with one
    /// optional following space, and the lines are joined with a single space.
    /// Returns null if no blockquote block is present.
    /// Handles both LF and CRLF line endings.
    /// </summary>
    public static string? ReadSummary(string markdown)
    {
        var parts = new List<string>();
        var inBlock = false;

        foreach (var raw in markdown.Split('\n'))
        {
            var line = raw.TrimEnd('\r');
            if (line.StartsWith(">", StringComparison.Ordinal))
            {
                // Strip leading '>' and one optional space.
                var content = line.Length > 1 && line[1] == ' '
                    ? line[2..]
                    : line[1..];
                parts.Add(content);
                inBlock = true;
            }
            else if (inBlock)
            {
                // First non-blockquote line after the block ends it.
                break;
            }
        }

        if (parts.Count == 0)
        {
            return null;
        }

        return string.Join(" ", parts).Trim();
    }
}
