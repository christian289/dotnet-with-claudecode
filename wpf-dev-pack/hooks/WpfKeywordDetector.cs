#!/usr/bin/env dotnet

// WPF Keyword Detector Hook
// Detects WPF/C#/.NET keywords in user prompts and suggests relevant skills.
// Modeled after oh-my-claudecode's auto-trigger system.
// Input: stdin JSON with "prompt" field

using System.Text.Json;

// Read JSON from stdin
var input = Console.In.ReadToEnd();
if (string.IsNullOrWhiteSpace(input))
    return;

string? prompt = null;
try
{
    using var doc = JsonDocument.Parse(input);
    if (doc.RootElement.TryGetProperty("prompt", out var p))
        prompt = p.GetString();
}
catch
{
    return;
}

if (string.IsNullOrWhiteSpace(prompt))
    return;

var (skills, agents) = DetectKeywordsAndAgents(prompt);

if (skills.Count > 0 || agents.Count > 0)
{
    var messages = new List<string>();

    if (skills.Count > 0)
    {
        var skillList = string.Join(", ", skills.Take(5)); // Show max 5
        var moreCount = skills.Count - 5;
        var suffix = moreCount > 0 ? $" (+{moreCount} more)" : "";
        messages.Add($"Skills: {skillList}{suffix}");
    }

    if (agents.Count > 0)
    {
        messages.Add($"Recommended agents: {string.Join(", ", agents)}");
    }

    Console.WriteLine($"[WPF Dev Pack] {string.Join(" | ", messages)}");
}

