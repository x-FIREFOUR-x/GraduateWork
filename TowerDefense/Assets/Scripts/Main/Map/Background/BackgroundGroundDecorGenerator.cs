using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

using Random = System.Random;


namespace TowerDefense.Main.Map.Background
{
    // Low-poly details standing on the background ground, built the same way as the ones baked onto
    // TowerTile and BlockedTile: grass tufts and blades, stones, dandelion-like flowers, boulders with
    // broadleaf trees and stone spires. The ground outside the tile grid is cut into
    // tile-sized cells and each decorated cell gets one tile-like arrangement, standing on the terrain height
    public static class BackgroundGroundDecorGenerator
    {
        private const float detailScale = 3.1f;
        private const float crownScale = 1.27f;

        private const int boulderVariants = 6;
        private const int cliffVariants = 3;
        private const int blockedVariants = boulderVariants + cliffVariants;

        // Share of cells that carry an obstacle arrangement (rocks, trees, spires) outside the forests
        private const float obstacleShare = 0.06f;

        // Trees come in big groups: a slow noise over the ground marks forests, and inside one almost
        // every cell is a tree cell. forestFrequency is per world unit, so a forest spans several cells
        private const float forestFrequency = 0.03f;
        private const float forestThreshold = 0.52f;
        private const float forestObstacleShare = 0.85f;

        // Chance that a cell is decorated at all: full next to the tile grid, thinning out with distance
        private const float nearCellChance = 1f;
        private const float farCellChance = 0.45f;

        private enum Palette { Grass, DryGrass, Stem, Stone, Pebble, Petal, FlowerCenter, Bark, Foliage, Rock, DarkFoliage, Cliff }

        private const int rampWidth = 32;
        private const int paletteRows = 16;


        public static GameObject Generate(Transform parent, Material material, Vector3 fieldCenter, float gridHalfWidth, Vector3 tileSize,
                                          float maxDistance, Func<float, float, float> groundHeight, int seed)
        {
            Builder builder = new(tileSize, groundHeight);

            Random noiseRng = new(seed);
            float forestOffsetX = noiseRng.Next(0, 10000);
            float forestOffsetZ = noiseRng.Next(0, 10000);

            int tilesPerSide = Mathf.RoundToInt(gridHalfWidth * 2f / tileSize.x);
            float firstTileX = fieldCenter.x - gridHalfWidth + tileSize.x * 0.5f;
            float firstTileZ = fieldCenter.z - gridHalfWidth + tileSize.z * 0.5f;
            int ring = Mathf.CeilToInt(maxDistance / Mathf.Min(tileSize.x, tileSize.z));

            for (int i = -ring; i < tilesPerSide + ring; i++)
            {
                for (int j = -ring; j < tilesPerSide + ring; j++)
                {
                    if (i >= 0 && i < tilesPerSide && j >= 0 && j < tilesPerSide)
                        continue;

                    float cellX = firstTileX + i * tileSize.x;
                    float cellZ = firstTileZ + j * tileSize.z;

                    float dx = Mathf.Max(Mathf.Abs(cellX - fieldCenter.x) - gridHalfWidth, 0f);
                    float dz = Mathf.Max(Mathf.Abs(cellZ - fieldCenter.z) - gridHalfWidth, 0f);
                    float edgeDistance = Mathf.Sqrt(dx * dx + dz * dz);
                    if (edgeDistance > maxDistance)
                        continue;

                    Random cellRng = new(unchecked(seed * 73856093 ^ i * 19349663 ^ j * 83492791));
                    float forestNoise = Mathf.PerlinNoise((cellX + forestOffsetX) * forestFrequency, (cellZ + forestOffsetZ) * forestFrequency);
                    bool forest = forestNoise > forestThreshold;

                    // Forests are not thinned with distance, or the group would fall apart into stray trees
                    float chance = Mathf.Lerp(nearCellChance, farCellChance, edgeDistance / maxDistance);
                    if (!forest && cellRng.NextDouble() > chance)
                        continue;

                    bool obstacle = cellRng.NextDouble() < (forest ? forestObstacleShare : obstacleShare);
                    int variant = cellRng.Next(0, blockedVariants);

                    if (forest)
                        variant = cellRng.Next(0, boulderVariants);

                    float detail = forest ? 1f : Mathf.Lerp(1f, 0.45f, edgeDistance / maxDistance);
                    builder.BuildCell(cellX, cellZ, cellRng, obstacle, variant, forest, detail);
                }
            }

            GameObject decor = new GameObject("BackgroundGroundDecor");
            decor.transform.SetParent(parent, false);

            decor.AddComponent<MeshFilter>().sharedMesh = builder.ToMesh();

            MeshRenderer meshRenderer = decor.AddComponent<MeshRenderer>();
            if (material != null)
                meshRenderer.sharedMaterial = material;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

            return decor;
        }


