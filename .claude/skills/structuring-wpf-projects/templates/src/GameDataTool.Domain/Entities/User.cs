namespace GameDataTool.Domain.Entities;

using GameDataTool.Domain.Exceptions;
using GameDataTool.Domain.ValueObjects;

/// <summary>
/// 사용자 도메인 엔티티
/// User domain entity
/// </summary>
public sealed class User
{
    public Guid Id { get; init; }
    public string Name { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; private set; }

    // EF Core를 위한 Private 생성자
    // Private constructor for EF Core
    private User() { }

    public User(Guid id, string name, Email email)
    {
        Id = id;
        Name = name;
        Email = email;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        // 도메인 비즈니스 규칙 검증
        // Domain business rule validation
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("이름은 필수입니다.");
            // Name is required.

        if (name.Length > 100)
            throw new DomainException("이름은 100자를 초과할 수 없습니다.");
            // Name cannot exceed 100 characters.

        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateEmail(Email email)
    {
        Email = email ?? throw new DomainException("이메일은 필수입니다.");
        // Email is required.
        UpdatedAt = DateTime.UtcNow;
    }
}
