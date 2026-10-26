# Polygon Editor – Documentation

Click the "Help" button to open this window at any time.

## 1. User guide

### Mouse

- Left button + drag on a vertex: moves the vertex. Constraints and continuity are kept;
  other vertices and control points move if needed. If the constraints cannot be satisfied
  for the new position, the whole polygon is moved instead, so the vertex always stays
  under the cursor.
- Left button + drag on a Bézier control point (blue point): moves the control point.
  Its own segment's vertices stay in place; a neighbouring line segment may rotate or
  stretch to keep G1/C1 continuity.
- Left button + drag on an edge: moves the whole polygon.
- Right button on an edge: edge menu.
    - Add vertex – inserts a vertex in the middle of the edge.
    - Set length… – fixed length constraint (line segments only). The dialog shows the
      current length as the default value.
    - Make parallel… – then left-click a second edge (in any polygon) to make the two
      edges parallel. The first edge is highlighted in orange while waiting.
      A right click cancels the selection.
    - Convert to line / Convert to Bézier / Convert to arc – changes the edge shape.
    - Remove L=…/Remove ∥n – removes a constraint of this edge.
- Right button on a vertex: vertex menu.
    - Delete vertex – disabled when the polygon has only 3 vertices.
    - Continuity G0 / G1 / C1 – shown only when the vertex touches a Bézier segment
      or an arc. The current value is marked with ✓.

### Keyboard

- The editor is operated with the mouse only; there are no keyboard shortcuts.
  In the length dialog Enter confirms and Esc cancels.

### Toolbar

