# MVVM Pattern with CollectionView — Advanced Patterns

> Core concepts: See [SKILL.md](SKILL.md)

## Utilizing CollectionView Features in Service Layer

Service Layer can encapsulate various CollectionView features.

```csharp
// Services/MemberCollectionService.cs
namespace MyApp.Services;

public sealed class MemberCollectionService
{
    private ObservableCollection<Member> Source { get; } = [];

    public IEnumerable CreateView(Predicate<object>? filter = null)
    {
        var viewSource = new CollectionViewSource { Source = Source };
        var view = viewSource.View;

        if (filter is not null)
        {
            view.Filter = filter;
        }

        return view;
    }

    // Create sorted view
    public IEnumerable CreateSortedView(
        string propertyName,
        ListSortDirection direction = ListSortDirection.Ascending)
    {
        var viewSource = new CollectionViewSource { Source = Source };
        var view = viewSource.View;

        view.SortDescriptions.Add(
            new SortDescription(propertyName, direction)
        );

        return view;
    }

    // Create grouped view
    public IEnumerable CreateGroupedView(string groupPropertyName)
    {
        var viewSource = new CollectionViewSource { Source = Source };
        var view = viewSource.View;

        view.GroupDescriptions.Add(
            new PropertyGroupDescription(groupPropertyName)
        );

        return view;
    }

    public void Add(Member item) => Source.Add(item);
    public void Remove(Member? item) { if (item is not null) Source.Remove(item); }
    public void Clear() => Source.Clear();
}
```

---

## When Applying DI/IoC

```csharp
// Interface definition (uses pure BCL types only)
namespace MyApp.Services;

public interface IMemberCollectionService
{
    IEnumerable CreateView(Predicate<object>? filter = null);
    void Add(Member member);
    void Remove(Member? member);
    void Clear();
}

// DI container registration
services.AddSingleton<IMemberCollectionService, MemberCollectionService>();

// ViewModel constructor injection
namespace MyApp.ViewModels;

public sealed partial class AppViewModel(IMemberCollectionService memberService)
    : ObservableObject
{
    public IEnumerable? Members { get; } = memberService.CreateView();
}
```

---

## Grouping UI in XAML

When using grouped CollectionView, display groups using `GroupStyle` in ListBox or ItemsControl.

**XAML - Grouped ListBox:**

```xml
<ListBox ItemsSource="{Binding GroupedMembers}">
    <!-- Group header template -->
    <ListBox.GroupStyle>
        <GroupStyle>
            <GroupStyle.HeaderTemplate>
                <DataTemplate>
                    <Border Background="#E0E0E0" Padding="5">
                        <StackPanel Orientation="Horizontal">
                            <TextBlock Text="{Binding Name}"
                                       FontWeight="Bold"
                                       FontSize="14"/>
                            <TextBlock Text=" ("
                                       Foreground="Gray"/>
                            <TextBlock Text="{Binding ItemCount}"
                                       Foreground="Gray"/>
                            <TextBlock Text=" items)"
                                       Foreground="Gray"/>
                        </StackPanel>
                    </Border>
                </DataTemplate>
            </GroupStyle.HeaderTemplate>
            <!-- Optional: Group container style -->
            <GroupStyle.ContainerStyle>
                <Style TargetType="{x:Type GroupItem}">
                    <Setter Property="Margin" Value="0,0,0,10"/>
                </Style>
            </GroupStyle.ContainerStyle>
        </GroupStyle>
    </ListBox.GroupStyle>

    <!-- Item template -->
    <ListBox.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Name}" Padding="10,5"/>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

**XAML - Expander Style Grouping:**

```xml
<ListBox ItemsSource="{Binding GroupedMembers}">
    <ListBox.GroupStyle>
        <GroupStyle>
            <GroupStyle.ContainerStyle>
                <Style TargetType="{x:Type GroupItem}">
                    <Setter Property="Template">
                        <Setter.Value>
                            <ControlTemplate TargetType="{x:Type GroupItem}">
                                <Expander IsExpanded="True">
                                    <Expander.Header>
                                        <StackPanel Orientation="Horizontal">
                                            <TextBlock Text="{Binding Name}"
                                                       FontWeight="Bold"/>
                                            <TextBlock Text="{Binding ItemCount,
                                                       StringFormat=' ({0})'}"
                                                       Foreground="Gray"/>
                                        </StackPanel>
                                    </Expander.Header>
                                    <ItemsPresenter/>
                                </Expander>
                            </ControlTemplate>
                        </Setter.Value>
                    </Setter>
                </Style>
            </GroupStyle.ContainerStyle>
        </GroupStyle>
    </ListBox.GroupStyle>
</ListBox>
```

**Service Layer - Multiple Sort and Group:**

```csharp
// Create view with sorting and grouping combined
public IEnumerable CreateSortedGroupedView(
    string sortProperty,
    ListSortDirection sortDirection,
    string groupProperty)
{
    var viewSource = new CollectionViewSource { Source = Source };
    var view = viewSource.View;

    // Apply sort first, then group
    view.SortDescriptions.Add(new SortDescription(sortProperty, sortDirection));
    view.GroupDescriptions.Add(new PropertyGroupDescription(groupProperty));

    return view;
}
```
