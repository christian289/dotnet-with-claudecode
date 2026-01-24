# WPF Dev Pack - Intelligent Skill Auto-Trigger System

You are enhanced with specialized skills for WPF, C#, and .NET development. **Automatically activate relevant skills when keywords are detected, even without explicit user request.**

---

## PART 1: AUTO-TRIGGER PROTOCOL (CRITICAL)

### Core Rules

```
RULE 1: Detect WPF/C#/.NET keywords → Immediately activate relevant skills
RULE 2: Delegate complex tasks to specialized agents
RULE 3: Announce skill activation (except silent triggers)
RULE 4: Select most specific skill when multiple match
```

### Trigger Priority

1. **Explicit slash command** (`/wpf-dev-pack:skill-name`) → Highest
2. **Keyword-based auto-trigger** → See mapping tables below
3. **Context-based inference** → Determine from conversation context

---

## PART 2: KEYWORD-SKILL MAPPING

### Primary Keywords (Immediate Trigger)

| Keyword | Trigger Skill | Condition |
|---------|---------------|-----------|
| `customcontrol`, `custom control` | `authoring-wpf-controls`, `developing-wpf-customcontrols` | Control creation |
| `dependencyproperty`, `dependency property` | `defining-wpf-dependencyproperty` | DP definition |
| `mvvm`, `viewmodel`, `observableproperty`, `relaycommand` | `implementing-communitytoolkit-mvvm` | MVVM pattern |
| `drawingcontext`, `onrender`, `invalidatevisual` | `rendering-with-drawingcontext` | High-perf rendering |
| `drawingvisual` | `rendering-with-drawingvisual` | Lightweight visual |
| `controltemplate`, `control template` | `customizing-controltemplate` | Template customization |
| `resourcedictionary`, `generic.xaml` | `managing-styles-resourcedictionary`, `designing-wpf-customcontrol-architecture` | Resource/style |
| `animation`, `storyboard` | `creating-wpf-animations` | Animation |
| `collectionview`, `collectionviewsource` | `managing-wpf-collectionview-mvvm` | Collection view |
| `datatemplate` | `mapping-viewmodel-view-datatemplate` | View-ViewModel mapping |
| `adorner` | `implementing-wpf-adorners` | Decoration layer |
| `dragdrop`, `drag and drop` | `implementing-wpf-dragdrop` | Drag & drop |
| `routed event`, `routedevent` | `routing-wpf-events` | Event routing |
| `command`, `inputbinding`, `commandbinding` | `handling-wpf-input-commands` | Command binding |
| `dispatcher` | `threading-wpf-dispatcher` | Threading |
| `virtualizing`, `virtualizingstackpanel` | `virtualizing-wpf-ui` | UI virtualization |
| `freeze`, `freezable` | `optimizing-wpf-memory` | Memory optimization |
| `performance`, `high performance` | `rendering-wpf-high-performance` | Performance |
| `visualtree`, `logicaltree` | `navigating-visual-logical-tree` | Tree navigation |
| `flowdocument` | `creating-wpf-flowdocument` | Document |
| `dialog`, `messagebox` | `creating-wpf-dialogs` | Dialog |
| `clipboard` | `using-wpf-clipboard` | Clipboard |
| `localization`, `localize` | `localizing-wpf-applications` | Localization |
| `automation`, `automationpeer` | `implementing-wpf-automation` | UI Automation |
| `mediaelement`, `media` | `integrating-wpf-media` | Media |
| `behavior`, `interaction` | `using-wpf-behaviors-triggers` | Behaviors |
| `converter`, `ivalueconverter` | `using-converter-markup-extension` | Converter |
| `validation`, `validationrule` | `implementing-wpf-validation` | Validation |
| `binding`, `multibinding` | `advanced-data-binding` | Advanced binding |
| `popup`, `focus` | `managing-wpf-popup-focus` | Popup focus |

### .NET/C# Keywords

| Keyword | Trigger Skill | Condition |
|---------|---------------|-----------|
| `async`, `await`, `task` | `handling-async-operations` | Async programming |
| `parallel`, `plinq`, `concurrent` | `processing-parallel-tasks` | Parallel processing |
| `span`, `memory`, `arraypool` | `optimizing-memory-allocation` | Memory optimization |
| `pipeline`, `pipereader`, `pipewriter` | `implementing-io-pipelines` | I/O pipeline |
| `pubsub`, `channel`, `observable` | `implementing-pubsub-pattern` | Pub-Sub pattern |
| `repository`, `service layer` | `implementing-repository-pattern` | Repository pattern |
| `dependency injection`, `di`, `ioc` | `configuring-dependency-injection` | DI setup |
| `hashset`, `frozenset`, `dictionary` | `optimizing-fast-lookup` | Fast lookup |
| `regex`, `generatedregex` | `using-generated-regex` | Regex |
| `const string`, `literal` | `managing-literal-strings` | String management |

### Project Structure Keywords

| Keyword | Trigger Skill | Condition |
|---------|---------------|-----------|
| `project structure`, `solution structure` | `structuring-wpf-projects` | Project structure |
| `migrate`, `migration`, `.net framework` | `migrating-wpf-to-dotnet` | Migration |
| `startup`, `shutdown`, `application lifecycle` | `managing-wpf-application-lifecycle` | App lifecycle |
| `generichost`, `host builder` | `configuring-console-app-di` | Console app DI |

### Graphics Keywords

| Keyword | Trigger Skill | Condition |
|---------|---------------|-----------|
| `pathgeometry`, `geometry`, `path` | `implementing-2d-graphics` | 2D graphics |
| `brush`, `gradientbrush`, `lineargradient` | `creating-wpf-brushes` | Brush |
| `vector icon`, `pathgeometry icon` | `creating-wpf-vector-icons` | Vector icon |
| `hittest`, `hit test` | `implementing-hit-testing` | Hit testing |

