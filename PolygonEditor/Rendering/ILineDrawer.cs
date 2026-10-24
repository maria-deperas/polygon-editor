using PolygonEditor.Geometry;

namespace PolygonEditor.Rendering;

public interface ILineDrawer
{
    void DrawLine(Vec2 from, Vec2 to);
}