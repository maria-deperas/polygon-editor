using System;
using PolygonEditor.Geometry;
using PolygonEditor.Model;

namespace PolygonEditor.Editing;

public static class HitTester
{
    public readonly record struct VertexHit(Polygon Polygon, Vertex Vertex);

    public readonly record struct EdgeHit(Polygon Polygon, Edge Edge);

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
                if(DistanceToSegment(point, edge.Start.Position, edge.End.Position) <= tolerance){
                    return new EdgeHit(polygon, edge);
                }
            }
        }
        return null;
    }
}