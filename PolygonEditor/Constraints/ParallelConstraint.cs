using System;
using System.Collections.Generic;
using PolygonEditor.Geometry;
using PolygonEditor.Model;

namespace PolygonEditor.Constraints;

/// <summary>Two edges (possibly from different polygons) must be parallel.</summary>
public sealed class ParallelConstraint(Edge first, Edge second) : Constraint
{
    public Edge First { get; } = first;
    public Edge Second { get; } = second;

    public override IReadOnlyList<Edge> Edges => [First, Second];

    // Unit directions are parallel when their cross product is 0.
    public override bool IsSatisfied(double tolerance = 1e-6) =>
        Math.Abs(Vec2.Cross(First.Direction.Normalized(), Second.Direction.Normalized())) <= tolerance;
}
