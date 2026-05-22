# 시큐어 코딩 지침

## 핵심 원칙

- **최소 권한 원칙**: 필요한 최소한의 권한만 부여
- **심층 방어 (Defense in Depth)**: 여러 계층의 보안 적용
- **안전한 기본값**: 기본 설정은 항상 가장 안전하게
- **신뢰하지 않기**: 모든 외부 입력은 잠재적 위협으로 간주

---

## 1. 입력 검증 (Input Validation)

### 1.1 기본 원칙

- **모든 입력은 신뢰하지 않음** (사용자 입력, API 응답, 파일 등)
- **화이트리스트 방식 검증** 우선 (허용된 것만 통과)
- 서버 측에서 **반드시** 재검증 (클라이언트 검증만으로 불충분)

### 1.2 검증 항목

| 항목 | 검증 내용 |
|------|----------|
| 타입 | 예상되는 데이터 타입 확인 |
| 길이 | 최소/최대 길이 제한 |
| 범위 | 숫자 범위, 날짜 범위 등 |
| 형식 | 정규식 패턴 매칭 |
| 비즈니스 규칙 | 도메인 특화 규칙 |

### 1.3 C# 예시

```csharp
// 잘못된 예
// Bad practice
public void ProcessData(string userInput)
{
    var query = $"SELECT * FROM Users WHERE Name = '{userInput}'";
}

// 올바른 예
// Good practice
public void ProcessData(string userInput)
{
    if (string.IsNullOrWhiteSpace(userInput))
    {
        throw new ArgumentException("입력값이 비어있습니다.");
        // Input value is empty.
    }

    if (userInput.Length > 100)
    {
        throw new ArgumentException("입력값이 너무 깁니다.");
        // Input value is too long.
    }

    // 파라미터화된 쿼리 사용
    // Use parameterized query
    using var command = new SqlCommand("SELECT * FROM Users WHERE Name = @name");
    command.Parameters.AddWithValue("@name", userInput);
}
```

---

## 2. 출력 인코딩 (Output Encoding)

### 2.1 컨텍스트별 인코딩

| 출력 컨텍스트 | 인코딩 방법 |
|--------------|------------|
| HTML Body | HTML Entity Encoding |
| HTML Attribute | Attribute Encoding |
| JavaScript | JavaScript Encoding |
| URL Parameter | URL Encoding |
| CSS | CSS Encoding |
| SQL | 파라미터화된 쿼리 |

### 2.2 XSS 방지

```csharp
// HTML 인코딩
// HTML encoding
var safeOutput = System.Web.HttpUtility.HtmlEncode(userInput);

// ASP.NET Core Razor에서는 자동 인코딩
// Auto-encoding in ASP.NET Core Razor
@Model.UserName  // 자동으로 HTML 인코딩됨
```

---

## 3. 인증 및 세션 관리

### 3.1 비밀번호 정책

- 최소 12자 이상
- 대소문자, 숫자, 특수문자 조합
- 일반적인 비밀번호 패턴 금지
- **bcrypt, Argon2** 등 안전한 해시 알고리즘 사용

### 3.2 세션 관리

- 세션 ID는 암호학적으로 안전한 난수 생성기 사용
- 로그인 성공 시 세션 ID 재생성
- 적절한 세션 타임아웃 설정
- HTTPS 전용 쿠키 사용 (`Secure`, `HttpOnly`, `SameSite` 속성)

```csharp
// 쿠키 설정 예시
// Cookie configuration example
services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});
```

---

## 4. 접근 제어

### 4.1 원칙

- **인증 (Authentication)**: 사용자가 누구인지 확인
- **인가 (Authorization)**: 사용자가 무엇을 할 수 있는지 확인
- 모든 요청에서 권한 검증 수행

### 4.2 안티 패턴

```csharp
// 잘못된 예: 클라이언트 제공 ID로 직접 접근
// Bad: Direct access with client-provided ID
public IActionResult GetDocument(int documentId)
{
    return Ok(_repository.GetById(documentId));  // 위험!
}

// 올바른 예: 소유권 검증
// Good: Ownership verification
public IActionResult GetDocument(int documentId)
{
    var document = _repository.GetById(documentId);

    if (document.OwnerId != _currentUser.Id)
    {
        return Forbid();
    }

    return Ok(document);
}
```

---

## 5. 암호화

### 5.1 알고리즘 선택

| 용도 | 권장 알고리즘 |
|------|-------------|
| 대칭키 암호화 | AES-256-GCM |
| 비대칭키 암호화 | RSA-2048 이상, ECDSA |
| 해시 (일반) | SHA-256 이상 |
| 비밀번호 해시 | bcrypt, Argon2, PBKDF2 |
| 난수 생성 | `RandomNumberGenerator` |

### 5.2 금지 알고리즘

