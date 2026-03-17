# WPF MVVM Framework 선택 규칙

프로젝트에서 사용하는 MVVM 프레임워크를 감지하고, 적절한 코드 패턴을 적용합니다.

---

## 1. 프레임워크 감지 순서

```
1. CLAUDE.local.md에 명시적 설정이 있으면 → 해당 프레임워크 사용
2. csproj에서 NuGet 패키지 감지:
   - Prism.DryIoc 또는 Prism.Unity → Prism 9
   - CommunityToolkit.Mvvm → CommunityToolkit.Mvvm
3. 사용자 대화에서 키워드 감지:
   - "prism", "bindablebase", "delegatecommand" → Prism 9
   - "communitytoolkit", "observableproperty", "relaycommand" → CommunityToolkit.Mvvm
4. 감지 불가 → CommunityToolkit.Mvvm (기본값)
```

---

## 2. CLAUDE.local.md 설정 방법

프로젝트 루트의 `CLAUDE.local.md`에 다음 중 하나를 추가:

```markdown
## MVVM Framework
- 이 프로젝트는 **CommunityToolkit.Mvvm**을 사용합니다.
```

```markdown
## MVVM Framework
- 이 프로젝트는 **Prism 9**을 사용합니다.
```

---

## 3. 스킬 참조 분기

| MVVM Framework | Skill 코드 예제 | DI 패턴 | 네비게이션 |
|----------------|----------------|---------|-----------|
| **CommunityToolkit.Mvvm** | `SKILL.md` 참조 | GenericHost | DataTemplate 매핑 |
| **Prism 9** | `PRISM.md` 참조 | PrismApplication | RegionManager |

### 적용 규칙

```
WHEN generating ViewModel code:
  IF Prism 9 → Use BindableBase, SetProperty, DelegateCommand
  IF CommunityToolkit.Mvvm → Use ObservableObject, [ObservableProperty], [RelayCommand]

WHEN referencing skill examples:
  IF Prism 9 → Load PRISM.md (있을 경우), 없으면 SKILL.md
  IF CommunityToolkit.Mvvm → Load SKILL.md

WHEN setting up DI:
  IF Prism 9 → PrismApplication + IContainerRegistry
  IF CommunityToolkit.Mvvm → GenericHost + IServiceCollection
```

---

## 4. 프레임워크별 상세 규칙

- CommunityToolkit.Mvvm → `communitytoolkit-mvvm.md` 참조
- Prism 9 → `prism9.md` 참조
