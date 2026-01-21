# 새 케이스 추가 템플릿

새로운 HTML/CSS → WPF XAML 변환 실수를 발견했을 때 사용하는 템플릿.

---

## 케이스 추가 절차

1. 아래 템플릿을 복사
2. 해당 카테고리 파일에 추가 (clipping.md, animation.md, layout.md, transform.md)
3. [index.md](index.md)에 케이스 등록
4. 태그 섹션 업데이트

---

## 템플릿

````markdown
## C0XX: [간단한 제목] {#c0xx}

### CSS 원본

```css
/* 변환하려던 CSS 코드 */
.selector {
  property: value;
}
```
````

### 실수 내용

[무엇을 잘못했는지 1-2문장으로 설명]

### 오류 증상

[어떤 오류 메시지가 나왔는지, 또는 어떤 시각적 문제가 발생했는지]

### 원인

[왜 이런 문제가 발생하는지 - CSS와 WPF의 차이점 설명]

- 포인트 1
- 포인트 2

### 해결

```xml
<!-- ✅ 올바른 WPF XAML -->
<Element Property="Value" />
```

```xml
<!-- ❌ 잘못된 방법 (선택사항) -->
<Element Property="WrongValue" />
```

### 추가 정보 (선택사항)

[관련 문서 링크, 추가 팁 등]

### 태그

`#category1` `#category2` `#keyword`

````

---

## 케이스 번호 규칙

| 카테고리 | 번호 범위 | 예시 |
|----------|-----------|------|
| clipping | C001-C099 | C003, C004, C005 |
| animation | C100-C199 | C001 (현재, 추후 C100으로 마이그레이션 가능) |
| layout | C200-C299 | C006, C008 (현재) |
| transform | C300-C399 | C002, C007 (현재) |
| converters | C400-C499 | (예약) |
| 기타 | C900-C999 | (예약) |

> 참고: 현재 케이스 번호는 발견 순서대로 매겨졌습니다.
> 필요 시 카테고리별 번호 체계로 마이그레이션 가능합니다.

---

## 태그 목록

현재 사용 중인 태그:

### 카테고리
- `#clipping` - 클리핑 관련
- `#animation` - 애니메이션 관련
- `#layout` - 레이아웃/배치 관련
- `#transform` - 변환/회전 관련

### 키워드
- `#grid` - Grid 컨테이너
- `#canvas` - Canvas 컨테이너
- `#border` - Border 요소
- `#rotate` - 회전 변환
- `#duration` - Duration 속성
- `#staticresource` - StaticResource 바인딩
- `#cornerradius` - 둥근 모서리
- `#alignment` - 정렬 관련
- `#pseudo-element` - CSS ::before, ::after
- `#zindex` - z-order 관련
- `#converter` - IValueConverter
- `#diagonal` - 대각선 계산
- `#gradient` - 그라데이션

### 새 태그 추가 시
- 소문자, 하이픈 구분 사용
- index.md의 "태그별 분류" 섹션에 추가

---

## 예시: 새 케이스 추가

### 1. 케이스 작성 (animation.md에 추가)

```markdown
## C101: RepeatBehavior 타입 오류 {#c101}

### CSS 원본
```css
.element {
  animation: fade 2s ease infinite;
}
````

### 실수 내용

`RepeatBehavior`를 정수로 설정하려 함.

### 오류 증상

컴파일 오류 또는 애니메이션이 1회만 실행됨.

### 원인

`RepeatBehavior`는 문자열 `"Forever"` 또는 `RepeatBehavior` 구조체를 사용해야 함.

### 해결

```xml
<!-- ✅ 무한 반복 -->
<Storyboard RepeatBehavior="Forever">

<!-- ✅ 3회 반복 -->
<Storyboard RepeatBehavior="3x">
```

### 태그

`#animation` `#repeatbehavior` `#storyboard`

````

### 2. index.md 업데이트

```markdown
| C101 | RepeatBehavior 타입 오류 | `#animation` `#repeatbehavior` | [animation.md](animation.md#c101) |
````

### 3. 태그 섹션 업데이트

```markdown
### #animation

- [C001: Duration 타입 오류](animation.md#c001)
- [C101: RepeatBehavior 타입 오류](animation.md#c101) <!-- 추가 -->
```
