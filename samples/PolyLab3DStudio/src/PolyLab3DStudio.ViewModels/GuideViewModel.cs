namespace PolyLab3DStudio.ViewModels;

/// <summary>
/// Static content of the tool concept guide, transcribed from the
/// "도구 개념 가이드" design screen. Body text keeps the original Korean copy;
/// <c>**bold**</c> segments are rendered as bold inlines by the view layer.
/// </summary>
public sealed partial class GuideViewModel(ShellViewModel shell) : ObservableObject
{
    public ShellViewModel Shell { get; } = shell;

    [RelayCommand]
    private void GoStart() => Shell.GoStart();

    [RelayCommand]
    private void GoStudioFree() => Shell.GoStudioFree();

    public IReadOnlyList<WorldCard> WorldCards { get; } =
    [
        new(
            IconKey: "raster",
            Name: "래스터",
            EnglishTag: "RASTER",
            ToolName: "GIMP",
            Description: "그림이 **픽셀(색깔 점)의 격자**로 저장돼요. 확대하면 계단처럼 깨지지만, 사진처럼 미묘한 색을 다루는 데 최고예요.",
            Uses: "사진 보정 · 합성 · 디지털 페인팅",
            Formats: "PNG · JPG · XCF"),
        new(
            IconKey: "vector",
            Name: "벡터",
            EnglishTag: "VECTOR",
            ToolName: "Inkscape",
            Description: "그림이 **수학적인 선(패스)과 점(노드)**으로 저장돼요. 아무리 확대해도 깨지지 않아 로고·아이콘에 딱이에요.",
            Uses: "로고 · 아이콘 · 일러스트 · 인쇄물",
            Formats: "SVG · PDF · EPS"),
        new(
            IconKey: "mesh",
            Name: "3D",
            EnglishTag: "3D MODELING",
            ToolName: "폴리랩 → Blender",
            Description: "그림이 아니라 **공간 속의 입체(메시)**를 만들어요. 카메라와 빛을 정해서 원하는 각도의 그림을 \"찍어\" 냅니다.",
            Uses: "게임 소품 · 3D 프린팅 · 제품 시안",
            Formats: "OBJ · GLB · STL"),
    ];

    public IReadOnlyList<ToolGuideSection> ToolSections { get; } =
    [
        new(
            Number: "02",
            Title: "GIMP는 이렇게 동작해요",
            Tagline: "픽셀을 칠하는 도구",
            Bullets:
            [
                "**캔버스** — 정해진 크기의 픽셀 격자 위에서 작업해요. 폴리랩의 뷰포트처럼 **휠로 확대/축소**, 스페이스+드래그로 화면 이동.",
                "**레이어** — 투명한 필름을 겹쳐 놓은 구조. 폴리랩의 **[장면] 목록에 있는 오브젝트**와 같은 역할이에요. 하나씩 따로 고치고, 순서를 바꿔요.",
                "**선택 영역** — \"여기만 고칠게\"라고 범위를 정하는 것. 폴리랩에서 오브젝트를 **클릭해 선택**하는 것과 같은 개념이에요.",
                "**브러시와 필터** — 브러시로 픽셀을 직접 칠하고, 필터(흐림·선명 등)로 한꺼번에 계산해서 바꿔요.",
            ],
            WorkflowSteps: ["1 사진/캔버스 열기", "2 고칠 부분을 선택", "3 레이어 위에서 칠하고 보정", "4 PNG/JPG로 내보내기"],
            Shortcuts:
            [
                new("M", "이동"),
                new("R", "사각 선택"),
                new("F", "자유 선택"),
                new("P", "붓"),
                new("Shift+S", "크기"),
                new("Shift+R", "회전"),
                new("Ctrl+Z", "실행 취소", IsAccent: true),
            ],
            Footnote: "폴리랩과 똑같이 이동·회전·크기 도구가 따로 있고, Ctrl+Z도 그대로예요."),
        new(
            Number: "03",
            Title: "Inkscape는 이렇게 동작해요",
            Tagline: "선을 그리는 도구",
            Bullets:
            [
                "**오브젝트** — 사각형, 원, 패스 하나하나가 독립된 오브젝트예요. 폴리랩의 큐브·구와 완전히 같은 개념! 클릭해 선택하고 각각 옮겨요.",
                "**패스와 노드** — 모든 모양은 점(노드)을 곡선으로 이은 것. 노드를 끌면 모양이 바뀌어요. 3D에서 정점(Vertex)을 편집하는 것의 2D 버전이에요.",
                "**채우기와 윤곽선** — 오브젝트마다 안쪽 색(Fill)과 테두리(Stroke)를 정해요. 폴리랩의 **재질 패널**과 같은 자리예요.",
                "**불리언 연산** — 두 도형을 합치거나(Union) 한쪽으로 구멍을 뚫어요(Difference). 복잡한 로고도 기본 도형의 조합으로 만들어요.",
            ],
            WorkflowSteps: ["1 기본 도형 그리기", "2 노드를 다듬고 도형 합치기", "3 채우기·윤곽선 색 정하기", "4 SVG/PDF로 저장"],
            Shortcuts:
            [
                new("S", "선택"),
                new("N", "노드 편집"),
                new("R", "사각형"),
                new("E", "원"),
                new("B", "펜(베지어)"),
                new("Ctrl+D", "복제"),
                new("Ctrl+Z", "실행 취소", IsAccent: true),
            ],
            Footnote: "오브젝트를 한 번 클릭하면 이동·크기 핸들, 한 번 더 클릭하면 회전 핸들 — 폴리랩의 W·E·R을 클릭 횟수로 오가는 셈이에요."),
        new(
            Number: "04",
            Title: "Blender는 이렇게 동작해요",
            Tagline: "입체를 빚는 도구 — 폴리랩의 다음 단계",
            Bullets:
            [
                "**오브젝트 모드 vs 편집 모드** — 오브젝트 모드는 폴리랩과 똑같이 물체 단위로 이동·회전·크기를 다뤄요. **Tab**을 누르면 편집 모드로 들어가 정점·엣지·면을 직접 다듬어요.",
                "**메시** — 모든 입체는 정점(Vertex)·엣지(Edge)·면(Face)의 그물이에요. 폴리랩의 큐브·구도 같은 구조로 되어 있어요.",
                "**모디파이어** — 원본을 남긴 채 효과를 겹겹이 쌓는 비파괴 편집. Subdivision으로 매끈하게, Mirror로 좌우 대칭을 자동으로 만들어요.",
                "**머티리얼과 렌더** — 색·거칠기(Roughness)·금속성(Metalness)… 폴리랩 재질 패널과 같은 항목이 그대로 있어요. 카메라와 빛을 놓고 최종 그림을 렌더링합니다.",
            ],
            WorkflowSteps: ["1 도형 추가 (Shift+A)", "2 편집 모드로 다듬기", "3 머티리얼·조명", "4 렌더/내보내기"],
            Shortcuts:
            [
                new("G", "이동"),
                new("R", "회전"),
                new("S", "크기"),
                new("Tab", "편집 모드"),
                new("Shift+A", "추가"),
                new("X", "삭제"),
                new("Ctrl+Z", "실행 취소", IsAccent: true),
            ],
            Footnote: "폴리랩의 W·E·R이 Blender에서는 G·R·S예요. 이름만 다르고 개념은 완전히 같아요 — 휠 줌, 드래그 궤도 회전도 그대로!"),
    ];