---

## PART 3: AGENT AUTO-DELEGATION

### Delegate Complex Tasks to Specialized Agents

| Task Type | Delegate Agent | Trigger Condition |
|-----------|----------------|-------------------|
| Architecture analysis | `wpf-architect` | "architecture", "structure design", "best practice" |
| Code review | `wpf-code-reviewer` | "review", "MVVM violation", "code quality" |
| CustomControl development | `wpf-control-designer` | CustomControl creation |
| XAML styles/themes | `wpf-xaml-designer` | ControlTemplate, Style work |
| MVVM implementation | `wpf-mvvm-expert` | ViewModel, Command patterns |
| Data binding | `wpf-data-binding-expert` | Complex bindings, Converters |
| Performance optimization | `wpf-performance-optimizer` | Performance issues, rendering |

### Claude Pro Alternative Agents

| Default Agent | Alternative (Claude Pro) |
|---------------|--------------------------|
| `wpf-architect` (Opus) | `wpf-architect-low` (Sonnet) |
| `wpf-code-reviewer` (Opus) | `wpf-code-reviewer-low` (Sonnet) |

---

## PART 4: TRIGGER BEHAVIOR

### On Trigger

1. **Announce** (non-silent):
   > "WPF Dev Pack: Activating `implementing-communitytoolkit-mvvm` skill."

2. **Load Skill**: Reference SKILL.md content

3. **Execute**: Generate/modify code per skill guidelines

### Silent Triggers

These skills activate without announcement:

- `formatting-wpf-csharp-code` - Code formatting
- `using-xaml-property-element-syntax` - XAML property syntax
- `managing-literal-strings` - String management

### Multiple Keywords

When multiple keywords detected:

1. **Most specific first** (e.g., "drawingcontext" > "performance")
2. **Related skills can be referenced in parallel** (e.g., MVVM + DataBinding)
3. **Ask user if conflict**

---

## PART 5: COMMAND REFERENCE

### Slash Commands (User Direct Call)

| Command | Description |
|---------|-------------|
| `/wpf-dev-pack:make-wpf-project` | WPF project scaffolding |
| `/wpf-dev-pack:make-wpf-custom-control` | CustomControl generation |
| `/wpf-dev-pack:make-wpf-usercontrol` | UserControl generation |
| `/wpf-dev-pack:make-wpf-converter` | IValueConverter generation |
| `/wpf-dev-pack:make-wpf-behavior` | Behavior<T> generation |

### Direct Skill Invocation

```
/wpf-dev-pack:implementing-communitytoolkit-mvvm
/wpf-dev-pack:rendering-with-drawingcontext
/wpf-dev-pack:designing-wpf-customcontrol-architecture
```

---

## PART 6: SKILL CATEGORY INDEX

### UI & Controls (16)
`authoring-wpf-controls`, `developing-wpf-customcontrols`, `customizing-controltemplate`, `designing-wpf-customcontrol-architecture`, `defining-wpf-dependencyproperty`, `understanding-wpf-content-model`, `implementing-wpf-adorners`, `creating-wpf-dialogs`, `creating-wpf-flowdocument`, `using-wpf-behaviors-triggers`, `using-converter-markup-extension`, `using-xaml-property-element-syntax`, `binding-enum-command-parameters`, `displaying-slider-index`, `localizing-wpf-applications`, `implementing-wpf-automation`

### Data Binding & MVVM (8)
`implementing-communitytoolkit-mvvm`, `advanced-data-binding`, `implementing-wpf-validation`, `managing-wpf-collectionview-mvvm`, `mapping-viewmodel-view-datatemplate`, `configuring-dependency-injection`, `structuring-wpf-projects`

### Performance & Rendering (11)
`rendering-with-drawingcontext`, `rendering-with-drawingvisual`, `rendering-wpf-architecture`, `rendering-wpf-high-performance`, `implementing-2d-graphics`, `implementing-hit-testing`, `virtualizing-wpf-ui`, `optimizing-wpf-memory`, `checking-image-bounds-transform`, `navigating-visual-logical-tree`, `creating-graphics-in-code`

### Animation & Media (3)
`creating-wpf-animations`, `integrating-wpf-media`, `using-wpf-clipboard`

### Input & Events (4)
`handling-wpf-input-commands`, `routing-wpf-events`, `implementing-wpf-dragdrop`, `managing-wpf-popup-focus`

### Styling & Resources (5)
`managing-styles-resourcedictionary`, `resolving-icon-font-inheritance`, `formatting-wpf-csharp-code`, `creating-wpf-brushes`, `creating-wpf-vector-icons`

### Application & Threading (6)
`managing-wpf-application-lifecycle`, `threading-wpf-dispatcher`, `migrating-wpf-to-dotnet`, `localizing-wpf-with-baml`, `implementing-wpf-rtl-support`, `formatting-culture-aware-data`

### .NET Common (11)
`configuring-console-app-di`, `handling-async-operations`, `implementing-io-pipelines`, `implementing-pubsub-pattern`, `implementing-repository-pattern`, `managing-literal-strings`, `optimizing-fast-lookup`, `optimizing-io-operations`, `optimizing-memory-allocation`, `processing-parallel-tasks`, `using-generated-regex`

---

## PART 7: CHECKLIST

### Auto-Trigger Verification

- [ ] Keyword exists in mapping table?
- [ ] Correct skill selected?
- [ ] Multiple keyword handling needed?
- [ ] Agent delegation more appropriate?
- [ ] Silent trigger target?

### Post-Activation

- [ ] Reference skill content for code generation
- [ ] Follow WPF best practices
- [ ] No MVVM violations (ViewModel-related)
- [ ] Apply performance considerations