        private class Builder
        {
            private readonly Vector3 size;
            private readonly Func<float, float, float> groundHeight;

            // Sized for a typical map up front: growing these lists by doubling copies hundreds of thousands of entries
            private readonly List<Vector3> vertices = new(250000);
            private readonly List<Vector3> normals = new(250000);
            private readonly List<Vector2> uvs = new(250000);
            private readonly List<int> triangles = new(750000);

            private Random rng;
            private float cellX;
            private float cellZ;
            private int variant;
            private bool forest;
            private float detail = 1f;

            private static readonly float golden = (1 + Mathf.Sqrt(5)) / 2;
            private static readonly Vector3[] icoVertices =
            {
                new(-1, golden, 0), new(1, golden, 0), new(-1, -golden, 0), new(1, -golden, 0),
                new(0, -1, golden), new(0, 1, golden), new(0, -1, -golden), new(0, 1, -golden),
                new(golden, 0, -1), new(golden, 0, 1), new(-golden, 0, -1), new(-golden, 0, 1)
            };
            private static Vector3[] smoothSphereVertices;
            private static int[] smoothSphereFaces;

            private static readonly int[] icoFaces =
            {
                0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, 1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
                3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
            };


            public Builder(Vector3 size, Func<float, float, float> groundHeight)
            {
                this.size = size;
                this.groundHeight = groundHeight;
            }

            public void BuildCell(float cellX, float cellZ, Random cellRng, bool obstacle, int variant, bool forest, float detail)
            {
                this.cellX = cellX;
                this.cellZ = cellZ;
                this.variant = variant;
                this.forest = forest;
                this.detail = detail;
                rng = cellRng;

                if (obstacle)
                    BuildObstacleCell();
                else
                    BuildGrassCell();
            }

            // Distant cells get fewer filler blades: they are small on screen and the ground behind is the same green
            private int ScaledCount(int count)
            {
                return Mathf.Max(1, Mathf.RoundToInt(count * detail));
            }

            public Mesh ToMesh()
            {
                Mesh mesh = new Mesh();
                if (vertices.Count > 65000)
                    mesh.indexFormat = IndexFormat.UInt32;
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uvs);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateBounds();

                // The decor never changes after this, so the CPU-side copy of the mesh is not kept
                mesh.UploadMeshData(true);

                return mesh;
            }


            private void BuildGrassCell()
            {
                List<Vector2> placed = new();

                int tufts = rng.Next(2, 4);
                for (int i = 0; i < tufts; i++)
                {
                    if (TryPoint(0.14f, 0.22f, placed, out Vector2 p))
                        Tuft(p, 1f);
                }

                if (rng.NextDouble() < 0.5 && TryPoint(0.16f, 0.2f, placed, out Vector2 stonePoint))
                {
                    Stone(Surface(stonePoint), Range(0.28f, 0.45f), Palette.Stone, Range(0.55f, 0.75f));

                    int pebbles = rng.Next(1, 3);
                    for (int i = 0; i < pebbles; i++)
                    {
                        Vector2 offset = RandomDirection() * Range(0.07f, 0.11f);
                        Stone(Surface(stonePoint + offset), Range(0.09f, 0.14f), Palette.Stone, Range(0.5f, 0.7f));
                    }
                }

                if (rng.NextDouble() < 0.55 && TryPoint(0.15f, 0.2f, placed, out Vector2 flowerPoint))
                {
                    int flowers = rng.Next(1, 4);
                    for (int i = 0; i < flowers; i++)
                    {
                        Vector2 offset = i == 0 ? Vector2.zero : RandomDirection() * Range(0.04f, 0.07f);
                        Flower(Surface(flowerPoint + offset), Range(0.4f, 0.6f));
                    }
                }

                for (int i = 0, count = ScaledCount(40); i < count; i++)
                    ShortBlade(new Vector2(Range(0.06f, 0.94f), Range(0.06f, 0.94f)));
            }

