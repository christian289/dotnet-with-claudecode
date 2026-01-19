namespace GameDataTool.Domain.ValueObjects;

using GameDataTool.Domain.Exceptions;

/// <summary>
/// 이메일 값 객체
/// Email value object
/// </summary>
public sealed record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (!IsValid(value))
            throw new DomainException("유효하지 않은 이메일 형식입니다.");
            // Invalid email format.

        Value = value;
    }

    private static bool IsValid(string email) =>
        !string.IsNullOrWhiteSpace(email) &&
        email.Contains('@') &&
        email.Contains('.') &&
        email.Length <= 256;

    public override string ToString() => Value;
}
