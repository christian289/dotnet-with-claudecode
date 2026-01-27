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
    // SKILL KEYWORDS MAPPING (English + Korean)
    // ============================================================
    Dictionary<string, string[]> keywordSkillMap = new()
    {
        // ─────────────────────────────────────────────────────────
        // UI & Controls
        // ─────────────────────────────────────────────────────────
        ["customcontrol"] = ["authoring-wpf-controls", "developing-wpf-customcontrols"],
        ["custom control"] = ["authoring-wpf-controls", "developing-wpf-customcontrols"],
        ["커스텀컨트롤"] = ["authoring-wpf-controls", "developing-wpf-customcontrols"],
        ["커스텀 컨트롤"] = ["authoring-wpf-controls", "developing-wpf-customcontrols"],
        ["사용자 정의 컨트롤"] = ["authoring-wpf-controls", "developing-wpf-customcontrols"],
        ["dependencyproperty"] = ["defining-wpf-dependencyproperty"],
        ["dependency property"] = ["defining-wpf-dependencyproperty"],
        ["의존성 속성"] = ["defining-wpf-dependencyproperty"],
        ["종속성 속성"] = ["defining-wpf-dependencyproperty"],
        ["templatepart"] = ["developing-wpf-customcontrols"],
        ["onapplytemplate"] = ["developing-wpf-customcontrols"],
        ["controltemplate"] = ["customizing-controltemplate"],
        ["control template"] = ["customizing-controltemplate"],
        ["컨트롤 템플릿"] = ["customizing-controltemplate"],
        ["contentcontrol"] = ["understanding-wpf-content-model"],
        ["content model"] = ["understanding-wpf-content-model"],
        ["콘텐츠 모델"] = ["understanding-wpf-content-model"],
        ["adorner"] = ["implementing-wpf-adorners"],
        ["어도너"] = ["implementing-wpf-adorners"],
        ["장식자"] = ["implementing-wpf-adorners"],
        ["dialog"] = ["creating-wpf-dialogs"],
        ["대화상자"] = ["creating-wpf-dialogs"],
        ["다이얼로그"] = ["creating-wpf-dialogs"],
        ["messagebox"] = ["creating-wpf-dialogs"],
        ["메시지박스"] = ["creating-wpf-dialogs"],
        ["flowdocument"] = ["creating-wpf-flowdocument"],
        ["플로우 문서"] = ["creating-wpf-flowdocument"],
        ["behavior"] = ["using-wpf-behaviors-triggers"],
        ["비헤이비어"] = ["using-wpf-behaviors-triggers"],
        ["interaction.triggers"] = ["using-wpf-behaviors-triggers"],
        ["eventtrigger"] = ["using-wpf-behaviors-triggers"],
        ["이벤트 트리거"] = ["using-wpf-behaviors-triggers"],
        ["converter"] = ["using-converter-markup-extension"],
        ["컨버터"] = ["using-converter-markup-extension"],
        ["변환기"] = ["using-converter-markup-extension"],
        ["ivalueconverter"] = ["using-converter-markup-extension"],
        ["imultivalueconverter"] = ["using-converter-markup-extension"],
        ["markupextension"] = ["using-converter-markup-extension"],
        ["마크업 확장"] = ["using-converter-markup-extension"],
        ["property element"] = ["using-xaml-property-element-syntax"],
        ["속성 요소 구문"] = ["using-xaml-property-element-syntax"],
        ["localization"] = ["localizing-wpf-applications"],
        ["localize"] = ["localizing-wpf-applications"],
        ["지역화"] = ["localizing-wpf-applications"],
        ["현지화"] = ["localizing-wpf-applications"],
        ["다국어"] = ["localizing-wpf-applications"],
        ["x:uid"] = ["localizing-wpf-applications"],
        ["automation"] = ["implementing-wpf-automation"],
        ["automationpeer"] = ["implementing-wpf-automation"],
        ["uiautomation"] = ["implementing-wpf-automation"],
        ["자동화"] = ["implementing-wpf-automation"],
        ["접근성"] = ["implementing-wpf-automation"],
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
        ["storyboard"] = ["creating-wpf-animations"],
        ["스토리보드"] = ["creating-wpf-animations"],
        ["animation"] = ["creating-wpf-animations"],
        ["애니메이션"] = ["creating-wpf-animations"],
        ["doubleanimation"] = ["creating-wpf-animations"],
        ["coloranimation"] = ["creating-wpf-animations"],
        ["easingfunction"] = ["creating-wpf-animations"],
        ["이징 함수"] = ["creating-wpf-animations"],
        ["brush"] = ["creating-wpf-brushes"],
        ["브러시"] = ["creating-wpf-brushes"],
        ["lineargradient"] = ["creating-wpf-brushes"],
        ["선형 그라디언트"] = ["creating-wpf-brushes"],
        ["radialgradient"] = ["creating-wpf-brushes"],
        ["방사형 그라디언트"] = ["creating-wpf-brushes"],
        ["solidcolorbrush"] = ["creating-wpf-brushes"],
        ["단색 브러시"] = ["creating-wpf-brushes"],
        ["vector icon"] = ["creating-wpf-vector-icons"],
        ["벡터 아이콘"] = ["creating-wpf-vector-icons"],
        ["pathgeometry icon"] = ["creating-wpf-vector-icons"],
        ["icon font"] = ["resolving-icon-font-inheritance"],
        ["아이콘 폰트"] = ["resolving-icon-font-inheritance"],
        ["segoe fluent"] = ["resolving-icon-font-inheritance"],

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
        ["datatemplate"] = ["mapping-viewmodel-view-datatemplate"],
        ["데이터 템플릿"] = ["mapping-viewmodel-view-datatemplate"],
        ["hierarchicaldatatemplate"] = ["mapping-viewmodel-view-datatemplate"],
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
        ["pathgeometry"] = ["implementing-2d-graphics"],
        ["geometry"] = ["implementing-2d-graphics"],
        ["지오메트리"] = ["implementing-2d-graphics"],
        ["도형"] = ["implementing-2d-graphics"],
        ["streamgeometry"] = ["implementing-2d-graphics"],
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
        ["command"] = ["handling-wpf-input-commands"],
        ["커맨드"] = ["handling-wpf-input-commands"],
        ["명령"] = ["handling-wpf-input-commands"],
        ["routedcommand"] = ["handling-wpf-input-commands"],
        ["commandbinding"] = ["handling-wpf-input-commands"],
        ["inputbinding"] = ["handling-wpf-input-commands"],
        ["입력 바인딩"] = ["handling-wpf-input-commands"],
        ["keybinding"] = ["handling-wpf-input-commands"],
        ["키 바인딩"] = ["handling-wpf-input-commands"],
        ["routed event"] = ["routing-wpf-events"],
        ["routedevent"] = ["routing-wpf-events"],
        ["라우티드 이벤트"] = ["routing-wpf-events"],
        ["라우팅 이벤트"] = ["routing-wpf-events"],
        ["bubbling"] = ["routing-wpf-events"],
        ["버블링"] = ["routing-wpf-events"],
        ["tunneling"] = ["routing-wpf-events"],
        ["터널링"] = ["routing-wpf-events"],
        ["previewmouse"] = ["routing-wpf-events"],
        ["dragdrop"] = ["implementing-wpf-dragdrop"],
        ["drag and drop"] = ["implementing-wpf-dragdrop"],
        ["드래그 앤 드롭"] = ["implementing-wpf-dragdrop"],
        ["끌어서 놓기"] = ["implementing-wpf-dragdrop"],
        ["dodragdrop"] = ["implementing-wpf-dragdrop"],
        ["dataobject"] = ["implementing-wpf-dragdrop"],
        ["popup"] = ["managing-wpf-popup-focus"],
        ["팝업"] = ["managing-wpf-popup-focus"],
        ["popup focus"] = ["managing-wpf-popup-focus"],
        ["팝업 포커스"] = ["managing-wpf-popup-focus"],
        ["clipboard"] = ["using-wpf-clipboard"],
        ["클립보드"] = ["using-wpf-clipboard"],
        ["복사 붙여넣기"] = ["using-wpf-clipboard"],
        ["mediaelement"] = ["integrating-wpf-media"],
        ["미디어"] = ["integrating-wpf-media"],
        ["media player"] = ["integrating-wpf-media"],
        ["동영상"] = ["integrating-wpf-media"],

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
        ["migrate"] = ["migrating-wpf-to-dotnet"],
        ["migration"] = ["migrating-wpf-to-dotnet"],
        ["마이그레이션"] = ["migrating-wpf-to-dotnet"],
        ["이전"] = ["migrating-wpf-to-dotnet"],
        [".net framework"] = ["migrating-wpf-to-dotnet"],
        ["baml"] = ["localizing-wpf-with-baml"],
        ["flowdirection"] = ["implementing-wpf-rtl-support"],
        ["rtl"] = ["implementing-wpf-rtl-support"],
        ["right-to-left"] = ["implementing-wpf-rtl-support"],
        ["오른쪽에서 왼쪽"] = ["implementing-wpf-rtl-support"],
        ["culture"] = ["formatting-culture-aware-data"],
        ["cultureinfo"] = ["formatting-culture-aware-data"],
        ["문화권"] = ["formatting-culture-aware-data"],

        // ─────────────────────────────────────────────────────────
        // .NET Common
        // ─────────────────────────────────────────────────────────
        ["async"] = ["handling-async-operations"],
        ["await"] = ["handling-async-operations"],
        ["비동기"] = ["handling-async-operations"],
        ["task"] = ["handling-async-operations"],
        ["valuetask"] = ["handling-async-operations"],
        ["configureawait"] = ["handling-async-operations"],
        ["parallel"] = ["processing-parallel-tasks"],
        ["병렬"] = ["processing-parallel-tasks"],
        ["병렬 처리"] = ["processing-parallel-tasks"],
        ["plinq"] = ["processing-parallel-tasks"],
        ["concurrentdictionary"] = ["processing-parallel-tasks"],
        ["span"] = ["optimizing-memory-allocation"],
        ["memory<"] = ["optimizing-memory-allocation"],
        ["메모리 할당"] = ["optimizing-memory-allocation"],
        ["arraypool"] = ["optimizing-memory-allocation"],
        ["stackalloc"] = ["optimizing-memory-allocation"],
        ["pipeline"] = ["implementing-io-pipelines"],
        ["파이프라인"] = ["implementing-io-pipelines"],
        ["pipereader"] = ["implementing-io-pipelines"],
        ["pipewriter"] = ["implementing-io-pipelines"],
        ["pubsub"] = ["implementing-pubsub-pattern"],
        ["pub/sub"] = ["implementing-pubsub-pattern"],
        ["발행 구독"] = ["implementing-pubsub-pattern"],
        ["channel"] = ["implementing-pubsub-pattern"],
        ["채널"] = ["implementing-pubsub-pattern"],
        ["observable"] = ["implementing-pubsub-pattern"],
        ["repository pattern"] = ["implementing-repository-pattern"],
        ["리포지토리 패턴"] = ["implementing-repository-pattern"],
        ["저장소 패턴"] = ["implementing-repository-pattern"],
        ["service layer"] = ["implementing-repository-pattern"],
        ["서비스 레이어"] = ["implementing-repository-pattern"],
        ["hashset"] = ["optimizing-fast-lookup"],
        ["frozenset"] = ["optimizing-fast-lookup"],
        ["dictionary"] = ["optimizing-fast-lookup"],
        ["딕셔너리"] = ["optimizing-fast-lookup"],
        ["사전"] = ["optimizing-fast-lookup"],
        ["빠른 조회"] = ["optimizing-fast-lookup"],
        ["regex"] = ["using-generated-regex"],
        ["정규식"] = ["using-generated-regex"],
        ["정규 표현식"] = ["using-generated-regex"],
        ["regular expression"] = ["using-generated-regex"],
        ["generatedregex"] = ["using-generated-regex"],
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
        ["자동 업데이트"] = ["publishing-wpf-apps"]
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
