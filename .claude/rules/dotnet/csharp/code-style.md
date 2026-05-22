# C# Code Generation Guidelines

## 1. .NET Version

- Use the newest .NET by default.

## 2. Use the Latest C# Syntax

- Use the Context7 MCP and default to the latest C# syntax.
- Use older syntax only when the user explicitly targets a specific
  .NET / .NET Framework version.
- Examples to prefer: collection expressions, spread element, range
  operator, string interpolation, primary constructors, etc.

## 3. Class Design

- Apply the `sealed` keyword to classes that do not need to be
  inherited from.
- Apply primary constructors wherever they fit.

## 4. Namespace Management

- For namespaces that come from the BCL, FCL, or NuGet packages, create
  a `GlobalUsings.cs` file in each .NET project and consolidate
  `global using` declarations there.
- Inside individual code files, only `using` user-defined namespaces.

## 5. Console Applications

- For .NET console applications, default to top-level statements unless
  the user specifies otherwise.

## 6. Programming Paradigm

- Default to procedural programming for C# code.
- Use object-oriented programming only when explicitly requested.

## 7. Advanced .NET APIs

For high-performance .NET API topics, consult them through the
**`microsoft-docs` MCP plugin**:

| Topic | Lookup keywords |
|-------|-----------------|
| Memory efficiency | `Span<T>`, `ArrayPool`, `stackalloc` |
| Async programming | `Task`, `ValueTask`, `ConfigureAwait`, `IAsyncEnumerable` |
| Parallelism | `Parallel`, `PLINQ`, `ConcurrentDictionary` |
| Fast lookup | `HashSet`, `FrozenSet`, `Dictionary` |
| Pub/Sub patterns | `System.Threading.Channels`, `System.Reactive` |
| Fast I/O | `FileStream`, `StreamWriter` buffering |
| Streaming | `System.IO.Pipelines`, `PipeReader`, `PipeWriter` |

> Previous skills (e.g., `/optimizing-memory-allocation`) have been
> moved to `archive-skills/`. Looking these topics up in the official
> docs yields more up-to-date information.

## 8. File Structure

- Define one type per `.cs` file (separate files for classes and
  interfaces).
- When a class and interface have a strict 1:1 relationship, drop the
  interface and keep only the class — the interface adds debugging
  friction with no payoff.

## 9. Namespace Style

- Default to file-scoped namespaces.
- Minimize block-scoped namespaces.

## 10. Immutable-Type Optimization

- For optimization through immutability, actively use `record`,
  `readonly struct`, `init`, `ReadOnlyCollection<T>`,
  `ReadOnlyList<T>`, `ReadOnlySpan<T>`, and related types.
- When a function returns a value that is semantically immutable,
  return a read-only type.

**Example:**

```csharp
public IReadOnlyList<int> ReadOnlyReturnFunc()
{
    List<int> result = [];

    // ...

    return result.AsReadOnly();
}
```

## 11. `Span<T>` Caveats

> **📌 Detailed guide**: look up `Span<T>` and `ReadOnlySpan<T>` via the
> `microsoft-docs` MCP.

- ⚠️ `Span<T>` and `ReadOnlySpan<T>` **cannot be used together with
  async/await**.
- They are `ref struct`s, so: no boxing, cannot be stored as class
  fields, cannot be captured by lambdas.

## 12. Code Style

- Use early-return style.
- For `switch` statements, use pattern matching style.

## 13. Literal String Handling

> **📌 Detailed guide**: see the `/managing-literal-strings` skill.

- Predefine literal strings as `const string` and reference those.
- Use Constants classes that segregate strings by message category.

## 14. Console Application DI

> **📌 Detailed guide**: see the `/configuring-console-app-di` skill.

- Wire dependency injection using GenericHost.

## 15. Repository Pattern

> **📌 Detailed guide**: see the `/implementing-repository-pattern` skill.

- Abstract the data access layer.
- Pair it with a Service Layer.
