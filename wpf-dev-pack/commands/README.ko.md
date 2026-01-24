[🇺🇸 English](./README.md)

# Commands

WPF 개발을 위한 사용자 호출 가능한 슬래시 명령어입니다.

## 명령어 목록

| 명령어 | 설명 |
|--------|------|
| `/wpf-dev-pack:make-wpf-project` | MVVM 구조로 새 WPF 프로젝트 스캐폴드 |
| `/wpf-dev-pack:make-wpf-custom-control` | ControlTemplate이 포함된 CustomControl 생성 |
| `/wpf-dev-pack:make-wpf-usercontrol` | XAML과 코드비하인드가 포함된 UserControl 생성 |
| `/wpf-dev-pack:make-wpf-converter` | IValueConverter 또는 IMultiValueConverter 생성 |
| `/wpf-dev-pack:make-wpf-behavior` | Microsoft.Xaml.Behaviors를 사용한 Behavior<T> 생성 |

## 사용 예시

### 새 프로젝트 생성

```bash
# CommunityToolkit.Mvvm 사용 (권장)
/wpf-dev-pack:make-wpf-project MyApp

# Prism Framework 사용
/wpf-dev-pack:make-wpf-project MyApp --prism
```

### 컴포넌트 생성

```bash
# Button을 상속하는 CustomControl
/wpf-dev-pack:make-wpf-custom-control MyButton Button

# UserControl
/wpf-dev-pack:make-wpf-usercontrol SearchBox

# Bool to Visibility Converter
/wpf-dev-pack:make-wpf-converter BoolToVisibility

# TextBox Behavior
/wpf-dev-pack:make-wpf-behavior SelectAllOnFocus TextBox
```

## 디렉토리 구조

각 명령어는 다음을 포함합니다:
- `COMMAND.md` - 명령어 정의 및 지침
- 추가 템플릿 또는 스크립트 (필요시)
