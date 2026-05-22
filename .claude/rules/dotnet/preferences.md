# Coding Preferences

## 1. MCP Usage

- Use the Context7 MCP.
- Use the Serena MCP.
- For Microsoft-specific topics, use the MicrosoftDocs MCP.

## 2. Converting Code from Other Languages

- When the user asks about programming in a language other than .NET C#,
  if you first generate code in that language and then convert it to C#,
  also describe the nuance of the converted code (idioms, ownership,
  exception model, etc., as relevant).

## 3. Prototyping Principles

- Generate prototyping code as minimally as possible.
- Keep the code short and unambiguous.
- Do not introduce abstractions.

## 4. Scope of Answers

- Proceed only after entering Plan Mode and getting confirmation from
  the user.

### 2.5 Korean Text with Inline English

- **Conditional rule**: When you have **chosen** to write Korean first,
  also add a parallel English line. This is not "always bilingual".
- For documents where English is the primary language (e.g., `SKILL.md`,
  English-only code comments), keep the examples English-only — do not
  add Korean alongside.
- When Korean is the chosen language: directly under each Korean line
  (in log messages, comments, exception messages, etc.), add the
  equivalent English line. Translate per-line, not per-page.

**Example:**

```csharp
// 사용자 인증 실패
// User authentication failed
throw new AuthenticationException("인증에 실패했습니다.");
throw new AuthenticationException("Authentication failed.");
```
