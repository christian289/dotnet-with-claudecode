using WpfDevPackMcp.Knowledge;
using Xunit;

namespace WpfDevPackMcp.Tests;

public sealed class TopicDocReaderTests
{
    // ── ReadTitle ────────────────────────────────────────────────────────────

    [Fact]
    public void ReadTitle_H1Present_ReturnsText()
    {
        const string md = "# CommunityToolkit.Mvvm Guidelines\n\n> Summary.\n\nbody";
        Assert.Equal("CommunityToolkit.Mvvm Guidelines", TopicDocReader.ReadTitle(md));
    }

    [Fact]
    public void ReadTitle_NoH1_ReturnsNull()
    {
        const string md = "## Only H2\n\nbody\n";
        Assert.Null(TopicDocReader.ReadTitle(md));
    }

    [Fact]
    public void ReadTitle_H1WithExtraSpaces_ReturnsTrimmed()
    {
        const string md = "#   Spaced Title   \nbody";
        Assert.Equal("Spaced Title", TopicDocReader.ReadTitle(md));
    }

    [Fact]
    public void ReadTitle_CrlfLineEnding_ReturnsText()
    {
        const string md = "# CRLF Title\r\n\r\n> Summary.\r\nbody";
        Assert.Equal("CRLF Title", TopicDocReader.ReadTitle(md));
    }

    // ── ReadSummary ──────────────────────────────────────────────────────────

    [Fact]
    public void ReadSummary_SingleLineBlockquote_ReturnsTrimmedText()
    {
        const string md = "# Title\n\n> This is the summary.\n\nbody";
        Assert.Equal("This is the summary.", TopicDocReader.ReadSummary(md));
    }

    [Fact]
    public void ReadSummary_MultiLineBlockquote_JoinsWithSpace()
    {
        const string md = "# Title\n\n> Line one.\n> Line two.\n\nbody";
        Assert.Equal("Line one. Line two.", TopicDocReader.ReadSummary(md));
    }

    [Fact]
    public void ReadSummary_NoBlockquote_ReturnsNull()
    {
        const string md = "# Title\n\nJust a paragraph, no blockquote.\n";
        Assert.Null(TopicDocReader.ReadSummary(md));
    }

    [Fact]
    public void ReadSummary_CrlfLineEnding_ReturnsJoined()
    {
        const string md = "# Title\r\n\r\n> First line.\r\n> Second line.\r\n\r\nbody";
        Assert.Equal("First line. Second line.", TopicDocReader.ReadSummary(md));
    }

    [Fact]
    public void ReadSummary_BlockquoteWithoutSpace_StillStripsMarker()
    {
        // '>' with no following space: still strip the '>' character only.
        const string md = "# Title\n\n>No space after marker.\n\nbody";
        Assert.Equal("No space after marker.", TopicDocReader.ReadSummary(md));
    }

    [Fact]
    public void ReadSummary_OnlyFirstContiguousBlock_Returned()
    {
        // Two separate blockquote blocks; only the first should be captured.
        const string md = "# Title\n\n> First block.\n\nnon-quote line\n\n> Second block.\n";
        Assert.Equal("First block.", TopicDocReader.ReadSummary(md));
    }
}
