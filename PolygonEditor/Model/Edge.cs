using PolygonEditor.Geometry;

namespace PolygonEditor.Model;

/// <summary>Edge from Start to End; its Shape decides whether it is a segment, Bézier or arc.</summary>
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
    
    // Tangent vectors at the ends (for a Bezier: 3(P1 - P0) and 3(P3 - P2))
    public Vec2 TangentAtStart => Shape.TangentAtStart(this);
    public Vec2 TangentAtEnd => Shape.TangentAtEnd(this);
    
    // True when the edges touch; two touching edges cannot be made parallel
    public bool SharesVertexWith(Edge other) =>
        Start == other.Start || Start == other.End ||
        End == other.Start || End == other.End;
}