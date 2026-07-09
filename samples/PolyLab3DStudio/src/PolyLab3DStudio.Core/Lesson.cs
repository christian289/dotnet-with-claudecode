namespace PolyLab3DStudio.Core;

/// <summary>
/// One course entry. Interactive kinds (Lesson/Mission) carry a seed scene and
/// tutorial tasks; Quiz carries questions; Reading carries pages.
/// </summary>
public sealed record Lesson(
    string Id,
    LessonKind Kind,
    string Title,
    string English,
    int Minutes,
    string? Seed = null,
    string? Tip = null,
    IReadOnlyList<TutorialTask>? Tasks = null,
    IReadOnlyList<QuizQuestion>? Questions = null,
    IReadOnlyList<ReadingPage>? Pages = null);
