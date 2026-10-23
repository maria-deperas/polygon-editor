using PolygonEditor.Constraints;
using PolygonEditor.Geometry;

namespace PolygonEditor.Model;

/// <summary>
/// The scene shown at startup. To add a polygon, add one more AddPolygon call with its points.
/// </summary>
public static class DemoScene
{
    public static Scene Create()
    {
        var scene = new Scene();

        var house = scene.AddPolygon(new Polygon([
            new Vec2(120, 320),
            new Vec2(120, 160),
            new Vec2(220, 80),
            new Vec2(320, 160),
            new Vec2(320, 320),
        ]));

        var kite = scene.AddPolygon(new Polygon([
            new Vec2(480, 120),
            new Vec2(640, 200),
            new Vec2(640, 380),
            new Vec2(440, 300),
        ]));

        // Not enforced yet (solver comes in step 2), but already part of the data.
        scene.AddConstraint(new FixedLengthConstraint(house.Edges[4], 200));
        scene.AddConstraint(new ParallelConstraint(house.Edges[0], kite.Edges[1]));

        return scene;
    }
}
