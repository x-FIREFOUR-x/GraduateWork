using UnityEngine;

using static TowerDefense.EditorTools.Towers.MeshSculpting;

using Random = System.Random;


namespace TowerDefense.EditorTools.Towers
{
    // The elven archer tower: a slender tower of pale stone with a wooden deck on top and three elves on the deck,
    // turning towards the target together. Their captain stands in front, holding up a sword and bringing it down to
    // give the time; behind him two archers stand side by side, each with a longbow held out in the left hand and
    // drawing with the right, and both loose at once.
    // The elves are built about one unit tall facing +Z and drawn larger. The moving parts are meshes of their own: the
    // archers' drawing arms, the captain's sword arm, the bowstrings, which are unit long pieces stretched from the bow
    // tips to the nock, and the arrows lying on the bows. Colors come from a gradient palette texture like the other
    // towers': one ramp per material, each shifting from a cool shadow to a warm light
    public static class ArcherMeshBuilder
    {
        public enum Palette { Wood, DarkWood, Stone, Moss, Leather, Cloak, Tunic, Gold, Silver, Blond, Copper, SilverHair, Face, Bone, Feather, Royal }

        private static readonly Color32[][] paletteRamps =
        {
            new Color32[] { new(96, 62, 30, 255), new(160, 112, 58, 255), new(208, 162, 98, 255), new(238, 206, 150, 255) },
            new Color32[] { new(46, 30, 22, 255), new(88, 58, 38, 255), new(130, 92, 60, 255), new(166, 128, 90, 255) },
            new Color32[] { new(96, 100, 104, 255), new(150, 152, 148, 255), new(196, 196, 186, 255), new(228, 226, 214, 255) },
            new Color32[] { new(30, 52, 30, 255), new(62, 96, 44, 255), new(104, 140, 62, 255), new(150, 178, 92, 255) },
            new Color32[] { new(46, 30, 22, 255), new(98, 64, 40, 255), new(150, 104, 64, 255), new(196, 154, 108, 255) },
            new Color32[] { new(18, 40, 30, 255), new(36, 78, 50, 255), new(66, 118, 70, 255), new(118, 156, 96, 255) },
            new Color32[] { new(30, 56, 52, 255), new(56, 102, 86, 255), new(96, 146, 112, 255), new(156, 188, 140, 255) },
            new Color32[] { new(84, 54, 22, 255), new(156, 116, 48, 255), new(212, 172, 84, 255), new(246, 226, 156, 255) },
            new Color32[] { new(52, 60, 78, 255), new(118, 128, 146, 255), new(186, 192, 200, 255), new(244, 242, 232, 255) },
            new Color32[] { new(120, 86, 40, 255), new(186, 146, 74, 255), new(226, 196, 120, 255), new(250, 236, 180, 255) },
            new Color32[] { new(70, 26, 14, 255), new(140, 58, 26, 255), new(196, 98, 46, 255), new(232, 150, 92, 255) },
            new Color32[] { new(110, 118, 132, 255), new(170, 176, 186, 255), new(214, 218, 222, 255), new(246, 246, 244, 255) },
            // Fair skin from shadow through its own colour and light to a flush, for the sculpted faces
            new Color32[] { new(140, 98, 86, 255), new(214, 168, 142, 255), new(238, 202, 176, 255), new(222, 150, 132, 255) },
            new Color32[] { new(150, 146, 136, 255), new(196, 192, 182, 255), new(230, 228, 220, 255), new(250, 250, 246, 255) },
            // Olive armour of the archer women, from a dark bronze green in shadow to a pale gold in the light
            new Color32[] { new(38, 42, 20, 255), new(88, 94, 44, 255), new(146, 146, 78, 255), new(204, 196, 128, 255) },
            // Deep slate blue: the archer women's undershirts and sleeves, the captain's cloak, the gems
            new Color32[] { new(12, 14, 24, 255), new(28, 32, 50, 255), new(50, 58, 84, 255), new(86, 96, 128, 255) },
        };
        private const int rampWidth = 32;
        private const int rowHeight = 4;
        private const int paletteRows = 16;

        // The tower: its shaft from its flared foot up to the cornice, the deck on top of it with its railing. The deck
        // top is where the elves stand
        private const float platformSink = 0.15f;
        private const float shaftBottom = 0.1f;
        private const float shaftTop = 3.6f;
        public const float PlatformHeight = 3.9f;
        public const float DeckRadius = 1.75f;
        private const float railHeight = 0.32f;

        // The captain stands at the front of the deck a little right of its middle, the archers behind him to either
        // side, so the left one shoots well clear of his shoulder; positions in the turning part's space. Built about a
        // unit tall and drawn this much larger: the archers about 2.3 tall, slender, the captain a little taller
        public const float ArcherScale = 2.3f;
        public const float CaptainScale = 2.45f;
        public static readonly Vector3 LeftArcherPosition = new(-0.8f, 0f, -0.42f);
        public static readonly Vector3 RightArcherPosition = new(0.95f, 0f, -0.45f);
        public static readonly Vector3 CaptainPosition = new(0.3f, 0f, 0.78f);

        // The legs are meshes of their own so the elves can walk: a thigh turning at the hip and, inside it, a shin with
        // the foot turning at the knee. Hips and knees for the right leg, in an elf's own space; the left mirrors them
        public static readonly Vector3 ArcherHip = new(0.052f, 0.52f, 0f);
        public static readonly Vector3 ArcherKnee = new(0.046f, 0.29f, 0.012f);
        public static readonly Vector3 CaptainHip = new(0.05f, 0.52f, 0f);
        public static readonly Vector3 CaptainKnee = new(0.046f, 0.28f, 0.012f);

        // A point of the right leg moved over to the side given: 1 for the right, -1 for the left
        public static Vector3 OnSide(Vector3 point, float side)
        {
            return new Vector3(point.x * side, point.y, point.z);
        }

        // An archer woman's thigh in its own space, turning at the hip at the origin: full at the hip and tapering to the
        // knee, in dark leggings bound by leather straps
        public static Mesh BuildArcherThigh(float side)
        {
            Builder builder = new(side > 0f ? 361 : 363, shadeNoise: 0.55f, noiseScale: 6f);
            float x = 0.052f * side;
            Vector3[] spine = { new(x, 0.535f, 0f), new(x, 0.45f, 0.006f), new(x * 0.95f, 0.37f, 0.01f), new(x * 0.9f, 0.3f, 0.012f) };
            builder.Tube(spine, new[] { 0.05f, 0.046f, 0.038f, 0.03f }, Palette.Royal, 0.3f, sides: 22, roundStart: false, toneEnd: 0.18f);
            foreach ((float t, float radius) in new[] { (0.35f, 0.047f), (0.6f, 0.042f) })
            {
                Vector3 at = Vector3.Lerp(spine[1], spine[2], t);
                builder.Torus(at, new Vector3(0.12f * side, 1f, 0.15f), new Vector2(radius, radius * 0.95f), 0.0032f, Palette.Leather, 0.3f, segments: 20, sides: 4);
            }
            builder.Ball(new Vector3(x * 0.9f, 0.295f, 0.026f), new Vector3(0.026f, 0.026f, 0.022f), Palette.Royal, 0.35f);

            // A seam down the front of the leggings, and on the outer side a cut lacing showing the skin, crossed by cords
            builder.Tube(new[] { spine[0] + new Vector3(0f, 0f, 0.048f), spine[1] + new Vector3(0f, 0f, 0.046f), spine[2] + new Vector3(0f, 0f, 0.039f),
                                 spine[3] + new Vector3(0f, 0.01f, 0.03f) },
                         new[] { 0.0018f, 0.0018f, 0.0016f, 0.0014f }, Palette.Royal, 0.05f, sides: 4);
            Vector3 Outer(float t, float lift) => Vector3.Lerp(spine[1], spine[2], t) + new Vector3(side * (Mathf.Lerp(0.046f, 0.038f, t) + lift), 0f, 0f);
            builder.Tube(new[] { Outer(0f, -0.004f), Outer(0.5f, -0.003f), Outer(1f, -0.004f) }, new[] { 0.007f, 0.008f, 0.006f }, Palette.Face, 0.5f, sides: 8);
            for (int cross = 0; cross < 4; cross++)
            {
                float t = 0.1f + cross * 0.25f;
                builder.Tube(new[] { Outer(t, 0f) + new Vector3(0f, 0f, -0.008f), Outer(t + 0.12f, 0.002f) + new Vector3(0f, 0f, 0.008f) }, new[] { 0.0015f, 0.0015f },
                             Palette.Leather, 0.25f, sides: 4);
                builder.Tube(new[] { Outer(t, 0f) + new Vector3(0f, 0f, 0.008f), Outer(t + 0.12f, 0.002f) + new Vector3(0f, 0f, -0.008f) }, new[] { 0.0015f, 0.0015f },
                             Palette.Leather, 0.25f, sides: 4);
            }

            return Shifted(builder.ToMesh(), -OnSide(ArcherHip, side));
        }

        // An archer woman's shin with its foot in its own space, turning at the knee at the origin: the armoured boot,
        // shaped to the calf, the ankle slim
        public static Mesh BuildArcherShin(float side)
        {
            Builder builder = new(side > 0f ? 365 : 367, shadeNoise: 0.55f, noiseScale: 6f);
            float x = 0.046f * side;
            FemaleBoot(builder, x);

            // A gold lip round the top of the greave, plates round the ankle, a ridge down the shin, filigree on the toe
            builder.Torus(new Vector3(x, 0.312f, 0.01f), new Vector3(0f, 1f, 0.15f), new Vector2(0.036f, 0.036f), 0.003f, Palette.Gold, 0.65f, segments: 20, sides: 4);
            for (int plate = 0; plate < 2; plate++)
                builder.Torus(new Vector3(x, 0.065f + plate * 0.022f, -0.002f), new Vector3(0f, 1f, 0.2f), new Vector2(0.029f, 0.029f), 0.004f, Palette.Feather,
                              0.6f - 0.1f * plate, segments: 20, sides: 5);
            builder.Tube(new[] { new Vector3(x, 0.27f, 0.039f), new Vector3(x, 0.19f, 0.038f), new Vector3(x, 0.11f, 0.03f) }, new[] { 0.004f, 0.005f, 0.003f },
                         Palette.Feather, 0.75f, sides: 6);
            builder.Tube(new[] { new Vector3(x, 0.043f, 0.05f), new Vector3(x * 1.04f, 0.036f, 0.085f), new Vector3(x * 1.05f, 0.03f, 0.11f) },
                         new[] { 0.0018f, 0.002f, 0.0012f }, Palette.Bone, 0.7f, sides: 4);

            return Shifted(builder.ToMesh(), -OnSide(ArcherKnee, side));
        }

        // The captain's thigh in its own space, turning at the hip at the origin: in brown leggings, full at the hip
        public static Mesh BuildCaptainThigh(float side)
        {
            Builder builder = new(side > 0f ? 371 : 373, shadeNoise: 0.55f, noiseScale: 6f);
            float x = 0.05f * side;
            builder.Tube(new[] { new Vector3(x, 0.535f, 0f), new Vector3(x, 0.44f, 0.006f), new Vector3(x * 0.96f, 0.36f, 0.01f), new Vector3(x * 0.92f, 0.28f, 0.012f) },
                         new[] { 0.049f, 0.045f, 0.037f, 0.031f }, Palette.DarkWood, 0.5f, sides: 22, roundStart: false, toneEnd: 0.3f);
            builder.Ball(new Vector3(x * 0.92f, 0.285f, 0.026f), new Vector3(0.026f, 0.026f, 0.022f), Palette.DarkWood, 0.45f);

            // Seams down the front and the outer side of the leggings, a leather patch over the knee
            foreach (Vector3 face in new[] { new Vector3(0f, 0f, 1f), new Vector3(side, 0f, 0f) })
                builder.Tube(new[] { new Vector3(x, 0.52f, 0f) + face * 0.048f, new Vector3(x, 0.44f, 0.006f) + face * 0.044f, new Vector3(x * 0.96f, 0.36f, 0.01f) + face * 0.036f,
                                     new Vector3(x * 0.92f, 0.3f, 0.012f) + face * 0.03f },
                             new[] { 0.0018f, 0.0018f, 0.0016f, 0.0014f }, Palette.DarkWood, 0.15f, sides: 4);
            builder.Sculpt(new Vector3(x * 0.92f, 0.3f, 0.035f), new Vector3(0.022f, 0.026f, 0.01f), new[] { (Vector3.forward, 0.6f, 0.2f) }, Palette.Leather,
                           direction => 0.4f + 0.2f * direction.y, 2);

            return Shifted(builder.ToMesh(), -OnSide(CaptainHip, side));
        }

        // The captain's shin with its foot in its own space, turning at the knee at the origin: the legging down to the
        // tall riding boot
        public static Mesh BuildCaptainShin(float side)
        {
            Builder builder = new(side > 0f ? 375 : 377, shadeNoise: 0.55f, noiseScale: 6f);
            float x = 0.046f * side;
            builder.Tube(new[] { new Vector3(x, 0.285f, 0.012f), new Vector3(x, 0.25f, 0.008f), new Vector3(x, 0.22f, 0.005f) }, new[] { 0.031f, 0.032f, 0.033f },
                         Palette.DarkWood, 0.4f, sides: 22, toneEnd: 0.3f);
            ElfBoot(builder, x);

            return Shifted(builder.ToMesh(), -OnSide(CaptainKnee, side));
        }

        // The mesh moved by the offset given, so it turns round a point other than the one it was built about
        private static Mesh Shifted(Mesh mesh, Vector3 offset)
        {
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] += offset;
            mesh.vertices = vertices;
            mesh.RecalculateBounds();

            return mesh;
        }

        // An archer in his own space: the bow held out in his left hand, its string from tip to tip, the arrow lying on
        // the line from the nock forward past the bow
        public static readonly Vector3 BowTopTip = new(-0.06f, 1.14f, 0.3f);
        public static readonly Vector3 BowBottomTip = new(-0.06f, 0.52f, 0.3f);
        private const float bowGripZ = 0.34f;
        private const float arrowLineX = -0.045f;
        private const float arrowLineY = 0.83f;
        // The arrow's length, and where it rests on the bow by the grip; it lies from the nock in the drawing hand
        // through there
        public const float ArrowLength = 0.42f;
        public static readonly Vector3 ArrowRest = new(arrowLineX, arrowLineY, bowGripZ);

