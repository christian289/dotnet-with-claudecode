namespace AvaloniaCollectionViewSample.ViewModels;

// ViewModel은 IEnumerable만 사용 (순수 BCL 타입)
// Avalonia 어셈블리 참조 없음
// ViewModel uses only IEnumerable (pure BCL type)
// No Avalonia assembly references
public sealed partial class MainViewModel : ObservableObject
{
    private readonly IMemberCollectionService _memberService;

    public IEnumerable? AllMembers { get; }
    public IEnumerable? ActiveMembers { get; }

    [ObservableProperty] private string _newMemberName = string.Empty;
    [ObservableProperty] private string _newMemberDepartment = string.Empty;
    [ObservableProperty] private bool _newMemberIsActive = true;

    public MainViewModel(IMemberCollectionService memberService)
    {
        _memberService = memberService;

        AllMembers = memberService.CreateView();
        ActiveMembers = memberService.CreateView(item => item is Member m && m.IsActive);

        // 샘플 데이터
        // Sample data
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
