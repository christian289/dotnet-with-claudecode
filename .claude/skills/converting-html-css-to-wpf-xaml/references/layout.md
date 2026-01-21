# 레이아웃 관련 케이스

WPF에서 CSS position, pseudo-element, 정렬을 구현할 때 발생하는 실수들.

---

## C006: 회전 요소 Grid 컨테이너 오류 {#c006}

### CSS 원본

```css
.card::before {
  content: '';
  position: absolute;
  width: 100px;
  height: 130%;
  animation: rotBGimg 3s linear infinite;
}
```

### 실수 내용

Grid 내에서 `HorizontalAlignment="Center"`, `VerticalAlignment="Center"`로 회전 요소 배치.

### 오류 증상

- 바인딩이 동작하지 않음
- 또는 렌더링 문제 발생 (요소가 잘리거나 안 보임)

### 원인

- Grid는 자식 요소의 크기를 **레이아웃 시점에 결정**
- 회전하는 요소가 부모 영역을 벗어나면 클리핑되거나 렌더링 문제 발생
- Grid는 레이아웃 영역 밖으로 나가는 요소를 처리하지 못함

### 해결

- **Canvas 사용**: 자식 요소의 크기와 위치를 절대 좌표로 지정
- `Canvas.Left`, `Canvas.Top`으로 위치 지정
- Canvas는 자식의 크기를 **제한하지 않음** → 회전해도 렌더링됨

```xml
<!-- ❌ Grid - 회전 요소가 잘릴 수 있음 -->
<Grid>
    <Rectangle HorizontalAlignment="Center"
               VerticalAlignment="Center"
               RenderTransformOrigin="0.5,0.5">
        <Rectangle.RenderTransform>
            <RotateTransform Angle="0" />
        </Rectangle.RenderTransform>
    </Rectangle>
</Grid>

<!-- ✅ Canvas - 회전 요소가 자유롭게 렌더링 -->
<Canvas>
    <Rectangle Canvas.Left="45"
               Canvas.Top="{Binding ActualHeight, ..., Converter={x:Static local:CenterOffsetConverter.Instance}}"
               RenderTransformOrigin="0.5,0.5">
        <Rectangle.RenderTransform>
            <RotateTransform Angle="0" />
        </Rectangle.RenderTransform>
    </Rectangle>
</Canvas>
```

### CSS `position: absolute` → WPF 매핑

| CSS                  | WPF                          |
| -------------------- | ---------------------------- |
| `position: absolute` | `Canvas` 컨테이너 사용       |
| `left: 45px`         | `Canvas.Left="45"`           |
| `top: 50%`           | `Canvas.Top="{Binding ...}"` |

### 태그

`#layout` `#canvas` `#grid` `#rotate` `#position-absolute`

---

## C008: ContentPresenter Canvas 내 배치 오류 {#c008}

### CSS 원본

```css
.card h2 {
  z-index: 1;
  color: white;
  font-size: 2em;
}
```

### 실수 내용

Canvas 내에 ContentPresenter를 배치하여 절대 좌표로 위치 지정 시도.

### 오류 증상

중앙 정렬이 제대로 작동하지 않음. ContentPresenter가 (0, 0) 위치에 고정됨.

### 원인

- Canvas 내에서는 `HorizontalAlignment`, `VerticalAlignment`가 **무시**됨
- Canvas는 자식 요소를 `Canvas.Left`, `Canvas.Top`으로만 배치
- Alignment 속성은 아무 효과 없음

### 해결

- ContentPresenter는 **Canvas 밖 Grid에 배치**
- Grid 레벨에서 중앙 정렬 적용

```xml
<!-- ❌ Canvas 내 ContentPresenter - Alignment 무시됨 -->
<Canvas>
    <Rectangle ... />
    <ContentPresenter HorizontalAlignment="Center"
                      VerticalAlignment="Center" />  <!-- 작동 안 함! -->
</Canvas>

<!-- ✅ Grid 내 ContentPresenter + Canvas 분리 -->
<Grid>
    <Canvas>
        <!-- 회전 요소들 (::before, ::after) -->
        <Rectangle ... />
        <Border ... />
    </Canvas>

    <!-- ContentPresenter는 Grid에서 정렬 -->
    <ContentPresenter HorizontalAlignment="{TemplateBinding HorizontalContentAlignment}"
                      VerticalAlignment="{TemplateBinding VerticalContentAlignment}" />
</Grid>
```

### z-order 제어

Grid 내에서 선언 순서가 z-order 결정:

- Canvas (먼저 선언) → 아래 레이어
- ContentPresenter (나중 선언) → 위 레이어

또는 명시적으로 `Panel.ZIndex` 사용:

```xml
<Canvas Panel.ZIndex="0" />
<ContentPresenter Panel.ZIndex="1" />
```

### 태그

`#layout` `#canvas` `#alignment` `#contentpresenter` `#zindex`

---

## CSS Pseudo-element → WPF 구현 패턴

### CSS ::before, ::after

WPF에는 pseudo-element가 없으므로 **Canvas 내 요소로 구현**.

```css
/* CSS */
.card::before {
  /* 첫 번째 레이어 */
}
.card::after {
  /* 두 번째 레이어 */
}
.card h2 {
  z-index: 1; /* 최상위 */
}
```

```xml
<!-- WPF: 선언 순서로 z-order 제어 -->
<Grid>
    <Canvas>
        <!-- ::before 역할 (먼저 선언 = 아래) -->
        <Rectangle ... />

        <!-- ::after 역할 (나중 선언 = 위) -->
        <Border ... />
    </Canvas>

    <!-- h2 역할 (Canvas 밖 = 최상위) -->
    <ContentPresenter ... />
</Grid>
```

### 태그

`#layout` `#pseudo-element` `#before` `#after` `#zindex`
