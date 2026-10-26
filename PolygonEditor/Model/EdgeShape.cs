using PolygonEditor.Geometry;

namespace PolygonEditor.Model;

/// <summary>
/// What an edge looks like between its two vertices. Switching an edge between a segment,
/// a Bezier curve and an arc only replaces this object; vertices and constraints stay.
/// </summary>
public abstract class EdgeShape
{
    // Tangent vectors at the ends of the edge; a straight segment uses its direction
    public virtual Vec2 TangentAtStart(Edge edge) => edge.Direction;
    public virtual Vec2 TangentAtEnd(Edge edge) => edge.Direction;

    // Called after the edge's vertices moved by startDelta and endDelta
    public virtual void FollowEnds(Vec2 startDelta, Vec2 endDelta) { }
}

/// <summary>A straight line segment.</summary>
public sealed class LineShape : EdgeShape;

/// <summary>Cubic Bezier segment: Start, Control1, Control2, End.</summary>
public sealed class BezierShape(Vec2 control1, Vec2 control2) : EdgeShape
{
    public Vec2 Control1 { get; set; } = control1;
    public Vec2 Control2 { get; set; } = control2;
    
    // 3(P1 - P0) and 3(P3 - P2)
    public override Vec2 TangentAtStart(Edge edge) => 3 * (Control1 - edge.Start.Position);
    public override Vec2 TangentAtEnd(Edge edge) => 3 * (edge.End.Position - Control2);

    // Each arm moves together with its vertex
    public override void FollowEnds(Vec2 startDelta, Vec2 endDelta)
    {
        Control1 += startDelta;
        Control2 += endDelta;
    }
}

/// <summary>Circular arc between the edge's vertices.</summary>
public sealed class ArcShape : EdgeShape;
