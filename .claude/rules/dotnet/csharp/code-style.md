# CSharp 코드 생성 지침

## 1. .NET 버전

- Newest .NET을 기본으로 사용.

## 2. 최신 C# 문법 사용

- Context7 mcp를 사용하여 기본적으로 최신 C# 문법을 사용할 것.
- 따로 .NET 또는 .NET Framework에 대한 요청이 있는 경우에만 그 버전에 맞춘 C# 문법을 이용할 것.
- 예시: collection expressions, spread element, Range Operator, string interpolation 등.

## 3. Class 설계

- 상속이 필요하지 않은 Class는 `sealed` 키워드 적용.
- Primary constructors를 적용할 수 있는 상황에서는 기본적으로 적용.

## 4. Namespace 관리

- BCL, FCL, Nuget Package에서 적용되는 Namespace는 .NET Project마다 GlobalUsings.cs 파일을 생성하여 global using을 적용할 것.
- 실제 코드 페이지에서 using으로 적용되는 부분은 사용자가 직접 정의한 namespace만 정의할 것.

## 5. Console Application

- .NET으로 Console Application Project를 작성할 때는 특별한 요청이 있는 게 아니라면 Top-Level Statement를 기본으로 설정.

## 6. 프로그래밍 패러다임

- 기본적으로는 C# 코드를 절차지향 프로그래밍(Procedural Programming)으로 작성할 것.
- 요청을 할 경우에만 객체지향 프로그래밍(Object Orientation Programming)으로 작성할 것.

## 7. Advanced .NET API

고성능 .NET API 주제는 **`microsoft-docs` MCP 플러그인**으로 조회할 것:

| 주제 | 조회 키워드 |
|------|------------|
| 메모리 효율화 | `Span<T>`, `ArrayPool`, `stackalloc` |
| 비동기 프로그래밍 | `Task`, `ValueTask`, `ConfigureAwait`, `IAsyncEnumerable` |
| 병렬 처리 | `Parallel`, `PLINQ`, `ConcurrentDictionary` |
| 고속 탐색 | `HashSet`, `FrozenSet`, `Dictionary` |
| Pub-Sub 패턴 | `System.Threading.Channels`, `System.Reactive` |
| 고속 입출력 | `FileStream`, `StreamWriter` buffering |
| Streaming | `System.IO.Pipelines`, `PipeReader`, `PipeWriter` |

> 이전 스킬(`/optimizing-memory-allocation` 등)은 `archive-skills/`로 이동됨. 공식 문서 조회가 더 최신 정보를 제공함.

## 8. 파일 구조

- .cs 파일 1개에는 Type을 1개만 정의할 것. (class와 interface 타입의 파일 분리)
- class와 interface가 1:1 관계일 때는 디버깅이 오히려 불편하기 때문에 interface를 사용하지 않고 class만 이용할 것.

## 9. Namespace 스타일

- File Scope namespace를 기본으로 사용할 것.
- Block scope 최소화

## 10. 불변 타입 최적화

- 불변 타입을 통한 최적화를 위해서 `record`, `readonly struct`, `init`, `ReadOnlyCollection<T>`, `ReadOnlyList<T>`, `ReadOnlySpan<T>`과 같은 키워드를 적극적으로 활용.
- 함수에서 return할 때 불변의 의미를 가진 값을 리턴할 때는 Readonly Type을 적용할 것.

**예시:**

```csharp
public IReadOnlyList ReadOnlyReturnFunc()
{
    List<int> result = [];

    // 중략...

    return result.AsReadOnly();
}
```

## 11. Span<T> 사용 주의사항

> **📌 상세 가이드**: `microsoft-docs` MCP로 `Span<T>`, `ReadOnlySpan<T>` 조회

- ⚠️ Span<T>, ReadOnlySpan<T>는 **async-await와 함께 사용 불가**
- ref struct이므로 Boxing 불가, 클래스 필드 저장 불가, 람다 캡처 불가

## 12. 코드 스타일

- Early Return 코드 스타일을 이용할 것.
- switch 문 사용 시 Pattern Matching 코드 스타일을 이용할 것.

## 13. Literal String 처리

> **📌 상세 가이드**: `/managing-literal-strings` skill 참조

- Literal string은 `const string`으로 사전 정의하여 사용
- Constants 클래스로 메시지 유형별 분리 관리

## 14. Console Application DI

> **📌 상세 가이드**: `/configuring-console-app-di` skill 참조

- GenericHost를 사용한 의존성 주입 패턴

## 15. Repository 패턴

> **📌 상세 가이드**: `/implementing-repository-pattern` skill 참조

- 데이터 접근 계층 추상화
- Service Layer와 함께 사용
