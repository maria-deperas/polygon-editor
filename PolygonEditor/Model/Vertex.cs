using PolygonEditor.Geometry;

namespace PolygonEditor.Model;

public sealed class Vertex(Vec2 position)
{
    public Vec2 Position { get; set; } = position;

    /// <summary>Default for surnames A-K is G1.</summary>
    public Continuity Continuity { get; set; } = Continuity.G1;

    /// <summary>
    /// True when the user picked the continuity by hand. Switching an edge to a curve
    /// only applies the default continuity when this is false.
    /// </summary>
    public bool ContinuityIsExplicit { get; set; }
}
