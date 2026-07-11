using PolyLab3DStudio.Core;

namespace PolyLab3DStudio.ViewModels;

/// <summary>
/// App shell: screen navigation (start / courses / studio / settings), progress
/// aggregation, settings persistence, and lesson dispatch — the design's
/// top-level component state.
/// </summary>
public sealed partial class ShellViewModel : ObservableObject
{
    private readonly SettingsStore _settingsStore;
    private readonly ProgressStore _progressStore;
    private Dictionary<string, bool> _progress;
    private string _screenKey = "start";
    private string _returnKey = "start";
    private bool _loadingSettings;

    public ShellViewModel(SettingsStore settingsStore, ProgressStore progressStore)
    {
        _settingsStore = settingsStore;
        _progressStore = progressStore;
        _progress = progressStore.Load();

        UserSettings settings = settingsStore.Load();
        _loadingSettings = true;
        _english = settings.English;
        _englishOn = settings.EnglishOn;
        _gridOn = settings.Grid;
        _sensitivity = settings.Sensitivity;
        _invertZoom = settings.InvertZoom;
        _loadingSettings = false;

        Start = new StartViewModel(this);
        Courses = new CoursesViewModel(this);
        Studio = new StudioViewModel(this);
        Settings = new SettingsViewModel(this);
        Guide = new GuideViewModel(this);
        Pipeline = new PipelineViewModel(this);
        Dict = new DictViewModel(this);
        ToolMap = new ToolMapViewModel(this);

        _currentViewModel = Start;
        RefreshProgress();
    }

    public StartViewModel Start { get; }

    public CoursesViewModel Courses { get; }

    public StudioViewModel Studio { get; }

    public SettingsViewModel Settings { get; }

    public GuideViewModel Guide { get; }

    public PipelineViewModel Pipeline { get; }

    public DictViewModel Dict { get; }

    public ToolMapViewModel ToolMap { get; }

    [NotifyPropertyChangedFor(nameof(IsStudioActive))]
    [ObservableProperty] private object _currentViewModel;

    public bool IsStudioActive => CurrentViewModel == Studio;

    // ---------------- settings (write-through persistence) ----------------

    private bool? _english;

    [ObservableProperty] private bool _englishOn;

    [ObservableProperty] private bool _gridOn;

    [NotifyPropertyChangedFor(nameof(SensitivityLabel))]
    [ObservableProperty] private double _sensitivity;

    [ObservableProperty] private bool _invertZoom;

    public string SensitivityLabel => string.Create(CultureInfo.InvariantCulture, $"×{Sensitivity:0.0}");

    partial void OnEnglishOnChanged(bool value)
    {
        _english = value;
        SaveSettings();
        Courses.Rebuild();
    }

    partial void OnGridOnChanged(bool value) => SaveSettings();

    partial void OnSensitivityChanged(double value) => SaveSettings();

    partial void OnInvertZoomChanged(bool value) => SaveSettings();

    private void SaveSettings()
    {
        if (!_loadingSettings)
        {
            _settingsStore.Save(new UserSettings(_english, GridOn, Sensitivity, InvertZoom));
        }
    }

    // ---------------- progress ----------------

    [ObservableProperty] private string _progressText = "";

    [ObservableProperty] private double _overallPct;

    [ObservableProperty] private string _continueSub = "";

    public bool IsDone(string courseId, Lesson lesson) =>
        _progress.GetValueOrDefault(CourseCatalog.LessonKey(courseId, lesson));

    public void MarkDone(string courseId, Lesson lesson)
    {
        _progress[CourseCatalog.LessonKey(courseId, lesson)] = true;
        _progressStore.Save(_progress);
        RefreshProgress();
        Courses.Rebuild();
    }

    public void ResetProgress()
    {
        _progress = [];
        _progressStore.Reset();
        RefreshProgress();
        Courses.Rebuild();
    }

    private (Course Course, int LessonIndex)? FindNext()
    {
        foreach (Course course in CourseCatalog.All)
        {
            for (int i = 0; i < course.Lessons.Count; i++)
            {
                if (!IsDone(course.Id, course.Lessons[i]))
                {
                    return (course, i);
                }
            }
        }

        return null;
    }

    private void RefreshProgress()
    {
        int total = 0, done = 0;
        foreach (Course course in CourseCatalog.All)
        {
            foreach (Lesson lesson in course.Lessons)
            {
                total++;
                if (IsDone(course.Id, lesson))
                {
                    done++;
                }
            }
        }

        ProgressText = $"{done}/{total} 완료";
        OverallPct = total > 0 ? Math.Round(done * 100.0 / total) : 0;

        (Course Course, int LessonIndex)? next = FindNext();
        ContinueSub = next is { } n
            ? $"코스 {CourseCatalog.All.ToList().IndexOf(n.Course) + 1} · {n.Course.Lessons[n.LessonIndex].Title}"
            : "모든 코스 완료! 자유 모드로";
    }

    // ---------------- navigation ----------------

    public void GoStart() => SetScreen("start", Start);

    public void GoCourses()
    {
        Courses.Rebuild();
        SetScreen("courses", Courses);
    }

    public void GoStudioFree()
    {
        Studio.EnterFreeMode();
        SetScreen("studio", Studio);
    }

    public void GoGuide() => SetScreen("guide", Guide);

    public void GoPipeline() => SetScreen("pipeline", Pipeline);

    public void GoDict() => SetScreen("dict", Dict);

    /// <summary>Term-tip deep link: open the dictionary pre-searched with a word.</summary>
    public void GoDictSearch(string word)
    {
        Dict.SearchFor(word);
        SetScreen("dict", Dict);
    }

    public void GoToolMap() => SetScreen("map", ToolMap);

    public void GoSettings()
    {
        _returnKey = _screenKey;
        SetScreen("settings", Settings);
    }

    public void SettingsBack()
    {
        switch (_returnKey)
        {
            case "courses":
                GoCourses();
                break;
            case "studio":
                SetScreen("studio", Studio);
                break;
            default:
                GoStart();
                break;
        }
    }

    public void StartContinue()
    {
        if (FindNext() is { } next)
        {
            StartLesson(next.Course.Id, next.LessonIndex);
        }
        else
        {
            GoStudioFree();
        }
    }

    public void StartLesson(string courseId, int lessonIndex)
    {
        Course? course = CourseCatalog.Find(courseId);
        Lesson? lesson = CourseCatalog.GetLesson(courseId, lessonIndex);
        if (course is null || lesson is null)
        {
            return;
        }

        if (lesson.Kind == LessonKind.Quiz)
        {
            Courses.ActiveQuiz = new QuizViewModel(
                lesson,
                markDone: () => MarkDone(courseId, lesson),
                close: () => Courses.ActiveQuiz = null);
            SetScreen("courses", Courses);
            return;
        }

        if (lesson.Kind == LessonKind.Reading)
        {
            Courses.ActiveReading = new ReadingViewModel(
                lesson,
                markDone: () => MarkDone(courseId, lesson),
                close: () => Courses.ActiveReading = null);
            SetScreen("courses", Courses);
            return;
        }

        Studio.LoadLesson(course, lessonIndex);
        SetScreen("studio", Studio);
    }

    private void SetScreen(string key, object viewModel)
    {
        _screenKey = key;
        CurrentViewModel = viewModel;
    }
}
