using System;
using PolygonEditor.Geometry;
using PolygonEditor.Model;

namespace PolygonEditor.Editing;

public static class HitTester
{
    public readonly record struct VertexHit(Polygon Polygon, Vertex Vertex);
    public readonly record struct EdgeHit(Polygon Polygon, Edge Edge);
    public readonly record struct ControlPointHit(Polygon Polygon, Edge Edge, BezierShape Bezier, bool IsFirst);
    private const int CurveSamples = 32;

    public static double DistanceToSegment(Vec2 p, Vec2 a, Vec2 b)
    {
        Vec2 ab = b - a;
        Vec2 ap = p - a;
        double lengthSq = Vec2.Dot(ab, ab);
        if (lengthSq < 1e-12) return (p - a).Length;
        
        double t = Vec2.Dot(ap, ab) / lengthSq;
        t = Math.Clamp(t, 0, 1);

        Vec2 closest = a + ab * t;
        return (p - closest).Length;
    }

    public static VertexHit? FindVertex(Scene scene, Vec2 point, double radius)
    {
        foreach (Polygon polygon in scene.Polygons)
        {
            foreach (Vertex vertex in polygon.Vertices)
            {
                if ((vertex.Position - point).Length <= radius)
                {
                    return new VertexHit(polygon, vertex);
                }
            }
        }

        return null;
    }

    public static EdgeHit? FindEdge(Scene scene, Vec2 point, double tolerance)
    {
        foreach (Polygon polygon in scene.Polygons){
            foreach (Edge edge in polygon.Edges){
                if (DistanceToEdge(point, polygon, edge) <= tolerance){
                    return new EdgeHit(polygon, edge);
                }
            }
        }
        return null;
    }
    
    public static ControlPointHit? FindControlPoint(Scene scene, Vec2 point, double radius)
    {
        foreach (Polygon polygon in scene.Polygons)
        foreach (Edge edge in polygon.Edges)
        {
            if (edge.Shape is not BezierShape bezier) continue;
            if ((bezier.Control1 - point).Length <= radius)
                return new ControlPointHit(polygon, edge, bezier, true);
            if ((bezier.Control2 - point).Length <= radius)
                return new ControlPointHit(polygon, edge, bezier, false);
        }
        return null;
    }

    // A curve is approximated by a polyline of CurveSamples segments
    public static double DistanceToEdge(Vec2 p, Polygon polygon, Edge edge)
    {
        Func<double, Vec2> curve = edge.Shape.Curve(polygon, edge);

        double best = double.MaxValue;
        Vec2 prev = curve(0);
        for (int i = 1; i <= CurveSamples; i++)
        {
            Vec2 next = curve((double)i / CurveSamples);
            best = Math.Min(best, DistanceToSegment(p, prev, next));
            prev = next;
        }
        return best;
    }
}