    public IReadOnlyList<ComparisonRow> ComparisonRows { get; } =
    [
        new(
            Label: "하는 일",
            Gimp: "사진 보정·합성, 디지털 페인팅 등 **픽셀 이미지 편집**. 포토샵의 무료 대안으로 불려요.",
            Inkscape: "로고·아이콘·일러스트 등 **벡터 드로잉**. 일러스트레이터의 무료 대안이에요.",
            Blender: "**3D 모델링·애니메이션·렌더링**. 마야·3ds Max급 기능의 무료 대안이에요."),
        new(
            Label: "다루는 재료",
            Gimp: "픽셀 격자 (래스터)",
            Inkscape: "패스와 노드 (벡터)",
            Blender: "메시 — 정점·엣지·면 (3D)"),
        new(
            Label: "확대하면?",
            Gimp: "계단처럼 깨져요 (해상도 한계)",
            Inkscape: "아무리 확대해도 매끈해요",
            Blender: "입체라 어느 각도·거리도 자유로워요"),
        new(
            Label: "이런 작업이라면",
            Gimp: "여행 사진 색감 보정, 배경 지우기, 유튜브 썸네일 합성",
            Inkscape: "동아리 로고, 스티커 도안, 현수막처럼 크게 인쇄할 그림",
            Blender: "게임 소품, 3D 프린팅용 모델, 제품 시안 렌더"),
        new(
            Label: "기본 파일",
            Gimp: ".XCF → PNG/JPG 내보내기",
            Inkscape: ".SVG → PDF/PNG 내보내기",
            Blender: ".BLEND → GLB/OBJ/STL, PNG 렌더",
            IsMono: true),
        new(
            Label: "라이선스",
            Gimp: "GPLv3 (자유 소프트웨어)",
            Inkscape: "GPLv2 이상 (자유 소프트웨어)",
            Blender: "GPLv2 이상 (자유 소프트웨어)",
            IsLast: true),
    ];

    public IReadOnlyList<string> PointCloudBullets { get; } =
    [
        "**표면이 없어요** — 점들 사이가 비어 있어서 그대로는 3D 프린팅이나 게임에 못 써요. 그래서 점들을 이어 **메시로 변환(재구성)**하는 단계가 꼭 필요해요.",
        "**모델링과 반대 방향** — 폴리랩·Blender가 \"무(無)에서 입체를 만드는\" 도구라면, 포인트 클라우드는 **현실을 3D로 복사해 오는 입구**예요.",
    ];

    public IReadOnlyList<string> PointCloudWorkflow { get; } =
        ["1 스캔으로 점 얻기", "2 정리·정합", "3 메시로 변환", "4 Blender 등에서 편집"];

    public IReadOnlyList<MiniToolCard> PointCloudTools { get; } =
    [
        new(
            Name: "CloudCompare",
            Body: "포인트 클라우드 정리·비교·측정의 표준 무료 도구.",
            Badge: "GPL · 무료"),
        new(
            Name: "MeshLab",
            Body: "점을 메시로 재구성하고 다듬는 데 강한 무료 도구.",
            Badge: "GPL · 무료"),
    ];
}
