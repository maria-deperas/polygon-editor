namespace PolygonEditor.Model;

/// <summary>How two edges meet at a vertex. Only matters when a curve is next to the vertex.</summary>
public enum Continuity
{
    /// <summary>Position only: the edges just touch.</summary>
    G0,

    /// <summary>Same tangent direction (unit tangent vectors are equal).</summary>
    G1,

    /// <summary>Same tangent vector (direction and length).</summary>
    C1,
}
