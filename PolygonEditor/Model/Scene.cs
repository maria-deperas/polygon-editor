using System.Collections.Generic;
using PolygonEditor.Constraints;

namespace PolygonEditor.Model;

/// <summary>
/// Everything on the canvas. Constraints live here, not on edges, because one constraint
/// (e.g. parallel) can join edges of two different polygons.
/// </summary>
public sealed class Scene
{
    public List<Polygon> Polygons { get; } = [];
    public List<Constraint> Constraints { get; } = [];

    public Polygon AddPolygon(Polygon polygon)
    {
        Polygons.Add(polygon);
        return polygon;
    }

    public void AddConstraint(Constraint constraint) => Constraints.Add(constraint);
}
