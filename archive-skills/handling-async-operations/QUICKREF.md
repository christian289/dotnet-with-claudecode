# Async Programming Quick Reference

## 1. Task vs ValueTask

```csharp
// General async: Use Task
public async Task<Data> LoadAsync() { }

// Frequent cache hits: Use ValueTask
public ValueTask<Data> GetAsync(string key)
{
    if (_cache.TryGetValue(key, out var cached))
        return new ValueTask<Data>(cached);

    return new ValueTask<Data>(LoadFromDbAsync(key));
}
```

## 2. CancellationToken

```csharp
public async Task<Data> LoadAsync(CancellationToken ct = default)
{
    ct.ThrowIfCancellationRequested();
    return await _httpClient.GetFromJsonAsync<Data>(url, ct);
}

// Timeout
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
await LongOperationAsync(cts.Token);
```

## 3. Anti-patterns

```csharp
// No async void
public async void BadMethod() { }

// No .Result, .Wait() (deadlock)
var result = GetDataAsync().Result;

// Use await
var result = await GetDataAsync();
```
