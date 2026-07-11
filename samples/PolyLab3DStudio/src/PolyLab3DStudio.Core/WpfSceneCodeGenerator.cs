namespace PolyLab3DStudio.Core;

/// <summary>
/// Generates the copy-paste-ready MainWindow.xaml / MainWindow.xaml.cs shown in
/// the XAML export modal, in two flavors:
/// code-behind (scene built in C#, ported verbatim from the design's
/// genXamlCode/genCsCode) and declarative (scene structure, materials,
/// transforms, and lights expressed in XAML; C# keeps only the orbit camera
/// and the procedural mesh factories).
/// </summary>
public static class WpfSceneCodeGenerator
{
    // ==================== runnable project scaffold ====================

    /// <summary>Csproj for the exported project; the TFM comes from the modal's .NET picker.</summary>
    public static string GenerateProjectFile(string targetFramework) =>
        $"""
        <Project Sdk="Microsoft.NET.Sdk">

          <PropertyGroup>
            <OutputType>WinExe</OutputType>
            <TargetFramework>{targetFramework}</TargetFramework>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <UseWPF>true</UseWPF>
          </PropertyGroup>

        </Project>
        """;

    public static string GenerateAppXaml() =>
        """
        <Application x:Class="PolyLabScene.App"
                     xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                     xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                     StartupUri="MainWindow.xaml" />
        """;

    public static string GenerateAppCs() =>
        """
        using System.Windows;

        namespace PolyLabScene
        {
            public partial class App : Application
            {
            }
        }
        """;

    // ==================== code-behind variant ====================

    public static string GenerateXaml() =>
        """
        <Window x:Class="PolyLabScene.MainWindow"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                Title="PolyLab Scene" Width="1100" Height="700">
            <Grid Background="#1B2530"
                  MouseLeftButtonDown="View_MouseDown" MouseLeftButtonUp="View_MouseUp"
                  MouseMove="View_MouseMove" MouseWheel="View_MouseWheel">
                <Viewport3D>
                    <Viewport3D.Camera>
                        <PerspectiveCamera x:Name="Camera" FieldOfView="50"/>
                    </Viewport3D.Camera>
                    <ModelVisual3D x:Name="SceneRoot"/>
                </Viewport3D>
                <TextBlock Text="폴리랩에서 내보낸 장면 · 드래그: 회전 · 휠: 확대/축소"
                           Foreground="#88FFFFFF" FontSize="12" Margin="14"
                           HorizontalAlignment="Left" VerticalAlignment="Bottom"/>
            </Grid>
        </Window>
        """;

