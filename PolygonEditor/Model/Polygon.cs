using System;
using System.Collections.Generic;
using PolygonEditor.Geometry;

namespace PolygonEditor.Model;

/// <summary>
/// Closed polygon: a cyclic list of vertices. Edges[i] goes from Vertices[i]
/// to Vertices[(i + 1) % n], so the last edge closes the polygon.
/// </summary>
public sealed class Polygon
{
    private readonly List<Vertex> _vertices = [];
    private readonly List<Edge> _edges = [];

    public IReadOnlyList<Vertex> Vertices => _vertices;
    public IReadOnlyList<Edge> Edges => _edges;

    public Polygon(IEnumerable<Vec2> points)
    {
        foreach (var point in points)
            _vertices.Add(new Vertex(point));

        if (_vertices.Count < 3)
            throw new ArgumentException("A polygon needs at least 3 vertices.", nameof(points));

        for (var i = 0; i < _vertices.Count; i++)
            _edges.Add(new Edge(_vertices[i], _vertices[(i + 1) % _vertices.Count]));
    }

    public Edge EdgeAfter(Vertex vertex) => _edges[_vertices.IndexOf(vertex)];

    public Edge EdgeBefore(Vertex vertex)
    {
        var index = _vertices.IndexOf(vertex);
        return _edges[(index - 1 + _edges.Count) % _edges.Count];
    }

    /// <summary>Moves the whole polygon, including Bezier control points. Never breaks constraints.</summary>
    public void Translate(Vec2 offset)
    {
        foreach (var vertex in _vertices)
            vertex.Position += offset;

        foreach (var edge in _edges)
        {
            if (edge.Shape is BezierShape bezier)
            {
                bezier.Control1 += offset;
                bezier.Control2 += offset;
            }
        }
    }
}
