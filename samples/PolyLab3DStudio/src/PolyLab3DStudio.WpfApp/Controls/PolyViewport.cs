using PolyLab3DStudio.Core;
using PolyLab3DStudio.ViewModels;

namespace PolyLab3DStudio.Controls;

/// <summary>
/// The studio's 3D viewport: a Viewport3D wrapped with the design's orbit camera
/// (smoothed goal interpolation, auto-spin), grid/axes helpers, selection
/// wireframe, click picking, and move/rotate/scale tool drags. Numeric behavior
/// (speeds, clamps, thresholds) mirrors Viewport.jsx.
/// </summary>
public sealed class PolyViewport : Border
{
    private const double LerpFactor = 0.22;
    private const double AutoSpinStep = 0.0028;
    private const double OrbitSpeed = 0.007;
    private const double PanSpeed = 0.0016;
    private const double MoveClickThresholdPx = 5;

    public static readonly DependencyProperty ObjectsSourceProperty = DependencyProperty.Register(
        nameof(ObjectsSource), typeof(IEnumerable<SceneObjectViewModel>), typeof(PolyViewport),
        new PropertyMetadata(null, (d, _) => ((PolyViewport)d).RebuildObjects()));

    public static readonly DependencyProperty SelectedIdProperty = DependencyProperty.Register(
        nameof(SelectedId), typeof(string), typeof(PolyViewport), new PropertyMetadata(null));

    public static readonly DependencyProperty ToolProperty = DependencyProperty.Register(
        nameof(Tool), typeof(string), typeof(PolyViewport), new PropertyMetadata(ToolCatalog.Select));

    public static readonly DependencyProperty LightIntensityProperty = DependencyProperty.Register(
        nameof(LightIntensity), typeof(double), typeof(PolyViewport),
        new PropertyMetadata(1.1, (d, _) => ((PolyViewport)d).UpdateLight()));

    public static readonly DependencyProperty LightAngleProperty = DependencyProperty.Register(
        nameof(LightAngle), typeof(double), typeof(PolyViewport),
        new PropertyMetadata(35.0, (d, _) => ((PolyViewport)d).UpdateLight()));

    public static readonly DependencyProperty ShowGridProperty = DependencyProperty.Register(
        nameof(ShowGrid), typeof(bool), typeof(PolyViewport),
        new PropertyMetadata(true, (d, _) => ((PolyViewport)d).UpdateGridVisibility()));

    public static readonly DependencyProperty SensitivityProperty = DependencyProperty.Register(
        nameof(Sensitivity), typeof(double), typeof(PolyViewport), new PropertyMetadata(1.0));

    public static readonly DependencyProperty InvertZoomProperty = DependencyProperty.Register(
        nameof(InvertZoom), typeof(bool), typeof(PolyViewport), new PropertyMetadata(false));

    public static readonly DependencyProperty AutoSpinProperty = DependencyProperty.Register(
        nameof(AutoSpin), typeof(bool), typeof(PolyViewport), new PropertyMetadata(false));

    private readonly Viewport3D _viewport = new();
    private readonly PerspectiveCamera _camera = new() { FieldOfView = 50, NearPlaneDistance = 0.1, FarPlaneDistance = 300 };
    private readonly DirectionalLight _sun = new();
    private readonly ModelVisual3D _gridVisual = new();
    private readonly Model3DGroup _gridGroup = new();
    private readonly ModelVisual3D _selectionVisual = new();
    private readonly ScaleTransform3D _selectionScale = new();
    private readonly TranslateTransform3D _selectionTranslate = new();

    private readonly Dictionary<string, ObjectEntry> _entries = [];
    private readonly Dictionary<DependencyObject, string> _visualToId = [];

    private double _theta = Math.PI / 4, _phi = 1.06, _radius = 10;
    private Point3D _target = new(0, 0.7, 0);
    private double _goalTheta = Math.PI / 4, _goalPhi = 1.06, _goalRadius = 10;
    private Point3D _goalTarget = new(0, 0.7, 0);

