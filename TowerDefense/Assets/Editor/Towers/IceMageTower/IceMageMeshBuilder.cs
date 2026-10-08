using UnityEngine;

using static TowerDefense.EditorTools.Towers.MeshSculpting;

using Random = System.Random;


namespace TowerDefense.EditorTools.Towers
{
    // The ice mage tower: a round stone tower gone white with frost, and the ice mage standing on top of it.
    // The tower is built in world units and stands still. The mage turns towards the target with the platform he
    // stands on; he is a mesh of his own, built at his own size and drawn larger, his staff arm and the floating crystal
    // on the staff are meshes of their own so they can move. Parts that give off light, such as ice crystals, glowing
    // windows, runes and the mage's eyes, are built into separate meshes for a glowing material.
    // Built the way the catapult is: smooth tubes, rings and sculpted lumps from the shared sculptor, colours from a
    // gradient palette with muted, earthy tones, shaded by noise
    public static class IceMageMeshBuilder
    {
        public enum Palette { Stone, DarkStone, Ice, Snow, Robe, RobeDark, Trim, Face, Beard, Wood, DarkWood, Iron, Frost, Gold, Leather, Violet }

        // Muted and cold: grey stone, ice from deep to pale clear blue, snow, a deep frost blue robe with silver trim and
        // violet accents.
        // Frost is the colour the glowing parts take, so it may be brighter than the rest
        private static readonly Color32[][] paletteRamps =
        {
            new Color32[] { new(62, 66, 74, 255), new(102, 106, 114, 255), new(142, 146, 152, 255), new(176, 180, 184, 255) },
            new Color32[] { new(40, 42, 50, 255), new(64, 68, 78, 255), new(92, 96, 106, 255), new(120, 124, 132, 255) },
            new Color32[] { new(52, 124, 184, 255), new(88, 164, 216, 255), new(136, 202, 238, 255), new(196, 232, 248, 255) },
            new Color32[] { new(150, 168, 190, 255), new(205, 216, 228, 255), new(234, 240, 246, 255), new(252, 253, 255, 255) },
            new Color32[] { new(28, 44, 78, 255), new(46, 70, 118, 255), new(74, 104, 156, 255), new(108, 138, 184, 255) },
            new Color32[] { new(20, 26, 48, 255), new(34, 44, 76, 255), new(54, 68, 108, 255), new(78, 94, 136, 255) },
            new Color32[] { new(90, 96, 108, 255), new(140, 146, 156, 255), new(186, 190, 198, 255), new(220, 222, 226, 255) },
            new Color32[] { new(120, 96, 96, 255), new(186, 160, 154, 255), new(208, 186, 178, 255), new(198, 150, 146, 255) },
            new Color32[] { new(150, 160, 176, 255), new(196, 204, 214, 255), new(226, 232, 238, 255), new(244, 246, 248, 255) },
            new Color32[] { new(60, 42, 30, 255), new(96, 70, 50, 255), new(132, 102, 76, 255), new(160, 130, 102, 255) },
            new Color32[] { new(40, 28, 22, 255), new(66, 48, 36, 255), new(92, 70, 54, 255), new(116, 92, 72, 255) },
            new Color32[] { new(40, 42, 48, 255), new(70, 74, 82, 255), new(106, 110, 118, 255), new(150, 152, 158, 255) },
            new Color32[] { new(40, 104, 170, 255), new(76, 150, 212, 255), new(124, 196, 238, 255), new(184, 228, 250, 255) },
            new Color32[] { new(100, 74, 32, 255), new(150, 114, 52, 255), new(186, 152, 80, 255), new(210, 184, 120, 255) },
            new Color32[] { new(52, 34, 24, 255), new(86, 56, 38, 255), new(122, 86, 60, 255), new(152, 114, 82, 255) },
            new Color32[] { new(40, 30, 62, 255), new(66, 50, 100, 255), new(98, 78, 140, 255), new(130, 108, 170, 255) },
        };
        private const int rampWidth = 32;
        private const int rowHeight = 4;
        private const int paletteRows = 16;

        // The top of the tower, and the low round dais in its middle the mage stands on, raised so the battlements
        // do not hide him; the platform turning him sits on the dais
        public const float PlatformHeight = 3.1f;
        private const float daisHeight = 0.22f;
        public const float MageStandHeight = PlatformHeight + daisHeight;
        // Height of the ring of ice shards circling the top
        public const float ShardsHeight = 3.75f;

        // The mage is built about 2 tall and drawn this much larger, so he stands about as tall as the attacking units
        public const float MageScale = 1.15f;
        // Where his staff arm turns, in his own space, and where the crystal floats above the staff, in the arm's space
        public static readonly Vector3 StaffShoulder = new(0.19f, 1.03f, 0f);
        public static readonly Vector3 StaffCrystalPosition = new(0.08f, 0.68f, 0.22f);

        // Courses of bricks the tower is laid in, from the plinth up to the cornice, and bricks round each course
        private const int courses = 9;
        private const int bricksPerCourse = 20;
        private const float wallBottom = 0.2f;
        private const float wallTop = 2.93f;
        private const float bottomRadius = 1.42f;
        private const float topRadius = 1.22f;


        public static Texture2D CreatePalette()
        {
            return MeshSculpting.CreatePalette(paletteRamps, rampWidth, paletteRows, rowHeight);
        }

