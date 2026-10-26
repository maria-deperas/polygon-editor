using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PolygonEditor.Constraints;
using PolygonEditor.Editing;
using PolygonEditor.Geometry;
using PolygonEditor.Model;
using PolygonEditor.Rendering;

namespace PolygonEditor.Views;

public sealed class EditorCanvas : Control
{
    private const double VertexHitRadius = 8;
    private const double EdgeHitTolerance = 5;
    private const double PointRadius = 2.5;

    private PixelBuffer? _buffer;
    private WriteableBitmap? _bitmap;
    private bool _useBresenham;
    private Polygon? _draggedPolygon;
    private Vec2 _lastMouse;
    private Edge? _parallelSource;

    // Vertex currently dragged by the mouse, null when nothing is dragged.
    private HitTester.VertexHit? _draggedVertex;
    
    // Bezier control point currently dragged, null when none.
    private HitTester.ControlPointHit? _draggedControl;

    public Scene Scene { get; set; } = new();

    public bool UseBresenham
    {
        get => _useBresenham;
        set
        {
            _useBresenham = value;
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        int width = (int)Bounds.Width;
        int height = (int)Bounds.Height;
        if (width <= 0 || height <= 0) return;

        if (UseBresenham)
        {
            if (_buffer is null || _buffer.Width != width || _buffer.Height != height)
            {
                _buffer = new PixelBuffer(width, height);
                _bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96),
                    PixelFormat.Bgra8888, AlphaFormat.Premul);
            }

            _buffer.Clear(0xFFFFFFFF);
            DrawEdges(new BresenhamLineDrawer(_buffer));

            using (var fb = _bitmap!.Lock())
            {
                var pixels = Unsafe.As<int[]>(_buffer.Pixels);
                for (var y = 0; y < height; y++)
                    Marshal.Copy(pixels, y * width, fb.Address + y * fb.RowBytes, width);
            }

            context.DrawImage(_bitmap!, new Rect(0, 0, width, height));
        }
        else
        {
            context.FillRectangle(Brushes.White, new Rect(Bounds.Size));
            DrawEdges(new LibraryLineDrawer(context));
        }

        DrawControlPolygons(context);
        DrawVertices(context);
        DrawContinuityLabels(context);
        DrawConstraintIcons(context);
        
