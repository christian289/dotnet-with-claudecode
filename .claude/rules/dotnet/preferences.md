# Coding 관련 공통 지침

## 1. MCP 사용

- Context7 mcp 사용.
- Serena mcp 사용.
- Microsoft 기술에 관련해서는 MicrosoftDocs mcp를 사용.

## 2. 다른 언어 코드 변환

- 이 프로젝트에서 .NET C#이 아닌 프로그래밍 코드를 질문할 때 다른 언어의 코드 생성 후 C#으로 코드를 변환했을 때는 어떤 뉘앙스의 코드인지 함께 남겨줄 것.

## 3. ProtoTyping 원칙

- ProtoTyping 관련 코드는 최소한으로 생성.
- 코드는 짧고 명료하게 생성.
- 추상화를 하지 않을 것.

## 4. 답변 범위

- Plan Mode를 선행한 뒤에 사용자에게 확인 받고 나서 진행할 것.

### 2.5 한글 문장과 영문 병기

- **조건부 규칙**: 한글을 먼저 **작성하기로 선택한 경우에만** 영문 병기. 항상 병기가 아님.
- 영문이 기본 언어인 문서(예: `SKILL.md`, 영문 주석 기반 코드)의 예시에는 영문 단일 주석 사용. 한글 병기 금지.
- 한글을 쓰기로 한 경우: Log, Comment, Exception Message 등의 한글 코드 바로 밑에 동일 의미의 영문 코드를 추가할 것. (코드 페이지 전체를 영역하는 것이 아니라, 해당 한글 줄의 영문 대응 줄만 추가)

**예시:**

```csharp
// 사용자 인증 실패
// User authentication failed
throw new AuthenticationException("인증에 실패했습니다.");
throw new AuthenticationException("Authentication failed.");
```
