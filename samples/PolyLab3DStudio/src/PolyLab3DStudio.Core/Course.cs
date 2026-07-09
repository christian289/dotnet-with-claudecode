namespace PolyLab3DStudio.Core;

public sealed record Course(
    string Id,
    string Num,
    string Title,
    string English,
    string Description,
    IReadOnlyList<Lesson> Lessons);