    public static string GenerateCs(IReadOnlyList<SceneObjectState> objects, double lightIntensity, double lightAngle)
    {
        string objLines = objects.Count > 0
            ? string.Join('\n', objects.Select(ObjectLine))
            : "            // (장면이 비어 있어요 — 폴리랩에서 도형을 추가한 뒤 다시 내보내 보세요)";

        return
            $$"""
            using System;
            using System.Windows;
            using System.Windows.Input;
            using System.Windows.Media;
            using System.Windows.Media.Media3D;

            namespace PolyLabScene
            {
                public partial class MainWindow : Window
                {
                    private double _theta = 45, _phi = 55, _radius = 10;
                    private Point _last;
                    private bool _dragging;

                    public MainWindow()
                    {
                        InitializeComponent();
                        BuildScene();
                        UpdateCamera();
                    }

                    // ==== 폴리랩에서 내보낸 장면 ====
                    private void BuildScene()
                    {
            {{objLines}}

                        // 조명 (폴리랩 값: 밝기 {{F(lightIntensity)}}, 방향 {{F(lightAngle)}}°)
                        double ang = {{F(lightAngle)}} * Math.PI / 180.0;
                        byte lv = (byte)Math.Min(255.0, {{F(lightIntensity)}} * 200.0);
                        var lightDir = new Vector3D(-Math.Cos(ang) * 7, -8, -Math.Sin(ang) * 7);
                        lightDir.Normalize();
                        SceneRoot.Children.Add(new ModelVisual3D { Content = new DirectionalLight(Color.FromRgb(lv, lv, lv), lightDir) });
                        SceneRoot.Children.Add(new ModelVisual3D { Content = new AmbientLight(Color.FromRgb(72, 78, 86)) });
                    }

                    private void AddObject(MeshGeometry3D mesh, Point3D pos, double ryDeg, double scale, string hex, double roughness, double metalness)
                    {
                        var color = (Color)ColorConverter.ConvertFromString(hex);
                        var mat = new MaterialGroup();
                        mat.Children.Add(new DiffuseMaterial(new SolidColorBrush(color)));
                        if (roughness < 0.9)
                        {
                            byte s = (byte)(metalness > 0.5 ? 230 : 150);
                            mat.Children.Add(new SpecularMaterial(new SolidColorBrush(Color.FromRgb(s, s, s)), 10 + (1 - roughness) * 90));
                        }
                        var model = new GeometryModel3D(mesh, mat) { BackMaterial = mat };
                        var tg = new Transform3DGroup();
                        tg.Children.Add(new ScaleTransform3D(scale, scale, scale));
                        tg.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), ryDeg)));
                        tg.Children.Add(new TranslateTransform3D(pos.X, pos.Y, pos.Z));
                        model.Transform = tg;
                        SceneRoot.Children.Add(new ModelVisual3D { Content = model });
                    }

                    // 포인트 클라우드: 점마다 아주 작은 정팔면체를 만들어 렌더링해요
                    // heightColormap이 켜지면 점의 높이(Y)에 따라 그라데이션 색을 입혀요
                    private void AddPointCloud(Point3DCollection pts, Point3D pos, double ryDeg, double scale, string hex, bool heightColormap)
                    {
                        var color = (Color)ColorConverter.ConvertFromString(hex);
                        var m = PointCloudMeshFactory.FromPoints(pts);
                        var mat = new MaterialGroup();
                        if (heightColormap)
                        {
                            mat.Children.Add(new DiffuseMaterial(PointCloudMeshFactory.HeightBrush(1.0)));
                            mat.Children.Add(new EmissiveMaterial(PointCloudMeshFactory.HeightBrush(0.35)));
                        }
                        else
                        {
                            mat.Children.Add(new DiffuseMaterial(new SolidColorBrush(color)));
                            mat.Children.Add(new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(90, color.R, color.G, color.B))));
                        }
                        var model = new GeometryModel3D(m, mat) { BackMaterial = mat };
                        var tg = new Transform3DGroup();
                        tg.Children.Add(new ScaleTransform3D(scale, scale, scale));
                        tg.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), ryDeg)));
                        tg.Children.Add(new TranslateTransform3D(pos.X, pos.Y, pos.Z));
                        model.Transform = tg;
                        SceneRoot.Children.Add(new ModelVisual3D { Content = model });
                    }

            {{OrbitCameraSection}}
                }

            {{FactoriesSection}}
            }
            """;
    }

    // ==================== declarative (XAML-centric) variant ====================

    public static string GenerateDeclarativeXaml(IReadOnlyList<SceneObjectState> objects, double lightIntensity, double lightAngle)
    {
        string materials = objects.Count > 0
            ? string.Join('\n', objects.Select((o, i) => MaterialResource(o, i + 1)))
            : "        <!-- (장면이 비어 있어요 — 폴리랩에서 도형을 추가한 뒤 다시 내보내 보세요) -->";

        string visuals = objects.Count > 0
            ? string.Join('\n', objects.Select((o, i) => ObjectVisual(o, i + 1)))
            : "";

        double ang = lightAngle * Math.PI / 180.0;
        byte lv = (byte)Math.Min(255.0, lightIntensity * 200.0);
        double dx = -Math.Cos(ang) * 7, dy = -8, dz = -Math.Sin(ang) * 7;
        double len = Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        string lightColor = $"#{lv:X2}{lv:X2}{lv:X2}";
        string lightDir = string.Create(CultureInfo.InvariantCulture, $"{dx / len:0.###},{dy / len:0.###},{dz / len:0.###}");

        return
            $$"""
            <Window x:Class="PolyLabScene.MainWindow"
                    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:local="clr-namespace:PolyLabScene"
                    Title="PolyLab Scene" Width="1100" Height="700">
                <!-- 장면 전체(재질·변형·조명)가 XAML로 선언되어 있어요.
                     메시는 절차적으로 만들어야 해서 {local:SceneMesh ...} 마크업 확장이 공급해요. -->
                <Window.Resources>
            {{materials}}
                </Window.Resources>
                <Grid Background="#1B2530"
                      MouseLeftButtonDown="View_MouseDown" MouseLeftButtonUp="View_MouseUp"
                      MouseMove="View_MouseMove" MouseWheel="View_MouseWheel">
                    <Viewport3D>
                        <Viewport3D.Camera>
                            <PerspectiveCamera x:Name="Camera" FieldOfView="50"/>
                        </Viewport3D.Camera>

                        <!-- 조명 (폴리랩 값: 밝기 {{F(lightIntensity)}}, 방향 {{F(lightAngle)}}°) -->
                        <ModelVisual3D>
                            <ModelVisual3D.Content>
                                <Model3DGroup>
                                    <DirectionalLight Color="{{lightColor}}" Direction="{{lightDir}}"/>
                                    <AmbientLight Color="#484E56"/>
                                </Model3DGroup>
                            </ModelVisual3D.Content>
                        </ModelVisual3D>
            {{visuals}}
                    </Viewport3D>
                    <TextBlock Text="폴리랩에서 내보낸 장면 (XAML 선언형) · 드래그: 회전 · 휠: 확대/축소"
                               Foreground="#88FFFFFF" FontSize="12" Margin="14"
                               HorizontalAlignment="Left" VerticalAlignment="Bottom"/>
                </Grid>
            </Window>
            """;
    }