        // The tower: a plinth with steps up to the door, a wall of brick courses with a string course of dark stone
        // round it, arrow slits, windows, a lantern either side of the door, a banner, corbels carrying the cornice and
        // the platform with its battlements.
        // The ice gathers in a few places, so the tower stays readable from afar. At the front left, where the icon
        // camera sees it, ice has run down the wall from the platform in strands that froze as they went, thick at the
        // top and thinning into icicles, and has grown into a spiky outcrop at the foot; the battlements over it are
        // grown over, one wholly, and icicles hang from the cornice and a window sill there. A smaller run and outcrop
        // are on the far side, a lone outcrop on the right.
        // The glowing mesh holds the window and lantern light, the rune circle and the crystals; the ice is a mesh of
        // its own, for a material lighting it faintly from inside, so its shaded side stays icy blue instead of going
        // grey like stone
        public static (Mesh tower, Mesh glow, Mesh ice) BuildTower()
        {
            Builder builder = new(211, shadeNoise: 0.5f, noiseScale: 1.6f);
            Builder glow = new(223, shadeNoise: 0.25f, noiseScale: 3f);
            Builder ice = new(227, shadeNoise: 0.3f, noiseScale: 3f);
            Random rng = new(229);

            builder.Frustum(new Vector3(0f, -0.15f, 0f), 1.72f, 1.62f, wallBottom + 0.15f, 18, Palette.DarkStone, 0.45f, 0.1f, 0f);

            // Two stone steps up to the door
            builder.Box(new Vector3(0f, 0.03f, 1.93f), Quaternion.identity, new Vector3(0.95f, 0.12f, 0.3f), Palette.Stone, 0.45f);
            builder.Box(new Vector3(0f, 0.1f, 1.72f), Quaternion.identity, new Vector3(0.85f, 0.2f, 0.3f), Palette.Stone, 0.5f);

            // Courses of bricks, every other one turned half a brick so the joints do not line up, each set a hair in
            // from the one below, which shows as a mortar line
            float courseHeight = (wallTop - wallBottom) / courses;
            for (int course = 0; course < courses; course++)
            {
                float bottom = wallBottom + course * courseHeight;
                builder.Frustum(new Vector3(0f, bottom, 0f), WallRadius(bottom), WallRadius(bottom + courseHeight) - 0.015f, courseHeight - 0.01f, bricksPerCourse,
                                Palette.Stone, 0.42f + 0.06f * (course % 2), 0.14f, course % 2 == 0 ? 0f : 180f / bricksPerCourse);
            }

            // A string course of dark stone round the wall between the windows and the cornice
            builder.Frustum(new Vector3(0f, stringCourse, 0f), WallRadius(stringCourse) + 0.05f, WallRadius(stringCourse + 0.08f) + 0.05f, 0.08f, 24,
                            Palette.DarkStone, 0.55f, 0.08f, 0f);

            BuildDoor(builder);
            foreach (float angle in windowAngles)
                BuildWindow(builder, glow, angle, windowHeight);
            foreach (float angle in slitAngles)
                BuildArrowSlit(builder, angle, slitHeight);
            foreach (float angle in new[] { -lanternAngle, lanternAngle })
                BuildLantern(builder, glow, angle, lanternHeight);
            BuildBanner(builder, glow, bannerAngle);

            // Corbels under the cornice, the cornice flaring out on them under the platform, the platform slab, paved
            const int corbels = 18;
            for (int i = 0; i < corbels; i++)
            {
                Vector3 outward = Around((i + 0.5f) * 360f / corbels);
                builder.Box(outward * (WallRadius(wallTop) + 0.04f) + Vector3.up * (wallTop - 0.07f), Quaternion.LookRotation(outward),
                            new Vector3(0.1f, 0.14f, 0.12f), Palette.Stone, 0.4f + 0.05f * (i % 2));
            }
            builder.Frustum(new Vector3(0f, wallTop, 0f), WallRadius(wallTop), 1.4f, 0.12f, 18, Palette.Stone, 0.5f, 0.08f, 0f);
            builder.Frustum(new Vector3(0f, wallTop + 0.12f, 0f), 1.4f, 1.4f, PlatformHeight - wallTop - 0.12f, 18, Palette.Stone, 0.55f, 0.06f, 10f);
            builder.Paving(new Vector3(0f, PlatformHeight + 0.004f, 0f), 1.18f, 0.45f, 16, Palette.Stone, 0.62f);

            // Battlements: plain blocks round the edge of the platform, except where the ice ran down, where it has grown
            // over them, wholly over one and over the top and outer side of a few
            const int merlons = 8;
            for (int i = 0; i < merlons; i++)
            {
                float angle = (i + 0.5f) * 360f / merlons;
                Vector3 outward = Around(angle);
                Vector3 along = Around(angle + 90f);
                Vector3 center = outward * 1.27f + Vector3.up * (PlatformHeight + 0.2f);
                Vector3 size = new(0.44f, 0.4f, 0.28f);
                builder.Box(center, Quaternion.LookRotation(outward), size, Palette.Stone, 0.48f + 0.08f * (i % 3) / 2f);

                if (i == encasedMerlon)
                    EncaseInIce(ice, glow, rng, center, outward, along, size, wholly: true);
                else if (System.Array.IndexOf(icedMerlons, i) >= 0)
                    EncaseInIce(ice, glow, rng, center, outward, along, size, wholly: false);
            }

            // The dais in the middle, and the rune circle laid into the floor round it
            builder.Frustum(new Vector3(0f, PlatformHeight - 0.02f, 0f), 0.62f, 0.56f, daisHeight + 0.02f, 14, Palette.DarkStone, 0.5f, 0.1f, 0f, 0.15f);
            glow.Torus(new Vector3(0f, PlatformHeight + 0.012f, 0f), Vector3.up, new Vector2(0.96f, 0.96f), 0.014f, Palette.Frost, 0.65f, segments: 56, sides: 6);
            glow.Torus(new Vector3(0f, PlatformHeight + 0.012f, 0f), Vector3.up, new Vector2(0.78f, 0.78f), 0.01f, Palette.Frost, 0.6f, segments: 48, sides: 6);
            const int runes = 10;
            for (int i = 0; i < runes; i++)
            {
                float angle = i * Mathf.PI * 2 / runes;
                glow.Ball(new Vector3(Mathf.Cos(angle) * 0.87f, PlatformHeight + 0.012f, Mathf.Sin(angle) * 0.87f), new Vector3(0.035f, 0.008f, 0.02f), Palette.Frost, 0.75f);
            }

            // Icicles along the stretch of cornice over each run of ice
            foreach ((float from, float to, int count) in new[] { (mainIce - 30f, mainIce + 30f, 9), (backIce - 18f, backIce + 18f, 4) })
            {
                for (int i = 0; i < count; i++)
                {
                    float angle = Mathf.Lerp(from, to, (i + (float)rng.NextDouble() * 0.6f) / count);
                    Vector3 root = Around(angle) * 1.33f + Vector3.up * (wallTop + 0.02f);
                    Icicle(ice, root, 0.15f + (float)rng.NextDouble() * 0.3f, 0.045f);
                }
            }

            // The main run: strands of ice frozen down the wall from the cornice, the middle ones longest, an outcrop
            // where they reached the ground, icicles off the window sill beside them and a cluster of crystals on the
            // platform above
            IceFlow(ice, mainIce - 11f, 1.15f, 0.13f);
            IceFlow(ice, mainIce, 0.35f, 0.16f);
            IceFlow(ice, mainIce + 10f, 0.9f, 0.12f);
            IceFlow(ice, mainIce + 17f, 1.75f, 0.08f);
            IceLedge(ice, rng, mainIce - 14f, mainIce + 19f);
            IceOutcrop(ice, glow, rng, mainIce - 4f, 0.05f, WallRadius(0.2f) + 0.12f, 0.42f, 5);
            IceOutcrop(ice, glow, rng, mainIce - 22f, 0.02f, WallRadius(0.2f) + 0.2f, 0.26f, 3);
            FrozenSill(ice, rng, windowAngles[0], windowHeight);
            CrystalCluster(glow, rng, mainIce + 3f, PlatformHeight, 1.12f, 3, 0.3f);

            // The run on the far side, shorter
            IceFlow(ice, backIce - 8f, 1.5f, 0.11f);
            IceFlow(ice, backIce + 4f, 0.75f, 0.13f);
            IceLedge(ice, rng, backIce - 11f, backIce + 7f);
            IceOutcrop(ice, glow, rng, backIce, 0.03f, WallRadius(0.2f) + 0.12f, 0.32f, 4);

            // A lone outcrop on the right of the door
            IceOutcrop(ice, glow, rng, rightIce, 0.03f, WallRadius(0.2f) + 0.15f, 0.3f, 3);

            // Big blocks of ice besides: heaped at the foot by both runs and on the right, and one lying on the platform
            IceBoulder(ice, rng, mainIce + 14f, 0.22f, 1.62f, 0.46f);
            IceBoulder(ice, rng, mainIce - 40f, 0.18f, 1.66f, 0.38f);
            IceBoulder(ice, rng, backIce - 27f, 0.22f, 1.62f, 0.42f);
            IceBoulder(ice, rng, 95f, 0.18f, 1.66f, 0.34f);
            IceBoulder(ice, rng, mainIce - 26f, PlatformHeight + 0.12f, 0.98f, 0.22f);

            // Small crystals here and there: by the steps, on the string course, on the platform floor, at the foot
            // of the far side
            CrystalCluster(glow, rng, -28f, 0f, 1.68f, 2, 0.28f);
            CrystalCluster(glow, rng, 32f, stringCourse + 0.08f, WallRadius(stringCourse) + 0.06f, 2, 0.18f);
            CrystalCluster(glow, rng, 200f, PlatformHeight, 1.05f, 2, 0.25f);
            CrystalCluster(glow, rng, 215f, 0f, 1.7f, 3, 0.3f);

            return (builder.ToMesh(), glow.ToMesh(), ice.ToMesh());
        }

        // Where the ice gathers, as angles round the tower (0 is the front, with the door): the main run at the front
        // left, between the door and a window, where the icon camera sees it, a shorter run on the far side and a lone
        // outcrop on the right; and the battlements grown over, wholly over the main run and partly elsewhere
        private const float mainIce = 316f;
        private const float backIce = 150f;
        private const float rightIce = 55f;
        private const int encasedMerlon = 6;
        private static readonly int[] icedMerlons = { 7, 3, 0 };

        // Direction out from the middle of the tower at an angle round it
        private static Vector3 Around(float angle)
        {
            return Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        }

