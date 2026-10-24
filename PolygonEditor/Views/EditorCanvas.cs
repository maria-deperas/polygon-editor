using System.Linq;
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
    private const double EdgeHitTolerance = 5;

    private PixelBuffer? _buffer;
    private WriteableBitmap? _bitmap;
    private bool _useBresenham;
    private Polygon? _draggedPolygon;
    private Vec2 _lastMouse;

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
        Vec2 pos = ToVec2(e);
        
        var props = e.GetCurrentPoint(this).Properties;
        
        if (props.IsRightButtonPressed)
        {
            ShowContextMenu(pos);
            return;
        }

        _draggedVertex = HitTester.FindVertex(Scene, pos, VertexHitRadius);
        if (_draggedVertex == null)
        {
            var edgeHit = HitTester.FindEdge(Scene, pos, EdgeHitTolerance);
            if (edgeHit != null)
            {
                _draggedPolygon = edgeHit.Value.Polygon;
                _lastMouse = pos;
            }
        }

        if (_draggedVertex != null || _draggedPolygon != null)
            e.Pointer.Capture(this);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        Vec2 pos = ToVec2(e);

        if (_draggedVertex != null)
        {
            _draggedVertex.Value.Vertex.Position = pos;
        }
        else if (_draggedPolygon != null)
        {
            _draggedPolygon.Translate(pos - _lastMouse);
            _lastMouse = pos;
        }
        else
        {
            return;
        }

        InvalidateVisual();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        e.Pointer.Capture(null);
        _draggedVertex = null;
        _draggedPolygon = null;
    }

    private void ShowContextMenu(Vec2 pos)
    {
        var vertexHit = HitTester.FindVertex(Scene, pos, VertexHitRadius);
        if (vertexHit != null) { ShowVertexMenu(vertexHit.Value); return; }

        var edgeHit = HitTester.FindEdge(Scene, pos, EdgeHitTolerance);
        if (edgeHit != null) ShowEdgeMenu(edgeHit.Value);
    }

    private void ShowEdgeMenu(HitTester.EdgeHit hit)
    {
        var item = new MenuItem { Header = "Add vertex" };
        item.Click += (_, _) =>
        {
            Scene.RemoveConstraintsOf(hit.Edge);
            hit.Polygon.SplitEdge(hit.Edge);
            InvalidateVisual();
        };
        new ContextMenu { Items = { item } }.Open(this);
    }

    private void ShowVertexMenu(HitTester.VertexHit hit)
    {
        var item = new MenuItem { Header = "Delete vertex" };
        item.IsEnabled = hit.Polygon.Vertices.Count > 3;
        item.Click += (_, _) =>
        {
            var touching = hit.Polygon.Edges
                .Where(e => e.Start == hit.Vertex || e.End == hit.Vertex)
                .ToList();
            foreach (var edge in touching)
                Scene.RemoveConstraintsOf(edge);
            hit.Polygon.RemoveVertex(hit.Vertex);
            InvalidateVisual();
        };
        new ContextMenu { Items = { item } }.Open(this);
    }
}