- ❌ MD5 (해시 충돌 취약)
- ❌ SHA-1 (해시 충돌 취약)
- ❌ DES, 3DES (키 길이 부족)
- ❌ `System.Random` (암호학적으로 안전하지 않음)

### 5.3 안전한 난수 생성

```csharp
// 안전한 난수 생성
// Secure random number generation
using var rng = RandomNumberGenerator.Create();
var bytes = new byte[32];
rng.GetBytes(bytes);
var token = Convert.ToBase64String(bytes);
```

---

## 6. 에러 처리 및 로깅

### 6.1 에러 처리 원칙

- 상세한 에러 메시지를 사용자에게 노출하지 않음
- 내부적으로는 상세 로깅, 외부에는 일반적 메시지
- 스택 트레이스 노출 금지

```csharp
// 잘못된 예
// Bad practice
catch (Exception ex)
{
    return BadRequest(ex.ToString());  // 스택 트레이스 노출!
}

// 올바른 예
// Good practice
catch (Exception ex)
{
    _logger.LogError(ex, "데이터 처리 중 오류 발생");
    // Error occurred during data processing
    return BadRequest("요청을 처리할 수 없습니다.");
    // Unable to process the request.
}
```

### 6.2 로깅 주의사항

**로깅 금지 항목:**
- 비밀번호, API 키, 토큰
- 신용카드 번호, 주민등록번호
- 개인 식별 정보 (PII)

---

## 7. SQL Injection 방지

### 7.1 필수 규칙

- **항상 파라미터화된 쿼리 사용**
- 문자열 연결로 쿼리 생성 금지
- ORM 사용 시에도 Raw SQL 주의

```csharp
// 잘못된 예: SQL Injection 취약
// Bad: SQL Injection vulnerable
var query = $"SELECT * FROM Users WHERE Email = '{email}'";

// 올바른 예: 파라미터화된 쿼리
// Good: Parameterized query
var query = "SELECT * FROM Users WHERE Email = @Email";
command.Parameters.AddWithValue("@Email", email);

// Entity Framework 사용 시
// When using Entity Framework
var user = context.Users.FirstOrDefault(u => u.Email == email);
```

---

## 8. 파일 업로드 보안

### 8.1 검증 항목

- 파일 확장자 화이트리스트 검증
- MIME 타입 검증 (확장자만으로 불충분)
- 파일 크기 제한
- 파일 내용 검사 (매직 바이트)
- 업로드 경로를 웹 루트 외부로 설정

### 8.2 파일명 처리

```csharp
// 안전한 파일명 생성
// Generate safe filename
var safeFileName = Path.GetRandomFileName() +
    Path.GetExtension(originalFileName);

// 경로 조작 방지
// Prevent path traversal
var fileName = Path.GetFileName(userProvidedPath);  // 경로 제거
```

---

## 9. 의존성 보안

### 9.1 관리 원칙

- 정기적인 의존성 업데이트
- 취약점 스캔 도구 사용 (`dotnet list package --vulnerable`)
- 사용하지 않는 패키지 제거
- 신뢰할 수 있는 소스에서만 패키지 설치

### 9.2 취약점 확인

```bash
# NuGet 패키지 취약점 확인
# Check NuGet package vulnerabilities
dotnet list package --vulnerable

# 또는 OWASP Dependency-Check 사용
# Or use OWASP Dependency-Check
```

---

## 10. 통신 보안

### 10.1 HTTPS 필수

- 모든 통신에 TLS 1.2 이상 사용
- HTTP → HTTPS 리다이렉션 적용
- HSTS 헤더 설정

```csharp
// ASP.NET Core HTTPS 강제
// Force HTTPS in ASP.NET Core
app.UseHttpsRedirection();
app.UseHsts();
```

### 10.2 API 보안

- API 키는 헤더로 전송 (URL 파라미터 금지)
- Rate Limiting 적용
- CORS 정책 최소화

---

## 11. 체크리스트

### 코드 리뷰 시 확인 항목

- [ ] 모든 사용자 입력에 검증 적용
- [ ] SQL 쿼리가 파라미터화되어 있음
- [ ] 출력 시 적절한 인코딩 적용
- [ ] 민감 정보가 로그에 기록되지 않음
- [ ] 에러 메시지에 상세 정보 미노출
- [ ] 파일 업로드 시 적절한 검증 수행
- [ ] 암호화에 안전한 알고리즘 사용
- [ ] 접근 제어가 모든 엔드포인트에 적용
- [ ] HTTPS 통신 강제
- [ ] 의존성 취약점 검사 완료

---

## 12. 참고 자료

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/)
- [Microsoft Security Documentation](https://docs.microsoft.com/en-us/security/)
- [CWE Top 25](https://cwe.mitre.org/top25/)
