namespace WpfCollectionViewSample.WpfServices;

// Services/MemberCollectionService.cs
// 이 클래스는 PresentationFramework 참조 가능
// This class can reference PresentationFramework
public sealed class MemberCollectionService : IMemberCollectionService
{
    private ObservableCollection<Member> Source { get; } = [];

    // Factory Method: 필터링된 뷰 생성
    // IEnumerable로 반환하여 ViewModel이 WPF 타입을 모르게 함
    // Factory Method: Create filtered view
    // Returns IEnumerable so ViewModel doesn't know WPF types
    public IEnumerable CreateView(Predicate<object>? filter = null)
    {
        var viewSource = new CollectionViewSource { Source = Source };
        var view = viewSource.View;

        if (filter is not null)
        {
            view.Filter = filter;
        }

        return view; // ICollectionView는 IEnumerable을 상속
                     // ICollectionView inherits IEnumerable
    }

    // 정렬된 뷰 생성
    // Create sorted view
    public IEnumerable CreateSortedView(
        string propertyName,
        ListSortDirection direction = ListSortDirection.Ascending)
    {
        var viewSource = new CollectionViewSource { Source = Source };
        var view = viewSource.View;

        view.SortDescriptions.Add(new SortDescription(propertyName, direction));

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
