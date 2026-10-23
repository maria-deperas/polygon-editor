using System.Collections.Generic;
using PolygonEditor.Model;

namespace PolygonEditor.Constraints;

/// <summary>
/// A geometric relation that must hold for one or more edges.
/// Step 1 only stores constraints; the solver that enforces them comes in step 2.
/// </summary>
public abstract class Constraint
{
    /// <summary>The edges this constraint refers to. Used to drop it when an edge disappears.</summary>
    public abstract IReadOnlyList<Edge> Edges { get; }

    public abstract bool IsSatisfied(double tolerance = 1e-6);
}
