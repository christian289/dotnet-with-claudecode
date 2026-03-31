---
description: Implements WPF input handling with RoutedCommand, ICommand, CommandBinding, and InputBinding patterns. Use when creating keyboard shortcuts, menu commands, or custom command implementations.
---

# WPF Input and Commands Patterns

> **MVVM Framework Rule**: `.claude/rules/dotnet/wpf/mvvm-framework.md` 설정에 따라 코드 스타일이 결정됩니다.
> Prism 9 사용 시 → [PRISM.md](PRISM.md) 참조

Handling user input and implementing command patterns in WPF applications.

## 1. Command System Overview

```
ICommand (Interface)
├── RoutedCommand (WPF built-in)
│   ├── ApplicationCommands (Copy, Paste, Cut, etc.)
│   ├── NavigationCommands (BrowseBack, BrowseForward, etc.)
│   ├── MediaCommands (Play, Pause, Stop, etc.)
│   └── EditingCommands (ToggleBold, ToggleItalic, etc.)
└── RelayCommand / DelegateCommand (MVVM)
```

---

## 2. Built-in Commands

### 2.1 ApplicationCommands

```xml
<Window.CommandBindings>
    <CommandBinding Command="ApplicationCommands.Copy"
                    Executed="CopyCommand_Executed"
                    CanExecute="CopyCommand_CanExecute"/>
    <CommandBinding Command="ApplicationCommands.Paste"
                    Executed="PasteCommand_Executed"/>
</Window.CommandBindings>

<StackPanel>
    <Button Command="ApplicationCommands.Copy" Content="Copy (Ctrl+C)"/>
    <Button Command="ApplicationCommands.Paste" Content="Paste (Ctrl+V)"/>
    <Button Command="ApplicationCommands.Undo" Content="Undo (Ctrl+Z)"/>
    <Button Command="ApplicationCommands.Redo" Content="Redo (Ctrl+Y)"/>
</StackPanel>
```

```csharp
private void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
{
    // Copy logic
    Clipboard.SetText(SelectedText);
}

private void CopyCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
{
    // Enable condition
    e.CanExecute = !string.IsNullOrEmpty(SelectedText);
}
```

### 2.2 Common Built-in Commands

| Category | Commands |
|----------|----------|
| **ApplicationCommands** | New, Open, Save, SaveAs, Close, Print, Copy, Cut, Paste, Undo, Redo, Find, Replace, SelectAll, Delete, Properties, Help |
| **NavigationCommands** | BrowseBack, BrowseForward, BrowseHome, BrowseStop, Refresh, Favorites, Search, GoToPage, NextPage, PreviousPage, FirstPage, LastPage |
| **MediaCommands** | Play, Pause, Stop, Record, NextTrack, PreviousTrack, FastForward, Rewind, ChannelUp, ChannelDown, TogglePlayPause, IncreaseVolume, DecreaseVolume, MuteVolume |
| **EditingCommands** | ToggleBold, ToggleItalic, ToggleUnderline, IncreaseFontSize, DecreaseFontSize, AlignLeft, AlignCenter, AlignRight, AlignJustify |

---

> **Advanced**: See [ADVANCED.md](ADVANCED.md) for custom RoutedCommand creation, keyboard/mouse event handling (PreviewKeyDown, drag logic), and FocusManager patterns.

---

## 3. InputBinding (Keyboard/Mouse Shortcuts)

### 3.1 KeyBinding

```xml
<Window.InputBindings>
    <!-- Keyboard shortcut -->
    <KeyBinding Key="N" Modifiers="Control" Command="ApplicationCommands.New"/>
    <KeyBinding Key="S" Modifiers="Control" Command="ApplicationCommands.Save"/>
    <KeyBinding Key="S" Modifiers="Control+Shift" Command="ApplicationCommands.SaveAs"/>
    <KeyBinding Key="F4" Modifiers="Alt" Command="ApplicationCommands.Close"/>

    <!-- Function keys -->
    <KeyBinding Key="F1" Command="ApplicationCommands.Help"/>
    <KeyBinding Key="F5" Command="{x:Static cmd:CustomCommands.RefreshData}"/>

    <!-- MVVM Command binding -->
    <KeyBinding Key="Delete" Command="{Binding DeleteCommand}"/>
</Window.InputBindings>
```

### 3.2 MouseBinding

```xml
<Border.InputBindings>
    <!-- Double-click -->
    <MouseBinding MouseAction="LeftDoubleClick"
                  Command="{Binding OpenDetailCommand}"/>

    <!-- Middle click -->
    <MouseBinding MouseAction="MiddleClick"
                  Command="{Binding CloseTabCommand}"/>

    <!-- Ctrl + Left click -->
    <MouseBinding MouseAction="LeftClick"
                  Modifiers="Control"
                  Command="{Binding MultiSelectCommand}"/>
</Border.InputBindings>
```

---

## 4. CommandParameter

### 4.1 Static Parameter

```xml
<Button Command="{Binding NavigateCommand}"
        CommandParameter="Home"
        Content="Go to Home"/>

<Button Command="{Binding SetZoomCommand}"
        CommandParameter="100"
        Content="Zoom 100%"/>
```

### 4.2 Binding Parameter

```xml
<ListBox x:Name="ItemList" ItemsSource="{Binding Items}"/>
<Button Command="{Binding DeleteCommand}"
        CommandParameter="{Binding SelectedItem, ElementName=ItemList}"
        Content="Delete Selected"/>

<!-- Self-reference -->
<Button Command="{Binding ProcessCommand}"
        CommandParameter="{Binding RelativeSource={RelativeSource Self}}"
        Content="Process This Button"/>
```

---

## 5. CommandTarget

### 5.1 Redirecting Command Target

```xml
<StackPanel>
    <TextBox x:Name="TargetTextBox"/>

    <!-- Commands target the TextBox even when button is focused -->
    <Button Command="ApplicationCommands.Copy"
            CommandTarget="{Binding ElementName=TargetTextBox}"
            Content="Copy from TextBox"/>

    <Button Command="ApplicationCommands.Paste"
            CommandTarget="{Binding ElementName=TargetTextBox}"
            Content="Paste to TextBox"/>
</StackPanel>
```

---

## 6. References

- [Commanding Overview - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/commanding-overview)
- [Input Overview - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/input-overview)
- [Focus Overview - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/focus-overview)