    private DragState? _drag;
    private long _lastZoomEmit;
    private bool _rendering;
    private INotifyCollectionChanged? _observedCollection;

    /// <summary>Debounced cloud-geometry rebuilds (object id → due tick), see OnFrame.</summary>
    private readonly Dictionary<string, long> _pendingGeometry = [];
    private const int GeometryDebounceMs = 180;

    public PolyViewport()
    {
        Background = new SolidColorBrush(Color.FromRgb(0x1B, 0x25, 0x30));
        ClipToBounds = true;
        Focusable = false;

        _viewport.Camera = _camera;
        _viewport.Children.Add(new ModelVisual3D
        {
            Content = new Model3DGroup
            {
                Children = { _sun, new AmbientLight(Color.FromRgb(72, 78, 86)) },
            },
        });

        BuildGrid();
        _gridVisual.Content = _gridGroup;
        _viewport.Children.Add(_gridVisual);

        BuildSelectionBox();
        _viewport.Children.Add(_selectionVisual);

        Child = _viewport;
        UpdateLight();
        UpdateCamera();

        Loaded += (_, _) => StartRendering();
        Unloaded += (_, _) => StopRendering();

        MouseDown += OnViewportMouseDown;
        MouseMove += OnViewportMouseMove;
        MouseUp += OnViewportMouseUp;
        MouseWheel += OnViewportMouseWheel;
        ContextMenuOpening += (_, e) => e.Handled = true;
    }

    public event Action<string?>? SelectRequested;

    public event Action<TransformCommit>? TransformCommitted;

    public event Action<string>? ActionPerformed;

    public IEnumerable<SceneObjectViewModel>? ObjectsSource
    {
        get => (IEnumerable<SceneObjectViewModel>?)GetValue(ObjectsSourceProperty);
        set => SetValue(ObjectsSourceProperty, value);
    }

    public string? SelectedId
    {
        get => (string?)GetValue(SelectedIdProperty);
        set => SetValue(SelectedIdProperty, value);
    }

    public string Tool
    {
        get => (string)GetValue(ToolProperty);
        set => SetValue(ToolProperty, value);
    }

    public double LightIntensity
    {
        get => (double)GetValue(LightIntensityProperty);
        set => SetValue(LightIntensityProperty, value);
    }

    public double LightAngle
    {
        get => (double)GetValue(LightAngleProperty);
        set => SetValue(LightAngleProperty, value);
    }

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public double Sensitivity
    {
        get => (double)GetValue(SensitivityProperty);
        set => SetValue(SensitivityProperty, value);
    }

    public bool InvertZoom
    {
        get => (bool)GetValue(InvertZoomProperty);
        set => SetValue(InvertZoomProperty, value);
    }

    public bool AutoSpin
    {
        get => (bool)GetValue(AutoSpinProperty);
        set => SetValue(AutoSpinProperty, value);
    }

    /// <summary>Camera presets, matching the design's setView.</summary>
    public void SetView(string name)
    {
        switch (name)
        {
            case "front":
                _goalTheta = 0;
                _goalPhi = 1.35;
                break;
            case "top":
                _goalTheta = 0;
                _goalPhi = 0.14;
                break;
            case "side":
                _goalTheta = Math.PI / 2;
                _goalPhi = 1.35;
                break;
            default:
                _goalTheta = Math.PI / 4;
                _goalPhi = 1.06;
                _goalRadius = 10;
                _goalTarget = new Point3D(0, 0.7, 0);
                break;
        }
    }

    // ---------------- frame loop ----------------

    private void StartRendering()
    {
        if (!_rendering)
        {
            _rendering = true;
            CompositionTarget.Rendering += OnFrame;
        }
    }

