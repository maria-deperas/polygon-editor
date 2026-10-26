using PolygonEditor.Constraints;
using PolygonEditor.Geometry;

namespace PolygonEditor.Model;

/// <summary>
/// The scene shown at startup. To add a polygon: create it from its points, add it to
/// scene.Polygons, then optionally change edge shapes, continuity and constraints.
/// </summary>
public static class DemoScene
{
    public static Scene Create()
    {
        var scene = new Scene();

        // Left: Bezier corner (G1 / C1), half-circle arc, two parallel pairs
        var left = new Polygon([
            new Vec2(100, 500),
            new Vec2(100, 250),
            new Vec2(200, 150),
            new Vec2(320, 150),
            new Vec2(420, 250),
            new Vec2(420, 380),
            new Vec2(300, 420),
            new Vec2(250, 500),
        ]);
        scene.Polygons.Add(left);

        left.Edges[1].Shape = new BezierShape(new Vec2(100, 180), new Vec2(150, 120));
        SetContinuity(left.Vertices[1], Continuity.G1);
        SetContinuity(left.Vertices[2], Continuity.C1);

        left.Edges[5].Shape = new ArcShape(); // G0 at both ends: half circle
        SetContinuity(left.Vertices[5], Continuity.G0);
        SetContinuity(left.Vertices[6], Continuity.G0);

        // The bottom edge has two constraints at once
        scene.Constraints.Add(new FixedLengthConstraint(left.Edges[7], left.Edges[7].Length));
        scene.Constraints.Add(new ParallelConstraint(left.Edges[2], left.Edges[7]));

        // Right: two Beziers joined with C1, arc tangent to the right wall (G1)
        var right = new Polygon([
            new Vec2(560, 150),
            new Vec2(760, 120),
            new Vec2(880, 220),
            new Vec2(880, 400),
            new Vec2(780, 520),
            new Vec2(620, 480),
        ]);
        scene.Polygons.Add(right);

        right.Edges[0].Shape = new BezierShape(new Vec2(560, 90), new Vec2(680, 80));
        right.Edges[1].Shape = new BezierShape(new Vec2(840, 160), new Vec2(880, 160));
        SetContinuity(right.Vertices[0], Continuity.G1);
        SetContinuity(right.Vertices[1], Continuity.C1);
        SetContinuity(right.Vertices[2], Continuity.G1);

        right.Edges[3].Shape = new ArcShape();
        SetContinuity(right.Vertices[3], Continuity.G1); // the arc leaves the wall tangentially
        SetContinuity(right.Vertices[4], Continuity.G0);

        scene.Constraints.Add(new FixedLengthConstraint(right.Edges[5], right.Edges[5].Length));

        // Constraint between two polygons: both vertical walls stay parallel
        scene.Constraints.Add(new ParallelConstraint(left.Edges[0], right.Edges[2]));

        // Fit control points so the chosen continuity holds from the start
        ContinuitySolver.Apply(scene);
        return scene;
    }

    // Explicit continuity is kept when edges next to the vertex change shape
    private static void SetContinuity(Vertex vertex, Continuity continuity)
    {
        vertex.Continuity = continuity;
        vertex.ContinuityIsExplicit = true;
    }
}