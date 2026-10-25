using System.Collections.Generic;
using System.Linq;
using PolygonEditor.Geometry;
using PolygonEditor.Model;

namespace PolygonEditor.Constraints;

public static class ConstraintSolver
{
    private const int MaxIterations = 100;
    
    public static bool MoveVertex(Scene scene, Vertex vertex, Vec2 target)
    {
        var saved = new Dictionary<Vertex, Vec2>();
        foreach (Polygon polygon in scene.Polygons)
        {
            foreach (Vertex v in polygon.Vertices)
            {
                saved.Add(v, v.Position);
            }
        }

        vertex.Position = target;

        for (int i = 0; i < MaxIterations; i++)
        {
            if (scene.Constraints.All(c => c.IsSatisfied()))
                return true;

            foreach (var c in scene.Constraints)
            {
                c.Project(vertex);
            }
        }

        if (scene.Constraints.All(c => c.IsSatisfied()))
            return true;

        // Move impossible: restore positions
        foreach (var (v, pos) in saved)
        {
            v.Position = pos;
        }
        return false;
    }
}