using System;
using System.Collections.Generic;
using PolygonEditor.Model;

namespace PolygonEditor.Constraints;

public sealed class FixedLengthConstraint(Edge edge, double length) : Constraint
{
    public Edge Edge { get; } = edge;
    public double Length { get; set;} =  length;

    public override IReadOnlyList<Edge> Edges => [Edge];

    public override bool IsSatisfied(double tolerance = 1E-06)
    {
        return Math.Abs(Edge.Length - Length) <=  tolerance;
    }
}