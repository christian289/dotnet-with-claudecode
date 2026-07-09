namespace PolyLab3DStudio.Core;

/// <summary>A 3D point/vector in scene units (Y up, matching the studio's coordinate legend).</summary>
public readonly record struct Point3(double X, double Y, double Z);