        // A strand of ice that ran down the wall from the cornice and froze as it went: half sunk into the wall, lumpy,
        // thick where it came over the edge, thinning as it falls and ending in an icicle at the height given,
        // shading from bright at the top to deeper blue at the tip
        private static void IceFlow(Builder ice, float angle, float to, float width)
        {
            const int points = 6;
            float from = wallTop - 0.05f;
            Vector3[] spine = new Vector3[points + 2];
            float[] radii = new float[points + 2];

            // It starts on the lip of the cornice and curls over it onto the wall
            spine[0] = Around(angle) * 1.36f + Vector3.up * (wallTop + 0.1f);
            radii[0] = width * 0.9f;
            for (int k = 0; k < points; k++)
            {
                float t = k / (points - 1f);
                float height = Mathf.Lerp(from, to + width, t);
                float thickness = width * Mathf.Lerp(1.05f, 0.45f, t) * (1f + 0.18f * Mathf.Sin(k * 2.3f + angle));
                float wander = angle + Mathf.Sin(k * 1.7f + angle * 0.1f) * 1.2f;
                spine[k + 1] = Around(wander) * (WallRadius(height) + thickness * 0.25f) + Vector3.up * height;
                radii[k + 1] = thickness;
            }
            spine[points + 1] = Around(angle) * (WallRadius(to) + width * 0.15f) + Vector3.up * to;
            radii[points + 1] = 0.003f;

            ice.Tube(spine, radii, Palette.Ice, 0.75f, sides: 8, roundStart: false, roundEnd: false, wrinkle: 0.12f, toneEnd: 0.3f);
        }

        // Ice heaped along the lip of the cornice over a run, from one angle to another, where the strands came over
        // the edge, so they spill out of one frozen mass rather than starting each on its own
        private static void IceLedge(Builder ice, Random rng, float from, float to)
        {
            int lumps = Mathf.Max(2, Mathf.RoundToInt((to - from) / 8f));
            for (int k = 0; k < lumps; k++)
            {
                float angle = Mathf.Lerp(from, to, k / (lumps - 1f));
                ice.Rock(Around(angle) * 1.35f + Vector3.up * (wallTop + 0.1f), 0.15f + (float)rng.NextDouble() * 0.05f, 0.6f, 1, Palette.Ice, 0.65f,
                         centered: true, toneGradient: 0.25f);
            }
        }

        // A big angular block of ice at an angle round the tower, a height and a distance from its middle, tipped
        // against the wall: a few broad faces, lighter towards the top, a smaller chunk broken off at its foot and a
        // short spike or two grown out of its top
        private static void IceBoulder(Builder ice, Random rng, float angle, float height, float distance, float size)
        {
            float Next(float min, float max) => min + (float)rng.NextDouble() * (max - min);
            Vector3 outward = Around(angle);
            Vector3 along = Around(angle + 90f);
            Vector3 point = outward * distance + Vector3.up * height;

            ice.Rock(point, size, Next(0.8f, 1f), 0, Palette.Ice, Next(0.45f, 0.55f), centered: true, toneGradient: 0.3f);
            ice.Rock(point + outward * size * 0.75f + along * (size * Next(-0.4f, 0.4f)) + Vector3.down * size * 0.45f, size * 0.45f, 0.8f, 0,
                     Palette.Ice, 0.4f, centered: true, toneGradient: 0.3f);
            int spikes = size > 0.3f ? 2 : 1;
            for (int s = 0; s < spikes; s++)
            {
                Vector3 lean = (Vector3.up * 1.5f + outward * Next(0.1f, 0.5f) + along * Next(-0.5f, 0.5f)).normalized;
                IceSpike(ice, point + Vector3.up * size * 0.6f + along * (size * (s - 0.5f) * 0.5f), lean, size * Next(0.8f, 1.2f), size * 0.2f,
                         Next(0.5f, 0.65f));
            }
        }

        // A spiky outcrop of ice at an angle round the tower, a height and a distance from its middle: a low faceted
        // mound with chunky pointed columns breaking out of it, the tallest in the middle, all leaning outwards; one of
        // them glows from inside
        private static void IceOutcrop(Builder ice, Builder glow, Random rng, float angle, float height, float distance, float size, int spikes)
        {
            float Next(float min, float max) => min + (float)rng.NextDouble() * (max - min);
            Vector3 outward = Around(angle);
            Vector3 along = Around(angle + 90f);
            Vector3 root = outward * distance + Vector3.up * height;

            ice.Rock(root, size, 0.45f, 1, Palette.Ice, 0.45f, centered: true, toneGradient: 0.25f);
            for (int s = 0; s < spikes; s++)
            {
                float spread = spikes == 1 ? 0f : s / (spikes - 1f) * 2f - 1f;
                float tall = 1f - 0.45f * Mathf.Abs(spread) + Next(-0.1f, 0.1f);
                Vector3 lean = (Vector3.up * 1.5f + outward * Next(0.3f, 0.7f) + along * spread * 0.6f).normalized;
                Vector3 foot = root + along * (spread * size * 0.6f) + outward * (Next(-0.15f, 0.2f) * size);
                float length = size * 2.2f * tall;
                float radius = size * Next(0.22f, 0.3f) * (0.6f + 0.4f * tall);
                if (s == 1)
                    Crystal(glow, foot, lean, length * 0.9f, radius * 0.8f, 0.55f);
                else
                    IceSpike(ice, foot, lean, length, radius, Next(0.45f, 0.65f));
            }
        }

        // A chunky, five sided column of ice ending in a point, shading lighter towards the point
        private static void IceSpike(Builder ice, Vector3 foot, Vector3 direction, float length, float radius, float tone)
        {
            direction = direction.normalized;
            ice.Tube(new[] { foot - direction * radius * 0.5f, foot + direction * length * 0.55f, foot + direction * length * 0.8f, foot + direction * length },
                     new[] { radius, radius * 0.9f, radius * 0.55f, 0.003f }, Palette.Ice, tone - 0.2f, sides: 5, roundStart: false, roundEnd: false,
                     toneEnd: tone + 0.25f);
        }

        // An icicle hanging from the point given, bright at the root and deep blue at the tip
        private static void Icicle(Builder ice, Vector3 root, float length, float radius)
        {
            ice.Tube(new[] { root, root + Vector3.down * length * 0.5f, root + Vector3.down * length },
                     new[] { radius, radius * 0.55f, 0.002f }, Palette.Ice, 0.8f, sides: 6, roundStart: false, roundEnd: false, toneEnd: 0.3f);
        }

        // A window sill frozen over: a crust of ice on it and icicles hanging off its front edge
        private static void FrozenSill(Builder ice, Random rng, float angle, float height)
        {
            Vector3 outward = Around(angle);
            Vector3 along = Around(angle + 90f);
            Vector3 sill = outward * (WallRadius(height) + 0.03f) + Vector3.up * (height - 0.2f);
            ice.Rock(sill + along * 0.06f, 0.11f, 0.4f, 1, Palette.Ice, 0.6f, centered: true, toneGradient: 0.2f);
            for (int k = -2; k <= 2; k++)
                Icicle(ice, sill + outward * 0.09f + along * (k * 0.065f) + Vector3.down * 0.02f, 0.08f + (float)rng.NextDouble() * 0.18f, 0.028f);
        }

        // Crystals growing out of one spot at an angle round the tower, fanning upwards and outwards, the first longest
        private static void CrystalCluster(Builder glow, Random rng, float angle, float height, float distance, int count, float length)
        {
            Vector3 outward = Around(angle);
            Vector3 along = Around(angle + 90f);
            Vector3 root = outward * distance + Vector3.up * height;
            for (int c = 0; c < count; c++)
            {
                float spread = c - (count - 1) * 0.5f;
                Vector3 lean = (Vector3.up * 1.4f + outward * (0.3f + 0.2f * c) + along * (spread * 0.35f)).normalized;
                float crystalLength = length * (1f - 0.12f * c) + (float)rng.NextDouble() * 0.1f;
                Crystal(glow, root + along * (spread * 0.1f), lean, crystalLength, Mathf.Max(0.04f, 0.09f - 0.01f * c), 0.6f);
            }
        }

