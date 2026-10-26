# Polygon Editor

Computer Graphics, project 1

Polygon editor with geometric constraints, cubic Bézier segments and circular arcs.
Fixed length and parallel edges, default continuity G1.

## Features

- Moving vertices, Bézier control points and whole polygons; adding and deleting vertices
- Fixed length and parallel constraints (also between different polygons)
- Edges as line segments, cubic Bézier segments or circular arcs, G0/G1/C1 continuity
- Line drawing: library or own Bresenham implementation
- Incremental Bézier drawing (power basis + forward differences)

## Running

Requires the .NET 10 SDK.

    dotnet run --project PolygonEditor

## Documentation

[PolygonEditor/Docs/Documentation.md](PolygonEditor/Docs/Documentation.md),
also available in the app via the Help button.