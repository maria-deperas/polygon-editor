using PolygonEditor.Geometry;

namespace PolygonEditor.Model;

public sealed class Vertex(Vec2 position)
{
    public Vec2 Position { get; set; } = position;
    public Continuity Continuity { get; set; } = Continuity.G1;
    public bool ContinuityIsExplicit { get; set; } = false;
}