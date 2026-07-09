namespace PolyLab3DStudio.Core;

/// <summary>Port of the design's matches() — decides whether an event satisfies a task check.</summary>
public static class TaskChecker
{
    /// <param name="countOf">Returns how many objects of a shape kind are currently in the scene (for "count" checks).</param>
    public static bool Matches(TaskCheck check, TaskEvent evt, Func<ShapeKind, int> countOf)
    {
        if (check.Type != evt.Type)
        {
            return false;
        }

        return check.Type switch
        {
            "action" => check.Action == evt.Action,
            "add" => check.Shape is null || check.Shape == evt.Shape,
            "count" => check.Shape == evt.Shape && check.Shape is ShapeKind s && countOf(s) >= check.Count,
            "tool" => check.Tool == evt.Tool,
            "transform" => (check.Mode is null || check.Mode == evt.Mode) && (!check.Shift || evt.Shift),
            "slider" or "light" => check.Key == evt.Key,
            _ => true, // select / delete / color
        };
    }
}
