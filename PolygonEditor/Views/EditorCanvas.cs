using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PolygonEditor.Geometry;
using PolygonEditor.Model;
using PolygonEditor.Rendering;

namespace PolygonEditor.Views;

public sealed class EditorCanvas : Control
{
    public Scene Scene { get; set; } = new();
    
    private PixelBuffer? _buffer;
    private WriteableBitmap? _bitmap;
    
    private bool _useBresenham;

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

    private void DrawEdges(ILineDrawer drawer)
    {
        foreach (var polygon in Scene.Polygons)
        {
            foreach (var edge in polygon.Edges)
            {
                drawer.DrawLine(edge.Start.Position, edge.End.Position);
            }
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
}