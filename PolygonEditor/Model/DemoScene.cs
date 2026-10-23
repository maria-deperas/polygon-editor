using System;
using PolygonEditor.Constraints;
using PolygonEditor.Geometry;

namespace PolygonEditor.Model;

public static class DemoScene
{
    public static Scene Create()
    {
        var scene = new Scene();

        var house = new Polygon([
            new Vec2(120, 320),
            new Vec2(120, 160),
            new Vec2(220, 80),
            new Vec2(320, 160),
            new Vec2(320, 320),
        ]);

        var quad = new Polygon([
            new Vec2(480, 120),
            new Vec2(640, 200),
            new Vec2(640, 380),
            new Vec2(440, 300),
        ]);

        scene.Polygons.Add(house);
        scene.Polygons.Add(quad);

        scene.Constraints.Add(new FixedLengthConstraint(house.Edges[4], 200));
        scene.Constraints.Add(new ParallelConstraint(house.Edges[0], quad.Edges[1]));
        
        Console.WriteLine(scene.Constraints[0].IsSatisfied());

        return scene;
    }
}