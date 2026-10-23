using System;
using System.Collections.Generic;
using PolygonEditor.Geometry;

namespace PolygonEditor.Model;

public sealed class Polygon
{
    private readonly List<Vertex> _vertices = [];
    private readonly List<Edge> _edges = [];
    
    public IReadOnlyList<Vertex> Vertices => _vertices;
    public IReadOnlyList<Edge> Edges => _edges;

    public Polygon(IEnumerable<Vec2> points)
    {
        foreach (var point in points)
        {
            _vertices.Add(new Vertex(point));
        }
        
        int n = _vertices.Count;
        if (n < 3) throw new ArgumentException("A polygon needs at least 3 vertices.");

        for (int i = 0; i < n; i++)
        {
            _edges.Add(new Edge(_vertices[i], _vertices[(i + 1) % n]));
        }
    }
}