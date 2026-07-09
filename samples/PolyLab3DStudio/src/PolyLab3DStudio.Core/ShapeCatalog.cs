namespace PolyLab3DStudio.Core;

/// <summary>
/// The nine creatable shapes (six solids + three point clouds) with the design's
/// icon path data. Arc commands are rewritten with separated flags so WPF's
/// geometry mini-language parser accepts them.
/// </summary>
public static class ShapeCatalog
{
    public static IReadOnlyList<ShapeDefinition> Solids { get; } =
    [
        new(ShapeKind.Cube, "큐브", "Cube", 0.5,
            "M12 2 l8 4.5 v9 L12 20 l-8 -4.5 v-9 L12 2 z M12 11 l8 -4.5 M12 11 L4 6.5 M12 11 v9", false),
        new(ShapeKind.Sphere, "구", "Sphere", 0.62,
            "M12 3 a9 9 0 1 0 0 18 a9 9 0 0 0 0 -18 z M3.4 12 c3 2.6 14.2 2.6 17.2 0", false),
        new(ShapeKind.Cylinder, "원기둥", "Cylinder", 0.55,
            "M5 6 c0 -1.7 3.1 -3 7 -3 s7 1.3 7 3 v12 c0 1.7 -3.1 3 -7 3 s-7 -1.3 -7 -3 V6 M5 6 c0 1.7 3.1 3 7 3 s7 -1.3 7 -3", false),
        new(ShapeKind.Cone, "원뿔", "Cone", 0.55,
            "M12 3 l8 14.2 M12 3 L4 17.2 M4 17.2 c0 2 3.6 3.3 8 3.3 s8 -1.3 8 -3.3", false),
        new(ShapeKind.Torus, "도넛", "Torus", 0.19,
            "M12 6 a9 5.5 0 1 0 0 11 a9 5.5 0 1 0 0 -11 z M12 9.5 a3.6 2 0 1 0 0 4 a3.6 2 0 1 0 0 -4 z", false),
        new(ShapeKind.Board, "판", "Board", 0.06,
            "M3 12 l8 -4.5 h10 L13 12 H3 z M3 12 v4 h10 v-4 M21 7.5 v4 l-8 4.5", false),
    ];

    public static IReadOnlyList<ShapeDefinition> Clouds { get; } =
    [
        new(ShapeKind.CloudSphere, "점 구", "Point Cloud Sphere", 0.78,
            "M20.5 12 h.01 M19.4 16.3 h.01 M16.3 19.4 h.01 M12 20.5 h.01 M7.7 19.4 h.01 M4.6 16.3 h.01 M3.5 12 h.01 M4.6 7.7 h.01 M7.7 4.6 h.01 M12 3.5 h.01 M16.3 4.6 h.01 M19.4 7.7 h.01 M9 13 h.01 M15 13 h.01 M12 9.5 h.01", true),
        new(ShapeKind.CloudTorus, "점 도넛", "Point Cloud Torus", 0.24,
            "M21 12 h.01 M19.8 15.5 h.01 M16.5 18 h.01 M12 19 h.01 M7.5 18 h.01 M4.2 15.5 h.01 M3 12 h.01 M4.2 8.5 h.01 M7.5 6 h.01 M12 5 h.01 M16.5 6 h.01 M19.8 8.5 h.01 M16 12 h.01 M14 15 h.01 M10 15 h.01 M8 12 h.01 M10 9 h.01 M14 9 h.01", true),
        new(ShapeKind.CloudTerrain, "점 지형", "Point Cloud Terrain", 0.5,
            "M4 8 h.01 M8 6.5 h.01 M12 8 h.01 M16 6 h.01 M20 7.5 h.01 M4 12 h.01 M8 10.5 h.01 M12 12 h.01 M16 10 h.01 M20 11.5 h.01 M4 16 h.01 M8 14.5 h.01 M12 16 h.01 M16 14 h.01 M20 15.5 h.01 M4 20 h.01 M8 18.5 h.01 M12 20 h.01 M16 18 h.01 M20 19.5 h.01", true),
    ];

    public static IReadOnlyList<ShapeDefinition> All { get; } = [.. Solids, .. Clouds];

    public static ShapeDefinition Get(ShapeKind kind) => All.First(d => d.Kind == kind);

    /// <summary>Spawn palette used when adding shapes, in the design's order.</summary>
    public static IReadOnlyList<string> AddPalette { get; } =
        ["#4C8DF5", "#F0562F", "#FFC24B", "#7BC96F", "#37A6A0", "#8A6FE8", "#F275B0", "#E8EAED"];

    /// <summary>Material swatches shown in the properties panel.</summary>
    public static IReadOnlyList<string> Swatches { get; } =
        ["#E8EAED", "#F0562F", "#FFC24B", "#7BC96F", "#37A6A0", "#4C8DF5", "#8A6FE8", "#F275B0", "#8B5A3C", "#3A4148"];
}