        // The mage in his own space, standing on the origin, facing +Z: a long frost blue robe flaring at the hem, open
        // down the front over a lighter under-robe and edged in silver, silver stitching round the hem, a cape, a fur
        // mantle, a sash with a snowflake clasp, a spellbook and two vials of frost at his hips, his left hand raised in
        // a sleeve closed with fur at the wrist, a frost orb over the palm, a gaunt, lined, sculpted face with glowing eyes and pointed
        // ears, long white brows, moustache, hair and a beard to the belt, and a tall wide brimmed hat with a crooked tip.
        // His right arm with the staff is built on its own
        public static (Mesh mage, Mesh glow) BuildMage()
        {
            Builder builder = new(241, shadeNoise: 0.55f, noiseScale: 5f);
            Builder glow = new(251, shadeNoise: 0.2f, noiseScale: 9f);

            // Robe flaring to the ground, its hem and front edged in silver
            builder.Tube(new[] { new Vector3(0f, 0.04f, 0f), new Vector3(0f, 0.25f, 0.005f), new Vector3(0f, 0.6f, 0.01f), new Vector3(0f, 0.95f, 0f),
                                 new Vector3(0f, 1.12f, 0f) },
                         new[] { 0.36f, 0.3f, 0.22f, 0.2f, 0.13f }, Palette.Robe, 0.45f, sides: 24, squash: new Vector2(1f, 0.85f), roundStart: false, wrinkle: 0.06f);
            builder.Torus(new Vector3(0f, 0.05f, 0f), Vector3.up, new Vector2(0.36f, 0.31f), 0.018f, Palette.Trim, 0.55f, segments: 40, waviness: 0.012f);

            // The robe opens down the front over a lighter under-robe, the edges of the opening in silver
            builder.Tube(new[] { new Vector3(0f, 0.06f, 0.302f), new Vector3(0f, 0.35f, 0.248f), new Vector3(0f, 0.7f, 0.188f), new Vector3(0f, 0.95f, 0.17f) },
                         new[] { 0.03f, 0.028f, 0.025f, 0.022f }, Palette.Ice, 0.58f, sides: 10, squash: new Vector2(3.6f, 0.3f), roundStart: false, roundEnd: false,
                         wrinkle: 0.05f);
            foreach (float side in new[] { -1f, 1f })
            {
                builder.Tube(new[] { new Vector3(side * 0.11f, 0.06f, 0.29f), new Vector3(side * 0.1f, 0.35f, 0.24f), new Vector3(side * 0.09f, 0.7f, 0.18f),
                                     new Vector3(side * 0.08f, 0.95f, 0.164f) },
                             new[] { 0.014f, 0.014f, 0.013f, 0.012f }, Palette.Trim, 0.55f, sides: 6);
            }

            // Embroidery round the hem: a zigzag of silver stitches
            const int stitches = 44;
            for (int i = 0; i < stitches; i++)
            {
                float angle = i * Mathf.PI * 2 / stitches;
                float height = 0.1f + (i % 2) * 0.03f;
                float radius = Mathf.Lerp(0.36f, 0.3f, (height - 0.04f) / 0.21f) + 0.012f;
                builder.Ball(new Vector3(Mathf.Cos(angle) * radius, height, 0.005f + Mathf.Sin(angle) * radius * 0.85f), Vector3.one * 0.011f, Palette.Trim, 0.7f);
            }

            // Cape falling from the shoulders down the back
            builder.Tube(new[] { new Vector3(0f, 1.05f, -0.1f), new Vector3(0f, 0.7f, -0.19f), new Vector3(0f, 0.3f, -0.25f), new Vector3(0f, 0.04f, -0.28f) },
                         new[] { 0.2f, 0.27f, 0.33f, 0.37f }, Palette.RobeDark, 0.45f, sides: 20, squash: new Vector2(1f, 0.28f), roundStart: false, roundEnd: false,
                         wrinkle: 0.08f);

            // Fur mantle round the shoulders
            builder.Torus(new Vector3(0f, 1.07f, 0f), Vector3.up, new Vector2(0.18f, 0.155f), 0.065f, Palette.Snow, 0.62f, segments: 28, sides: 10, waviness: 0.015f);
            builder.Torus(new Vector3(0f, 1.115f, 0f), Vector3.up, new Vector2(0.13f, 0.11f), 0.05f, Palette.Snow, 0.72f, segments: 24, sides: 8, waviness: 0.01f);

            // Sash round the waist with its ends hanging at the front, held by a glowing snowflake clasp
            builder.Torus(new Vector3(0f, 0.62f, 0.01f), Vector3.up, new Vector2(0.228f, 0.195f), 0.03f, Palette.Violet, 0.5f, segments: 32, waviness: 0.008f);
            builder.Tube(new[] { new Vector3(0.05f, 0.6f, 0.2f), new Vector3(0.07f, 0.45f, 0.225f), new Vector3(0.085f, 0.3f, 0.245f) },
                         new[] { 0.028f, 0.026f, 0.02f }, Palette.Violet, 0.45f, sides: 10, squash: new Vector2(1.4f, 0.45f), wrinkle: 0.05f);
            builder.Tube(new[] { new Vector3(-0.02f, 0.6f, 0.205f), new Vector3(-0.01f, 0.47f, 0.225f), new Vector3(0f, 0.36f, 0.24f) },
                         new[] { 0.026f, 0.024f, 0.018f }, Palette.Violet, 0.55f, sides: 10, squash: new Vector2(1.4f, 0.45f), wrinkle: 0.05f);
            Snowflake(glow, new Vector3(0f, 0.625f, 0.215f), 0.035f);

            // A spellbook hanging from the sash at his left hip, its pages showing at the edge, a gold clasp
            Vector3 book = new(-0.235f, 0.5f, 0.06f);
            builder.Tube(new[] { new Vector3(-0.215f, 0.625f, 0.07f), book + new Vector3(0.005f, 0.055f, 0f) }, new[] { 0.006f, 0.006f }, Palette.Leather, 0.3f, sides: 5);
            builder.Sculpt(book, new Vector3(0.022f, 0.06f, 0.048f), new[] { (Vector3.left, 0.7f, 0.1f) }, Palette.Violet, direction => 0.4f, 2);
            builder.Sculpt(book + new Vector3(0.003f, 0f, 0.006f), new Vector3(0.017f, 0.055f, 0.046f), new[] { (Vector3.forward, 0.7f, 0.05f) },
                           Palette.Snow, direction => 0.55f, 2);
            builder.Ball(book + new Vector3(-0.022f, 0f, 0.02f), new Vector3(0.008f, 0.014f, 0.008f), Palette.Gold, 0.7f);

            // Two vials of glowing frost hanging at his right hip
            for (int vial = 0; vial < 2; vial++)
            {
                Vector3 bottle = new(0.235f, 0.5f - vial * 0.025f, 0.05f + vial * 0.05f);
                builder.Tube(new[] { new Vector3(0.22f, 0.625f, 0.05f + vial * 0.04f), bottle + Vector3.up * 0.04f }, new[] { 0.004f, 0.004f }, Palette.Leather, 0.3f, sides: 5);
                glow.Ball(bottle, new Vector3(0.02f, 0.032f, 0.02f), Palette.Frost, 0.45f);
                builder.Ball(bottle + Vector3.up * 0.035f, new Vector3(0.011f, 0.01f, 0.011f), Palette.Wood, 0.5f);
            }

            // Left arm raised in front of him, the open palm forward with a frost orb hanging over it
            Vector3 palm = new(-0.285f, 0.86f, 0.36f);
            // The sleeve narrows to the wrist, where a band of fur closes it snugly
            builder.Tube(new[] { new Vector3(-0.19f, 1.03f, 0f), new Vector3(-0.27f, 0.9f, 0.07f), new Vector3(-0.31f, 0.82f, 0.2f), new Vector3(-0.29f, 0.83f, 0.29f) },
                         new[] { 0.065f, 0.074f, 0.08f, 0.06f }, Palette.Robe, 0.45f, sides: 16, wrinkle: 0.06f);
            builder.Torus(new Vector3(-0.29f, 0.832f, 0.285f), new Vector3(0.15f, 0.08f, 1f), new Vector2(0.058f, 0.058f), 0.02f, Palette.Snow, 0.58f,
                          sides: 8, waviness: 0.006f);
            builder.Tube(new[] { new Vector3(-0.29f, 0.835f, 0.28f), palm }, new[] { 0.03f, 0.028f }, Palette.Face, 0.5f, sides: 10);
            OpenHand(builder, palm);
            glow.Ball(palm + new Vector3(0f, 0.04f, 0.08f), Vector3.one * 0.05f, Palette.Frost, 0.85f);
            glow.Torus(palm + new Vector3(0f, 0.04f, 0.08f), new Vector3(0.3f, 1f, 0.2f), new Vector2(0.075f, 0.075f), 0.006f, Palette.Frost, 0.7f, segments: 24, sides: 5);

            BuildHead(builder, glow);
            BuildHat(builder, glow);

            return (builder.ToMesh(), glow.ToMesh());
        }

