namespace WpfCollectionViewSample.Core;

using System.Collections;

// Interface 정의 (순수 BCL 타입만 사용)
// Interface definition (uses pure BCL types only)
public interface IMemberCollectionService
{
    IEnumerable CreateView(Predicate<object>? filter = null);
    void Add(Member member);
    void Remove(Member? member);
    void Clear();
}
