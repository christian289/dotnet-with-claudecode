---
name: html-css-to-wpf-xaml
description: HTML/CSS를 WPF CustomControl XAML로 변환할 때 필요한 가이드. CSS 애니메이션, overflow 클리핑, pseudo-element, 회전 요소 등을 WPF로 변환할 때 발생하는 일반적인 실수와 올바른 패턴을 제공한다. 다음 상황에서 사용: (1) HTML/CSS를 WPF XAML로 변환, (2) CSS 애니메이션을 WPF Storyboard로 변환, (3) CSS border-radius + overflow: hidden을 WPF 클리핑으로 구현, (4) CSS ::before/::after pseudo-element를 WPF로 구현, (5) CSS transform: rotate를 WPF RotateTransform으로 변환
---

# HTML/CSS → WPF XAML 변환 가이드

## CSS → WPF 핵심 매핑 테이블

| CSS                                  | WPF 구현 방법                                                  | 참조                                    |
| ------------------------------------ | -------------------------------------------------------------- | --------------------------------------- |
| `overflow: hidden` + `border-radius` | `Border.Clip` + `RectangleGeometry` (RadiusX/Y + MultiBinding) | [clipping.md](references/clipping.md)   |
| `position: absolute` (회전 요소)     | `Canvas` + `Canvas.Left/Top`                                   | [layout.md](references/layout.md)       |
| `animation-duration: 3s`             | `Duration="0:0:3"` 인라인                                      | [animation.md](references/animation.md) |
| `height: 130%` (회전 요소)           | Converter로 동적 계산 (배율 2.0)                               | [transform.md](references/transform.md) |
| `::before`, `::after`                | Canvas 내 요소, 선언 순서로 z-order                            | [layout.md](references/layout.md)       |
| `z-index`                            | 선언 순서 또는 `Panel.ZIndex`                                  | [layout.md](references/layout.md)       |
| 중앙 정렬 콘텐츠                     | Canvas 밖 Grid에서 Alignment 적용                              | [layout.md](references/layout.md)       |
| `spacing`                            | Maring 속성으로 대체                                           | -                                       |

## 핵심 규칙 요약

### 1. Duration은 항상 인라인

```xml
<!-- ✅ -->
<DoubleAnimation Duration="0:0:3" />
<!-- ❌ StaticResource 바인딩 불가 -->
```

### 2. 둥근 모서리 클리핑은 Border.Clip + RectangleGeometry

```xml
<Border CornerRadius="20">
    <Border.Clip>
        <RectangleGeometry RadiusX="20" RadiusY="20">
            <RectangleGeometry.Rect>
                <MultiBinding Converter="{x:Static local:SizeToRectConverter.Instance}">
                    <Binding Path="ActualWidth" RelativeSource="{RelativeSource AncestorType=Border}" />
                    <Binding Path="ActualHeight" RelativeSource="{RelativeSource AncestorType=Border}" />
                </MultiBinding>
            </RectangleGeometry.Rect>
        </RectangleGeometry>
    </Border.Clip>
</Border>
```

### 3. 회전 요소는 Canvas 내 배치

```xml
<Canvas>
    <Rectangle Canvas.Left="45" Canvas.Top="{Binding ...}" RenderTransformOrigin="0.5,0.5">
        <Rectangle.RenderTransform>
            <RotateTransform Angle="0" />
        </Rectangle.RenderTransform>
    </Rectangle>
</Canvas>
```

### 4. ContentPresenter는 Canvas 밖 Grid에 배치

```xml
<Grid>
    <Canvas><!-- 회전 요소들 --></Canvas>
    <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center" />
</Grid>
```

## 참조 문서

| 파일                                                       | 내용                                                    |
| ---------------------------------------------------------- | ------------------------------------------------------- |
| [references/index.md](references/index.md)                 | 전체 케이스 목록 (빠른 검색용)                          |
| [references/clipping.md](references/clipping.md)           | 클리핑 관련 실수 (Grid.Clip, OpacityMask, ClipToBounds) |
| [references/animation.md](references/animation.md)         | 애니메이션/Duration 관련                                |
| [references/layout.md](references/layout.md)               | Canvas/Grid/정렬, pseudo-element 관련                   |
| [references/transform.md](references/transform.md)         | 회전/높이 계산 관련                                     |
| [references/converters.md](references/converters.md)       | 필수 Converter 패턴                                     |
| [references/case-template.md](references/case-template.md) | 새 케이스 추가용 템플릿                                 |