    public static string GenerateDeclarativeCs() =>
        $$"""
        using System;
        using System.Windows;
        using System.Windows.Input;
        using System.Windows.Markup;
        using System.Windows.Media;
        using System.Windows.Media.Media3D;

        namespace PolyLabScene
        {
            public partial class MainWindow : Window
            {
                private double _theta = 45, _phi = 55, _radius = 10;
                private Point _last;
                private bool _dragging;

                public MainWindow()
                {
                    InitializeComponent();
                    UpdateCamera();
                }

        {{OrbitCameraSection}}
            }

            // ==== XAML의 {local:SceneMesh ...}가 호출하는 절차적 메시 공급자 ====
            public sealed class SceneMeshExtension : MarkupExtension
            {
                public SceneMeshExtension() { Kind = "Cube"; }
                public SceneMeshExtension(string kind) { Kind = kind; }

                public string Kind { get; set; }

                // 포인트 클라우드 점 개수 (0 = 종류 기본값)
                public int Count { get; set; }

                public override object ProvideValue(IServiceProvider serviceProvider)
                {
                    switch (Kind)
                    {
                        case "Sphere": return MeshFactory.Sphere();
                        case "Cylinder": return MeshFactory.Cylinder();
                        case "Cone": return MeshFactory.Cone();
                        case "Torus": return MeshFactory.Torus();
                        case "Board": return MeshFactory.Board();
                        case "CloudSphere": return PointCloudMeshFactory.FromPoints(PointCloudFactory.Sphere(CloudCount(1500)));
                        case "CloudTorus": return PointCloudMeshFactory.FromPoints(PointCloudFactory.Torus(CloudCount(1600)));
                        case "CloudTerrain": return PointCloudMeshFactory.FromPoints(PointCloudFactory.Terrain(TerrainGrid(CloudCount(1849))));
                        default: return MeshFactory.Cube();
                    }
                }

                private int CloudCount(int fallback) { return Count > 0 ? Count : fallback; }

                // (n+1)² 그리드가 요청 개수에 가장 가깝도록 n을 고른다
                private static int TerrainGrid(int count) { return Math.Max(2, (int)Math.Round(Math.Sqrt(count)) - 1); }
            }

        {{FactoriesSection}}
        }
        """;

    // ==================== shared code sections ====================

