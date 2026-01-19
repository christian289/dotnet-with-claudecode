namespace GameDataTool.Application.DTOs;

/// <summary>
/// 사용자 DTO
/// User DTO
/// </summary>
public sealed record UserDto(Guid Id, string Name, string Email, DateTime CreatedAt, DateTime? UpdatedAt);
