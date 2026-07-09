namespace PolyLab3DStudio.Core;

public sealed record QuizQuestion(string Question, IReadOnlyList<string> Options, int Answer, string Why);