    private const string OrbitCameraSection =
        """
                // ==== 궤도 카메라 (드래그: 회전 · 휠: 확대/축소) ====
                private void UpdateCamera()
                {
                    double t = _theta * Math.PI / 180.0, p = _phi * Math.PI / 180.0;
                    var target = new Point3D(0, 0.7, 0);
                    var pos = new Point3D(
                        target.X + _radius * Math.Sin(p) * Math.Sin(t),
                        target.Y + _radius * Math.Cos(p),
                        target.Z + _radius * Math.Sin(p) * Math.Cos(t));
                    Camera.Position = pos;
                    Camera.LookDirection = new Vector3D(target.X - pos.X, target.Y - pos.Y, target.Z - pos.Z);
                    Camera.UpDirection = new Vector3D(0, 1, 0);
                }

                private void View_MouseDown(object sender, MouseButtonEventArgs e)
                { _dragging = true; _last = e.GetPosition(this); ((UIElement)sender).CaptureMouse(); }

                private void View_MouseUp(object sender, MouseButtonEventArgs e)
                { _dragging = false; ((UIElement)sender).ReleaseMouseCapture(); }

                private void View_MouseMove(object sender, MouseEventArgs e)
                {
                    if (!_dragging) return;
                    var p = e.GetPosition(this);
                    _theta -= (p.X - _last.X) * 0.4;
                    _phi = Math.Max(8, Math.Min(87, _phi - (p.Y - _last.Y) * 0.4));
                    _last = p;
                    UpdateCamera();
                }

                private void View_MouseWheel(object sender, MouseWheelEventArgs e)
                {
                    _radius = Math.Max(2.5, Math.Min(40, _radius * (e.Delta > 0 ? 0.9 : 1.1)));
                    UpdateCamera();
                }
        """;

