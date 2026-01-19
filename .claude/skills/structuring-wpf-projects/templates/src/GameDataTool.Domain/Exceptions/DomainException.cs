namespace GameDataTool.Domain.Exceptions;

/// <summary>
/// 도메인 규칙 위반 예외
/// Domain rule violation exception
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}