    private void StopRendering()
    {
        if (_rendering)
        {
            _rendering = false;
            CompositionTarget.Rendering -= OnFrame;
        }
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        if (AutoSpin && _drag is null)
        {
            _goalTheta += AutoSpinStep;
        }

        _theta += (_goalTheta - _theta) * LerpFactor;
        _phi += (_goalPhi - _phi) * LerpFactor;
        _radius += (_goalRadius - _radius) * LerpFactor;
        _target = new Point3D(
            _target.X + ((_goalTarget.X - _target.X) * LerpFactor),
            _target.Y + ((_goalTarget.Y - _target.Y) * LerpFactor),
            _target.Z + ((_goalTarget.Z - _target.Z) * LerpFactor));

        UpdateCamera();
        UpdateSelectionBox();
        ProcessPendingGeometry();
    }

    /// <summary>Rebuilds cloud geometry after the point-count slider settles (100k rebuilds are heavy).</summary>
    private void ProcessPendingGeometry()
    {
        if (_pendingGeometry.Count == 0)
        {
            return;
        }

        long now = Environment.TickCount64;
        foreach ((string id, long due) in _pendingGeometry.ToList())
        {
            if (now < due)
            {
                continue;
            }

            _pendingGeometry.Remove(id);
            if (_entries.TryGetValue(id, out ObjectEntry? entry))
            {
                entry.Model.Geometry = MeshConverter.ForKind(entry.Vm.Kind, entry.Vm.PointCount);
            }
        }
    }

    private void UpdateCamera()
    {
        var pos = new Point3D(
            _target.X + (_radius * Math.Sin(_phi) * Math.Sin(_theta)),
            _target.Y + (_radius * Math.Cos(_phi)),
            _target.Z + (_radius * Math.Sin(_phi) * Math.Cos(_theta)));
        _camera.Position = pos;
        _camera.LookDirection = new Vector3D(_target.X - pos.X, _target.Y - pos.Y, _target.Z - pos.Z);
        _camera.UpDirection = new Vector3D(0, 1, 0);
    }

    private void UpdateLight()
    {
        double ang = LightAngle * Math.PI / 180.0;
        var dir = new Vector3D(-Math.Cos(ang) * 7, -8, -Math.Sin(ang) * 7);
        dir.Normalize();
        byte lv = (byte)Math.Min(255.0, LightIntensity * 200.0);
        _sun.Direction = dir;
        _sun.Color = Color.FromRgb(lv, lv, lv);
    }

    // ---------------- scene helpers (grid, axes, selection box) ----------------

    private void BuildGrid()
    {
        // THREE.GridHelper(20, 20, 0x51677E, 0x31435A): center lines lighter.
        var regular = new MeshGeometry3D();
        var center = new MeshGeometry3D();
        const double t = 0.012, y = 0.001, half = 10;
        for (int i = -10; i <= 10; i++)
        {
            MeshGeometry3D mesh = i == 0 ? center : regular;
            MeshConverter.AddBox(mesh, new Point3D(i, y, 0), t, t, half * 2);
            MeshConverter.AddBox(mesh, new Point3D(0, y, i), half * 2, t, t);
        }

        regular.Freeze();
        center.Freeze();
        _gridGroup.Children.Add(new GeometryModel3D(regular, Unlit(Color.FromRgb(0x31, 0x43, 0x5A))));
        _gridGroup.Children.Add(new GeometryModel3D(center, Unlit(Color.FromRgb(0x51, 0x67, 0x7E))));

        // AxesHelper(2.4) with the legend colors (X red / Y green / Z blue).
        var axes = new Model3DGroup();
        const double axisT = 0.02, axisY = 0.003, len = 2.4;
        var xMesh = new MeshGeometry3D();
        MeshConverter.AddBox(xMesh, new Point3D(len / 2, axisY, 0), len, axisT, axisT);
        var yMesh = new MeshGeometry3D();
        MeshConverter.AddBox(yMesh, new Point3D(0, (len / 2) + axisY, 0), axisT, len, axisT);
        var zMesh = new MeshGeometry3D();
        MeshConverter.AddBox(zMesh, new Point3D(0, axisY, len / 2), axisT, axisT, len);
        axes.Children.Add(new GeometryModel3D(xMesh, Unlit(Color.FromRgb(0xE2, 0x57, 0x4C))));
        axes.Children.Add(new GeometryModel3D(yMesh, Unlit(Color.FromRgb(0x4E, 0x9A, 0x51))));
        axes.Children.Add(new GeometryModel3D(zMesh, Unlit(Color.FromRgb(0x4C, 0x8D, 0xF5))));
        axes.Freeze();
        _viewport.Children.Add(new ModelVisual3D { Content = axes });
    }

