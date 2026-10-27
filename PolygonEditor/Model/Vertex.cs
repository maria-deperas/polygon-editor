using PolygonEditor.Geometry;

namespace PolygonEditor.Model;

/// <summary>Polygon corner shared by two edges; stores the continuity required there.</summary>
public sealed class Vertex(Vec2 position)
{
    public Vec2 Position { get; set; } = position;
    
    // G1 by default
    public Continuity Continuity { get; set; } = Continuity.G1;
    
    // Continuity selected by the user
    public bool ContinuityIsExplicit { get; set; } = false;
}