            private void BuildObstacleCell()
            {
                Vector2[] spots =
                {
                    new(0.5f, 0.5f), new(0.24f, 0.26f), new(0.76f, 0.27f), new(0.25f, 0.75f), new(0.75f, 0.74f)
                };

                if (variant < boulderVariants)
                    BuildBoulders(spots);
                else
                    BuildCliffs(spots);
            }

            private float ThemeHeightScale()
            {
                return (variant % 3) switch
                {
                    1 => 1.25f,
                    2 => 0.82f,
                    _ => 1f
                };
            }

            private void BuildBoulders(Vector2[] spots)
            {
                for (int i = 0; i < spots.Length; i++)
                {
                    Vector2 spot = spots[i] + new Vector2(Range(-0.03f, 0.03f), Range(-0.03f, 0.03f));
                    float room = RoomForObstacle(spot);

                    // In a forest the boulder cells are almost all trees, a rock only now and then
                    bool isRock = forest ? rng.NextDouble() < 0.12 : i == 0 || rng.NextDouble() < 0.6;
                    if (isRock)
                        Rock(Surface(spot), Mathf.Min(Range(1.1f, 2.2f), room), Range(0.55f, 0.85f), room);
                    else
                        Tree(Surface(spot), Range(3.2f, 4.2f) * TreeHeightScale(), Range(0.22f, 0.3f), room);
                }

                int tufts = rng.Next(1, 3);
                List<Vector2> placed = new();
                for (int i = 0; i < tufts; i++)
                {
                    if (TryPoint(0.16f, 0.2f, placed, out Vector2 p))
                        Tuft(p, 0.9f);
                }

                for (int i = 0, count = ScaledCount(30); i < count; i++)
                    ShortBlade(new Vector2(Range(0.06f, 0.94f), Range(0.06f, 0.94f)));
            }

            private void BuildCliffs(Vector2[] spots)
            {
                for (int i = 0; i < spots.Length; i++)
                {
                    Vector2 spot = spots[i] + new Vector2(Range(-0.03f, 0.03f), Range(-0.03f, 0.03f));
                    float room = RoomForObstacle(spot);

                    float height = (i == 0 ? Range(3.4f, 4.6f) : Range(1.4f, 2.5f)) * ThemeHeightScale();
                    float radius = i == 0 ? Range(1.2f, 1.7f) : Range(0.6f, 1.1f);

                    Cliff(Surface(spot), height, radius, room);
                }

                List<Vector2> placed = new();
                int scree = rng.Next(4, 7);
                for (int i = 0; i < scree; i++)
                {
                    if (TryPoint(0.12f, 0.15f, placed, out Vector2 p))
                        Stone(Surface(p), Range(0.12f, 0.26f), Palette.Rock, Range(0.4f, 0.65f));
                }

                for (int i = 0, count = ScaledCount(16); i < count; i++)
                    ShortBlade(new Vector2(Range(0.06f, 0.94f), Range(0.06f, 0.94f)));
            }

            private float TreeHeightScale()
            {
                return variant switch
                {
                    1 => 1.5f,
                    3 => 1.33f,
                    _ => 1f
                };
            }

            private float RoomForObstacle(Vector2 spot)
            {
                const float border = 0.2f;

                float toBorderX = size.x * (0.5f - Mathf.Abs(spot.x - 0.5f));
                float toBorderZ = size.z * (0.5f - Mathf.Abs(spot.y - 0.5f));

                return Mathf.Max(Mathf.Min(toBorderX, toBorderZ) - border, 0.3f);
            }

            private void Tree(Vector3 basePoint, float height, float trunkRadius, float room)
            {
                Trunk(basePoint, height, trunkRadius, trunkRadius * 0.7f);

                int crowns = rng.Next(2, 4);
                for (int i = 0; i < crowns; i++)
                {
                    float level = height * Range(0.7f, 0.95f) + i * height * 0.22f;
                    float shift = Mathf.Min(0.25f, room * 0.2f);
                    Vector3 center = basePoint + Vector3.up * level + new Vector3(Range(-shift, shift), 0, Range(-shift, shift));

                    float radius = Mathf.Min(height * Range(0.42f, 0.52f), room - shift) * crownScale;

                    Blob(center, radius, Palette.DarkFoliage, Range(0.85f, 1f), 1);
                }
            }

