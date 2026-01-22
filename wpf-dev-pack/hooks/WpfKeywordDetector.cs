#!/usr/bin/env dotnet

// WPF Keyword Detector Hook
// Detects WPF-related keywords in user prompts and suggests relevant skills.
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

var detectedSkills = DetectKeywords(prompt);

if (detectedSkills.Count > 0)
    Console.WriteLine($"[WPF Dev Pack] Detected relevant skills: {string.Join(", ", detectedSkills)}");

static HashSet<string> DetectKeywords(string prompt)
{
    var lowerPrompt = prompt.ToLowerInvariant();
    var detectedSkills = new HashSet<string>();

    Dictionary<string, string[]> keywordSkillMap = new()
    {
        // CustomControl keywords
        ["customcontrol"] = ["authoring-wpf-controls", "developing-wpf-customcontrols"],
        ["custom control"] = ["authoring-wpf-controls", "developing-wpf-customcontrols"],
        ["dependencyproperty"] = ["defining-wpf-dependencyproperty"],
        ["dependency property"] = ["defining-wpf-dependencyproperty"],
        ["templatepart"] = ["developing-wpf-customcontrols"],
        ["onapplytemplate"] = ["developing-wpf-customcontrols"],

        // XAML/Style keywords
        ["controltemplate"] = ["customizing-controltemplate"],
        ["control template"] = ["customizing-controltemplate"],
        ["resourcedictionary"] = ["managing-styles-resourcedictionary"],
        ["resource dictionary"] = ["managing-styles-resourcedictionary"],
        ["generic.xaml"] = ["designing-wpf-customcontrol-architecture"],
        ["storyboard"] = ["creating-wpf-animations"],
        ["animation"] = ["creating-wpf-animations"],

        // MVVM keywords
        ["mvvm"] = ["implementing-communitytoolkit-mvvm"],
        ["viewmodel"] = ["implementing-communitytoolkit-mvvm"],
        ["observableproperty"] = ["implementing-communitytoolkit-mvvm"],
        ["relaycommand"] = ["implementing-communitytoolkit-mvvm"],
        ["collectionview"] = ["managing-wpf-collectionview-mvvm"],
        ["datatemplate"] = ["mapping-viewmodel-view-datatemplate"],

        // Rendering keywords
        ["drawingcontext"] = ["rendering-with-drawingcontext"],
        ["drawing context"] = ["rendering-with-drawingcontext"],
        ["drawingvisual"] = ["rendering-with-drawingvisual"],
        ["drawing visual"] = ["rendering-with-drawingvisual"],
        ["onrender"] = ["rendering-with-drawingcontext"],
        ["invalidatevisual"] = ["rendering-with-drawingcontext"],

        // Performance keywords
        ["virtualizingstackpanel"] = ["virtualizing-wpf-ui"],
        ["virtualizing"] = ["virtualizing-wpf-ui"],
        ["freeze"] = ["optimizing-wpf-memory"],
        ["freezable"] = ["optimizing-wpf-memory"],
        ["bitmapcache"] = ["rendering-wpf-high-performance"],
        ["performance"] = ["rendering-wpf-high-performance", "optimizing-wpf-memory"],

        // Other WPF keywords
        ["adorner"] = ["implementing-wpf-adorners"],
        ["dragdrop"] = ["implementing-wpf-dragdrop"],
        ["drag and drop"] = ["implementing-wpf-dragdrop"],
        ["routed event"] = ["routing-wpf-events"],
        ["routedevent"] = ["routing-wpf-events"],
        ["command binding"] = ["handling-wpf-input-commands"],
        ["inputbinding"] = ["handling-wpf-input-commands"],
        ["flowdocument"] = ["creating-wpf-flowdocument"],
        ["dialog"] = ["creating-wpf-dialogs"],
        ["messagebox"] = ["creating-wpf-dialogs"],
        ["clipboard"] = ["using-wpf-clipboard"],
        ["localization"] = ["localizing-wpf-applications"],
        ["automation"] = ["implementing-wpf-automation"],
        ["mediaelement"] = ["integrating-wpf-media"],
        ["visualtree"] = ["navigating-visual-logical-tree"],
        ["logicaltree"] = ["navigating-visual-logical-tree"],
        ["dispatcher"] = ["threading-wpf-dispatcher"]
    };

    foreach (var (keyword, skills) in keywordSkillMap)
    {
        if (lowerPrompt.Contains(keyword))
        {
            foreach (var skill in skills)
                detectedSkills.Add(skill);
        }
    }

    return detectedSkills;
}
