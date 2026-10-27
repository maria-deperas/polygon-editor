using System;
using PolygonEditor.Geometry;

namespace PolygonEditor.Rendering;

/// <summary>Draws segments pixel by pixel with the integer Bresenham algorithm (all octants).</summary>
public sealed class BresenhamLineDrawer(PixelBuffer buffer) : ILineDrawer
{
    public void DrawLine(int x0, int y0, int x1, int y1, uint color)
    {
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int x = x0, y = y0;
        buffer.SetPixel(x, y, color);

        if (dx >= dy) // gentle: x changes every step
        {
            int d = 2 * dy - dx;
            int incrE = 2 * dy;
            int incrNE = 2 * (dy - dx);
            while (x != x1)
            {
                if (d < 0) d += incrE; // choose E
                else { d += incrNE; y += sy; } // choose NE
                x += sx;
                buffer.SetPixel(x, y, color);
            }
        }
        else // steep: y changes every step
        {
            int d = 2 * dx - dy;
            int incrN = 2 * dx;
            int incrNE = 2 * (dx - dy);
            while (y != y1)
            {
                if (d < 0) d += incrN; // choose N
                else { d += incrNE; x += sx; } // choose NE
                y += sy;
                buffer.SetPixel(x, y, color);
            }
        }
    }

    public void DrawLine(Vec2 from, Vec2 to)
    {
        DrawLine((int)Math.Round(from.X), (int)Math.Round(from.Y), (int)Math.Round(to.X), (int)Math.Round(to.Y), 0xFF000000);
    }
}