# 클리핑 관련 케이스

WPF에서 CSS `overflow: hidden` + `border-radius` 조합을 구현할 때 발생하는 실수들.

---

## C003: Grid.Clip 클리핑 오류 {#c003}

### CSS 원본

```css
.card {
  overflow: hidden;
  border-radius: 20px;
}
```

### 실수 내용

Grid.Clip에 RectangleGeometry를 바인딩하여 둥근 모서리 클리핑 시도. 내부 요소(회전하는 Rectangle)가 렌더링 자체가 되지 않음.

### 오류 증상

- 회전하는 요소가 화면에 표시되지 않음
- 또는 일부만 보이고 잘림

### 원인

- Grid.Clip은 Grid의 레이아웃 영역 기준으로 클리핑
- 회전하는 Rectangle이 Grid 영역을 벗어나면 **렌더링 전에** 잘림
- Grid는 자식 요소의 크기를 레이아웃 시점에 결정하므로 회전 변환 후 영역을 고려하지 않음

### 해결

```xml
<!-- Border.Clip 사용 + 내부에 Canvas 배치 -->
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
    <Canvas>
        <!-- Canvas는 자식 크기를 제한하지 않아 회전해도 렌더링됨 -->
        <Rectangle ... RenderTransformOrigin="0.5,0.5">
            <Rectangle.RenderTransform>
                <RotateTransform Angle="0" />
            </Rectangle.RenderTransform>
        </Rectangle>
    </Canvas>
</Border>
```

### 태그

`#clipping` `#grid` `#rotate` `#canvas`

---

## C004: OpacityMask 클리핑 오류 {#c004}

### CSS 원본

```css
.card {
  overflow: hidden;
  border-radius: 20px;
}
```

### 실수 내용

Grid.Clip 대신 OpacityMask와 VisualBrush를 사용하여 클리핑 시도.

### 오류 증상

그라데이션 바가 둥근 모서리 바깥으로 튀어나옴.

### 원인

- OpacityMask는 **투명도만 조절**
- 레이아웃 클리핑(실제로 영역을 잘라내는 것)을 하지 않음
- 요소는 여전히 전체 영역에 렌더링되고, 마스크 영역 외부는 투명해질 뿐 잘리지 않음

### 해결

OpacityMask 대신 **Border.Clip + RectangleGeometry** 사용.

```xml
<!-- ❌ OpacityMask - 클리핑 안 됨 -->
<Grid>
    <Grid.OpacityMask>
        <VisualBrush>
            <VisualBrush.Visual>
                <Border Background="White" CornerRadius="20" />
            </VisualBrush.Visual>
        </VisualBrush>
    </Grid.OpacityMask>
</Grid>

<!-- ✅ Border.Clip - 실제 클리핑 -->
<Border>
    <Border.Clip>
        <RectangleGeometry RadiusX="20" RadiusY="20" ... />
    </Border.Clip>
</Border>
```

### 태그

`#clipping` `#opacitymask` `#visualbrush`

---

## C005: ClipToBounds + CornerRadius 오류 {#c005}

### CSS 원본

```css
.card {
  overflow: hidden;
  border-radius: 20px;
}
```

### 실수 내용

Border에 `ClipToBounds="True"`와 `CornerRadius`를 함께 사용하면 둥근 모서리로 클리핑될 것으로 기대.

### 오류 증상

자식 요소가 둥근 모서리 바깥으로 튀어나옴. 사각형으로만 클리핑됨.

### 원인

- WPF `ClipToBounds="True"`는 **사각형 영역으로만 클리핑**
- `CornerRadius`는 **시각적 모서리만** 둥글게 함
- 클리핑 영역 자체는 여전히 직사각형

### 해결

```xml
<!-- ❌ ClipToBounds만으로는 부족 -->
<Border ClipToBounds="True" CornerRadius="20">
    <!-- 자식이 모서리 밖으로 나감 -->
</Border>

<!-- ✅ Border.Clip + RectangleGeometry -->
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
    <!-- content -->
</Border>
```

### 핵심 포인트

- `CornerRadius`와 `RadiusX/RadiusY` 값을 **동일하게** 유지
- `ActualWidth/ActualHeight`를 바인딩하여 동적 크기 대응
- `SizeToRectConverter`로 `(0, 0, ActualWidth, ActualHeight)` Rect 생성

### 태그

`#clipping` `#cornerradius` `#border` `#cliptobounds`
