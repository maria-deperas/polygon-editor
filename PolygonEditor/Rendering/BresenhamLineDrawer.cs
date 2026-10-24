using System;
using PolygonEditor.Geometry;

namespace PolygonEditor.Rendering;

public sealed class BresenhamLineDrawer(PixelBuffer buffer) : ILineDrawer
{
    public void DrawLine(int x0, int y0, int x1, int y1, uint color)
    {
        int dx =  Math.Abs(x1 - x0);
        int dy = -Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;
        
        while (true)
        {
            buffer.SetPixel(x0, y0, color);
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }   // step in x
            if (e2 <= dx) { err += dx; y0 += sy; }   // step in y
        }
    }

    public void DrawLine(Vec2 from, Vec2 to)
    {
        DrawLine((int)Math.Round(from.X), (int)Math.Round(from.Y), (int)Math.Round(to.X), (int)Math.Round(to.Y), 0xFF000000);
    }
}