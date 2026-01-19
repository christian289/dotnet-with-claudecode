namespace GameDataTool.Application.Services;

using GameDataTool.Application.DTOs;
using GameDataTool.Application.Exceptions;
using GameDataTool.Application.Interfaces;

/// <summary>
/// 사용자 애플리케이션 서비스
/// User application service
/// </summary>
public sealed class UserService(IUserRepository userRepository)
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<UserDto?> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : ToDto(user);
    }

    public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(ToDto).ToList();
    }

    public async Task<UserDto> CreateUserAsync(string name, string email, CancellationToken cancellationToken = default)
    {
        var user = new User(Guid.NewGuid(), name, new Email(email));
        await _userRepository.AddAsync(user, cancellationToken);
        return ToDto(user);
    }

    public async Task UpdateUserNameAsync(Guid id, string newName, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("사용자를 찾을 수 없습니다.");
            // User not found.

        user.UpdateName(newName);
        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("사용자를 찾을 수 없습니다.");
            // User not found.

        await _userRepository.DeleteAsync(id, cancellationToken);
    }

    private static UserDto ToDto(User user) =>
        new(user.Id, user.Name, user.Email.Value, user.CreatedAt, user.UpdatedAt);
}
