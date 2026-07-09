namespace PolyLab3DStudio.Core;

/// <summary>Vertex positions plus triangle indices (three per triangle), UI-framework independent.</summary>
public sealed class MeshData
{
    private readonly List<Point3> _positions = [];
    private readonly List<int> _indices = [];

    public IReadOnlyList<Point3> Positions => _positions;

    public IReadOnlyList<int> TriangleIndices => _indices;

    public int AddVertex(Point3 p)
    {
        _positions.Add(p);
        return _positions.Count - 1;
    }

    public void AddTriangle(int a, int b, int c)
    {
        _indices.Add(a);
        _indices.Add(b);
        _indices.Add(c);
    }
}
