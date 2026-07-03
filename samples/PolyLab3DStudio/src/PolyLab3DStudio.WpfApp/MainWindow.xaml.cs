using System.Windows.Input;
using PolyLab3DStudio.Services;

namespace PolyLab3DStudio;

public partial class MainWindow : Window
{
    private const double OrbitSensitivity = 0.4;   // degrees per pixel
    private const double MinPitch = -85;
    private const double MaxPitch = 85;
    private const double MinDistance = 2;
    private const double MaxDistance = 18;
    private const double ClickDragThreshold = 4;   // pixels before a click becomes an orbit drag

    private readonly SceneRenderer _renderer;

    private double _yaw = 40;      // horizontal orbit angle in degrees
    private double _pitch = 22;    // vertical orbit angle in degrees
    private double _distance = 6;
    private Point _lastMouse;
    private Point _pressMouse;
    private bool _isOrbiting;
    private bool _dragMoved;

    public MainWindow(MainViewModel viewModel, SceneRenderer renderer)
    {
        InitializeComponent();

        _renderer = renderer;
        DataContext = viewModel;
        renderer.Attach(Viewport, viewModel);
        UpdateCamera();

        ViewportSurface.MouseLeftButtonDown += OnViewportMouseDown;
        ViewportSurface.MouseLeftButtonUp += OnViewportMouseUp;
        ViewportSurface.MouseMove += OnViewportMouseMove;
        ViewportSurface.MouseWheel += OnViewportMouseWheel;
    }

    private void OnViewportMouseDown(object sender, MouseButtonEventArgs e)
    {
        _isOrbiting = true;
        _dragMoved = false;
        _pressMouse = e.GetPosition(ViewportSurface);
        _lastMouse = _pressMouse;
        ViewportSurface.CaptureMouse();
    }

    private void OnViewportMouseUp(object sender, MouseButtonEventArgs e)
    {
        ViewportSurface.ReleaseMouseCapture();
        _isOrbiting = false;

        if (!_dragMoved)
        {
            SelectObjectAt(e.GetPosition(Viewport));
        }
    }

    private void OnViewportMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isOrbiting)
        {
            return;
        }

        Point current = e.GetPosition(ViewportSurface);

        if (!_dragMoved &&
            Math.Abs(current.X - _pressMouse.X) < ClickDragThreshold &&
            Math.Abs(current.Y - _pressMouse.Y) < ClickDragThreshold)
        {
            return;
        }

        _dragMoved = true;
        _yaw += (current.X - _lastMouse.X) * OrbitSensitivity;
        _pitch = Math.Clamp(_pitch + (current.Y - _lastMouse.Y) * OrbitSensitivity, MinPitch, MaxPitch);
        _lastMouse = current;
        UpdateCamera();
    }

    private void OnViewportMouseWheel(object sender, MouseWheelEventArgs e)
    {
        double zoomFactor = e.Delta > 0 ? 0.9 : 1.1;
        _distance = Math.Clamp(_distance * zoomFactor, MinDistance, MaxDistance);
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        double yawRad = _yaw * Math.PI / 180;
        double pitchRad = _pitch * Math.PI / 180;

        double x = _distance * Math.Cos(pitchRad) * Math.Sin(yawRad);
        double y = _distance * Math.Sin(pitchRad);
        double z = _distance * Math.Cos(pitchRad) * Math.Cos(yawRad);

        var position = new Point3D(x, y, z);
        Camera.Position = position;
        Camera.LookDirection = new Vector3D(-position.X, -position.Y, -position.Z);
    }

    private void SelectObjectAt(Point point)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        HitTestResult result = VisualTreeHelper.HitTest(Viewport, point);
        if (result is not RayMeshGeometry3DHitTestResult meshHit ||
            meshHit.VisualHit is not ModelVisual3D visual)
        {
            return;
        }

        SceneObjectViewModel? target = _renderer.FindObject(visual);
        if (target is not null)
        {
            viewModel.SelectedObject = target;
        }
    }
}