        // The mage's right arm in its own space, turning round the shoulder at the origin: a wide sleeve and a hand
        // wrapped round a gnarled wooden staff standing on the ground at his side, its top grown into three prongs
        // that hold the crystal
        public static Mesh BuildStaffArm()
        {
            Builder builder = new(263, shadeNoise: 0.55f, noiseScale: 5f);

            // The sleeve narrows to the wrist, where a band of fur closes it snugly
            builder.Tube(new[] { Vector3.zero, new Vector3(0.07f, -0.14f, 0.04f), new Vector3(0.09f, -0.27f, 0.1f), new Vector3(0.08f, -0.31f, 0.155f) },
                         new[] { 0.065f, 0.074f, 0.08f, 0.058f }, Palette.Robe, 0.45f, sides: 16, wrinkle: 0.06f);
            builder.Torus(new Vector3(0.081f, -0.311f, 0.152f), new Vector3(0f, -0.3f, 1f), new Vector2(0.056f, 0.056f), 0.02f, Palette.Snow, 0.58f,
                          sides: 8, waviness: 0.006f);

            Vector3 grip = new(0.08f, -0.32f, 0.22f);
            builder.Tube(new[] { new Vector3(0.085f, -0.315f, 0.15f), grip + new Vector3(0.035f, 0f, -0.012f) }, new[] { 0.028f, 0.026f }, Palette.Face, 0.5f, sides: 10);
            GripHand(builder, grip);

            // Gnarled staff from the ground up past his shoulder, a knot at the top, a silver band under the prongs
            Vector3 top = new(0.08f, 0.5f, 0.22f);
            builder.Tube(new[] { new Vector3(0.08f, -1.02f, 0.22f), new Vector3(0.09f, -0.6f, 0.215f), new Vector3(0.075f, -0.2f, 0.225f), new Vector3(0.085f, 0.2f, 0.218f), top },
                         new[] { 0.025f, 0.028f, 0.03f, 0.03f, 0.034f }, Palette.Wood, 0.45f, sides: 10, wrinkle: 0.15f);
            builder.Ball(top, new Vector3(0.048f, 0.042f, 0.048f), Palette.Wood, 0.4f);
            builder.Torus(top + Vector3.down * 0.05f, Vector3.up, new Vector2(0.037f, 0.037f), 0.009f, Palette.Trim, 0.6f);

            // Two leather cords tied under the prongs, one with a little ice charm at its end
            foreach ((float x, float length, bool charm) in new[] { (0.03f, 0.2f, true), (-0.02f, 0.15f, false) })
            {
                Vector3 knot = top + new Vector3(x, -0.03f, 0.01f);
                Vector3 end = knot + new Vector3(x * 0.6f, -length, 0.015f);
                builder.Tube(new[] { knot, Vector3.Lerp(knot, end, 0.5f) + new Vector3(x * 0.4f, 0f, 0.01f), end }, new[] { 0.005f, 0.005f, 0.004f },
                             Palette.Leather, 0.35f, sides: 5);
                if (charm)
                    Crystal(builder, end, Vector3.down, 0.07f, 0.016f, 0.6f, pointedBothEnds: true);
                else
                    builder.Ball(end, Vector3.one * 0.012f, Palette.Trim, 0.6f);
            }

            Vector3 prongBase = top + Vector3.up * 0.02f;
            for (int prong = 0; prong < 3; prong++)
            {
                float angle = (prong * 120f + 30f) * Mathf.Deg2Rad;
                Vector3 outward = new(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                builder.Tube(new[] { prongBase + outward * 0.03f, prongBase + outward * 0.075f + Vector3.up * 0.08f, prongBase + outward * 0.07f + Vector3.up * 0.17f,
                                     prongBase + outward * 0.025f + Vector3.up * 0.23f },
                             new[] { 0.02f, 0.016f, 0.012f, 0.004f }, Palette.Wood, 0.5f, sides: 8);
            }

            return builder.ToMesh();
        }

        // The crystal floating between the prongs of the staff: a long six sided crystal pointed at both ends
        public static Mesh BuildStaffCrystal()
        {
            Builder glow = new(271, shadeNoise: 0.2f, noiseScale: 12f);
            Crystal(glow, new Vector3(0f, -0.1f, 0f), Vector3.up, 0.23f, 0.055f, 0.75f, pointedBothEnds: true);

            return glow.ToMesh();
        }

        // Ice shards circling the top of the tower, around its own centre
        public static Mesh BuildShards()
        {
            Builder glow = new(281, shadeNoise: 0.25f, noiseScale: 5f);
            const int shards = 6;
            for (int i = 0; i < shards; i++)
            {
                float angle = i * 360f / shards;
                Vector3 center = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * 1.55f + Vector3.up * (Mathf.Sin(i * 2.1f) * 0.15f);
                Vector3 axis = Quaternion.Euler(20f * (i % 2 * 2 - 1), angle, 25f) * Vector3.up;
                Crystal(glow, center - axis * 0.18f, axis, 0.38f, 0.06f, 0.6f, pointedBothEnds: true);
            }

            return glow.ToMesh();
        }

        // A splinter of ice flung off where the frost beam hits. Unit sized, the particle system scales it
        public static Mesh BuildIceShard()
        {
            Builder glow = new(293, shadeNoise: 0.2f, noiseScale: 6f);
            Crystal(glow, new Vector3(0f, -0.25f, 0f), Vector3.up, 0.5f, 0.12f, 0.7f, pointedBothEnds: true);

            return glow.ToMesh();
        }


        // Ice grown over a battlement: a thick cap over its top, pointed columns breaking out of the cap and icicles
        // hanging off its outer edge; or wholly over it, with ice heaped against its sides and front as well, so that
        // little of the stone shows, and a glowing crystal among the columns
        private static void EncaseInIce(Builder ice, Builder glow, Random rng, Vector3 center, Vector3 outward, Vector3 along, Vector3 size, bool wholly)
        {
            float Next(float min, float max) => min + (float)rng.NextDouble() * (max - min);
            Vector3 half = size * 0.5f;
            Vector3 top = center + Vector3.up * half.y;

            ice.Rock(top + Vector3.up * 0.03f + along * Next(-0.04f, 0.04f), 0.27f, 0.4f, 1, Palette.Ice, 0.62f, centered: true, toneGradient: 0.25f);
            for (int k = -1; k <= 1; k++)
                Icicle(ice, top + outward * (half.z + 0.03f) + along * (k * 0.13f + Next(-0.03f, 0.03f)), 0.1f + (float)rng.NextDouble() * 0.16f, 0.035f);

            int spikes = wholly ? 4 : 2;
            for (int s = 0; s < spikes; s++)
            {
                float spread = s / (spikes - 1f) * 2f - 1f;
                Vector3 lean = (Vector3.up * 1.6f + outward * Next(0f, 0.6f) + along * spread * 0.5f).normalized;
                IceSpike(ice, top + along * (spread * 0.13f) + outward * Next(-0.06f, 0.06f), lean, Next(0.22f, 0.38f), Next(0.05f, 0.07f), Next(0.5f, 0.65f));
            }

            if (!wholly)
                return;

            ice.Rock(center + along * (half.x + 0.03f) + Vector3.up * Next(-0.05f, 0.1f), 0.2f, 0.95f, 1, Palette.Ice, 0.5f, centered: true, toneGradient: 0.3f);
            ice.Rock(center - along * (half.x + 0.03f) + Vector3.up * Next(-0.1f, 0.05f), 0.19f, 0.95f, 1, Palette.Ice, 0.48f, centered: true, toneGradient: 0.3f);
            ice.Rock(center + outward * (half.z + 0.05f) + Vector3.down * 0.05f, 0.2f, 0.9f, 1, Palette.Ice, 0.52f, centered: true, toneGradient: 0.3f);
            ice.Rock(center - outward * (half.z + 0.04f) + Vector3.up * 0.05f, 0.2f, 0.9f, 1, Palette.Ice, 0.55f, centered: true, toneGradient: 0.3f);
            Crystal(glow, top + Vector3.up * 0.04f, (Vector3.up * 2f + outward).normalized, 0.42f, 0.07f, 0.6f);
        }

        // The door faces front; the windows sit at one height round the tower, the arrow slits lower, the lanterns
        // either side of the door, the banner hangs from the cornice on the left, the string course runs above the windows
        private const float windowHeight = 1.95f;
        private static readonly float[] windowAngles = { -70f, 70f, 180f };
        private const float slitHeight = 1.05f;
        private static readonly float[] slitAngles = { 110f, 225f };
        private const float lanternAngle = 24f;
        private const float lanternHeight = 1.35f;
        private const float bannerAngle = 262f;
        private const float stringCourse = 2.5f;

        private static float WallRadius(float height)
        {
            return Mathf.Lerp(bottomRadius, topRadius, Mathf.InverseLerp(wallBottom, wallTop, height));
        }

        // A narrow dark arrow slit in the wall, framed in stone, with a sill under it
        private static void BuildArrowSlit(Builder builder, float angle, float height)
        {
            Quaternion facing = Quaternion.Euler(0f, angle, 0f);
            Vector3 outward = facing * Vector3.forward;
            Vector3 side = facing * Vector3.right;
            Vector3 center = outward * WallRadius(height) + Vector3.up * height;

            builder.Box(center, facing, new Vector3(0.05f, 0.34f, 0.04f), Palette.Iron, 0.02f);
            foreach (float s in new[] { -1f, 1f })
                builder.Box(center + side * (0.065f * s) + outward * 0.01f, facing, new Vector3(0.07f, 0.4f, 0.06f), Palette.Stone, 0.55f);
            builder.Box(center + Vector3.up * 0.215f + outward * 0.015f, facing, new Vector3(0.22f, 0.06f, 0.08f), Palette.Stone, 0.6f);
            builder.Box(center + Vector3.down * 0.215f + outward * 0.025f, facing, new Vector3(0.24f, 0.05f, 0.1f), Palette.Stone, 0.6f);
        }

        // An iron lantern on a bracket out of the wall, a glowing frost crystal in its cage
        private static void BuildLantern(Builder builder, Builder glow, float angle, float height)
        {
            Vector3 outward = Around(angle);
            Vector3 along = Around(angle + 90f);
            Vector3 wall = outward * WallRadius(height + 0.15f) + Vector3.up * (height + 0.15f);
            Vector3 light = outward * (WallRadius(height) + 0.17f) + Vector3.up * height;

            builder.Box(wall, Quaternion.LookRotation(outward), new Vector3(0.08f, 0.12f, 0.03f), Palette.Iron, 0.4f);
            builder.Beam(wall, light + Vector3.up * 0.15f, 0.025f, 0.025f, Palette.Iron, 0.45f);
            builder.Beam(light + Vector3.up * 0.15f, light + Vector3.up * 0.09f, 0.012f, 0.012f, Palette.Iron, 0.45f);

            builder.Frustum(light + Vector3.up * 0.06f, 0.075f, 0.015f, 0.05f, 4, Palette.Iron, 0.4f, 0.05f, 45f);
            builder.Frustum(light + Vector3.down * 0.08f, 0.04f, 0.065f, 0.025f, 4, Palette.Iron, 0.35f, 0.05f, 45f);
            foreach (Vector3 corner in new[] { outward + along, outward - along, -outward + along, -outward - along })
                builder.Beam(light + corner * 0.042f + Vector3.down * 0.06f, light + corner * 0.05f + Vector3.up * 0.06f, 0.008f, 0.008f, Palette.Iron, 0.4f);
            glow.Ball(light, new Vector3(0.045f, 0.06f, 0.045f), Palette.Frost, 0.8f);
        }

        // A banner of the frost order hanging from an iron rod under the cornice: deep blue cloth edged in silver,
        // ending in a point, with a glowing crystal sewn on
        private static void BuildBanner(Builder builder, Builder glow, float angle)
        {
            Vector3 outward = Around(angle);
            Vector3 along = Around(angle + 90f);
            Quaternion facing = Quaternion.LookRotation(outward);
            const float top = wallTop - 0.17f;
            const float length = 0.72f;
            const float width = 0.36f;
            Vector3 center = outward * (WallRadius(top - length * 0.5f) + 0.06f) + Vector3.up * (top - length * 0.5f);
            Vector3 bottom = center + Vector3.down * length * 0.5f;
            Vector3 rod = outward * (WallRadius(top) + 0.07f) + Vector3.up * top;

            builder.Cylinder(rod, along, 0.018f, width + 0.12f, 6, Palette.Iron, 0.45f, true);
            foreach (float s in new[] { -1f, 1f })
                builder.Ball(rod + along * (s * (width * 0.5f + 0.07f)), Vector3.one * 0.028f, Palette.Iron, 0.5f);

            builder.Box(center, facing, new Vector3(width, length, 0.02f), Palette.Robe, 0.5f);
            builder.Flag(bottom - along * width * 0.5f, bottom + along * width * 0.5f, bottom + Vector3.down * 0.24f, Palette.Robe, 0.45f);
            foreach (float s in new[] { -1f, 1f })
                builder.Box(center + along * (s * (width * 0.5f - 0.015f)) + outward * 0.012f, facing, new Vector3(0.025f, length, 0.01f), Palette.Trim, 0.6f);
            builder.Box(center + Vector3.up * (length * 0.5f - 0.03f) + outward * 0.012f, facing, new Vector3(width, 0.03f, 0.01f), Palette.Trim, 0.6f);

            Crystal(glow, center + outward * 0.02f + Vector3.down * 0.13f, Vector3.up, 0.26f, 0.045f, 0.7f, pointedBothEnds: true);
        }

        // Dark plank door in the front of the tower, rounded at the top, with iron bands and a ring, framed in stone
        private static void BuildDoor(Builder builder)
        {
            float wall = WallRadius(0.7f);
            builder.Box(new Vector3(0f, 0.7f, wall - 0.02f), Quaternion.identity, new Vector3(0.62f, 1f, 0.1f), Palette.DarkWood, 0.4f);
            builder.Ball(new Vector3(0f, 1.2f, wall - 0.03f), new Vector3(0.31f, 0.31f, 0.05f), Palette.DarkWood, 0.38f);
            foreach (float y in new[] { 0.45f, 0.95f })
                builder.Box(new Vector3(0f, y, wall + 0.035f), Quaternion.identity, new Vector3(0.64f, 0.05f, 0.02f), Palette.Iron, 0.45f);
            builder.Torus(new Vector3(0.18f, 0.72f, wall + 0.05f), Vector3.forward, new Vector2(0.04f, 0.04f), 0.01f, Palette.Iron, 0.5f, segments: 14, sides: 5);

            foreach (float x in new[] { -0.37f, 0.37f })
                builder.Box(new Vector3(x, 0.7f, wall), Quaternion.identity, new Vector3(0.13f, 1f, 0.16f), Palette.Stone, 0.55f);
            builder.Torus(new Vector3(0f, 1.2f, wall + 0.01f), Vector3.forward, new Vector2(0.37f, 0.37f), 0.075f, Palette.Stone, 0.55f,
                          segments: 16, sides: 6, arcStart: 180f, arcDegrees: 180f);
        }

        // Narrow arched window glowing with cold light, its stone frame and sill, facing out at the angle given
        private static void BuildWindow(Builder builder, Builder glow, float angle, float height)
        {
            Quaternion facing = Quaternion.Euler(0f, angle, 0f);
            Vector3 outward = facing * Vector3.forward;
            Vector3 center = outward * (WallRadius(height) - 0.02f) + Vector3.up * height;

            glow.Box(center, facing, new Vector3(0.2f, 0.4f, 0.06f), Palette.Frost, 0.7f);
            glow.Ball(center + Vector3.up * 0.2f, new Vector3(0.1f, 0.1f, 0.03f), Palette.Frost, 0.7f);

            Vector3 side = facing * Vector3.right;
            foreach (float s in new[] { -1f, 1f })
                builder.Box(center + side * (0.14f * s) + outward * 0.02f, facing, new Vector3(0.08f, 0.44f, 0.1f), Palette.Stone, 0.55f);
            // The ring measures its angles from an axis across it that depends on which way it faces: for a ring facing
            // nearly forward or back that axis lies sideways, otherwise it points down. Either way this keeps the top half
            float archStart = Mathf.Abs(Vector3.Dot(outward, Vector3.forward)) > 0.9f ? 180f : 90f;
            builder.Torus(center + Vector3.up * 0.2f + outward * 0.03f, outward, new Vector2(0.14f, 0.14f), 0.04f, Palette.Stone, 0.55f,
                          segments: 12, sides: 6, arcStart: archStart, arcDegrees: 180f);
            builder.Box(center + Vector3.down * 0.23f + outward * 0.05f, facing, new Vector3(0.32f, 0.05f, 0.14f), Palette.Stone, 0.6f);
            builder.Sculpt(center + Vector3.down * 0.19f + outward * 0.06f, new Vector3(0.15f, 0.03f, 0.06f), new[] { (Vector3.up, 0.6f, 0.3f) },
                           Palette.Ice, direction => 0.6f, 2);
        }

        // Sculpted gaunt old face: a long thin nose, deep set eyes under heavy brows, high cheekbones over hollow
        // cheeks. Glowing icy eyes, long white brows of a few strands, a drooping moustache, hair falling from under
        // the hat and a long beard of locks reaching the sash
        private static void BuildHead(Builder builder, Builder glow)
        {
            Vector3 head = new(0f, 1.25f, 0.02f);
            Vector3 headRadii = new(0.115f, 0.13f, 0.115f);
            Vector3 noseTip = new(0f, -0.22f, 1f);
            Vector3 eyeSocket = new(0.36f, 0.13f, 0.92f);
            Vector3 cheekbone = new(0.55f, 0.02f, 0.82f);
            Vector3 hollow = new(0.6f, -0.3f, 0.75f);
            Vector3 browRidge = new(0.36f, 0.3f, 0.9f);
            var features = new System.Collections.Generic.List<(Vector3 direction, float width, float height)>
            {
                (new Vector3(0f, -0.05f, 1f), 0.17f, 0.55f),
                (noseTip, 0.1f, 0.12f),
                (new Vector3(0f, 0.12f, 1f), 0.08f, 0.08f),
                (new Vector3(0f, 0.38f, 0.93f), 0.12f, -0.05f),
                (new Vector3(0f, -0.7f, 0.7f), 0.3f, 0.08f),
            };
            var creases = new System.Collections.Generic.List<Vector3>();
            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 Side(Vector3 direction) => new(direction.x * side, direction.y, direction.z);

                features.Add((Side(browRidge), 0.22f, 0.1f));
                features.Add((Side(eyeSocket), 0.12f, -0.1f));
                features.Add((Side(cheekbone), 0.2f, 0.1f));
                features.Add((Side(hollow), 0.2f, -0.06f));
                // Bags under the eyes, hollow temples, wings of the nose
                features.Add((Side(new Vector3(0.33f, 0.01f, 0.94f)), 0.07f, 0.035f));
                features.Add((Side(new Vector3(0.85f, 0.3f, 0.4f)), 0.2f, -0.05f));
                features.Add((Side(new Vector3(0.14f, -0.24f, 1f)), 0.07f, 0.09f));

                // Folds from the wings of the nose down past the mouth, and crow's feet at the corners of the eyes
                for (int i = 0; i < 4; i++)
                    features.Add((Side(Vector3.Lerp(new Vector3(0.17f, -0.14f, 0.97f), new Vector3(0.3f, -0.45f, 0.84f), i / 3f)), 0.055f, 0.035f));
                for (int line = 0; line < 3; line++)
                {
                    Vector3 corner = Side(new Vector3(0.56f, 0.08f + line * 0.06f, 0.8f));
                    features.Add((corner, 0.035f, -0.018f));
                    creases.Add(corner);
                }
            }

            // Three lines across the forehead, each a row of small dents
            for (int line = 0; line < 3; line++)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector3 dent = new(Mathf.Lerp(-0.34f, 0.34f, i / 5f), 0.47f + line * 0.075f - Mathf.Abs(i - 2.5f) * 0.012f, 0.88f - line * 0.05f);
                    features.Add((dent, 0.05f, -0.016f));
                    creases.Add(dent);
                }
            }