            private void Trunk(Vector3 basePoint, float height, float bottomRadius, float topRadius)
            {
                const int sides = 12;
                float barkTone = Range(0.35f, 0.7f);
                Vector3 top = basePoint + Vector3.up * height;

                for (int i = 0; i < sides; i++)
                {
                    float angle = i * Mathf.PI * 2 / sides;
                    float nextAngle = (i + 1) * Mathf.PI * 2 / sides;
                    Vector3 side = new(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                    Vector3 nextSide = new(Mathf.Cos(nextAngle), 0, Mathf.Sin(nextAngle));

                    AddSmoothQuad(basePoint + side * bottomRadius, basePoint + nextSide * bottomRadius,
                                  top + nextSide * topRadius, top + side * topRadius,
                                  side, nextSide, UV(Palette.Bark, barkTone));
                }
            }

            private void Cliff(Vector3 basePoint, float height, float radius, float room)
            {
                const int sides = 16;
                int levels = rng.Next(5, 8);
                float tone = Range(0.35f, 0.6f);
                float turn = Range(0, Mathf.PI * 2);

                radius = Mathf.Min(radius, room / 1.2f);

                Vector3 lean = new Vector3(Range(-0.35f, 0.35f), 0, Range(-0.35f, 0.35f)) * radius;

                float[] ringRadius = new float[levels + 1];
                float[] ringTop = new float[levels + 1];
                float rises = 0;

                ringRadius[0] = radius;
                for (int level = 1; level <= levels; level++)
                {
                    float ledge = rng.NextDouble() < 0.34 ? Range(0.26f, 0.42f) : Range(0.02f, 0.1f);
                    ringRadius[level] = Mathf.Max(ringRadius[level - 1] * (1 - ledge), radius * 0.16f);

                    ringTop[level] = ringTop[level - 1] + Range(0.6f, 1.5f);
                    rises = ringTop[level];
                }

                Vector3[] points = new Vector3[(levels + 1) * sides];
                Vector3[] ringCenters = new Vector3[levels + 1];
                for (int level = 0; level <= levels; level++)
                {
                    float t = ringTop[level] / rises;
                    Vector3 center = basePoint + Vector3.up * (height * t) + lean * t * t;

                    for (int i = 0; i < sides; i++)
                    {
                        float angle = turn + i * Mathf.PI * 2 / sides;
                        Vector3 direction = new(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                        points[level * sides + i] = center + direction * ringRadius[level] * Range(0.94f, 1.07f);
                    }
                }

                FitInside(points, basePoint, room);

                for (int level = 0; level <= levels; level++)
                {
                    Vector3 sum = Vector3.zero;
                    for (int i = 0; i < sides; i++)
                        sum += points[level * sides + i];

                    ringCenters[level] = sum / sides;
                }

                Vector3 inside = basePoint + Vector3.up * height * 0.5f;
                for (int level = 0; level < levels; level++)
                {
                    Vector2 uv = UV(Palette.Cliff, Mathf.Clamp01(tone + level / (float)levels * 0.2f));
                    Vector3 lower = ringCenters[level];
                    Vector3 upper = ringCenters[level + 1];

                    for (int i = 0; i < sides; i++)
                    {
                        int next = (i + 1) % sides;
                        Vector3 a = points[level * sides + i];
                        Vector3 b = points[level * sides + next];
                        Vector3 c = points[(level + 1) * sides + next];
                        Vector3 d = points[(level + 1) * sides + i];

                        AddSmoothQuad(a, b, c, d, BandNormal(a, d, lower, upper), BandNormal(b, c, lower, upper), uv);
                    }
                }

                int topRing = levels * sides;
                Vector3 peak = ringCenters[levels] + Vector3.up * height * Range(0.04f, 0.12f);

                for (int i = 0; i < sides; i++)
                    AddFlatTriangle(peak, points[topRing + i], points[topRing + (i + 1) % sides], inside, Palette.Cliff, tone);
            }

            private void Blob(Vector3 basePoint, float radius, Palette palette, float flatness, float sink = 0.1f)
            {
                BuildSmoothSphere();
                Quaternion rotation = Quaternion.AngleAxis(Range(0, 360), Vector3.up);
                Vector3 center = basePoint + Vector3.up * radius * flatness * (1 - sink);
                float tone = Range(0.4f, 0.65f);
                float seed = Range(0, 10);

                int start = vertices.Count;
                foreach (Vector3 spherePoint in smoothSphereVertices)
                {
                    float bumps = 1 + 0.12f * Mathf.Sin(spherePoint.x * 4 + seed) * Mathf.Cos(spherePoint.z * 5 - seed)
                                    + 0.08f * Mathf.Sin(spherePoint.y * 7 + seed);

                    Vector3 local = spherePoint * bumps;
                    local = new Vector3(local.x * radius, local.y * radius * flatness, local.z * radius);

                    Vector3 normal = rotation * new Vector3(spherePoint.x, spherePoint.y / Mathf.Max(flatness, 0.1f), spherePoint.z).normalized;
                    AddVertex(center + rotation * local, normal, UV(palette, Mathf.Clamp01(tone + spherePoint.y * 0.25f)));
                }

                foreach (int index in smoothSphereFaces)
                    triangles.Add(start + index);
            }

            private void Rock(Vector3 basePoint, float radius, float flatness, float room)
            {
                BuildSmoothSphere();

                Quaternion rotation = Quaternion.Euler(Range(-12, 12), Range(0, 360), Range(-12, 12));
                Vector3 scale = new(radius * Range(0.85f, 1.25f), radius * flatness, radius * Range(0.85f, 1.25f));
                Vector3 center = basePoint + Vector3.up * scale.y * 0.45f;
                float tone = Range(0.4f, 0.6f);
                float seed = Range(0, 10);

                Vector3[] points = new Vector3[smoothSphereVertices.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    Vector3 direction = smoothSphereVertices[i];

                    float shape = 1
                        + 0.3f * Mathf.Sin(direction.x * 2.3f + seed) * Mathf.Cos(direction.z * 2.7f - seed)
                        + 0.2f * Mathf.Sin(direction.y * 3.1f + direction.x * 2.9f + seed * 2)
                        + 0.12f * Mathf.Cos(direction.z * 5.3f + direction.y * 4.7f - seed);

                    Vector3 local = direction * shape;
                    points[i] = center + rotation * new Vector3(local.x * scale.x, local.y * scale.y, local.z * scale.z);
                }

                FitInside(points, new Vector3(center.x, basePoint.y, center.z), room);

                for (int f = 0; f < smoothSphereFaces.Length; f += 3)
                {
                    AddFlatTriangle(points[smoothSphereFaces[f]], points[smoothSphereFaces[f + 1]], points[smoothSphereFaces[f + 2]],
                                    center, Palette.Rock, tone);
                }
            }

            private static void FitInside(Vector3[] points, Vector3 pivot, float room)
            {
                float extent = 0;
                foreach (Vector3 point in points)
                    extent = Mathf.Max(extent, Mathf.Max(Mathf.Abs(point.x - pivot.x), Mathf.Abs(point.z - pivot.z)));

                if (extent <= room || extent <= 0)
                    return;

                float factor = room / extent;
                for (int i = 0; i < points.Length; i++)
                    points[i] = pivot + (points[i] - pivot) * factor;
            }

            private static void BuildSmoothSphere()
            {
                if (smoothSphereVertices != null)
                    return;

                List<Vector3> points = new();
                foreach (Vector3 icoVertex in icoVertices)
                    points.Add(icoVertex.normalized);

                List<int> faces = new(icoFaces);
                Dictionary<(int, int), int> middles = new();

                for (int step = 0; step < 2; step++)
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

                smoothSphereVertices = points.ToArray();
                smoothSphereFaces = faces.ToArray();

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

            private void AddSmoothQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normalAd, Vector3 normalBc, Vector2 uv)
            {
                int start = vertices.Count;
                AddVertex(a, normalAd, uv);
                AddVertex(b, normalBc, uv);
                AddVertex(c, normalBc, uv);
                AddVertex(d, normalAd, uv);

                AddTriangle(start, start + 2, start + 1);
                AddTriangle(start, start + 3, start + 2);
            }

            private static Vector3 Outward(Vector3 point, Vector3 center)
            {
                Vector3 direction = new(point.x - center.x, 0, point.z - center.z);
                return direction.sqrMagnitude > 1e-8f ? direction.normalized : Vector3.forward;
            }

            private static Vector3 BandNormal(Vector3 bottom, Vector3 top, Vector3 bottomCenter, Vector3 topCenter)
            {
                Vector3 outward = Outward(bottom, bottomCenter);
                float bottomReach = new Vector2(bottom.x - bottomCenter.x, bottom.z - bottomCenter.z).magnitude;
                float topReach = new Vector2(top.x - topCenter.x, top.z - topCenter.z).magnitude;
                float rise = Mathf.Max(top.y - bottom.y, 0.001f);

                return new Vector3(outward.x * rise, bottomReach - topReach, outward.z * rise).normalized;
            }

            private void AddFlatTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 inside, Palette palette, float tone)
            {
                Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
                if (Vector3.Dot(normal, (a + b + c) / 3 - inside) < 0)
                {
                    (b, c) = (c, b);
                    normal = -normal;
                }

                Vector2 uv = UV(palette, Mathf.Clamp01(tone + normal.y * 0.3f));

                int start = vertices.Count;
                AddVertex(a, normal, uv);
                AddVertex(b, normal, uv);
                AddVertex(c, normal, uv);
                AddTriangle(start, start + 1, start + 2);
            }

            private void Tuft(Vector2 center, float scale)
            {
                int blades = rng.Next(7, 11);
                for (int i = 0; i < blades; i++)
                {
                    Vector2 direction = RandomDirection();
                    float offset = Range(0, 0.1f);
                    Vector3 basePoint = Surface(center + direction * offset * detailScale / size.x);

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

            private void Blade(Vector3 basePoint, Vector2 lean, float height, float width, float leanAmount, Palette palette, float tone)
            {
                height *= detailScale;
                width *= detailScale;

                Vector3 leanDirection = new Vector3(lean.x, 0, lean.y).normalized;
                Vector3 side = Quaternion.AngleAxis(Range(-40, 40), Vector3.up) * new Vector3(-leanDirection.z, 0, leanDirection.x);

                Vector3 mid = basePoint + Vector3.up * height * 0.5f + leanDirection * height * leanAmount * 0.25f;
                Vector3 tip = basePoint + Vector3.up * height + leanDirection * height * leanAmount;

                int start = vertices.Count;
                AddVertex(basePoint - side * width * 0.5f, Vector3.up, UV(palette, 0.05f));
                AddVertex(basePoint + side * width * 0.5f, Vector3.up, UV(palette, 0.05f));
                AddVertex(mid - side * width * 0.32f, Vector3.up, UV(palette, 0.35f + 0.25f * tone));
                AddVertex(mid + side * width * 0.32f, Vector3.up, UV(palette, 0.35f + 0.25f * tone));
                AddVertex(tip, Vector3.up, UV(palette, 0.7f + 0.3f * tone));

                AddDoubleSided(start, start + 2, start + 3);
                AddDoubleSided(start, start + 3, start + 1);
                AddDoubleSided(start + 2, start + 4, start + 3);
            }

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
                    AddTriangle(start, start + 1, start + 2);
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
                float x = cellX + (p.x - 0.5f) * size.x;
                float z = cellZ + (p.y - 0.5f) * size.z;
                return new Vector3(x, groundHeight(x, z), z);
            }

            private bool TryPoint(float margin, float spacing, List<Vector2> placed, out Vector2 point)
            {
                for (int attempt = 0; attempt < 40; attempt++)
                {
                    point = new Vector2(Range(margin, 1 - margin), Range(margin, 1 - margin));
                    if (FarFrom(point, placed, spacing))
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
                float v = ((int)palette + 0.5f) / paletteRows;
                return new Vector2(u, v);
            }

            private void AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
            {
                vertices.Add(position);
                normals.Add(normal);
                uvs.Add(uv);
            }

            private void AddTriangle(int a, int b, int c)
            {
                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);
            }

            private void AddDoubleSided(int a, int b, int c)
            {
                AddTriangle(a, b, c);
                AddTriangle(a, c, b);
            }
        }
    }
}