    private void UpdateGridVisibility() => _gridVisual.Content = ShowGrid ? _gridGroup : null;

    private void BuildSelectionBox()
    {
        // Unit-cube wireframe (12 edges), scaled to the selected object's bounds
        // each frame — the equivalent of THREE.BoxHelper in #FFC24B.
        var mesh = new MeshGeometry3D();
        const double t = 0.02;
        double[] signs = [-0.5, 0.5];
        foreach (double a in signs)
        {
            foreach (double b in signs)
            {
                MeshConverter.AddBox(mesh, new Point3D(0, a, b), 1 + t, t, t);
                MeshConverter.AddBox(mesh, new Point3D(a, 0, b), t, 1 + t, t);
                MeshConverter.AddBox(mesh, new Point3D(a, b, 0), t, t, 1 + t);
            }
        }

        mesh.Freeze();
        var transform = new Transform3DGroup();
        transform.Children.Add(_selectionScale);
        transform.Children.Add(_selectionTranslate);
        _selectionVisual.Content = new GeometryModel3D(mesh, Unlit(Color.FromRgb(0xFF, 0xC2, 0x4B))) { Transform = transform };
    }

    private void UpdateSelectionBox()
    {
        if (SelectedId is { } id && _entries.TryGetValue(id, out ObjectEntry? entry))
        {
            Rect3D b = entry.Model.Bounds;
            if (!b.IsEmpty)
            {
                _selectionScale.ScaleX = b.SizeX;
                _selectionScale.ScaleY = b.SizeY;
                _selectionScale.ScaleZ = b.SizeZ;
                _selectionTranslate.OffsetX = b.X + (b.SizeX / 2);
                _selectionTranslate.OffsetY = b.Y + (b.SizeY / 2);
                _selectionTranslate.OffsetZ = b.Z + (b.SizeZ / 2);
                SetSelectionVisible(true);
                return;
            }
        }

        SetSelectionVisible(false);
    }

    private void SetSelectionVisible(bool visible)
    {
        bool contained = _viewport.Children.Contains(_selectionVisual);
        if (visible && !contained)
        {
            _viewport.Children.Add(_selectionVisual);
        }
        else if (!visible && contained)
        {
            _viewport.Children.Remove(_selectionVisual);
        }
    }

