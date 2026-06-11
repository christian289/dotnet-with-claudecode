namespace ClaudeDesk.ViewModels;

public abstract record ChatEvent;
public sealed record ChatText(string Text) : ChatEvent;
public sealed record ToolStarted(string Name, IDictionary<string, object?>? Arguments) : ChatEvent;
public sealed record ToolCompleted(string CallId, object? Result) : ChatEvent;
public sealed record ChatFailed(Exception Error) : ChatEvent;