        // The drawing arm: two bones from the right shoulder, the upper arm and the forearm with the hand. Drawing, the
        // elbow swings round the shoulder level with it, from in front of him out to his side and on behind him, the
        // forearm always pointing at the arrow rest on the bow; the hand holds the nock a little ahead of and above the
        // wrist. Angles round the shoulder from straight ahead towards his right, with the string slack and fully drawn
        public static readonly Vector3 DrawShoulder = new(0.13f, 0.79f, 0f);
        public const float UpperArmLength = 0.15f;
        public const float ForearmLength = 0.15f;
        public const float ElbowRestAngle = -15f;
        public const float ElbowDrawnAngle = 130f;
        public static readonly Vector3 NockFromWrist = new(0f, 0.04f, 0.05f);

        // The captain's sword arm turns round his right shoulder; built held out straight ahead
        public static readonly Vector3 CommandShoulder = new(0.135f, 0.81f, 0f);


        public static Texture2D CreatePalette()
        {
            return MeshSculpting.CreatePalette(paletteRamps, rampWidth, paletteRows, rowHeight);
        }

        // A slender elven tower of pale stone the archers stand on top of: living roots gripping its flared foot, courses
        // of pale blocks banded in gold, a pointed arched door and tall lancet windows, ivy winding up it, corbels
        // carrying a wooden deck with a carved railing round it, green banners hanging from the railing. An archery butt
        // and a young silver birch stand in the back corners of the tile
        public static Mesh BuildTower()
        {
            Builder builder = new(311, shadeNoise: 0.75f, noiseScale: 1.8f);
            Random rng = new(313);

            // Flared foot sunk into the tile, then the shaft in courses of blocks, each set a hair in from the one below
            builder.Frustum(new Vector3(0f, -platformSink, 0f), 1.95f, 1.8f, platformSink + 0.25f, 16, Palette.Stone, 0.4f, 0.12f, 0f);
            const int courses = 11;
            float courseHeight = (shaftTop - shaftBottom) / courses;
            for (int course = 0; course < courses; course++)
            {
                float bottom = shaftBottom + course * courseHeight;
                builder.Frustum(new Vector3(0f, bottom, 0f), ShaftRadius(bottom), ShaftRadius(bottom + courseHeight) - 0.012f, courseHeight - 0.01f, 16,
                                Palette.Stone, 0.5f + 0.05f * (course % 2), 0.12f, course % 2 == 0 ? 0f : 11.25f, 0.05f);
            }
            foreach (float y in new[] { 1.2f, 2.4f, 3.35f })
                builder.Torus(new Vector3(0f, y, 0f), Vector3.up, new Vector2(ShaftRadius(y) + 0.02f, ShaftRadius(y) + 0.02f), 0.03f, Palette.Gold, 0.55f,
                              segments: 32, sides: 6);

            // Living roots gripping the foot and climbing a little way up the wall
            for (int i = 0; i < 7; i++)
            {
                float angle = (i + 0.3f) * 360f / 7 + (float)rng.NextDouble() * 15f;
                Vector3 outward = Around(angle);
                Vector3 along = Around(angle + 90f);
                float climb = 0.7f + (float)rng.NextDouble() * 0.5f;
                Vector3[] root =
                {
                    outward * 2.12f + Vector3.down * 0.05f,
                    outward * 1.9f + Vector3.up * 0.14f + along * 0.05f,
                    outward * (ShaftRadius(0.5f) + 0.05f) + Vector3.up * 0.5f - along * 0.03f,
                    outward * (ShaftRadius(climb) + 0.02f) + Vector3.up * climb + along * 0.06f,
                };
                builder.Tube(root, new[] { 0.09f, 0.13f, 0.08f, 0.015f }, Palette.DarkWood, 0.25f, sides: 8, wrinkle: 0.15f, toneEnd: 0.6f);
            }

            BuildTowerDoor(builder);
            foreach (float angle in new[] { 70f, 180f, 290f })
                BuildLancet(builder, angle, 1.6f);
            foreach (float angle in new[] { 0f, 125f, 235f })
                BuildLancet(builder, angle, 2.85f);

            // Ivy winding up the shaft in two vines, leaves along them
            foreach (float start in new[] { 30f, 215f })
            {
                const int points = 9;
                Vector3[] vine = new Vector3[points];
                float[] radii = new float[points];
                for (int k = 0; k < points; k++)
                {
                    float t = k / (points - 1f);
                    float y = Mathf.Lerp(0.1f, shaftTop - 0.1f, t);
                    vine[k] = Around(start + t * 140f + Mathf.Sin(k * 1.3f) * 8f) * (ShaftRadius(y) + 0.02f) + Vector3.up * y;
                    radii[k] = Mathf.Lerp(0.03f, 0.012f, t);
                }
                builder.Tube(vine, radii, Palette.Moss, 0.2f, sides: 6, toneEnd: 0.45f);
                for (int leaf = 0; leaf < 20; leaf++)
                {
                    float t = (leaf + 0.5f) / 20f;
                    int k = Mathf.Min(points - 2, (int)(t * (points - 1)));
                    Vector3 at = Vector3.Lerp(vine[k], vine[k + 1], t * (points - 1) - k);
                    Vector3 outward = new Vector3(at.x, 0f, at.z).normalized;
                    Vector3 side = Vector3.Cross(Vector3.up, outward) * (leaf % 2 == 0 ? 0.05f : -0.05f);
                    builder.Sculpt(at + outward * 0.02f + side, new Vector3(0.05f, 0.035f, 0.05f), new[] { (outward, 0.6f, -0.5f), (-outward, 0.6f, -0.5f) },
                                   Palette.Moss, direction => 0.35f + 0.25f * (leaf % 3) / 2f + 0.2f * direction.y, 2);
                }
            }

            // Corbels carrying the cornice, the cornice flaring out under the deck
            for (int i = 0; i < 16; i++)
            {
                Vector3 outward = Around((i + 0.5f) * 22.5f);
                builder.Sculpt(outward * (ShaftRadius(shaftTop) + 0.04f) + Vector3.up * (shaftTop - 0.06f), new Vector3(0.07f, 0.11f, 0.07f),
                               new[] { (Vector3.down, 0.6f, -0.4f), (outward, 0.5f, 0.1f) }, Palette.Stone, direction => 0.4f + 0.25f * direction.y, 2);
            }
            builder.Frustum(new Vector3(0f, shaftTop, 0f), ShaftRadius(shaftTop), DeckRadius + 0.05f, 0.16f, 16, Palette.Stone, 0.55f, 0.08f, 0f);
            builder.Torus(new Vector3(0f, shaftTop + 0.16f, 0f), Vector3.up, new Vector2(DeckRadius + 0.05f, DeckRadius + 0.05f), 0.025f, Palette.Gold, 0.6f,
                          segments: 40, sides: 6);

            // The wooden deck: boards laid round a middle disc, the grain shading from board to board
            builder.Cylinder(new Vector3(0f, PlatformHeight - 0.07f, 0f), Vector3.up, DeckRadius, 0.12f, 24, Palette.DarkWood, 0.45f, false);
            builder.Paving(new Vector3(0f, PlatformHeight - 0.005f, 0f), DeckRadius - 0.04f, 0.45f, 12, Palette.Wood, 0.5f);

            // Carved railing: slim posts with gold leaf finials, a top rail and a lower rail
            const int posts = 18;
            for (int i = 0; i < posts; i++)
            {
                Vector3 at = Around(i * 360f / posts) * (DeckRadius - 0.06f);
                builder.Tube(new[] { at + Vector3.up * PlatformHeight, at + Vector3.up * (PlatformHeight + 0.18f), at + Vector3.up * (PlatformHeight + railHeight) },
                             new[] { 0.028f, 0.02f, 0.026f }, Palette.Wood, 0.45f, sides: 8, roundStart: false, toneEnd: 0.7f);
                builder.Ball(at + Vector3.up * (PlatformHeight + railHeight + 0.04f), new Vector3(0.025f, 0.04f, 0.025f), Palette.Gold, 0.65f);
            }
            builder.Torus(new Vector3(0f, PlatformHeight + railHeight, 0f), Vector3.up, new Vector2(DeckRadius - 0.06f, DeckRadius - 0.06f), 0.025f, Palette.Wood,
                          0.6f, segments: 48, sides: 6);
            builder.Torus(new Vector3(0f, PlatformHeight + 0.12f, 0f), Vector3.up, new Vector2(DeckRadius - 0.06f, DeckRadius - 0.06f), 0.013f, Palette.Wood,
                          0.4f, segments: 48, sides: 5, waviness: 0.01f);

            // Two green banners hanging from the railing down the sides, a gold leaf on each
            foreach (float angle in new[] { 95f, 265f })
            {
                Vector3 outward = Around(angle);
                Quaternion facing = Quaternion.LookRotation(outward);
                Vector3 top = outward * (DeckRadius + 0.02f) + Vector3.up * (PlatformHeight + railHeight - 0.02f);
                Vector3 bottom = outward * (ShaftRadius(2.75f) + 0.08f) + Vector3.up * 2.75f;
                Vector3 middle = Vector3.Lerp(top, bottom, 0.5f);
                builder.Tube(new[] { top, middle + outward * 0.03f, bottom }, new[] { 0.06f, 0.065f, 0.065f }, Palette.Cloak, 0.6f, sides: 8,
                             squash: new Vector2(3.4f, 0.25f), roundStart: false, roundEnd: false, wrinkle: 0.08f, toneEnd: 0.25f);
                builder.Flag(bottom - Vector3.Cross(Vector3.up, outward) * 0.2f, bottom + Vector3.Cross(Vector3.up, outward) * 0.2f,
                             bottom + Vector3.down * 0.22f, Palette.Cloak, 0.25f);
                builder.Sculpt(middle + outward * 0.045f, new Vector3(0.06f, 0.1f, 0.06f), new[] { (outward, 0.6f, -0.6f), (-outward, 0.6f, -0.6f) },
                               Palette.Gold, direction => 0.55f + 0.2f * direction.y, 2);
            }

            // Moss gathered at the foot
            for (int i = 0; i < 10; i++)
            {
                Vector3 outward = Around(i * 36f + (float)rng.NextDouble() * 20f);
                builder.Sculpt(outward * 1.92f + Vector3.up * 0.1f, new Vector3(0.16f, 0.06f, 0.16f), new[] { (Vector3.up, 0.6f, 0.3f) }, Palette.Moss,
                               direction => 0.35f + 0.3f * direction.y, 2);
            }

            BuildArcheryButt(builder, new Vector3(2.0f, 0f, -1.95f));
            BuildBirch(builder, new Vector3(-2.0f, 0f, -1.95f));

            return builder.ToMesh();
        }

        private static Vector3 Around(float angle)
        {
            return Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        }

        private static float ShaftRadius(float height)
        {
            return Mathf.Lerp(1.5f, 1.25f, Mathf.InverseLerp(shaftBottom, shaftTop, height));
        }

        // A tall lancet window at an angle round the tower: a dark opening ending in a pointed arch, framed in pale stone
        // with a gold leaf at the point and a sill under it
        private static void BuildLancet(Builder builder, float angle, float height)
        {
            Vector3 outward = Around(angle);
            Vector3 side = Vector3.Cross(Vector3.up, outward);
            Quaternion facing = Quaternion.LookRotation(outward);
            Vector3 center = outward * (ShaftRadius(height) - 0.01f) + Vector3.up * height;

            builder.Box(center, facing, new Vector3(0.16f, 0.42f, 0.04f), Palette.Royal, 0.12f);
            Arch(builder, center + Vector3.up * 0.21f + outward * 0.015f, outward, 0.08f, 0.16f, Palette.Royal, 0.12f, fill: true);
            foreach (float s in new[] { -1f, 1f })
                builder.Box(center + side * (s * 0.11f) + outward * 0.03f, facing, new Vector3(0.06f, 0.44f, 0.07f), Palette.Stone, 0.6f);
            Arch(builder, center + Vector3.up * 0.21f + outward * 0.035f, outward, 0.11f, 0.2f, Palette.Stone, 0.65f, fill: false);
            builder.Ball(center + Vector3.up * 0.43f + outward * 0.06f, new Vector3(0.025f, 0.035f, 0.012f), Palette.Gold, 0.7f);
            builder.Box(center + Vector3.down * 0.24f + outward * 0.05f, facing, new Vector3(0.28f, 0.04f, 0.1f), Palette.Stone, 0.65f);
        }

        // The door in the front of the tower: two dark wooden leaves under a pointed arch, gold leaf handles, framed in
        // pale stone, a step before it
        private static void BuildTowerDoor(Builder builder)
        {
            Vector3 outward = Vector3.forward;
            const float height = 0.5f;
            Vector3 center = outward * (ShaftRadius(height) - 0.01f) + Vector3.up * height;
            builder.Box(center, Quaternion.identity, new Vector3(0.44f, 0.72f, 0.06f), Palette.DarkWood, 0.4f);
            Arch(builder, center + Vector3.up * 0.36f + outward * 0.01f, outward, 0.22f, 0.3f, Palette.DarkWood, 0.35f, fill: true);
            builder.Box(center + outward * 0.035f + Vector3.up * 0.08f, Quaternion.identity, new Vector3(0.01f, 0.9f, 0.01f), Palette.DarkWood, 0.15f);
            foreach (float s in new[] { -1f, 1f })
            {
                builder.Box(center + Vector3.right * (s * 0.26f) + outward * 0.03f, Quaternion.identity, new Vector3(0.08f, 0.74f, 0.09f), Palette.Stone, 0.6f);
                builder.Sculpt(center + Vector3.right * (s * 0.05f) + outward * 0.045f, new Vector3(0.02f, 0.045f, 0.012f), new[] { (outward, 0.6f, 0.2f) },
                               Palette.Gold, direction => 0.6f + 0.2f * direction.y, 2);
            }
            Arch(builder, center + Vector3.up * 0.36f + outward * 0.04f, outward, 0.27f, 0.36f, Palette.Stone, 0.65f, fill: false);
            builder.Box(new Vector3(0f, 0.05f, ShaftRadius(0.1f) + 0.2f), Quaternion.identity, new Vector3(0.7f, 0.12f, 0.3f), Palette.Stone, 0.55f);
        }