    private static Brush HeightGradientBrush(double opacity)
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 0),
            Opacity = opacity,
        };
        foreach ((double offset, string hex) in Core.HeightColormap.Stops)
        {
            brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(hex), offset));
        }

        brush.Freeze();
        return brush;
    }

    private static Material Unlit(Color color)
    {
        var group = new MaterialGroup();
        group.Children.Add(new DiffuseMaterial(Brushes.Black));
        group.Children.Add(new EmissiveMaterial(new SolidColorBrush(color)));
        group.Freeze();
        return group;
    }

    // ---------------- object sync ----------------

    private void RebuildObjects()
    {
        if (_observedCollection is not null)
        {
            _observedCollection.CollectionChanged -= OnCollectionChanged;
            _observedCollection = null;
        }

        foreach (ObjectEntry entry in _entries.Values)
        {
            DetachEntry(entry);
        }

        _entries.Clear();
        _visualToId.Clear();

        if (ObjectsSource is null)
        {
            return;
        }

        foreach (SceneObjectViewModel vm in ObjectsSource)
        {
            AddEntry(vm);
        }

        if (ObjectsSource is INotifyCollectionChanged incc)
        {
            _observedCollection = incc;
            incc.CollectionChanged += OnCollectionChanged;
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            RebuildObjects();
            return;
        }

        if (e.OldItems is not null)
        {
            foreach (SceneObjectViewModel vm in e.OldItems.OfType<SceneObjectViewModel>())
            {
                if (_entries.Remove(vm.Id, out ObjectEntry? entry))
                {
                    DetachEntry(entry);
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (SceneObjectViewModel vm in e.NewItems.OfType<SceneObjectViewModel>())
            {
                AddEntry(vm);
            }
        }
    }

    private void AddEntry(SceneObjectViewModel vm)
    {
        var scale = new ScaleTransform3D(vm.Scale, vm.Scale, vm.Scale);
        var rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), vm.RotationY);
        var translate = new TranslateTransform3D(vm.X, vm.Y, vm.Z);
        var transform = new Transform3DGroup();
        transform.Children.Add(scale);
        transform.Children.Add(new RotateTransform3D(rotation));
        transform.Children.Add(translate);

        Material material = BuildMaterial(vm);
        var model = new GeometryModel3D(MeshConverter.ForKind(vm.Kind, vm.PointCount), material)
        {
            BackMaterial = material,
            Transform = transform,
        };
        var visual = new ModelVisual3D { Content = model };

        var entry = new ObjectEntry(vm, visual, model, scale, rotation, translate);
        _entries[vm.Id] = entry;
        _visualToId[visual] = vm.Id;
        _viewport.Children.Add(visual);
        vm.PropertyChanged += OnObjectPropertyChanged;
    }

    private void DetachEntry(ObjectEntry entry)
    {
        entry.Vm.PropertyChanged -= OnObjectPropertyChanged;
        _viewport.Children.Remove(entry.Visual);
        _visualToId.Remove(entry.Visual);
    }

    private void OnObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not SceneObjectViewModel vm || !_entries.TryGetValue(vm.Id, out ObjectEntry? entry))
        {
            return;
        }

        switch (e.PropertyName)
        {
            case nameof(SceneObjectViewModel.X):
                entry.Translate.OffsetX = vm.X;
                break;
            case nameof(SceneObjectViewModel.Y):
                entry.Translate.OffsetY = vm.Y;
                break;
            case nameof(SceneObjectViewModel.Z):
                entry.Translate.OffsetZ = vm.Z;
                break;
            case nameof(SceneObjectViewModel.RotationY):
                entry.Rotation.Angle = vm.RotationY;
                break;
            case nameof(SceneObjectViewModel.Scale):
                entry.Scale.ScaleX = vm.Scale;
                entry.Scale.ScaleY = vm.Scale;
                entry.Scale.ScaleZ = vm.Scale;
                break;
            case nameof(SceneObjectViewModel.PointCount):
                _pendingGeometry[vm.Id] = Environment.TickCount64 + GeometryDebounceMs;
                break;
            case nameof(SceneObjectViewModel.ColorHex):
            case nameof(SceneObjectViewModel.Roughness):
            case nameof(SceneObjectViewModel.Metalness):
            case nameof(SceneObjectViewModel.HeightColormap):
                Material material = BuildMaterial(vm);
                entry.Model.Material = material;
                entry.Model.BackMaterial = material;
                break;
        }
    }

    /// <summary>Material rules identical to the exported WPF code (and Emissive for clouds).</summary>
    private static Material BuildMaterial(SceneObjectViewModel vm)
    {
        if (vm.IsCloud && vm.HeightColormap)
        {
            // Height colormap: the cloud mesh carries normalized heights in
            // TextureCoordinates.X, painted by a horizontal gradient.
            var colormap = new MaterialGroup();
            colormap.Children.Add(new DiffuseMaterial(HeightGradientBrush(1.0)));
            colormap.Children.Add(new EmissiveMaterial(HeightGradientBrush(0.35)));
            colormap.Freeze();
            return colormap;
        }

        var color = (Color)ColorConverter.ConvertFromString(vm.ColorHex);
        var group = new MaterialGroup();
        group.Children.Add(new DiffuseMaterial(new SolidColorBrush(color)));

        if (vm.IsCloud)
        {
            group.Children.Add(new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(90, color.R, color.G, color.B))));
        }
        else if (vm.Roughness < 0.9)
        {
            byte s = (byte)(vm.Metalness > 0.5 ? 230 : 150);
            group.Children.Add(new SpecularMaterial(
                new SolidColorBrush(Color.FromRgb(s, s, s)),
                10 + ((1 - vm.Roughness) * 90)));
        }

        group.Freeze();
        return group;
    }

    // ---------------- input ----------------

    private void OnViewportMouseDown(object sender, MouseButtonEventArgs e)
    {
        Point pos = e.GetPosition(this);
        var drag = new DragState { Start = pos, Last = pos };

        if (e.ChangedButton == MouseButton.Left)
        {
            string? hitId = Pick(pos);
            if (hitId is not null && Tool != ToolCatalog.Select && SelectedId == hitId
                && _entries.TryGetValue(hitId, out ObjectEntry? entry))
            {
                drag.Mode = Tool;
                drag.Entry = entry;
                drag.StartRotation = entry.Rotation.Angle;
                drag.StartScale = entry.Scale.ScaleX;
                drag.PlaneY = entry.Translate.OffsetY;
                if (PlanePoint(pos, drag.PlaneY) is { } grabPoint)
                {
                    drag.Grab = new Vector3D(
                        grabPoint.X - entry.Translate.OffsetX,
                        0,
                        grabPoint.Z - entry.Translate.OffsetZ);
                }
            }
            else
            {
                drag.Mode = "orbit";
            }
        }
        else if (e.ChangedButton is MouseButton.Right or MouseButton.Middle)
        {
            drag.Mode = "pan";
            e.Handled = true;
        }
        else
        {
            return;
        }

        _drag = drag;
        CaptureMouse();
    }

    private void OnViewportMouseMove(object sender, MouseEventArgs e)
    {
        if (_drag is not { } drag)
        {
            return;
        }

        Point pos = e.GetPosition(this);
        double dx = pos.X - drag.Last.X;
        double dy = pos.Y - drag.Last.Y;
        drag.Last = pos;
        double totX = pos.X - drag.Start.X;
        double totY = pos.Y - drag.Start.Y;
        if (Math.Abs(totX) + Math.Abs(totY) > MoveClickThresholdPx)
        {
            drag.Moved = true;
        }

        if (!drag.Moved)
        {
            return;
        }

        double sens = Sensitivity;
        switch (drag.Mode)
        {
            case "orbit":
                _goalTheta -= dx * OrbitSpeed * sens;
                _goalPhi = Math.Min(1.52, Math.Max(0.12, _goalPhi - (dy * OrbitSpeed * sens)));
                break;

            case "pan":
            {
                Vector3D forward = _camera.LookDirection;
                forward.Normalize();
                Vector3D right = Vector3D.CrossProduct(forward, new Vector3D(0, 1, 0));
                right.Normalize();
                Vector3D up = Vector3D.CrossProduct(right, forward);
                _goalTarget += (right * (-dx * PanSpeed * _goalRadius)) + (up * (dy * PanSpeed * _goalRadius));
                break;
            }

            case "move" when drag.Entry is { } entry:
                drag.Did = true;
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    drag.UsedShift = true;
                    entry.Translate.OffsetY = Math.Max(0.06, entry.Translate.OffsetY - (dy * 0.02));
                    drag.PlaneY = entry.Translate.OffsetY;
                }
                else if (PlanePoint(pos, drag.PlaneY) is { } pt)
                {
                    entry.Translate.OffsetX = pt.X - drag.Grab.X;
                    entry.Translate.OffsetZ = pt.Z - drag.Grab.Z;
                }

                break;

            case "rotate" when drag.Entry is { } rotEntry:
                drag.Did = true;
                rotEntry.Rotation.Angle = drag.StartRotation + (totX * 0.02 * 180 / Math.PI);
                break;

            case "scale" when drag.Entry is { } scaleEntry:
            {
                drag.Did = true;
                double s = Math.Min(5, Math.Max(0.15, drag.StartScale * (1 - (totY * 0.006))));
                scaleEntry.Scale.ScaleX = s;
                scaleEntry.Scale.ScaleY = s;
                scaleEntry.Scale.ScaleZ = s;
                break;
            }
        }
    }

    private void OnViewportMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_drag is not { } drag)
        {
            return;
        }

        _drag = null;
        ReleaseMouseCapture();

        if (!drag.Moved)
        {
            if (drag.Mode != "pan")
            {
                SelectRequested?.Invoke(Pick(e.GetPosition(this)));
            }

            return;
        }

        switch (drag.Mode)
        {
            case "orbit":
                ActionPerformed?.Invoke("orbit");
                return;
            case "pan":
                ActionPerformed?.Invoke("pan");
                return;
        }

        if (drag.Did && drag.Entry is { } entry)
        {
            TransformCommitted?.Invoke(new TransformCommit(
                entry.Vm.Id,
                Round2(entry.Translate.OffsetX),
                Round2(entry.Translate.OffsetY),
                Round2(entry.Translate.OffsetZ),
                Math.Round(entry.Rotation.Angle),
                Round2(entry.Scale.ScaleX),
                drag.Mode!,
                drag.UsedShift));
        }
    }

    private void OnViewportMouseWheel(object sender, MouseWheelEventArgs e)
    {
        int dir = (e.Delta > 0 ? -1 : 1) * (InvertZoom ? -1 : 1);
        _goalRadius = Math.Min(40, Math.Max(2.5, _goalRadius * (1 + (dir * 0.09))));
        e.Handled = true;

        long now = Environment.TickCount64;
        if (now - _lastZoomEmit > 900)
        {
            _lastZoomEmit = now;
            ActionPerformed?.Invoke("zoom");
        }
    }

    private string? Pick(Point pos)
    {
        string? found = null;
        VisualTreeHelper.HitTest(
            _viewport,
            null,
            result =>
            {
                if (result is RayMeshGeometry3DHitTestResult mesh
                    && _visualToId.TryGetValue(mesh.VisualHit, out string? id))
                {
                    found = id;
                    return HitTestResultBehavior.Stop;
                }

                return HitTestResultBehavior.Continue;
            },
            new PointHitTestParameters(pos));
        return found;
    }

    /// <summary>Intersects the mouse ray with the horizontal plane y = planeY.</summary>
    private Point3D? PlanePoint(Point pos, double planeY)
    {
        double w = ActualWidth, h = ActualHeight;
        if (w <= 0 || h <= 0)
        {
            return null;
        }

        Vector3D forward = _camera.LookDirection;
        forward.Normalize();
        Vector3D right = Vector3D.CrossProduct(forward, new Vector3D(0, 1, 0));
        right.Normalize();
        Vector3D up = Vector3D.CrossProduct(right, forward);

        // WPF's PerspectiveCamera.FieldOfView is the horizontal angle in degrees.
        double tanH = Math.Tan(_camera.FieldOfView * Math.PI / 360.0);
        double u = ((pos.X / w) * 2) - 1;
        double v = ((pos.Y / h) * 2) - 1;
        Vector3D dir = forward + (right * (u * tanH)) + (up * (-v * tanH * h / w));
        dir.Normalize();

        Point3D origin = _camera.Position;
        if (Math.Abs(dir.Y) < 1e-9)
        {
            return null;
        }

        double t = (planeY - origin.Y) / dir.Y;
        return t <= 0 ? null : origin + (dir * t);
    }

    private static double Round2(double v) => Math.Round(v * 100) / 100;

    private sealed class DragState
    {
        public string? Mode;
        public Point Start;
        public Point Last;
        public bool Moved;
        public bool Did;
        public bool UsedShift;
        public ObjectEntry? Entry;
        public double StartRotation;
        public double StartScale;
        public double PlaneY;
        public Vector3D Grab;
    }

    private sealed record ObjectEntry(
        SceneObjectViewModel Vm,
        ModelVisual3D Visual,
        GeometryModel3D Model,
        ScaleTransform3D Scale,
        AxisAngleRotation3D Rotation,
        TranslateTransform3D Translate);
}
