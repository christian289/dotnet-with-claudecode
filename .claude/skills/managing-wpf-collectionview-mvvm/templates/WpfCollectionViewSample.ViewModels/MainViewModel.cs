namespace WpfCollectionViewSample.ViewModels;

// ViewModel은 IEnumerable만 사용 (순수 BCL 타입)
// ViewModel uses only IEnumerable (pure BCL type)
// WPF 어셈블리 참조 없음
// No WPF assembly references
public sealed partial class MainViewModel : ObservableObject
{
    private readonly IMemberCollectionService _memberService;

    // 전체 멤버 뷰
    // All members view
    public IEnumerable? AllMembers { get; }

    // 활성 멤버만 필터링된 뷰
    // Filtered view for active members only
    public IEnumerable? ActiveMembers { get; }

    [ObservableProperty] private string _newMemberName = string.Empty;
    [ObservableProperty] private string _newMemberDepartment = string.Empty;
    [ObservableProperty] private bool _newMemberIsActive = true;

    public MainViewModel(IMemberCollectionService memberService)
    {
        _memberService = memberService;

        // Service에서 IEnumerable로 받음
        // Receives IEnumerable from Service
        AllMembers = memberService.CreateView();
        ActiveMembers = memberService.CreateView(item => item is Member m && m.IsActive);

        // 샘플 데이터 추가
        // Add sample data
        _memberService.Add(new Member("홍길동", "개발팀", true));
        _memberService.Add(new Member("김철수", "디자인팀", true));
        _memberService.Add(new Member("이영희", "기획팀", false));
    }

    [RelayCommand]
    private void AddMember()
    {
        if (string.IsNullOrWhiteSpace(NewMemberName))
            return;

        _memberService.Add(new Member(NewMemberName, NewMemberDepartment, NewMemberIsActive));

        NewMemberName = string.Empty;
        NewMemberDepartment = string.Empty;
        NewMemberIsActive = true;
    }
}
