using System.Collections.Generic;
using System.Linq;
using PolygonEditor.Geometry;
using PolygonEditor.Model;

namespace PolygonEditor.Constraints;

/// <summary>Satisfies all constraints by repeatedly projecting them one by one.</summary>
public static class ConstraintSolver
{
    private const int MaxIterations = 100;
    
    // sets the vertex to the cursor's position
    public static bool MoveVertex(Scene scene, Vertex vertex, Vec2 target)
    {
        Vec2 old = vertex.Position;
        vertex.Position = target;
        if (Solve(scene, vertex))
            return true;
        vertex.Position = old;
        return false;
    }

    public static bool Solve(Scene scene, Vertex? pinned)
    {
        var saved = new Dictionary<Vertex, Vec2>();
        foreach (Polygon polygon in scene.Polygons)
        foreach (Vertex v in polygon.Vertices)
            saved.Add(v, v.Position);

        for (int i = 0; i < MaxIterations; i++)
        {
            if (scene.Constraints.All(c => c.IsSatisfied()))
                return true;

            foreach (var c in scene.Constraints)
                c.Project(pinned);
        }

        if (scene.Constraints.All(c => c.IsSatisfied()))
            return true;

        // Constraints cannot be satisfied: restore positions
        foreach (var (v, pos) in saved)
            v.Position = pos;
        return false;
    }
}