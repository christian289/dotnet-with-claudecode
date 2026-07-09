using PolyLab3DStudio.Core;

namespace PolyLab3DStudio.ViewModels;

/// <summary>The courses screen: course cards plus the quiz and reading modals.</summary>
public sealed partial class CoursesViewModel(ShellViewModel shell) : ObservableObject
{
    public ShellViewModel Shell { get; } = shell;

    public ObservableCollection<CourseRowViewModel> Courses { get; } = [];

    [ObservableProperty] private QuizViewModel? _activeQuiz;

    [ObservableProperty] private ReadingViewModel? _activeReading;

    /// <summary>Recomputes all course/lesson rows from the current progress map.</summary>
    public void Rebuild()
    {
        Courses.Clear();
        foreach (Course course in CourseCatalog.All)
        {
            int done = 0;
            var lessons = new List<LessonRowViewModel>(course.Lessons.Count);
            for (int i = 0; i < course.Lessons.Count; i++)
            {
                Lesson lesson = course.Lessons[i];
                bool isDone = Shell.IsDone(course.Id, lesson);
                if (isDone)
                {
                    done++;
                }

                int index = i;
                lessons.Add(new LessonRowViewModel(
                    lesson, i + 1, isDone, Shell.EnglishOn,
                    () => Shell.StartLesson(course.Id, index)));
            }

            double pct = course.Lessons.Count > 0 ? Math.Round(done * 100.0 / course.Lessons.Count) : 0;
            Courses.Add(new CourseRowViewModel(
                course.Num, course.Title, course.English, course.Description,
                $"{done}/{course.Lessons.Count} 완료", pct, lessons));
        }
    }

    [RelayCommand]
    private void GoStart() => Shell.GoStart();

    [RelayCommand]
    private void GoStudioFree() => Shell.GoStudioFree();
}
