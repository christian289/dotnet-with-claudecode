namespace PolyLab3DStudio.Core;

/// <summary>
/// UI-framework-independent triangle mesh. Vertices are duplicated per
/// triangle so renderers that average shared-vertex normals produce the
/// flat-shaded look that defines low-poly art.
/// </summary>
public sealed record MeshData(IReadOnlyList<Vector3> Positions, IReadOnlyList<int> Indices);
