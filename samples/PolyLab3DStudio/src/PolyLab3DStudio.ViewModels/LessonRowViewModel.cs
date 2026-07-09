using PolyLab3DStudio.Core;

namespace PolyLab3DStudio.ViewModels;

/// <summary>One lesson row inside a course card (rebuilt whenever progress changes).</summary>
public sealed partial class LessonRowViewModel(Lesson lesson, int number, bool isDone, bool englishOn, Action start) : ObservableObject
{
    public LessonKind Kind { get; } = lesson.Kind;

    public string KindLabel { get; } = lesson.Kind switch
    {
        LessonKind.Mission => "미션",
        LessonKind.Quiz => "퀴즈",
        LessonKind.Reading => "이론",
        _ => "레슨",
    };

    public string Title { get; } = lesson.Title;

    public string English { get; } = englishOn ? lesson.English : "";

    public string Minutes { get; } = $"{lesson.Minutes}분";

    public bool IsDone { get; } = isDone;

    public string Glyph { get; } = isDone ? "✓" : number.ToString(CultureInfo.InvariantCulture);

    public string Cta { get; } = isDone ? "복습" : "시작 →";

    [RelayCommand]
    private void Start() => start();
}