            builder.Sculpt(head, headRadii, features.ToArray(), Palette.Face, direction =>
            {
                float tone = 0.5f
                    - 0.2f * (Falloff(direction, eyeSocket, 0.14f) + Falloff(direction, Mirror(eyeSocket), 0.14f))
                    + 0.35f * (Falloff(direction, cheekbone, 0.22f) + Falloff(direction, Mirror(cheekbone), 0.22f))
                    + 0.4f * Falloff(direction, noseTip, 0.14f);
                foreach (Vector3 crease in creases)
                    tone -= 0.12f * Falloff(direction, crease, 0.04f);
                return tone;
            }, 4);

            foreach (float side in new[] { -1f, 1f })
            {
                // Eye: white, a glowing icy iris and a heavy lid
                Vector3 socket = new(eyeSocket.x * side, eyeSocket.y, eyeSocket.z);
                Vector3 eye = head + Vector3.Scale(socket.normalized * 0.93f, headRadii);
                Vector3 look = (socket.normalized + Vector3.forward * 2f).normalized;
                builder.Ball(eye, Vector3.one * 0.025f, Palette.Beard, 1f);
                glow.Ball(eye + look * 0.02f, new Vector3(0.015f, 0.015f, 0.008f), Palette.Frost, 0.85f);
                builder.Ball(eye + new Vector3(0f, 0.016f, 0.005f), new Vector3(0.029f, 0.012f, 0.024f), Palette.Face, 0.42f);

                // Long pointed ears showing through the hair, thin and swept back
                builder.Tube(new[] { new Vector3(0.105f * side, 1.25f, -0.01f), new Vector3(0.15f * side, 1.29f, -0.04f), new Vector3(0.19f * side, 1.35f, -0.075f) },
                             new[] { 0.03f, 0.022f, 0.004f }, Palette.Face, 0.72f, sides: 10, squash: new Vector2(1f, 0.4f));

                for (int strand = 0; strand < 3; strand++)
                {
                    Vector3 inner = new((0.025f + 0.012f * strand) * side, 1.29f + 0.004f * strand, 0.118f - 0.005f * strand);
                    Vector3 outer = new((0.11f + 0.015f * strand) * side, 1.315f + 0.015f * strand, 0.08f - 0.01f * strand);
                    builder.Tube(new[] { inner, Vector3.Lerp(inner, outer, 0.5f) + Vector3.up * 0.008f, outer }, new[] { 0.015f, 0.018f, 0.004f },
                                 Palette.Beard, 0.62f + 0.06f * strand, sides: 8);
                }

                for (int strand = 0; strand < 3; strand++)
                {
                    Vector3 root = new(0.01f * side, 1.205f - 0.005f * strand, 0.135f - 0.004f * strand);
                    Vector3 middle = new((0.06f + 0.01f * strand) * side, 1.19f - 0.008f * strand, 0.12f - 0.005f * strand);
                    Vector3 tip = new((0.1f + 0.015f * strand) * side, 1.08f - 0.03f * strand, 0.1f - 0.01f * strand);
                    builder.Tube(new[] { root, middle, tip }, new[] { 0.024f, 0.022f, 0.004f }, Palette.Beard, 0.78f - 0.05f * strand, sides: 10);
                }
            }