        // A pointed arch facing outward at the spring point given: two curved sides meeting at the top, either as a
        // frame of stone, or filling the space under it
        private static void Arch(Builder builder, Vector3 spring, Vector3 outward, float halfWidth, float rise, Palette palette, float tone, bool fill)
        {
            Vector3 side = Vector3.Cross(Vector3.up, outward);
            if (fill)
            {
                for (int i = 0; i < 4; i++)
                {
                    float t = (i + 0.5f) / 4f;
                    float width = halfWidth * Mathf.Sqrt(1f - t * t * t);
                    builder.Box(spring + Vector3.up * (rise * t), Quaternion.LookRotation(outward), new Vector3(width * 2f, rise / 4f + 0.01f, 0.04f), palette, tone);
                }
                return;
            }

            foreach (float s in new[] { -1f, 1f })
            {
                Vector3 foot = spring + side * (s * halfWidth);
                Vector3 bend = spring + side * (s * halfWidth * 0.75f) + Vector3.up * (rise * 0.6f);
                Vector3 apex = spring + Vector3.up * rise;
                builder.Tube(new[] { foot, bend, apex }, new[] { 0.03f, 0.028f, 0.025f }, palette, tone, sides: 6, squash: new Vector2(1f, 1.2f));
            }
        }

        // Archery butt on a wooden stand: a thick straw boss with painted rings and two arrows in it
        private static void BuildArcheryButt(Builder builder, Vector3 butt)
        {
            Vector3 facing = new Vector3(-1f, 0f, 1f).normalized;
            foreach (float s in new[] { -1f, 1f })
            {
                Vector3 side = Vector3.Cross(Vector3.up, facing) * (s * 0.3f);
                builder.Beam(butt + side - facing * 0.15f, butt + side + Vector3.up * 0.95f, 0.06f, 0.06f, Palette.DarkWood, 0.45f);
            }
            Vector3 boss = butt + Vector3.up * 0.65f + facing * 0.02f;
            builder.Cylinder(boss, facing, 0.38f, 0.16f, 16, Palette.Blond, 0.55f, false);
            foreach ((float radius, Palette palette, float tone) in new[] { (0.3f, Palette.Royal, 0.6f), (0.2f, Palette.Bone, 0.85f), (0.1f, Palette.Copper, 0.6f) })
                builder.Cylinder(boss + facing * 0.085f, facing, radius, 0.01f, 16, palette, tone, false);
            Arrow(builder, boss + facing * 0.05f + new Vector3(0.05f, 0.04f, 0f), -facing + new Vector3(0.05f, -0.1f, 0f), 2.2f);
            Arrow(builder, boss + facing * 0.05f + new Vector3(-0.08f, -0.1f, 0f), -facing + new Vector3(-0.1f, 0.05f, 0f), 2.2f);
        }

        // Young silver birch: a slender white trunk with dark marks and a crown of leaves
        private static void BuildBirch(Builder builder, Vector3 tree)
        {
            Vector3[] trunk = { tree, tree + new Vector3(0.03f, 0.6f, 0f), tree + new Vector3(-0.04f, 1.2f, 0.03f), tree + new Vector3(0.02f, 1.65f, 0f) };
            builder.Tube(trunk, new[] { 0.09f, 0.075f, 0.06f, 0.035f }, Palette.Bone, 0.7f, sides: 14, roundStart: false, wrinkle: 0.06f);
            for (int mark = 0; mark < 6; mark++)
                builder.Ball(Vector3.Lerp(trunk[0], trunk[3], 0.1f + mark * 0.14f) + new Vector3(mark % 2 == 0 ? 0.06f : -0.05f, 0f, 0.03f),
                             new Vector3(0.025f, 0.012f, 0.02f), Palette.DarkWood, 0.15f);
            for (int clump = 0; clump < 6; clump++)
            {
                float angle = clump * Mathf.PI * 2 / 6;
                Vector3 at = trunk[3] + new Vector3(Mathf.Cos(angle) * 0.28f, -0.15f + 0.1f * (clump % 2), Mathf.Sin(angle) * 0.28f);
                builder.Sculpt(at, new Vector3(0.24f, 0.2f, 0.24f), new[] { (Vector3.up, 0.6f, 0.2f) }, Palette.Moss, direction => 0.45f + 0.3f * direction.y, 2);
            }
            builder.Sculpt(trunk[3] + Vector3.up * 0.12f, new Vector3(0.26f, 0.22f, 0.26f), new[] { (Vector3.up, 0.6f, 0.2f) }, Palette.Moss,
                           direction => 0.55f + 0.3f * direction.y, 2);
        }

        // An archer in his own space, standing on the origin, facing +Z: his body, his head and his left arm holding the
        // longbow out in front of him. The drawing arm is a mesh of its own. hair picks the colour of his hair
        public static Mesh BuildArcher(Palette hair)
        {
            Builder builder = new(hair == Palette.DarkWood ? 321 : 327, shadeNoise: 0.55f, noiseScale: 6f);

            FemaleBody(builder);
            FemaleHead(builder, hair);

            // Quiver slung across her back, arrows sticking out of it over her right shoulder
            Vector3 quiverBottom = new(-0.06f, 0.42f, -0.11f);
            Vector3 quiverTop = new(0.08f, 0.82f, -0.13f);
            builder.Tube(new[] { quiverBottom, quiverTop }, new[] { 0.032f, 0.036f }, Palette.Leather, 0.3f, sides: 18, roundEnd: false, toneEnd: 0.6f);
            foreach (float t in new[] { 0.2f, 0.85f })
                builder.Torus(Vector3.Lerp(quiverBottom, quiverTop, t), quiverTop - quiverBottom, new Vector2(0.036f, 0.036f), 0.005f, Palette.Gold, 0.6f,
                              segments: 12, sides: 5);
            Vector3 up = (quiverTop - quiverBottom).normalized;
            for (int i = 0; i < 5; i++)
            {
                Vector3 spread = new((i - 2) * 0.011f, 0f, (i % 2) * 0.012f - 0.006f);
                // Point down in the quiver, the feathers showing above its mouth
                float rise = 0.07f + 0.015f * (i % 3);
                Arrow(builder, quiverTop + spread + up * (rise - ArrowLength * 0.5f), -up, 0.5f);
            }
            builder.Tube(new[] { new Vector3(-0.1f, 0.78f, 0.03f), new Vector3(0f, 0.66f, 0.09f), new Vector3(0.1f, 0.5f, 0.05f) },
                         new[] { 0.008f, 0.008f, 0.008f }, Palette.Leather, 0.35f, sides: 6, squash: new Vector2(2f, 0.5f));

            // Left arm held out in front, the bow in the fist
            Vector3 shoulder = new(-0.13f, 0.79f, 0f);
            Vector3 elbow = Vector3.Lerp(shoulder, new Vector3(-0.07f, arrowLineY - 0.01f, bowGripZ - 0.04f), 0.5f) + new Vector3(-0.012f, -0.006f, 0f);
            Vector3 wrist = new(-0.07f, arrowLineY - 0.01f, bowGripZ - 0.04f);
            ElfArm(builder, shoulder, elbow, wrist, Palette.Royal, bracer: true, bracerPalette: Palette.Feather);
            // The hand closed round the grip: its back to the left, the knuckles ahead, the fingers wrapping round the
            // front of the grip, the index finger on top under the arrow, the thumb over the middle finger
            Hand(builder, wrist, Vector3.forward, Vector3.left, 1f, new[] { 85f, 88f, 90f, 80f }, 70f);

            BuildBow(builder);

            return builder.ToMesh();
        }

        // The captain in his own space, standing on the origin, facing +Z: a silver breastplate over his tunic, a royal
        // blue cloak, a silver circlet in his silver hair, his left hand resting on his sword belt. The sword arm is a
        // mesh of its own
        public static Mesh BuildCaptain()
        {
            Builder builder = new(331, shadeNoise: 0.55f, noiseScale: 6f);

            ElfBody(builder, Palette.Tunic, Palette.Royal, armoured: true);
            ElfHead(builder, Palette.SilverHair, circlet: true);

            // Left arm bent, the hand on the belt; an empty scabbard hanging at his left hip
            ElfArm(builder, new Vector3(-0.13f, 0.79f, 0f), new Vector3(-0.18f, 0.66f, -0.02f), new Vector3(-0.11f, 0.57f, 0.06f), Palette.Royal, bracer: true);
            Hand(builder, new Vector3(-0.11f, 0.57f, 0.06f), new Vector3(0.6f, -0.5f, 0.6f), new Vector3(-0.5f, 0.2f, 0.8f), 1f, new[] { 25f, 30f, 35f, 30f }, 20f);
            builder.Tube(new[] { new Vector3(-0.1f, 0.54f, -0.02f), new Vector3(-0.13f, 0.38f, -0.06f), new Vector3(-0.15f, 0.22f, -0.1f) },
                         new[] { 0.016f, 0.015f, 0.012f }, Palette.Leather, 0.35f, sides: 8, squash: new Vector2(1.8f, 0.6f), toneEnd: 0.15f);
            builder.Ball(new Vector3(-0.15f, 0.21f, -0.1f), new Vector3(0.018f, 0.018f, 0.014f), Palette.Silver, 0.7f);

            return builder.ToMesh();
        }

        // The upper bone of an archer's drawing arm in its own space: from the shoulder at the origin along +Z to the
        // elbow. The tower turns it, and the forearm, so the hand reaches the nock with the elbow bending out and back
        public static Mesh BuildDrawUpperArm()
        {
            Builder builder = new(341, shadeNoise: 0.55f, noiseScale: 6f);
            Vector3 elbow = Vector3.forward * UpperArmLength;
            builder.Tube(new[] { Vector3.back * 0.01f, elbow * 0.35f, elbow * 0.75f, elbow }, new[] { 0.034f, 0.031f, 0.027f, 0.024f }, Palette.Royal, 0.55f,
                         sides: 18, wrinkle: 0.07f, toneEnd: 0.35f);
            builder.Ball(Vector3.zero, Vector3.one * 0.034f, Palette.Royal, 0.5f);
            builder.Ball(elbow, Vector3.one * 0.024f, Palette.Royal, 0.35f);

            return builder.ToMesh();
        }

        // The lower bone of an archer's drawing arm in its own space: from the elbow at the origin along +Z to the wrist,
        // a cuff and the bare wrist, and the hand drawing the string: its back turned out (+X), three fingers hooked,
        // one above the arrow and two below, the little finger and the thumb tucked in, a leather tab over the fingers
        public static Mesh BuildDrawForearm()
        {
            Builder builder = new(343, shadeNoise: 0.55f, noiseScale: 6f);
            Vector3 wrist = Vector3.forward * ForearmLength;
            builder.Tube(new[] { Vector3.zero, wrist * 0.3f, wrist * 0.85f }, new[] { 0.024f, 0.026f, 0.019f }, Palette.Royal, 0.4f, sides: 18, wrinkle: 0.06f,
                         toneEnd: 0.6f);
            builder.Torus(wrist * 0.85f, Vector3.forward, new Vector2(0.021f, 0.021f), 0.006f, Palette.Royal, 0.7f, segments: 12, sides: 5, waviness: 0.003f);
            builder.Tube(new[] { wrist * 0.82f, wrist }, new[] { 0.016f, 0.0145f }, Palette.Face, 0.4f, sides: 14);
            builder.Tube(new[] { wrist * 0.38f, wrist * 0.9f }, new[] { 0.027f, 0.022f }, Palette.Feather, 0.35f, sides: 14, roundStart: false, roundEnd: false,
                         toneEnd: 0.6f);
            builder.Tube(new[] { wrist * 0.42f, wrist * 0.6f, wrist * 0.85f }, new[] { 0.002f, 0.0025f, 0.0018f }, Palette.Bone, 0.7f, sides: 4);
            Hand(builder, wrist, Vector3.forward, Vector3.right, -1f, new[] { 95f, 65f, 65f, 65f }, 45f);
            builder.Ball(wrist + new Vector3(-0.008f, 0f, 0.05f), new Vector3(0.012f, 0.02f, 0.008f), Palette.Leather, 0.4f);

            return builder.ToMesh();
        }

        // The captain's sword arm in its own space, turning round the shoulder at the origin, built held out straight
        // ahead: a sleeve, a silver vambrace, the hand closed in a fist round the hilt, and a long leaf bladed elven sword
        // running on from the fist, angled up from the arm the way a sword held out at arm's length is; a curved gold
        // guard, a wired grip and a pommel set with sapphires
        public static Mesh BuildCommandArm()
        {
            Builder builder = new(347, shadeNoise: 0.55f, noiseScale: 6f);
            Vector3 elbow = new(0f, -0.015f, 0.14f);
            Vector3 wrist = new(0f, -0.025f, 0.27f);
            ElfArm(builder, Vector3.zero, elbow, wrist, Palette.Royal, bracer: true);

            // The hilt passes through the fist across the hand; the blade runs on from it up and ahead
            Vector3 forearm = (wrist - elbow).normalized;
            Vector3 hilt = (forearm * 0.8f + Vector3.up * 0.6f).normalized;
            Vector3 knuckles = Vector3.ProjectOnPlane(Vector3.right, hilt).normalized;
            Vector3 round = Vector3.Cross(hilt, knuckles);
            Vector3 fist = wrist + forearm * 0.035f + Vector3.up * 0.008f;

            // The back of the hand, the knuckles along it, four fingers curled round the grip from the knuckles over and
            // under it, the index finger nearest the guard, the thumb laid over the index finger
            builder.Tube(new[] { wrist, fist + knuckles * 0.012f - hilt * 0.004f }, new[] { 0.016f, 0.019f }, Palette.Face, 0.45f, sides: 14);
            for (int i = 0; i < 4; i++)
            {
                float along = (i - 1.5f) * 0.0155f;
                Vector3 center = fist + hilt * along;
                Vector3 Ring(float angle) => center + (knuckles * Mathf.Cos(angle) + round * Mathf.Sin(angle)) * 0.017f;
                builder.Ball(Ring(0f) + knuckles * 0.004f, Vector3.one * 0.0068f, Palette.Face, 0.6f);
                builder.Tube(new[] { Ring(0f), Ring(1.2f), Ring(2.4f), Ring(3.4f) }, new[] { 0.0075f, 0.007f, 0.0065f, 0.005f }, Palette.Face,
                             0.5f + 0.04f * (i % 2), sides: 7, toneEnd: 0.65f);
            }
            builder.Tube(new[] { wrist + round * 0.012f + knuckles * 0.004f, fist + hilt * 0.012f + round * 0.02f, fist + hilt * 0.026f + round * 0.012f + knuckles * 0.01f },
                         new[] { 0.01f, 0.0085f, 0.006f }, Palette.Face, 0.5f, sides: 7, toneEnd: 0.65f);

            // Grip wound with leather and gold wire, the pommel below the little finger, the guard above the index finger
            builder.Tube(new[] { fist - hilt * 0.04f, fist + hilt * 0.038f }, new[] { 0.0095f, 0.0095f }, Palette.Leather, 0.3f, sides: 6, toneEnd: 0.55f);
            Vector3 pommel = fist - hilt * 0.05f;
            builder.Ball(pommel, Vector3.one * 0.014f, Palette.Gold, 0.6f);
            foreach (float s in new[] { -1f, 1f })
                builder.Ball(pommel + round * (s * 0.011f), new Vector3(0.006f, 0.006f, 0.006f), Palette.Royal, 0.85f);
            // The sword is turned in the hand with its edges up and down, so the guard runs up and down across them
            Vector3 guard = fist + hilt * 0.045f;
            builder.Tube(new[] { guard - round * 0.055f + hilt * 0.012f, guard - round * 0.028f, guard, guard + round * 0.028f,
                                 guard + round * 0.055f + hilt * 0.012f },
                         new[] { 0.006f, 0.009f, 0.011f, 0.009f, 0.006f }, Palette.Gold, 0.6f, sides: 6);
            foreach (float s in new[] { -1f, 1f })
                builder.Ball(guard + round * (s * 0.058f) + hilt * 0.014f, new Vector3(0.009f, 0.009f, 0.009f), Palette.Gold, 0.75f);

            // The blade, its flat facing the sides, a fuller down its middle and gold runes along it
            Vector3 Blade(float along, float across = 0f) => guard + hilt * along + round * across;
            builder.Tube(new[] { Blade(0.005f), Blade(0.12f), Blade(0.26f), Blade(0.36f), Blade(0.42f) }, new[] { 0.016f, 0.02f, 0.017f, 0.01f, 0.001f },
                         Palette.Silver, 0.45f, sides: 6, squash: new Vector2(0.22f, 1f), roundStart: false, roundEnd: false, toneEnd: 0.9f);
            builder.Tube(new[] { Blade(0.02f), Blade(0.33f) }, new[] { 0.004f, 0.002f }, Palette.Silver, 0.2f, sides: 4);
            for (int rune = 0; rune < 5; rune++)
                builder.Ball(Blade(0.06f + rune * 0.05f) + knuckles * 0.004f, new Vector3(0.004f, 0.004f, 0.004f), Palette.Gold, 0.7f);

            return builder.ToMesh();
        }

