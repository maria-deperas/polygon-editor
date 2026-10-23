using System.Collections.Generic;
using PolygonEditor.Constraints;

namespace PolygonEditor.Model;

public sealed class Scene
{
    public List<Polygon> Polygons { get; } = [];
    public List<Constraint> Constraints { get; } = [];
}