# Dialogs and Popups

## MessageBox

MewUI provides a native message box API:

```csharp
// Simple message
MessageBox.Show("Operation completed", "Success", MessageBoxButtons.Ok, MessageBoxIcon.Information);

// Confirmation dialog
var result = MessageBox.Show(
    "Are you sure you want to delete this item?",
    "Confirm Delete",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question);

if (result == MessageBoxResult.Yes)
{
    // Delete item
}

// With parent window handle
MessageBox.Show(window.Handle, "Error occurred", "Error", MessageBoxButtons.Ok, MessageBoxIcon.Error);
```

### MessageBox Options

| Buttons | Values |
|---------|--------|
| `MessageBoxButtons.Ok` | OK button only |
| `MessageBoxButtons.OkCancel` | OK and Cancel |
| `MessageBoxButtons.YesNo` | Yes and No |
| `MessageBoxButtons.YesNoCancel` | Yes, No, and Cancel |

| Icons | Description |
|-------|-------------|
| `MessageBoxIcon.None` | No icon |
| `MessageBoxIcon.Information` | Info icon |
| `MessageBoxIcon.Warning` | Warning icon |
| `MessageBoxIcon.Error` | Error icon |
| `MessageBoxIcon.Question` | Question icon |

| Results | Value |
|---------|-------|
| `MessageBoxResult.Ok` | OK clicked |
| `MessageBoxResult.Cancel` | Cancel clicked |
| `MessageBoxResult.Yes` | Yes clicked |
| `MessageBoxResult.No` | No clicked |

## File Dialogs

```csharp
// Open file
var path = FileDialog.OpenFile(new OpenFileDialogOptions
{
    Title = "Select a file",
    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
});
if (path != null) { /* use path */ }

// Open multiple files
var paths = FileDialog.OpenFiles(new OpenFileDialogOptions
{
    Title = "Select files"
});

// Save file
var savePath = FileDialog.SaveFile(new SaveFileDialogOptions
{
    Title = "Save as",
    Filter = "Text files (*.txt)|*.txt",
    DefaultExtension = ".txt"
});

// Select folder
var folder = FileDialog.SelectFolder(new FolderDialogOptions
{
    Title = "Select folder"
});
```

## Context Menu

MewUI has built-in `ContextMenu` support on controls:

```csharp
// Fluent API
new Button()
    .Content("Right-click me")
    .ContextMenu(new ContextMenu()
        .Item("Cut", () => Cut())
        .Item("Copy", () => Copy())
        .Item("Paste", () => Paste())
        .Separator()
        .SubMenu("More", new ContextMenu()
            .Item("Option 1", () => { })
            .Item("Option 2", () => { })
        )
    )

// Or assign property
var button = new Button().Content("Click me");
button.ContextMenu = new ContextMenu()
    .Item("Action", () => DoAction());
```

Context menus auto-show on right-click via built-in handling in `Control`:

```csharp
// Built-in behavior in Control.OnMouseDown:
if (e.Button == MouseButton.Right && ContextMenu != null)
{
    ContextMenu.ShowAt(this, e.Position);
    e.Handled = true;
}
```

### Manual Context Menu

```csharp
// Show context menu manually
protected override void OnMouseDown(MouseEventArgs e)  // Note: MouseEventArgs
{
    if (e.Button == MouseButton.Right)
    {
        var menu = new ContextMenu()
            .Item("Option 1", () => { })
            .Item("Option 2", () => { });
        menu.ShowAt(this, e.Position);
        e.Handled = true;
    }
}
```

## Custom Window Dialog

Note: MewUI does NOT have `ShowDialog()`. Create a custom dialog window:

```csharp
public class ConfirmDialog : Window
{
    public bool Confirmed { get; private set; }

    public ConfirmDialog(string message)
    {
        this.Title("Confirm")
            .Width(300).Height(150)
            .Content(
                new StackPanel().Margin(16).Spacing(16).Children(
                    new Label().Text(message),
                    new StackPanel()
                        .Horizontal()
                        .HorizontalAlignment(HorizontalAlignment.Right)
                        .Spacing(8)
                        .Children(
                            new Button().Content("Cancel").OnClick(() => Close()),
                            new Button().Content("OK").OnClick(() => { Confirmed = true; Close(); })
                        )
                )
            );
    }
}

// Usage (non-modal - window opens separately)
var dialog = new ConfirmDialog("Are you sure?");
dialog.Closed += () => {
    if (dialog.Confirmed)
    {
        // Handle confirmation
    }
};
dialog.Show();
```

## Popup System (Internal)

MewUI's popup system is internal and used by controls like `ComboBox` and `ContextMenu`. It's managed through `Window.ShowPopup()`, `Window.ClosePopup()`, and `Window.UpdatePopup()` methods.

For custom popup behavior, use `ContextMenu` or create a separate window.
