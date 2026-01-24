[🇰🇷 한국어](./README.ko.md)

# Commands

User-invokable slash commands for WPF development.

## Command List

| Command | Description |
|---------|-------------|
| `/wpf-dev-pack:make-wpf-project` | Scaffold a new WPF project with MVVM structure |
| `/wpf-dev-pack:make-wpf-custom-control` | Generate a CustomControl with ControlTemplate |
| `/wpf-dev-pack:make-wpf-usercontrol` | Generate a UserControl with XAML and code-behind |
| `/wpf-dev-pack:make-wpf-converter` | Generate an IValueConverter or IMultiValueConverter |
| `/wpf-dev-pack:make-wpf-behavior` | Generate a Behavior<T> using Microsoft.Xaml.Behaviors |

## Usage Examples

### Create a New Project

```bash
# With CommunityToolkit.Mvvm (Recommended)
/wpf-dev-pack:make-wpf-project MyApp

# With Prism Framework
/wpf-dev-pack:make-wpf-project MyApp --prism
```

### Generate Components

```bash
# CustomControl inheriting from Button
/wpf-dev-pack:make-wpf-custom-control MyButton Button

# UserControl
/wpf-dev-pack:make-wpf-usercontrol SearchBox

# Bool to Visibility Converter
/wpf-dev-pack:make-wpf-converter BoolToVisibility

# TextBox Behavior
/wpf-dev-pack:make-wpf-behavior SelectAllOnFocus TextBox
```

## Directory Structure

Each command contains:
- `COMMAND.md` - Command definition and instructions
- Additional templates or scripts (if needed)
