using System;
using System.Collections.Generic;
using PolygonEditor.Geometry;
using PolygonEditor.Model;

namespace PolygonEditor.Constraints;

/// <summary>Keeps two edges parallel (possibly in different polygons).</summary>
public sealed class ParallelConstraint(Edge first, Edge second) : Constraint
{
    public Edge First { get; } = first;
    public Edge Second { get; } = second;

    public override IReadOnlyList<Edge> Edges => [First, Second];

    public override bool IsSatisfied(double tolerance = 1E-06)
    {
        return Math.Abs(Vec2.Cross(First.Direction.Normalized(), Second.Direction.Normalized())) <= tolerance;
    }

    public override void Project(Vertex? pinned)
    {
        Vec2 u1 = First.Direction.Normalized();
        Vec2 u2 = Second.Direction.Normalized();
        Vec2 target;

        int sign = Vec2.Dot(u1, u2) < 0 ? -1 : 1;

        Vec2 u2s = u2 * sign;

        if (pinned == First.Start || pinned == First.End)
        {
            target = u1;
        }
        else if (pinned == Second.Start || pinned == Second.End)
        {
            target = u2s;
        }
        else
        {
            target = (u1 + u2s).Normalized();
        }
        
        Align(First, target, pinned);
        Align(Second, target * sign, pinned);
    }

    private static void Align(Edge e, Vec2 dir, Vertex? pinned)
    {
        double len = e.Length;
        
        if (pinned == e.Start)
        {
            e.End.Position =  e.Start.Position + (dir * len);
        }
        else if (pinned == e.End)
        {
            e.Start.Position =  e.End.Position - (dir * len);
        }
        else
        {
            Vec2 mid = e.Midpoint;
            e.Start.Position = mid - (dir * len) / 2;
            e.End.Position = mid + (dir * len) / 2;
        }
    }
    public override string GetLabel(int number) => $"∥{number}";
}