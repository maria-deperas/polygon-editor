using System;
using PolygonEditor.Geometry;

namespace PolygonEditor.Rendering;

public static class ArcDrawer
{
    public static void Draw(ILineDrawer drawer, Arc arc)
    {
        int n = Math.Max(8, (int)(arc.Radius * Math.Abs(arc.Sweep) / 5));
        Vec2 prev = arc.PointAt(0);
        for (int i = 1; i <= n; i++)
        {
            Vec2 next = arc.PointAt((double)i / n);
            drawer.DrawLine(prev, next);
            prev = next;
        }
    }
}