        // Unit long piece of bowstring along +Z, stretched between its two ends by the tower
        public static Mesh BuildString()
        {
            Builder builder = new(351, shadeNoise: 0.2f, noiseScale: 6f);
            builder.Cylinder(new Vector3(0f, 0f, 0.5f), Vector3.forward, 0.0045f, 1f, 4, Palette.Bone, 0.8f, true);

            return builder.ToMesh();
        }

        // An arrow with its tip at the origin and its shaft running back along -Z: a leaf shaped silver head, a pale
        // shaft, white feathers. The one lying on the bow and the one in flight
        public static Mesh BuildArrow()
        {
            Builder builder = new(353, shadeNoise: 0.3f, noiseScale: 6f);
            Arrow(builder, Vector3.zero, Vector3.forward, 1f);

            return builder.ToMesh();
        }

        // A splinter of the shaft flung off where the arrow hits. Unit long, the particle system scales it
        public static Mesh BuildSplinter()
        {
            Builder builder = new(357, shadeNoise: 0.5f, noiseScale: 4f);
            builder.TaperedBeam(new Vector3(0f, 0f, -0.5f), new Vector3(0.04f, 0f, 0.5f), 0.14f, 0.08f, 0.02f, 0.02f, Palette.Wood, 0.6f);

            return builder.ToMesh();
        }

        // An arrow with its tip at tip pointing along direction, scaled by size
        private static void Arrow(Builder builder, Vector3 tip, Vector3 direction, float size)
        {
            direction = direction.normalized;
            Vector3 across = Vector3.Cross(direction, Mathf.Abs(direction.y) > 0.9f ? Vector3.right : Vector3.up).normalized;
            Vector3 across2 = Vector3.Cross(direction, across);
            float length = ArrowLength * size;
            Vector3 end = tip - direction * length;

            builder.Tube(new[] { end + direction * 0.008f * size, tip - direction * 0.045f * size }, new[] { 0.0042f * size, 0.0048f * size }, Palette.Wood, 0.45f,
                         sides: 6, roundStart: false, roundEnd: false, toneEnd: 0.75f);

            // Broadhead: a socket over the shaft, two wide barbed blades meeting in a point
            builder.Tube(new[] { tip - direction * 0.05f * size, tip - direction * 0.03f * size }, new[] { 0.0055f * size, 0.0045f * size }, Palette.Silver, 0.35f,
                         sides: 6, roundStart: false);
            Vector3 Head(float along, float wide) => tip - direction * (along * size) + across * (wide * size);
            builder.Flag(Head(0f, 0f), Head(0.04f, 0.012f), Head(0.026f, 0f), Palette.Silver, 0.75f);
            builder.Flag(Head(0f, 0f), Head(0.026f, 0f), Head(0.04f, -0.012f), Palette.Silver, 0.6f);

            // Cresting: two coloured rings below the feathers
            foreach ((float at, Palette palette) in new[] { (0.1f, Palette.Royal), (0.115f, Palette.Gold) })
                builder.Cylinder(end + direction * (at * size), direction, 0.005f * size, 0.008f * size, 6, palette, 0.65f, false);

            // Three feathers, each a long low vane rising to a rounded back
            for (int vane = 0; vane < 3; vane++)
            {
                float angle = vane * Mathf.PI * 2 / 3;
                Vector3 outward = across * Mathf.Cos(angle) + across2 * Mathf.Sin(angle);
                Vector3 Vane(float along, float rise) => end + direction * (along * size) + outward * ((0.004f + rise) * size);
                float tone = vane == 0 ? 0.35f : 0.75f;
                builder.Flag(Vane(0.015f, 0f), Vane(0.085f, 0f), Vane(0.03f, 0.016f), Palette.Bone, tone);
                builder.Flag(Vane(0.015f, 0f), Vane(0.03f, 0.016f), Vane(0.02f, 0.017f), Palette.Bone, tone - 0.05f);
            }

            // The nock: a little split cap at the end
            foreach (float s in new[] { -1f, 1f })
                builder.Ball(end + direction * 0.002f * size + across * (s * 0.003f * size), new Vector3(0.0025f, 0.0025f, 0.0025f) * size, Palette.Bone, 0.6f);
        }

        // Longbow held at the grip: two limbs of pale wood curving back from the leather wrapped grip and out again at
        // their tips, gold caps at the tips and gold leaves inlaid at the grip
        private static void BuildBow(Builder builder)
        {
            Vector3 grip = new(-0.06f, arrowLineY - 0.02f, bowGripZ);

            // The riser: thick at the grip, an arrow shelf cut in it above the hand, swelling into the limbs
            builder.Tube(new[] { grip + new Vector3(0f, -0.07f, -0.008f), grip + new Vector3(0f, -0.03f, 0.004f), grip + new Vector3(0f, 0.02f, 0.006f),
                                 grip + new Vector3(0f, 0.07f, -0.006f) },
                         new[] { 0.014f, 0.019f, 0.017f, 0.014f }, Palette.DarkWood, 0.35f, sides: 14, squash: new Vector2(0.75f, 1f), toneEnd: 0.6f);
            builder.Sculpt(grip + new Vector3(0.012f, 0.03f, 0.006f), new Vector3(0.006f, 0.008f, 0.012f), new[] { (Vector3.right, 0.6f, 0.2f) }, Palette.Leather,
                           direction => 0.4f, 2);

            foreach (float s in new[] { 1f, -1f })
            {
                Vector3 tip = s > 0f ? BowTopTip : BowBottomTip;
                // Each limb bends back towards the archer, then flicks forward again at its tip
                Vector3[] limb =
                {
                    grip + new Vector3(0f, s * 0.06f, -0.004f),
                    grip + new Vector3(0f, s * 0.13f, -0.016f),
                    grip + new Vector3(0f, s * 0.21f, -0.036f),
                    new(tip.x, tip.y - s * 0.035f, tip.z - 0.016f),
                    tip + new Vector3(0f, s * 0.008f, 0.02f),
                };
                builder.Tube(limb, new[] { 0.014f, 0.012f, 0.0095f, 0.007f, 0.004f }, Palette.Wood, 0.3f, sides: 8, squash: new Vector2(0.55f, 1f), toneEnd: 0.85f);
                // The string looped round the limb at the tip
                builder.Torus(tip + new Vector3(0f, -s * 0.004f, 0f), limb[4] - limb[3], new Vector2(0.007f, 0.007f), 0.0022f, Palette.Bone, 0.75f, segments: 8,
                              sides: 4);
                builder.Ball(tip, new Vector3(0.008f, 0.012f, 0.008f), Palette.Gold, 0.7f);
                for (int leaf = 1; leaf <= 3; leaf++)
                {
                    Vector3 at = Vector3.Lerp(limb[leaf], limb[leaf + 1], 0.4f) + new Vector3(0f, 0f, 0.012f);
                    builder.Sculpt(at, new Vector3(0.006f, 0.016f, 0.006f), new[] { (Vector3.forward, 0.6f, 0.2f) }, Palette.Gold,
                                   direction => 0.5f + 0.3f * direction.y, 2);
                }
            }
            // Leather wrapped round the grip in turns
            builder.Tube(new[] { grip + Vector3.down * 0.035f, grip + Vector3.up * 0.025f }, new[] { 0.02f, 0.02f }, Palette.Leather, 0.3f, sides: 8, wrinkle: 0.1f,
                         toneEnd: 0.5f);
            for (int turn = 0; turn < 5; turn++)
                builder.Torus(grip + Vector3.up * (-0.03f + turn * 0.013f), new Vector3(0f, 1f, 0.25f), new Vector2(0.021f, 0.016f), 0.0022f, Palette.Leather, 0.2f,
                              segments: 10, sides: 4);
            foreach (float s in new[] { 1f, -1f })
                builder.Ball(grip + new Vector3(0f, s * 0.05f, 0.012f), new Vector3(0.006f, 0.014f, 0.004f), Palette.Gold, 0.75f);
        }

        // An elven archer woman's body in her own space, on the origin, facing +Z: slender legs in dark leggings, tall
        // armoured boots of olive metal chased with pale filigree and ending in long points, a dark fitted undershirt, a
        // corset of olive armour shaped to her with chased filigree and two shaped cups, a gold necklace of leaves, a
        // belt slung at her waist and a skirt of long pointed strips in green and pale grey hanging from it, layered olive
        // shoulder guards
        private static void FemaleBody(Builder builder)
        {
            // The legs are meshes of their own

            // The dark undershirt: hips wider than the narrow waist, the chest above it, the shoulders, the neck
            builder.Tube(new[] { new Vector3(0f, 0.44f, 0.005f), new Vector3(0f, 0.51f, 0.006f), new Vector3(0f, 0.585f, 0.002f), new Vector3(0f, 0.66f, 0.004f),
                                 new Vector3(0f, 0.74f, 0f), new Vector3(0f, 0.8f, 0f), new Vector3(0f, 0.84f, 0f) },
                         new[] { 0.1f, 0.1f, 0.074f, 0.084f, 0.096f, 0.09f, 0.042f }, Palette.Royal, 0.12f, sides: 32, squash: new Vector2(1f, 0.74f), roundStart: false,
                         wrinkle: 0.03f, toneEnd: 0.35f);

            // The corset of olive armour from the hips up under the chest, chased with pale filigree, a gold edge along
            // its top; two shaped cups above it
            builder.Tube(new[] { new Vector3(0f, 0.5f, 0.006f), new Vector3(0f, 0.585f, 0.003f), new Vector3(0f, 0.67f, 0.005f), new Vector3(0f, 0.725f, 0.004f) },
                         new[] { 0.106f, 0.08f, 0.091f, 0.1f }, Palette.Feather, 0.35f, sides: 32, squash: new Vector2(1f, 0.76f), roundStart: false, roundEnd: false,
                         toneEnd: 0.7f);
            builder.Torus(new Vector3(0f, 0.725f, 0.004f), Vector3.up, new Vector2(0.1f, 0.076f), 0.004f, Palette.Gold, 0.65f, segments: 32, sides: 5);
            foreach (float side in new[] { -1f, 1f })
            {
                builder.Ball(new Vector3(side * 0.034f, 0.725f, 0.058f), new Vector3(0.036f, 0.03f, 0.027f), Palette.Feather, 0.62f);

                // Filigree: a pale line curling up from the waist over each side of the corset and out across the cup
                Vector3[] curl =
                {
                    new(side * 0.012f, 0.56f, 0.067f), new(side * 0.03f, 0.6f, 0.072f), new(side * 0.05f, 0.64f, 0.07f), new(side * 0.044f, 0.69f, 0.08f),
                    new(side * 0.022f, 0.71f, 0.083f),
                };
                builder.Tube(curl, new[] { 0.0025f, 0.003f, 0.003f, 0.0028f, 0.002f }, Palette.Bone, 0.7f, sides: 5);
                builder.Tube(new[] { new Vector3(side * 0.05f, 0.64f, 0.07f), new Vector3(side * 0.074f, 0.62f, 0.055f), new Vector3(side * 0.082f, 0.58f, 0.045f) },
                             new[] { 0.0025f, 0.0025f, 0.0015f }, Palette.Bone, 0.65f, sides: 5);
                builder.Tube(new[] { new Vector3(side * 0.018f, 0.735f, 0.085f), new Vector3(side * 0.04f, 0.75f, 0.083f), new Vector3(side * 0.058f, 0.735f, 0.07f) },
                             new[] { 0.002f, 0.0024f, 0.0018f }, Palette.Bone, 0.7f, sides: 5);

                // Layered shoulder guards of olive armour with filigree on the top plate
                for (int plate = 0; plate < 2; plate++)
                {
                    Vector3 center = new(side * (0.114f + plate * 0.012f), 0.797f - plate * 0.026f, 0f);
                    builder.Sculpt(center, new Vector3(0.046f, 0.02f, 0.046f) * (1f - 0.1f * plate), new[]
                    {
                        (new Vector3(side, -0.6f, 0f), 0.55f, 0.25f),
                        (Vector3.up, 0.5f, 0.12f),
                    }, Palette.Feather, direction => 0.45f - 0.1f * plate + 0.3f * direction.y, 3);
                }
                builder.Tube(new[] { new Vector3(side * 0.09f, 0.818f, 0.02f), new Vector3(side * 0.118f, 0.82f, 0f), new Vector3(side * 0.14f, 0.8f, -0.02f) },
                             new[] { 0.002f, 0.0025f, 0.0015f }, Palette.Bone, 0.7f, sides: 5);
            }

            // A gold necklace of little leaves lying on the collarbones
            builder.Torus(new Vector3(0f, 0.795f, 0.012f), new Vector3(0f, 1f, 0.35f), new Vector2(0.045f, 0.04f), 0.003f, Palette.Gold, 0.65f, segments: 24, sides: 4);
            for (int leaf = -4; leaf <= 4; leaf++)
            {
                float angle = leaf * 0.24f;
                Vector3 at = new(Mathf.Sin(angle) * 0.045f, 0.782f - Mathf.Cos(angle) * 0.008f, 0.014f + Mathf.Cos(angle) * 0.042f);
                builder.Ball(at, new Vector3(0.0055f, 0.011f, 0.004f), Palette.Gold, 0.6f + 0.1f * (leaf % 2));
            }

            // A dark leather cincher laced round the waist under the armour's lower edge
            builder.Tube(new[] { new Vector3(0f, 0.51f, 0.006f), new Vector3(0f, 0.56f, 0.004f), new Vector3(0f, 0.6f, 0.003f) }, new[] { 0.109f, 0.091f, 0.084f },
                         Palette.Leather, 0.15f, sides: 32, squash: new Vector2(1f, 0.76f), roundStart: false, roundEnd: false, toneEnd: 0.35f);
            for (int lace = 0; lace < 3; lace++)
                builder.Tube(new[] { new Vector3(-0.01f, 0.52f + lace * 0.025f, 0.088f - lace * 0.006f), new Vector3(0.01f, 0.53f + lace * 0.025f, 0.088f - lace * 0.006f) },
                             new[] { 0.002f, 0.002f }, Palette.Gold, 0.6f, sides: 4);

            // Belt slung at the waist, a gold clasp
            builder.Torus(new Vector3(0f, 0.53f, 0.004f), new Vector3(0.12f, 1f, 0f), new Vector2(0.104f, 0.08f), 0.008f, Palette.Leather, 0.25f, segments: 32,
                          sides: 6);
            builder.Ball(new Vector3(0f, 0.53f, 0.086f), new Vector3(0.014f, 0.012f, 0.005f), Palette.Gold, 0.7f);

            // The skirt: a shorter pale grey underskirt of strips round the hips, and over it long green panels hanging
            // in front and behind down past the knees, light at the belt and darkening to ragged points
            for (int i = 0; i < 12; i++)
            {
                float angle = (i + 0.5f) * 30f;
                SkirtStrip(builder, angle, 0.53f, 0.4f + 0.02f * (i % 2), 0.028f, Palette.Silver, 0.6f, 0.002f);
            }
            foreach (float facing in new[] { 0f, 180f })
            {
                for (int panel = -2; panel <= 2; panel++)
                {
                    float angle = facing + panel * 17f;
                    float bottom = 0.2f + 0.035f * Mathf.Abs(panel) + (facing > 0f ? 0.04f : 0f);
                    SkirtStrip(builder, angle, 0.535f, bottom, 0.026f, Palette.Moss, 0.75f - 0.06f * Mathf.Abs(panel), 0.01f);
                }
            }
        }

