namespace PolyLab3DStudio.ViewModels;

/// <summary>
/// One comparison-table row of the ".NET 3D 도구 지도" screen. The five value
/// columns follow the design's fixed tool order: 순정 Media3D /
/// HelixToolkit.Wpf / Helix.SharpDX / Vortice·Silk.NET / Stride.
/// </summary>
public sealed record ToolMapRow(string Label, string A, string B, string C, string D, string E);
