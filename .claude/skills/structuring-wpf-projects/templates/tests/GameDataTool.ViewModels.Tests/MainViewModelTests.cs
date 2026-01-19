namespace GameDataTool.ViewModels.Tests;

using FluentAssertions;
using GameDataTool.Application.DTOs;
using GameDataTool.Application.Interfaces;
using GameDataTool.Application.Services;
using NSubstitute;
using Xunit;

public sealed class MainViewModelTests
{
    private readonly IUserRepository _userRepository;
    private readonly UserService _userService;
    private readonly MainViewModel _sut;

    public MainViewModelTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _userService = new UserService(_userRepository);
        _sut = new MainViewModel(_userService);
    }

    [Fact]
    public void CreateUserCommand_WhenNameAndEmailEmpty_ShouldBeDisabled()
    {
        // Arrange
        _sut.NewUserName = string.Empty;
        _sut.NewUserEmail = string.Empty;

        // Assert
        _sut.CreateUserCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void CreateUserCommand_WhenNameAndEmailProvided_ShouldBeEnabled()
    {
        // Arrange
        _sut.NewUserName = "테스트";
        _sut.NewUserEmail = "test@example.com";

        // Assert
        _sut.CreateUserCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task LoadUsersCommand_ShouldLoadUsers()
    {
        // Arrange
        var users = new List<GameDataTool.Domain.Entities.User>
        {
            new(Guid.NewGuid(), "사용자1", new GameDataTool.Domain.ValueObjects.Email("user1@example.com")),
            new(Guid.NewGuid(), "사용자2", new GameDataTool.Domain.ValueObjects.Email("user2@example.com"))
        };
        _userRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(users);

        // Act
        await _sut.LoadUsersCommand.ExecuteAsync(null);

        // Assert
        _sut.Users.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateUserCommand_ShouldAddUserToCollection()
    {
        // Arrange
        _sut.NewUserName = "새 사용자";
        // New user
        _sut.NewUserEmail = "new@example.com";

        // Act
        await _sut.CreateUserCommand.ExecuteAsync(null);

        // Assert
        _sut.Users.Should().HaveCount(1);
        _sut.NewUserName.Should().BeEmpty();
        _sut.NewUserEmail.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteUserCommand_ShouldRemoveUserFromCollection()
    {
        // Arrange
        var userDto = new UserDto(Guid.NewGuid(), "테스트", "test@example.com", DateTime.UtcNow, null);
        _sut.Users.Add(userDto);

        // Act
        await _sut.DeleteUserCommand.ExecuteAsync(userDto);

        // Assert
        _sut.Users.Should().BeEmpty();
    }
}
