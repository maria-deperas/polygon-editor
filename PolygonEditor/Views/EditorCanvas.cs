using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PolygonEditor.Geometry;
using PolygonEditor.Model;

namespace PolygonEditor.Views;

public sealed class EditorCanvas : Control
{
    private static readonly Pen EdgePen = new(Brushes.Black, 1);
    public Scene Scene { get; set; } = new();

    public override void Render(DrawingContext context)
    {
        context.FillRectangle(Brushes.White, new Rect(Bounds.Size));

        foreach (var polygon in Scene.Polygons)
        {
            foreach (var edge in polygon.Edges)
                context.DrawLine(EdgePen, ToPoint(edge.Start.Position), ToPoint(edge.End.Position));

            foreach (var vertex in polygon.Vertices)
                context.DrawEllipse(Brushes.Black, null, ToPoint(vertex.Position), 4, 4);
        }
    }

    private static Point ToPoint(Vec2 v) => new(v.X, v.Y);
}