namespace PolyLab3DStudio.ViewModels;

/// <summary>
/// A card in the "three graphic worlds" section. Text fields marked as markup
/// accept lightweight <c>**bold**</c> emphasis rendered by the view layer.
/// </summary>
public sealed record WorldCard(
    string IconKey,
    string Name,
    string EnglishTag,
    string ToolName,
    string Description,
    string Uses,
    string Formats);
