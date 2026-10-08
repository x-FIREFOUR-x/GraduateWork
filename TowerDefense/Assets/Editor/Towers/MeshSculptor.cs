using System.Collections.Generic;

using UnityEngine;

using Random = System.Random;


namespace TowerDefense.EditorTools.Towers
{
    // Sculpting tools shared by the tower bakers: smooth tubes, rings, sculpted lumps, balls, beams, boxes, rocks and
    // cups, written into one mesh. Colours come from a palette texture of gradient rows: a part names its row by a
    // value of the tower's own palette enum and picks a shade along it, and ToMesh lets every vertex drift along its
    // row by smooth noise, so surfaces show shades rather than one flat paint
    public class MeshSculptor<TPalette> where TPalette : struct, System.Enum
    {
        private readonly Random rng;

        private readonly List<Vector3> vertices = new();
        private readonly List<Vector3> normals = new();
        private readonly List<Vector2> uvs = new();
        private readonly List<int> triangles = new();

        private static readonly float golden = (1 + Mathf.Sqrt(5)) / 2;
        private static readonly Vector3[] icoVertices =
        {
            new(-1, golden, 0), new(1, golden, 0), new(-1, -golden, 0), new(1, -golden, 0),
            new(0, -1, golden), new(0, 1, golden), new(0, -1, -golden), new(0, 1, -golden),
            new(golden, 0, -1), new(golden, 0, 1), new(-golden, 0, -1), new(-golden, 0, 1)
        };
        private static readonly int[] icoFaces =
        {
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, 1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
        };


        // How far the shade of a vertex may drift from the tone it was given, how fine the blotches are, and where
        // in the noise this mesh sits, so two meshes do not repeat the same pattern
        private readonly float shadeNoise;
        private readonly int rampWidth;
        private readonly int paletteRows;
        private readonly float noiseScale;
        private readonly Vector3 noiseOffset;


        // rampWidth and paletteRows describe the palette texture the UVs point into: how many shades a row holds
        // and how many rows (materials) there are
        public MeshSculptor(int seed, int rampWidth, int paletteRows, float shadeNoise = 0.12f, float noiseScale = 6f)
        {
            rng = new Random(seed);
            this.rampWidth = rampWidth;
            this.paletteRows = paletteRows;
            this.shadeNoise = shadeNoise;
            this.noiseScale = noiseScale;
            // Not drawn from rng, so the shapes come out the same as without the noise
            noiseOffset = new Vector3(seed * 1.37f, seed * 2.11f, seed * 0.73f);
        }

        public Mesh ToMesh()
        {
            ShadeVertices();

            // A tower part with many fine details may pass the 65535 vertices a 16 bit index can reach
            Mesh mesh = new Mesh { indexFormat = vertices.Count > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16 };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            return mesh;
        }


        public void Box(Vector3 center, Quaternion rotation, Vector3 size, TPalette palette, float tone)
        {
            Vector3 half = size * 0.5f;
            Vector3 back = center - rotation * Vector3.forward * half.z;
            Vector3 front = center + rotation * Vector3.forward * half.z;

            Prism(back, front, rotation, new Vector2(half.x, half.y), new Vector2(half.x, half.y), palette, tone);
        }

        // Square beam from one point to another; it keeps its top facing up as well as the slope lets it
        public void Beam(Vector3 from, Vector3 to, float width, float height, TPalette palette, float tone)
        {
            TaperedBeam(from, to, width, height, width, height, palette, tone);
        }

        public void TaperedBeam(Vector3 from, Vector3 to, float fromWidth, float fromHeight, float toWidth, float toHeight,
                                TPalette palette, float tone)
        {
            Vector3 direction = (to - from).normalized;
            Vector3 up = Mathf.Abs(Vector3.Dot(direction, Vector3.up)) > 0.99f ? Vector3.forward : Vector3.up;
            Quaternion rotation = Quaternion.LookRotation(direction, up);

            Prism(from, to, rotation, new Vector2(fromWidth, fromHeight) * 0.5f, new Vector2(toWidth, toHeight) * 0.5f, palette, tone);
        }

