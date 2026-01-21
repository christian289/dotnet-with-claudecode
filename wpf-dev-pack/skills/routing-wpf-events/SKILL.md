---
name: routing-wpf-events
description: Implements WPF routed events including Bubbling, Tunneling, and Direct strategies. Use when creating custom routed events, handling event propagation, or understanding Preview events.
---

# WPF Routed Events Patterns

Understanding and implementing WPF's routed event system for event propagation through element trees.

## 1. Routing Strategies Overview

```
                    Window (Root)
                       │
    ┌──────────────────┼──────────────────┐
    │                  │                  │
  Grid              Border            StackPanel
    │                  │                  │
 Button             TextBox           ListBox
    │
ContentPresenter
    │
 TextBlock (Event Source)

Tunneling (Preview): Window → Grid → Button → ContentPresenter → TextBlock
Bubbling:            TextBlock → ContentPresenter → Button → Grid → Window
Direct:              Only TextBlock
```

---

## 2. Routing Strategy Types

| Strategy | Direction | Event Name Pattern | Use Case |
|----------|-----------|-------------------|----------|
| **Tunneling** | Root → Source (downward) | PreviewXxx | Input validation, cancellation before processing |
| **Bubbling** | Source → Root (upward) | Xxx | Normal event handling |
| **Direct** | Source only | Xxx | Events that don't propagate (MouseEnter, MouseLeave) |

---

## 3. Tunneling and Bubbling Example

### 3.1 XAML Setup

```xml
<Window PreviewMouseDown="Window_PreviewMouseDown"
        MouseDown="Window_MouseDown">
    <Grid PreviewMouseDown="Grid_PreviewMouseDown"
          MouseDown="Grid_MouseDown">
        <Button PreviewMouseDown="Button_PreviewMouseDown"
                MouseDown="Button_MouseDown"
                Content="Click Me"/>
    </Grid>
</Window>
```

### 3.2 Event Handler Order

```csharp
// Execution order when Button is clicked:
// 버튼 클릭 시 실행 순서:
// 1. Window_PreviewMouseDown  (Tunneling)
// 2. Grid_PreviewMouseDown    (Tunneling)
// 3. Button_PreviewMouseDown  (Tunneling)
// 4. Button_MouseDown         (Bubbling)
// 5. Grid_MouseDown           (Bubbling)
// 6. Window_MouseDown         (Bubbling)

private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
{
    Debug.WriteLine("1. Window PreviewMouseDown (Tunneling)");
}

private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
{
    Debug.WriteLine("2. Grid PreviewMouseDown (Tunneling)");
}

private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
{
    Debug.WriteLine("3. Button PreviewMouseDown (Tunneling)");
}

private void Button_MouseDown(object sender, MouseButtonEventArgs e)
{
    Debug.WriteLine("4. Button MouseDown (Bubbling)");
}

private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
{
    Debug.WriteLine("5. Grid MouseDown (Bubbling)");
}

private void Window_MouseDown(object sender, MouseButtonEventArgs e)
{
    Debug.WriteLine("6. Window MouseDown (Bubbling)");
}
```

---

## 4. Stopping Event Propagation

### 4.1 Using Handled Property

```csharp
private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
{
    // Stop further propagation
    // 이후 전파 중지
    e.Handled = true;

    // Only events 1, 2, 3 will fire
    // 1, 2, 3번 이벤트만 발생
}

private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
{
    // Stop bubbling to parent
    // 부모로 버블링 중지
    e.Handled = true;

    // Window_MouseDown won't fire
    // Window_MouseDown은 발생하지 않음
}
```

### 4.2 Handling Already-Handled Events

```csharp
// Register handler that receives even handled events
// 처리된 이벤트도 받는 핸들러 등록
public MainWindow()
{
    InitializeComponent();

    // handledEventsToo: true - receives events even if Handled = true
    // handledEventsToo: true - Handled = true여도 이벤트 수신
    AddHandler(
        MouseDownEvent,
        new MouseButtonEventHandler(OnMouseDownHandledToo),
        handledEventsToo: true);
}

private void OnMouseDownHandledToo(object sender, MouseButtonEventArgs e)
{
    // This handler is called even if e.Handled = true elsewhere
    // 다른 곳에서 e.Handled = true로 설정해도 호출됨
    Debug.WriteLine($"MouseDown received, Handled: {e.Handled}");
}
```

