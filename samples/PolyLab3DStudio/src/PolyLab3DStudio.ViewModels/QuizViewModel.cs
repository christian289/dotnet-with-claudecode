using PolyLab3DStudio.Core;

namespace PolyLab3DStudio.ViewModels;

/// <summary>Quiz modal state machine: pick → reveal → next → score, as in the design.</summary>
public sealed partial class QuizViewModel : ObservableObject
{
    private readonly Action _markDone;
    private readonly Action _close;
    private int? _picked;

    public QuizViewModel(Lesson lesson, Action markDone, Action close)
    {
        Lesson = lesson;
        _markDone = markDone;
        _close = close;
        LoadQuestion();
    }

    public Lesson Lesson { get; }

    public string Title => Lesson.Title;

    private IReadOnlyList<QuizQuestion> Questions => Lesson.Questions ?? [];

    public ObservableCollection<QuizOptionViewModel> Options { get; } = [];

    [NotifyPropertyChangedFor(nameof(Counter))]
    [NotifyPropertyChangedFor(nameof(QuestionText))]
    [NotifyPropertyChangedFor(nameof(NextLabel))]
    [ObservableProperty] private int _questionIndex;

    [ObservableProperty] private bool _revealed;

    [NotifyPropertyChangedFor(nameof(IsActive))]
    [NotifyPropertyChangedFor(nameof(Score))]
    [NotifyPropertyChangedFor(nameof(ResultMessage))]
    [ObservableProperty] private bool _finished;

    [ObservableProperty] private int _correct;

    [ObservableProperty] private string _feedback = "";

    [ObservableProperty] private bool _feedbackIsCorrect = true;

    [ObservableProperty] private string _why = "";

    public bool IsActive => !Finished;

    public string Counter => $"Q{QuestionIndex + 1} / {Questions.Count}";

    public string QuestionText => QuestionIndex < Questions.Count ? Questions[QuestionIndex].Question : "";

    public string NextLabel => QuestionIndex + 1 >= Questions.Count ? "결과 보기" : "다음 문제";

    public string Score => $"{Correct} / {Questions.Count}";

    public string ResultMessage => Correct == Questions.Count
        ? "완벽해요! 다음 코스로 넘어가 볼까요?"
        : (Correct >= Questions.Count - 1
            ? "좋아요! 헷갈린 개념만 한 번 더 살펴보세요."
            : "괜찮아요. 레슨을 다시 해보면 금방 익숙해져요.");

    [RelayCommand]
    private void Next()
    {
        if (QuestionIndex + 1 >= Questions.Count)
        {
            Finished = true;
            _markDone();
            return;
        }

        QuestionIndex++;
        _picked = null;
        Revealed = false;
        LoadQuestion();
    }

    [RelayCommand]
    private void Retry()
    {
        QuestionIndex = 0;
        _picked = null;
        Revealed = false;
        Correct = 0;
        Finished = false;
        LoadQuestion();
    }

    [RelayCommand]
    private void Close() => _close();

    private void LoadQuestion()
    {
        Options.Clear();
        if (QuestionIndex >= Questions.Count)
        {
            return;
        }

        QuizQuestion q = Questions[QuestionIndex];
        for (int i = 0; i < q.Options.Count; i++)
        {
            int index = i;
            Options.Add(new QuizOptionViewModel(((char)('A' + i)).ToString(), q.Options[i], () => Pick(index)));
        }
    }

    private void Pick(int index)
    {
        if (Revealed || QuestionIndex >= Questions.Count)
        {
            return;
        }

        QuizQuestion q = Questions[QuestionIndex];
        _picked = index;
        Revealed = true;

        bool ok = index == q.Answer;
        if (ok)
        {
            Correct++;
        }

        Feedback = ok ? "정답이에요!" : $"아쉬워요 — 정답은 \"{q.Options[q.Answer]}\"";
        FeedbackIsCorrect = ok;
        Why = q.Why;

        for (int i = 0; i < Options.Count; i++)
        {
            Options[i].State = i == q.Answer
                ? QuizOptionState.Correct
                : (i == _picked ? QuizOptionState.WrongPick : QuizOptionState.Dimmed);
        }
    }
}