        // Four sided prism between two rectangles lying across the rotation's forward axis
        private void Prism(Vector3 back, Vector3 front, Quaternion rotation, Vector2 backHalf, Vector2 frontHalf, TPalette palette, float tone)
        {
            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;

            Vector3[] Ring(Vector3 center, Vector2 half) => new[]
            {
                center - right * half.x - up * half.y,
                center + right * half.x - up * half.y,
                center + right * half.x + up * half.y,
                center - right * half.x + up * half.y
            };

            Vector3[] b = Ring(back, backHalf);
            Vector3[] f = Ring(front, frontHalf);
            Vector3 inside = (back + front) * 0.5f;
            float shade = tone + Range(-0.05f, 0.05f);

            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4;
                Polygon(new[] { b[i], b[next], f[next], f[i] }, inside, palette, shade);
            }
            Polygon(b, inside, palette, shade);
            Polygon(f, inside, palette, shade);
        }

        // Round bar around an axis. Smooth sides read as rope or a pole, flat ones as a cut wooden disc
        public void Cylinder(Vector3 center, Vector3 axis, float radius, float length, int sides, TPalette palette, float tone, bool smooth)
        {
            axis = axis.normalized;
            Vector3 across = Vector3.Cross(axis, Mathf.Abs(axis.y) > 0.9f ? Vector3.right : Vector3.up).normalized;
            Vector3 across2 = Vector3.Cross(axis, across);
            Vector3 start = center - axis * length * 0.5f;
            Vector3 end = center + axis * length * 0.5f;

            Vector3[] startRing = new Vector3[sides];
            Vector3[] endRing = new Vector3[sides];
            Vector3[] radial = new Vector3[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = (i + 0.5f) * Mathf.PI * 2 / sides;
                radial[i] = across * Mathf.Cos(angle) + across2 * Mathf.Sin(angle);
                startRing[i] = start + radial[i] * radius;
                endRing[i] = end + radial[i] * radius;
            }

            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                Vector3[] quad = { startRing[i], startRing[next], endRing[next], endRing[i] };

                if (smooth)
                    SmoothQuad(quad, new[] { radial[i], radial[next], radial[next], radial[i] }, palette, tone);
                else
                    Polygon(quad, center, palette, tone + Range(-0.04f, 0.04f));
            }

