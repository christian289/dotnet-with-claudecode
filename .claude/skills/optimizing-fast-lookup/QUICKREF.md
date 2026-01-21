# Fast Lookup Quick Reference

## 1. HashSet<T>

```csharp
// O(1) existence check
var allowedIds = new HashSet<int> { 1, 2, 3, 4, 5 };

if (allowedIds.Contains(userId))
{
    // Allowed user
}
```

## 2. FrozenSet<T> (.NET 8+)

```csharp
using System.Collections.Frozen;

// Immutable fast lookup
var allowedExtensions = new[] { ".jpg", ".png", ".gif" }
    .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
```

## 3. Dictionary Optimization

```csharp
// Two lookups
if (dict.ContainsKey(key))
    var value = dict[key];

// Single lookup
if (dict.TryGetValue(key, out var value))
{
    // Use value
}
```

## 4. When to Use

| Scenario | Recommended Collection |
|----------|------------------------|
| Frequently modified set | `HashSet<T>` |
| Read-only configuration | `FrozenSet<T>` |
| Key-Value cache | `Dictionary<K,V>` |
