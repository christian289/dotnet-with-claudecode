using PolyLab3DStudio.Core;

namespace PolyLab3DStudio.ViewModels;

/// <summary>Reading (이론) modal: page navigation with completion on the last page.</summary>
public sealed partial class ReadingViewModel : ObservableObject
{
    private readonly Action _markDone;
    private readonly Action _close;

    public ReadingViewModel(Lesson lesson, Action markDone, Action close)
    {
        Lesson = lesson;
        _markDone = markDone;
        _close = close;
    }

    public Lesson Lesson { get; }

    public string Title => Lesson.Title;

    private IReadOnlyList<ReadingPage> Pages => Lesson.Pages ?? [];

    [NotifyPropertyChangedFor(nameof(PageLabel))]
    [NotifyPropertyChangedFor(nameof(PageTitle))]
    [NotifyPropertyChangedFor(nameof(Paragraphs))]
    [NotifyPropertyChangedFor(nameof(Code))]
    [NotifyPropertyChangedFor(nameof(HasCode))]
    [NotifyPropertyChangedFor(nameof(PrevEnabled))]
    [NotifyPropertyChangedFor(nameof(NextLabel))]
    [ObservableProperty] private int _page;

    public string PageLabel => Pages.Count > 0 ? $"{Page + 1} / {Pages.Count}" : "";

    public string PageTitle => Page < Pages.Count ? Pages[Page].Title : "";

    public IReadOnlyList<string> Paragraphs => Page < Pages.Count ? Pages[Page].Paragraphs : [];

    public string? Code => Page < Pages.Count ? Pages[Page].Code : null;

    public bool HasCode => !string.IsNullOrEmpty(Code);

    public bool PrevEnabled => Page > 0;

    public string NextLabel => Page + 1 >= Pages.Count ? "완료 ✓" : "다음 →";

    [RelayCommand]
    private void Prev()
    {
        if (Page > 0)
        {
            Page--;
        }
    }

    [RelayCommand]
    private void Next()
    {
        if (Page + 1 >= Pages.Count)
        {
            _markDone();
            _close();
            return;
        }

        Page++;
    }

    [RelayCommand]
    private void Close() => _close();
}
