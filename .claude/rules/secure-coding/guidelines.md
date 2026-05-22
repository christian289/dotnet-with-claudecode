# Secure Coding Guidelines

## Core Principles

- **Least privilege**: grant only the minimum permissions required.
- **Defense in depth**: layer security controls.
- **Secure defaults**: defaults should be the safest option.
- **Trust nothing**: treat every external input as a potential threat.

---

## 1. Input Validation

### 1.1 Basics

- **Trust no input** (user input, API responses, files, etc.).
- **Prefer allowlist validation** (let only the permitted pass).
- The server **must** re-validate (client-side validation alone is
  insufficient).

### 1.2 What to Validate

| Aspect | What to Check |
|--------|---------------|
| Type | The expected data type |
| Length | Minimum / maximum length |
| Range | Numeric range, date range, etc. |
| Format | Regex pattern matching |
| Business rules | Domain-specific constraints |

### 1.3 C# Example

```csharp
// Bad practice
public void ProcessData(string userInput)
{
    var query = $"SELECT * FROM Users WHERE Name = '{userInput}'";
}

// Good practice
public void ProcessData(string userInput)
{
    if (string.IsNullOrWhiteSpace(userInput))
    {
        throw new ArgumentException("Input value is empty.");
    }

    if (userInput.Length > 100)
    {
        throw new ArgumentException("Input value is too long.");
    }

    // Use a parameterized query
    using var command = new SqlCommand("SELECT * FROM Users WHERE Name = @name");
    command.Parameters.AddWithValue("@name", userInput);
}
```

---

## 2. Output Encoding

### 2.1 Encoding per Context

| Output Context | Encoding |
|----------------|----------|
| HTML body | HTML entity encoding |
| HTML attribute | Attribute encoding |
| JavaScript | JavaScript encoding |
| URL parameter | URL encoding |
| CSS | CSS encoding |
| SQL | Parameterized queries |

### 2.2 XSS Prevention

```csharp
// HTML encoding
var safeOutput = System.Web.HttpUtility.HtmlEncode(userInput);

// ASP.NET Core Razor auto-encodes
@Model.UserName  // Encoded automatically
```

---

## 3. Authentication and Session Management

### 3.1 Password Policy

- At least 12 characters.
- Mix of uppercase, lowercase, digits, and special characters.
- Disallow common password patterns.
- Use a strong hash algorithm such as **bcrypt** or **Argon2**.

### 3.2 Session Management

- Generate session IDs with a cryptographically secure RNG.
- Regenerate the session ID on successful login.
- Set an appropriate session timeout.
- Use HTTPS-only cookies (`Secure`, `HttpOnly`, `SameSite`).

```csharp
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

## 4. Access Control

### 4.1 Principles

- **Authentication** identifies who the user is.
- **Authorization** decides what the user is allowed to do.
- Authorize on every request.

### 4.2 Anti-Pattern

```csharp
// Bad: direct access using a client-provided ID
public IActionResult GetDocument(int documentId)
{
    return Ok(_repository.GetById(documentId));  // Dangerous!
}

// Good: verify ownership
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

## 5. Cryptography

### 5.1 Algorithm Selection

| Use Case | Recommended Algorithm |
|----------|-----------------------|
| Symmetric encryption | AES-256-GCM |
| Asymmetric encryption | RSA-2048+, ECDSA |
| General hashing | SHA-256+ |
| Password hashing | bcrypt, Argon2, PBKDF2 |
| Random number generation | `RandomNumberGenerator` |

### 5.2 Forbidden Algorithms

- ❌ MD5 (hash-collision vulnerable)
- ❌ SHA-1 (hash-collision vulnerable)
- ❌ DES, 3DES (key length insufficient)
- ❌ `System.Random` (not cryptographically secure)

### 5.3 Secure Random Generation

```csharp
// Secure random number generation
using var rng = RandomNumberGenerator.Create();
var bytes = new byte[32];
rng.GetBytes(bytes);
var token = Convert.ToBase64String(bytes);
```

---

## 6. Error Handling and Logging

### 6.1 Error Handling Principles

- Do not expose detailed error messages to the user.
- Log details internally; return generic messages externally.
- Never expose stack traces.

```csharp
// Bad
catch (Exception ex)
{
    return BadRequest(ex.ToString());  // Stack trace leak!
}

// Good
catch (Exception ex)
{
    _logger.LogError(ex, "Error occurred during data processing");
    return BadRequest("Unable to process the request.");
}
```

### 6.2 Logging Cautions

**Do NOT log:**

- Passwords, API keys, tokens
- Credit card numbers, national IDs
- Personally identifiable information (PII)

---

## 7. SQL Injection Prevention

### 7.1 Required Rules

- **Always use parameterized queries.**
- Do not build queries via string concatenation.
- Watch raw SQL even when using an ORM.

```csharp
// Bad: SQL Injection vulnerable
var query = $"SELECT * FROM Users WHERE Email = '{email}'";

// Good: parameterized query
var query = "SELECT * FROM Users WHERE Email = @Email";
command.Parameters.AddWithValue("@Email", email);

// When using Entity Framework
var user = context.Users.FirstOrDefault(u => u.Email == email);
```

---

## 8. File Upload Security

### 8.1 What to Validate

- Allowlist file extensions.
- Validate MIME types (extension alone is insufficient).
- Enforce a maximum file size.
- Inspect file contents (magic bytes).
- Store uploads outside the web root.

### 8.2 Filename Handling

```csharp
// Generate a safe filename
var safeFileName = Path.GetRandomFileName() +
    Path.GetExtension(originalFileName);

// Prevent path traversal
var fileName = Path.GetFileName(userProvidedPath);
```

---

## 9. Dependency Security

### 9.1 Management Principles

- Update dependencies regularly.
- Use vulnerability scanners (`dotnet list package --vulnerable`).
- Remove unused packages.
- Install packages only from trusted sources.

### 9.2 Vulnerability Check

```bash
# Check NuGet package vulnerabilities
dotnet list package --vulnerable

# Or use OWASP Dependency-Check
```

---

## 10. Communication Security

### 10.1 HTTPS is Mandatory

- Use TLS 1.2 or higher for all communication.
- Redirect HTTP → HTTPS.
- Set HSTS headers.

```csharp
// Force HTTPS in ASP.NET Core
app.UseHttpsRedirection();
app.UseHsts();
```

### 10.2 API Security

- Pass API keys in headers (never in URL parameters).
- Apply rate limiting.
- Tighten CORS policies.

---

## 11. Checklist

### Items to Verify During Code Review

- [ ] All user input is validated.
- [ ] SQL queries are parameterized.
- [ ] Output is appropriately encoded.
- [ ] Sensitive information is not logged.
- [ ] Error messages do not leak detail.
- [ ] File uploads are validated thoroughly.
- [ ] Cryptography uses secure algorithms.
- [ ] Access control is enforced on every endpoint.
- [ ] HTTPS is enforced.
- [ ] Dependency vulnerability scan was performed.

---

## 12. References

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/)
- [Microsoft Security Documentation](https://docs.microsoft.com/en-us/security/)
- [CWE Top 25](https://cwe.mitre.org/top25/)
