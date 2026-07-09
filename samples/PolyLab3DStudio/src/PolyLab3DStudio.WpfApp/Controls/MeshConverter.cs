using PolyLab3DStudio.Core;

namespace PolyLab3DStudio.Controls;

/// <summary>Converts Core mesh/point data into frozen WPF Media3D geometry.</summary>
public static class MeshConverter
{
    private static readonly Dictionary<ShapeKind, MeshGeometry3D> Cache = [];

    public static MeshGeometry3D ForKind(ShapeKind kind) => ForKind(kind, 0);

    /// <param name="pointCount">Cloud point count (0 = kind default); ignored for solids.</param>
    public static MeshGeometry3D ForKind(ShapeKind kind, int pointCount)
    {
        if (ShapeCatalog.Get(kind).IsCloud)
        {
            // Clouds vary per object (point count), so they are built fresh instead of cached.
            MeshGeometry3D cloud = PointCloud(PointCloudFactory.ForKind(kind, pointCount));
            cloud.Freeze();
            return cloud;
        }

        if (Cache.TryGetValue(kind, out MeshGeometry3D? cached))
        {
            return cached;
        }

        MeshGeometry3D geometry = ToMeshGeometry3D(kind switch
        {
            ShapeKind.Sphere => MeshFactory.Sphere(),
            ShapeKind.Cylinder => MeshFactory.Cylinder(),
            ShapeKind.Cone => MeshFactory.Cone(),
            ShapeKind.Torus => MeshFactory.Torus(),
            ShapeKind.Board => MeshFactory.Board(),
            _ => MeshFactory.Cube(),
        });

        geometry.Freeze();
        Cache[kind] = geometry;
        return geometry;
    }

    public static MeshGeometry3D ToMeshGeometry3D(MeshData data)
    {
        var m = new MeshGeometry3D();
        foreach (Point3 p in data.Positions)
        {
            m.Positions.Add(new Point3D(p.X, p.Y, p.Z));
        }

        foreach (int i in data.TriangleIndices)
        {
            m.TriangleIndices.Add(i);
        }

        return m;
    }

    /// <summary>
    /// Renders each scan point as a tiny octahedron, exactly like the design's
    /// exported AddPointCloud code (point size 0.022). Each point's normalized
    /// height is written into TextureCoordinates.X so a horizontal gradient
    /// brush can paint the height colormap.
    /// </summary>
    public static MeshGeometry3D PointCloud(IReadOnlyList<Point3> points, double s = 0.022)
    {
        int[] idx = [3, 0, 4, 3, 4, 1, 3, 1, 5, 3, 5, 0, 2, 4, 0, 2, 1, 4, 2, 5, 1, 2, 0, 5];
        var m = new MeshGeometry3D();

        double minY = double.MaxValue, maxY = double.MinValue;
        foreach (Point3 p in points)
        {
            minY = Math.Min(minY, p.Y);
            maxY = Math.Max(maxY, p.Y);
        }

        double range = Math.Max(1e-6, maxY - minY);

        foreach (Point3 p in points)
        {
            int b = m.Positions.Count;
            m.Positions.Add(new Point3D(p.X - s, p.Y, p.Z));
            m.Positions.Add(new Point3D(p.X + s, p.Y, p.Z));
            m.Positions.Add(new Point3D(p.X, p.Y - s, p.Z));
            m.Positions.Add(new Point3D(p.X, p.Y + s, p.Z));
            m.Positions.Add(new Point3D(p.X, p.Y, p.Z - s));
            m.Positions.Add(new Point3D(p.X, p.Y, p.Z + s));

            double u = (p.Y - minY) / range;
            for (int k = 0; k < 6; k++)
            {
                m.TextureCoordinates.Add(new Point(u, 0));
            }

            foreach (int k in idx)
            {
                m.TriangleIndices.Add(b + k);
            }
        }

        return m;
    }

    /// <summary>Adds an axis-aligned box to a mesh (used for grid/axis/selection lines).</summary>
    public static void AddBox(MeshGeometry3D m, Point3D center, double sx, double sy, double sz)
    {
        double x = sx / 2, y = sy / 2, z = sz / 2;
        Point3D[] p =
        [
            new(center.X - x, center.Y - y, center.Z - z), new(center.X + x, center.Y - y, center.Z - z),
            new(center.X + x, center.Y + y, center.Z - z), new(center.X - x, center.Y + y, center.Z - z),
            new(center.X - x, center.Y - y, center.Z + z), new(center.X + x, center.Y - y, center.Z + z),
            new(center.X + x, center.Y + y, center.Z + z), new(center.X - x, center.Y + y, center.Z + z),
        ];
        int[][] faces = [[0, 3, 2, 1], [4, 5, 6, 7], [0, 4, 7, 3], [1, 2, 6, 5], [3, 7, 6, 2], [0, 1, 5, 4]];
        foreach (int[] face in faces)
        {
            int b = m.Positions.Count;
            foreach (int i in face)
            {
                m.Positions.Add(p[i]);
            }

            m.TriangleIndices.Add(b);
            m.TriangleIndices.Add(b + 1);
            m.TriangleIndices.Add(b + 2);
            m.TriangleIndices.Add(b);
            m.TriangleIndices.Add(b + 2);
            m.TriangleIndices.Add(b + 3);
        }
    }
}
