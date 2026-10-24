using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PolygonEditor.Editing;
using PolygonEditor.Geometry;
using PolygonEditor.Model;
using PolygonEditor.Rendering;

namespace PolygonEditor.Views;

public sealed class EditorCanvas : Control
{
    private const double VertexHitRadius = 8;

    private PixelBuffer? _buffer;
    private WriteableBitmap? _bitmap;
    private bool _useBresenham;

    // Vertex currently dragged by the mouse, null when nothing is dragged.
    private HitTester.VertexHit? _draggedVertex;

    public Scene Scene { get; set; } = new();

    public bool UseBresenham
    {
        get => _useBresenham;
        set
        {
            _useBresenham = value;
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        int width = (int)Bounds.Width;
        int height = (int)Bounds.Height;
        if (width <= 0 || height <= 0) return;

        if (UseBresenham)
        {
            if (_buffer is null || _buffer.Width != width || _buffer.Height != height)
            {
                _buffer = new PixelBuffer(width, height);
                _bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96),
                    PixelFormat.Bgra8888, AlphaFormat.Premul);
            }

            _buffer.Clear(0xFFFFFFFF);
            DrawEdges(new BresenhamLineDrawer(_buffer));

            using (var fb = _bitmap!.Lock())
            {
                var pixels = Unsafe.As<int[]>(_buffer.Pixels);
                for (var y = 0; y < height; y++)
                    Marshal.Copy(pixels, y * width, fb.Address + y * fb.RowBytes, width);
            }

            context.DrawImage(_bitmap!, new Rect(0, 0, width, height));
        }
        else
        {
            context.FillRectangle(Brushes.White, new Rect(Bounds.Size));
            DrawEdges(new LibraryLineDrawer(context));
        }

        DrawVertices(context);
    }

    private static Point ToPoint(Vec2 v) => new(v.X, v.Y);

    private Vec2 ToVec2(PointerEventArgs e)
    {
        Point p = e.GetPosition(this);
        return new Vec2(p.X, p.Y);
    }

    private void DrawEdges(ILineDrawer drawer)
    {
        foreach (var polygon in Scene.Polygons)
        {
            foreach (var edge in polygon.Edges)
                drawer.DrawLine(edge.Start.Position, edge.End.Position);
        }
    }

    private void DrawVertices(DrawingContext context)
    {
        foreach (var polygon in Scene.Polygons)
        {
            foreach (var vertex in polygon.Vertices)
                context.DrawEllipse(Brushes.Black, null, ToPoint(vertex.Position), 4, 4);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _draggedVertex = HitTester.FindVertex(Scene, ToVec2(e), VertexHitRadius);

        // Keep receiving move events even when the cursor leaves the control.
        if (_draggedVertex != null)
            e.Pointer.Capture(this);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_draggedVertex == null) return;

        _draggedVertex.Value.Vertex.Position = ToVec2(e);
        InvalidateVisual();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        e.Pointer.Capture(null);
        _draggedVertex = null;
    }
}
