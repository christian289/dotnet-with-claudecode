[🇺🇸 English](./README.md)

# Skills

WPF/.NET 개발을 위한 커맨드 스킬입니다. `formatting-wpf-csharp-code`를 제외하면
모두 슬래시로 호출하며, 이 스킬은 Claude가 자동 적용합니다(`user-invocable: false`;
편집 시 CodeFormatter PostToolUse 훅도 동작).

> **지식 토픽은 스킬이 아닙니다.** ~50개의 WPF 지식 토픽(MVVM, 렌더링,
> 스레딩, 스타일링, 서드파티 라이브러리, Prism 9 컴패니언, 테스트 등)은
> `skills/`에 번들되지 **않습니다**. 이들은 레포의 `knowledge/<id>/TOPIC.md`에
> 순수 마크다운으로 존재하며 **WpfDevPackMcp** MCP 서버가 온디맨드로
> 제공합니다(`search_wpf_topics` / `get_wpf_topic`).
> [`set-repo-path`](./set-repo-path/SKILL.md)를 한 번 실행해 서버가 읽을 로컬
> 클론 경로를 지정하세요. 자세한 내용은 플러그인 [README](../README.ko.md)의
> "스킬 & 지식" 참조.

## 커맨드 스킬 (11개)

### 🛠️ 스캐폴딩 (7개)

| 스킬 | 설명 |
|------|------|
| `make-wpf-project` | WPF 프로젝트 스캐폴드 (MVVM/DI) |
| `make-wpf-custom-control` | CustomControl 스캐폴드 |
| `make-wpf-usercontrol` | UserControl 스캐폴드 |
| `make-wpf-converter` | IValueConverter 스캐폴드 |
| `make-wpf-behavior` | Behavior<T> 스캐폴드 |
| `make-wpf-viewmodel` | ViewModel + View + DataTemplate 매핑 스캐폴드 |
| `make-wpf-service` | 서비스 인터페이스 + 구현 + DI 등록 스캐폴드 |

### 🎨 코드 품질 (1개)

| 스킬 | 설명 |
|------|------|
| `formatting-wpf-csharp-code` | C# / XAML 서식 & 스타일 (편집 시 CodeFormatter 훅이 자동 적용) |

### 🔧 플러그인 운영 (3개)

| 스킬 | 설명 |
|------|------|
| `collecting-wpf-dev-pack-feedback` | 익명화된 피드백 문서 수집 (추후 반영용) |
| `configuring-wpf-dev-pack-language` | 프로젝트별 응답 언어 설정 (`.claude/wpf-dev-pack.local.md`) |
| `set-repo-path` | WpfDevPackMcp가 지식을 읽어올 로컬 저장소 클론 경로 설정 |

## 사용법

커맨드 스킬은 직접 호출합니다:

```
/wpf-dev-pack:make-wpf-project MyApp
```

WPF 지식은 그냥 질문하면 됩니다 — WpfDevPackMcp MCP 서버의 instructions가
에이전트로 하여금 답변 전 토픽 카탈로그를 검색(`search_wpf_topics`)하고
관련 토픽을 로드(`get_wpf_topic`)하도록 안내합니다.
