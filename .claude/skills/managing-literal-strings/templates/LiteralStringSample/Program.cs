// Literal String Sample - const string 사용 패턴 예제
// Literal String Sample - const string usage pattern example

using LiteralStringSample.Constants;

// 애플리케이션 시작 로그
// Application start log
Console.WriteLine(LogMessages.ApplicationStarted);

// 좋은 예: const string 사용
// Good example: using const string
try
{
    var input = GetUserInput();

    if (string.IsNullOrEmpty(input))
    {
        throw new ArgumentException(Messages.InvalidInput);
    }

    ProcessData(input);
    Console.WriteLine(Messages.OperationSuccess);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
catch (Exception)
{
    Console.WriteLine(Messages.ErrorOccurred);
    Console.WriteLine(LogMessages.UnhandledException);
}

// 종료 확인
// Exit confirmation
Console.WriteLine(Messages.ConfirmExit);
Console.ReadKey();

// 애플리케이션 종료 로그
// Application stop log
Console.WriteLine(LogMessages.ApplicationStopped);

static string? GetUserInput()
{
    Console.Write("Enter data: ");
    return Console.ReadLine();
}

static void ProcessData(string data)
{
    // 포맷 문자열과 함께 const string 사용
    // Using const string with format string
    Console.WriteLine(string.Format(LogMessages.UserLoggedIn, data));

    // 성능 경고 예시
    // Performance warning example
    var processingTime = Random.Shared.Next(100, 600);
    if (processingTime > 500)
    {
        Console.WriteLine(string.Format(LogMessages.PerformanceWarning, processingTime));
    }
}
