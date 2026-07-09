namespace PolyLab3DStudio.Core;

public static class ToolCatalog
{
    public const string Select = "select";
    public const string Move = "move";
    public const string Rotate = "rotate";
    public const string Scale = "scale";

    public static IReadOnlyList<ToolDefinition> All { get; } =
    [
        new(Select, "선택", "Select (Q)", "Q",
            "M7 3 l11 10 l-5.6 .9 l2.6 5.4 l-2.7 1.3 l-2.6 -5.4 L7 18 V3 z"),
        new(Move, "이동", "Move (W)", "W",
            "M12 2 v20 M2 12 h20 M12 2 l-2.5 2.5 M12 2 l2.5 2.5 M12 22 l-2.5 -2.5 M12 22 l2.5 -2.5 M2 12 l2.5 -2.5 M2 12 l2.5 2.5 M22 12 l-2.5 -2.5 M22 12 l-2.5 2.5"),
        new(Rotate, "회전", "Rotate (E)", "E",
            "M20.5 12 a8.5 8.5 0 1 1 -2.6 -6.1 M20.5 3.5 v4 h-4"),
        new(Scale, "크기", "Scale (R)", "R",
            "M4 20 L20 4 M20 4 h-5.5 M20 4 v5.5 M4 20 h5.5 M4 20 v-5.5"),
    ];

    /// <summary>Status bar hint per active tool, from the design.</summary>
    public static string HintFor(string tool) => tool switch
    {
        Move => "선택한 오브젝트를 드래그: 바닥 위 이동 · Shift+드래그: 위/아래 이동",
        Rotate => "선택한 오브젝트를 좌우로 드래그: 회전",
        Scale => "선택한 오브젝트를 위아래로 드래그: 크기 조절",
        _ => "클릭: 선택 · 드래그: 시점 회전 · 휠: 확대/축소 · 오른쪽 드래그: 화면 이동",
    };
}
