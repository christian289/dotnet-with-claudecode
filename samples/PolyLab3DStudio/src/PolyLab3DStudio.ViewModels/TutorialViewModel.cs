using PolyLab3DStudio.Core;

namespace PolyLab3DStudio.ViewModels;

/// <summary>
/// State of the in-viewport tutorial overlay: which lesson is running, which
/// task is active, and completion. Task advancement is driven by studio events.
/// </summary>
public sealed partial class TutorialViewModel : ObservableObject
{
    private readonly Action _onExitToCourses;
    private readonly Action _onContinueFree;
    private readonly Action _onCompleted;

    public TutorialViewModel(
        Course course,
        int lessonIndex,
        Action onExitToCourses,
        Action onContinueFree,
        Action onCompleted)
    {
        Course = course;
        LessonIndex = lessonIndex;
        Lesson = course.Lessons[lessonIndex];
        _onExitToCourses = onExitToCourses;
        _onContinueFree = onContinueFree;
        _onCompleted = onCompleted;

        CourseLabel = $"코스 {int.Parse(Course.Num, CultureInfo.InvariantCulture)}";
        Tasks = [.. (Lesson.Tasks ?? []).Select((t, i) => new TutorialTaskRowViewModel(t.Text, i + 1))];
        UpdateRows();
    }

    public Course Course { get; }

    public int LessonIndex { get; }

    public Lesson Lesson { get; }

    public string CourseLabel { get; }

    public string Title => Lesson.Title;

    public string? Tip => Lesson.Tip;

    public bool HasTip => !string.IsNullOrEmpty(Lesson.Tip);

    public ObservableCollection<TutorialTaskRowViewModel> Tasks { get; }

    [NotifyPropertyChangedFor(nameof(StepLabel))]
    [ObservableProperty] private int _taskIndex;

    [NotifyPropertyChangedFor(nameof(IsActive))]
    [ObservableProperty] private bool _completed;

    public bool IsActive => !Completed;

    public string StepLabel =>
        Lesson.Tasks is { Count: > 0 } tasks ? $"{Math.Min(TaskIndex + 1, tasks.Count)} / {tasks.Count}" : "";

    /// <summary>Checks the current task against an event; advances and completes as in the design.</summary>
    public void Advance(TaskEvent evt, Func<ShapeKind, int> countOf)
    {
        if (Completed || Lesson.Tasks is not { Count: > 0 } tasks || TaskIndex >= tasks.Count)
        {
            return;
        }

        if (!TaskChecker.Matches(tasks[TaskIndex].Check, evt, countOf))
        {
            return;
        }

        TaskIndex++;
        UpdateRows();

        if (TaskIndex >= tasks.Count)
        {
            Completed = true;
            _onCompleted();
        }
    }

    [RelayCommand]
    private void Exit() => _onExitToCourses();

    [RelayCommand]
    private void BackToCourses() => _onExitToCourses();

    [RelayCommand]
    private void ContinueFree() => _onContinueFree();

    private void UpdateRows()
    {
        for (int i = 0; i < Tasks.Count; i++)
        {
            Tasks[i].State = i < TaskIndex
                ? TutorialTaskState.Done
                : (i == TaskIndex && !Completed ? TutorialTaskState.Now : TutorialTaskState.Pending);
        }
    }
}
