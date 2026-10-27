using System.Collections.Generic;
using PolygonEditor.Model;

namespace PolygonEditor.Constraints;

/// <summary>Geometric relation on one or more edges that the ConstraintSolver keeps satisfied.</summary>
public abstract class Constraint
{
    public abstract IReadOnlyList<Edge> Edges { get; }
    public abstract bool IsSatisfied(double tolerance = 1e-6);
    public abstract void Project(Vertex? pinned);
    public abstract string GetLabel(int number);
}