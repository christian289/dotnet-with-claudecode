namespace DrawingContextSample.Controls;

/// <summary>
/// DrawingContext를 사용하여 대량의 도형을 고속으로 렌더링하는 캔버스
/// Canvas that renders large number of shapes at high speed using DrawingContext
/// </summary>
public sealed class HighPerformanceCanvas : FrameworkElement
{
    private readonly Random _random = new();
    private readonly List<ShapeData> _shapes = [];
    private readonly Pen _pen = new(Brushes.Black, 1);

    public HighPerformanceCanvas()
    {
        // Pen을 Freeze하여 성능 최적화
        // Freeze Pen for performance optimization
        _pen.Freeze();
    }

    /// <summary>
    /// 대량의 도형을 렌더링하고 소요 시간을 반환합니다.
    /// Renders large number of shapes and returns elapsed time.
    /// </summary>
    public async Task<TimeSpan> DrawShapesAsync(int count)
    {
        // 기존 데이터 제거
        // Clear existing data
        _shapes.Clear();

        double width = ActualWidth > 0 ? ActualWidth : 400;
        double height = ActualHeight > 0 ? ActualHeight : 400;

        // 데이터 생성 (측정 전)
        // Generate data before measurement
        for (int i = 0; i < count; i++)
        {
            _shapes.Add(CreateRandomShapeData(width, height));

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

        foreach (var shape in _shapes)
        {
            dc.DrawRectangle(shape.Fill, _pen, shape.Bounds);
        }
    }

    private ShapeData CreateRandomShapeData(double width, double height)
    {
        double x = _random.NextDouble() * (width - 30);
        double y = _random.NextDouble() * (height - 30);
        double size = 10 + _random.NextDouble() * 20;

        var brush = new SolidColorBrush(Color.FromRgb(
            (byte)_random.Next(256),
            (byte)_random.Next(256),
            (byte)_random.Next(256)));
        brush.Freeze();

        return new ShapeData(new Rect(x, y, size, size), brush);
    }

    public void Clear()
    {
        _shapes.Clear();
        InvalidateVisual();
    }

    // 도형 데이터 구조체 (가벼운 값 타입)
    // Shape data struct (lightweight value type)
    private readonly record struct ShapeData(Rect Bounds, Brush Fill);
}
