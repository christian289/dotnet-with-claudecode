# AvaloniaUI Project 코드 생성 지침

## 핵심 원칙

- .NET CSharp 코드 생성 지침을 기본으로 할 것
- WPF 지침의 MVVM 원칙을 동일하게 적용
- UI 커스터마이징 시 Avalonia Custom Control Library 프로젝트 사용
- Converter, AvaloniaUI Service Layer는 Avalonia Class Library 프로젝트 사용
- CommunityToolkit.Mvvm NuGet Package 사용

---

## 1. Dependency Injection

> **📌 상세 가이드**: `/configuring-avalonia-dependency-injection` skill 참조

- 기본적으로 AddSingleton()만 사용
- GenericHost로 DI 컨테이너 구성

---

## 2. 솔루션 및 프로젝트 구조

> **📌 상세 가이드**: `/structuring-avalonia-projects` skill 참조

**프로젝트 명명 규칙:**

| 접미사 | 타입 | 용도 |
|--------|------|------|
| `.Abstractions` | .NET Class Library | Interface, abstract class (IoC) |
| `.Core` | .NET Class Library | 비즈니스 로직 (UI 독립) |
| `.ViewModels` | .NET Class Library | MVVM ViewModel (UI 독립) |
| `.AvaloniaServices` | Avalonia Class Library | Avalonia 관련 서비스 |
| `.AvaloniaLib` | Avalonia Class Library | 재사용 가능한 컴포넌트 |
| `.AvaloniaApp` | Avalonia Application | 실행 진입점 |
| `.UI` | Avalonia Custom Control Library | 커스텀 컨트롤 |

---

## 3. MVVM 패턴

> **📌 상세 가이드**: `/implementing-communitytoolkit-mvvm` skill 참조

### 핵심 제약

- **ViewModel 클래스에 UI 프레임워크 의존성 금지**
  - `Avalonia`로 시작하는 네임스페이스 참조 금지
  - 예외: Custom Control 프로젝트 내부 ViewModel
- **MVVM 제약은 ViewModel에만 적용**
  - Converter, Service, Manager는 UI 프레임워크 참조 가능

### 참조 어셈블리 규칙

**ViewModel 프로젝트 참조 금지:**
- ❌ `Avalonia.Base.dll`
- ❌ `Avalonia.Controls.dll`
- ❌ `Avalonia.Markup.Xaml.dll`

**ViewModel 프로젝트 참조 가능:**
- ✅ BCL 타입만 (IEnumerable, ObservableCollection 등)
- ✅ CommunityToolkit.Mvvm

---

## 4. AXAML 코드 작성

> **📌 상세 가이드**: `/designing-avalonia-customcontrol-architecture` skill 참조

- CustomControl + ControlTheme을 통한 Stand-Alone Control Style 사용
- Generic.axaml은 MergedDictionaries 허브로만 사용
- 각 컨트롤 ControlTheme을 개별 AXAML 파일로 분리
- StyledProperty 사용 (DependencyProperty 대신)
- CSS Class 기반 스타일 적용 (Classes 속성)
- Pseudo Classes를 사용한 상태 관리 (:pointerover, :pressed 등)

---

## 5. CollectionView 패턴

> **📌 상세 가이드**: `/using-avalonia-collectionview` skill 참조

**⚠️ AvaloniaUI는 WPF의 CollectionViewSource를 지원하지 않음**

- DataGridCollectionView 사용 (권장)
- 또는 ReactiveUI + DynamicData 사용

---

## 6. DataTemplate View-ViewModel 매핑

> **📌 상세 가이드**: `/mapping-viewmodel-view-datatemplate` skill 참조

- Mappings.axaml에 ViewModel-View DataTemplate 정의
- ContentControl.Content에 ViewModel 바인딩하여 자동 View 렌더링

---

## 7. WPF vs AvaloniaUI 주요 차이점