static (HashSet<string> skills, HashSet<string> agents) DetectKeywordsAndAgents(string prompt)
{
    var lowerPrompt = prompt.ToLowerInvariant();
    var detectedSkills = new HashSet<string>();
    var detectedAgents = new HashSet<string>();

    // ============================================================
    // SKILL KEYWORDS MAPPING
    // ============================================================
    Dictionary<string, string[]> keywordSkillMap = new()
    {
        // ─────────────────────────────────────────────────────────
        // UI & Controls
        // ─────────────────────────────────────────────────────────
        ["customcontrol"] = ["authoring-wpf-controls", "developing-wpf-customcontrols"],
        ["custom control"] = ["authoring-wpf-controls", "developing-wpf-customcontrols"],
        ["dependencyproperty"] = ["defining-wpf-dependencyproperty"],
        ["dependency property"] = ["defining-wpf-dependencyproperty"],
        ["templatepart"] = ["developing-wpf-customcontrols"],
        ["onapplytemplate"] = ["developing-wpf-customcontrols"],
        ["controltemplate"] = ["customizing-controltemplate"],
        ["control template"] = ["customizing-controltemplate"],
        ["contentcontrol"] = ["understanding-wpf-content-model"],
        ["content model"] = ["understanding-wpf-content-model"],
        ["adorner"] = ["implementing-wpf-adorners"],
        ["dialog"] = ["creating-wpf-dialogs"],
        ["messagebox"] = ["creating-wpf-dialogs"],
        ["flowdocument"] = ["creating-wpf-flowdocument"],
        ["behavior"] = ["using-wpf-behaviors-triggers"],
        ["interaction.triggers"] = ["using-wpf-behaviors-triggers"],
        ["eventtrigger"] = ["using-wpf-behaviors-triggers"],
        ["converter"] = ["using-converter-markup-extension"],
        ["ivalueconverter"] = ["using-converter-markup-extension"],
        ["imultivalueconverter"] = ["using-converter-markup-extension"],
        ["markupextension"] = ["using-converter-markup-extension"],
        ["property element"] = ["using-xaml-property-element-syntax"],
        ["localization"] = ["localizing-wpf-applications"],
        ["localize"] = ["localizing-wpf-applications"],
        ["x:uid"] = ["localizing-wpf-applications"],
        ["automation"] = ["implementing-wpf-automation"],
        ["automationpeer"] = ["implementing-wpf-automation"],
        ["uiautomation"] = ["implementing-wpf-automation"],

        // ─────────────────────────────────────────────────────────
        // XAML/Style
        // ─────────────────────────────────────────────────────────
        ["resourcedictionary"] = ["managing-styles-resourcedictionary"],
        ["resource dictionary"] = ["managing-styles-resourcedictionary"],
        ["generic.xaml"] = ["designing-wpf-customcontrol-architecture"],
        ["themes/generic"] = ["designing-wpf-customcontrol-architecture"],
        ["basedonstyle"] = ["managing-styles-resourcedictionary"],
        ["dynamicresource"] = ["managing-styles-resourcedictionary"],
        ["staticresource"] = ["managing-styles-resourcedictionary"],
        ["storyboard"] = ["creating-wpf-animations"],
        ["animation"] = ["creating-wpf-animations"],
        ["doubleanimation"] = ["creating-wpf-animations"],
        ["coloranimation"] = ["creating-wpf-animations"],
        ["easingfunction"] = ["creating-wpf-animations"],
        ["brush"] = ["creating-wpf-brushes"],
        ["lineargradient"] = ["creating-wpf-brushes"],
        ["radialgradient"] = ["creating-wpf-brushes"],
        ["solidcolorbrush"] = ["creating-wpf-brushes"],
        ["vector icon"] = ["creating-wpf-vector-icons"],
        ["pathgeometry icon"] = ["creating-wpf-vector-icons"],
        ["icon font"] = ["resolving-icon-font-inheritance"],
        ["segoe fluent"] = ["resolving-icon-font-inheritance"],

        // ─────────────────────────────────────────────────────────
        // MVVM & Data Binding
        // ─────────────────────────────────────────────────────────
        ["mvvm"] = ["implementing-communitytoolkit-mvvm"],
        ["viewmodel"] = ["implementing-communitytoolkit-mvvm"],
        ["observableproperty"] = ["implementing-communitytoolkit-mvvm"],
        ["relaycommand"] = ["implementing-communitytoolkit-mvvm"],
        ["inotifypropertychanged"] = ["implementing-communitytoolkit-mvvm"],
        ["communitytoolkit"] = ["implementing-communitytoolkit-mvvm"],
        ["collectionview"] = ["managing-wpf-collectionview-mvvm"],
        ["collectionviewsource"] = ["managing-wpf-collectionview-mvvm"],
        ["icollectionview"] = ["managing-wpf-collectionview-mvvm"],
        ["datatemplate"] = ["mapping-viewmodel-view-datatemplate"],
        ["hierarchicaldatatemplate"] = ["mapping-viewmodel-view-datatemplate"],
        ["binding"] = ["advanced-data-binding"],
        ["multibinding"] = ["advanced-data-binding"],
        ["prioritybinding"] = ["advanced-data-binding"],
        ["relativesource"] = ["advanced-data-binding"],
        ["elementname"] = ["advanced-data-binding"],
        ["validation"] = ["implementing-wpf-validation"],
        ["validationrule"] = ["implementing-wpf-validation"],
        ["idataerrorinfo"] = ["implementing-wpf-validation"],
        ["inotifydataerrorinfo"] = ["implementing-wpf-validation"],
        ["dependency injection"] = ["configuring-dependency-injection"],
        ["generichost"] = ["configuring-dependency-injection"],
        ["host builder"] = ["configuring-dependency-injection"],
        ["addsingleton"] = ["configuring-dependency-injection"],
        ["project structure"] = ["structuring-wpf-projects"],
        ["solution structure"] = ["structuring-wpf-projects"],

        // ─────────────────────────────────────────────────────────
        // Rendering & Performance
        // ─────────────────────────────────────────────────────────
        ["drawingcontext"] = ["rendering-with-drawingcontext"],
        ["drawing context"] = ["rendering-with-drawingcontext"],
        ["onrender"] = ["rendering-with-drawingcontext"],
        ["invalidatevisual"] = ["rendering-with-drawingcontext"],
        ["drawingvisual"] = ["rendering-with-drawingvisual"],
        ["drawing visual"] = ["rendering-with-drawingvisual"],
        ["containervisual"] = ["rendering-with-drawingvisual"],
        ["render pipeline"] = ["rendering-wpf-architecture"],
        ["measure arrange"] = ["rendering-wpf-architecture"],
        ["layoutpass"] = ["rendering-wpf-architecture"],
        ["bitmapache"] = ["rendering-wpf-high-performance"],
        ["cachemode"] = ["rendering-wpf-high-performance"],
        ["performance"] = ["rendering-wpf-high-performance"],
        ["pathgeometry"] = ["implementing-2d-graphics"],
        ["geometry"] = ["implementing-2d-graphics"],
        ["streamgeometry"] = ["implementing-2d-graphics"],
        ["hittest"] = ["implementing-hit-testing"],
        ["hit test"] = ["implementing-hit-testing"],
        ["visualtreehelper.hittest"] = ["implementing-hit-testing"],
        ["virtualizingstackpanel"] = ["virtualizing-wpf-ui"],
        ["virtualizing"] = ["virtualizing-wpf-ui"],
        ["virtualizationmode"] = ["virtualizing-wpf-ui"],
        ["freeze"] = ["optimizing-wpf-memory"],
        ["freezable"] = ["optimizing-wpf-memory"],
        ["weak event"] = ["optimizing-wpf-memory"],
        ["visualtree"] = ["navigating-visual-logical-tree"],
        ["logicaltree"] = ["navigating-visual-logical-tree"],
        ["visualtreehelper"] = ["navigating-visual-logical-tree"],
        ["logicaltreehelper"] = ["navigating-visual-logical-tree"],
        ["transform"] = ["checking-image-bounds-transform"],
        ["rendertransform"] = ["checking-image-bounds-transform"],
        ["transformtoancestor"] = ["checking-image-bounds-transform"],

        // ─────────────────────────────────────────────────────────
        // Input & Events
        // ─────────────────────────────────────────────────────────
        ["command"] = ["handling-wpf-input-commands"],
        ["routedcommand"] = ["handling-wpf-input-commands"],
        ["commandbinding"] = ["handling-wpf-input-commands"],
        ["inputbinding"] = ["handling-wpf-input-commands"],
        ["keybinding"] = ["handling-wpf-input-commands"],
        ["routed event"] = ["routing-wpf-events"],
        ["routedevent"] = ["routing-wpf-events"],
        ["bubbling"] = ["routing-wpf-events"],
        ["tunneling"] = ["routing-wpf-events"],
        ["previewmouse"] = ["routing-wpf-events"],
        ["dragdrop"] = ["implementing-wpf-dragdrop"],
        ["drag and drop"] = ["implementing-wpf-dragdrop"],
        ["dodragdrop"] = ["implementing-wpf-dragdrop"],
        ["dataobject"] = ["implementing-wpf-dragdrop"],
        ["popup"] = ["managing-wpf-popup-focus"],
        ["popup focus"] = ["managing-wpf-popup-focus"],
        ["clipboard"] = ["using-wpf-clipboard"],
        ["mediaelement"] = ["integrating-wpf-media"],
        ["media player"] = ["integrating-wpf-media"],

        // ─────────────────────────────────────────────────────────
        // Application & Threading
        // ─────────────────────────────────────────────────────────
        ["dispatcher"] = ["threading-wpf-dispatcher"],
        ["dispatcherpriority"] = ["threading-wpf-dispatcher"],
        ["invoke"] = ["threading-wpf-dispatcher"],
        ["begininvoke"] = ["threading-wpf-dispatcher"],
        ["application lifecycle"] = ["managing-wpf-application-lifecycle"],
        ["app.xaml"] = ["managing-wpf-application-lifecycle"],
        ["startup"] = ["managing-wpf-application-lifecycle"],
        ["sessionending"] = ["managing-wpf-application-lifecycle"],
        ["migrate"] = ["migrating-wpf-to-dotnet"],
        ["migration"] = ["migrating-wpf-to-dotnet"],
        [".net framework"] = ["migrating-wpf-to-dotnet"],
        ["baml"] = ["localizing-wpf-with-baml"],
        ["flowdirection"] = ["implementing-wpf-rtl-support"],
        ["rtl"] = ["implementing-wpf-rtl-support"],
        ["right-to-left"] = ["implementing-wpf-rtl-support"],
        ["culture"] = ["formatting-culture-aware-data"],
        ["cultureinfo"] = ["formatting-culture-aware-data"],

        // ─────────────────────────────────────────────────────────
        // .NET Common
        // ─────────────────────────────────────────────────────────
        ["async"] = ["handling-async-operations"],
        ["await"] = ["handling-async-operations"],
        ["task"] = ["handling-async-operations"],
        ["valuetask"] = ["handling-async-operations"],
        ["configureawait"] = ["handling-async-operations"],
        ["parallel"] = ["processing-parallel-tasks"],
        ["plinq"] = ["processing-parallel-tasks"],
        ["concurrentdictionary"] = ["processing-parallel-tasks"],
        ["span"] = ["optimizing-memory-allocation"],
        ["memory<"] = ["optimizing-memory-allocation"],
        ["arraypool"] = ["optimizing-memory-allocation"],
        ["stackalloc"] = ["optimizing-memory-allocation"],
        ["pipeline"] = ["implementing-io-pipelines"],
        ["pipereader"] = ["implementing-io-pipelines"],
        ["pipewriter"] = ["implementing-io-pipelines"],
        ["pubsub"] = ["implementing-pubsub-pattern"],
        ["channel"] = ["implementing-pubsub-pattern"],
        ["observable"] = ["implementing-pubsub-pattern"],
        ["repository pattern"] = ["implementing-repository-pattern"],
        ["service layer"] = ["implementing-repository-pattern"],
        ["hashset"] = ["optimizing-fast-lookup"],
        ["frozenset"] = ["optimizing-fast-lookup"],
        ["dictionary"] = ["optimizing-fast-lookup"],
        ["regex"] = ["using-generated-regex"],
        ["generatedregex"] = ["using-generated-regex"],
        ["const string"] = ["managing-literal-strings"],
        ["literal string"] = ["managing-literal-strings"],
        ["console app di"] = ["configuring-console-app-di"],
        ["console host"] = ["configuring-console-app-di"]
    };

    // ============================================================
    // AGENT KEYWORDS MAPPING
    // ============================================================
    Dictionary<string, string> agentKeywordMap = new()
    {
        // Architecture analysis
        ["아키텍처"] = "wpf-architect",
        ["architecture"] = "wpf-architect",
        ["best practice"] = "wpf-architect",
        ["구조 설계"] = "wpf-architect",
        ["structure design"] = "wpf-architect",
        ["design pattern"] = "wpf-architect",

        // Code review
        ["리뷰"] = "wpf-code-reviewer",
        ["review"] = "wpf-code-reviewer",
        ["검토"] = "wpf-code-reviewer",
        ["mvvm 위반"] = "wpf-code-reviewer",
        ["mvvm violation"] = "wpf-code-reviewer",
        ["code quality"] = "wpf-code-reviewer",

        // Performance
        ["성능 최적화"] = "wpf-performance-optimizer",
        ["performance optimization"] = "wpf-performance-optimizer",
        ["렌더링 최적화"] = "wpf-performance-optimizer",
        ["rendering optimization"] = "wpf-performance-optimizer",
        ["메모리 누수"] = "wpf-performance-optimizer",
        ["memory leak"] = "wpf-performance-optimizer",

        // XAML/Style
        ["controltemplate 작성"] = "wpf-xaml-designer",
        ["style 작성"] = "wpf-xaml-designer",
        ["테마"] = "wpf-xaml-designer",
        ["theme"] = "wpf-xaml-designer"
    };

    // Detect skills
    foreach (var (keyword, skills) in keywordSkillMap)
    {
        if (lowerPrompt.Contains(keyword))
        {
            foreach (var skill in skills)
                detectedSkills.Add(skill);
        }
    }

    // Detect agents
    foreach (var (keyword, agent) in agentKeywordMap)
    {
        if (lowerPrompt.Contains(keyword))
        {
            detectedAgents.Add(agent);
        }
    }

    return (detectedSkills, detectedAgents);
}