    private const string FactoriesSection =
        """
            // ==== 기본 도형 메시 생성기 ====
            public static class MeshFactory
            {
                public static MeshGeometry3D Cube() { return Box(1, 1, 1); }
                public static MeshGeometry3D Board() { return Box(1.6, 0.12, 1.6); }
                public static MeshGeometry3D Cylinder() { return Frustum(0.5, 0.5, 1.1); }
                public static MeshGeometry3D Cone() { return Frustum(0.55, 0.0, 1.1); }

                public static MeshGeometry3D Box(double sx, double sy, double sz)
                {
                    var m = new MeshGeometry3D();
                    double x = sx / 2, y = sy / 2, z = sz / 2;
                    Point3D[] p = {
                        new Point3D(-x,-y,-z), new Point3D(x,-y,-z), new Point3D(x,y,-z), new Point3D(-x,y,-z),
                        new Point3D(-x,-y,z), new Point3D(x,-y,z), new Point3D(x,y,z), new Point3D(-x,y,z)
                    };
                    int[][] faces = { new[]{0,3,2,1}, new[]{4,5,6,7}, new[]{0,4,7,3}, new[]{1,2,6,5}, new[]{3,7,6,2}, new[]{0,1,5,4} };
                    foreach (var fc in faces)
                    {
                        int b = m.Positions.Count;
                        foreach (var idx in fc) m.Positions.Add(p[idx]);
                        m.TriangleIndices.Add(b); m.TriangleIndices.Add(b + 1); m.TriangleIndices.Add(b + 2);
                        m.TriangleIndices.Add(b); m.TriangleIndices.Add(b + 2); m.TriangleIndices.Add(b + 3);
                    }
                    return m;
                }

                public static MeshGeometry3D Sphere(double r = 0.62, int slices = 24, int stacks = 16)
                {
                    var m = new MeshGeometry3D();
                    for (int i = 0; i <= stacks; i++)
                    {
                        double phi = Math.PI * i / stacks;
                        for (int j = 0; j <= slices; j++)
                        {
                            double th = 2 * Math.PI * j / slices;
                            m.Positions.Add(new Point3D(r * Math.Sin(phi) * Math.Cos(th), r * Math.Cos(phi), r * Math.Sin(phi) * Math.Sin(th)));
                        }
                    }
                    for (int i = 0; i < stacks; i++)
                        for (int j = 0; j < slices; j++)
                        {
                            int a = i * (slices + 1) + j, b = a + slices + 1;
                            m.TriangleIndices.Add(a); m.TriangleIndices.Add(b); m.TriangleIndices.Add(a + 1);
                            m.TriangleIndices.Add(b); m.TriangleIndices.Add(b + 1); m.TriangleIndices.Add(a + 1);
                        }
                    return m;
                }

                public static MeshGeometry3D Frustum(double rBottom, double rTop, double h, int slices = 32)
                {
                    var m = new MeshGeometry3D();
                    double y0 = -h / 2, y1 = h / 2;
                    for (int j = 0; j <= slices; j++)
                    {
                        double t = 2 * Math.PI * j / slices;
                        m.Positions.Add(new Point3D(rBottom * Math.Cos(t), y0, rBottom * Math.Sin(t)));
                        m.Positions.Add(new Point3D(rTop * Math.Cos(t), y1, rTop * Math.Sin(t)));
                    }
                    for (int j = 0; j < slices; j++)
                    {
                        int a = j * 2;
                        m.TriangleIndices.Add(a); m.TriangleIndices.Add(a + 2); m.TriangleIndices.Add(a + 1);
                        m.TriangleIndices.Add(a + 1); m.TriangleIndices.Add(a + 2); m.TriangleIndices.Add(a + 3);
                    }
                    int cb = m.Positions.Count; m.Positions.Add(new Point3D(0, y0, 0));
                    int ct = m.Positions.Count; m.Positions.Add(new Point3D(0, y1, 0));
                    for (int j = 0; j < slices; j++)
                    {
                        m.TriangleIndices.Add(cb); m.TriangleIndices.Add(j * 2); m.TriangleIndices.Add((j + 1) * 2);
                        m.TriangleIndices.Add(ct); m.TriangleIndices.Add((j + 1) * 2 + 1); m.TriangleIndices.Add(j * 2 + 1);
                    }
                    return m;
                }

                public static MeshGeometry3D Torus(double R = 0.45, double r = 0.18, int seg = 32, int tube = 16)
                {
                    var m = new MeshGeometry3D();
                    for (int i = 0; i <= seg; i++)
                    {
                        double u = 2 * Math.PI * i / seg;
                        for (int j = 0; j <= tube; j++)
                        {
                            double v = 2 * Math.PI * j / tube;
                            m.Positions.Add(new Point3D((R + r * Math.Cos(v)) * Math.Cos(u), r * Math.Sin(v), (R + r * Math.Cos(v)) * Math.Sin(u)));
                        }
                    }
                    for (int i = 0; i < seg; i++)
                        for (int j = 0; j < tube; j++)
                        {
                            int a = i * (tube + 1) + j, b = a + tube + 1;
                            m.TriangleIndices.Add(a); m.TriangleIndices.Add(b); m.TriangleIndices.Add(a + 1);
                            m.TriangleIndices.Add(b); m.TriangleIndices.Add(b + 1); m.TriangleIndices.Add(a + 1);
                        }
                    return m;
                }
            }

            // ==== 포인트 클라우드 생성기 (스캔 데이터를 흉내 낸 점들) ====
            public static class PointCloudFactory
            {
                public static Point3DCollection Sphere(int n = 1500, int seed = 7)
                {
                    var rnd = new Random(seed);
                    var pts = new Point3DCollection();
                    for (int i = 0; i < n; i++)
                    {
                        double u = rnd.NextDouble() * 2 - 1, t = rnd.NextDouble() * Math.PI * 2;
                        double s = Math.Sqrt(1 - u * u);
                        double r = 0.75 * (1 + (rnd.NextDouble() - 0.5) * 0.06);
                        pts.Add(new Point3D(s * Math.Cos(t) * r, u * r, s * Math.Sin(t) * r));
                    }
                    return pts;
                }

                public static Point3DCollection Torus(int n = 1600, int seed = 13)
                {
                    var rnd = new Random(seed);
                    var pts = new Point3DCollection();
                    for (int i = 0; i < n; i++)
                    {
                        double u = rnd.NextDouble() * Math.PI * 2, v = rnd.NextDouble() * Math.PI * 2;
                        double r = 0.2 * (1 + (rnd.NextDouble() - 0.5) * 0.15);
                        pts.Add(new Point3D((0.55 + r * Math.Cos(v)) * Math.Cos(u), r * Math.Sin(v), (0.55 + r * Math.Cos(v)) * Math.Sin(u)));
                    }
                    return pts;
                }

                public static Point3DCollection Terrain(int n = 42, int seed = 42)
                {
                    var rnd = new Random(seed);
                    var pts = new Point3DCollection();
                    for (int i = 0; i <= n; i++)
                        for (int j = 0; j <= n; j++)
                        {
                            double x = ((double)i / n - 0.5) * 2.0, z = ((double)j / n - 0.5) * 2.0;
                            double y = 0.28 * Math.Sin(x * 2.6) * Math.Cos(z * 2.2) + 0.14 * Math.Sin(x * 5.1 + 1.3) * Math.Sin(z * 4.3) + (rnd.NextDouble() - 0.5) * 0.02;
                            pts.Add(new Point3D(x, y, z));
                        }
                    return pts;
                }
            }

            // ==== 점들을 아주 작은 정팔면체 메시로 변환 ====
            // 각 점의 정규화 높이를 TextureCoordinates.X에 담아 높이 컬러맵을 지원해요
            public static class PointCloudMeshFactory
            {
                public static MeshGeometry3D FromPoints(Point3DCollection pts)
                {
                    var m = new MeshGeometry3D();
                    double s = 0.022; // 점 하나의 크기
                    double minY = double.MaxValue, maxY = double.MinValue;
                    foreach (var p in pts)
                    {
                        minY = Math.Min(minY, p.Y);
                        maxY = Math.Max(maxY, p.Y);
                    }
                    double range = Math.Max(1e-6, maxY - minY);
                    foreach (var p in pts)
                    {
                        int b = m.Positions.Count;
                        m.Positions.Add(new Point3D(p.X - s, p.Y, p.Z));
                        m.Positions.Add(new Point3D(p.X + s, p.Y, p.Z));
                        m.Positions.Add(new Point3D(p.X, p.Y - s, p.Z));
                        m.Positions.Add(new Point3D(p.X, p.Y + s, p.Z));
                        m.Positions.Add(new Point3D(p.X, p.Y, p.Z - s));
                        m.Positions.Add(new Point3D(p.X, p.Y, p.Z + s));
                        double u = (p.Y - minY) / range;
                        for (int k = 0; k < 6; k++) m.TextureCoordinates.Add(new Point(u, 0));
                        int[] idx = { 3, 0, 4, 3, 4, 1, 3, 1, 5, 3, 5, 0, 2, 4, 0, 2, 1, 4, 2, 5, 1, 2, 0, 5 };
                        foreach (var k in idx) m.TriangleIndices.Add(b + k);
                    }
                    return m;
                }

                // 높이 컬러맵 브러시 (낮음 → 높음): 파랑 → 청록 → 초록 → 노랑 → 주황
                public static Brush HeightBrush(double opacity)
                {
                    var b = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0), Opacity = opacity };
                    b.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#4C8DF5"), 0.0));
                    b.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#37A6A0"), 0.25));
                    b.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#7BC96F"), 0.5));
                    b.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FFC24B"), 0.75));
                    b.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F0562F"), 1.0));
                    b.Freeze();
                    return b;
                }
            }
        """;