            // Long hair falling from under the hat over the back of the shoulders
            for (int strand = 0; strand < 5; strand++)
            {
                float angle = Mathf.Lerp(105f, 255f, strand / 4f) * Mathf.Deg2Rad;
                Vector3 around = new(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                builder.Tube(new[] { head + around * 0.11f + Vector3.up * 0.08f, head + around * 0.15f + Vector3.down * 0.1f, head + around * 0.17f + Vector3.down * 0.27f },
                             new[] { 0.035f, 0.04f, 0.01f }, Palette.Beard, 0.62f + 0.04f * (strand % 2), sides: 10);
            }

            // Beard: a body under the chin with locks falling over it to the sash, curling at the ends, clasped in silver
            builder.Tube(new[] { new Vector3(0f, 1.2f, 0.07f), new Vector3(0f, 1.08f, 0.12f), new Vector3(0f, 0.9f, 0.15f), new Vector3(0f, 0.72f, 0.15f),
                                 new Vector3(0f, 0.58f, 0.13f) },
                         new[] { 0.1f, 0.14f, 0.13f, 0.09f, 0.03f }, Palette.Beard, 0.7f, squash: new Vector2(1f, 0.6f));
            for (int lock_ = 0; lock_ < 7; lock_++)
            {
                float across = (lock_ - 3) / 3f;
                float edge = Mathf.Abs(across);
                float width = 1f - 0.35f * edge;
                float curl = (lock_ % 2 == 0 ? 1f : -1f) * 0.025f;

                Vector3[] path =
                {
                    new(across * 0.09f, 1.17f - edge * 0.03f, 0.1f - edge * 0.03f),
                    new(across * 0.115f, 1f, 0.15f - edge * 0.03f),
                    new(across * 0.08f + curl, 0.8f + edge * 0.06f, 0.16f - edge * 0.02f),
                    new(across * 0.03f + curl * 1.6f, 0.6f + edge * 0.1f, 0.14f),
                };
                builder.Tube(path, new[] { 0.045f * width, 0.05f * width, 0.035f * width, 0.006f }, Palette.Beard, 0.66f + 0.05f * (lock_ % 3), sides: 10,
                             squash: new Vector2(1f, 0.75f));
            }
            builder.Torus(new Vector3(0f, 0.7f, 0.155f), new Vector3(0f, -1f, -0.2f), new Vector2(0.03f, 0.03f), 0.01f, Palette.Trim, 0.65f);
        }

