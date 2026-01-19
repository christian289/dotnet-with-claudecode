namespace GameDataTool.Application.Tests;

using FluentAssertions;
using GameDataTool.Application.Exceptions;
using GameDataTool.Application.Interfaces;
using GameDataTool.Application.Services;
using GameDataTool.Domain.Entities;
using GameDataTool.Domain.ValueObjects;
using NSubstitute;
using Xunit;

public sealed class UserServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _sut = new UserService(_userRepository);
    }

    [Fact]
    public async Task GetUserAsync_WhenUserExists_ShouldReturnUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User(userId, "테스트", new Email("test@example.com"));
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.GetUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Name.Should().Be("테스트");
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetUserAsync_WhenUserNotExists_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _sut.GetUserAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateUserNameAsync_WhenUserNotExists_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var act = async () => await _sut.UpdateUserNameAsync(userId, "새 이름");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage("사용자를 찾을 수 없습니다.");
                 // User not found.
    }

    [Fact]
    public async Task CreateUserAsync_ShouldAddUserAndReturnDto()
    {
        // Arrange
        var name = "새 사용자";
        // New user
        var email = "new@example.com";

        // Act
        var result = await _sut.CreateUserAsync(name, email);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(name);
        result.Email.Should().Be(email);
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}
