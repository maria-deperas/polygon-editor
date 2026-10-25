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
}