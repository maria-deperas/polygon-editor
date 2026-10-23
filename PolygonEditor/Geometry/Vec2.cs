using System;

namespace PolygonEditor.Geometry;

/// <summary>
/// Immutable 2D vector / point. Used everywhere in the model instead of Avalonia's Point,
/// so the model does not depend on the UI library and has the vector operations we need.
/// </summary>
public readonly record struct Vec2(double X, double Y)
{
    public static readonly Vec2 Zero = new(0, 0);

    public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vec2 operator -(Vec2 v) => new(-v.X, -v.Y);
    public static Vec2 operator *(Vec2 v, double s) => new(v.X * s, v.Y * s);
    public static Vec2 operator *(double s, Vec2 v) => v * s;
    public static Vec2 operator /(Vec2 v, double s) => new(v.X / s, v.Y / s);

    public double Length => Math.Sqrt(X * X + Y * Y);

    /// <summary>Unit vector in the same direction, or Zero for a zero-length vector.</summary>
    public Vec2 Normalized()
    {
        var length = Length;
        return length < 1e-12 ? Zero : this / length;
    }

    public static double Dot(Vec2 a, Vec2 b) => a.X * b.X + a.Y * b.Y;

    /// <summary>Z component of the 3D cross product; 0 means the vectors are parallel.</summary>
    public static double Cross(Vec2 a, Vec2 b) => a.X * b.Y - a.Y * b.X;

    public static double Distance(Vec2 a, Vec2 b) => (a - b).Length;

    public static Vec2 Lerp(Vec2 a, Vec2 b, double t) => a + (b - a) * t;

    public override string ToString() => $"({X:0.##}, {Y:0.##})";
}
