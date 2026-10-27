using System;
using System.Collections.Generic;
using PolygonEditor.Geometry;
using PolygonEditor.Model;

namespace PolygonEditor.Constraints;

/// <summary>Keeps an edge at a given length.</summary>
public sealed class FixedLengthConstraint(Edge edge, double length) : Constraint
{
    public Edge Edge { get; } = edge;
    public double Length { get; set;} =  length;

    public override IReadOnlyList<Edge> Edges => [Edge];

    public override bool IsSatisfied(double tolerance = 1e-06)
    {
        return Math.Abs(Edge.Length - Length) <=  tolerance;
    }
    
    public override void Project(Vertex? pinned)
    {
        var a = Edge.Start;
        var b = Edge.End;
        var d = Edge.Direction;
        var dir = d.Length < 1e-12 ? new Vec2(1, 0) : d.Normalized();
        var error = d.Length - Length;

        if (pinned == a)
        {
            b.Position -= dir * error;
        }
        else if (pinned == b)
        {
            a.Position += dir * error;
        }
        else
        {
            a.Position += dir * (error / 2);
            b.Position -= dir * (error / 2);
        }
    }
    
    public override string GetLabel(int number) => $"L={Length:0}";
}