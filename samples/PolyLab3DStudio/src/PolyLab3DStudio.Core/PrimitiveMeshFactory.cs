namespace PolyLab3DStudio.Core;

/// <summary>
/// Generates unit-sized low-poly meshes centered at the origin.
/// Segment counts are intentionally low to keep the faceted look.
/// </summary>
public static class PrimitiveMeshFactory
{
    private const int RadialSegments = 10;   // cylinder / cone side count
    private const int TorusMajorSegments = 14;
    private const int TorusMinorSegments = 8;

    public static MeshData Create(PrimitiveKind kind) => kind switch
    {
        PrimitiveKind.Cube => CreateCube(),
        PrimitiveKind.Sphere => CreateIcosphere(),
        PrimitiveKind.Cylinder => CreateCylinder(),
        PrimitiveKind.Cone => CreateCone(),
        PrimitiveKind.Torus => CreateTorus(),
        PrimitiveKind.Plane => CreatePlane(),
        _ => CreateCube(),
    };

    private static MeshData CreateCube()
    {
        MeshBuilder builder = new();
        Vector3[] c =
        [
            new(-0.5f, -0.5f, -0.5f), new(0.5f, -0.5f, -0.5f),
            new(0.5f, 0.5f, -0.5f), new(-0.5f, 0.5f, -0.5f),
            new(-0.5f, -0.5f, 0.5f), new(0.5f, -0.5f, 0.5f),
            new(0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f),
        ];

        builder.AddQuad(c[4], c[5], c[6], c[7]); // front  (+Z)
        builder.AddQuad(c[1], c[0], c[3], c[2]); // back   (-Z)
        builder.AddQuad(c[5], c[1], c[2], c[6]); // right  (+X)
        builder.AddQuad(c[0], c[4], c[7], c[3]); // left   (-X)
        builder.AddQuad(c[7], c[6], c[2], c[3]); // top    (+Y)
        builder.AddQuad(c[0], c[1], c[5], c[4]); // bottom (-Y)

        return builder.Build();
    }

    private static MeshData CreateIcosphere()
    {
        float t = (1f + MathF.Sqrt(5f)) / 2f;

        Vector3[] v =
        [
            new(-1, t, 0), new(1, t, 0), new(-1, -t, 0), new(1, -t, 0),
            new(0, -1, t), new(0, 1, t), new(0, -1, -t), new(0, 1, -t),
            new(t, 0, -1), new(t, 0, 1), new(-t, 0, -1), new(-t, 0, 1),
        ];

        for (int i = 0; i < v.Length; i++)
        {
            v[i] = Vector3.Normalize(v[i]) * 0.5f;
        }

        int[][] faces =
        [
            [0, 11, 5], [0, 5, 1], [0, 1, 7], [0, 7, 10], [0, 10, 11],
            [1, 5, 9], [5, 11, 4], [11, 10, 2], [10, 7, 6], [7, 1, 8],
            [3, 9, 4], [3, 4, 2], [3, 2, 6], [3, 6, 8], [3, 8, 9],
            [4, 9, 5], [2, 4, 11], [6, 2, 10], [8, 6, 7], [9, 8, 1],
        ];

        MeshBuilder builder = new();

        // One subdivision pass: 20 -> 80 faces, still clearly faceted.
        foreach (int[] f in faces)
        {
            Vector3 a = v[f[0]];
            Vector3 b = v[f[1]];
            Vector3 c = v[f[2]];
            Vector3 ab = Vector3.Normalize(a + b) * 0.5f;
            Vector3 bc = Vector3.Normalize(b + c) * 0.5f;
            Vector3 ca = Vector3.Normalize(c + a) * 0.5f;

            builder.AddTriangle(a, ab, ca);
            builder.AddTriangle(b, bc, ab);
            builder.AddTriangle(c, ca, bc);
            builder.AddTriangle(ab, bc, ca);
        }

        return builder.Build();
    }

    private static MeshData CreateCylinder()
    {
        MeshBuilder builder = new();
        Vector3 topCenter = new(0, 0.5f, 0);
        Vector3 bottomCenter = new(0, -0.5f, 0);

        for (int i = 0; i < RadialSegments; i++)
        {
            (Vector3 b0, Vector3 b1) = RimEdge(i, RadialSegments, -0.5f, 0.5f);
            (Vector3 t0, Vector3 t1) = RimEdge(i, RadialSegments, 0.5f, 0.5f);

            builder.AddQuad(b0, b1, t1, t0);              // side
            builder.AddTriangle(topCenter, t0, t1);       // top cap
            builder.AddTriangle(bottomCenter, b1, b0);    // bottom cap
        }

        return builder.Build();
    }

    private static MeshData CreateCone()
    {
        MeshBuilder builder = new();
        Vector3 apex = new(0, 0.5f, 0);
        Vector3 bottomCenter = new(0, -0.5f, 0);

        for (int i = 0; i < RadialSegments; i++)
        {
            (Vector3 b0, Vector3 b1) = RimEdge(i, RadialSegments, -0.5f, 0.5f);

            builder.AddTriangle(apex, b0, b1);            // side
            builder.AddTriangle(bottomCenter, b1, b0);    // bottom cap
        }

        return builder.Build();
    }

    private static MeshData CreateTorus()
    {
        const float majorRadius = 0.35f;
        const float minorRadius = 0.15f;
        MeshBuilder builder = new();

        Vector3 Point(int major, int minor)
        {
            float u = major % TorusMajorSegments * MathF.Tau / TorusMajorSegments;
            float w = minor % TorusMinorSegments * MathF.Tau / TorusMinorSegments;
            float r = majorRadius + minorRadius * MathF.Cos(w);
            return new Vector3(r * MathF.Cos(u), minorRadius * MathF.Sin(w), r * MathF.Sin(u));
        }

        for (int i = 0; i < TorusMajorSegments; i++)
        {
            for (int j = 0; j < TorusMinorSegments; j++)
            {
                builder.AddQuad(Point(i, j), Point(i, j + 1), Point(i + 1, j + 1), Point(i + 1, j));
            }
        }

        return builder.Build();
    }

    private static MeshData CreatePlane()
    {
        MeshBuilder builder = new();
        builder.AddQuad(
            new Vector3(-0.5f, 0, 0.5f), new Vector3(0.5f, 0, 0.5f),
            new Vector3(0.5f, 0, -0.5f), new Vector3(-0.5f, 0, -0.5f));
        builder.AddQuad(
            new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 0, -0.5f),
            new Vector3(0.5f, 0, 0.5f), new Vector3(-0.5f, 0, 0.5f));
        return builder.Build();
    }

    private static (Vector3 P0, Vector3 P1) RimEdge(int segment, int segmentCount, float y, float radius)
    {
        float a0 = segment * MathF.Tau / segmentCount;
        float a1 = (segment + 1) * MathF.Tau / segmentCount;
        return (
            new Vector3(radius * MathF.Cos(a0), y, radius * MathF.Sin(a0)),
            new Vector3(radius * MathF.Cos(a1), y, radius * MathF.Sin(a1)));
    }

    private sealed class MeshBuilder
    {
        private readonly List<Vector3> _positions = [];
        private readonly List<int> _indices = [];

        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            int start = _positions.Count;
            _positions.Add(a);
            _positions.Add(b);
            _positions.Add(c);
            _indices.Add(start);
            _indices.Add(start + 1);
            _indices.Add(start + 2);
        }

        public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            AddTriangle(a, b, c);
            AddTriangle(a, c, d);
        }

        public MeshData Build() => new(_positions.AsReadOnly(), _indices.AsReadOnly());
    }
}
