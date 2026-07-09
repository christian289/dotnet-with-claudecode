namespace PolyLab3DStudio.ViewModels;

public enum QuizOptionState
{
    Normal,
    Correct,
    WrongPick,
    Dimmed,
}

/// <summary>One answer choice in the quiz modal.</summary>
public sealed partial class QuizOptionViewModel(string letter, string label, Action pick) : ObservableObject
{
    public string Letter { get; } = letter;

    public string Label { get; } = label;

    [NotifyPropertyChangedFor(nameof(Mark))]
    [ObservableProperty] private QuizOptionState _state = QuizOptionState.Normal;

    public string Mark => State switch
    {
        QuizOptionState.Correct => "✓",
        QuizOptionState.WrongPick => "✕",
        _ => "",
    };

    [RelayCommand]
    private void Pick() => pick();
}
