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

        // Size multiplier for tree crowns. Crowns hang in the air above the tile, so unlike trunks and rocks
        // they may reach past the tile border; raise it for leafier trees, lower it to keep them inside
        private const float crownScale = 1.27f;

        // Keeps the middle of a tower tile free for the tower, in tile space
        private const float towerClearRadius = 0.3f;

        // How much of the margin the tile border keeps a fir crown may eat into, and how much clear air two
        // crowns keep between them. Both are deliberately small: a crown still ends up inside its own tile,
        // and a negative gap only lets two of them brush past each other rather than grow into one mass
        private const float crownReach = 1.16f;
        private const float crownGap = -0.1f;

        // How many blocked tile variants each obstacle theme gets; the baker asks for BlockedVariants of them
        private const int boulderVariants = 6;
        private const int cliffVariants = 3;
        private const int coniferVariants = 3;

        public const int BlockedVariants = boulderVariants + cliffVariants + coniferVariants;

        // What stands on a blocked tile: rounded rocks with broadleaf trees, a stone outcrop or a fir grove
        private enum BlockedTheme { Boulders, Cliffs, Conifers }

        private enum Palette { Grass, DryGrass, Stem, Stone, Pebble, Petal, FlowerCenter, Bark, Foliage, Rock, DarkFoliage, Cliff, Needle }

        private static readonly Color32[][] paletteRamps =
        {
            new Color32[] { new(38, 104, 66, 255), new(160, 222, 132, 255) },
            new Color32[] { new(128, 124, 60, 255), new(222, 208, 130, 255) },
            new Color32[] { new(40, 100, 62, 255), new(110, 170, 100, 255) },
            new Color32[] { new(70, 76, 86, 255), new(205, 210, 216, 255) },
            new Color32[] { new(110, 70, 40, 255), new(230, 178, 122, 255) },
            new Color32[] { new(220, 170, 60, 255), new(255, 246, 176, 255) },
            new Color32[] { new(180, 70, 40, 255), new(250, 150, 90, 255) },
            new Color32[] { new(74, 50, 30, 255), new(156, 112, 70, 255) },
            new Color32[] { new(26, 78, 48, 255), new(112, 178, 96, 255) },
            new Color32[] { new(56, 61, 68, 255), new(142, 149, 158, 255) },
            new Color32[] { new(24, 70, 40, 255), new(80, 140, 76, 255) },
            new Color32[] { new(52, 55, 62, 255), new(178, 182, 190, 255) },
            new Color32[] { new(18, 58, 46, 255), new(92, 148, 104, 255) },
        };
        private const int rampWidth = 32;
        private const int rowHeight = 4;
        // Fixed, so adding a color never shifts the rows meshes were baked with
        private const int paletteRows = 16;


        public static Texture2D CreatePalette()
        {
            Texture2D paletteTexture = new Texture2D(rampWidth, paletteRows * rowHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            for (int row = 0; row < paletteRows; row++)
            {
                Color32[] ramp = paletteRamps[Mathf.Min(row, paletteRamps.Length - 1)];

                for (int x = 0; x < rampWidth; x++)
                {
                    Color color = Color.Lerp(ramp[0], ramp[1], x / (rampWidth - 1f));
                    for (int y = 0; y < rowHeight; y++)
                        paletteTexture.SetPixel(x, row * rowHeight + y, color);
                }
            }
            paletteTexture.Apply(false, false);

            return paletteTexture;
        }

        public static Mesh Build(int bitMaskForPathsConnections, int variant, Vector3 tileSize, bool isBlocked = false)
        {
            return new Builder(bitMaskForPathsConnections, variant, tileSize, isBlocked).Build();
        }


        private class Builder
        {
            private readonly int bitMaskForPathsConnections;
            private readonly int variant;
            private readonly bool isBlocked;
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
            // Icosphere with smooth normals: rocks and crowns look round, not faceted
            private static Vector3[] smoothSphereVertices;
            private static int[] smoothSphereFaces;

            private static readonly int[] icoFaces =
            {
                0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, 1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
                3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
            };


            public Builder(int bitMaskForPathsConnections, int variant, Vector3 size, bool isBlocked)
            {
                this.bitMaskForPathsConnections = bitMaskForPathsConnections;
                this.variant = variant;
                this.isBlocked = isBlocked;
                this.size = size;
                rng = new Random(bitMaskForPathsConnections * 7919 + variant * 104729 + (isBlocked ? 5471 : 17));
            }

            public Mesh Build()
            {
                if (isBlocked)
                    BuildBlockedTile();
                else if (bitMaskForPathsConnections == TileShape.NoPath)
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

            // Obstacles a tower can not be built on. They cover the whole tile but stay inside it.
            // Which obstacles stand on the tile is decided by the variant, so the map mixes groves and outcrops
            private void BuildBlockedTile()
            {
                // Spots go from the middle outwards, the middle one carries the biggest obstacle
                Vector2[] spots =
                {
                    new(0.5f, 0.5f), new(0.24f, 0.26f), new(0.76f, 0.27f), new(0.25f, 0.75f), new(0.75f, 0.74f)
                };

                switch (Theme())
                {
                    case BlockedTheme.Cliffs:
                        BuildCliffs(spots);
                        break;

                    case BlockedTheme.Conifers:
                        BuildConifers(spots);
                        break;

                    default:
                        BuildBoulders(spots);
                        break;
                }
            }

            private BlockedTheme Theme()
            {
                if (variant < boulderVariants)
                    return BlockedTheme.Boulders;

                return variant < boulderVariants + cliffVariants ? BlockedTheme.Cliffs : BlockedTheme.Conifers;
            }

            // Height per variant inside a theme, so tiles of one theme are not copies of each other
            private float ThemeHeightScale()
            {
                // The parentheses matter: a switch expression binds tighter than %, so without them
                // this reads as variant % (3 switch ...), which is 0 for every variant
                return (variant % 3) switch
                {
                    1 => 1.25f,
                    2 => 0.82f,
                    _ => 1f
                };
            }

            // Which of the fir groves this tile is: 0, 1, 2
            private int GroveIndex => variant - boulderVariants - cliffVariants;

            // How a fir grove is laid out per variant: how many firs stand on the tile, how far out from the
            // middle they stand, and how big they grow. Standing further out leaves more open ground but
            // costs crown width, because the tile border is what a crown runs into first; pulling the ring
            // in trades that border room for room between the firs
            private (int Firs, float Spread, float Scale) Grove()
            {
                return GroveIndex switch
                {
                    0 => (3, 0.96f, 0.95f),
                    1 => (4, 0.92f, 1.3f),
                    _ => (3, 1f, 1.22f)
                };
            }

            // Round rocks with broadleaf trees between them
            private void BuildBoulders(Vector2[] spots)
            {
                for (int i = 0; i < spots.Length; i++)
                {
                    Vector2 spot = spots[i] + new Vector2(Range(-0.03f, 0.03f), Range(-0.03f, 0.03f));
                    float room = RoomForObstacle(spot);

                    bool isRock = i == 0 || rng.NextDouble() < 0.6;
                    if (isRock)
                        Rock(Surface(spot), Mathf.Min(Range(1.1f, 2.2f), room), Range(0.55f, 0.85f), room);
                    else
                        Tree(Surface(spot), Range(3.2f, 4.2f) * TreeHeightScale(), Range(0.22f, 0.3f), room);
                }

                int tufts = rng.Next(1, 3);
                List<Vector2> placed = new();
                for (int i = 0; i < tufts; i++)
                {
                    if (TryPoint(0.16f, 0.2f, placed, q => true, out Vector2 p))
                        Tuft(p, 0.9f);
                }

                for (int i = 0; i < 30; i++)
                    ShortBlade(new Vector2(Range(0.06f, 0.94f), Range(0.06f, 0.94f)));
            }

            // A stone outcrop: one tall spire in the middle, lower ones around it, scree at their feet
            private void BuildCliffs(Vector2[] spots)
            {
                for (int i = 0; i < spots.Length; i++)
                {
                    Vector2 spot = spots[i] + new Vector2(Range(-0.03f, 0.03f), Range(-0.03f, 0.03f));
                    float room = RoomForObstacle(spot);

                    // The middle spire carries the silhouette, the outer ones are stumpier
                    float height = (i == 0 ? Range(3.4f, 4.6f) : Range(1.4f, 2.5f)) * ThemeHeightScale();
                    float radius = i == 0 ? Range(1.2f, 1.7f) : Range(0.6f, 1.1f);

                    Cliff(Surface(spot), height, radius, room);
                }

                // Broken stone gathers where the spires meet the ground
                List<Vector2> placed = new();
                int scree = rng.Next(4, 7);
                for (int i = 0; i < scree; i++)
                {
                    if (TryPoint(0.12f, 0.15f, placed, q => true, out Vector2 p))
                        Stone(Surface(p), Range(0.12f, 0.26f), Palette.Rock, Range(0.4f, 0.65f));
                }

                // Bare rock keeps far less grass than a grove
                for (int i = 0; i < 16; i++)
                    ShortBlade(new Vector2(Range(0.06f, 0.94f), Range(0.06f, 0.94f)));
            }

            // A fir grove: firs stand clear of each other, with a lower obstacle filling the middle
            private void BuildConifers(Vector2[] spots)
            {
                (int firs, float spread, float scale) = Grove();

                List<Vector2> standing = new();
                List<float> crowns = new();

                // Firs need more elbow room than boulders, so their ring of spots is pushed outwards.
                // The outer ones go up first; whatever room is left in the middle decides what stands there
                // The outer spots are stored corner by corner, not around the tile, so a grove that uses only
                // some of them walks this ring instead: two firs end up opposite, three spread over three sides
                int[] ring = { 1, 2, 4, 3 };

                Vector2 center = new(0.5f, 0.5f);
                for (int i = 0; i < firs; i++)
                {
                    Vector2 spot = center + (spots[ring[i * ring.Length / firs]] - center) * spread
                                 + new Vector2(Range(-0.02f, 0.02f), Range(-0.02f, 0.02f));

                    Vector3 basePoint = Surface(spot);
                    float room = Mathf.Min(RoomForObstacle(spot) * crownReach, RoomBetween(basePoint, standing, crowns, crownGap));

                    crowns.Add(Conifer(basePoint, Range(3.4f, 4.8f) * scale, Range(0.16f, 0.22f), room));
                    standing.Add(new Vector2(basePoint.x, basePoint.z));
                }

                Vector2 middle = spots[0] + new Vector2(Range(-0.04f, 0.04f), Range(-0.04f, 0.04f));
                Vector3 middleBase = Surface(middle);
                float middleRoom = RoomForObstacle(middle);

                // A full grown fir in the middle would swallow the others, so it is a boulder or a young one.
                // A boulder lies on the ground and is happy under a canopy; a sapling has to fit between them
                if (rng.NextDouble() < 0.5)
                {
                    Rock(middleBase, Mathf.Min(Range(0.8f, 1.3f), middleRoom), Range(0.5f, 0.75f), middleRoom);
                }
                else
                {
                    float sapling = Mathf.Min(middleRoom, RoomBetween(middleBase, standing, crowns, crownGap));
                    Conifer(middleBase, Mathf.Min(Range(2f, 2.8f) * scale, sapling * 5f), Range(0.11f, 0.15f), sapling);
                }

                int tufts = rng.Next(2, 4);
                List<Vector2> placed = new();
                for (int i = 0; i < tufts; i++)
                {
                    if (TryPoint(0.16f, 0.2f, placed, q => true, out Vector2 p))
                        Tuft(p, 0.75f);
                }

                for (int i = 0; i < 26; i++)
                    ShortBlade(new Vector2(Range(0.06f, 0.94f), Range(0.06f, 0.94f)));
            }

            // Trunk height per variant, so trees on the map are not all of the same height
            private float TreeHeightScale()
            {
                return variant switch
                {
                    1 => 1.5f,
                    3 => 1.33f,
                    _ => 1f
                };
            }

            // How wide a crown may grow here before it comes within gap of one that already stands
            private static float RoomBetween(Vector3 point, List<Vector2> standing, List<float> crowns, float gap)
            {
                float room = float.MaxValue;
                for (int i = 0; i < standing.Count; i++)
                {
                    float distance = (new Vector2(point.x, point.z) - standing[i]).magnitude;
                    room = Mathf.Min(room, distance - crowns[i] - gap);
                }

                return Mathf.Max(room, 0.25f);
            }

            // How wide an obstacle may grow at this spot without hanging over the tile border
            private float RoomForObstacle(Vector2 spot)
            {
                const float border = 0.2f;

                float toBorderX = size.x * (0.5f - Mathf.Abs(spot.x - 0.5f));
                float toBorderZ = size.z * (0.5f - Mathf.Abs(spot.y - 0.5f));

                return Mathf.Max(Mathf.Min(toBorderX, toBorderZ) - border, 0.3f);
            }

            // Tapered trunk with a few lumpy crowns on top
            private void Tree(Vector3 basePoint, float height, float trunkRadius, float room)
            {
                Trunk(basePoint, height, trunkRadius, trunkRadius * 0.7f);

                int crowns = rng.Next(2, 4);
                for (int i = 0; i < crowns; i++)
                {
                    float level = height * Range(0.7f, 0.95f) + i * height * 0.22f;
                    float shift = Mathf.Min(0.25f, room * 0.2f);
                    Vector3 center = basePoint + Vector3.up * level + new Vector3(Range(-shift, shift), 0, Range(-shift, shift));

                    // A crown grows with the tree, until the spot around the trunk runs out
                    float radius = Mathf.Min(height * Range(0.42f, 0.52f), room - shift) * crownScale;

                    Blob(center, radius, Palette.DarkFoliage, Range(0.85f, 1f), 1);
                }
            }

            // Tapered round stem, smooth around so it does not look faceted
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

            // Fir: a bare stem carrying needled skirts that get smaller and lighter towards the top.
            // Returns how wide the crown ended up, so the next fir can keep its distance
            private float Conifer(Vector3 basePoint, float height, float trunkRadius, float room)
            {
                float bare = height * Range(0.16f, 0.2f);
                float top = height * 0.78f;
                Trunk(basePoint, height * 0.86f, trunkRadius, trunkRadius * 0.45f);

                int tiers = rng.Next(3, 5);
                float widest = Mathf.Min(height * Range(0.26f, 0.32f) * crownScale, room * Range(0.84f, 1f));
                float spacing = (top - bare) / tiers;

                for (int i = 0; i < tiers; i++)
                {
                    float t = i / (float)tiers;
                    // Each skirt stops short of the one above it, so the stem shows between the branch layers
                    float tierHeight = spacing * Range(0.65f, 0.8f);

                    Cone(basePoint + Vector3.up * (bare + spacing * i), Mathf.Lerp(widest, widest * 0.3f, t), tierHeight,
                         Mathf.Lerp(0.3f, 0.72f, t) + Range(-0.06f, 0.06f));
                }

                // The very top closes the tree off in a point
                Cone(basePoint + Vector3.up * top, widest * 0.26f, height - top, Range(0.72f, 0.9f));

                return widest;
            }

            // Needled skirt: a drooping cone carrying smooth slope normals, so the wall reads round, not faceted
            private void Cone(Vector3 basePoint, float radius, float height, float tone)
            {
                const int sides = 20;
                float turn = Range(0, 360);
                Vector3 apex = basePoint + Vector3.up * height;

                Vector3[] rim = new Vector3[sides];
                Vector3[] slope = new Vector3[sides];
                for (int i = 0; i < sides; i++)
                {
                    Quaternion rotation = Quaternion.AngleAxis(turn + i * 360f / sides, Vector3.up);
                    rim[i] = basePoint + rotation * new Vector3(0, Range(-0.05f, 0.01f) * height, radius * Range(0.96f, 1.04f));

                    // Normal of the cone wall: out by the height, up by the radius
                    Vector3 outward = Outward(rim[i], basePoint);
                    slope[i] = new Vector3(outward.x * height, radius, outward.z * height).normalized;
                }

                Vector2 uv = UV(Palette.Needle, Mathf.Clamp01(tone));

                for (int i = 0; i < sides; i++)
                {
                    int next = (i + 1) % sides;

                    AddSmoothTriangle(apex, rim[i], rim[next], Vector3.up, slope[i], slope[next], uv);
                    // Underside, so the skirt is not see-through when the camera looks up the slope
                    AddFlatTriangle(basePoint, rim[i], rim[next], apex, Palette.Needle, tone * 0.6f);
                }
            }

            // Stone spire: a prism that narrows in uneven steps, smooth around its section and stepped in profile
            private void Cliff(Vector3 basePoint, float height, float radius, float room)
            {
                const int sides = 16;
                int levels = rng.Next(5, 8);
                float tone = Range(0.35f, 0.6f);
                float turn = Range(0, Mathf.PI * 2);

                // Uneven rings reach past the radius, so it is trimmed up front rather than by squashing the
                // finished spire, which would cost it its height as well
                radius = Mathf.Min(radius, room / 1.2f);

                // A slight lean stops a group of spires from looking like a row of posts
                Vector3 lean = new Vector3(Range(-0.35f, 0.35f), 0, Range(-0.35f, 0.35f)) * radius;

                // Bands of uneven thickness, and a width that mostly holds but now and then breaks back
                // sharply: the profile steps like weathered rock instead of tapering evenly
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

                // Taken from the finished points, so the normals follow any trimming FitInside did
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
                    // Each band carries its own normals, so the ring is smooth in section while the bands
                    // meet in hard creases: a wall shades as a wall, a break shades as a ledge
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

                // Flat, chipped top
                int top = levels * sides;
                Vector3 peak = ringCenters[levels] + Vector3.up * height * Range(0.04f, 0.12f);

                for (int i = 0; i < sides; i++)
                    AddFlatTriangle(peak, points[top + i], points[top + (i + 1) % sides], inside, Palette.Cliff, tone);
            }

            // Rounded lump: a subdivided sphere with a gently uneven surface and smooth shading
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

            // Angular rock: a coarse sphere pushed around by noise and shaded flat, so every face is visible
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

                    // Broad slabs plus smaller steps make the surface look broken, not bumpy
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

            // Shrinks a shape around its footing until it fits into the given half width
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

                // Two subdivisions turn 20 faces into 320
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

            // Side of a trunk: both edges keep their own outward normal, so the trunk looks round
            private void AddSmoothQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normalAd, Vector3 normalBc, Vector2 uv)
            {
                int start = vertices.Count;
                AddVertex(a, normalAd, uv);
                AddVertex(b, normalBc, uv);
                AddVertex(c, normalBc, uv);
                AddVertex(d, normalAd, uv);

                triangles.AddRange(new[] { start, start + 2, start + 1, start, start + 3, start + 2 });
            }

            // Horizontal direction from a centre out to one of its points
            private static Vector3 Outward(Vector3 point, Vector3 center)
            {
                Vector3 direction = new(point.x - center.x, 0, point.z - center.z);
                return direction.sqrMagnitude > 1e-8f ? direction.normalized : Vector3.forward;
            }

            // Normal of the wall between two rings: out by how far it rises, up by how much the ring narrows.
            // A near vertical band points sideways, a band that breaks sharply back points up like a shelf
            private static Vector3 BandNormal(Vector3 bottom, Vector3 top, Vector3 bottomCenter, Vector3 topCenter)
            {
                Vector3 outward = Outward(bottom, bottomCenter);
                float bottomReach = new Vector2(bottom.x - bottomCenter.x, bottom.z - bottomCenter.z).magnitude;
                float topReach = new Vector2(top.x - topCenter.x, top.z - topCenter.z).magnitude;
                float rise = Mathf.Max(top.y - bottom.y, 0.001f);

                return new Vector3(outward.x * rise, bottomReach - topReach, outward.z * rise).normalized;
            }

            // Smooth shading: the corners carry their own normals, so neighbouring faces blend instead of showing an edge.
            // Wind a, b, c so that Cross(b - a, c - a) points out of the shape
            private void AddSmoothTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 na, Vector3 nb, Vector3 nc, Vector2 uv)
            {
                int start = vertices.Count;
                AddVertex(a, na, uv);
                AddVertex(b, nb, uv);
                AddVertex(c, nc, uv);
                triangles.AddRange(new[] { start, start + 1, start + 2 });
            }

            // Flat shading: the face keeps its own normal, turned away from the inside of the shape, and its own shade
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
                triangles.AddRange(new[] { start, start + 1, start + 2 });
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
                float v = ((int)palette + 0.5f) / paletteRows;
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