        // Tall wizard's hat: a wide soft brim drooping at the edge, a cone rising into a crooked tip, a silver band,
        // a glowing snowflake at the front and a few glowing stars scattered over it
        private static void BuildHat(Builder builder, Builder glow)
        {
            builder.Cylinder(new Vector3(0f, 1.36f, 0.01f), Vector3.up, 0.34f, 0.025f, 28, Palette.Robe, 0.42f, true);
            builder.Torus(new Vector3(0f, 1.355f, 0.01f), Vector3.up, new Vector2(0.34f, 0.34f), 0.018f, Palette.RobeDark, 0.4f, segments: 36, waviness: 0.02f);

            Vector3[] spine =
            {
                new(0f, 1.36f, 0.01f), new(0f, 1.55f, 0f), new(0f, 1.75f, -0.03f), new(0f, 1.9f, -0.09f), new(0.04f, 1.98f, -0.17f), new(0.1f, 1.97f, -0.23f)
            };
            float[] radii = { 0.15f, 0.12f, 0.085f, 0.05f, 0.025f, 0.008f };
            builder.Tube(spine, radii, Palette.Robe, 0.5f, sides: 20, roundStart: false, wrinkle: 0.06f);
            builder.Torus(new Vector3(0f, 1.395f, 0.01f), Vector3.up, new Vector2(0.148f, 0.148f), 0.017f, Palette.Violet, 0.55f, waviness: 0.006f);

            Snowflake(glow, new Vector3(0f, 1.47f, 0.148f), 0.04f);
            foreach ((float height, float angle) in new[] { (1.6f, 50f), (1.72f, -60f), (1.52f, 150f), (1.82f, 200f) })
            {
                float radius = Mathf.Lerp(0.12f, 0.085f, Mathf.InverseLerp(1.55f, 1.75f, height)) + 0.004f;
                Vector3 around = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                glow.Ball(new Vector3(0f, height, -0.03f * Mathf.InverseLerp(1.55f, 1.9f, height)) + around * radius, Vector3.one * 0.012f, Palette.Frost, 0.9f);
            }
        }

        // Open hand with the palm facing forward: a sculpted palm, four fingers spread a little and the thumb
        private static void OpenHand(Builder builder, Vector3 palm)
        {
            builder.Sculpt(palm, new Vector3(0.04f, 0.048f, 0.018f), new[] { (Vector3.forward, 0.6f, 0.1f) }, Palette.Face, direction => 0.5f, 2);

            float[] lengths = { 0.8f, 1f, 0.95f, 0.75f };
            for (int i = 0; i < 4; i++)
            {
                float across = (i - 1.5f) * 0.018f;
                Vector3 knuckle = palm + new Vector3(across, 0.042f, 0f);
                Vector3 tip = knuckle + new Vector3(across * 0.8f, 0.065f * lengths[i], -0.012f);
                builder.Tube(new[] { knuckle, Vector3.Lerp(knuckle, tip, 0.5f) + Vector3.forward * 0.004f, tip }, new[] { 0.012f, 0.011f, 0.009f },
                             Palette.Face, 0.55f, sides: 8);
            }

            builder.Tube(new[] { palm + new Vector3(0.036f, 0f, 0.004f), palm + new Vector3(0.058f, 0.025f, 0.012f), palm + new Vector3(0.065f, 0.05f, 0.008f) },
                         new[] { 0.015f, 0.013f, 0.01f }, Palette.Face, 0.55f, sides: 8);
        }

        // Hand wrapped round an upright staff at grip: the palm on the outer side, four fingers stacked up the staff
        // curling round its front, the thumb folded over the top one
        private static void GripHand(Builder builder, Vector3 grip)
        {
            builder.Sculpt(grip + new Vector3(0.036f, 0f, -0.01f), new Vector3(0.022f, 0.042f, 0.036f), new[] { (Vector3.right, 0.6f, 0.08f) },
                           Palette.Face, direction => 0.5f, 2);

            for (int i = 0; i < 4; i++)
            {
                Vector3 level = Vector3.up * ((i - 1.5f) * 0.019f);
                builder.Tube(new[] { grip + level + new Vector3(0.036f, 0f, 0.018f), grip + level + new Vector3(0.022f, 0f, 0.04f),
                                     grip + level + new Vector3(-0.012f, 0f, 0.036f), grip + level + new Vector3(-0.034f, 0f, 0.01f) },
                             new[] { 0.012f, 0.012f, 0.011f, 0.009f }, Palette.Face, 0.55f + 0.04f * (i % 2), sides: 8);
            }

            builder.Tube(new[] { grip + new Vector3(0.032f, 0.032f, -0.018f), grip + new Vector3(0.008f, 0.042f, 0f), grip + new Vector3(-0.012f, 0.038f, 0.018f) },
                         new[] { 0.015f, 0.013f, 0.01f }, Palette.Face, 0.58f, sides: 8);
        }

        // Six sided ice crystal from its foot along a direction, pointed at the far end, or at both ends
        private static void Crystal(Builder builder, Vector3 foot, Vector3 direction, float length, float radius, float tone, bool pointedBothEnds = false)
        {
            direction = direction.normalized;
            if (pointedBothEnds)
            {
                builder.Tube(new[] { foot, foot + direction * length * 0.45f, foot + direction * length }, new[] { 0.002f, radius, 0.002f },
                             Palette.Frost, tone - 0.15f, sides: 6, roundStart: false, roundEnd: false, toneEnd: tone + 0.2f);
            }
            else
            {
                builder.Tube(new[] { foot, foot + direction * length * 0.7f, foot + direction * length }, new[] { radius, radius * 0.95f, 0.002f },
                             Palette.Frost, tone - 0.2f, sides: 6, roundStart: false, roundEnd: false, toneEnd: tone + 0.2f);
            }
        }

        // Six armed snowflake facing forward, with a short branch half way out along each arm
        private static void Snowflake(Builder builder, Vector3 center, float size)
        {
            builder.Ball(center, Vector3.one * size * 0.25f, Palette.Frost, 0.9f);
            for (int arm = 0; arm < 6; arm++)
            {
                Quaternion turn = Quaternion.AngleAxis(arm * 60f, Vector3.forward);
                Vector3 tip = center + turn * Vector3.up * size;
                builder.Tube(new[] { center, tip }, new[] { size * 0.12f, size * 0.06f }, Palette.Frost, 0.85f, sides: 5, roundStart: false);

                Vector3 middle = Vector3.Lerp(center, tip, 0.55f);
                foreach (float branch in new[] { -40f, 40f })
                    builder.Tube(new[] { middle, middle + turn * Quaternion.AngleAxis(branch, Vector3.forward) * Vector3.up * size * 0.35f },
                                 new[] { size * 0.07f, size * 0.03f }, Palette.Frost, 0.85f, sides: 4, roundStart: false);
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
