namespace PolyLab3DStudio.Core;

/// <summary>
/// Deterministic point cloud generators that fake scan data, matching the studio
/// design's exported WPF code (same seeds and formulas).
/// </summary>
public static class PointCloudFactory
{
    /// <summary>Point count bounds selectable in the properties panel.</summary>
    public const int MinCount = 100;

    public const int MaxCount = 100_000;

    public static IReadOnlyList<Point3> ForKind(ShapeKind kind) => ForKind(kind, 0);

    /// <summary>Generates the cloud with a requested point count (0 = kind default).</summary>
    public static IReadOnlyList<Point3> ForKind(ShapeKind kind, int count)
    {
        int n = count > 0 ? Math.Clamp(count, MinCount, MaxCount) : DefaultCount(kind);
        return kind switch
        {
            ShapeKind.CloudSphere => Sphere(n),
            ShapeKind.CloudTorus => Torus(n),
            ShapeKind.CloudTerrain => Terrain(TerrainGridFromCount(n)),
            _ => [],
        };
    }

    public static int DefaultCount(ShapeKind kind) => kind switch
    {
        ShapeKind.CloudTorus => 1600,
        ShapeKind.CloudTerrain => 1849, // 43 × 43 grid
        _ => 1500,
    };

    /// <summary>Terrain is an (n+1)² grid; picks n so the grid best matches the requested count.</summary>
    public static int TerrainGridFromCount(int count) =>
        Math.Max(2, (int)Math.Round(Math.Sqrt(count)) - 1);

    public static IReadOnlyList<Point3> Sphere(int n = 1500, int seed = 7)
    {
        var rnd = new Random(seed);
        var pts = new List<Point3>(n);
        for (int i = 0; i < n; i++)
        {
            double u = (rnd.NextDouble() * 2) - 1, t = rnd.NextDouble() * Math.PI * 2;
            double s = Math.Sqrt(1 - (u * u));
            double r = 0.75 * (1 + ((rnd.NextDouble() - 0.5) * 0.06));
            pts.Add(new Point3(s * Math.Cos(t) * r, u * r, s * Math.Sin(t) * r));
        }

        return pts;
    }

    public static IReadOnlyList<Point3> Torus(int n = 1600, int seed = 13)
    {
        var rnd = new Random(seed);
        var pts = new List<Point3>(n);
        for (int i = 0; i < n; i++)
        {
            double u = rnd.NextDouble() * Math.PI * 2, v = rnd.NextDouble() * Math.PI * 2;
            double r = 0.2 * (1 + ((rnd.NextDouble() - 0.5) * 0.15));
            pts.Add(new Point3(
                (0.55 + (r * Math.Cos(v))) * Math.Cos(u),
                r * Math.Sin(v),
                (0.55 + (r * Math.Cos(v))) * Math.Sin(u)));
        }

        return pts;
    }

    public static IReadOnlyList<Point3> Terrain(int n = 42, int seed = 42)
    {
        var rnd = new Random(seed);
        var pts = new List<Point3>((n + 1) * (n + 1));
        for (int i = 0; i <= n; i++)
        {
            for (int j = 0; j <= n; j++)
            {
                double x = (((double)i / n) - 0.5) * 2.0, z = (((double)j / n) - 0.5) * 2.0;
                double y = (0.28 * Math.Sin(x * 2.6) * Math.Cos(z * 2.2))
                    + (0.14 * Math.Sin((x * 5.1) + 1.3) * Math.Sin(z * 4.3))
                    + ((rnd.NextDouble() - 0.5) * 0.02);
                pts.Add(new Point3(x, y, z));
            }
        }

        return pts;
    }
}