| 항목 | WPF | AvaloniaUI |
|------|-----|------------|
| 파일 확장자 | .xaml | .axaml |
| 스타일 정의 | Style + ControlTemplate | ControlTheme |
| 상태 관리 | Trigger, DataTrigger | Pseudo Classes, Style Selector |
| CSS 지원 | ❌ | ✅ (Classes 속성) |
| 리소스 병합 | MergedDictionaries + ResourceDictionary | MergedDictionaries + ResourceInclude |
| 의존성 속성 | DependencyProperty | StyledProperty, DirectProperty |
| CollectionView | CollectionViewSource | DataGridCollectionView, ReactiveUI |

**⚠️ ResourceInclude vs MergeResourceInclude:**
- **ResourceInclude**: 일반 ResourceDictionary 파일에서 사용
- **MergeResourceInclude**: App.axaml의 Application.Resources에서만 사용

---

## 8. 필수 NuGet 패키지

```xml
<!-- AvaloniaUI Application -->
<ItemGroup>
  <PackageReference Include="Avalonia" Version="11.0.*" />
  <PackageReference Include="Avalonia.Desktop" Version="11.0.*" />
  <PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.*" />
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.*" />
  <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
</ItemGroup>

<!-- Optional: DataGrid support -->
<ItemGroup>
  <PackageReference Include="Avalonia.Controls.DataGrid" Version="11.0.*" />
</ItemGroup>

<!-- Optional: ReactiveUI support -->
<ItemGroup>
  <PackageReference Include="ReactiveUI.Avalonia" Version="20.1.*" />
  <PackageReference Include="DynamicData" Version="9.0.*" />
</ItemGroup>
```

---

## 9. 체크리스트

### AvaloniaUI 프로젝트

- [ ] ViewModel에 Avalonia 참조 없음 확인
- [ ] ViewModel은 순수 BCL 타입만 사용
- [ ] CustomControl은 기존 Avalonia 컨트롤 상속
- [ ] CustomControl에서 StyledProperty 사용
- [ ] Generic.axaml은 MergedDictionaries 허브로만 사용
- [ ] 각 컨트롤 ControlTheme을 개별 AXAML 파일로 분리
- [ ] CSS Class 기반 스타일 적용
- [ ] Pseudo Classes를 사용한 상태 관리
- [ ] CollectionView 대신 DataGridCollectionView 사용
- [ ] App.axaml.cs에서 GenericHost 설정 및 DI 컨테이너 구성

---

## 10. 주의사항

### ⚠️ 자주 발생하는 실수

1. ViewModel에 Avalonia 네임스페이스 참조 - MVVM 위반
2. ViewModel에서 Avalonia.Base.dll, Avalonia.Controls.dll 참조
3. CustomControl을 TemplatedControl에서 직접 상속 - 기존 컨트롤 상속 필요
4. WPF의 DependencyProperty를 그대로 사용 - StyledProperty 사용 필요
5. WPF의 Trigger를 그대로 사용 - Pseudo Classes와 Style Selector 사용 필요
6. WPF의 CollectionViewSource 사용 - DataGridCollectionView 또는 ReactiveUI 사용 필요
7. Generic.axaml에 직접 ControlTheme 작성 - 개별 파일로 분리 후 ResourceInclude
8. App.axaml.cs에서 GenericHost 설정 누락

---

## 11. 공식 문서

- [AvaloniaUI Documentation](https://docs.avaloniaui.net/)
- [Styled Properties](https://docs.avaloniaui.net/docs/guides/custom-controls/defining-properties)
- [Control Themes](https://docs.avaloniaui.net/docs/guides/styles-and-resources/control-themes)
- [MVVM Pattern](https://docs.avaloniaui.net/docs/concepts/the-mvvm-pattern/)
- [Dependency Injection](https://docs.avaloniaui.net/docs/guides/implementation-guides/how-to-use-dependency-injection)