        if (_parallelSource != null)
        {
            var pen = new Pen(Brushes.Orange, 3);
            context.DrawLine(pen, ToPoint(_parallelSource.Start.Position), ToPoint(_parallelSource.End.Position));
        }
    }

    private static Point ToPoint(Vec2 v) => new(v.X, v.Y);

    private Vec2 ToVec2(PointerEventArgs e)
    {
        Point p = e.GetPosition(this);
        return new Vec2(p.X, p.Y);
    }

    private void DrawEdges(ILineDrawer drawer)
    {
        foreach (var polygon in Scene.Polygons)
        {
            foreach (var edge in polygon.Edges)
            {
                if (edge.Shape is BezierShape bezier)
                    BezierDrawer.Draw(drawer, edge.Start.Position, bezier.Control1,
                        bezier.Control2, edge.End.Position);
                else if (edge.Shape is ArcShape && ArcGeometry.Compute(polygon, edge) is Arc arc)
                    ArcDrawer.Draw(drawer, arc);
                else
                    drawer.DrawLine(edge.Start.Position, edge.End.Position);
            }
        }
    }

    private void DrawVertices(DrawingContext context)
    {
        foreach (var polygon in Scene.Polygons)
        {
            foreach (var vertex in polygon.Vertices)
                context.DrawEllipse(Brushes.Black, null, ToPoint(vertex.Position), PointRadius, PointRadius);
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Vec2 pos = ToVec2(e);
        
        var props = e.GetCurrentPoint(this).Properties;
        
        if (props.IsRightButtonPressed)
        {
            ShowContextMenu(pos);
            return;
        }
        
        if (_parallelSource != null)
        {
            var target = HitTester.FindEdge(Scene, pos, EdgeHitTolerance);
            if (target != null && target.Value.Edge != _parallelSource
                               && !target.Value.Edge.SharesVertexWith(_parallelSource))
            {
                var constraint = new ParallelConstraint(_parallelSource, target.Value.Edge);
                Scene.Constraints.Add(constraint);
                if (!ConstraintSolver.Solve(Scene, null))
                    Scene.Constraints.Remove(constraint);
            }
            _parallelSource = null;
            InvalidateVisual();
            return;
        }

        _draggedVertex = HitTester.FindVertex(Scene, pos, VertexHitRadius);
        if (_draggedVertex == null)
            _draggedControl = HitTester.FindControlPoint(Scene, pos, VertexHitRadius);

        if (_draggedVertex == null && _draggedControl == null)
        {
            var edgeHit = HitTester.FindEdge(Scene, pos, EdgeHitTolerance);
            if (edgeHit != null)
            {
                _draggedPolygon = edgeHit.Value.Polygon;
                _lastMouse = pos;
            }
        }

        if (_draggedVertex != null || _draggedControl != null || _draggedPolygon != null)
            e.Pointer.Capture(this);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        Vec2 pos = ToVec2(e);

        if (_draggedVertex != null)
        {
            var hit = _draggedVertex.Value;
            var before = ContinuitySolver.Snapshot(Scene);
            if (ConstraintSolver.MoveVertex(Scene, hit.Vertex, pos))
                ContinuitySolver.FollowVertices(Scene, before);
            else
                hit.Polygon.Translate(pos - hit.Vertex.Position); // already moves control points
            ContinuitySolver.Apply(Scene);
        }
        else if (_draggedControl != null)
        {
            var hit = _draggedControl.Value;
            ContinuitySolver.MoveControlPoint(Scene, hit.Polygon, hit.Edge, hit.Bezier, hit.IsFirst, pos);
        }
        else if (_draggedPolygon != null)
        {
            _draggedPolygon.Translate(pos - _lastMouse);
            _lastMouse = pos;
        }
        else
        {
            return;
        }

        InvalidateVisual();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        e.Pointer.Capture(null);
        _draggedVertex = null;
        _draggedPolygon = null;
        _draggedControl = null;
    }

    private void ShowContextMenu(Vec2 pos)
    {
        _parallelSource = null;
        InvalidateVisual();

        var vertexHit = HitTester.FindVertex(Scene, pos, VertexHitRadius);
        if (vertexHit != null) { ShowVertexMenu(vertexHit.Value); return; }

        var edgeHit = HitTester.FindEdge(Scene, pos, EdgeHitTolerance);
        if (edgeHit != null) ShowEdgeMenu(edgeHit.Value);
    }

    private void ShowEdgeMenu(HitTester.EdgeHit hit)
    {
        var item = new MenuItem { Header = "Add vertex" };
        item.Click += (_, _) =>
        {
            Scene.RemoveConstraintsOf(hit.Edge);
            hit.Polygon.SplitEdge(hit.Edge);
            InvalidateVisual();
        };
        var menu = new ContextMenu();
        menu.Items.Add(item);
        
        var setLength = new MenuItem { Header = "Set length…" };
        setLength.Click += async (_, _) =>
        {
            if (TopLevel.GetTopLevel(this) is not Window owner) return;
            double? length = await new LengthDialog(hit.Edge.Length).ShowDialog<double?>(owner);
            if (length == null) return;

            var old = Scene.Constraints.OfType<FixedLengthConstraint>()
                .FirstOrDefault(f => f.Edge == hit.Edge);
            if (old != null)
                Scene.Constraints.Remove(old);

            var constraint = new FixedLengthConstraint(hit.Edge, length.Value);
            Scene.Constraints.Add(constraint);

            if (!ConstraintSolver.Solve(Scene, null))
            {
                // Impossible length: undo
                Scene.Constraints.Remove(constraint);
                if (old != null)
                    Scene.Constraints.Add(old);
            }
            
            InvalidateVisual();
        };
        
        if (hit.Edge.Shape is LineShape)
            menu.Items.Add(setLength);
        
        var makeParallel = new MenuItem { Header = "Make parallel…" };
        makeParallel.Click += (_, _) =>
        {
            _parallelSource = hit.Edge;
            InvalidateVisual();
        };
        menu.Items.Add(makeParallel);
        
        if (hit.Edge.Shape is not LineShape)
            menu.Items.Add(ShapeItem("Convert to line", hit, () => new LineShape()));
        if (hit.Edge.Shape is not BezierShape)
            menu.Items.Add(ShapeItem("Convert to Bézier", hit, () =>
            {
                Vec2 start = hit.Edge.Start.Position;
                Vec2 dir = hit.Edge.Direction;
                return new BezierShape(start + dir / 3, start + dir * 2 / 3);
            }));
        if (hit.Edge.Shape is not ArcShape)
            menu.Items.Add(ShapeItem("Convert to arc", hit, () => new ArcShape()));

        foreach (var c in Scene.Constraints)
        {
            if (!c.Edges.Contains(hit.Edge))
                continue;

            var remove = new MenuItem { Header = $"Remove {Scene.LabelOf(c)}" };
            remove.Click += (_, _) =>
            {
                Scene.Constraints.Remove(c);
                InvalidateVisual();
            };
            menu.Items.Add(remove);
        }

        menu.Open(this);
    }

    private void ShowVertexMenu(HitTester.VertexHit hit)
    {
        var item = new MenuItem { Header = "Delete vertex" };
        item.IsEnabled = hit.Polygon.Vertices.Count > 3;
        item.Click += (_, _) =>
        {
            var touching = hit.Polygon.Edges
                .Where(e => e.Start == hit.Vertex || e.End == hit.Vertex)
                .ToList();
            foreach (var edge in touching)
                Scene.RemoveConstraintsOf(edge);
            hit.Polygon.RemoveVertex(hit.Vertex);
            InvalidateVisual();
        };
        var menu = new ContextMenu { Items = { item } };

        var inEdge = hit.Polygon.EdgeBefore(hit.Vertex);
        var outEdge = hit.Polygon.EdgeAfter(hit.Vertex);
        bool nearArc = inEdge.Shape is ArcShape || outEdge.Shape is ArcShape;
        if (inEdge.Shape is BezierShape || outEdge.Shape is BezierShape || nearArc)
        {
            foreach (var continuity in new[] { Continuity.G0, Continuity.G1, Continuity.C1 })
            {
                if (nearArc && continuity == Continuity.C1) continue; // arcs: G0/G1 only
                // Between two arcs neither can follow the other: G0 only
                if (inEdge.Shape is ArcShape && outEdge.Shape is ArcShape && continuity != Continuity.G0)
                    continue;
                string mark = hit.Vertex.Continuity == continuity ? "✓ " : "";
                var option = new MenuItem { Header = $"{mark}Continuity {continuity}" };
                option.Click += (_, _) =>
                {
                    hit.Vertex.Continuity = continuity;
                    hit.Vertex.ContinuityIsExplicit = true;
                    if (continuity != Continuity.G0)
                        KeepSingleG1(hit.Polygon, hit.Vertex);
                    ContinuitySolver.Apply(Scene);
                    InvalidateVisual();
                };
                menu.Items.Add(option);
            }
        }

        menu.Open(this);
    }

    private void DrawConstraintIcons(DrawingContext context)
    {
        var count = new Dictionary<Edge, int>();
        foreach (var c in Scene.Constraints)
        {
            string label = Scene.LabelOf(c);

            foreach (var edge in c.Edges)
            {
                int n = count.GetValueOrDefault(edge);
                count[edge] = n + 1;
                Vec2 pos = edge.Midpoint + new Vec2(6, 4 + 14 * n);
                var text = new FormattedText(label, CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.Red);
                context.DrawText(text, ToPoint(pos));
            }
        }
    }
    
    private void DrawControlPolygons(DrawingContext context)
    {
        var dashed = new Pen(Brushes.CornflowerBlue, 1, DashStyle.Dash);

        foreach (var polygon in Scene.Polygons)
        {
            foreach (var edge in polygon.Edges)
            {
                if (edge.Shape is not BezierShape bezier)
                    continue;

                Point p0 = ToPoint(edge.Start.Position);
                Point p1 = ToPoint(bezier.Control1);
                Point p2 = ToPoint(bezier.Control2);
                Point p3 = ToPoint(edge.End.Position);

                context.DrawLine(dashed, p0, p1);
                context.DrawLine(dashed, p1, p2);
                context.DrawLine(dashed, p2, p3);

                context.DrawEllipse(Brushes.RoyalBlue, null, p1, PointRadius, PointRadius);
                context.DrawEllipse(Brushes.RoyalBlue, null, p2, PointRadius, PointRadius);
            }
        }
    }
    
    private void DrawContinuityLabels(DrawingContext context)
    {
        foreach (var polygon in Scene.Polygons)
        foreach (var v in polygon.Vertices)
        {
            if (polygon.EdgeBefore(v).Shape is LineShape &&
                polygon.EdgeAfter(v).Shape is LineShape)
                continue;

            var text = new FormattedText(v.Continuity.ToString(), CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, Typeface.Default, 11, Brushes.RoyalBlue);
            context.DrawText(text, ToPoint(v.Position + new Vec2(8, -18)));
        }
    }
    
    private MenuItem ShapeItem(string header, HitTester.EdgeHit hit, Func<EdgeShape> create)
    {
        var item = new MenuItem { Header = header };
        item.Click += (_, _) =>
        {
            // Length/parallel constraints describe a straight segment, so drop them
            Scene.RemoveConstraintsOf(hit.Edge);
            var shape = create();
            hit.Edge.Shape = shape;

            Vertex start = hit.Edge.Start, end = hit.Edge.End;
            if (shape is BezierShape)
            {
                // Default continuity (G1) unless the user already chose one
                foreach (var v in new[] { start, end })
                    if (!v.ContinuityIsExplicit)
                        v.Continuity = Continuity.G1;
                // A neighbouring arc may now have G1 at both ends
                KeepSingleG1(hit.Polygon, start);
                KeepSingleG1(hit.Polygon, end);
            }
            else if (shape is ArcShape)
            {
                // C1 is not defined for arcs: treat it as G1
                foreach (var v in new[] { start, end })
                    if (v.Continuity == Continuity.C1)
                        v.Continuity = Continuity.G1;

                // Arcs default to G0, and only one end may be G1
                foreach (var v in new[] { start, end })
                    if (!v.ContinuityIsExplicit)
                        v.Continuity = Continuity.G0;
                if (start.Continuity != Continuity.G0 && end.Continuity != Continuity.G0)
                    end.Continuity = Continuity.G0;

                // Between two arcs neither can follow the other: only G0 is possible
                foreach (var v in new[] { start, end })
                    if (hit.Polygon.EdgeBefore(v).Shape is ArcShape &&
                        hit.Polygon.EdgeAfter(v).Shape is ArcShape)
                        v.Continuity = Continuity.G0;
            }

            ContinuitySolver.Apply(Scene);
            InvalidateVisual();
        };
        return item;
    }
    
    // An arc may be G1 at one end only: the opposite end of a neighboring arc becomes G0
    private static void KeepSingleG1(Polygon polygon, Vertex v)
    {
        if (v.Continuity == Continuity.G0) return;
        Edge inEdge = polygon.EdgeBefore(v);
        Edge outEdge = polygon.EdgeAfter(v);
        if (inEdge.Shape is ArcShape) inEdge.Start.Continuity = Continuity.G0;
        if (outEdge.Shape is ArcShape) outEdge.End.Continuity = Continuity.G0;
    }
}
