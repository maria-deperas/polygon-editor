using System;

namespace PolygonEditor.Geometry;

/// <summary>Circular arc: starts at StartAngle and turns by Sweep radians (sign = direction).</summary>
public readonly record struct Arc(Vec2 Center, double Radius, double StartAngle, double Sweep)
{
    public Vec2 PointAt(double t)
    {
        double angle = StartAngle + Sweep * t;
        return Center + new Vec2(Math.Cos(angle), Math.Sin(angle)) * Radius;
    }
}