using System.Collections.Generic;
using System.Linq;
using PolygonEditor.Constraints;

namespace PolygonEditor.Model;

public sealed class Scene
{
    public List<Polygon> Polygons { get; } = [];
    public List<Constraint> Constraints { get; } = [];

    public void RemoveConstraintsOf(Edge edge)
    {
        Constraints.RemoveAll(c => c.Edges.Contains(edge));
    }
}