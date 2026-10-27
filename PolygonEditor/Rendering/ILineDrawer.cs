using PolygonEditor.Geometry;

namespace PolygonEditor.Rendering;

/// <summary>Draws a line segment; implemented by the library and the Bresenham drawer.</summary>
public interface ILineDrawer
{
    void DrawLine(Vec2 from, Vec2 to);
}