    // ==================== helpers ====================

    private static string ObjectLine(SceneObjectState o)
    {
        string comment = string.IsNullOrEmpty(o.Name) ? o.Kind.ToString() : o.Name;
        return IsCloud(o.Kind)
            ? $"            AddPointCloud({CloudFactoryCall(o.Kind, o.PointCount)}, new Point3D({F(o.X)}, {F(o.Y)}, {F(o.Z)}), {F(o.RotationY)}, {F(o.Scale)}, \"{o.ColorHex}\", {(o.HeightColormap ? "true" : "false")}); // {comment}"
            : $"            AddObject({MeshFactoryCall(o.Kind)}, new Point3D({F(o.X)}, {F(o.Y)}, {F(o.Z)}), {F(o.RotationY)}, {F(o.Scale)}, \"{o.ColorHex}\", {F(o.Roughness)}, {F(o.Metalness)}); // {comment}";
    }

    private static string MaterialResource(SceneObjectState o, int index)
    {
        string comment = string.IsNullOrEmpty(o.Name) ? o.Kind.ToString() : o.Name;
        if (IsCloud(o.Kind) && o.HeightColormap)
        {
            string stopLines = string.Join('\n', HeightColormap.Stops.Select(s =>
                $"                    <GradientStop Color=\"{s.Hex}\" Offset=\"{F(s.Offset)}\"/>"));
            return $$"""
        <!-- {{comment}} (높이 컬러맵) -->
        <MaterialGroup x:Key="Mat{{index}}">
            <DiffuseMaterial>
                <DiffuseMaterial.Brush>
                    <LinearGradientBrush StartPoint="0,0" EndPoint="1,0">
{{stopLines}}
                    </LinearGradientBrush>
                </DiffuseMaterial.Brush>
            </DiffuseMaterial>
            <EmissiveMaterial>
                <EmissiveMaterial.Brush>
                    <LinearGradientBrush StartPoint="0,0" EndPoint="1,0" Opacity="0.35">
{{stopLines}}
                    </LinearGradientBrush>
                </EmissiveMaterial.Brush>
            </EmissiveMaterial>
        </MaterialGroup>
""";
        }

        if (IsCloud(o.Kind))
        {
            string emissive = $"#5A{o.ColorHex.TrimStart('#')}";
            return $"""
                    <!-- {comment} -->
                    <MaterialGroup x:Key="Mat{index}">
                        <DiffuseMaterial Brush="{o.ColorHex}"/>
                        <EmissiveMaterial Brush="{emissive}"/>
                    </MaterialGroup>
            """;
        }

        if (o.Roughness >= 0.9)
        {
            return $"""
                    <!-- {comment} -->
                    <MaterialGroup x:Key="Mat{index}">
                        <DiffuseMaterial Brush="{o.ColorHex}"/>
                    </MaterialGroup>
            """;
        }

        string specular = o.Metalness > 0.5 ? "#E6E6E6" : "#969696";
        string power = F(10 + ((1 - o.Roughness) * 90));
        return $"""
                <!-- {comment} -->
                <MaterialGroup x:Key="Mat{index}">
                    <DiffuseMaterial Brush="{o.ColorHex}"/>
                    <SpecularMaterial Brush="{specular}" SpecularPower="{power}"/>
                </MaterialGroup>
        """;
    }

