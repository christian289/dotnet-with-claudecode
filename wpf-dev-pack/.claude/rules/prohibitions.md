# Prohibitions

Items explicitly banned from wpf-dev-pack.
Do NOT introduce any of these during code generation, review, or refactoring.

---

## 1. ViewModelLocator

**Prohibited.**

- Looks convenient at first, but erodes MVVM discipline over time during maintenance.
- Convention-based mapping is implicit, making debugging harder and overlapping with the DI container's responsibility.
- Wire View-ViewModel via **DataTemplate mapping** or **direct DI container resolve** instead.

```xml
<!-- Prohibited -->
<Window vm:ViewModelLocator.AutoWireViewModel="True" />

<!-- Correct: DataTemplate mapping -->
<DataTemplate DataType="{x:Type vm:DashboardViewModel}">
    <views:DashboardView />
</DataTemplate>
```
