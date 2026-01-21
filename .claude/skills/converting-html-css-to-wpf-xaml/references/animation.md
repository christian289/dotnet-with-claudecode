# 애니메이션 관련 케이스

WPF에서 CSS animation을 Storyboard로 변환할 때 발생하는 실수들.

---

## C001: Duration 타입 오류 {#c001}

### CSS 원본

```css
.card::before {
  animation: rotBGimg 3s linear infinite;
}

@keyframes rotBGimg {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}
```

### 실수 내용

CSS `animation: rotBGimg 3s`의 `3s`를 WPF로 변환할 때, `sys:TimeSpan`을 ResourceDictionary에 정의하고 `Duration="{StaticResource ...}"`로 바인딩.

```xml
<!-- ❌ 잘못된 시도 -->
<sys:TimeSpan x:Key="RotationDuration">0:0:3</sys:TimeSpan>
...
<DoubleAnimation Duration="{StaticResource RotationDuration}" />
```

### 오류 메시지

```
'System.Windows.Media.Animation.Timeline.Duration' 설정에서 예외 throw 되었습니다.
```

### 원인

- WPF `Duration` 속성은 `System.Windows.Duration` 타입
- `System.TimeSpan`과 `System.Windows.Duration`은 **다른 타입**
- XAML에서 문자열 `"0:0:3"`은 `Duration`으로 자동 변환되지만
- `StaticResource`로 `TimeSpan`을 바인딩하면 타입 변환이 일어나지 않음

### 해결

```xml
<!-- ✅ Duration은 항상 인라인으로 작성 -->
<DoubleAnimation Storyboard.TargetName="GradientBarRotation"
                 Storyboard.TargetProperty="Angle"
                 From="0"
                 To="360"
                 Duration="0:0:3" />
```

### CSS → WPF 애니메이션 매핑

| CSS                                   | WPF                                            |
| ------------------------------------- | ---------------------------------------------- |
| `animation-duration: 3s`              | `Duration="0:0:3"`                             |
| `animation-timing-function: linear`   | (기본값, 별도 설정 불필요)                     |
| `animation-iteration-count: infinite` | `RepeatBehavior="Forever"` (Storyboard에 설정) |
| `@keyframes from/to`                  | `From="0" To="360"`                            |

### 전체 패턴

```xml
<ControlTemplate.Triggers>
    <EventTrigger RoutedEvent="Loaded">
        <BeginStoryboard>
            <Storyboard RepeatBehavior="Forever">
                <DoubleAnimation Storyboard.TargetName="RotateTransformName"
                                 Storyboard.TargetProperty="Angle"
                                 From="0"
                                 To="360"
                                 Duration="0:0:3" />
            </Storyboard>
        </BeginStoryboard>
    </EventTrigger>
</ControlTemplate.Triggers>
```

### 태그

`#animation` `#duration` `#staticresource` `#storyboard`
