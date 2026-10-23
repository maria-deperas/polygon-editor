using System;
using System.Collections.Generic;
using PolygonEditor.Geometry;
using PolygonEditor.Model;

namespace PolygonEditor.Constraints;

public sealed class ParallelConstraint(Edge first, Edge second) : Constraint
{
    public Edge First { get; } = first;
    public Edge Second { get; } = second;

    public override IReadOnlyList<Edge> Edges => [First, Second];

    public override bool IsSatisfied(double tolerance = 1E-06)
    {
        return Math.Abs(Vec2.Cross(First.Direction.Normalized(), Second.Direction.Normalized())) <= tolerance;
    }
}