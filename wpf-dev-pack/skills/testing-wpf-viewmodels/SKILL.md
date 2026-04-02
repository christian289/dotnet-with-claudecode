---
description: "Implements xUnit unit tests for WPF ViewModels with CommunityToolkit.Mvvm. Covers PropertyChanged verification, RelayCommand testing, CanExecute logic, and service mocking with NSubstitute. Use when writing ViewModel tests or setting up a test project for WPF MVVM."
user-invocable: false
model: sonnet
---

# WPF ViewModel Unit Testing

Unit test patterns for WPF ViewModels using xUnit and CommunityToolkit.Mvvm.

## NuGet Packages

```xml
<ItemGroup>
  <PackageReference Include="xunit" Version="2.*" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
  <PackageReference Include="NSubstitute" Version="5.*" />
  <PackageReference Include="FluentAssertions" Version="7.*" />
</ItemGroup>
```

---

## 1. PropertyChanged Verification

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAssertions;

public sealed class UserViewModelTests
{
    [Fact]
    public void Setting_UserName_Raises_PropertyChanged()
    {
        // Arrange
        var vm = new UserViewModel();
        var changedProperties = new List<string>();
        vm.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        // Act
        vm.UserName = "Alice";

        // Assert
        changedProperties.Should().Contain(nameof(UserViewModel.UserName));
    }

    [Fact]
    public void Setting_Same_Value_Does_Not_Raise_PropertyChanged()
    {
        var vm = new UserViewModel { UserName = "Alice" };
        var raised = false;
        vm.PropertyChanged += (_, _) => raised = true;

        vm.UserName = "Alice";

        raised.Should().BeFalse();
    }
}
```

## 2. RelayCommand Testing

```csharp
public sealed class OrderViewModelTests
{
    [Fact]
    public void SaveCommand_Executes_When_CanSave_Is_True()
    {
        var mockService = Substitute.For<IOrderService>();
        var vm = new OrderViewModel(mockService) { OrderName = "Test" };

        vm.SaveCommand.CanExecute(null).Should().BeTrue();
        vm.SaveCommand.Execute(null);

        mockService.Received(1).Save(Arg.Any<Order>());
    }

    [Fact]
    public void SaveCommand_Cannot_Execute_When_OrderName_Is_Empty()
    {
        var mockService = Substitute.For<IOrderService>();
        var vm = new OrderViewModel(mockService) { OrderName = "" };

        vm.SaveCommand.CanExecute(null).Should().BeFalse();
    }
}
```

## 3. AsyncRelayCommand Testing

```csharp
public sealed class DataViewModelTests
{
    [Fact]
    public async Task LoadCommand_Populates_Items()
    {
        var mockService = Substitute.For<IDataService>();
        mockService.GetAllAsync().Returns(["Item1", "Item2"]);

        var vm = new DataViewModel(mockService);

        await vm.LoadCommand.ExecuteAsync(null);

        vm.Items.Should().HaveCount(2);
        vm.Items.Should().Contain("Item1");
    }

    [Fact]
    public async Task LoadCommand_Sets_IsLoading_During_Execution()
    {
        var tcs = new TaskCompletionSource<List<string>>();
        var mockService = Substitute.For<IDataService>();
        mockService.GetAllAsync().Returns(tcs.Task);

        var vm = new DataViewModel(mockService);
        var loadTask = vm.LoadCommand.ExecuteAsync(null);

        vm.LoadCommand.IsRunning.Should().BeTrue();

        tcs.SetResult(["Item1"]);
        await loadTask;

        vm.LoadCommand.IsRunning.Should().BeFalse();
    }
}
```

## 4. NotifyCanExecuteChangedFor Testing

```csharp
public sealed class FormViewModelTests
{
    [Fact]
    public void Changing_Email_Raises_SubmitCommand_CanExecuteChanged()
    {
        var vm = new FormViewModel();
        var canExecuteChanged = false;
        vm.SubmitCommand.CanExecuteChanged += (_, _) => canExecuteChanged = true;

        vm.Email = "test@example.com";

        canExecuteChanged.Should().BeTrue();
    }
}
```

## 5. Service Mocking with NSubstitute

```csharp
public sealed class DashboardViewModelTests
{
    private readonly INavigationService _navService;
    private readonly IDataService _dataService;
    private readonly DashboardViewModel _vm;

    public DashboardViewModelTests()
    {
        _navService = Substitute.For<INavigationService>();
        _dataService = Substitute.For<IDataService>();
        _vm = new DashboardViewModel(_navService, _dataService);
    }

    [Fact]
    public void NavigateCommand_Calls_NavigationService()
    {
        _vm.NavigateCommand.Execute("Settings");

        _navService.Received(1).NavigateTo("Settings");
    }

    [Fact]
    public void Constructor_Does_Not_Call_Services()
    {
        // ViewModel 생성자에서 서비스 호출 금지 확인
        // Verify constructor doesn't call services
        _dataService.DidNotReceive().GetAllAsync();
        _navService.DidNotReceive().NavigateTo(Arg.Any<string>());
    }
}
```

## 6. Test Project Structure

```
{SolutionName}.Tests/
├── {SolutionName}.Tests.csproj
├── GlobalUsings.cs
├── ViewModels/
│   ├── DashboardViewModelTests.cs
│   ├── SettingsViewModelTests.cs
│   └── FormViewModelTests.cs
└── Fixtures/
    └── ServiceFixtures.cs          (shared mock setups)
```

### GlobalUsings.cs

```csharp
global using Xunit;
global using NSubstitute;
global using FluentAssertions;
```

---

## Checklist

- [ ] Each ViewModel has a corresponding test class
- [ ] PropertyChanged events verified for key properties
- [ ] All Commands tested (Execute + CanExecute)
- [ ] Async commands tested with TaskCompletionSource for timing control
- [ ] Services mocked — no real I/O in unit tests
- [ ] Constructor side-effects verified (should be minimal)

> **Prism 9 사용자**: See [PRISM.md](PRISM.md) for Prism-specific testing patterns.
