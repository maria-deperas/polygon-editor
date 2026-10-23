namespace PolygonEditor.Model;

/// <summary>How two edges meet at a vertex.</summary>
public enum Continuity
{
    /// <summary>Position only: the edges just touch.</summary>
    G0,
    /// <summary>Same tangent direction (the unit tangent vectors are equal).</summary>
    G1,
    /// <summary>Same tangent vector (both direction and length are equal).</summary>
    C1
}