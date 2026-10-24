using Avalonia.Media;
using Avalonia;
using PolygonEditor.Geometry;

namespace PolygonEditor.Rendering;

public sealed class LibraryLineDrawer (DrawingContext context) : ILineDrawer
{
    private static readonly Pen EdgePen = new(Brushes.Black, 1);
    
    public void DrawLine(Vec2 from, Vec2 to)
    {
        context.DrawLine(EdgePen, new Point(from.X, from.Y), new Point(to.X, to.Y));
    }
}