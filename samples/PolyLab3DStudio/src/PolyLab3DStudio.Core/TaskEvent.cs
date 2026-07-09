namespace PolyLab3DStudio.Core;

/// <summary>An event raised by studio interactions, matched against <see cref="TaskCheck"/>.</summary>
public sealed record TaskEvent(
    string Type,
    string? Action = null,
    ShapeKind? Shape = null,
    string? Tool = null,
    string? Mode = null,
    bool Shift = false,
    string? Key = null)
{
    public static TaskEvent ForAction(string action) => new("action", Action: action);

    public static TaskEvent ForAdd(ShapeKind shape) => new("add", Shape: shape);

    public static TaskEvent ForCount(ShapeKind shape) => new("count", Shape: shape);

    public static TaskEvent ForTool(string tool) => new("tool", Tool: tool);

    public static TaskEvent ForTransform(string? mode = null, bool shift = false) => new("transform", Mode: mode, Shift: shift);

    public static TaskEvent ForSlider(string key) => new("slider", Key: key);

    public static TaskEvent ForLight(string key) => new("light", Key: key);

    public static TaskEvent Select { get; } = new("select");

    public static TaskEvent Delete { get; } = new("delete");

    public static TaskEvent Color { get; } = new("color");
}
