using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PolygonEditor.Geometry;
using PolygonEditor.Model;

namespace PolygonEditor.Views;

/// <summary>The control the scene is drawn on. For now it uses the library's DrawLine.</summary>
public sealed class EditorCanvas : Control
{
    private const double VertexRadius = 4;

    private static readonly IBrush BackgroundBrush = Brushes.White;
    private static readonly Pen EdgePen = new(Brushes.Black, 1);
    private static readonly IBrush VertexBrush = Brushes.Black;

    public Scene Scene { get; set; } = new();

    public override void Render(DrawingContext context)
    {
        // Filling the background also makes the whole area receive mouse events later.
        context.FillRectangle(BackgroundBrush, new Rect(Bounds.Size));

        foreach (var polygon in Scene.Polygons)
        {
            foreach (var edge in polygon.Edges)
                context.DrawLine(EdgePen, ToPoint(edge.Start.Position), ToPoint(edge.End.Position));

            foreach (var vertex in polygon.Vertices)
                context.DrawEllipse(VertexBrush, null, ToPoint(vertex.Position), VertexRadius, VertexRadius);
        }
    }

    private static Point ToPoint(Vec2 v) => new(v.X, v.Y);
}
