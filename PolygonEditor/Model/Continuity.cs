namespace PolygonEditor.Model;

/// <summary>How two edges meet at a vertex.</summary>
public enum Continuity
{
    //Position only: the edges just touch.
    G0,
    // Same tangent direction (the unit tangent vectors are equal).
    G1,
    // Same tangent vector (both direction and length are equal).
    C1
}