namespace GameDataTool.Domain.Tests;

using FluentAssertions;
using GameDataTool.Domain.Entities;
using GameDataTool.Domain.Exceptions;
using GameDataTool.Domain.ValueObjects;
using Xunit;

public sealed class UserTests
{
    [Fact]
    public void User_Create_ShouldSetProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "테스트 사용자";
        // Test user
        var email = new Email("test@example.com");

        // Act
        var user = new User(id, name, email);

        // Assert
        user.Id.Should().Be(id);
        user.Name.Should().Be(name);
        user.Email.Should().Be(email);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void User_UpdateName_ShouldUpdateNameAndTimestamp()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), "원래 이름", new Email("test@example.com"));
        // Original name
        var newName = "새 이름";
        // New name

        // Act
        user.UpdateName(newName);

        // Assert
        user.Name.Should().Be(newName);
        user.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void User_UpdateName_WithInvalidName_ShouldThrowDomainException(string? invalidName)
    {
        // Arrange
        var user = new User(Guid.NewGuid(), "테스트", new Email("test@example.com"));

        // Act
        var act = () => user.UpdateName(invalidName!);

        // Assert
        act.Should().Throw<DomainException>()
           .WithMessage("이름은 필수입니다.");
           // Name is required.
    }
}
