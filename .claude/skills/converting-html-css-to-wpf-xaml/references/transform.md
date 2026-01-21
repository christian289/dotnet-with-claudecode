# 변환/회전 관련 케이스

WPF에서 CSS transform, 크기 계산을 구현할 때 발생하는 실수들.

---

## C002: 그라데이션 바 개수 오류 {#c002}

### CSS 원본

```css
.card::before {
  content: '';
  position: absolute;
  width: 100px;
  background-image: linear-gradient(
    180deg,
    rgb(0, 183, 255),
    rgb(255, 48, 255)
  );
  height: 130%;
  animation: rotBGimg 3s linear infinite;
}
```

### 실수 내용

사용자가 "분홍색 애니메이션"을 언급하자, CSS `::before`를 **두 개의 그라데이션 바**(180도 오프셋)로 잘못 구현.

### 오류 증상

- 두 개의 바가 회전하며 어색한 애니메이션 발생
- CSS 원본과 다른 시각적 결과

### 원인

CSS `::before`의 동작을 정확히 이해하지 못함.

- **단일 세로 바**가 중앙에서 회전하면
- 바의 **상단과 하단이 자연스럽게** 테두리의 반대편에 나타남
- 추가 바가 필요 없음

### 해결

```xml
<!-- ✅ 단일 Rectangle만 사용 -->
<Rectangle x:Name="GradientBar"
           Width="100"
           Height="{Binding ActualHeight, ..., Converter=...}"
           RenderTransformOrigin="0.5,0.5">
    <Rectangle.Fill>
        <!-- 상단(cyan) → 하단(magenta) 그라데이션 -->
        <LinearGradientBrush StartPoint="0.5,0" EndPoint="0.5,1">
            <GradientStop Offset="0" Color="#00B7FF" />
            <GradientStop Offset="1" Color="#FF30FF" />
        </LinearGradientBrush>
    </Rectangle.Fill>
    <Rectangle.RenderTransform>
        <RotateTransform Angle="0" />
    </Rectangle.RenderTransform>
</Rectangle>
```

### 핵심 이해

- 세로 바가 360도 회전하면
- 0도: 상단=cyan, 하단=magenta
- 180도: 상단=magenta (아래로 갔던 것이 위로), 하단=cyan
- 자연스럽게 양쪽 색상이 번갈아 나타남

### 태그

`#transform` `#pseudo-element` `#before` `#gradient`

---

## C007: CSS height: 130% 해석 오류 {#c007}

### CSS 원본

```css
.card {
  width: 190px;
  height: 254px;
}

.card::before {
  width: 100px;
  height: 130%; /* 부모 높이의 130% */
}
```

### 실수 내용

CSS `height: 130%`를 그대로 1.3 배수로 계산.

### 오류 증상

회전 시 바가 모서리에 닿지 않음. 사각형의 모서리 부분이 비어 보임.

### 원인

바가 회전할 때 **모든 모서리에 닿으려면** 바의 높이가 카드의 **대각선 길이**보다 커야 함.

```
카드 크기: 190 x 254
대각선 길이: √(190² + 254²) ≈ 317px
254 × 1.3 = 330px → 겨우 충분하지만 여유 없음
```

### 해결

배율을 **2.0**으로 설정하여 충분한 높이 확보:

```xml
<!-- ResourceDictionary -->
<sys:String x:Key="HeightMultiplier">2.0</sys:String>

<!-- ControlTemplate -->
<Rectangle Height="{Binding ActualHeight,
                    RelativeSource={RelativeSource AncestorType=Border},
                    Converter={x:Static local:HeightMultiplierConverter.Instance},
                    ConverterParameter={StaticResource HeightMultiplier}}"
           Canvas.Top="{Binding ActualHeight,
                        RelativeSource={RelativeSource AncestorType=Border},
                        Converter={x:Static local:CenterOffsetConverter.Instance},
                        ConverterParameter={StaticResource HeightMultiplier}}" />
```

### 계산 공식

| 항목                     | 공식                                                              |
| ------------------------ | ----------------------------------------------------------------- |
| 바 높이                  | `ActualHeight × 2.0`                                              |
| Canvas.Top (중앙 오프셋) | `(ActualHeight - ActualHeight × 2.0) / 2` = `ActualHeight × -0.5` |

### 왜 2.0인가?

- 대각선 = √(w² + h²)
- 190x254의 경우: √(36100 + 64516) = √100616 ≈ 317
- 254 × 2.0 = 508 → 대각선(317)보다 충분히 큼
- 여유있게 커버하여 모든 모서리에 확실히 닿음

### 태그

`#transform` `#height` `#diagonal` `#converter`

---

## CSS transform → WPF 매핑

| CSS                           | WPF                                            |
| ----------------------------- | ---------------------------------------------- |
| `transform: rotate(45deg)`    | `<RotateTransform Angle="45" />`               |
| `transform-origin: center`    | `RenderTransformOrigin="0.5,0.5"`              |
| `transform-origin: top left`  | `RenderTransformOrigin="0,0"`                  |
| `transform: scale(1.5)`       | `<ScaleTransform ScaleX="1.5" ScaleY="1.5" />` |
| `transform: translateX(10px)` | `<TranslateTransform X="10" />`                |

### RenderTransform 적용 패턴

```xml
<Rectangle RenderTransformOrigin="0.5,0.5">
    <Rectangle.RenderTransform>
        <RotateTransform x:Name="MyRotation" Angle="0" />
    </Rectangle.RenderTransform>
</Rectangle>
```

### 애니메이션 연결

```xml
<DoubleAnimation Storyboard.TargetName="MyRotation"
                 Storyboard.TargetProperty="Angle"
                 From="0" To="360" Duration="0:0:3" />
```

### 태그

`#transform` `#rotate` `#scale` `#translate`
