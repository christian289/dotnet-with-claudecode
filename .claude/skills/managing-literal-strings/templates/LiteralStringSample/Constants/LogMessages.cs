namespace LiteralStringSample.Constants;

/// <summary>
/// 로그 메시지 상수 정의
/// Log message constants
/// </summary>
public static class LogMessages
{
    // 정보 로그
    // Information logs
    public const string ApplicationStarted = "애플리케이션이 시작되었습니다.";
    // Application started.

    public const string ApplicationStopped = "애플리케이션이 종료되었습니다.";
    // Application stopped.

    public const string UserLoggedIn = "사용자가 로그인했습니다: {0}";
    // User logged in: {0}

    public const string UserLoggedOut = "사용자가 로그아웃했습니다: {0}";
    // User logged out: {0}

    // 경고 로그
    // Warning logs
    public const string PerformanceWarning = "성능 경고: 처리 시간이 {0}ms를 초과했습니다.";
    // Performance warning: Processing time exceeded {0}ms.

    public const string RetryAttempt = "재시도 중... (시도 {0}/{1})";
    // Retrying... (attempt {0}/{1})

    // 오류 로그
    // Error logs
    public const string UnhandledException = "처리되지 않은 예외가 발생했습니다.";
    // An unhandled exception occurred.

    public const string DatabaseConnectionFailed = "데이터베이스 연결에 실패했습니다.";
    // Database connection failed.
}
