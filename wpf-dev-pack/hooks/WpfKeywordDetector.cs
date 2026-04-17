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
    Console.WriteLine("========================================");
    Console.WriteLine("[WPF Dev Pack] Hook Triggered");

    if (skills.Count > 0)
    {
        int shown = 0;
        foreach (var skill in skills.Take(5))
        {
            Console.WriteLine($"  -> /wpf-dev-pack:{skill}");
            shown++;
        }
        if (skills.Count > shown)
            Console.WriteLine($"  -> (+{skills.Count - shown} more)");
    }

    if (agents.Count > 0)
    {
        Console.WriteLine($"  Recommended agents: {string.Join(", ", agents)}");
    }

    Console.WriteLine("========================================");
}

static (HashSet<string> skills, HashSet<string> agents) DetectKeywordsAndAgents(string prompt)
{
    var lowerPrompt = prompt.ToLowerInvariant();
    var detectedSkills = new HashSet<string>();
    var detectedAgents = new HashSet<string>();

    // ============================================================
    // SKILL KEYWORDS MAPPING (English + Korean)
    // ============================================================
    Dictionary<string, string[]> keywordSkillMap = new()
    {
        // ─────────────────────────────────────────────────────────
        // UI & Controls
        // ─────────────────────────────────────────────────────────
        ["customcontrol"] = ["authoring-wpf-controls"],
        ["custom control"] = ["authoring-wpf-controls"],
        ["커스텀컨트롤"] = ["authoring-wpf-controls"],
        ["커스텀 컨트롤"] = ["authoring-wpf-controls"],
        ["사용자 정의 컨트롤"] = ["authoring-wpf-controls"],
        ["themeinfo"] = ["configuring-wpf-themeinfo"],
        ["theme info"] = ["configuring-wpf-themeinfo"],
        ["테마 정보"] = ["configuring-wpf-themeinfo"],
        ["assemblyinfo"] = ["configuring-wpf-themeinfo"],
        ["assembly info"] = ["configuring-wpf-themeinfo"],

        // ─────────────────────────────────────────────────────────
        // XAML/Style
        // ─────────────────────────────────────────────────────────
        ["resourcedictionary"] = ["managing-styles-resourcedictionary"],
        ["resource dictionary"] = ["managing-styles-resourcedictionary"],
        ["리소스 사전"] = ["managing-styles-resourcedictionary"],
        ["리소스 딕셔너리"] = ["managing-styles-resourcedictionary"],
        ["generic.xaml"] = ["designing-wpf-customcontrol-architecture", "configuring-wpf-themeinfo"],
        ["themes/generic"] = ["designing-wpf-customcontrol-architecture", "configuring-wpf-themeinfo"],
        ["basedonstyle"] = ["managing-styles-resourcedictionary"],
        ["dynamicresource"] = ["managing-styles-resourcedictionary"],
        ["staticresource"] = ["managing-styles-resourcedictionary"],
        ["icon font"] = ["resolving-icon-font-inheritance"],
        ["아이콘 폰트"] = ["resolving-icon-font-inheritance"],
        ["segoe fluent"] = ["resolving-icon-font-inheritance"],
        ["property element"] = ["using-xaml-property-element-syntax"],
        ["속성 요소 구문"] = ["using-xaml-property-element-syntax"],
        ["converter"] = ["using-converter-markup-extension"],
        ["컨버터"] = ["using-converter-markup-extension"],
        ["변환기"] = ["using-converter-markup-extension"],
        ["ivalueconverter"] = ["using-converter-markup-extension"],
        ["imultivalueconverter"] = ["using-converter-markup-extension"],
        ["markupextension"] = ["using-converter-markup-extension"],
        ["마크업 확장"] = ["using-converter-markup-extension"],

        // ─────────────────────────────────────────────────────────
        // MVVM & Data Binding
        // ─────────────────────────────────────────────────────────
        ["mvvm"] = ["implementing-communitytoolkit-mvvm"],
        ["viewmodel"] = ["implementing-communitytoolkit-mvvm"],
        ["뷰모델"] = ["implementing-communitytoolkit-mvvm"],
        ["observableproperty"] = ["implementing-communitytoolkit-mvvm"],
        ["relaycommand"] = ["implementing-communitytoolkit-mvvm"],
        ["inotifypropertychanged"] = ["implementing-communitytoolkit-mvvm"],
        ["communitytoolkit"] = ["implementing-communitytoolkit-mvvm"],
        ["collectionview"] = ["managing-wpf-collectionview-mvvm"],
        ["collectionviewsource"] = ["managing-wpf-collectionview-mvvm"],
        ["icollectionview"] = ["managing-wpf-collectionview-mvvm"],
        ["컬렉션뷰"] = ["managing-wpf-collectionview-mvvm"],
        ["binding"] = ["advanced-data-binding"],
        ["바인딩"] = ["advanced-data-binding"],
        ["데이터 바인딩"] = ["advanced-data-binding"],
        ["multibinding"] = ["advanced-data-binding"],
        ["멀티 바인딩"] = ["advanced-data-binding"],
        ["prioritybinding"] = ["advanced-data-binding"],
        ["relativesource"] = ["advanced-data-binding"],
        ["elementname"] = ["advanced-data-binding"],
        ["validation"] = ["implementing-wpf-validation"],
        ["유효성 검사"] = ["implementing-wpf-validation"],
        ["검증"] = ["implementing-wpf-validation"],
        ["validationrule"] = ["implementing-wpf-validation"],
        ["idataerrorinfo"] = ["implementing-wpf-validation"],
        ["inotifydataerrorinfo"] = ["implementing-wpf-validation"],
        ["dependency injection"] = ["configuring-dependency-injection"],
        ["의존성 주입"] = ["configuring-dependency-injection"],
        ["종속성 주입"] = ["configuring-dependency-injection"],
        ["generichost"] = ["configuring-dependency-injection"],
        ["host builder"] = ["configuring-dependency-injection"],
        ["addsingleton"] = ["configuring-dependency-injection"],
        ["project structure"] = ["structuring-wpf-projects"],
        ["프로젝트 구조"] = ["structuring-wpf-projects"],
        ["solution structure"] = ["structuring-wpf-projects"],
        ["솔루션 구조"] = ["structuring-wpf-projects"],

        // ─────────────────────────────────────────────────────────
        // Rendering & Performance
        // ─────────────────────────────────────────────────────────
        ["drawingcontext"] = ["rendering-with-drawingcontext"],
        ["drawing context"] = ["rendering-with-drawingcontext"],
        ["드로잉 컨텍스트"] = ["rendering-with-drawingcontext"],
        ["onrender"] = ["rendering-with-drawingcontext"],
        ["invalidatevisual"] = ["rendering-with-drawingcontext"],
        ["drawingvisual"] = ["rendering-with-drawingvisual"],
        ["drawing visual"] = ["rendering-with-drawingvisual"],
        ["드로잉 비주얼"] = ["rendering-with-drawingvisual"],
        ["containervisual"] = ["rendering-with-drawingvisual"],
        ["render pipeline"] = ["rendering-wpf-architecture"],
        ["렌더링 파이프라인"] = ["rendering-wpf-architecture"],
        ["measure arrange"] = ["rendering-wpf-architecture"],
        ["레이아웃 패스"] = ["rendering-wpf-architecture"],
        ["layoutpass"] = ["rendering-wpf-architecture"],
        ["bitmapcache"] = ["rendering-wpf-high-performance"],
        ["cachemode"] = ["rendering-wpf-high-performance"],
        ["performance"] = ["rendering-wpf-high-performance"],
        ["성능"] = ["rendering-wpf-high-performance"],
        ["고성능"] = ["rendering-wpf-high-performance"],
        ["hittest"] = ["implementing-hit-testing"],
        ["hit test"] = ["implementing-hit-testing"],
        ["히트 테스트"] = ["implementing-hit-testing"],
        ["충돌 검사"] = ["implementing-hit-testing"],
        ["visualtreehelper.hittest"] = ["implementing-hit-testing"],
        ["virtualizingstackpanel"] = ["virtualizing-wpf-ui"],
        ["virtualizing"] = ["virtualizing-wpf-ui"],
        ["가상화"] = ["virtualizing-wpf-ui"],
        ["virtualizationmode"] = ["virtualizing-wpf-ui"],
        ["freeze"] = ["optimizing-wpf-memory"],
        ["freezable"] = ["optimizing-wpf-memory"],
        ["프리즈"] = ["optimizing-wpf-memory"],
        ["동결"] = ["optimizing-wpf-memory"],
        ["weak event"] = ["optimizing-wpf-memory"],
        ["약한 이벤트"] = ["optimizing-wpf-memory"],
        ["메모리 최적화"] = ["optimizing-wpf-memory"],
        ["visualtree"] = ["navigating-visual-logical-tree"],
        ["비주얼 트리"] = ["navigating-visual-logical-tree"],
        ["logicaltree"] = ["navigating-visual-logical-tree"],
        ["로지컬 트리"] = ["navigating-visual-logical-tree"],
        ["논리 트리"] = ["navigating-visual-logical-tree"],
        ["visualtreehelper"] = ["navigating-visual-logical-tree"],
        ["logicaltreehelper"] = ["navigating-visual-logical-tree"],
        ["transform"] = ["checking-image-bounds-transform"],
        ["변환"] = ["checking-image-bounds-transform"],
        ["rendertransform"] = ["checking-image-bounds-transform"],
        ["transformtoancestor"] = ["checking-image-bounds-transform"],

        // ─────────────────────────────────────────────────────────
        // Input & Events
        // ─────────────────────────────────────────────────────────
        ["routed event"] = ["routing-wpf-events"],
        ["routedevent"] = ["routing-wpf-events"],
        ["라우티드 이벤트"] = ["routing-wpf-events"],
        ["라우팅 이벤트"] = ["routing-wpf-events"],
        ["bubbling"] = ["routing-wpf-events"],
        ["버블링"] = ["routing-wpf-events"],
        ["tunneling"] = ["routing-wpf-events"],
        ["터널링"] = ["routing-wpf-events"],
        ["previewmouse"] = ["routing-wpf-events"],
        ["popup"] = ["managing-wpf-popup-focus"],
        ["팝업"] = ["managing-wpf-popup-focus"],
        ["popup focus"] = ["managing-wpf-popup-focus"],
        ["팝업 포커스"] = ["managing-wpf-popup-focus"],

        // ─────────────────────────────────────────────────────────
        // Application & Threading
        // ─────────────────────────────────────────────────────────
        ["dispatcher"] = ["threading-wpf-dispatcher"],
        ["디스패처"] = ["threading-wpf-dispatcher"],
        ["dispatcherpriority"] = ["threading-wpf-dispatcher"],
        ["invoke"] = ["threading-wpf-dispatcher"],
        ["begininvoke"] = ["threading-wpf-dispatcher"],
        ["application lifecycle"] = ["managing-wpf-application-lifecycle"],
        ["앱 생명주기"] = ["managing-wpf-application-lifecycle"],
        ["애플리케이션 수명주기"] = ["managing-wpf-application-lifecycle"],
        ["app.xaml"] = ["managing-wpf-application-lifecycle"],
        ["startup"] = ["managing-wpf-application-lifecycle"],
        ["시작"] = ["managing-wpf-application-lifecycle"],
        ["sessionending"] = ["managing-wpf-application-lifecycle"],

        // ─────────────────────────────────────────────────────────
        // .NET Common
        // ─────────────────────────────────────────────────────────
        ["repository pattern"] = ["implementing-repository-pattern"],
        ["리포지토리 패턴"] = ["implementing-repository-pattern"],
        ["저장소 패턴"] = ["implementing-repository-pattern"],
        ["service layer"] = ["implementing-repository-pattern"],
        ["서비스 레이어"] = ["implementing-repository-pattern"],
        ["const string"] = ["managing-literal-strings"],
        ["상수 문자열"] = ["managing-literal-strings"],
        ["literal string"] = ["managing-literal-strings"],
        ["리터럴 문자열"] = ["managing-literal-strings"],
        ["console app di"] = ["configuring-console-app-di"],
        ["콘솔 앱 di"] = ["configuring-console-app-di"],
        ["console host"] = ["configuring-console-app-di"],

        // ─────────────────────────────────────────────────────────
        // Build & Deployment
        // ─────────────────────────────────────────────────────────
        // PDB embedding
        ["pdb"] = ["embedding-pdb-in-exe"],
        ["pdb 임베드"] = ["embedding-pdb-in-exe"],
        ["pdb 내장"] = ["embedding-pdb-in-exe"],
        ["pdb embedded"] = ["embedding-pdb-in-exe"],
        ["debugtype"] = ["embedding-pdb-in-exe"],
        ["디버그 심볼"] = ["embedding-pdb-in-exe"],
        ["debug symbols"] = ["embedding-pdb-in-exe"],
        ["source link"] = ["embedding-pdb-in-exe"],
        ["소스 링크"] = ["embedding-pdb-in-exe"],

        // Publishing & Packaging
        ["publish"] = ["publishing-wpf-apps"],
        ["배포"] = ["publishing-wpf-apps"],
        ["deploy"] = ["publishing-wpf-apps"],
        ["release"] = ["publishing-wpf-apps"],
        ["릴리스"] = ["publishing-wpf-apps"],
        ["packaging"] = ["publishing-wpf-apps"],
        ["패키징"] = ["publishing-wpf-apps"],
        ["dotnet publish"] = ["publishing-wpf-apps"],
        ["self-contained"] = ["publishing-wpf-apps"],
        ["selfcontained"] = ["publishing-wpf-apps"],
        ["자체 포함"] = ["publishing-wpf-apps"],
        ["single-file"] = ["publishing-wpf-apps"],
        ["singlefile"] = ["publishing-wpf-apps"],
        ["단일 파일"] = ["publishing-wpf-apps"],
        ["publishsinglefile"] = ["publishing-wpf-apps"],
        ["readytorun"] = ["publishing-wpf-apps"],
        ["r2r"] = ["publishing-wpf-apps"],

        // Installers
        ["installer"] = ["publishing-wpf-apps"],
        ["인스톨러"] = ["publishing-wpf-apps"],
        ["설치 프로그램"] = ["publishing-wpf-apps"],
        ["velopack"] = ["publishing-wpf-apps"],
        ["msix"] = ["publishing-wpf-apps"],
        ["nsis"] = ["publishing-wpf-apps"],
        ["inno setup"] = ["publishing-wpf-apps"],
        ["innosetup"] = ["publishing-wpf-apps"],
        ["auto update"] = ["publishing-wpf-apps"],
        ["자동 업데이트"] = ["publishing-wpf-apps"],

        // ─────────────────────────────────────────────────────────
        // 3rd Party Libraries
        // ─────────────────────────────────────────────────────────
        // FlaUI cross-process input
        ["flaui"] = ["flaui-cross-process-input", "flaui-wpf-element-discovery"],
        ["cross-process"] = ["flaui-cross-process-input"],
        ["크로스 프로세스"] = ["flaui-cross-process-input"],
        ["sendinput"] = ["flaui-cross-process-input"],
        ["keybd_event"] = ["flaui-cross-process-input"],
        ["stuck key"] = ["flaui-cross-process-input"],
        ["키 고착"] = ["flaui-cross-process-input"],
        ["xunit.runner.json"] = ["flaui-cross-process-input"],
        ["parallelizetestcollections"] = ["flaui-cross-process-input"],
        ["parallelizeassembly"] = ["flaui-cross-process-input"],

        // FlaUI element discovery
        ["findalldescendants"] = ["flaui-wpf-element-discovery"],
        ["automationid"] = ["flaui-wpf-element-discovery"],
        ["byautomationid"] = ["flaui-wpf-element-discovery"],
        ["automationpeer"] = ["flaui-wpf-element-discovery"],
        ["uia tree"] = ["flaui-wpf-element-discovery"],
        ["uia 트리"] = ["flaui-wpf-element-discovery"],

        // Nodify
        ["nodify"] = ["integrating-nodify"],
        ["nodifyeditor"] = ["integrating-nodify"],
        ["node graph"] = ["integrating-nodify"],
        ["노드 에디터"] = ["integrating-nodify"],
        ["노드 그래프"] = ["integrating-nodify"],

        // WPF-UI (Fluent)
        ["wpf-ui"] = ["integrating-wpfui-fluent"],
        ["wpfui"] = ["integrating-wpfui-fluent"],
        ["fluentwindow"] = ["integrating-wpfui-fluent"],
        ["navigationview"] = ["integrating-wpfui-fluent"],

        // LiveCharts2
        ["livecharts"] = ["integrating-livecharts2"],
        ["cartesianchart"] = ["integrating-livecharts2"],
        ["piechart"] = ["integrating-livecharts2"],

        // FluentValidation
        ["fluentvalidation"] = ["validating-with-fluentvalidation"],
        ["abstractvalidator"] = ["validating-with-fluentvalidation"],
        ["rulefor"] = ["validating-with-fluentvalidation"],

        // ErrorOr
        ["erroror"] = ["handling-errors-with-erroror"],
        ["result pattern"] = ["handling-errors-with-erroror"],
        ["결과 패턴"] = ["handling-errors-with-erroror"],

        // ScottPlot (single keyword — compound match in compoundKeywordMap)
        ["scottplot"] = ["scottplot-syncing-modifier-keys-for-mousewheel"],

        // ─────────────────────────────────────────────────────────
        // Scaffolding
        // ─────────────────────────────────────────────────────────
        ["viewmodel 생성"] = ["make-wpf-viewmodel"],
        ["뷰모델 생성"] = ["make-wpf-viewmodel"],
        ["generate viewmodel"] = ["make-wpf-viewmodel"],
        ["create viewmodel"] = ["make-wpf-viewmodel"],
        ["scaffold viewmodel"] = ["make-wpf-viewmodel"],
        ["서비스 생성"] = ["make-wpf-service"],
        ["generate service"] = ["make-wpf-service"],
        ["create service"] = ["make-wpf-service"],
        ["scaffold service"] = ["make-wpf-service"],

        // ─────────────────────────────────────────────────────────
        // Testing
        // ─────────────────────────────────────────────────────────
        ["viewmodel test"] = ["testing-wpf-viewmodels"],
        ["뷰모델 테스트"] = ["testing-wpf-viewmodels"],
        ["단위 테스트"] = ["testing-wpf-viewmodels"],
        ["unit test"] = ["testing-wpf-viewmodels"],
        ["xunit"] = ["testing-wpf-viewmodels"],
        ["nsubstitute"] = ["testing-wpf-viewmodels"],

        // ─────────────────────────────────────────────────────────
        // Feature Intent Keywords (기능 의도)
        // Users describe what they WANT, not technical terms
        // ─────────────────────────────────────────────────────────
        // Charts & Data Visualization
        ["차트"] = ["integrating-livecharts2"],
        ["chart"] = ["integrating-livecharts2"],
        ["그래프"] = ["integrating-livecharts2"],
        ["graph"] = ["integrating-livecharts2"],
        ["데이터 시각화"] = ["integrating-livecharts2"],
        ["data visualization"] = ["integrating-livecharts2"],
        ["dashboard"] = ["integrating-livecharts2"],
        ["대시보드"] = ["integrating-livecharts2"],

        // Input Validation
        ["입력 검증"] = ["validating-with-fluentvalidation", "implementing-wpf-validation"],
        ["input validation"] = ["validating-with-fluentvalidation", "implementing-wpf-validation"],
        ["폼 검증"] = ["validating-with-fluentvalidation", "implementing-wpf-validation"],
        ["form validation"] = ["validating-with-fluentvalidation", "implementing-wpf-validation"],

        // Modern UI / Fluent Design
        ["모던 ui"] = ["integrating-wpfui-fluent"],
        ["modern ui"] = ["integrating-wpfui-fluent"],
        ["플루언트 디자인"] = ["integrating-wpfui-fluent"],
        ["fluent design"] = ["integrating-wpfui-fluent"],

        // Node Editor / Visual Scripting
        ["노드 편집기"] = ["integrating-nodify"],
        ["node editor"] = ["integrating-nodify"],
        ["비주얼 스크립팅"] = ["integrating-nodify"],
        ["visual scripting"] = ["integrating-nodify"],
        ["워크플로우 편집기"] = ["integrating-nodify"],
        ["workflow editor"] = ["integrating-nodify"],

        // Error Handling
        ["에러 처리"] = ["handling-errors-with-erroror"],
        ["error handling"] = ["handling-errors-with-erroror"],
        ["예외 처리 패턴"] = ["handling-errors-with-erroror"],

        // Multi-language

        // Drag and Drop (intent)

        // Large Data / Performance (intent)
        ["대용량 데이터"] = ["virtualizing-wpf-ui", "rendering-wpf-high-performance"],
        ["large data"] = ["virtualizing-wpf-ui", "rendering-wpf-high-performance"],
        ["느려"] = ["rendering-wpf-high-performance", "optimizing-wpf-memory"],
        ["slow"] = ["rendering-wpf-high-performance", "optimizing-wpf-memory"],
        ["렉"] = ["rendering-wpf-high-performance", "optimizing-wpf-memory"],
        ["lag"] = ["rendering-wpf-high-performance", "optimizing-wpf-memory"],

        // UI Automation Testing (intent)
        ["ui 테스트"] = ["flaui-cross-process-input", "flaui-wpf-element-discovery"],
        ["ui test"] = ["flaui-cross-process-input", "flaui-wpf-element-discovery"],
        ["자동화 테스트"] = ["flaui-cross-process-input", "flaui-wpf-element-discovery"],
        ["automation test"] = ["flaui-cross-process-input", "flaui-wpf-element-discovery"],

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

    // ============================================================
    // COMPOUND KEYWORDS MAPPING (ALL keywords must match)
    // ============================================================
    (string[] requiredKeywords, string[] skills)[] compoundKeywordMap =
    [
        // ScottPlot — trigger only when scottplot + mousewheel + focus + modifier all present
        (["scottplot", "mousewheel", "focus", "modifier"], ["scottplot-syncing-modifier-keys-for-mousewheel"])
    ];

    // Detect skills (single keyword)
    foreach (var (keyword, skills) in keywordSkillMap)
    {
        if (lowerPrompt.Contains(keyword))
        {
            foreach (var skill in skills)
                detectedSkills.Add(skill);
        }
    }

    // Detect skills (compound keywords — ALL must match)
    foreach (var (requiredKeywords, skills) in compoundKeywordMap)
    {
        if (requiredKeywords.All(k => lowerPrompt.Contains(k)))
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