        // One strip of the skirt at an angle round her: a flat panel from the belt down to a point, lying out from her hips
        // and flaring as it falls, its colour darkening towards its point
        private static void SkirtStrip(Builder builder, float angle, float top, float bottom, float halfWidth, Palette palette, float tone, float lift)
        {
            Vector3 outward = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            Vector3 across = Quaternion.Euler(0f, angle + 90f, 0f) * Vector3.forward;
            Vector3 Surface(float y, float away) => Vector3.Scale(outward, new Vector3(0.108f, 0f, 0.084f)) + outward * (away + lift) + Vector3.up * y +
                                                    Vector3.forward * 0.004f;

            // The panel falls in three lengths, shading darker as it goes, and ends in two ragged points
            Vector3 Edge(float t, float side) => Surface(Mathf.Lerp(top, bottom + 0.05f, t), 0.004f + 0.02f * t) + across * (side * halfWidth * (1f + 0.1f * t));
            for (int band = 0; band < 3; band++)
            {
                float from = band / 3f, to = (band + 1) / 3f;
                float shade = tone + 0.12f - 0.12f * band;
                builder.Flag(Edge(from, -1f), Edge(from, 1f), Edge(to, 1f), palette, shade);
                builder.Flag(Edge(from, -1f), Edge(to, 1f), Edge(to, -1f), palette, shade - 0.03f);
            }
            Vector3 middle = Surface(bottom + 0.035f, 0.024f);
            builder.Flag(Edge(1f, -1f), middle, Surface(bottom, 0.026f) - across * (halfWidth * 0.55f), palette, tone - 0.28f);
            builder.Flag(middle, Edge(1f, 1f), Surface(bottom - 0.012f, 0.028f) + across * (halfWidth * 0.5f), palette, tone - 0.3f);
            builder.Flag(Edge(1f, -1f), Edge(1f, 1f), middle, palette, tone - 0.22f);
        }

        // A tall armoured boot at x, its foot pointing ahead along +Z: a shaft of olive armour shaped to the calf, a knee
        // guard, pale filigree curling down the shin, a slender foot ending in a long point that turns up, a dark sole
        private static void FemaleBoot(Builder builder, float x)
        {
            builder.Tube(new[] { new Vector3(x, 0.05f, -0.004f), new Vector3(x, 0.1f, 0f), new Vector3(x, 0.18f, -0.002f), new Vector3(x, 0.27f, 0.006f),
                                 new Vector3(x, 0.31f, 0.01f) },
                         new[] { 0.027f, 0.025f, 0.035f, 0.032f, 0.036f }, Palette.Feather, 0.3f, sides: 22, roundStart: false, roundEnd: false, toneEnd: 0.6f);
            builder.Sculpt(new Vector3(x, 0.3f, 0.032f), Vector3.one * 0.022f, new[] { (Vector3.forward, 0.6f, 0.25f), (Vector3.up, 0.5f, 0.3f) }, Palette.Feather,
                           direction => 0.55f + 0.2f * direction.y, 3);
            builder.Tube(new[] { new Vector3(x, 0.27f, 0.038f), new Vector3(x * 1.1f, 0.21f, 0.035f), new Vector3(x * 0.92f, 0.15f, 0.033f),
                                 new Vector3(x * 1.08f, 0.09f, 0.031f) },
                         new[] { 0.0025f, 0.003f, 0.0028f, 0.0018f }, Palette.Bone, 0.7f, sides: 5);

            builder.Tube(new[] { new Vector3(x, 0.035f, -0.028f), new Vector3(x, 0.04f, 0.015f), new Vector3(x * 1.03f, 0.03f, 0.065f), new Vector3(x * 1.05f, 0.02f, 0.11f),
                                 new Vector3(x * 1.05f, 0.04f, 0.14f) },
                         new[] { 0.028f, 0.029f, 0.022f, 0.01f, 0.002f }, Palette.Feather, 0.4f, sides: 18, squash: new Vector2(1f, 0.62f), toneEnd: 0.7f);
            builder.Tube(new[] { new Vector3(x, 0.008f, -0.03f), new Vector3(x, 0.008f, 0.03f), new Vector3(x * 1.03f, 0.009f, 0.075f), new Vector3(x * 1.05f, 0.012f, 0.105f) },
                         new[] { 0.028f, 0.029f, 0.022f, 0.009f }, Palette.Royal, 0.15f, sides: 18, squash: new Vector2(1f, 0.25f), roundStart: false);
        }

        // An elven woman's head: a fine oval face with high cheekbones, a small straight nose, full lips and a gently
        // pointed chin, large almond eyes with dark lashes under fine arched brows, long pointed ears with gold drops
        // hanging from them, and long thick hair falling in waves down her back and over her shoulders, parted in the
        // middle
        private static void FemaleHead(Builder builder, Palette hair)
        {
            Vector3 head = new(0f, 0.93f, 0.01f);
            Vector3 headRadii = new(0.057f, 0.072f, 0.063f);
            Vector3 eyeSocket = new(0.38f, 0.1f, 0.9f);
            Vector3 cheek = new(0.58f, -0.15f, 0.78f);
            builder.Sculpt(head, headRadii, new[]
            {
                (new Vector3(0f, -0.06f, 1f), 0.09f, 0.12f),
                (new Vector3(0f, -0.17f, 1f), 0.08f, 0.07f),
                (eyeSocket, 0.14f, -0.05f),
                (Mirror(eyeSocket), 0.14f, -0.05f),
                (cheek, 0.24f, 0.09f),
                (Mirror(cheek), 0.24f, 0.09f),
                (new Vector3(0f, -0.84f, 0.52f), 0.26f, 0.12f),
                (new Vector3(0.55f, -0.65f, 0.45f), 0.3f, -0.08f),
                (new Vector3(-0.55f, -0.65f, 0.45f), 0.3f, -0.08f),
            }, Palette.Face, direction => 0.6f
                - 0.15f * (Falloff(direction, eyeSocket, 0.15f) + Falloff(direction, Mirror(eyeSocket), 0.15f))
                + 0.14f * (Falloff(direction, cheek, 0.2f) + Falloff(direction, Mirror(cheek), 0.2f))
                + 0.1f * direction.y, 4);

            builder.Tube(new[] { new Vector3(0f, 0.81f, 0f), new Vector3(0f, 0.88f, 0.005f) }, new[] { 0.024f, 0.022f }, Palette.Face, 0.35f, sides: 14,
                         toneEnd: 0.6f);

            // Full lips, a shade deeper than the skin
            Vector3 mouth = head + new Vector3(0f, -0.042f, 0.057f);
            builder.Tube(new[] { mouth + new Vector3(-0.012f, 0.002f, -0.002f), mouth + new Vector3(0f, 0.004f, 0.003f), mouth + new Vector3(0.012f, 0.002f, -0.002f) },
                         new[] { 0.0022f, 0.0028f, 0.0022f }, Palette.Face, 0.68f, sides: 6);
            builder.Tube(new[] { mouth + new Vector3(-0.009f, -0.004f, -0.002f), mouth + new Vector3(0f, -0.0065f, 0.003f), mouth + new Vector3(0.009f, -0.004f, -0.002f) },
                         new[] { 0.0028f, 0.0036f, 0.0028f }, Palette.Face, 0.72f, sides: 6);

            foreach (float side in new[] { -1f, 1f })
            {
                // Large almond eyes: white, a green iris, a dark pupil, dark lashes along the upper lid, a fine arched brow
                Vector3 socket = new(eyeSocket.x * side, eyeSocket.y, eyeSocket.z);
                Vector3 eye = head + Vector3.Scale(socket.normalized * 0.93f, headRadii);
                Vector3 look = (socket.normalized + Vector3.forward * 2f).normalized;
                builder.Ball(eye, new Vector3(0.016f, 0.0095f, 0.01f), Palette.Bone, 0.95f);
                builder.Ball(eye + look * 0.007f, new Vector3(0.0075f, 0.0075f, 0.004f), Palette.Moss, 0.6f);
                builder.Ball(eye + look * 0.009f, new Vector3(0.0035f, 0.0035f, 0.002f), Palette.DarkWood, 0f);
                builder.Tube(new[] { eye + new Vector3(-0.017f * side, 0.004f, 0.004f), eye + new Vector3(0f, 0.0095f, 0.009f),
                                     eye + new Vector3(0.022f * side, 0.008f, 0.001f) },
                             new[] { 0.0022f, 0.0026f, 0.0012f }, Palette.DarkWood, 0.05f, sides: 5);
                builder.Tube(new[] { eye + new Vector3(-0.012f * side, 0.018f, 0.007f), eye + new Vector3(0.004f * side, 0.024f, 0.006f),
                                     eye + new Vector3(0.02f * side, 0.02f, 0f) },
                             new[] { 0.0018f, 0.0024f, 0.001f }, hair, 0.25f, sides: 5);

                // Long pointed ear sweeping up and back, its inner fold, a gold drop hanging from the lobe
                builder.Tube(new[] { new Vector3(0.053f * side, 0.91f, 0.002f), new Vector3(0.059f * side, 0.93f, -0.006f), new Vector3(0.076f * side, 0.955f, -0.022f),
                                     new Vector3(0.098f * side, 0.99f, -0.045f) },
                             new[] { 0.011f, 0.015f, 0.01f, 0.0015f }, Palette.Face, 0.5f, sides: 10, squash: new Vector2(1f, 0.4f), toneEnd: 0.7f);
                builder.Tube(new[] { new Vector3(0.059f * side, 0.925f, 0.002f), new Vector3(0.072f * side, 0.95f, -0.014f), new Vector3(0.087f * side, 0.975f, -0.032f) },
                             new[] { 0.004f, 0.005f, 0.0015f }, Palette.Face, 0.2f, sides: 6);
                builder.Tube(new[] { new Vector3(0.056f * side, 0.903f, 0.004f), new Vector3(0.057f * side, 0.885f, 0.006f) }, new[] { 0.0015f, 0.0015f }, Palette.Gold,
                             0.6f, sides: 4);
                builder.Ball(new Vector3(0.057f * side, 0.878f, 0.006f), new Vector3(0.004f, 0.007f, 0.003f), Palette.Gold, 0.75f);

                // Thick hair falling past the ear and in front of the shoulder down onto the breast, in waves
                for (int strand = 0; strand < 4; strand++)
                {
                    float spread = strand * 0.008f;
                    float wave = (strand % 2 == 0 ? 1f : -1f) * 0.006f;
                    builder.Tube(new[] { new Vector3((0.035f + spread * 0.4f) * side, 1.0f, 0.03f - spread), new Vector3((0.064f + spread) * side, 0.94f, 0.012f - spread),
                                         new Vector3((0.078f + spread) * side + wave, 0.86f, 0.03f - spread), new Vector3((0.085f + spread) * side - wave, 0.78f, 0.06f - spread),
                                         new Vector3((0.08f + spread) * side + wave, 0.7f, 0.078f - spread), new Vector3((0.07f + spread) * side, 0.64f + 0.012f * strand, 0.08f - spread) },
                                 new[] { 0.01f, 0.013f, 0.014f, 0.013f, 0.009f, 0.0025f }, hair, 0.25f + 0.1f * strand, sides: 10, wrinkle: 0.12f, toneEnd: 0.75f);
                }

                // A braid down the left side, from behind the ear to the breast, tied with gold
                if (side < 0f)
                {
                    Vector3[] braid =
                    {
                        new(-0.06f, 0.95f, -0.01f), new(-0.08f, 0.88f, 0.02f), new(-0.088f, 0.8f, 0.05f), new(-0.082f, 0.72f, 0.07f), new(-0.075f, 0.66f, 0.075f),
                    };
                    builder.Tube(braid, new[] { 0.009f, 0.0095f, 0.009f, 0.008f, 0.006f }, hair, 0.3f, sides: 10, wrinkle: 0.35f, toneEnd: 0.6f);
                    for (int twist = 1; twist < 8; twist++)
                    {
                        Vector3 at = Vector3.Lerp(braid[0], braid[4], twist / 8f);
                        builder.Torus(at, braid[4] - braid[0], new Vector2(0.0095f, 0.0095f), 0.002f, hair, 0.15f, segments: 10, sides: 4);
                    }
                    builder.Torus(braid[4], braid[4] - braid[3], new Vector2(0.007f, 0.007f), 0.0025f, Palette.Gold, 0.7f, segments: 10, sides: 4);
                    builder.Tube(new[] { braid[4], braid[4] + new Vector3(0f, -0.035f, 0.004f) }, new[] { 0.0065f, 0.0015f }, hair, 0.5f, sides: 6, toneEnd: 0.8f);
                }

                // The front of the hair swept from the parting down over the temple
                for (int strand = 0; strand < 3; strand++)
                {
                    float y = 1.005f - strand * 0.006f;
                    builder.Tube(new[] { new Vector3(0.004f * side, y + 0.004f, 0.05f - strand * 0.005f), new Vector3((0.03f + strand * 0.005f) * side, y - 0.006f, 0.058f - strand * 0.005f),
                                         new Vector3((0.052f + strand * 0.004f) * side, y - 0.04f, 0.04f - strand * 0.005f) },
                                 new[] { 0.007f, 0.008f, 0.004f }, hair, 0.4f + 0.12f * strand, sides: 8, toneEnd: 0.7f);
                }
            }

            // The crown, and the long hair falling in waves down her back to below the shoulder blades
            builder.Sculpt(head + new Vector3(0f, 0.02f, -0.01f), new Vector3(0.066f, 0.07f, 0.072f), new[]
            {
                (new Vector3(0f, -0.4f, 1f), 0.5f, -0.6f),
                (new Vector3(0f, 1f, 0f), 0.4f, 0.08f),
                (new Vector3(0f, -0.5f, -1f), 0.5f, 0.15f),
            }, hair, direction => 0.3f + 0.45f * direction.y, 3);
            for (int lock_ = 0; lock_ < 13; lock_++)
            {
                float across = (lock_ - 6) / 6f;
                float wave = (lock_ % 2 == 0 ? 1f : -1f) * (0.007f + 0.002f * (lock_ % 3));
                float end = 0.5f + 0.06f * Mathf.Abs(across) + 0.03f * ((lock_ * 7) % 4) / 3f;
                Vector3[] path =
                {
                    new(across * 0.034f, 0.998f, -0.045f),
                    new(across * 0.056f, 0.94f, -0.086f),
                    new(across * 0.068f + wave, 0.85f, -0.11f),
                    new(across * 0.072f - wave, 0.76f, -0.118f),
                    new(across * 0.068f + wave, 0.66f, -0.12f),
                    new(across * 0.06f - wave * 0.5f, (0.66f + end) * 0.5f, -0.116f),
                    new(across * 0.052f, end, -0.11f),
                };
                builder.Tube(path, new[] { 0.014f, 0.016f, 0.017f, 0.016f, 0.014f, 0.01f, 0.003f }, hair, 0.18f + 0.07f * (lock_ % 3), sides: 10,
                             squash: new Vector2(1f, 0.65f), wrinkle: 0.15f, toneEnd: 0.75f);
                foreach (float s in new[] { -1f, 1f })
                {
                    Vector3 offset = new(s * 0.006f, 0f, -0.007f);
                    builder.Tube(new[] { path[1] + offset, path[2] + offset * 1.3f, path[3] + offset, path[4] + offset * 0.8f, path[5] + offset * 0.5f,
                                         path[6] + new Vector3(0f, 0.03f, -0.004f) },
                                 new[] { 0.004f, 0.0045f, 0.0045f, 0.004f, 0.003f, 0.001f }, hair, 0.45f + 0.12f * ((lock_ + (s > 0f ? 1 : 0)) % 3), sides: 6,
                                 toneEnd: 0.9f);
                }
            }
        }

