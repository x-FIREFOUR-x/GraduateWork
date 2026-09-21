using System;
using System.Collections.Generic;

using UnityEngine;

using Random = System.Random;


namespace TowerDefense.EditorTools.Tile
{
    // Low-poly 3D details standing on a tile: grass blades and tufts, stones, pebbles and flowers.
    // Meshes are in world units relative to the tile top center; colors come from a gradient palette texture.
    public static class TileDecorBuilder
    {
        // Size multiplier for every detail (blades, stones, flowers); positions are not affected
        private const float detailScale = 3.1f;

        // Keeps the middle of a tower tile free for the tower, in tile space
        private const float towerClearRadius = 0.3f;

        private enum Palette { Grass, DryGrass, Stem, Stone, Pebble, Petal, FlowerCenter }

        private static readonly Color32[][] paletteRamps =
        {
            new Color32[] { new(38, 104, 66, 255), new(160, 222, 132, 255) },
            new Color32[] { new(128, 124, 60, 255), new(222, 208, 130, 255) },
            new Color32[] { new(40, 100, 62, 255), new(110, 170, 100, 255) },
            new Color32[] { new(70, 76, 86, 255), new(205, 210, 216, 255) },
            new Color32[] { new(110, 70, 40, 255), new(230, 178, 122, 255) },
            new Color32[] { new(220, 170, 60, 255), new(255, 246, 176, 255) },
            new Color32[] { new(180, 70, 40, 255), new(250, 150, 90, 255) },
        };
        private const int rampWidth = 32;
        private const int rowHeight = 4;


