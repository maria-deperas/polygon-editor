using PolygonEditor.Geometry;

namespace PolygonEditor.Model;

/// <summary>
/// What an edge looks like between its two vertices. Switching an edge between a segment,
/// a Bezier curve and an arc only replaces this object; vertices and constraints stay.
/// </summary>
public abstract class EdgeShape;

public sealed class LineShape : EdgeShape;

/// <summary>Cubic Bezier segment: Start, Control1, Control2, End.</summary>
public sealed class BezierShape(Vec2 control1, Vec2 control2) : EdgeShape
{
    public Vec2 Control1 { get; set; } = control1;
    public Vec2 Control2 { get; set; } = control2;
}

/// <summary>Circular arc between the edge's vertices (details in step 3).</summary>
public sealed class ArcShape : EdgeShape;