        // An elf's body in his own space, on the origin, facing +Z: slender legs in leggings, tall soft boots folded at
        // the top, a long tunic belted at the waist with a split skirt, a leather bodice, a hood lying round his
        // shoulders and a cloak falling down his back. armoured adds a silver breastplate and pauldrons
        private static void ElfBody(Builder builder, Palette tunic, Palette cloak, bool armoured)
        {
            // The legs are meshes of their own

            // Tunic: slim at the waist, flaring a little to a split skirt down to the knees, darker at its hem
            builder.Tube(new[] { new Vector3(0f, 0.36f, 0.005f), new Vector3(0f, 0.46f, 0.005f), new Vector3(0f, 0.56f, 0f), new Vector3(0f, 0.68f, 0f),
                                 new Vector3(0f, 0.77f, 0f), new Vector3(0f, 0.84f, 0f) },
                         new[] { 0.1f, 0.09f, 0.084f, 0.1f, 0.115f, 0.05f }, tunic, 0.25f, sides: 32, squash: new Vector2(1f, 0.75f), roundStart: false,
                         wrinkle: 0.06f, toneEnd: 0.7f);
            builder.Torus(new Vector3(0f, 0.36f, 0.005f), Vector3.up, new Vector2(0.1f, 0.075f), 0.006f, Palette.Gold, 0.55f, segments: 24, waviness: 0.008f);
            builder.Tube(new[] { new Vector3(0f, 0.355f, 0.08f), new Vector3(0f, 0.5f, 0.072f) }, new[] { 0.004f, 0.004f }, Palette.DarkWood, 0.2f, sides: 4);

            // Leather bodice laced up the front, a belt with a gold leaf buckle
            builder.Tube(new[] { new Vector3(0f, 0.53f, 0.003f), new Vector3(0f, 0.6f, 0.002f), new Vector3(0f, 0.7f, 0f) }, new[] { 0.097f, 0.096f, 0.11f },
                         Palette.Leather, 0.3f, sides: 32, squash: new Vector2(1f, 0.78f), roundStart: false, roundEnd: false, toneEnd: 0.6f);
            for (int lace = 0; lace < 4; lace++)
            {
                float y = 0.56f + lace * 0.035f;
                builder.Tube(new[] { new Vector3(-0.012f, y, 0.081f), new Vector3(0.012f, y + 0.015f, 0.082f) }, new[] { 0.0028f, 0.0028f }, Palette.Bone, 0.6f, sides: 4);
                builder.Tube(new[] { new Vector3(0.012f, y, 0.081f), new Vector3(-0.012f, y + 0.015f, 0.082f) }, new[] { 0.0028f, 0.0028f }, Palette.Bone, 0.6f, sides: 4);
            }
            builder.Torus(new Vector3(0f, 0.52f, 0.003f), Vector3.up, new Vector2(0.104f, 0.082f), 0.012f, Palette.Leather, 0.2f, segments: 28, waviness: 0.004f);
            builder.Ball(new Vector3(0f, 0.52f, 0.088f), new Vector3(0.022f, 0.016f, 0.007f), Palette.Gold, 0.7f);

            if (armoured)
            {
                builder.Tube(new[] { new Vector3(0f, 0.58f, 0.004f), new Vector3(0f, 0.67f, 0.002f), new Vector3(0f, 0.76f, 0f) }, new[] { 0.106f, 0.12f, 0.127f },
                             Palette.Silver, 0.35f, sides: 32, squash: new Vector2(1f, 0.8f), roundStart: false, roundEnd: false, toneEnd: 0.85f);
                builder.Tube(new[] { new Vector3(0f, 0.6f, 0.095f), new Vector3(0f, 0.7f, 0.099f), new Vector3(0f, 0.75f, 0.088f) }, new[] { 0.005f, 0.006f, 0.005f },
                             Palette.Gold, 0.65f, sides: 5);
                // Gold filigree curling out from the ridge across the chest, a leaf at each end
                foreach (float side in new[] { -1f, 1f })
                {
                    for (int curl = 0; curl < 2; curl++)
                    {
                        float y = 0.66f + curl * 0.05f;
                        Vector3[] line = { new(side * 0.008f, y, 0.099f), new(side * 0.035f, y + 0.02f, 0.095f), new(side * 0.06f, y + 0.005f, 0.085f) };
                        builder.Tube(line, new[] { 0.003f, 0.003f, 0.002f }, Palette.Gold, 0.6f, sides: 4);
                        builder.Ball(line[2], new Vector3(0.008f, 0.005f, 0.004f), Palette.Gold, 0.7f);
                    }
                }
                foreach (float side in new[] { -1f, 1f })
                {
                    builder.Sculpt(new Vector3(0.115f * side, 0.8f, 0f), new Vector3(0.06f, 0.035f, 0.055f), new[]
                    {
                        (new Vector3(side, -0.6f, 0f), 0.55f, 0.25f),
                        (Vector3.up, 0.5f, 0.15f),
                    }, Palette.Silver, direction => 0.45f + 0.3f * direction.y, 2);
                    builder.Sculpt(new Vector3(0.135f * side, 0.775f, 0f), new Vector3(0.045f, 0.025f, 0.045f), new[] { (new Vector3(side, -0.5f, 0f), 0.6f, 0.2f) },
                                   Palette.Silver, direction => 0.35f + 0.25f * direction.y, 2);
                }
            }
            else
            {
                // A leaf shaped leather guard on the left shoulder with a gold vein down it
                builder.Sculpt(new Vector3(-0.115f, 0.8f, 0f), new Vector3(0.055f, 0.03f, 0.05f), new[]
                {
                    (new Vector3(-1f, -0.6f, 0f), 0.55f, 0.25f),
                    (Vector3.up, 0.5f, 0.15f),
                }, Palette.Leather, direction => 0.4f + 0.25f * direction.y, 2);
                builder.Tube(new[] { new Vector3(-0.09f, 0.835f, 0.03f), new Vector3(-0.125f, 0.83f, 0f), new Vector3(-0.155f, 0.79f, -0.03f) },
                             new[] { 0.003f, 0.003f, 0.002f }, Palette.Gold, 0.7f, sides: 4);
            }

            // A vine of gold leaves embroidered round the hem of the tunic
            for (int leaf = 0; leaf < 18; leaf++)
            {
                float angle = leaf * Mathf.PI * 2 / 18;
                Vector3 at = new(Mathf.Sin(angle) * 0.102f, 0.38f + 0.01f * (leaf % 2), 0.005f + Mathf.Cos(angle) * 0.077f);
                builder.Ball(at, new Vector3(0.013f, 0.007f, 0.009f), Palette.Gold, 0.5f + 0.12f * (leaf % 2));
            }

            // Strips of a short armoured skirt hanging from the belt across the front, each with a gold rivet at its top and
            // a stitched hem: leather for the archers, silver edged in gold for the captain
            Palette guardPalette = armoured ? Palette.Silver : Palette.Cloak;
            foreach (float angle in new[] { -42f, -14f, 14f, 42f })
            {
                Vector3 outward = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                Vector3 across = Quaternion.Euler(0f, angle + 90f, 0f) * Vector3.forward;
                Vector3 Surface(float y, float lift) => Vector3.Scale(outward, new Vector3(0.106f, 0f, 0.083f)) + outward * lift + Vector3.up * y;
                builder.Tube(new[] { Surface(0.505f, 0.006f), Surface(0.45f, 0.01f), Surface(0.395f, 0.016f) }, new[] { 0.021f, 0.022f, 0.022f }, guardPalette,
                             armoured ? 0.6f : 0.45f, sides: 8, squash: new Vector2(1.6f, 0.28f), roundStart: false, roundEnd: false, toneEnd: armoured ? 0.35f : 0.2f);
                builder.Ball(Surface(0.495f, 0.014f), Vector3.one * 0.005f, Palette.Gold, 0.75f);
                builder.Tube(new[] { Surface(0.4f, 0.021f) - across * 0.03f, Surface(0.398f, 0.023f), Surface(0.4f, 0.021f) + across * 0.03f },
                             new[] { 0.002f, 0.002f, 0.002f }, armoured ? Palette.Gold : Palette.Bone, armoured ? 0.65f : 0.45f, sides: 4);
            }

            if (armoured)
            {
                // A gorget round the neck, lames over the belly below the breastplate, rivets along its lower edge
                builder.Torus(new Vector3(0f, 0.8f, 0f), Vector3.up, new Vector2(0.05f, 0.046f), 0.012f, Palette.Silver, 0.65f, segments: 20, sides: 6);
                builder.Torus(new Vector3(0f, 0.81f, 0f), Vector3.up, new Vector2(0.046f, 0.042f), 0.0035f, Palette.Gold, 0.65f, segments: 20, sides: 4);
                for (int lame = 0; lame < 2; lame++)
                {
                    float y = 0.565f - lame * 0.026f;
                    builder.Torus(new Vector3(0f, y, 0.003f), Vector3.up, new Vector2(0.104f - lame * 0.003f, 0.082f - lame * 0.002f), 0.007f, Palette.Silver,
                                  0.45f - 0.08f * lame, segments: 28, sides: 6);
                }
                for (int rivet = 0; rivet < 9; rivet++)
                {
                    float angle = (rivet - 4) * 0.28f;
                    builder.Ball(new Vector3(Mathf.Sin(angle) * 0.108f, 0.588f, 0.004f + Mathf.Cos(angle) * 0.087f), Vector3.one * 0.004f, Palette.Gold, 0.8f);
                }
            }
            else
            {
                // A jerkin of leaf scales over the bodice: rows of pointed leaves overlapping like shingles, each row set
                // half a leaf over from the one above, their points tipped out a little, in greens shading from leaf to leaf;
                // a gold edged leaf at the top of each side
                const int rows = 6;
                for (int row = 0; row < rows; row++)
                {
                    float y = 0.755f - row * 0.033f;
                    float radius = Mathf.Lerp(0.112f, 0.1f, row / (rows - 1f));
                    for (int column = -4; column <= 4; column++)
                    {
                        float angle = (column + (row % 2) * 0.5f) * 0.3f;
                        if (Mathf.Abs(angle) > 1.25f)
                            continue;
                        Vector3 outward = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle) * 0.78f).normalized;
                        Vector3 across = Vector3.Cross(Vector3.up, outward).normalized;
                        Vector3 center = new(Mathf.Sin(angle) * (radius + 0.004f), y, 0.003f + Mathf.Cos(angle) * (radius * 0.78f + 0.004f));
                        Vector3 top = center + Vector3.up * 0.016f - outward * 0.002f;
                        Vector3 left = center - across * 0.0155f;
                        Vector3 right = center + across * 0.0155f;
                        Vector3 point = center - Vector3.up * 0.021f + outward * 0.006f;
                        float tone = 0.35f + 0.12f * ((row * 3 + column * 5 + 20) % 4) / 3f + 0.08f * (1f - row / (rows - 1f));
                        builder.Flag(top, left, point, Palette.Cloak, tone);
                        builder.Flag(top, point, right, Palette.Cloak, tone + 0.06f);
                        builder.Tube(new[] { top, point }, new[] { 0.0012f, 0.0012f }, Palette.Cloak, 0.15f, sides: 3);
                    }
                }
            }

            // A dagger at the right hip: a leather sheath with a gold chape, a wrapped grip, a little guard and pommel
            builder.Tube(new[] { new Vector3(0.085f, 0.5f, 0.03f), new Vector3(0.095f, 0.42f, 0.045f), new Vector3(0.1f, 0.36f, 0.055f) },
                         new[] { 0.012f, 0.011f, 0.006f }, Palette.Leather, 0.3f, sides: 6, squash: new Vector2(1.6f, 0.6f), toneEnd: 0.55f);
            builder.Ball(new Vector3(0.1f, 0.36f, 0.055f), new Vector3(0.008f, 0.01f, 0.006f), Palette.Gold, 0.65f);
            builder.Tube(new[] { new Vector3(0.083f, 0.51f, 0.027f), new Vector3(0.078f, 0.56f, 0.02f) }, new[] { 0.006f, 0.006f }, Palette.DarkWood, 0.4f, sides: 5);
            builder.Tube(new[] { new Vector3(0.07f, 0.512f, 0.03f), new Vector3(0.096f, 0.508f, 0.024f) }, new[] { 0.004f, 0.004f }, Palette.Gold, 0.6f, sides: 4);
            builder.Ball(new Vector3(0.077f, 0.565f, 0.019f), Vector3.one * 0.007f, Palette.Gold, 0.7f);

            // Hood lying in folds round the shoulders, the cloak falling from it down his back to his calves, darker below
            builder.Torus(new Vector3(0f, 0.8f, -0.015f), Vector3.up, new Vector2(0.072f, 0.06f), 0.022f, cloak, 0.55f, segments: 20, sides: 8, waviness: 0.01f);
            builder.Tube(new[] { new Vector3(0f, 0.82f, -0.065f), new Vector3(0f, 0.6f, -0.11f), new Vector3(0f, 0.38f, -0.14f), new Vector3(0f, 0.18f, -0.16f) },
                         new[] { 0.1f, 0.13f, 0.15f, 0.16f }, cloak, 0.6f, sides: 28, squash: new Vector2(1f, 0.2f), roundStart: false, roundEnd: false,
                         wrinkle: 0.12f, toneEnd: 0.2f);
            builder.Torus(new Vector3(0f, 0.18f, -0.16f), Vector3.up, new Vector2(0.16f, 0.032f), 0.006f, Palette.Gold, 0.55f, segments: 24, sides: 4,
                          arcStart: 0f, arcDegrees: 360f, waviness: 0.006f);
            // A leaf shaped gold brooch pinning the cloak at the left shoulder, a green stone set in it
            builder.Sculpt(new Vector3(-0.07f, 0.8f, 0.06f), new Vector3(0.022f, 0.032f, 0.01f), new[] { (Vector3.forward, 0.6f, 0.2f) }, Palette.Gold,
                           direction => 0.5f + 0.3f * direction.y, 2);
            builder.Ball(new Vector3(-0.07f, 0.8f, 0.07f), new Vector3(0.008f, 0.01f, 0.005f), Palette.Moss, 0.7f);
        }

        // A tall soft riding boot at x, its foot pointing ahead along +Z: a shaft shaped to the calf and creased round the
        // ankle, its top folded down in a cuff, a slender foot with a long toe turning up a little at its point, a thin
        // sole and a low heel, a seam down the front, a strap with a gold buckle round the ankle
        private static void ElfBoot(Builder builder, float x)
        {
            builder.Tube(new[] { new Vector3(x, 0.05f, -0.005f), new Vector3(x, 0.1f, 0f), new Vector3(x, 0.17f, -0.004f), new Vector3(x, 0.235f, 0.004f) },
                         new[] { 0.03f, 0.029f, 0.034f, 0.035f }, Palette.Leather, 0.25f, sides: 22, roundStart: false, roundEnd: false, wrinkle: 0.08f,
                         toneEnd: 0.55f);
            foreach (float y in new[] { 0.075f, 0.098f })
                builder.Torus(new Vector3(x, y, 0.002f), new Vector3(0f, 1f, 0.15f), new Vector2(0.031f, 0.031f), 0.0035f, Palette.Leather, 0.2f, segments: 14,
                              sides: 4, waviness: 0.004f);

            // The cuff folded down over the top, lighter on its turned out side
            builder.Tube(new[] { new Vector3(x, 0.215f, 0.004f), new Vector3(x, 0.25f, 0.006f), new Vector3(x, 0.268f, 0.006f) }, new[] { 0.039f, 0.041f, 0.039f },
                         Palette.Leather, 0.45f, sides: 22, roundStart: false, roundEnd: false, wrinkle: 0.1f, toneEnd: 0.7f);

            // The foot: from the heel out to a long point
            builder.Tube(new[] { new Vector3(x, 0.035f, -0.03f), new Vector3(x, 0.04f, 0.015f), new Vector3(x * 1.03f, 0.03f, 0.065f), new Vector3(x * 1.05f, 0.022f, 0.1f),
                                 new Vector3(x * 1.05f, 0.03f, 0.122f) },
                         new[] { 0.03f, 0.031f, 0.025f, 0.013f, 0.003f }, Palette.Leather, 0.3f, sides: 18, squash: new Vector2(1f, 0.62f), toneEnd: 0.55f);
            builder.Tube(new[] { new Vector3(x, 0.008f, -0.034f), new Vector3(x, 0.008f, 0.02f), new Vector3(x * 1.03f, 0.009f, 0.07f), new Vector3(x * 1.05f, 0.012f, 0.1f) },
                         new[] { 0.03f, 0.032f, 0.026f, 0.012f }, Palette.DarkWood, 0.15f, sides: 18, squash: new Vector2(1f, 0.25f), roundStart: false);
            builder.Sculpt(new Vector3(x, 0.014f, -0.03f), new Vector3(0.024f, 0.014f, 0.02f), new[] { (Vector3.down, 0.6f, 0.2f) }, Palette.DarkWood,
                           direction => 0.15f, 2);

            // A seam down the front of the foot, and the strap with its buckle
            for (int stitch = 0; stitch < 5; stitch++)
            {
                float t = stitch / 4f;
                builder.Ball(new Vector3(x * (1f + 0.04f * t), Mathf.Lerp(0.062f, 0.04f, t), Mathf.Lerp(0.02f, 0.085f, t)), Vector3.one * 0.0022f, Palette.Bone, 0.5f);
            }
            builder.Torus(new Vector3(x, 0.06f, 0.004f), new Vector3(0f, 1f, -0.5f), new Vector2(0.032f, 0.034f), 0.004f, Palette.DarkWood, 0.3f, segments: 14,
                          sides: 4);
            builder.Ball(new Vector3(x + Mathf.Sign(x) * 0.031f, 0.058f, 0.01f), new Vector3(0.004f, 0.007f, 0.008f), Palette.Gold, 0.7f);
        }

        // A hand from the wrist: along runs from the wrist out through the knuckles, back is the back of the hand,
        // side turns it for a left (1) or right (-1) hand as needed so that across, from the little finger to the index
        // finger, comes out the right way. A bone for each finger fans out from the wrist to its knuckle, together making
        // the palm; each finger then bends at its three joints by curl, in degrees, from the little finger to the index;
        // the thumb rises from the side of the palm and bends in across it by thumb
        private static void Hand(Builder builder, Vector3 wrist, Vector3 along, Vector3 back, float side, float[] curl, float thumb)
        {
            along = along.normalized;
            back = Vector3.ProjectOnPlane(back, along).normalized;
            Vector3 across = Vector3.Cross(back, along) * side;
            float[] lengths = { 0.78f, 0.95f, 1f, 0.92f };

            // The palm and the back of the hand in one piece: longer than wide, wider than thick, flatter on the back,
            // a little cupped on the palm side, broadening from the wrist to the knuckles; lighter on the back
            Vector3 palmCenter = wrist + along * 0.024f;
            builder.Sculpt(palmCenter, Vector3.one * 0.016f, new[]
            {
                (along, 0.55f, 0.45f),
                (-along, 0.5f, 0.15f),
                (across, 0.5f, 0.32f),
                (-across, 0.5f, 0.28f),
                (along * 0.6f + across * 0.8f, 0.4f, 0.2f),
                (along * 0.6f - across * 0.8f, 0.4f, 0.18f),
                (back, 0.7f, -0.4f),
                (-back, 0.7f, -0.3f),
            }, Palette.Face, direction => 0.48f + 0.12f * Vector3.Dot(direction, back), 3);

            for (int i = 0; i < 4; i++)
            {
                float spread = (i - 1.5f) * 0.0105f;
                Vector3 root = wrist + along * 0.008f + across * (spread * 0.45f) + back * 0.006f;
                Vector3 knuckle = wrist + along * (0.044f - 0.003f * Mathf.Abs(i - 2f)) + across * spread + back * 0.003f;
                // A tendon running faintly over the back of the hand to each knuckle
                builder.Tube(new[] { root, Vector3.Lerp(root, knuckle, 0.5f) + back * 0.001f, knuckle + back * 0.002f }, new[] { 0.0025f, 0.003f, 0.0035f },
                             Palette.Face, 0.6f, sides: 6);
                builder.Ball(knuckle + back * 0.003f, Vector3.one * 0.0062f, Palette.Face, 0.6f);

                // Three bones, each bending further in towards the palm
                Vector3 direction = along;
                Vector3 up = back;
                Vector3 point = knuckle;
                Vector3[] path = new Vector3[4];
                path[0] = point;
                float[] bones = { 0.022f, 0.015f, 0.012f };
                for (int bone = 0; bone < 3; bone++)
                {
                    float angle = curl[i] * Mathf.Deg2Rad * (bone == 0 ? 0.8f : 1f);
                    Vector3 bent = direction * Mathf.Cos(angle) - up * Mathf.Sin(angle);
                    up = up * Mathf.Cos(angle) + direction * Mathf.Sin(angle);
                    direction = bent;
                    point += direction * (bones[bone] * lengths[i]);
                    path[bone + 1] = point;
                }
                builder.Tube(path, new[] { 0.0079f, 0.0072f, 0.0064f, 0.0052f }, Palette.Face, 0.5f, sides: 14, toneEnd: 0.65f);
                foreach (Vector3 joint in new[] { path[1], path[2] })
                    builder.Ball(joint + up * 0.0018f, Vector3.one * 0.0052f, Palette.Face, 0.6f);
                builder.Ball(path[3] + up * 0.003f - direction * 0.003f, new Vector3(0.0038f, 0.0038f, 0.0038f), Palette.Face, 0.85f);
            }

            // The fleshy pads of the palm: at the base of the thumb and along the little finger's side, and the pads below
            // the fingers
            builder.Ball(wrist + along * 0.016f + across * 0.012f - back * 0.006f, new Vector3(0.011f, 0.011f, 0.011f), Palette.Face, 0.52f);
            builder.Ball(wrist + along * 0.022f - across * 0.012f - back * 0.005f, new Vector3(0.008f, 0.008f, 0.008f), Palette.Face, 0.5f);
            builder.Ball(wrist + along * 0.038f - back * 0.006f, new Vector3(0.009f, 0.009f, 0.009f), Palette.Face, 0.5f);

            // The thumb: from the base of the palm out to the side, then bending in across the palm
            Vector3 thumbDirection = (along * 0.55f + across * 0.75f - back * 0.35f).normalized;
            Vector3 thumbUp = Vector3.ProjectOnPlane(back, thumbDirection).normalized;
            Vector3 thumbPoint = wrist + along * 0.01f + across * 0.016f - back * 0.004f;
            Vector3[] thumbPath = new Vector3[4];
            thumbPath[0] = thumbPoint;
            float[] thumbBones = { 0.02f, 0.016f, 0.013f };
            for (int bone = 0; bone < 3; bone++)
            {
                float angle = thumb * Mathf.Deg2Rad * (bone == 0 ? 0.3f : 0.7f);
                Vector3 inward = (-across * 0.6f - thumbUp * 0.8f).normalized;
                thumbDirection = (thumbDirection * Mathf.Cos(angle) + inward * Mathf.Sin(angle)).normalized;
                thumbPoint += thumbDirection * thumbBones[bone];
                thumbPath[bone + 1] = thumbPoint;
            }
            builder.Tube(thumbPath, new[] { 0.012f, 0.0092f, 0.0078f, 0.0058f }, Palette.Face, 0.5f, sides: 14, toneEnd: 0.65f);
            builder.Ball(thumbPath[3] - thumbDirection * 0.003f, new Vector3(0.0042f, 0.0042f, 0.0042f), Palette.Face, 0.85f);
        }

        // Arm from the shoulder to the wrist: a sleeve creased at the elbow, a leather or silver bracer on the forearm
        private static void ElfArm(Builder builder, Vector3 shoulder, Vector3 elbow, Vector3 wrist, Palette sleeve, bool bracer,
                                   Palette bracerPalette = Palette.Leather)
        {
            // The upper arm full at the shoulder and tapering to the elbow, the forearm full below the elbow and slim at
            // the wrist, where the skin shows below a turned back cuff
            builder.Tube(new[] { shoulder, Vector3.Lerp(shoulder, elbow, 0.35f), Vector3.Lerp(shoulder, elbow, 0.75f), elbow },
                         new[] { 0.034f, 0.031f, 0.027f, 0.024f }, sleeve, 0.55f, sides: 18, wrinkle: 0.07f, toneEnd: 0.35f);
            builder.Ball(elbow, Vector3.one * 0.024f, sleeve, 0.35f);
            Vector3 along = (wrist - elbow).normalized;
            builder.Tube(new[] { elbow, Vector3.Lerp(elbow, wrist, 0.3f), Vector3.Lerp(elbow, wrist, 0.85f) }, new[] { 0.024f, 0.026f, 0.019f }, sleeve, 0.4f,
                         sides: 18, wrinkle: 0.06f, toneEnd: 0.6f);
            builder.Torus(Vector3.Lerp(elbow, wrist, 0.85f), along, new Vector2(0.021f, 0.021f), 0.006f, sleeve, 0.7f, segments: 12, sides: 5, waviness: 0.003f);
            builder.Tube(new[] { Vector3.Lerp(elbow, wrist, 0.82f), wrist }, new[] { 0.016f, 0.0145f }, Palette.Face, 0.4f, sides: 14);
            if (bracer)
            {
                builder.Tube(new[] { Vector3.Lerp(elbow, wrist, 0.35f), wrist - along * 0.005f }, new[] { 0.027f, 0.023f }, bracerPalette, 0.35f, sides: 14,
                             roundStart: false, roundEnd: false, toneEnd: 0.6f);
                builder.Torus(Vector3.Lerp(elbow, wrist, 0.68f), along, new Vector2(0.027f, 0.027f), 0.003f, Palette.Gold, 0.65f, segments: 12, sides: 4);
            }
        }

        // Elf's head: a narrow face with high cheekbones, a fine straight nose and a pointed chin, almond eyes of green,
        // long pointed ears sweeping back, long straight hair falling down his back from a parting, two thin braids at
        // the temples. circlet adds a silver circlet with a gem on the brow
        private static void ElfHead(Builder builder, Palette hair, bool circlet)
        {
            Vector3 head = new(0f, 0.93f, 0.01f);
            Vector3 headRadii = new(0.06f, 0.075f, 0.065f);
            Vector3 eyeSocket = new(0.38f, 0.1f, 0.9f);
            Vector3 cheek = new(0.6f, -0.18f, 0.76f);
            builder.Sculpt(head, headRadii, new[]
            {
                (new Vector3(0f, 0.02f, 1f), 0.08f, 0.08f),
                (new Vector3(0f, -0.1f, 1f), 0.1f, 0.17f),
                (new Vector3(0f, -0.19f, 1f), 0.08f, 0.08f),
                (eyeSocket, 0.14f, -0.06f),
                (Mirror(eyeSocket), 0.14f, -0.06f),
                (cheek, 0.22f, 0.08f),
                (Mirror(cheek), 0.22f, 0.08f),
                (new Vector3(0f, -0.82f, 0.55f), 0.34f, 0.14f),
                (new Vector3(0.5f, -0.72f, 0.45f), 0.3f, 0.1f),
                (new Vector3(-0.5f, -0.72f, 0.45f), 0.3f, 0.1f),
                (new Vector3(0.34f, 0.24f, 0.9f), 0.2f, 0.1f),
                (new Vector3(-0.34f, 0.24f, 0.9f), 0.2f, 0.1f),
                (new Vector3(0.62f, -0.4f, 0.65f), 0.22f, -0.07f),
                (new Vector3(-0.62f, -0.4f, 0.65f), 0.22f, -0.07f),
            }, Palette.Face, direction => 0.55f
                - 0.2f * (Falloff(direction, eyeSocket, 0.15f) + Falloff(direction, Mirror(eyeSocket), 0.15f))
                + 0.12f * (Falloff(direction, cheek, 0.2f) + Falloff(direction, Mirror(cheek), 0.2f))
                + 0.15f * direction.y, 4);

            // Neck
            builder.Tube(new[] { new Vector3(0f, 0.82f, 0f), new Vector3(0f, 0.88f, 0.005f) }, new[] { 0.031f, 0.028f }, Palette.Face, 0.3f, sides: 14,
                         toneEnd: 0.55f);


            // Lips: a fine upper lip and a fuller lower one, a shade deeper than the skin
            Vector3 mouth = head + new Vector3(0f, -0.043f, 0.058f);
            builder.Tube(new[] { mouth + new Vector3(-0.013f, 0.002f, -0.002f), mouth + new Vector3(0f, 0.004f, 0.003f), mouth + new Vector3(0.013f, 0.002f, -0.002f) },
                         new[] { 0.002f, 0.0024f, 0.002f }, Palette.Face, 0.3f, sides: 5);
            builder.Tube(new[] { mouth + new Vector3(-0.01f, -0.004f, -0.002f), mouth + new Vector3(0f, -0.006f, 0.003f), mouth + new Vector3(0.01f, -0.004f, -0.002f) },
                         new[] { 0.0025f, 0.003f, 0.0025f }, Palette.Face, 0.55f, sides: 5);

            foreach (float side in new[] { -1f, 1f })
            {
                // Almond eyes: white, a green iris, a dark pupil, a fine lid and a slanting brow
                Vector3 socket = new(eyeSocket.x * side, eyeSocket.y, eyeSocket.z);
                Vector3 eye = head + Vector3.Scale(socket.normalized * 0.93f, headRadii);
                Vector3 look = (socket.normalized + Vector3.forward * 2f).normalized;
                builder.Ball(eye, new Vector3(0.015f, 0.0075f, 0.01f), Palette.Bone, 0.95f);
                builder.Ball(eye + look * 0.007f, new Vector3(0.007f, 0.007f, 0.004f), Palette.Moss, 0.6f);
                builder.Ball(eye + look * 0.009f, new Vector3(0.0035f, 0.0035f, 0.002f), Palette.DarkWood, 0f);
                builder.Ball(eye + new Vector3(0f, 0.007f, 0.002f), new Vector3(0.018f, 0.005f, 0.01f), Palette.Face, 0.45f);
                builder.Tube(new[] { eye + new Vector3(-0.016f * side, 0.004f, 0.004f), eye + new Vector3(0f, 0.008f, 0.009f),
                                     eye + new Vector3(0.019f * side, 0.006f, 0.002f) },
                             new[] { 0.0018f, 0.002f, 0.0012f }, Palette.DarkWood, 0.1f, sides: 4);
                // The lower lid and a faint shadow under the eye
                builder.Tube(new[] { eye + new Vector3(-0.014f * side, -0.003f, 0.004f), eye + new Vector3(0f, -0.0065f, 0.008f),
                                     eye + new Vector3(0.016f * side, -0.003f, 0.003f) },
                             new[] { 0.0016f, 0.0019f, 0.0014f }, Palette.Face, 0.3f, sides: 4);
                // Heavy brows, low and drawn down towards the nose in a frown
                builder.Tube(new[] { eye + new Vector3(-0.016f * side, 0.008f, 0.009f), eye + new Vector3(0.002f * side, 0.014f, 0.009f),
                                     eye + new Vector3(0.02f * side, 0.016f, 0.002f) },
                             new[] { 0.0055f, 0.006f, 0.003f }, hair, 0.25f, sides: 6, toneEnd: 0.55f);

                // Long pointed ear sweeping up and back
                builder.Tube(new[] { new Vector3(0.056f * side, 0.91f, 0.002f), new Vector3(0.062f * side, 0.93f, -0.006f), new Vector3(0.078f * side, 0.955f, -0.022f),
                                     new Vector3(0.1f * side, 0.99f, -0.045f) },
                             new[] { 0.012f, 0.016f, 0.011f, 0.0015f }, Palette.Face, 0.45f, squash: new Vector2(1f, 0.4f), toneEnd: 0.65f);
                // The fold inside the ear, darker, and the lobe
                builder.Tube(new[] { new Vector3(0.062f * side, 0.925f, 0.002f), new Vector3(0.075f * side, 0.95f, -0.014f), new Vector3(0.09f * side, 0.975f, -0.032f) },
                             new[] { 0.005f, 0.006f, 0.002f }, Palette.Face, 0.15f, sides: 6);
                builder.Ball(new Vector3(0.058f * side, 0.903f, 0.004f), new Vector3(0.006f, 0.008f, 0.005f), Palette.Face, 0.75f);

                // Hair swept back behind the ear in a few strands, falling behind the shoulder; a thin braid at the temple
                // tied off with gold
                for (int strand = 0; strand < 4; strand++)
                {
                    float spread = strand * 0.006f;
                    float wave = (strand % 2 == 0 ? 1f : -1f) * 0.006f;
                    builder.Tube(new[] { new Vector3((0.035f + spread) * side, 1.0f - strand * 0.004f, 0.02f - spread), new Vector3((0.06f + spread) * side, 0.96f, -0.055f),
                                         new Vector3((0.085f + spread) * side + wave, 0.89f, -0.078f - spread), new Vector3((0.1f + spread) * side, 0.8f, -0.085f),
                                         new Vector3((0.095f + spread) * side - wave, 0.71f + strand * 0.01f, -0.09f - spread) },
                                 new[] { 0.011f, 0.013f, 0.012f, 0.009f, 0.003f }, hair, 0.25f + 0.07f * strand, sides: 7, wrinkle: 0.12f, toneEnd: 0.85f);
                }
                Vector3[] braid = { new(0.045f * side, 0.975f, 0.04f), new(0.058f * side, 0.92f, 0.045f), new(0.064f * side, 0.86f, 0.044f),
                                    new(0.066f * side, 0.8f, 0.04f) };
                builder.Tube(braid, new[] { 0.006f, 0.0065f, 0.006f, 0.0045f }, hair, 0.3f, sides: 6, wrinkle: 0.3f, toneEnd: 0.7f);
                for (int twist = 1; twist < 4; twist++)
                    builder.Torus(Vector3.Lerp(braid[0], braid[3], twist / 4f), braid[3] - braid[0], new Vector2(0.006f, 0.006f), 0.0015f, hair, 0.2f, segments: 8,
                                  sides: 4);
                builder.Torus(braid[3], braid[3] - braid[2], new Vector2(0.005f, 0.005f), 0.002f, Palette.Gold, 0.7f, segments: 8, sides: 4);
                builder.Tube(new[] { braid[3], braid[3] + new Vector3(0f, -0.025f, 0.002f) }, new[] { 0.005f, 0.0015f }, hair, 0.55f, sides: 5, toneEnd: 0.9f);

                // Short strands of the fringe swept out from the parting to the side
                for (int strand = 0; strand < 3; strand++)
                {
                    float y = 1.0f - strand * 0.008f;
                    builder.Tube(new[] { new Vector3(0.004f * side, y + 0.006f, 0.05f - strand * 0.006f), new Vector3((0.025f + strand * 0.004f) * side, y, 0.06f - strand * 0.004f),
                                         new Vector3((0.05f + strand * 0.004f) * side, y - 0.02f, 0.046f - strand * 0.005f) },
                                 new[] { 0.007f, 0.007f, 0.002f }, hair, 0.45f + 0.1f * strand, sides: 6, toneEnd: 0.85f);
                }
            }

            // Crown of the head and the long hair down the back, parted in the middle: nine locks waving as they fall,
            // dark where they leave the head and lighter towards their ends, a fine strand laid over each
            builder.Sculpt(head + new Vector3(0f, 0.018f, -0.008f), new Vector3(0.066f, 0.07f, 0.07f), new[]
            {
                (new Vector3(0f, -0.4f, 1f), 0.5f, -0.6f),
                (new Vector3(0f, 1f, 0f), 0.4f, 0.05f),
                (new Vector3(0f, -0.5f, -1f), 0.5f, 0.12f),
            }, hair, direction => 0.35f + 0.45f * direction.y, 3);
            // Strands combed back over the crown from the parting into the long hair
            for (int strand = 0; strand < 10; strand++)
            {
                float across = (strand - 4.5f) / 4.5f;
                float side = Mathf.Sign(across);
                builder.Tube(new[] { new Vector3(side * 0.006f, 1.005f, 0.05f), new Vector3(across * 0.03f, 1.012f, 0.012f), new Vector3(across * 0.05f, 0.995f, -0.03f),
                                     new Vector3(across * 0.052f, 0.96f, -0.065f) },
                             new[] { 0.004f, 0.0065f, 0.0065f, 0.004f }, hair, 0.4f + 0.08f * (strand % 3), sides: 5, toneEnd: 0.7f);
            }

            // The long hair down the back: thirteen locks waving as they fall, each with its own length, dark where they
            // leave the head and lighter towards their ends, two finer strands laid over each
            for (int lock_ = 0; lock_ < 13; lock_++)
            {
                float across = (lock_ - 6) / 6f;
                float wave = (lock_ % 2 == 0 ? 1f : -1f) * (0.005f + 0.002f * (lock_ % 3));
                float end = 0.6f + 0.06f * Mathf.Abs(across) + 0.025f * ((lock_ * 7) % 4) / 3f;
                Vector3[] path =
                {
                    new(across * 0.032f, 0.995f, -0.045f),
                    new(across * 0.05f, 0.94f, -0.085f),
                    new(across * 0.06f + wave, 0.86f, -0.112f),
                    new(across * 0.064f - wave, 0.78f, -0.118f),
                    new(across * 0.058f + wave, (0.78f + end) * 0.5f, -0.115f),
                    new(across * 0.05f - wave * 0.5f, end, -0.108f),
                };
                builder.Tube(path, new[] { 0.012f, 0.013f, 0.013f, 0.011f, 0.008f, 0.0025f }, hair, 0.18f + 0.06f * (lock_ % 3), sides: 7,
                             squash: new Vector2(1f, 0.65f), wrinkle: 0.15f, toneEnd: 0.8f);
                foreach (float s in new[] { -1f, 1f })
                {
                    Vector3 offset = new(s * 0.005f, 0f, -0.006f);
                    builder.Tube(new[] { path[1] + offset, path[2] + offset * 1.3f, path[3] + offset, path[4] + offset * 0.6f, path[5] + new Vector3(0f, 0.025f, -0.004f) },
                                 new[] { 0.0035f, 0.004f, 0.0038f, 0.003f, 0.001f }, hair, 0.5f + 0.12f * ((lock_ + (s > 0f ? 1 : 0)) % 3), sides: 5, toneEnd: 0.95f);
                }
            }

            if (circlet)
            {
                builder.Torus(head + new Vector3(0f, 0.035f, 0f), new Vector3(0f, 1f, -0.25f), new Vector2(0.066f, 0.07f), 0.004f, Palette.Silver, 0.75f,
                              segments: 24, sides: 5);
                builder.Ball(head + new Vector3(0f, 0.05f, 0.07f), new Vector3(0.009f, 0.012f, 0.005f), Palette.Royal, 0.85f);
                builder.Tube(new[] { head + new Vector3(-0.02f, 0.045f, 0.066f), head + new Vector3(0f, 0.062f, 0.068f), head + new Vector3(0.02f, 0.045f, 0.066f) },
                             new[] { 0.003f, 0.003f, 0.003f }, Palette.Silver, 0.8f, sides: 4);
            }
        }


        // The shared sculpting tools, on this tower's palette
        private class Builder : MeshSculptor<Palette>
        {
            public Builder(int seed, float shadeNoise = 0.12f, float noiseScale = 6f) : base(seed, rampWidth, paletteRows, shadeNoise, noiseScale)
            {
            }
        }
    }

}