        public static Texture2D CreatePalette()
        {
            int rows = paletteRamps.Length;
            Texture2D paletteTexture = new Texture2D(rampWidth, rows * rowHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            for (int row = 0; row < rows; row++)
            {
                for (int x = 0; x < rampWidth; x++)
                {
                    Color color = Color.Lerp(paletteRamps[row][0], paletteRamps[row][1], x / (rampWidth - 1f));
                    for (int y = 0; y < rowHeight; y++)
                        paletteTexture.SetPixel(x, row * rowHeight + y, color);
                }
            }
            paletteTexture.Apply(false, false);

            return paletteTexture;
        }

        public static Mesh Build(int bitMaskForPathsConnections, int variant, Vector3 tileSize)
        {
            return new Builder(bitMaskForPathsConnections, variant, tileSize).Build();
        }


        private class Builder
        {
            private readonly int bitMaskForPathsConnections;
            private readonly Vector3 size;
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


            public Builder(int bitMaskForPathsConnections, int variant, Vector3 size)
            {
                this.bitMaskForPathsConnections = bitMaskForPathsConnections;
                this.size = size;
                rng = new Random(bitMaskForPathsConnections * 7919 + variant * 104729 + 17);
            }

            public Mesh Build()
            {
                if (bitMaskForPathsConnections == TileShape.NoPath)
                    BuildGrassTile();
                else
                    BuildPathTile();

                Mesh mesh = new Mesh();
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uvs);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateBounds();

                return mesh;
            }


            private void BuildGrassTile()
            {
                List<Vector2> placed = new();
                Func<Vector2, bool> awayFromTower = q => (q - new Vector2(0.5f, 0.5f)).magnitude > towerClearRadius;

                int tufts = rng.Next(2, 4);
                for (int i = 0; i < tufts; i++)
                {
                    if (TryPoint(0.14f, 0.22f, placed, awayFromTower, out Vector2 p))
                        Tuft(p, 1f);
                }

                if (rng.NextDouble() < 0.5 && TryPoint(0.16f, 0.2f, placed, awayFromTower, out Vector2 stonePoint))
                {
                    Stone(Surface(stonePoint), Range(0.28f, 0.45f), Palette.Stone, Range(0.55f, 0.75f));

                    int pebbles = rng.Next(1, 3);
                    for (int i = 0; i < pebbles; i++)
                    {
                        Vector2 offset = RandomDirection() * Range(0.07f, 0.11f);
                        Stone(Surface(stonePoint + offset), Range(0.09f, 0.14f), Palette.Stone, Range(0.5f, 0.7f));
                    }
                }

                if (rng.NextDouble() < 0.45 && TryPoint(0.15f, 0.2f, placed, awayFromTower, out Vector2 flowerPoint))
                {
                    int flowers = rng.Next(1, 4);
                    for (int i = 0; i < flowers; i++)
                    {
                        Vector2 offset = i == 0 ? Vector2.zero : RandomDirection() * Range(0.04f, 0.07f);
                        Flower(Surface(flowerPoint + offset), Range(0.4f, 0.6f));
                    }
                }

                for (int i = 0; i < 40; i++)
                {
                    Vector2 p = new(Range(0.06f, 0.94f), Range(0.06f, 0.94f));
                    if ((p - new Vector2(0.5f, 0.5f)).magnitude < towerClearRadius * 0.75f)
                        continue;

                    ShortBlade(p);
                }
            }

            private void BuildPathTile()
            {
                List<Vector2> placed = new();

                // Grass leaning over the path border
                int clumps = 0;
                for (int attempt = 0; attempt < 300 && clumps < 14; attempt++)
                {
                    Vector2 p = new(Range(0.07f, 0.93f), Range(0.07f, 0.93f));
                    float distance = TileShape.PathDistance(p.x, p.y, bitMaskForPathsConnections);
                    if (distance < 0.005f || distance > 0.04f || !FarFrom(p, placed, 0.09f))
                        continue;

                    placed.Add(p);
                    clumps++;

                    Vector2 towardPath = -PathGradient(p);
                    int blades = rng.Next(2, 5);
                    for (int i = 0; i < blades; i++)
                    {
                        Vector2 bladePoint = p + RandomDirection() * Range(0, 0.015f);
                        Vector2 lean = Rotate(towardPath, Range(-35, 35));
                        Blade(Surface(bladePoint), lean, Range(0.3f, 0.5f), Range(0.09f, 0.13f), Range(0.45f, 0.8f), Palette.Grass, Range(0.3f, 1f));
                    }
                }

                // Pebbles on the path, away from the middle where buildings stand
                int pebbles = rng.Next(2, 4);
                for (int i = 0; i < pebbles; i++)
                {
                    if (TryPoint(0.08f, 0.12f, placed, q => TileShape.PathDistance(q.x, q.y, bitMaskForPathsConnections) < -0.1f
                                                          && (q - new Vector2(0.5f, 0.5f)).magnitude > 0.24f, out Vector2 p))
                    {
                        Stone(Surface(p), Range(0.1f, 0.16f), Palette.Pebble, Range(0.45f, 0.65f), 0.25f);
                    }
                }

                if (rng.NextDouble() < 0.5 && TryPoint(0.14f, 0.15f, placed, q => TileShape.PathDistance(q.x, q.y, bitMaskForPathsConnections) > 0.14f, out Vector2 tuftPoint))
                    Tuft(tuftPoint, 0.8f);

                for (int i = 0; i < 16; i++)
                {
                    Vector2 p = new(Range(0.06f, 0.94f), Range(0.06f, 0.94f));
                    if (TileShape.PathDistance(p.x, p.y, bitMaskForPathsConnections) > 0.06f)
                        ShortBlade(p);
                }
            }


            private void Tuft(Vector2 center, float scale)
            {
                int blades = rng.Next(7, 11);
                for (int i = 0; i < blades; i++)
                {
                    Vector2 direction = RandomDirection();
                    float offset = Range(0, 0.1f);
                    Vector3 basePoint = Surface(center) + new Vector3(direction.x, 0, direction.y) * offset * detailScale;

                    float height = Range(0.5f, 0.8f) * (1 - 2.5f * offset) * scale;
                    Blade(basePoint, Rotate(direction, Range(-25, 25)), height, Range(0.12f, 0.16f) * scale, Range(0.2f, 0.5f),
                          rng.NextDouble() < 0.12 ? Palette.DryGrass : Palette.Grass, Range(0.4f, 1f));
                }
            }

            private void ShortBlade(Vector2 p)
            {
                Blade(Surface(p), RandomDirection(), Range(0.18f, 0.35f), Range(0.07f, 0.09f), Range(0.1f, 0.35f),
                      rng.NextDouble() < 0.1 ? Palette.DryGrass : Palette.Grass, Range(0.2f, 0.9f));
            }

            // Tapered, slightly bent blade, visible from both sides
            private void Blade(Vector3 basePoint, Vector2 lean, float height, float width, float leanAmount, Palette palette, float tone)
            {
                height *= detailScale;
                width *= detailScale;

                Vector3 leanDirection = new Vector3(lean.x, 0, lean.y).normalized;
                Vector3 side = Quaternion.AngleAxis(Range(-40, 40), Vector3.up) * new Vector3(-leanDirection.z, 0, leanDirection.x);

                Vector3 Point(float t) => basePoint + Vector3.up * height * t + leanDirection * height * leanAmount * t * t;

                int start = vertices.Count;
                AddVertex(Point(0) - side * width * 0.5f, Vector3.up, UV(palette, 0.05f));
                AddVertex(Point(0) + side * width * 0.5f, Vector3.up, UV(palette, 0.05f));
                AddVertex(Point(0.5f) - side * width * 0.32f, Vector3.up, UV(palette, 0.35f + 0.25f * tone));
                AddVertex(Point(0.5f) + side * width * 0.32f, Vector3.up, UV(palette, 0.35f + 0.25f * tone));
                AddVertex(Point(1), Vector3.up, UV(palette, 0.7f + 0.3f * tone));

                AddDoubleSided(start, start + 2, start + 3);
                AddDoubleSided(start, start + 3, start + 1);
                AddDoubleSided(start + 2, start + 4, start + 3);
            }

            // Jittered, flattened icosahedron with flat shading
            private void Stone(Vector3 basePoint, float radius, Palette palette, float flatness, float sink = 0.1f)
            {
                radius *= detailScale;

                Quaternion rotation = Quaternion.AngleAxis(Range(0, 360), Vector3.up);
                Vector3 center = basePoint + Vector3.up * radius * flatness * (0.5f - sink);
                float tone = Range(0.35f, 0.6f);

                Vector3[] points = new Vector3[icoVertices.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    Vector3 local = icoVertices[i].normalized * Range(0.8f, 1.15f);
                    local = new Vector3(local.x * radius, local.y * radius * flatness, local.z * radius);
                    points[i] = center + rotation * local;
                }

                for (int f = 0; f < icoFaces.Length; f += 3)
                {
                    Vector3 a = points[icoFaces[f]];
                    Vector3 b = points[icoFaces[f + 1]];
                    Vector3 c = points[icoFaces[f + 2]];

                    Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
                    if (Vector3.Dot(normal, (a + b + c) / 3 - center) < 0)
                    {
                        (b, c) = (c, b);
                        normal = -normal;
                    }

                    Vector2 uv = UV(palette, Mathf.Clamp01(tone + normal.y * 0.2f + Range(-0.08f, 0.08f)));

                    int start = vertices.Count;
                    AddVertex(a, normal, uv);
                    AddVertex(b, normal, uv);
                    AddVertex(c, normal, uv);
                    triangles.AddRange(new[] { start, start + 1, start + 2 });
                }
            }

            private void Flower(Vector3 basePoint, float height)
            {
                Vector2 lean = RandomDirection();
                float leanAmount = Range(0.05f, 0.2f);
                Blade(basePoint, lean, height, 0.05f, leanAmount, Palette.Stem, 0.6f);

                float stemHeight = height * detailScale;
                Vector3 head = basePoint + Vector3.up * stemHeight + new Vector3(lean.x, 0, lean.y) * stemHeight * leanAmount;
                float radius = Range(0.13f, 0.17f) * detailScale;
                float turn = Range(0, 360);

                int center = vertices.Count;
                AddVertex(head + Vector3.up * 0.03f, Vector3.up, UV(Palette.Petal, 0.95f));

                const int rimCount = 10;
                for (int i = 0; i < rimCount; i++)
                {
                    bool outer = i % 2 == 0;
                    Vector3 direction = Quaternion.AngleAxis(turn + i * 360f / rimCount, Vector3.up) * Vector3.forward;
                    AddVertex(head + direction * radius * (outer ? 1 : 0.45f) - Vector3.up * (outer ? 0.02f : 0),
                              Vector3.up, UV(Palette.Petal, outer ? 0.55f : 0.8f));
                }

                for (int i = 0; i < rimCount; i++)
                    AddDoubleSided(center, center + 1 + i, center + 1 + (i + 1) % rimCount);

                Stone(head + Vector3.up * 0.015f, radius * 0.35f / detailScale, Palette.FlowerCenter, 1f, 0);
            }


            private Vector3 Surface(Vector2 p)
            {
                return new Vector3((p.x - 0.5f) * size.x, TileShape.Height(p.x, p.y, bitMaskForPathsConnections) * size.y, (p.y - 0.5f) * size.z);
            }

            private Vector2 PathGradient(Vector2 p)
            {
                const float e = 0.01f;
                Vector2 gradient = new(TileShape.PathDistance(p.x + e, p.y, bitMaskForPathsConnections) - TileShape.PathDistance(p.x - e, p.y, bitMaskForPathsConnections),
                                       TileShape.PathDistance(p.x, p.y + e, bitMaskForPathsConnections) - TileShape.PathDistance(p.x, p.y - e, bitMaskForPathsConnections));
                return gradient.sqrMagnitude > 1e-8f ? gradient.normalized : RandomDirection();
            }

            private bool TryPoint(float margin, float spacing, List<Vector2> placed, Func<Vector2, bool> isAllowed, out Vector2 point)
            {
                for (int attempt = 0; attempt < 40; attempt++)
                {
                    point = new Vector2(Range(margin, 1 - margin), Range(margin, 1 - margin));
                    if (isAllowed(point) && FarFrom(point, placed, spacing))
                    {
                        placed.Add(point);
                        return true;
                    }
                }

                point = Vector2.zero;
                return false;
            }

            private static bool FarFrom(Vector2 point, List<Vector2> placed, float spacing)
            {
                foreach (Vector2 other in placed)
                {
                    if ((other - point).magnitude < spacing)
                        return false;
                }

                return true;
            }

            private float Range(float min, float max)
            {
                return min + (float)rng.NextDouble() * (max - min);
            }

            private Vector2 RandomDirection()
            {
                float angle = Range(0, Mathf.PI * 2);
                return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            }

            private static Vector2 Rotate(Vector2 vector, float degrees)
            {
                float radians = degrees * Mathf.Deg2Rad;
                float cos = Mathf.Cos(radians);
                float sin = Mathf.Sin(radians);
                return new Vector2(vector.x * cos - vector.y * sin, vector.x * sin + vector.y * cos);
            }

            private static Vector2 UV(Palette palette, float t)
            {
                float u = Mathf.Lerp(0.5f / rampWidth, 1 - 0.5f / rampWidth, t);
                float v = ((int)palette + 0.5f) / paletteRamps.Length;
                return new Vector2(u, v);
            }

            private void AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
            {
                vertices.Add(position);
                normals.Add(normal);
                uvs.Add(uv);
            }

            private void AddDoubleSided(int a, int b, int c)
            {
                triangles.AddRange(new[] { a, b, c, a, c, b });
            }
        }
    }

}
