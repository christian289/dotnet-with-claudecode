using PolyLab3DStudio.Core;

namespace PolyLab3DStudio.ViewModels;

/// <summary>
/// The "3D 사전" glossary screen, transcribed from the design: live text
/// search over word/english/short/detail plus category chip filtering
/// against <see cref="GlossaryCatalog"/>.
/// </summary>
public sealed partial class DictViewModel : ObservableObject
{
    private DictCategoryViewModel _selectedCategory;

    public DictViewModel(ShellViewModel shell)
    {
        Shell = shell;
        Categories = [.. GlossaryCatalog.Categories.Select(c => new DictCategoryViewModel(c, Select))];
        _selectedCategory = Categories[0];
        _selectedCategory.IsSelected = true;
        _terms = GlossaryCatalog.All;
        _countText = $"{_terms.Count}개 용어";
    }

    public ShellViewModel Shell { get; }

    public IReadOnlyList<DictCategoryViewModel> Categories { get; }

    [ObservableProperty] private string _query = "";

    [ObservableProperty] private IReadOnlyList<GlossaryTerm> _terms;

    [ObservableProperty] private string _countText;

    [ObservableProperty] private bool _isEmpty;

    partial void OnQueryChanged(string value) => Refresh();

    /// <summary>Term-tip deep link: reset to 전체 and search for the given word.</summary>
    public void SearchFor(string word)
    {
        Select(Categories[0]);
        Query = word;
    }

    private void Select(DictCategoryViewModel category)
    {
        if (category == _selectedCategory)
        {
            return;
        }

        _selectedCategory.IsSelected = false;
        category.IsSelected = true;
        _selectedCategory = category;
        Refresh();
    }

    private void Refresh()
    {
        string q = Query.Trim();
        string cat = _selectedCategory.Label;

        Terms =
        [
            .. GlossaryCatalog.All.Where(t =>
                (cat == "전체" || t.Category == cat) &&
                (q.Length == 0 ||
                 t.Word.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                 t.English.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                 t.Short.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                 t.Detail.Contains(q, StringComparison.OrdinalIgnoreCase))),
        ];
        CountText = $"{Terms.Count}개 용어";
        IsEmpty = Terms.Count == 0;
    }

    [RelayCommand]
    private void GoStart() => Shell.GoStart();
}
