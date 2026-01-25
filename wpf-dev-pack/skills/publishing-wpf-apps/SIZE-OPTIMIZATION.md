# Size Optimization for WPF Apps

---

## Options Comparison

| Option | Effect | WPF Support |
|--------|--------|-------------|
| EnableCompressionInSingleFile | 50-80MB compressed | ✅ |
| PublishReadyToRun | +Size, faster startup | ✅ |
| InvariantGlobalization | Slight reduction | ✅ |
| PublishTrimmed | Smallest size | ❌ |
| PublishAot | 10-30MB | ❌ |

---

## Compression (Recommended)

```xml
<PropertyGroup>
  <PublishSingleFile>true</PublishSingleFile>
  <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
</PropertyGroup>
```

**Trade-off**: Slower startup (decompression time)

---

## ReadyToRun (Faster Startup)

```xml
<PropertyGroup>
  <PublishReadyToRun>true</PublishReadyToRun>
</PropertyGroup>
```

**Trade-off**: Larger file size

---

## Invariant Globalization

```xml
<PropertyGroup>
  <InvariantGlobalization>true</InvariantGlobalization>
</PropertyGroup>
```

⚠️ Disables culture-specific formatting

---

## Realistic WPF Sizes

| Configuration | Approximate Size |
|---------------|------------------|
| Self-Contained | 150-200MB |
| + Compression | 50-80MB |
| + ReadyToRun | 200-250MB |
| + Compression + R2R | 80-120MB |

---

## Why Trimming/AOT Don't Work

**PublishTrimmed:**
- WPF uses heavy reflection
- XAML binding relies on runtime type discovery
- Results in `NETSDK1168` error

**PublishAot:**
- WPF requires JIT compilation
- Not supported by .NET team
