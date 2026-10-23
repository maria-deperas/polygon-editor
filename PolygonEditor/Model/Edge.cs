using PolygonEditor.Geometry;

namespace PolygonEditor.Model;

/// <summary>
/// A separate object (not just "vertex i and i+1") so constraints and the shape
/// can refer to it and it keeps its identity while vertices move.
/// </summary>
public sealed class Edge(Vertex start, Vertex end)
{
    public Vertex Start { get; } = start;
    public Vertex End { get; } = end;
    public EdgeShape Shape { get; set; } = new LineShape();

    public Vec2 Direction => End.Position - Start.Position;
    public double Length => Direction.Length;
    public Vec2 Midpoint => Vec2.Lerp(Start.Position, End.Position, 0.5);
}
