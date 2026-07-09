namespace PolyLab3DStudio.ViewModels;

/// <summary>One course card on the courses screen.</summary>
public sealed class CourseRowViewModel(
    string num,
    string title,
    string english,
    string description,
    string progressLabel,
    double pct,
    IReadOnlyList<LessonRowViewModel> lessons)
{
    public string Num { get; } = num;

    public string Title { get; } = title;

    public string English { get; } = english;

    public string Description { get; } = description;

    public string ProgressLabel { get; } = progressLabel;

    public double Pct { get; } = pct;

    public IReadOnlyList<LessonRowViewModel> Lessons { get; } = lessons;
}
