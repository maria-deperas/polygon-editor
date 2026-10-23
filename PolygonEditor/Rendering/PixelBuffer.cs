using System;

namespace PolygonEditor.Rendering;

public sealed class PixelBuffer(int width, int height)
{
    public int Width { get; } = width;
    public int Height { get; } = height;
    public uint[] Pixels { get; } = new uint[width * height];

    public void SetPixel(int x, int y, uint color)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return;

        Pixels[y * Width + x] = color;
    }

    public void Clear(uint color)
    {
        Array.Fill(Pixels, color);
    }
}