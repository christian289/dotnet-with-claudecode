namespace DrawingContextSample.Controls;

/// <summary>
/// StreamGeometry를 사용하여 삼각형을 고속으로 렌더링하는 캔버스
/// Canvas that renders triangles at high speed using StreamGeometry
/// </summary>
public sealed class TriangleCanvas : FrameworkElement
{
    private readonly Random _random = new();
    private readonly List<TriangleData> _triangles = [];
    private readonly Pen _pen = new(Brushes.Black, 1);

    public TriangleCanvas()
    {
        // Pen을 Freeze하여 성능 최적화
        // Freeze Pen for performance optimization
        _pen.Freeze();
    }

    /// <summary>
    /// 대량의 삼각형을 렌더링하고 소요 시간을 반환합니다.
    /// Renders large number of triangles and returns elapsed time.
    /// </summary>
    public async Task<TimeSpan> DrawTrianglesAsync(int count)
    {
        // 기존 데이터 제거
        // Clear existing data
        _triangles.Clear();

        double width = ActualWidth > 0 ? ActualWidth : 400;
        double height = ActualHeight > 0 ? ActualHeight : 400;

        // 데이터 생성 (측정 전)
        // Generate data before measurement
        for (int i = 0; i < count; i++)
        {
            _triangles.Add(CreateRandomTriangleData(width, height));

            // UI Hang 방지를 위해 주기적으로 양보
            // Yield periodically to prevent UI hang
            if (i % 100 == 0)
            {
                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);
            }
        }

        // 렌더링 시간만 측정
        // Measure only rendering time
        var stopwatch = Stopwatch.StartNew();
        InvalidateVisual();
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
        stopwatch.Stop();

        return stopwatch.Elapsed;
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        foreach (var triangle in _triangles)
        {
            // StreamGeometry를 사용한 경량 기하학 생성
            // Create lightweight geometry using StreamGeometry
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(triangle.Point1, isFilled: true, isClosed: true);
                ctx.LineTo(triangle.Point2, isStroked: true, isSmoothJoin: false);
                ctx.LineTo(triangle.Point3, isStroked: true, isSmoothJoin: false);
            }
            geometry.Freeze();

            dc.DrawGeometry(triangle.Fill, _pen, geometry);
        }
    }

    private TriangleData CreateRandomTriangleData(double width, double height)
    {
        double x = _random.NextDouble() * (width - 40);
        double y = _random.NextDouble() * (height - 40);
        double size = 20 + _random.NextDouble() * 20;

        var brush = new SolidColorBrush(Color.FromRgb(
            (byte)_random.Next(256),
            (byte)_random.Next(256),
            (byte)_random.Next(256)));
        brush.Freeze();

        return new TriangleData(
            new Point(x + size / 2, y),
            new Point(x, y + size),
            new Point(x + size, y + size),
            brush);
    }

    public void Clear()
    {
        _triangles.Clear();
        InvalidateVisual();
    }

    // 삼각형 데이터 구조체 (가벼운 값 타입)
    // Triangle data struct (lightweight value type)
    private readonly record struct TriangleData(Point Point1, Point Point2, Point Point3, Brush Fill);
}
