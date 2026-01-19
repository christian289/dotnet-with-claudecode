namespace GameDataTool.Application.Exceptions;

/// <summary>
/// 리소스를 찾을 수 없을 때 발생하는 예외
/// Exception thrown when a resource is not found
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