---

## 5. RoutedEventArgs Properties

```csharp
private void Element_MouseDown(object sender, MouseButtonEventArgs e)
{
    // Source: Element that raised the event (logical tree)
    // Source: 이벤트를 발생시킨 요소 (논리 트리)
    var source = e.Source;

    // OriginalSource: Actual element clicked (visual tree)
    // OriginalSource: 실제 클릭된 요소 (비주얼 트리)
    var originalSource = e.OriginalSource;

    // Example: Click on TextBlock inside Button
    // 예: Button 안의 TextBlock 클릭 시
    // Source = Button (logical source)
    // OriginalSource = TextBlock (visual source)

    // RoutedEvent: The routed event being handled
    // RoutedEvent: 처리 중인 라우티드 이벤트
    var routedEvent = e.RoutedEvent;

    // Handled: Whether the event has been handled
    // Handled: 이벤트 처리 여부
    var handled = e.Handled;
}
```

---

## 6. Creating Custom Routed Events

### 6.1 Bubbling Event

```csharp
namespace MyApp.Controls;

using System.Windows;

public class CustomSlider : Control
{
    // Register routed event
    // 라우티드 이벤트 등록
    public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
        name: "ValueChanged",
        routingStrategy: RoutingStrategy.Bubble,
        handlerType: typeof(RoutedPropertyChangedEventHandler<double>),
        ownerType: typeof(CustomSlider));

    // CLR event wrapper
    // CLR 이벤트 래퍼
    public event RoutedPropertyChangedEventHandler<double> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    // Raise the event
    // 이벤트 발생
    protected virtual void OnValueChanged(double oldValue, double newValue)
    {
        var args = new RoutedPropertyChangedEventArgs<double>(oldValue, newValue, ValueChangedEvent);
        RaiseEvent(args);
    }
}
```

### 6.2 Tunneling Event (with Preview)

```csharp
namespace MyApp.Controls;

using System.Windows;

public class CustomButton : Button
{
    // Tunneling (Preview) event
    // 터널링 (Preview) 이벤트
    public static readonly RoutedEvent PreviewClickedEvent = EventManager.RegisterRoutedEvent(
        name: "PreviewClicked",
        routingStrategy: RoutingStrategy.Tunnel,
        handlerType: typeof(RoutedEventHandler),
        ownerType: typeof(CustomButton));

    // Bubbling event
    // 버블링 이벤트
    public static readonly RoutedEvent ClickedEvent = EventManager.RegisterRoutedEvent(
        name: "Clicked",
        routingStrategy: RoutingStrategy.Bubble,
        handlerType: typeof(RoutedEventHandler),
        ownerType: typeof(CustomButton));

    public event RoutedEventHandler PreviewClicked
    {
        add => AddHandler(PreviewClickedEvent, value);
        remove => RemoveHandler(PreviewClickedEvent, value);
    }

    public event RoutedEventHandler Clicked
    {
        add => AddHandler(ClickedEvent, value);
        remove => RemoveHandler(ClickedEvent, value);
    }

    protected override void OnClick()
    {
        // Raise Preview (Tunneling) first
        // Preview (Tunneling) 먼저 발생
        var previewArgs = new RoutedEventArgs(PreviewClickedEvent, this);
        RaiseEvent(previewArgs);

        // If not handled, raise Bubbling event
        // 처리되지 않았으면 Bubbling 이벤트 발생
        if (!previewArgs.Handled)
        {
            var args = new RoutedEventArgs(ClickedEvent, this);
            RaiseEvent(args);
        }

        base.OnClick();
    }
}
```

### 6.3 Custom EventArgs

```csharp
namespace MyApp.Events;

using System.Windows;

public class ItemSelectedEventArgs : RoutedEventArgs
{
    public object SelectedItem { get; }
    public int SelectedIndex { get; }

    public ItemSelectedEventArgs(RoutedEvent routedEvent, object source, object selectedItem, int selectedIndex)
        : base(routedEvent, source)
    {
        SelectedItem = selectedItem;
        SelectedIndex = selectedIndex;
    }
}

public delegate void ItemSelectedEventHandler(object sender, ItemSelectedEventArgs e);
```

---

## 7. Class Event Handlers

### 7.1 Registering Class Handler