- Library Algorithm / Bresenham Algorithm – chooses how line segments are drawn
  (Avalonia's DrawLine or my own Bresenham implementation into a pixel buffer).
  Bézier curves and arcs are drawn as many short segments, so they use the chosen
  algorithm as well. Helper elements (vertices, control polygon, labels, the orange
  highlight) are always drawn with the library.

### What is shown on the scene

- Black points – vertices.
- Blue points and dashed blue lines – Bézier control points and the Bézier control polygon.
- Blue labels next to vertices – continuity class (only at vertices next to a curve).
- Red labels next to the middle of an edge – constraints: `L=120` is a fixed length,
  `∥1`, `∥2`, … mark parallel pairs (both edges of a pair have the same number).

## 2. Assumptions

- Constraints (fixed length, parallel) are defined for straight segments only.
  Converting an edge to a Bézier segment or an arc removes its constraints.
- Two edges that share a vertex cannot be made parallel (they would have to lie on one line).
- Adding or deleting a vertex removes the constraints of the edges it touches.
  The new edges are always straight segments, even if the old edge was a curve.
- A new constraint that cannot be satisfied (e.g. it contradicts other constraints) is
  not added. A vertex drag that cannot be satisfied moves the whole polygon instead.
- Default continuity is G1 (surname A–K). When an edge becomes a Bézier segment, its
  vertices get G1 unless the user has already chosen their continuity explicitly.
- Arcs support only G0 and G1. When an edge becomes an arc, C1 at its ends is changed to G1,
  and ends without an explicit choice get G0.
- Only one end of an arc can have G1. Choosing G1 at the other end switches the first end to G0.
- At a vertex between two arcs only G0 is possible (neither arc can follow the other).
- An arc has no control points: its circle is computed from its vertices and the neighbouring
  edge (see 4.4), so it simply follows its neighbours.
- Dragging a control point never moves the vertices of its own segment. With G1/C1 it may
  move the far vertex of a neighbouring straight segment; if that vertex is held by
  constraints, the neighbour is left as it is.

## 3. Data model

- `Vec2` – immutable 2D vector with the usual operators, `Dot`, `Cross`, `Length`, `Normalized`.
- `Scene` – list of `Polygon`s and a list of `Constraint`s. Constraints live in the scene,
  not in a polygon, because a parallel constraint can join edges of two different polygons.
- `Polygon` – ordered lists of `Vertex` and `Edge`; edge i goes from vertex i to vertex i+1
  (the last edge closes the polygon). Self-intersections are allowed.
  Operations: `Translate`, `SplitEdge`, `RemoveVertex`, `EdgeBefore`, `EdgeAfter`.
- `Vertex` – `Position`, `Continuity` (G0/G1/C1) and `ContinuityIsExplicit`
  (true once the user, or the demo scene, chose the continuity).
- `Edge` – references its `Start` and `End` vertices and has a `Shape`. It also computes
  `Direction`, `Length`, `Midpoint` and the tangent vectors at both ends.
- `EdgeShape` – what the edge looks like between its vertices:
    - `LineShape` – a straight segment,
    - `BezierShape` – cubic Bézier with two control points `Control1`, `Control2`
      (the curve is Start, Control1, Control2, End),
    - `ArcShape` – a circular arc; it stores nothing, the circle is computed by `ArcGeometry`.
      Changing an edge's shape replaces only this object; the vertices stay the same.
- `Constraint` (abstract) – `Edges`, `IsSatisfied()`, `Project(pinned)`, `GetLabel()`.
  Implementations: `FixedLengthConstraint(edge, length)`, `ParallelConstraint(first, second)`.
- `DemoScene` – builds the startup scene; adding a polygon takes a few lines
  (create it from points, add it to `scene.Polygons`, then set shapes, continuity and constraints).

Code layout: `Model` (data), `Geometry` (math types), `Constraints` (solvers),
`Rendering` (drawing algorithms), `Editing` (hit testing), `Views` (UI).
The model does not depend on the UI.

## 4. Algorithms

### 4.1 Constraint solver (`ConstraintSolver`)

Iterative projection (relaxation):

1. Save the positions of all vertices.
2. Repeat up to 100 times: if every constraint is satisfied (tolerance 1e-6), stop with success.
   Otherwise call `Project` on every constraint in turn. `Project` moves the vertices of its
   edges as little as possible so that this one constraint holds, possibly breaking another
   one, which is fixed in the next pass.
3. If the constraints are still not satisfied, restore the saved positions and report failure.

The pinned vertex is the vertex dragged by the user. Constraints never move it, so it stays
under the cursor; the other end of its edge is moved instead.

- Fixed length: the error `|AB| - L` is corrected along the edge direction – fully on the
  free end if one end is pinned, otherwise half at each end.
- Parallel: the target direction is the direction of the edge with the pinned vertex,
  or the average of both unit directions if neither is pinned (the second direction is
  flipped first if the edges point in opposite ways, so they are not forced to turn around).
  Each edge is then rotated to the target direction, keeping its length, around the pinned
  vertex or around its midpoint.

Moving a vertex = set the new position, run the solver; if it fails, the old position is
restored and the polygon is translated instead.
Adding a constraint = add it, run the solver; if it fails, the constraint is removed.

### 4.2 Continuity (`ContinuitySolver`)

Tangent vectors at the ends of an edge: a line has `B - A` at both ends; a Bézier segment
has `3(P1 - P0)` at the start and `3(P3 - P2)` at the end.

- `Apply` – for every vertex with G1 or C1 next to a Bézier segment, the control point
  next to the vertex is placed on the tangent line of the other edge:
    - G1: same direction, the arm keeps its current length,
    - C1: same tangent vector, so the arm is `tangent / 3`.
      Vertices are never moved here, so the edge constraints stay satisfied.
      The control point being dragged is locked; the one on the other side is fitted instead.
- After a vertex drag, the control points next to each moved vertex are moved by the same
  vector (`FollowVertices`), then `Apply` restores continuity.
- Dragging a control point next to a straight segment with G1/C1: the far vertex of that
  segment is moved (through the constraint solver) so the segment lies on the extension of
  the control arm. For C1 the segment length becomes 3 × arm length.

### 4.3 Bézier drawing (`BezierDrawer`) – incremental algorithm

1. Change to the power basis: `P(t) = A t³ + B t² + C t + D`, where
   `A = -P0 + 3P1 - 3P2 + P3`, `B = 3P0 - 6P1 + 3P2`, `C = 3(P1 - P0)`, `D = P0`.
2. Choose the number of steps n from the length of the control polygon (about 5 px per step),
   step h = 1/n.
3. Forward differences: `Δ1 = A h³ + B h² + C h`, `Δ2 = 6A h³ + 2B h²`, `Δ3 = 6A h³`.
4. Starting at `P0`, each next point is `point + Δ1`, then `Δ1 += Δ2`, `Δ2 += Δ3` –
   only additions per point. Consecutive points are joined with the selected line algorithm;
   the last point is set exactly to `P3`.

### 4.4 Arcs (`ArcGeometry`, `ArcDrawer`)

- G1 at the start A (neighbour edge is not an arc): the circle passes through A and B and is
  tangent at A to the tangent t of the previous edge. The centre lies on the normal n at A:
  `C = A + r·n`, with signed radius `r = |B - A|² / (2 n·(B - A))`. The sweep direction is the
  one that leaves A along t. G1 at the end B is the same construction from B backwards.
  If B lies on the tangent line, the arc degenerates and a straight segment is drawn.
- G0 at both ends: a half circle with the centre in the middle of the edge and radius |AB| / 2.
- Drawing: the arc is divided into about 5 px long pieces; points `C + r(cos φ, sin φ)` are
  joined with the selected line algorithm.

### 4.5 Bresenham line (`BresenhamLineDrawer`)

Integer midpoint algorithm for all octants: `dx = |x1 - x0|`, `dy = -|y1 - y0|`, error
`err = dx + dy`. In each step the pixel is set and, depending on `2·err`, x and/or y is advanced
and the error updated. Pixels go to a `PixelBuffer`, which is copied to a bitmap.

### 4.6 Hit testing (`HitTester`)

Priority: vertex (radius 8 px), then control point, then edge (5 px). Distance to a straight
edge is the distance to a segment; a curve is approximated by a polyline of sample points.
