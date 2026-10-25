using System;
using PolygonEditor.Geometry;

namespace PolygonEditor.Rendering;

/// <summary>
/// Draws a cubic Bezier curve incrementally: convert to the power basis,
/// then step along the curve with forward differences (additions only).
/// </summary>
public static class BezierDrawer
{
    public static void Draw(ILineDrawer drawer, Vec2 p0, Vec2 p1, Vec2 p2, Vec2 p3)
    {
        // Power basis: P(t) = A t^3 + B t^2 + C t + D
        Vec2 a = -p0 + 3 * p1 - 3 * p2 + p3;
        Vec2 b = 3 * p0 - 6 * p1 + 3 * p2;
        Vec2 c = 3 * (p1 - p0);
        Vec2 d = p0;

        // Number of steps based on the control polygon length
        double length = (p1 - p0).Length + (p2 - p1).Length + (p3 - p2).Length;
        int n = Math.Max(10, (int)(length / 5));
        double h = 1.0 / n;

        // Forward differences for step h
        Vec2 delta1 = a * (h * h * h) + b * (h * h) + c * h;
        Vec2 delta2 = a * (6 * h * h * h) + b * (2 * h * h);
        Vec2 delta3 = a * (6 * h * h * h);

        Vec2 point = d;
        for (int i = 0; i < n; i++)
        {
            Vec2 next = i == n - 1 ? p3 : point + delta1;
            drawer.DrawLine(point, next);
            point = next;
            delta1 += delta2;
            delta2 += delta3;
        }
    }
}