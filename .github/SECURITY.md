# Security Policy

## Reporting a Vulnerability

보안 취약점을 발견하셨다면, **공개 Issue로 등록하지 마시고** 아래 방법으로 비공개 보고해주세요.

### 보고 방법

1. GitHub Security Advisories 사용 (권장)
   - [Security Advisories](https://github.com/christian289/dotnet-with-claudecode/security/advisories/new)에서 비공개 보고

2. 또는 이메일로 연락
   - 저장소 소유자에게 직접 연락

### 보고 시 포함할 내용

- 취약점 설명
- 재현 단계
- 영향 범위
- 가능하다면 수정 제안

### 응답 시간

- 초기 응답: 48시간 이내
- 상태 업데이트: 7일 이내

## Supported Versions

| Version | Supported |
|---------|-----------|
| latest  | ✅        |

## Scope

이 프로젝트는 Claude Code용 스킬/에이전트 정의 파일을 제공합니다.
실행 코드가 아닌 설정 파일이므로, 주요 보안 고려사항은:

- 민감한 정보 노출 방지
- 악성 명령어 주입 방지
- 안전하지 않은 패턴 권장 방지