    private static string ObjectVisual(SceneObjectState o, int index)
    {
        string comment = string.IsNullOrEmpty(o.Name) ? o.Kind.ToString() : o.Name;
        string meshMarkup = IsCloud(o.Kind)
            ? $"{{local:SceneMesh {o.Kind}, Count={EffectiveCloudCount(o.Kind, o.PointCount)}}}"
            : $"{{local:SceneMesh {o.Kind}}}";
        return $$"""

                        <!-- {{comment}} -->
                        <ModelVisual3D>
                            <ModelVisual3D.Content>
                                <GeometryModel3D Geometry="{{meshMarkup}}"
                                                 Material="{StaticResource Mat{{index}}}"
                                                 BackMaterial="{StaticResource Mat{{index}}}">
                                    <GeometryModel3D.Transform>
                                        <Transform3DGroup>
                                            <ScaleTransform3D ScaleX="{{F(o.Scale)}}" ScaleY="{{F(o.Scale)}}" ScaleZ="{{F(o.Scale)}}"/>
                                            <RotateTransform3D>
                                                <RotateTransform3D.Rotation>
                                                    <AxisAngleRotation3D Axis="0,1,0" Angle="{{F(o.RotationY)}}"/>
                                                </RotateTransform3D.Rotation>
                                            </RotateTransform3D>
                                            <TranslateTransform3D OffsetX="{{F(o.X)}}" OffsetY="{{F(o.Y)}}" OffsetZ="{{F(o.Z)}}"/>
                                        </Transform3DGroup>
                                    </GeometryModel3D.Transform>
                                </GeometryModel3D>
                            </ModelVisual3D.Content>
                        </ModelVisual3D>
            """;
    }

    private static bool IsCloud(ShapeKind kind) =>
        kind is ShapeKind.CloudSphere or ShapeKind.CloudTorus or ShapeKind.CloudTerrain;

    private static string MeshFactoryCall(ShapeKind kind) => kind switch
    {
        ShapeKind.Sphere => "MeshFactory.Sphere()",
        ShapeKind.Cylinder => "MeshFactory.Cylinder()",
        ShapeKind.Cone => "MeshFactory.Cone()",
        ShapeKind.Torus => "MeshFactory.Torus()",
        ShapeKind.Board => "MeshFactory.Board()",
        _ => "MeshFactory.Cube()",
    };

    private static string CloudFactoryCall(ShapeKind kind, int pointCount)
    {
        int n = EffectiveCloudCount(kind, pointCount);
        return kind switch
        {
            ShapeKind.CloudTorus => $"PointCloudFactory.Torus({n})",
            ShapeKind.CloudTerrain => $"PointCloudFactory.Terrain({PointCloudFactory.TerrainGridFromCount(n)})",
            _ => $"PointCloudFactory.Sphere({n})",
        };
    }

    private static int EffectiveCloudCount(ShapeKind kind, int pointCount) =>
        pointCount > 0
            ? Math.Clamp(pointCount, PointCloudFactory.MinCount, PointCloudFactory.MaxCount)
            : PointCloudFactory.DefaultCount(kind);

    private static string F(double v) => (Math.Round(v * 100) / 100).ToString(CultureInfo.InvariantCulture);
}
