namespace PolyLab3DStudio.ViewModels;

public enum TutorialTaskState
{
    Pending,
    Now,
    Done,
}

/// <summary>One checklist row in the tutorial overlay.</summary>
public sealed partial class TutorialTaskRowViewModel(string text, int number) : ObservableObject
{
    public string Text { get; } = text;

    public int Number { get; } = number;

    [NotifyPropertyChangedFor(nameof(Glyph))]
    [ObservableProperty] private TutorialTaskState _state = TutorialTaskState.Pending;

    public string Glyph => State == TutorialTaskState.Done ? "✓" : Number.ToString(CultureInfo.InvariantCulture);
}
