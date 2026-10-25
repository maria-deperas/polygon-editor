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

    public void Translate(Vec2 delta)
    {
        foreach (var vertex in _vertices)
            vertex.Position += delta;

        foreach (var edge in _edges)
        {
            if (edge.Shape is BezierShape bezier)
            {
                bezier.Control1 += delta;
                bezier.Control2 += delta;
            }
        }
    }
    
    // Edge i goes from vertex i to vertex i+1
    public Edge EdgeBefore(Vertex v)
    {
        int n = _edges.Count;
        return _edges[(_vertices.IndexOf(v) - 1 + n) % n];
    }

    public Edge EdgeAfter(Vertex v) => _edges[_vertices.IndexOf(v)];
    
    public Vertex SplitEdge(Edge edge)
    {
        int i = _edges.IndexOf(edge);
        var newVertex = new Vertex(edge.Midpoint);
        _vertices.Insert(i + 1, newVertex);

        var first  = new Edge(_vertices[i], newVertex);
        var second = new Edge(newVertex, _vertices[(i + 2) % _vertices.Count]); 
        
        _edges[i] = first;
        _edges.Insert(i + 1, second);

        return newVertex;
    }

    public bool RemoveVertex(Vertex vertex)
    {
        int n = _vertices.Count;
        if (n <= 3) return false;

        int i = _vertices.IndexOf(vertex);
        var prev = _vertices[(i - 1 + n) % n];
        var next = _vertices[(i + 1) % n];
        var merged = new Edge(prev, next);

        _vertices.RemoveAt(i);

        if (i > 0)
        {
            _edges[i - 1] = merged;
            _edges.RemoveAt(i);
        }
        else
        {
            _edges.RemoveAt(n - 1);
            _edges.RemoveAt(0);
            _edges.Add(merged);
        }

        return true;
    }
}