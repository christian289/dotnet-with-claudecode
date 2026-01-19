namespace AvaloniaCollectionViewSample.AvaloniaServices;

// AvaloniaUI Service Layer
// DataGridCollectionView 사용
// Uses DataGridCollectionView
public sealed class MemberCollectionService : IMemberCollectionService
{
    private ObservableCollection<Member> Source { get; } = [];

    // DataGridCollectionView 반환
    // IEnumerable로 반환하여 ViewModel이 Avalonia 타입을 모르게 함
    // Returns DataGridCollectionView
    // Returns IEnumerable so ViewModel doesn't know Avalonia types
    public IEnumerable CreateView(Predicate<object>? filter = null)
    {
        var view = new DataGridCollectionView(Source);

        if (filter is not null)
        {
            view.Filter = filter;
        }

        return view;
    }

    public void Add(Member item) => Source.Add(item);

    public void Remove(Member? item)
    {
        if (item is not null)
            Source.Remove(item);
    }

    public void Clear() => Source.Clear();
}
