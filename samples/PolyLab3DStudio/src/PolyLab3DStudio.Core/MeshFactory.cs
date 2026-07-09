namespace PolyLab3DStudio.Core;

/// <summary>
/// Primitive mesh generators. The algorithms mirror the studio design's exported
/// WPF code (MeshFactory in the XAML export) so the in-app viewport and the
/// exported scene look identical.
/// </summary>
public static class MeshFactory
{
    public static MeshData Cube() => Box(1, 1, 1);

    public static MeshData Board() => Box(1.6, 0.12, 1.6);

    public static MeshData Cylinder() => Frustum(0.5, 0.5, 1.1);

    public static MeshData Cone() => Frustum(0.55, 0.0, 1.1);

    public static MeshData Box(double sx, double sy, double sz)
    {
        var m = new MeshData();
        double x = sx / 2, y = sy / 2, z = sz / 2;
        Point3[] p =
        [
            new(-x, -y, -z), new(x, -y, -z), new(x, y, -z), new(-x, y, -z),
            new(-x, -y, z), new(x, -y, z), new(x, y, z), new(-x, y, z),
        ];
        int[][] faces = [[0, 3, 2, 1], [4, 5, 6, 7], [0, 4, 7, 3], [1, 2, 6, 5], [3, 7, 6, 2], [0, 1, 5, 4]];

        foreach (int[] face in faces)
        {
            int b = m.Positions.Count;
            foreach (int idx in face)
            {
                m.AddVertex(p[idx]);
            }

            m.AddTriangle(b, b + 1, b + 2);
            m.AddTriangle(b, b + 2, b + 3);
        }

        return m;
    }

    public static MeshData Sphere(double r = 0.62, int slices = 32, int stacks = 20)
    {
        var m = new MeshData();
        for (int i = 0; i <= stacks; i++)
        {
            double phi = Math.PI * i / stacks;
            for (int j = 0; j <= slices; j++)
            {
                double th = 2 * Math.PI * j / slices;
                m.AddVertex(new Point3(r * Math.Sin(phi) * Math.Cos(th), r * Math.Cos(phi), r * Math.Sin(phi) * Math.Sin(th)));
            }
        }

        for (int i = 0; i < stacks; i++)
        {
            for (int j = 0; j < slices; j++)
            {
                int a = (i * (slices + 1)) + j, b = a + slices + 1;
                m.AddTriangle(a, b, a + 1);
                m.AddTriangle(b, b + 1, a + 1);
            }
        }

        return m;
    }

    public static MeshData Frustum(double rBottom, double rTop, double h, int slices = 32)
    {
        var m = new MeshData();
        double y0 = -h / 2, y1 = h / 2;
        for (int j = 0; j <= slices; j++)
        {
            double t = 2 * Math.PI * j / slices;
            m.AddVertex(new Point3(rBottom * Math.Cos(t), y0, rBottom * Math.Sin(t)));
            m.AddVertex(new Point3(rTop * Math.Cos(t), y1, rTop * Math.Sin(t)));
        }

        for (int j = 0; j < slices; j++)
        {
            int a = j * 2;
            m.AddTriangle(a, a + 2, a + 1);
            m.AddTriangle(a + 1, a + 2, a + 3);
        }

        int cb = m.AddVertex(new Point3(0, y0, 0));
        int ct = m.AddVertex(new Point3(0, y1, 0));
        for (int j = 0; j < slices; j++)
        {
            m.AddTriangle(cb, j * 2, (j + 1) * 2);
            m.AddTriangle(ct, ((j + 1) * 2) + 1, (j * 2) + 1);
        }

        return m;
    }

    public static MeshData Torus(double ringRadius = 0.45, double tubeRadius = 0.18, int segments = 40, int tubeSegments = 16)
    {
        var m = new MeshData();
        for (int i = 0; i <= segments; i++)
        {
            double u = 2 * Math.PI * i / segments;
            for (int j = 0; j <= tubeSegments; j++)
            {
                double v = 2 * Math.PI * j / tubeSegments;
                m.AddVertex(new Point3(
                    (ringRadius + (tubeRadius * Math.Cos(v))) * Math.Cos(u),
                    tubeRadius * Math.Sin(v),
                    (ringRadius + (tubeRadius * Math.Cos(v))) * Math.Sin(u)));
            }
        }

        for (int i = 0; i < segments; i++)
        {
            for (int j = 0; j < tubeSegments; j++)
            {
                int a = (i * (tubeSegments + 1)) + j, b = a + tubeSegments + 1;
                m.AddTriangle(a, b, a + 1);
                m.AddTriangle(b, b + 1, a + 1);
            }
        }

        return m;
    }
}
