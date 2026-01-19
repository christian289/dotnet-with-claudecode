namespace GameDataTool.ViewModels;

/// <summary>
/// 메인 윈도우 ViewModel
/// Main window ViewModel
/// </summary>
public sealed partial class MainViewModel(UserService userService) : ObservableObject
{
    private readonly UserService _userService = userService;

    [ObservableProperty] private ObservableCollection<UserDto> _users = [];
    [ObservableProperty] private UserDto? _selectedUser;
    [ObservableProperty] private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateUserCommand))]
    private string _newUserName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateUserCommand))]
    private string _newUserEmail = string.Empty;

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        IsLoading = true;

        try
        {
            var users = await _userService.GetAllUsersAsync();
            Users = new ObservableCollection<UserDto>(users);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreateUser))]
    private async Task CreateUserAsync()
    {
        IsLoading = true;

        try
        {
            var user = await _userService.CreateUserAsync(NewUserName, NewUserEmail);
            Users.Add(user);

            // 입력 필드 초기화
            // Clear input fields
            NewUserName = string.Empty;
            NewUserEmail = string.Empty;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanCreateUser() =>
        !string.IsNullOrWhiteSpace(NewUserName) &&
        !string.IsNullOrWhiteSpace(NewUserEmail);

    [RelayCommand]
    private async Task DeleteUserAsync(UserDto? user)
    {
        if (user is null) return;

        IsLoading = true;

        try
        {
            await _userService.DeleteUserAsync(user.Id);
            Users.Remove(user);

            if (SelectedUser == user)
            {
                SelectedUser = null;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