```csharp
namespace MyApp.Controls;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public class EnhancedTextBox : TextBox
{
    static EnhancedTextBox()
    {
        // Class handler - called before instance handlers
        // 클래스 핸들러 - 인스턴스 핸들러보다 먼저 호출
        EventManager.RegisterClassHandler(
            typeof(EnhancedTextBox),
            PreviewKeyDownEvent,
            new KeyEventHandler(OnPreviewKeyDownClass));

        EventManager.RegisterClassHandler(
            typeof(EnhancedTextBox),
            GotFocusEvent,
            new RoutedEventHandler(OnGotFocusClass));
    }

    private static void OnPreviewKeyDownClass(object sender, KeyEventArgs e)
    {
        // Called for all EnhancedTextBox instances
        // 모든 EnhancedTextBox 인스턴스에서 호출
        if (sender is EnhancedTextBox textBox)
        {
            // Common keyboard handling logic
            // 공통 키보드 처리 로직
        }
    }

    private static void OnGotFocusClass(object sender, RoutedEventArgs e)
    {
        // Auto-select all text on focus
        // 포커스 시 전체 텍스트 선택
        if (sender is EnhancedTextBox textBox)
        {
            textBox.SelectAll();
        }
    }
}
```

---

## 8. Attached Events

### 8.1 Using Attached Events

```xml
<!-- Handle Button.Click at Grid level (Bubbling) -->
<!-- Grid에서 Button.Click 처리 (Bubbling) -->
<Grid Button.Click="Grid_ButtonClick">
    <StackPanel>
        <Button Content="Button 1"/>
        <Button Content="Button 2"/>
        <Button Content="Button 3"/>
    </StackPanel>
</Grid>
```

```csharp
private void Grid_ButtonClick(object sender, RoutedEventArgs e)
{
    // Handle clicks from any child button
    // 모든 자식 버튼의 클릭 처리
    if (e.OriginalSource is Button button)
    {
        Debug.WriteLine($"Clicked: {button.Content}");
    }
}
```

### 8.2 Defining Attached Events

```csharp
namespace MyApp.Behaviors;

using System.Windows;

public static class ValidationBehavior
{
    // Attached routed event
    // 첨부 라우티드 이벤트
    public static readonly RoutedEvent ValidationErrorEvent = EventManager.RegisterRoutedEvent(
        name: "ValidationError",
        routingStrategy: RoutingStrategy.Bubble,
        handlerType: typeof(RoutedEventHandler),
        ownerType: typeof(ValidationBehavior));

    public static void AddValidationErrorHandler(DependencyObject d, RoutedEventHandler handler)
    {
        if (d is UIElement element)
        {
            element.AddHandler(ValidationErrorEvent, handler);
        }
    }

    public static void RemoveValidationErrorHandler(DependencyObject d, RoutedEventHandler handler)
    {
        if (d is UIElement element)
        {
            element.RemoveHandler(ValidationErrorEvent, handler);
        }
    }

    // Raise the attached event
    // 첨부 이벤트 발생
    public static void RaiseValidationError(UIElement element)
    {
        element.RaiseEvent(new RoutedEventArgs(ValidationErrorEvent, element));
    }
}
```

---

## 9. Common Event Handling Patterns

### 9.1 Event Aggregation

```csharp
// Handle events from multiple child elements at parent level
// 부모에서 여러 자식 요소의 이벤트 처리
private void ParentPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
{
    // Find the clicked element type
    // 클릭된 요소 타입 확인
    var clickedElement = e.OriginalSource as FrameworkElement;

    switch (clickedElement)
    {
        case Button button:
            HandleButtonClick(button);
            break;
        case TextBlock textBlock:
            HandleTextBlockClick(textBlock);
            break;
        case Image image:
            HandleImageClick(image);
            break;
    }
}
```

### 9.2 Event Suppression

```csharp
// Suppress events for specific conditions
// 특정 조건에서 이벤트 억제
private void Element_PreviewMouseDown(object sender, MouseButtonEventArgs e)
{
    if (IsReadOnly || IsDisabled)
    {
        // Prevent all mouse handling
        // 모든 마우스 처리 방지
        e.Handled = true;
    }
}
```

---

## 10. References

- [Routed Events Overview - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/routed-events-overview)
- [How to: Create a Custom Routed Event - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/how-to-create-a-custom-routed-event)
- [Marking Routed Events as Handled - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/marking-routed-events-as-handled-and-class-handling)
