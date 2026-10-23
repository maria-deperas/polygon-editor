using PolygonEditor.Geometry;

namespace PolygonEditor.Model;

public sealed class Edge
{
    public Edge(Vertex start, Vertex end)
    {
        Start = start;
        End = end;
    }
    
    public Vertex Start { get; }
    public Vertex End { get; }
    
    public EdgeShape Shape { get; set; } = new LineShape();
    
    public Vec2 Direction => End.Position - Start.Position;
    public double Length => Direction.Length;
    public Vec2 Midpoint => Start.Position + Direction / 2;
}