            Polygon(startRing, center, palette, tone - 0.05f);
            Polygon(endRing, center, palette, tone - 0.05f);
        }

        // Upright ring of stone blocks: every side face gets its own shade, so the blocks show apart.
        // The top gets the tone given plus topShade
        public void Frustum(Vector3 baseCenter, float bottomRadius, float topRadius, float height, int sides,
                            TPalette palette, float tone, float toneJitter, float turnDegrees, float topShade = 0.1f)
        {
            Vector3 top = baseCenter + Vector3.up * height;
            Vector3 inside = baseCenter + Vector3.up * height * 0.5f;
            float turn = turnDegrees * Mathf.Deg2Rad;

            Vector3[] bottomRing = new Vector3[sides];
            Vector3[] topRing = new Vector3[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = turn + i * Mathf.PI * 2 / sides;
                Vector3 direction = new(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                bottomRing[i] = baseCenter + direction * bottomRadius;
                topRing[i] = top + direction * topRadius;
            }

            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                Polygon(new[] { bottomRing[i], bottomRing[next], topRing[next], topRing[i] }, inside, palette,
                        tone + Range(-toneJitter, toneJitter));
            }
            PolygonFacing(topRing, Vector3.up, palette, tone + topShade);
        }

        // Flat slabs lying on a round top: a middle stone and a ring of slabs around it, each a little
        // smaller than its place so a gap is left around it
        public void Paving(Vector3 center, float radius, float innerRadius, int sides, TPalette palette, float tone)
        {
            const float gap = 0.05f;
            float turn = Mathf.PI / sides;

            Vector3 Point(float r, int i) =>
                center + new Vector3(Mathf.Cos(turn + i * Mathf.PI * 2 / sides), 0, Mathf.Sin(turn + i * Mathf.PI * 2 / sides)) * r;

            Vector3[] middle = new Vector3[sides];
            for (int i = 0; i < sides; i++)
                middle[i] = Point(innerRadius - gap, i);
            PolygonFacing(middle, Vector3.up, palette, tone + Range(-0.06f, 0.06f));

            for (int i = 0; i < sides; i++)
            {
                Vector3[] slab = { Point(innerRadius + gap, i), Point(radius, i), Point(radius, i + 1), Point(innerRadius + gap, i + 1) };

                // Pulled in towards its own middle, so the slabs do not touch each other along their sides
                Vector3 slabCenter = Centroid(slab);
                for (int k = 0; k < slab.Length; k++)
                    slab[k] = slabCenter + (slab[k] - slabCenter) * 0.94f;

                PolygonFacing(slab, Vector3.up, palette, tone + Range(-0.1f, 0.1f));
            }
        }

        // Open bucket: outer wall, a darker inner wall, the rim between them and a floor
        public void Cup(Vector3 baseCenter, float bottomRadius, float topRadius, float height, float thickness, int sides,
                        TPalette palette, float tone)
        {
            Vector3 top = baseCenter + Vector3.up * height;

            Vector3[] outerBottom = new Vector3[sides];
            Vector3[] outerTop = new Vector3[sides];
            Vector3[] innerBottom = new Vector3[sides];
            Vector3[] innerTop = new Vector3[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = (i + 0.5f) * Mathf.PI * 2 / sides;
                Vector3 direction = new(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                outerBottom[i] = baseCenter + direction * bottomRadius;
                outerTop[i] = top + direction * topRadius;
                innerBottom[i] = baseCenter + Vector3.up * thickness + direction * (bottomRadius - thickness);
                innerTop[i] = top + direction * (topRadius - thickness);
            }

            Vector3 axisPoint = baseCenter + Vector3.up * height * 0.5f;
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                float shade = tone + Range(-0.05f, 0.05f);
                Vector3 outward = new Vector3(outerTop[i].x + outerTop[next].x, 0, outerTop[i].z + outerTop[next].z) * 0.5f
                                - new Vector3(baseCenter.x, 0, baseCenter.z);

                PolygonFacing(new[] { outerBottom[i], outerBottom[next], outerTop[next], outerTop[i] }, outward, palette, shade);
                PolygonFacing(new[] { innerBottom[i], innerBottom[next], innerTop[next], innerTop[i] }, -outward, palette, shade - 0.25f);
                PolygonFacing(new[] { innerTop[i], innerTop[next], outerTop[next], outerTop[i] }, Vector3.up, palette, shade + 0.1f);
            }

            PolygonFacing(outerBottom, Vector3.down, palette, tone - 0.1f);
            PolygonFacing(innerBottom, Vector3.up, palette, tone - 0.3f);
        }

        // Angular stone: an icosphere pushed around by noise and shaded flat, so every face is visible.
        // It lies on the point given unless centered, then it is built around it
        // toneGradient lightens the faces towards the top of the stone and darkens them towards its foot, by that much
        // from the middle to the top, so ice or stone shades from its underside up rather than in one flat colour
        public void Rock(Vector3 point, float radius, float flatness, int subdivisions, TPalette palette, float tone, bool centered = false,
                         float toneGradient = 0f)
        {
            (Vector3[] sphere, int[] faces) = Icosphere(subdivisions);

            Quaternion rotation = Quaternion.Euler(Range(-15, 15), Range(0, 360), Range(-15, 15));
            Vector3 scale = new(radius * Range(0.9f, 1.15f), radius * flatness, radius * Range(0.9f, 1.15f));
            Vector3 center = centered ? point : point + Vector3.up * scale.y * 0.7f;
            float seed = Range(0, 10);

            Vector3[] points = new Vector3[sphere.Length];
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 direction = sphere[i];
                float shape = 1
                    + 0.18f * Mathf.Sin(direction.x * 2.3f + seed) * Mathf.Cos(direction.z * 2.7f - seed)
                    + 0.12f * Mathf.Sin(direction.y * 3.1f + direction.x * 2.9f + seed * 2);

                Vector3 local = direction * shape;
                points[i] = center + rotation * new Vector3(local.x * scale.x, local.y * scale.y, local.z * scale.z);
            }

            for (int f = 0; f < faces.Length; f += 3)
            {
                Vector3[] face = { points[faces[f]], points[faces[f + 1]], points[faces[f + 2]] };
                float height = (Centroid(face).y - center.y) / Mathf.Max(scale.y, 0.0001f);
                Polygon(face, center, palette, tone + Range(-0.06f, 0.06f) + toneGradient * Mathf.Clamp(height, -1f, 1f));
            }
        }

        // Smooth round lump, squashed by the radii given along each axis
        public void Ball(Vector3 center, Vector3 radii, TPalette palette, float tone)
        {
            (Vector3[] sphere, int[] faces) = Icosphere(2);

            int start = vertices.Count;
            foreach (Vector3 direction in sphere)
            {
                Vector3 normal = new Vector3(direction.x / radii.x, direction.y / radii.y, direction.z / radii.z).normalized;
                AddVertex(center + Vector3.Scale(direction, radii), normal, UV(palette, Mathf.Clamp01(tone + direction.y * 0.15f)));
            }

            foreach (int index in faces)
                triangles.Add(start + index);
        }

        // Drifts the shade of every vertex along its ramp by smooth noise over its position, in three octaves: wood
        // gets streaks, stone gets blotches, cloth looks worn and skin uneven. On top of that the mesh darkens
        // towards its foot, like the shadow near the ground, and faces turned up catch more light.
        // Only the position along the ramp changes, so a vertex keeps its colour, only lighter or darker
        private void ShadeVertices()
        {
            if (shadeNoise <= 0f || vertices.Count == 0)
                return;

            const float footShadow = 0.18f;
            const float upwardLight = 0.1f;

            float lowest = float.MaxValue, highest = float.MinValue;
            foreach (Vector3 vertex in vertices)
            {
                lowest = Mathf.Min(lowest, vertex.y);
                highest = Mathf.Max(highest, vertex.y);
            }

            float first = 0.5f / rampWidth;
            float last = 1f - 0.5f / rampWidth;
            for (int i = 0; i < uvs.Count; i++)
            {
                Vector3 point = vertices[i] * noiseScale + noiseOffset;
                // A broad octave sets whole parts apart from their neighbours, the finer ones mottle each part
                float noise = 0.45f * Noise(point * 0.35f + Vector3.one * 41.7f) + 0.35f * Noise(point) + 0.2f * Noise(point * 2.7f + Vector3.one * 17.3f);

                float tone = Mathf.InverseLerp(first, last, uvs[i].x);
                float height = Mathf.InverseLerp(lowest, highest, vertices[i].y);
                tone = Mathf.Clamp01(tone + (noise - 0.5f) * 2f * shadeNoise + (height - 0.5f) * footShadow + normals[i].y * upwardLight);
                uvs[i] = new Vector2(Mathf.Lerp(first, last, tone), uvs[i].y);
            }
        }

        // Smooth value noise in 0..1: random values on a lattice, blended smoothly between its points
        private static float Noise(Vector3 point)
        {
            int x = Mathf.FloorToInt(point.x), y = Mathf.FloorToInt(point.y), z = Mathf.FloorToInt(point.z);
            Vector3 f = point - new Vector3(x, y, z);
            f = new Vector3(f.x * f.x * (3f - 2f * f.x), f.y * f.y * (3f - 2f * f.y), f.z * f.z * (3f - 2f * f.z));

            float Lattice(int dx, int dy, int dz) => Hash(x + dx, y + dy, z + dz);

            float bottom = Mathf.Lerp(Mathf.Lerp(Lattice(0, 0, 0), Lattice(1, 0, 0), f.x), Mathf.Lerp(Lattice(0, 1, 0), Lattice(1, 1, 0), f.x), f.y);
            float top = Mathf.Lerp(Mathf.Lerp(Lattice(0, 0, 1), Lattice(1, 0, 1), f.x), Mathf.Lerp(Lattice(0, 1, 1), Lattice(1, 1, 1), f.x), f.y);
            return Mathf.Lerp(bottom, top, f.z);
        }

        private static float Hash(int x, int y, int z)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + z * 1274126177;
                h = (h ^ (h >> 13)) * 1274126177;
                return ((h ^ (h >> 16)) & 0xffff) / 65535f;
            }
        }

        // Smooth tube along a curved spine: the spine is smoothed through the points given, the radius follows
        // them, and the ends close in rounded caps. squash narrows the section across the spine, along the
        // side and then the depth of the first ring, for flattened shapes such as boots or a beard.
        // wrinkle ripples the surface into soft folds, for cloth; the shading then follows the folds
        // With toneEnd the shade runs from tone at the start of the tube to toneEnd at its end, as along an icicle
        public void Tube(Vector3[] spine, float[] radii, TPalette palette, float tone, int sides = 12, Vector2? squash = null,
                         bool roundStart = true, bool roundEnd = true, float wrinkle = 0f, float? toneEnd = null)
        {
            const int steps = 4;
            const int capRings = 3;
            Vector2 section = squash ?? Vector2.one;

            List<Vector3> points = new();
            List<float> pointRadii = new();
            for (int i = 0; i < spine.Length - 1; i++)
            {
                Vector3 before = spine[Mathf.Max(i - 1, 0)];
                Vector3 after = spine[Mathf.Min(i + 2, spine.Length - 1)];
                for (int step = 0; step < steps; step++)
                {
                    float t = step / (float)steps;
                    points.Add(CatmullRom(before, spine[i], spine[i + 1], after, t));
                    pointRadii.Add(Mathf.Lerp(radii[i], radii[i + 1], t));
                }
            }
            points.Add(spine[spine.Length - 1]);
            pointRadii.Add(radii[radii.Length - 1]);

            // Frames carried along the spine, so the rings do not twist
            int count = points.Count;
            Vector3[] tangents = new Vector3[count];
            for (int i = 0; i < count; i++)
                tangents[i] = (points[Mathf.Min(i + 1, count - 1)] - points[Mathf.Max(i - 1, 0)]).normalized;

            Vector3 reference = Mathf.Abs(Vector3.Dot(tangents[0], Vector3.right)) > 0.9f ? Vector3.up : Vector3.right;
            Vector3[] sideAxes = new Vector3[count];
            Vector3[] depthAxes = new Vector3[count];
            Vector3 side = (reference - tangents[0] * Vector3.Dot(reference, tangents[0])).normalized;
            for (int i = 0; i < count; i++)
            {
                side = (side - tangents[i] * Vector3.Dot(side, tangents[i])).normalized;
                sideAxes[i] = side;
                depthAxes[i] = Vector3.Cross(tangents[i], side);
            }

            // Every ring: centre, radius, and how far it leans towards a cap, which tilts its normals along the spine
            List<(Vector3 center, float radius, int frame, float lean)> rings = new();
            if (roundStart)
            {
                for (int j = capRings; j >= 1; j--)
                {
                    float angle = j / (float)capRings * Mathf.PI * 0.5f;
                    rings.Add((points[0] - tangents[0] * pointRadii[0] * Mathf.Sin(angle), pointRadii[0] * Mathf.Cos(angle), 0, -Mathf.Sin(angle)));
                }
            }
            for (int i = 0; i < count; i++)
                rings.Add((points[i], pointRadii[i], i, 0f));
            if (roundEnd)
            {
                for (int j = 1; j <= capRings; j++)
                {
                    float angle = j / (float)capRings * Mathf.PI * 0.5f;
                    rings.Add((points[count - 1] + tangents[count - 1] * pointRadii[count - 1] * Mathf.Sin(angle),
                               pointRadii[count - 1] * Mathf.Cos(angle), count - 1, Mathf.Sin(angle)));
                }
            }

            Vector3[,] positions = new Vector3[rings.Count, sides];
            Vector3[,] ringNormals = new Vector3[rings.Count, sides];
            for (int r = 0; r < rings.Count; r++)
            {
                var ring = rings[r];
                Vector3 sideAxis = sideAxes[ring.frame];
                Vector3 depthAxis = depthAxes[ring.frame];
                float lean = ring.lean;

                for (int k = 0; k < sides; k++)
                {
                    float angle = k * Mathf.PI * 2 / sides;
                    float cos = Mathf.Cos(angle);
                    float sin = Mathf.Sin(angle);

                    // Folds run roughly along the tube and fade out over the rounded caps
                    float folds = 1f + wrinkle * (1f - Mathf.Abs(lean)) * (0.6f * Mathf.Sin(5f * angle + r * 0.7f) + 0.4f * Mathf.Sin(3f * angle - r * 1.3f));

                    Vector3 offset = sideAxis * (cos * ring.radius * section.x) + depthAxis * (sin * ring.radius * section.y);
                    Vector3 outward = (sideAxis * (cos / section.x) + depthAxis * (sin / section.y)).normalized;

                    positions[r, k] = ring.center + offset * folds;
                    ringNormals[r, k] = (outward * Mathf.Sqrt(1f - lean * lean) + tangents[ring.frame] * lean).normalized;
                }
            }

            // With folds the normals are taken from the surface itself, so the folds catch the light
            if (wrinkle > 0f)
            {
                for (int r = 0; r < rings.Count; r++)
                {
                    for (int k = 0; k < sides; k++)
                    {
                        Vector3 around = positions[r, (k + 1) % sides] - positions[r, (k + sides - 1) % sides];
                        Vector3 alongSpine = positions[Mathf.Min(r + 1, rings.Count - 1), k] - positions[Mathf.Max(r - 1, 0), k];
                        Vector3 surface = Vector3.Cross(alongSpine, around);
                        if (surface.sqrMagnitude < 1e-12f)
                            continue;

                        surface.Normalize();
                        ringNormals[r, k] = Vector3.Dot(surface, ringNormals[r, k]) < 0 ? -surface : surface;
                    }
                }
            }

            int start = vertices.Count;
            for (int r = 0; r < rings.Count; r++)
            {
                for (int k = 0; k < sides; k++)
                {
                    Vector3 normal = ringNormals[r, k];
                    float ringTone = toneEnd.HasValue ? Mathf.Lerp(tone, toneEnd.Value, r / Mathf.Max(rings.Count - 1f, 1f)) : tone;
                    AddVertex(positions[r, k], normal, UV(palette, Mathf.Clamp01(ringTone + normal.y * 0.12f)));
                }
            }

            for (int r = 0; r < rings.Count - 1; r++)
            {
                for (int k = 0; k < sides; k++)
                {
                    int a = start + r * sides + k;
                    int b = start + r * sides + (k + 1) % sides;
                    int c = start + (r + 1) * sides + (k + 1) % sides;
                    int d = start + (r + 1) * sides + k;

                    SmoothTriangle(a, b, c);
                    SmoothTriangle(a, c, d);
                }
            }
        }

        // Smooth lump sculpted out of an ellipsoid: each bump pushes the surface out (or in, if negative) around
        // a direction from the centre and fades off smoothly with the angle from it, so a nose or a knuckle grows
        // out of the shape instead of sitting on it. toneAt picks the shade for each direction, for shadows and blush
        public void Sculpt(Vector3 center, Vector3 radii, (Vector3 direction, float width, float height)[] bumps, TPalette palette,
                           System.Func<Vector3, float> toneAt, int subdivisions = 3)
        {
            (Vector3[] sphere, int[] faces) = Icosphere(subdivisions);

            Vector3[] points = new Vector3[sphere.Length];
            for (int i = 0; i < sphere.Length; i++)
            {
                float push = 0f;
                foreach (var bump in bumps)
                    push += bump.height * MeshSculpting.Falloff(sphere[i], bump.direction, bump.width);

                points[i] = center + Vector3.Scale(sphere[i] * (1f + push), radii);
            }

            // Normals summed from the faces around each point, so the shading follows the sculpted shape
            Vector3[] pointNormals = new Vector3[sphere.Length];
            for (int f = 0; f < faces.Length; f += 3)
            {
                int a = faces[f], b = faces[f + 1], c = faces[f + 2];
                Vector3 face = Vector3.Cross(points[b] - points[a], points[c] - points[a]);
                if (Vector3.Dot(face, points[a] - center) < 0)
                    face = -face;

                pointNormals[a] += face;
                pointNormals[b] += face;
                pointNormals[c] += face;
            }

            int start = vertices.Count;
            for (int i = 0; i < sphere.Length; i++)
            {
                Vector3 normal = pointNormals[i].normalized;
                AddVertex(points[i], normal, UV(palette, Mathf.Clamp01(toneAt(sphere[i]) + normal.y * 0.08f)));
            }

            for (int f = 0; f < faces.Length; f += 3)
                SmoothTriangle(start + faces[f], start + faces[f + 1], start + faces[f + 2]);
        }

        // Smooth ring around an axis: radii of the ring itself, across the axis, and the thickness of its tube.
        // An arc of it only, when arcDegrees is under 360: it starts at arcStart, measured from the first axis
        // across the ring, and its ends are left open. waviness lets the ring wander along the axis, unevenly,
        // so a belt or a hem sags and rises like a real one instead of reading as a perfect hoop
        public void Torus(Vector3 center, Vector3 axis, Vector2 ringRadii, float thickness, TPalette palette, float tone, int segments = 20, int sides = 8,
                          float arcStart = 0f, float arcDegrees = 360f, float waviness = 0f)
        {
            bool isClosed = arcDegrees >= 360f;
            int ringCount = isClosed ? segments : segments + 1;

            axis = axis.normalized;
            Vector3 across = Vector3.Cross(axis, Mathf.Abs(Vector3.Dot(axis, Vector3.forward)) > 0.9f ? Vector3.up : Vector3.forward).normalized;
            Vector3 across2 = Vector3.Cross(axis, across);

            // across2 runs along the depth for an upright ring, so ringRadii.y squashes it front to back
            int start = vertices.Count;
            for (int s = 0; s < ringCount; s++)
            {
                float angle = (arcStart + s * Mathf.Min(arcDegrees, 360f) / segments) * Mathf.Deg2Rad;
                Vector3 ringPoint = center + across * (Mathf.Cos(angle) * ringRadii.x) + across2 * (Mathf.Sin(angle) * ringRadii.y)
                                  + axis * (waviness * (Mathf.Sin(2f * angle + 0.7f) + 0.5f * Mathf.Sin(3f * angle + 1.9f)));
                Vector3 outward = (across * (Mathf.Cos(angle) / ringRadii.x) + across2 * (Mathf.Sin(angle) / ringRadii.y)).normalized;

                for (int k = 0; k < sides; k++)
                {
                    float tubeAngle = k * Mathf.PI * 2 / sides;
                    Vector3 normal = outward * Mathf.Cos(tubeAngle) + axis * Mathf.Sin(tubeAngle);
                    AddVertex(ringPoint + normal * thickness, normal, UV(palette, Mathf.Clamp01(tone + normal.y * 0.12f)));
                }
            }

            for (int s = 0; s < segments; s++)
            {
                for (int k = 0; k < sides; k++)
                {
                    int a = start + s * sides + k;
                    int b = start + s * sides + (k + 1) % sides;
                    int next = isClosed ? (s + 1) % segments : s + 1;
                    int c = start + next * sides + (k + 1) % sides;
                    int d = start + next * sides + k;

                    SmoothTriangle(a, b, c);
                    SmoothTriangle(a, c, d);
                }
            }
        }

        // Triangle between existing vertices, wound so that its face agrees with their normals. A triangle with
        // no area, where a tube closes into a point, is left out
        private void SmoothTriangle(int a, int b, int c)
        {
            Vector3 face = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
            if (face.sqrMagnitude < 1e-14f)
                return;

            if (Vector3.Dot(face, normals[a] + normals[b] + normals[c]) < 0)
                (b, c) = (c, b);

            triangles.AddRange(new[] { a, b, c });
        }

        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (2f * p1 + (p2 - p0) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (3f * p1 - p0 - 3f * p2 + p3) * t3);
        }

        // Pennant cloth: one triangle seen from both sides
        public void Flag(Vector3 a, Vector3 b, Vector3 c, TPalette palette, float tone)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
            PolygonFacing(new[] { a, b, c }, normal, palette, tone);
            PolygonFacing(new[] { a, b, c }, -normal, palette, tone - 0.15f);
        }


        // Flat face turned away from a point inside the shape
        private void Polygon(Vector3[] points, Vector3 inside, TPalette palette, float tone)
        {
            PolygonFacing(points, Centroid(points) - inside, palette, tone);
        }

        // Flat face whose normal agrees with the facing given. Faces turned up are a little lighter,
        // the way the tile decor shades them. Points go around a convex outline, it is fanned from the first
        private void PolygonFacing(Vector3[] points, Vector3 facing, TPalette palette, float tone)
        {
            Vector3 normal = Normal(points);
            if (Vector3.Dot(normal, facing) < 0)
            {
                // Callers may hand the same outline in again, so it is turned around on a copy
                points = (Vector3[])points.Clone();
                System.Array.Reverse(points);
                normal = -normal;
            }

            Vector2 uv = UV(palette, Mathf.Clamp01(tone + normal.y * 0.15f));

            int start = vertices.Count;
            foreach (Vector3 point in points)
                AddVertex(point, normal, uv);

            for (int i = 1; i < points.Length - 1; i++)
                triangles.AddRange(new[] { start, start + i, start + i + 1 });
        }

        // Quad whose corners carry their own normals, so a round surface does not show its facets
        private void SmoothQuad(Vector3[] points, Vector3[] cornerNormals, TPalette palette, float tone)
        {
            Vector3 facing = cornerNormals[0] + cornerNormals[2];
            if (Vector3.Dot(Normal(points), facing) < 0)
            {
                System.Array.Reverse(points);
                System.Array.Reverse(cornerNormals);
            }

            Vector2 uv = UV(palette, Mathf.Clamp01(tone));

            int start = vertices.Count;
            for (int i = 0; i < points.Length; i++)
                AddVertex(points[i], cornerNormals[i], uv);

            triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
        }

        // Newell's method, also right for a face that is not exactly flat; for a triangle it is Cross(b - a, c - a).
        // Winding the points so that this normal points out of the shape is what Unity draws as the front
        private static Vector3 Normal(Vector3[] points)
        {
            Vector3 normal = Vector3.zero;
            for (int i = 0; i < points.Length; i++)
                normal += Vector3.Cross(points[i], points[(i + 1) % points.Length]);

            return normal.normalized;
        }

        private static Vector3 Centroid(Vector3[] points)
        {
            Vector3 sum = Vector3.zero;
            foreach (Vector3 point in points)
                sum += point;

            return sum / points.Length;
        }

        private static (Vector3[], int[]) Icosphere(int subdivisions)
        {
            List<Vector3> points = new();
            foreach (Vector3 icoVertex in icoVertices)
                points.Add(icoVertex.normalized);

            List<int> faces = new(icoFaces);
            Dictionary<(int, int), int> middles = new();

            for (int step = 0; step < subdivisions; step++)
            {
                List<int> subdivided = new();
                for (int f = 0; f < faces.Count; f += 3)
                {
                    int a = faces[f], b = faces[f + 1], c = faces[f + 2];
                    int ab = Middle(a, b), bc = Middle(b, c), ca = Middle(c, a);

                    subdivided.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
                }

                faces = subdivided;
                middles.Clear();
            }

            return (points.ToArray(), faces.ToArray());

            int Middle(int a, int b)
            {
                (int, int) key = a < b ? (a, b) : (b, a);
                if (middles.TryGetValue(key, out int middle))
                    return middle;

                points.Add(((points[a] + points[b]) * 0.5f).normalized);
                middle = points.Count - 1;
                middles[key] = middle;

                return middle;
            }
        }

        private float Range(float min, float max)
        {
            return min + (float)rng.NextDouble() * (max - min);
        }

        private Vector2 UV(TPalette palette, float t)
        {
            float u = Mathf.Lerp(0.5f / rampWidth, 1 - 0.5f / rampWidth, t);
            float v = (System.Convert.ToInt32(palette) + 0.5f) / paletteRows;
            return new Vector2(u, v);
        }

        private void AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            vertices.Add(position);
            normals.Add(normal);
            uvs.Add(uv);
        }
    }


    // Helpers for sculpting and palettes that do not depend on a tower's palette
    public static class MeshSculpting
    {
        // How strongly a feature centred on one direction shows at another: 1 on it, fading off smoothly with the angle
        public static float Falloff(Vector3 direction, Vector3 featureDirection, float width)
        {
            float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(direction.normalized, featureDirection.normalized), -1f, 1f)) / width;
            return Mathf.Exp(-angle * angle);
        }

        // The same direction on the other side of a face
        public static Vector3 Mirror(Vector3 direction)
        {
            return new Vector3(-direction.x, direction.y, direction.z);
        }

        // Palette texture of rows of rampWidth shades, each row rowHeight pixels high. A ramp may have more than two
        // stops, spread evenly along its row; rows past the last ramp repeat it
        public static Texture2D CreatePalette(Color32[][] ramps, int rampWidth, int paletteRows, int rowHeight)
        {
            Texture2D paletteTexture = new Texture2D(rampWidth, paletteRows * rowHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            for (int row = 0; row < paletteRows; row++)
            {
                Color32[] ramp = ramps[Mathf.Min(row, ramps.Length - 1)];

                for (int x = 0; x < rampWidth; x++)
                {
                    float position = x / (rampWidth - 1f) * (ramp.Length - 1);
                    int stop = Mathf.Min((int)position, ramp.Length - 2);
                    Color color = Color.Lerp(ramp[stop], ramp[stop + 1], position - stop);
                    for (int y = 0; y < rowHeight; y++)
                        paletteTexture.SetPixel(x, row * rowHeight + y, color);
                }
            }
            paletteTexture.Apply(false, false);

            return paletteTexture;
        }
    }

}
