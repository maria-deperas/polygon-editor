using System.Collections.Generic;
using System.Linq;
using PolygonEditor.Constraints;

namespace PolygonEditor.Model;

/// <summary>Everything on the canvas: polygons and the constraints between their edges.
/// Operations that change edges also keep the constraint list consistent.</summary>
public sealed class Scene
{
    public List<Polygon> Polygons { get; } = [];
    public List<Constraint> Constraints { get; } = [];

    public void RemoveConstraintsOf(Edge edge)
    {
        Constraints.RemoveAll(c => c.Edges.Contains(edge));
    }
    
    public string LabelOf(Constraint c)
    {
        int pairNumber = 0;
        foreach (var other in Constraints)
        {
            if (other.Edges.Count > 1)
                pairNumber++;

            if (other == c)
                return c.GetLabel(c.Edges.Count > 1 ? pairNumber : 0);
        }
        return "";
    }
    
    // Adds the constraint only if the solver can satisfy it; otherwise leaves the scene unchanged
    public bool TryAddConstraint(Constraint constraint)
    {
        Constraints.Add(constraint);
        if (ConstraintSolver.Solve(this, null))
            return true;
        Constraints.Remove(constraint);
        return false;
    }

    // An edge has at most one fixed length
    public bool SetFixedLength(Edge edge, double length)
    {
        var old = Constraints.OfType<FixedLengthConstraint>().FirstOrDefault(f => f.Edge == edge);
        if (old != null)
            Constraints.Remove(old);

        if (TryAddConstraint(new FixedLengthConstraint(edge, length)))
            return true;

        if (old != null)
            Constraints.Add(old);
        return false;
    }

    // Splitting or removing a vertex replaces edges, so constraints on the old edges must go
    public void SplitEdge(Polygon polygon, Edge edge)
    {
        RemoveConstraintsOf(edge);
        polygon.SplitEdge(edge);
    }

    public bool RemoveVertex(Polygon polygon, Vertex vertex)
    {
        if (polygon.Vertices.Count <= 3)
            return false;
        RemoveConstraintsOf(polygon.EdgeBefore(vertex));
        RemoveConstraintsOf(polygon.EdgeAfter(vertex));
        return polygon.RemoveVertex(vertex);
    }
}