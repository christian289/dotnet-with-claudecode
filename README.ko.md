[🇺🇸 English](./README.md)

# dotnet-with-claudecode

Claude Code를 활용한 .NET 개발 튜토리얼

## 개요

이 저장소는 Claude Code를 사용한 .NET/WPF 개발을 위한 스킬, 규칙, 에이전트 설정을 제공합니다.

## 콘텐츠

### [wpf-dev-pack](./wpf-dev-pack)

WPF 개발을 위한 Claude Code 플러그인.

### .claude/skills

#### AvaloniaUI 스킬

- `configuring-avalonia-dependency-injection` - AvaloniaUI DI 설정
- `designing-avalonia-customcontrol-architecture` - CustomControl 아키텍처
- `structuring-avalonia-projects` - 프로젝트 구조 설계
- `using-avalonia-collectionview` - DataGridCollectionView 패턴
- `fixing-avaloniaui-radialgradientbrush` - RadialGradientBrush 호환성 수정
- `converting-html-css-to-wpf-xaml` - HTML/CSS를 WPF XAML로 변환

#### MewUI 프레임워크 스킬

- `building-mewui-apps` - MewUI 앱 설정
- `creating-mewui-controls` - MewUI 컨트롤 생성
- `binding-mewui-data` - MewUI 데이터 바인딩
- `using-mewui-layout` - MewUI 레이아웃 시스템
- `rendering-mewui-elements` - MewUI 요소 렌더링
- `navigating-mewui-tree` - MewUI 트리 탐색

#### 프로젝트 관리 스킬

- `wpf-dev-pack-release` - wpf-dev-pack 릴리스 워크플로우

## 요구사항

- Claude Code CLI
- .NET 10.0 SDK (wpf-dev-pack 훅 실행용)
- wpf-dev-pack 필수 Claude Code 플러그인:
  - [context7](https://github.com/nicobailey/context7-mcp)
  - [serena](https://github.com/oraios/serena)
  - [microsoft-docs](https://github.com/nicobailey/microsoft-docs-mcp)
  - [csharp-lsp](https://github.com/nicobailey/csharp-lsp)

## 설치

### wpf-dev-pack 설치

```bash
# 1단계: 마켓플레이스 추가 (최초 1회)
/plugin marketplace add christian289/dotnet-with-claudecode

# 2단계: 플러그인 설치
/plugin install wpf-dev-pack@dotnet-claude-plugins
```

## Git Hooks 설정

이 저장소에는 `wpf-dev-pack`의 자동 버전 업데이트를 위한 공유 git hooks가 포함되어 있습니다.

### Git Hooks 설치

저장소 클론 후 다음 중 하나를 실행하세요:

```bash
# 방법 1: 직접 설정
git config core.hooksPath .githooks

# 방법 2: 설치 스크립트 사용 (Windows PowerShell)
.\.githooks\install.ps1

# 방법 2: 설치 스크립트 사용 (Linux/Mac)
./.githooks/install.sh
```

### Hook 동작

- **pre-push**: `wpf-dev-pack/` 디렉토리 변경사항 푸시 시 자동으로 패치 버전 업데이트 (`plugin.json`과 `README.md` 제외)

## 기여

기여를 환영합니다! [CONTRIBUTING.md](.github/CONTRIBUTING.md)를 참조하세요.

## 라이선스

이 프로젝트는 [MIT 라이선스](LICENSE) 하에 배포됩니다.

## 저자

- **christian289** - [GitHub](https://github.com/christian289)
