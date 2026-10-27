using System;
using PolygonEditor.Geometry;

namespace PolygonEditor.Model;

/// <summary>Computes the circle of an arc edge from its vertices and their continuity.</summary>
public static class ArcGeometry
{
    /// <returns>The arc, or null when it degenerates to a straight segment.</returns>
    public static Arc? Compute(Polygon polygon, Edge edge)
    {
        Vec2 a = edge.Start.Position;
        Vec2 b = edge.End.Position;

        // G1 at the start: leave A along the tangent of the edge before
        Edge before = polygon.EdgeBefore(edge.Start);
        if (edge.Start.Continuity != Continuity.G0 && !before.Shape.AdaptsToNeighbours)
            return Tangent(a, b, before.TangentAtEnd);

        // G1 at the end: the same construction started from B, going backwards
        Edge after = polygon.EdgeAfter(edge.End);
        if (edge.End.Continuity != Continuity.G0 && !after.Shape.AdaptsToNeighbours)
            return Tangent(b, a, -after.TangentAtStart);

        // G0 at both ends: half circle, radius = half of the edge length
        Vec2 center = edge.Midpoint;
        double start = Math.Atan2(a.Y - center.Y, a.X - center.X);
        return new Arc(center, edge.Length / 2, start, Math.PI);
    }

    // Circle through a and b that is tangent to direction t at a
    private static Arc? Tangent(Vec2 a, Vec2 b, Vec2 t)
    {
        Vec2 n = new Vec2(-t.Y, t.X).Normalized();
        Vec2 d = b - a;
        double nd = Vec2.Dot(n, d);
        if (Math.Abs(nd) < 1e-9) return null; // b lies on the tangent line

        double r = Vec2.Dot(d, d) / (2 * nd);  // signed: side of the centre
        Vec2 c = a + n * r;

        double start = Math.Atan2(a.Y - c.Y, a.X - c.X);
        double end = Math.Atan2(b.Y - c.Y, b.X - c.X);

        // Turn in the direction that leaves a along t (angle grows when cross > 0)
        bool increasing = Vec2.Cross(a - c, t) > 0;
        double sweep = end - start;
        if (increasing && sweep < 0) sweep += 2 * Math.PI;
        if (!increasing && sweep > 0) sweep -= 2 * Math.PI;

        return new Arc(c, Math.Abs(r), start, sweep);
    }
}