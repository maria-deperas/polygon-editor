using System.Collections.Generic;
using PolygonEditor.Geometry;
using PolygonEditor.Model;

namespace PolygonEditor.Constraints;

/// <summary>
/// Keeps G1/C1 continuity at vertices next to Bezier edges by moving control points.
/// Vertices are never moved here, so edge constraints stay satisfied.
/// </summary>
public static class ContinuitySolver
{
    // Where a control point next to v must be so that its arm follows the tangent
    private static Vec2 Fit(Vertex v, Vec2 current, Vec2 tangent) =>
        v.Continuity == Continuity.C1
            ? v.Position + tangent / 3
            : v.Position + tangent.Normalized() * (current - v.Position).Length;

    // Enforces continuity at every vertex.
    public static void Apply(Scene scene, BezierShape? locked = null, bool lockedIsFirst = false)
    {
        foreach (var polygon in scene.Polygons)
        foreach (var v in polygon.Vertices)
        {
            if (v.Continuity == Continuity.G0) continue;

            Edge inEdge = polygon.EdgeBefore(v);
            Edge outEdge = polygon.EdgeAfter(v);
            
            // An arc adapts to its neighbours itself (see ArcGeometry)
            if (inEdge.Shape.AdaptsToNeighbours || outEdge.Shape.AdaptsToNeighbours) continue;
            bool outLocked = outEdge.Shape == locked && lockedIsFirst;
            bool inLocked = inEdge.Shape == locked && !lockedIsFirst;

            if (outEdge.Shape is BezierShape outB && !outLocked)
                outB.Control1 = Fit(v, outB.Control1, inEdge.TangentAtEnd);
            else if (inEdge.Shape is BezierShape inB && !inLocked)
                inB.Control2 = Fit(v, inB.Control2, -outEdge.TangentAtStart);
        }
    }
    
    // Vertex positions before a move, so FollowVertices can tell how far each vertex went
    public static Dictionary<Vertex, Vec2> Snapshot(Scene scene)
    {
        var positions = new Dictionary<Vertex, Vec2>();
        foreach (var polygon in scene.Polygons)
        foreach (var v in polygon.Vertices)
            positions[v] = v.Position;
        return positions;
    }

    // Control points follow their vertex: a vertex moved by d moves its arms by d too
    public static void FollowVertices(Scene scene, Dictionary<Vertex, Vec2> before)
    {
        foreach (var polygon in scene.Polygons)
        foreach (var edge in polygon.Edges)
            edge.Shape.FollowEnds(edge.Start.Position - before[edge.Start],
                edge.End.Position - before[edge.End]);
    }

    // Moves a control point to the target. 
    public static void MoveControlPoint(Scene scene, Polygon polygon, Edge edge,
        BezierShape bezier, bool isFirst, Vec2 target)
    {
        Vertex v = isFirst ? edge.Start : edge.End;
        Edge other = isFirst ? polygon.EdgeBefore(v) : polygon.EdgeAfter(v);

        if (v.Continuity != Continuity.G0 && other.Shape.IsStraight)
        {
            Vertex far = isFirst ? other.Start : other.End;
            Vec2 arm = target - v.Position; // from the vertex towards the control point
            Vec2 farTarget = v.Continuity == Continuity.C1
                ? v.Position - 3 * arm
                : v.Position - arm.Normalized() * (far.Position - v.Position).Length;

            var before = Snapshot(scene);
            if (ConstraintSolver.MoveVertex(scene, far, farTarget))
                FollowVertices(scene, before);
        }

        // Set last, so the dragged point always stays under the cursor
        if (isFirst) bezier.Control1 = target;
        else bezier.Control2 = target;

        Apply(scene, bezier, isFirst);
    }
}