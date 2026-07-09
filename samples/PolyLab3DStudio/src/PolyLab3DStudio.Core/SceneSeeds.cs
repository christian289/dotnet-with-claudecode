namespace PolyLab3DStudio.Core;

/// <summary>Lesson seed scenes and the start screen's demo scene, from the design.</summary>
public static class SceneSeeds
{
    /// <summary>Snowman + props shown on the auto-spinning start screen viewport.</summary>
    public static IReadOnlyList<SceneObjectState> Demo { get; } =
    [
        new("d1", ShapeKind.Sphere, "", 0, 0.72, 0, 0, 1.18, "#E8EAED", 0.5, 0.02),
        new("d2", ShapeKind.Sphere, "", 0, 1.86, 0, 0, 0.82, "#E8EAED", 0.5, 0.02),
        new("d3", ShapeKind.Sphere, "", 0, 2.72, 0, 0, 0.55, "#E8EAED", 0.5, 0.02),
        new("d4", ShapeKind.Cone, "", 0, 3.42, 0, 0, 0.5, "#F0562F", 0.6, 0),
        new("d5", ShapeKind.Torus, "", 0, 2.28, 0, 0, 0.62, "#F0562F", 0.55, 0),
        new("d6", ShapeKind.Cube, "", 2.4, 0.5, -1.2, 20, 1, "#37A6A0", 0.5, 0.1),
        new("d7", ShapeKind.Cylinder, "", -2.5, 0.55, 1, 0, 0.9, "#FFC24B", 0.45, 0.05),
    ];

    /// <summary>Builds the seed objects for a lesson ("single" / "two" / "lit" / anything else = empty).</summary>
    public static IReadOnlyList<SceneObjectState> For(string? seed)
    {
        int i = 0;
        long stamp = Environment.TickCount64 % 100000;
        SceneObjectState Mk(ShapeKind kind, string name, double x, double y, double z, string color, double scale = 1) =>
            new($"seed{stamp}_{i++}", kind, name, x, y, z, 0, scale, color, 0.55, 0.05);

        return seed switch
        {
            "single" => [Mk(ShapeKind.Cube, "큐브 1", 0, 0.5, 0, "#F0562F")],
            "two" =>
            [
                Mk(ShapeKind.Sphere, "구 1", -1.1, 0.62, 0, "#37A6A0"),
                Mk(ShapeKind.Cube, "큐브 1", 1.1, 0.5, 0, "#FFC24B"),
            ],
            "lit" =>
            [
                Mk(ShapeKind.Sphere, "구 1", 0, 0.68, 0, "#E8EAED", 1.1),
                Mk(ShapeKind.Sphere, "구 2", 0, 1.72, 0, "#E8EAED", 0.78),
                Mk(ShapeKind.Sphere, "구 3", 0, 2.5, 0, "#E8EAED", 0.52),
                Mk(ShapeKind.Cone, "원뿔 1", 0, 3.2, 0, "#F0562F", 0.5),
            ],
            _ => [],
        };
    }
}
