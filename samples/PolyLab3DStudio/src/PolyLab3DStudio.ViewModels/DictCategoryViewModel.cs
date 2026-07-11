namespace PolyLab3DStudio.ViewModels;

/// <summary>One category filter chip on the 3D 사전 screen.</summary>
public sealed partial class DictCategoryViewModel(
    string label,
    Action<DictCategoryViewModel> select) : ObservableObject
{
    public string Label { get; } = label;

    [ObservableProperty] private bool _isSelected;

    [RelayCommand]
    private void Select() => select(this);
}
