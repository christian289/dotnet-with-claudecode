[🇰🇷 한국어](./README.ko.md)

# Skills

Specialized skills for WPF and .NET development.

> **📦 Archived**: Doc-mirror skills have been moved to `../../archive-skills/`. Topics covered by the `microsoft-docs` MCP plugin (e.g., DependencyProperty, ControlTemplate, Storyboard, DragDrop, async/await, Span<T>, GeneratedRegex) are no longer active skills.

## Skills by Category

### 🎨 UI & Controls

| Skill | Description |
|-------|-------------|
| `authoring-wpf-controls` | Control authoring decision guide (UserControl vs Control vs FrameworkElement) |
| `designing-wpf-customcontrol-architecture` | CustomControl architecture |
| `configuring-wpf-themeinfo` | ThemeInfo configuration |
| `binding-enum-command-parameters` | Enum binding patterns |
| `displaying-slider-index` | Slider UI patterns |

### 🔗 Data Binding & MVVM

| Skill | Description |
|-------|-------------|
| `implementing-communitytoolkit-mvvm` | CommunityToolkit.Mvvm |
| `implementing-wpf-validation` | Validation strategies |
| `managing-wpf-collectionview-mvvm` | CollectionView in MVVM |
| `mapping-viewmodel-view-datatemplate` | View-ViewModel mapping |
| `configuring-dependency-injection` | DI configuration |
| `structuring-wpf-projects` | Project structure |

### ⚡ Performance & Rendering

| Skill | Description |
|-------|-------------|
| `rendering-with-drawingcontext` | DrawingContext rendering |
| `rendering-with-drawingvisual` | DrawingVisual rendering |
| `rendering-wpf-architecture` | Rendering architecture |
| `rendering-wpf-high-performance` | High-performance rendering |
| `implementing-hit-testing` | Hit testing |
| `virtualizing-wpf-ui` | UI virtualization |
| `optimizing-wpf-memory` | Memory optimization (leak patterns) |
| `checking-image-bounds-transform` | Image transforms |
| `navigating-visual-logical-tree` | Tree navigation helpers |

### ⌨️ Input & Events

| Skill | Description |
|-------|-------------|
| `routing-wpf-events` | Routed events (custom event creation) |
| `managing-wpf-popup-focus` | Popup focus management |

### 🎨 Styling & Resources

| Skill | Description |
|-------|-------------|
| `managing-styles-resourcedictionary` | Styles & resources |
| `resolving-icon-font-inheritance` | Icon fonts |
| `formatting-wpf-csharp-code` | Code formatting |

### 🔧 Application & Threading

| Skill | Description |
|-------|-------------|
| `managing-wpf-application-lifecycle` | App lifecycle (Single Instance, IPC) |
| `threading-wpf-dispatcher` | Dispatcher & threading |

### 🔷 .NET Common

| Skill | Description |
|-------|-------------|
| `configuring-console-app-di` | Console app DI |
| `implementing-repository-pattern` | Repository pattern |
| `managing-literal-strings` | String management |

### 🧪 Testing

| Skill | Description |
|-------|-------------|
| `testing-wpf-viewmodels` | ViewModel xUnit testing |
| `managing-unit-tests` | Unit test strategy |

### 🛠️ Scaffolding

| Skill | Description |
|-------|-------------|
| `make-wpf-project` | Scaffold WPF project |
| `make-wpf-custom-control` | Scaffold CustomControl |
| `make-wpf-usercontrol` | Scaffold UserControl |
| `make-wpf-converter` | Scaffold IValueConverter |
| `make-wpf-behavior` | Scaffold Behavior |
| `make-wpf-viewmodel` | Scaffold ViewModel + View + DataTemplate |
| `make-wpf-service` | Scaffold service class |

### 🔌 3rd Party Libraries

| Skill | Description |
|-------|-------------|
| `integrating-wpfui-fluent` | WPF-UI Fluent Design |
| `integrating-livecharts2` | LiveCharts2 data visualization |
| `integrating-nodify` | Nodify node editor |
| `validating-with-fluentvalidation` | FluentValidation integration |
| `handling-errors-with-erroror` | ErrorOr result pattern |
| `flaui-cross-process-input` | FlaUI cross-process input fixes |
| `flaui-prism-dialog-discovery` | FlaUI + Prism dialog discovery |
| `flaui-wpf-element-discovery` | FlaUI WPF element discovery |
| `scottplot-syncing-modifier-keys-for-mousewheel` | ScottPlot modifier key sync |

### 📦 Build & Deployment

| Skill | Description |
|-------|-------------|
| `embedding-pdb-in-exe` | PDB embedding |
| `publishing-wpf-apps` | Publish & installer guidance |

## Skill Structure

Each skill directory contains:
- `SKILL.md` - Main skill documentation
- `PRISM.md` - Prism 9 companion (where applicable)
- `QUICKREF.md` - Quick reference (optional)
- Additional resources (scripts, templates)

## Usage

Skills are auto-triggered by keywords or can be invoked directly:

```
/wpf-dev-pack:implementing-communitytoolkit-mvvm
```
