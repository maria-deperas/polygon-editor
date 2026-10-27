using System;
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
    
    // Parametric curve of the edge, t in [0, 1]. The polygon is needed
    // because an arc depends on its neighbouring edges.
    public virtual Func<double, Vec2> Curve(Polygon polygon, Edge edge)
    {
        Vec2 a = edge.Start.Position;
        Vec2 d = edge.Direction;
        return t => a + d * t;
    }
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
    
    public override Func<double, Vec2> Curve(Polygon polygon, Edge edge)
    {
        Vec2 p0 = edge.Start.Position, p1 = Control1, p2 = Control2, p3 = edge.End.Position;
        return t => PointAt(p0, p1, p2, p3, t);
    }

    // Bernstein form of a cubic Bezier
    public static Vec2 PointAt(Vec2 p0, Vec2 p1, Vec2 p2, Vec2 p3, double t)
    {
        double u = 1 - t;
        return u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
    }
}

/// <summary>Circular arc between the edge's vertices.</summary>
public sealed class ArcShape : EdgeShape
{
    // A degenerate arc (null) is drawn and hit-tested as a straight segment
    public override Func<double, Vec2> Curve(Polygon polygon, Edge edge) =>
        ArcGeometry.Compute(polygon, edge) is Arc arc ? arc.PointAt : base.Curve(polygon, edge);
}
