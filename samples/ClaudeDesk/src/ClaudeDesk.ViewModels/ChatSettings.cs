namespace ClaudeDesk.ViewModels;

public sealed record ChatSettings
{
    public Provider Provider { get; init; }
    public string ModelId { get; init; } = string.Empty;
    public string? BaseUrl { get; init; }
}
