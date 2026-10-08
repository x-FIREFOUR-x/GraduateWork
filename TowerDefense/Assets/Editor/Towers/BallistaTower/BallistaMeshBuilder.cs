using UnityEngine;

using static TowerDefense.EditorTools.Towers.MeshSculpting;


namespace TowerDefense.EditorTools.Towers
{
    // A heavy dwarven ballista on a stone platform, worked by a stocky dwarf. The platform is built in world units, the
    // ground under it at zero; the ballista turns on a turntable on top of it, faces +Z and is drawn BallistaScale larger.
    // The ballista is a big crossbow on a post: a long stock with a groove on top, a front frame holding two torsion
    // skeins of rope with a bow arm thrust into each, a bowstring from the arm tips to a slider that runs along the
    // groove, and a windlass under the back of the stock with a crank handle on each side that winds the slider back.
    // The moving parts are meshes of their own: the bow arms bend back as the string is drawn, the string halves and
    // the windlass rope are unit long pieces stretched between their ends, the slider runs along the groove, the windlass
    // turns. The dwarf stands behind the ballista, aiming it by the two crank handles of the windlass, which he turns to
    // wind it; his arms are meshes of their own as well.
    // Colors come from a gradient palette texture like the catapult's: one ramp per material, faces pick a shade
    public static class BallistaMeshBuilder
    {
        public enum Palette { Wood, DarkWood, Stone, Rope, Iron, Leather, Banner, Gold, Steel, Mail, Beard, Face, Bone, Cloth, Feather, Fur }

        // Every ramp runs from a deep, cooler shadow through the colour itself to a warmer highlight. Muted and earthy
        // like the catapult: weathered wood, dark iron and bright steel, a burgundy clan colour, a ginger brown beard
        private static readonly Color32[][] paletteRamps =
        {
            new Color32[] { new(88, 52, 30, 255), new(140, 86, 48, 255), new(178, 120, 74, 255), new(205, 156, 108, 255) },
            new Color32[] { new(62, 38, 24, 255), new(98, 62, 38, 255), new(136, 92, 60, 255), new(165, 122, 84, 255) },
            new Color32[] { new(64, 66, 70, 255), new(104, 104, 104, 255), new(146, 144, 138, 255), new(178, 174, 164, 255) },
            new Color32[] { new(110, 92, 66, 255), new(156, 134, 98, 255), new(190, 170, 128, 255), new(212, 196, 156, 255) },
            new Color32[] { new(40, 42, 48, 255), new(74, 78, 88, 255), new(114, 118, 126, 255), new(160, 162, 166, 255) },
            new Color32[] { new(42, 26, 22, 255), new(92, 56, 36, 255), new(146, 98, 60, 255), new(196, 152, 104, 255) },
            new Color32[] { new(44, 12, 18, 255), new(100, 28, 36, 255), new(150, 52, 50, 255), new(190, 96, 80, 255) },
            new Color32[] { new(84, 52, 22, 255), new(156, 114, 46, 255), new(212, 170, 82, 255), new(246, 224, 152, 255) },
            new Color32[] { new(36, 44, 68, 255), new(100, 112, 136, 255), new(174, 180, 188, 255), new(244, 236, 216, 255) },
            new Color32[] { new(26, 30, 46, 255), new(72, 80, 98, 255), new(128, 132, 140, 255), new(186, 180, 164, 255) },
            new Color32[] { new(70, 34, 16, 255), new(140, 78, 36, 255), new(198, 132, 70, 255), new(238, 196, 134, 255) },
            // Skin from shadow through its own colour and light to a flush, for the sculpted face
            new Color32[] { new(120, 78, 66, 255), new(196, 146, 118, 255), new(214, 170, 140, 255), new(200, 124, 106, 255) },
            new Color32[] { new(150, 146, 136, 255), new(196, 192, 182, 255), new(226, 222, 212, 255), new(244, 242, 236, 255) },
            new Color32[] { new(30, 30, 34, 255), new(68, 60, 48, 255), new(108, 94, 70, 255), new(150, 132, 100, 255) },
            new Color32[] { new(120, 30, 30, 255), new(170, 52, 44, 255), new(206, 96, 74, 255), new(230, 160, 130, 255) },
            // Grey-brown fur, darker at the roots, frosted at the tips
            new Color32[] { new(52, 40, 30, 255), new(108, 88, 66, 255), new(164, 142, 112, 255), new(214, 200, 176, 255) },
        };
        private const int rampWidth = 32;
        private const int rowHeight = 4;
        // Fixed, so adding a color never shifts the rows meshes were baked with
        private const int paletteRows = 16;

        // The stone platform, filling the tile like the catapult's; its top is where the turntable stands
        public const float PlatformRadius = 2.45f;
        public const float PlatformHeight = 0.45f;
        private const float platformSink = 0.15f;

        // The ballista is built in world units, so it fits the tile with the bow arms swinging round. The bolt keeps its
        // size: it is built at its own size and drawn BoltScale larger, the one lying in the groove as well
        public const float BallistaScale = 1f;
        public const float BoltScale = 1.35f;

        // Laid out along the stock from the back: the windlass at its back end, the slider drawn back just in front of
        // it with the bolt from there to its tip, the front frame ahead of that. The windlass sits well forward of the
        // post, so the dwarf behind it has room even as the ballista kicks back at him, and the frame and the bow arms
        // stay on the tile
        private const float winchZ = -0.58f;
        private const float boltLength = BoltLength * BoltScale / BallistaScale;
        private const float loadedTipZ = winchZ + 0.1f + 0.34f + 0.02f + boltLength;
        public const float FrameZ = loadedTipZ + 0.5f;
        private const float frameZ = FrameZ;
        // Where things placed for the earlier layout, with the frame at 1.25, now stand
        private const float forward = FrameZ - 1.25f;

        // The stock: a long beam on the post, its top carrying two rails with the groove for the bolt between them,
        // low on a short post, so the dwarf behind it looks over the front frame along the bolt at what he aims at.
        // Its back end stops just behind the windlass
        private const float stockY = 0.95f;
        private const float stockHeight = 0.24f;
        private const float stockFront = FrameZ + 0.15f;
        private const float stockBack = winchZ - 0.1f;
        private const float railTop = stockY + stockHeight * 0.5f + 0.08f;

        // Height of the bowstring, the bolt and the bow arms
        public const float StringHeight = railTop + 0.05f;

        // The front frame with the torsion skeins; each bow arm turns round the middle of its skein
        private const float skeinX = 0.45f;
        public static readonly Vector3 LeftBowPivot = new(-skeinX, StringHeight, frameZ);
        public static readonly Vector3 RightBowPivot = new(skeinX, StringHeight, frameZ);
        public const float BowArmLength = 1.05f;
        // How far the arms are swept back round their skeins: with the string slack and fully drawn
        public const float BowReleasedAngle = 6f;
        public const float BowDrawnAngle = 30f;

        // The bolt, its tip at its origin and its shaft running back along -Z, as built
        public const float BoltLength = 1.35f;
        private const float boltRadius = 0.065f;
        // The loaded bolt lies in the groove with its nock against the drawn slider; the shot leaves from its tip
        public static readonly Vector3 LoadedBoltTip = new(0f, StringHeight, loadedTipZ);

        // The slider's claw holds the string; it runs along the groove between these two places
        public const float SliderReleasedZ = loadedTipZ + 0.3f;
        public const float SliderDrawnZ = loadedTipZ - boltLength - 0.02f;

        // The windlass through the back end of the stock: an axle with a drum inside it the rope winds on and a crank
        // either side, its handle pointing back at the dwarf at rest, at the height of his belly
        public static readonly Vector3 WinchCenter = new(0f, stockY + 0.05f, winchZ);
        private const float drumRadius = 0.09f;
        private const float crankX = 0.27f;
        // Long cranks, so his hands, and his beard behind them, stay well back from the ballista
        private const float crankRadius = 0.45f;
        private const float gripLength = 0.14f;
        // Where the rope comes up through a slot in the groove from the drum towards the slider
        public static readonly Vector3 RopeAnchor = new(0f, StringHeight, WinchCenter.z);
        // Turns of the windlass while the slider is wound from the front all the way back
        public const float WinchTurns = 2f;

        // The dwarf stands behind the ballista facing the way it shoots, his hands on the crank handles, so he aims it as
        // it turns and winds it from there. Built about 0.89 tall and drawn this much larger, so he stands about 2.2
        // tall, his eyes above the front frame, much broader than the catapult's gnome. In the turning part's space,
        // which the ballista is scaled in as well
        public const float DwarfScale = 2.5f;
        public const float DwarfYaw = 0f;
        // Where his hands are in his own space, from his shoulders: on the handles, his arms reaching out past his beard
        private const float dwarfReach = 0.27f;
        private static readonly float dwarfGripRise = WinchCenter.y * BallistaScale / DwarfScale - 0.6f;
        private const float dwarfGripIn = 0.215f - (crankX + gripLength * 0.5f) * BallistaScale / DwarfScale;
        public static readonly Vector3 DwarfPosition = new(0f, 0f, (WinchCenter.z - crankRadius) * BallistaScale - dwarfReach * DwarfScale);
        // Where his arms turn, in his own space
        public static readonly Vector3 DwarfLeftShoulder = new(-0.215f, 0.6f, 0f);
        public static readonly Vector3 DwarfRightShoulder = new(0.215f, 0.6f, 0f);


        public static Texture2D CreatePalette()
        {
            return MeshSculpting.CreatePalette(paletteRamps, rampWidth, paletteRows, rowHeight);
        }

        // Round stone platform of two block tiers, paved, with an iron ring the turntable runs on; a crate of spare
        // bolts and a barrel stand on the ground in the corners of the tile it leaves free
        public static Mesh BuildPlatform()
        {
            Builder builder = new(111, shadeNoise: 0.7f, noiseScale: 1.5f);
            const int sides = 12;
            const float lowerTop = 0.2f;

            builder.Frustum(new Vector3(0f, -platformSink, 0f), PlatformRadius, PlatformRadius - 0.05f, lowerTop + platformSink,
                            sides, Palette.Stone, 0.42f, 0.1f, 0f);
            builder.Frustum(new Vector3(0f, lowerTop, 0f), PlatformRadius - 0.12f, PlatformRadius - 0.17f, PlatformHeight - lowerTop,
                            sides, Palette.Stone, 0.55f, 0.1f, 360f / sides * 0.5f, -0.3f);
            builder.Paving(new Vector3(0f, PlatformHeight + 0.01f, 0f), PlatformRadius - 0.25f, 0.95f, sides, Palette.Stone, 0.7f);
            builder.Torus(new Vector3(0f, PlatformHeight + 0.02f, 0f), Vector3.up, new Vector2(0.85f, 0.85f), 0.05f, Palette.Iron, 0.45f, segments: 32, sides: 6);

            // Crate of spare bolts at the front left corner, bound in iron, bolts sticking out of its open top
            Vector3 crate = new(-1.95f, 0f, 1.95f);
            Quaternion crateTurn = Quaternion.Euler(0f, 30f, 0f);
            builder.Box(crate + Vector3.up * 0.3f, crateTurn, new Vector3(0.62f, 0.6f, 0.5f), Palette.DarkWood, 0.5f);
            foreach (float y in new[] { 0.08f, 0.52f })
                builder.Box(crate + Vector3.up * y, crateTurn, new Vector3(0.65f, 0.05f, 0.53f), Palette.Iron, 0.45f);
            for (int i = 0; i < 4; i++)
            {
                Vector3 foot = crate + crateTurn * new Vector3(-0.18f + i * 0.12f, 0.35f, (i % 2 - 0.5f) * 0.14f);
                Vector3 lean = (Vector3.up + crateTurn * new Vector3((i - 1.5f) * 0.15f, 0f, (i % 2 - 0.5f) * 0.3f)).normalized;
                Bolt(builder, foot + lean * BoltLength * 0.55f, lean, 0.55f);
            }

            // Barrel at the back right corner, hooped in iron
            Vector3 barrel = new(2.0f, 0f, -1.95f);
            builder.Tube(new[] { barrel, barrel + Vector3.up * 0.35f, barrel + Vector3.up * 0.7f }, new[] { 0.26f, 0.31f, 0.26f },
                         Palette.Wood, 0.5f, sides: 14, roundStart: false, roundEnd: false, wrinkle: 0.03f);
            builder.Cylinder(barrel + Vector3.up * 0.7f, Vector3.up, 0.25f, 0.02f, 14, Palette.DarkWood, 0.45f, false);
            foreach (float y in new[] { 0.12f, 0.58f })
                builder.Torus(barrel + Vector3.up * y, Vector3.up, new Vector2(0.29f, 0.29f), 0.02f, Palette.Iron, 0.45f, segments: 20, sides: 5);

            builder.Rock(new Vector3(2.1f, 0f, 1.85f), 0.2f, 0.7f, 1, Palette.Stone, 0.45f);
            builder.Rock(new Vector3(-1.85f, 0f, -2.15f), 0.16f, 0.7f, 1, Palette.Stone, 0.5f);

            return builder.ToMesh();
        }

        // Everything that turns with the ballista and does not move on its own: the turntable and post with its yoke,
        // the stock with its rails, iron plates and spare bolts, the front frame with the skeins, the windlass brackets,
        // the trigger and the clan banner
        public static Mesh BuildBallista()
        {
            Builder builder = new(123, shadeNoise: 0.6f, noiseScale: 2.5f);

            // Turntable on the iron ring, an iron rim round it, the post rising from it in iron bands, braces to the stock
            builder.Cylinder(new Vector3(0f, 0.07f, 0f), Vector3.up, 0.78f, 0.14f, 16, Palette.DarkWood, 0.5f, false);
            builder.Torus(new Vector3(0f, 0.1f, 0f), Vector3.up, new Vector2(0.78f, 0.78f), 0.035f, Palette.Iron, 0.5f, segments: 32, sides: 6);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2 / 8;
                builder.Ball(new Vector3(Mathf.Cos(angle) * 0.62f, 0.145f, Mathf.Sin(angle) * 0.62f), Vector3.one * 0.03f, Palette.Iron, 0.6f);
            }
            float postTop = stockY - stockHeight * 0.5f - 0.1f;
            builder.Cylinder(new Vector3(0f, (0.14f + postTop) * 0.5f, 0f), Vector3.up, 0.17f, postTop - 0.14f, 10, Palette.Wood, 0.55f, true);
            foreach (float y in new[] { 0.3f, postTop - 0.1f })
                builder.Cylinder(new Vector3(0f, y, 0f), Vector3.up, 0.19f, 0.06f, 10, Palette.Iron, 0.45f, false);
            foreach (float z in new[] { -1f, 1f })
                builder.Beam(new Vector3(0f, 0.35f, z * 0.12f), new Vector3(0f, stockY - stockHeight * 0.5f, z * 0.7f), 0.1f, 0.1f, Palette.DarkWood, 0.5f);

            // Yoke: two cheeks either side of the stock and the pin it rests on
            foreach (float x in new[] { -0.24f, 0.24f })
                builder.Box(new Vector3(x, stockY - 0.2f, 0f), Quaternion.identity, new Vector3(0.08f, 0.36f, 0.45f), Palette.DarkWood, 0.5f);
            builder.Cylinder(new Vector3(0f, stockY - 0.17f, 0f), Vector3.right, 0.06f, 0.64f, 8, Palette.Iron, 0.5f, true);

            // Stock with two rails on top and the groove between them, iron plates round it, rivets along the rails
            float stockLength = stockFront - stockBack;
            builder.Box(new Vector3(0f, stockY, (stockFront + stockBack) * 0.5f), Quaternion.identity, new Vector3(0.36f, stockHeight, stockLength),
                        Palette.Wood, 0.62f);
            foreach (float x in new[] { -0.09f, 0.09f })
                builder.Box(new Vector3(x, railTop - 0.04f, (stockFront + stockBack) * 0.5f - 0.05f), Quaternion.identity,
                            new Vector3(0.1f, 0.08f, stockLength - 0.1f), Palette.DarkWood, 0.5f);
            foreach (float z in new[] { -1.2f, -0.3f, 0.6f })
                builder.Box(new Vector3(0f, stockY, z + forward), Quaternion.identity, new Vector3(0.4f, stockHeight + 0.04f, 0.08f), Palette.Iron, 0.5f);
            for (int i = 0; i < 6; i++)
            {
                float z = Mathf.Lerp(stockBack + 0.25f, stockFront - 0.4f, i / 5f);
                foreach (float x in new[] { -0.18f, 0.18f })
                    builder.Ball(new Vector3(x, stockY + 0.06f, z), Vector3.one * 0.022f, Palette.Gold, 0.65f);
            }

            // Trigger lever under the stock in front of the yoke
            builder.Beam(new Vector3(0f, stockY - stockHeight * 0.5f, 0.45f), new Vector3(0f, stockY - 0.36f, 0.33f), 0.05f, 0.05f, Palette.Iron, 0.5f);
            builder.Ball(new Vector3(0f, stockY - 0.38f, 0.32f), Vector3.one * 0.045f, Palette.Iron, 0.55f);

            // Two spare bolts clipped to the right side of the stock
            foreach (float y in new[] { stockY - 0.05f, stockY + 0.06f })
                Bolt(builder, new Vector3(0.22f, y, 0.55f + forward), Vector3.forward, 0.75f);
            foreach (float z in new[] { -0.3f, 0.3f })
                builder.Box(new Vector3(0.22f, stockY, z + forward), Quaternion.identity, new Vector3(0.07f, 0.2f, 0.04f), Palette.Iron, 0.45f);

            // Front frame: beams above and below, inner posts leaving a window for the bolt, outer posts
            float frameBottom = StringHeight - 0.25f;
            float frameTop = StringHeight + 0.25f;
            foreach (float y in new[] { frameBottom, frameTop })
            {
                builder.Box(new Vector3(0f, y, frameZ), Quaternion.identity, new Vector3(1.32f, 0.12f, 0.26f), Palette.DarkWood, 0.55f);
                builder.Box(new Vector3(0f, y, frameZ), Quaternion.identity, new Vector3(1.36f, 0.14f, 0.06f), Palette.Iron, 0.45f);
            }
            foreach (float x in new[] { -0.24f, 0.24f, -0.66f, 0.66f })
                builder.Box(new Vector3(x, StringHeight, frameZ), Quaternion.identity, new Vector3(0.1f, frameTop - frameBottom, 0.24f), Palette.Wood, 0.6f);

            // Torsion skeins: twisted rope bundles in bands, held by iron washers with levers on top and below
            foreach (float x in new[] { -skeinX, skeinX })
            {
                const int bands = 5;
                float skeinBottom = frameBottom + 0.06f;
                float skeinTop = frameTop - 0.06f;
                for (int i = 0; i < bands; i++)
                {
                    float y = Mathf.Lerp(skeinBottom, skeinTop, (i + 0.5f) / bands);
                    builder.Cylinder(new Vector3(x, y, frameZ), Vector3.up, i % 2 == 0 ? 0.12f : 0.11f, (skeinTop - skeinBottom) / bands, 10, Palette.Rope,
                                     i % 2 == 0 ? 0.6f : 0.42f, true);
                }
                foreach (float y in new[] { frameBottom - 0.09f, frameTop + 0.09f })
                {
                    builder.Cylinder(new Vector3(x, y, frameZ), Vector3.up, 0.15f, 0.05f, 10, Palette.Iron, 0.5f, false);
                    builder.Box(new Vector3(x, y + (y > StringHeight ? 0.04f : -0.04f), frameZ), Quaternion.Euler(0f, 35f, 0f),
                                new Vector3(0.36f, 0.04f, 0.05f), Palette.Iron, 0.55f);
                }
            }

            // Iron bearing plates either side of the stock where the windlass axle runs through it
            foreach (float x in new[] { -0.185f, 0.185f })
            {
                builder.Box(new Vector3(x, WinchCenter.y - 0.02f, WinchCenter.z), Quaternion.identity, new Vector3(0.02f, stockHeight + 0.04f, 0.26f), Palette.Iron,
                            0.45f);
                builder.Ball(new Vector3(x * 1.08f, WinchCenter.y, WinchCenter.z), new Vector3(0.025f, 0.05f, 0.05f), Palette.Iron, 0.55f);
            }

            // The clan banner on a pole at the back right of the turntable: burgundy cloth with a gold hammer on it
            Vector3 poleFoot = new(0.55f, 0.14f, -0.45f);
            Vector3 poleTop = poleFoot + Vector3.up * 2.1f;
            builder.Cylinder((poleFoot + poleTop) * 0.5f, Vector3.up, 0.04f, 2.1f, 6, Palette.DarkWood, 0.4f, true);
            builder.Ball(poleTop + Vector3.up * 0.04f, Vector3.one * 0.06f, Palette.Gold, 0.7f);
            builder.Cylinder(poleTop + new Vector3(0.22f, -0.08f, 0f), Vector3.right, 0.02f, 0.46f, 6, Palette.DarkWood, 0.4f, true);
            Vector3 clothTop = poleTop + new Vector3(0.22f, -0.1f, 0f);
            builder.Box(clothTop + Vector3.down * 0.3f, Quaternion.identity, new Vector3(0.42f, 0.6f, 0.02f), Palette.Banner, 0.5f);
            builder.Flag(clothTop + new Vector3(-0.21f, -0.6f, 0f), clothTop + new Vector3(0.21f, -0.6f, 0f), clothTop + new Vector3(0f, -0.82f, 0f),
                         Palette.Banner, 0.45f);
            foreach (float z in new[] { -1f, 1f })
            {
                Vector3 face = clothTop + new Vector3(0f, -0.32f, 0.015f * z);
                builder.Box(face + Vector3.down * 0.06f, Quaternion.identity, new Vector3(0.04f, 0.24f, 0.01f), Palette.Gold, 0.6f);
                builder.Box(face + Vector3.up * 0.07f, Quaternion.identity, new Vector3(0.18f, 0.09f, 0.012f), Palette.Gold, 0.65f);
            }

            return builder.ToMesh();
        }

        // A bow arm in its own space, turning round the middle of its skein at the origin and reaching out along +X for the
        // right arm (side 1) or -X for the left one (side -1): a tapered beam, iron bound, with an iron nock at the tip
        public static Mesh BuildBowArm(float side)
        {
            Builder builder = new(side > 0f ? 131 : 137, shadeNoise: 0.6f, noiseScale: 2.5f);
            Vector3 tip = new(side * BowArmLength, 0f, 0f);

            builder.TaperedBeam(Vector3.zero, tip, 0.15f, 0.17f, 0.07f, 0.09f, Palette.Wood, 0.6f);
            foreach (float along in new[] { 0.3f, 0.65f, 0.95f })
            {
                float thickness = Mathf.Lerp(0.17f, 0.09f, along) + 0.03f;
                builder.Box(tip * along, Quaternion.identity, new Vector3(0.05f, thickness, thickness), Palette.Iron, 0.5f);
            }
            builder.Ball(tip, new Vector3(0.06f, 0.07f, 0.06f), Palette.Iron, 0.55f);

            return builder.ToMesh();
        }

        // Unit long piece of cord along +Z, stretched between its two ends by the tower: half of the bowstring, or the
        // windlass rope
        public static Mesh BuildCord(float radius)
        {
            Builder builder = new(141, shadeNoise: 0.3f, noiseScale: 6f);
            builder.Cylinder(new Vector3(0f, 0f, 0.5f), Vector3.forward, radius, 1f, 6, Palette.Rope, 0.55f, true);

            return builder.ToMesh();
        }

        // The slider in its own space, its claw holding the string at the origin: a block riding the rails behind it,
        // two iron prongs at its front either side of the nock, a pawl on its back
        public static Mesh BuildSlider()
        {
            Builder builder = new(149, shadeNoise: 0.6f, noiseScale: 3f);
            float bottom = railTop - StringHeight;

            builder.Box(new Vector3(0f, bottom + 0.05f, -0.18f), Quaternion.identity, new Vector3(0.34f, 0.1f, 0.32f), Palette.DarkWood, 0.55f);
            builder.Box(new Vector3(0f, bottom + 0.1f, -0.18f), Quaternion.identity, new Vector3(0.36f, 0.02f, 0.2f), Palette.Iron, 0.45f);
            foreach (float x in new[] { -0.09f, 0.09f })
                builder.Box(new Vector3(x, 0.02f, -0.01f), Quaternion.identity, new Vector3(0.035f, 0.11f, 0.06f), Palette.Iron, 0.55f);
            builder.Box(new Vector3(0f, bottom + 0.12f, -0.33f), Quaternion.Euler(-30f, 0f, 0f), new Vector3(0.06f, 0.03f, 0.12f), Palette.Iron, 0.5f);

            return builder.ToMesh();
        }

        // The windlass in its own space, turning round its axle at the origin along X: a drum with rope wound on it,
        // a ratchet wheel either side of it, and a crank at each end, its handle pointing back along -Z at rest
        public static Mesh BuildWinch()
        {
            Builder builder = new(151, shadeNoise: 0.6f, noiseScale: 3f);

            builder.Cylinder(Vector3.zero, Vector3.right, 0.035f, 2f * crankX + 0.04f, 8, Palette.Iron, 0.5f, true);
            builder.Cylinder(Vector3.zero, Vector3.right, drumRadius + 0.02f, 0.3f, 10, Palette.Wood, 0.55f, false);
            for (int i = 0; i < 5; i++)
                builder.Cylinder(Vector3.right * (-0.1f + i * 0.05f), Vector3.right, drumRadius + (i % 2 == 0 ? 0.03f : 0.025f), 0.05f, 10, Palette.Rope,
                                 i % 2 == 0 ? 0.6f : 0.45f, true);

            foreach (float side in new[] { -1f, 1f })
            {
                // Ratchet wheel against the side of the stock, its teeth all round
                Vector3 ratchet = Vector3.right * (side * 0.215f);
                builder.Cylinder(ratchet, Vector3.right, 0.1f, 0.03f, 16, Palette.Iron, 0.45f, false);
                for (int tooth = 0; tooth < 12; tooth++)
                {
                    float angle = tooth * Mathf.PI * 2 / 12;
                    builder.Ball(ratchet + new Vector3(0f, Mathf.Cos(angle), Mathf.Sin(angle)) * 0.105f, new Vector3(0.016f, 0.02f, 0.02f), Palette.Iron, 0.55f);
                }

                // Crank: an iron arm from the axle end back to a wooden handle sticking out sideways, a knob at its end
                Vector3 hub = Vector3.right * (side * crankX);
                Vector3 elbow = hub + Vector3.back * crankRadius;
                builder.Cylinder(hub, Vector3.right, 0.055f, 0.04f, 8, Palette.Iron, 0.55f, false);
                builder.TaperedBeam(hub, elbow, 0.05f, 0.07f, 0.04f, 0.05f, Palette.Iron, 0.5f);
                builder.Cylinder(elbow + Vector3.right * (side * gripLength * 0.5f), Vector3.right, 0.032f, gripLength, 8, Palette.Wood, 0.6f, true);
                builder.Ball(elbow + Vector3.right * (side * (gripLength + 0.01f)), Vector3.one * 0.04f, Palette.Iron, 0.55f);
            }

            return builder.ToMesh();
        }

        // The bolt, its tip at the origin and its shaft running back along -Z: an iron head, a shaft with a band at the
        // middle and three red feathered vanes at its end
        public static Mesh BuildBolt()
        {
            Builder builder = new(157, shadeNoise: 0.4f, noiseScale: 4f);
            Bolt(builder, Vector3.zero, Vector3.forward, 1f);

            return builder.ToMesh();
        }

        // A splinter of the shaft flung off where the bolt hits. Unit long, the particle system scales it
        public static Mesh BuildSplinter()
        {
            Builder builder = new(163, shadeNoise: 0.5f, noiseScale: 4f);
            builder.TaperedBeam(new Vector3(0f, 0f, -0.5f), new Vector3(0.05f, 0f, 0.5f), 0.22f, 0.12f, 0.02f, 0.02f, Palette.Wood, 0.55f);

            return builder.ToMesh();
        }

        // A heavy iron bolt with its tip at tip pointing along direction, scaled by size: a thick iron shaft with raised
        // bands, a big four sided armour piercing head with a collar, three steel fins at its end and a ring behind them
        private static void Bolt(Builder builder, Vector3 tip, Vector3 direction, float size)
        {
            direction = direction.normalized;
            Vector3 across = Vector3.Cross(direction, Mathf.Abs(direction.y) > 0.9f ? Vector3.right : Vector3.up).normalized;
            Vector3 across2 = Vector3.Cross(direction, across);
            float length = BoltLength * size;
            float radius = boltRadius * size;
            Vector3 end = tip - direction * length;

            builder.Tube(new[] { end, tip - direction * 0.3f * size }, new[] { radius, radius }, Palette.Iron, 0.55f, sides: 8, roundStart: false,
                         roundEnd: false);
            foreach (float t in new[] { 0.35f, 0.6f })
                builder.Cylinder(Vector3.Lerp(end, tip, t), direction, radius * 1.25f, 0.05f * size, 8, Palette.Iron, 0.35f, false);

            builder.Tube(new[] { tip - direction * 0.34f * size, tip - direction * 0.3f * size }, new[] { radius * 1.3f, radius * 1.5f }, Palette.Steel, 0.5f,
                         sides: 8, roundStart: false, roundEnd: false);
            builder.Tube(new[] { tip - direction * 0.3f * size, tip - direction * 0.2f * size, tip }, new[] { radius * 1.9f, radius * 2.1f, 0.004f },
                         Palette.Steel, 0.45f, sides: 4, roundStart: false, roundEnd: false, toneEnd: 0.85f);

            for (int fin = 0; fin < 3; fin++)
            {
                float angle = fin * Mathf.PI * 2 / 3;
                Vector3 outward = across * Mathf.Cos(angle) + across2 * Mathf.Sin(angle);
                Vector3 root = end + direction * 0.03f * size + outward * radius;
                Vector3 front = end + direction * 0.36f * size + outward * radius;
                Vector3 outer = end + direction * 0.06f * size + outward * (radius + 0.13f * size);
                Vector3 outerFront = end + direction * 0.2f * size + outward * (radius + 0.13f * size);
                builder.Flag(root, front, outerFront, Palette.Steel, 0.6f);
                builder.Flag(root, outerFront, outer, Palette.Steel, 0.55f);
            }
            builder.Cylinder(end + direction * 0.02f * size, direction, radius * 1.4f, 0.04f * size, 8, Palette.Iron, 0.4f, false);
        }


        // Stocky dwarf warrior in his own space, standing on the origin, facing +Z, built from smooth shapes only: heavy
        // boots with plated toes, greaves and knee cops, a mail shirt over his hips, a red kilt flap between plated
        // tassets, a broad belt with a gold buckle and pouches, a ridged breastplate with a gorget, pauldrons of three
        // overlapping plates, a sculpted face shouting under bushy brows, a spiked steel helmet with crossing bands,
        // cheek guards and goggles, a long flowing beard with two braids ending in gold rings, a long moustache, and a
        // war hammer slung across his back. His arms are meshes of their own
        public static Mesh BuildDwarf()
        {
            Builder builder = new(171, shadeNoise: 0.85f, noiseScale: 12f);

            foreach (float side in new[] { -1f, 1f })
                DwarfLeg(builder, 0.085f * side);

            // Mail shirt: a barrel of a body with a belly, its hem flaring over the hips above the knees
            builder.Tube(new[] { new Vector3(0f, 0.19f, 0.01f), new Vector3(0f, 0.27f, 0.02f), new Vector3(0f, 0.36f, 0.03f), new Vector3(0f, 0.48f, 0.02f),
                                 new Vector3(0f, 0.58f, 0f), new Vector3(0f, 0.64f, 0f) },
                         new[] { 0.225f, 0.215f, 0.232f, 0.232f, 0.19f, 0.08f }, Palette.Mail, 0.25f, sides: 24, squash: new Vector2(1f, 0.85f),
                         roundStart: false, wrinkle: 0.06f, toneEnd: 0.7f);
            builder.Torus(new Vector3(0f, 0.19f, 0.01f), Vector3.up, new Vector2(0.227f, 0.193f), 0.013f, Palette.Mail, 0.2f, segments: 36, waviness: 0.02f);
            builder.Tube(new[] { new Vector3(0f, 0.245f, 0.015f), new Vector3(0f, 0.2f, 0.012f), new Vector3(0f, 0.175f, 0.01f) },
                         new[] { 0.222f, 0.232f, 0.236f }, Palette.Mail, 0.3f, sides: 24, squash: new Vector2(1f, 0.85f), roundStart: false, roundEnd: false,
                         wrinkle: 0.03f, toneEnd: 0.15f);
            // The underside of the shirt closed with a dark padded lining, so no hole shows looking up under the hem
            builder.Ball(new Vector3(0f, 0.19f, 0.012f), new Vector3(0.232f, 0.03f, 0.198f), Palette.Cloth, 0.15f);
            for (int ring = 0; ring < 28; ring++)
            {
                float angle = ring * Mathf.PI * 2 / 28;
                builder.Ball(new Vector3(Mathf.Sin(angle) * 0.228f, 0.18f - 0.008f * (ring % 2), 0.01f + Mathf.Cos(angle) * 0.194f), new Vector3(0.016f, 0.012f, 0.016f),
                             Palette.Mail, 0.3f + 0.1f * (ring % 3));
            }

            // Kilt flap in the clan red hanging from the belt in front, crossed by dark and gold stripes
            builder.Tube(new[] { new Vector3(0f, 0.34f, 0.225f), new Vector3(0f, 0.25f, 0.235f), new Vector3(0.005f, 0.15f, 0.23f) },
                         new[] { 0.032f, 0.034f, 0.033f }, Palette.Banner, 0.65f, sides: 10, squash: new Vector2(3.2f, 0.32f), roundStart: false, roundEnd: false,
                         wrinkle: 0.08f, toneEnd: 0.3f);
            foreach ((float y, Palette palette, float tone) in new[] { (0.29f, Palette.Banner, 0.15f), (0.22f, Palette.Gold, 0.55f), (0.18f, Palette.Banner, 0.15f) })
                builder.Tube(new[] { new Vector3(-0.1f, y, 0.243f), new Vector3(0.1f, y, 0.243f) }, new[] { 0.006f, 0.006f }, palette, tone, sides: 6);
            foreach (float x in new[] { -0.05f, 0.05f })
                builder.Tube(new[] { new Vector3(x, 0.33f, 0.24f), new Vector3(x, 0.16f, 0.238f) }, new[] { 0.005f, 0.005f }, Palette.Banner, 0.2f, sides: 6);

            // Tassets either side of the flap: two overlapping plates hanging over each hip, a rivet on each
            foreach (float side in new[] { -1f, 1f })
            {
                for (int lame = 0; lame < 2; lame++)
                {
                    Vector3 outward = new Vector3(side * 0.75f, 0f, 1f).normalized;
                    Vector3 center = new Vector3(side * 0.14f, 0.3f - lame * 0.065f, 0.12f) + outward * (0.07f + lame * 0.012f);
                    builder.Sculpt(center, new Vector3(0.072f, 0.045f, 0.03f) * (1f - 0.08f * lame), new[]
                    {
                        (outward, 0.6f, -0.55f),
                        (-outward, 0.6f, -0.55f),
                        (Vector3.down, 0.5f, 0.15f),
                    }, Palette.Steel, direction => 0.55f - lame * 0.07f + 0.12f * direction.y, 2);
                    builder.Ball(center + outward * 0.03f + Vector3.up * 0.012f, Vector3.one * 0.009f, Palette.Gold, 0.8f);
                }
            }

            // Broad belt, a rounded gold buckle with a rune boss, pouches at both hips
            builder.Torus(new Vector3(0f, 0.355f, 0.03f), Vector3.up, new Vector2(0.242f, 0.21f), 0.032f, Palette.Leather, 0.4f, segments: 36, waviness: 0.014f);
            // Stitching along both edges of the belt
            for (int stitch = 0; stitch < 36; stitch++)
            {
                float angle = stitch * Mathf.PI * 2 / 36;
                foreach (float y in new[] { 0.334f, 0.376f })
                    builder.Ball(new Vector3(Mathf.Sin(angle) * 0.262f, y, 0.03f + Mathf.Cos(angle) * 0.229f), Vector3.one * 0.005f, Palette.Rope, 0.75f);
            }
            builder.Sculpt(new Vector3(0f, 0.355f, 0.245f), new Vector3(0.05f, 0.042f, 0.016f), new[] { (Vector3.forward, 0.5f, 0.3f) }, Palette.Gold,
                           direction => 0.55f + 0.2f * direction.y, 2);
            builder.Ball(new Vector3(0f, 0.355f, 0.262f), new Vector3(0.018f, 0.018f, 0.008f), Palette.Iron, 0.5f);
            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 pouch = new(side * 0.205f, 0.3f, 0.09f);
                builder.Sculpt(pouch, new Vector3(0.035f, 0.048f, 0.04f), new[] { (Vector3.down, 0.6f, 0.15f) }, Palette.Leather, direction => 0.45f, 2);
                builder.Ball(pouch + new Vector3(0f, 0.032f, 0.008f), new Vector3(0.038f, 0.02f, 0.043f), Palette.Leather, 0.28f);
                builder.Ball(pouch + new Vector3(side * 0.01f, 0.018f, 0.045f), Vector3.one * 0.008f, Palette.Gold, 0.8f);
            }

            // Breastplate over the chest with a ridge down its middle and a gold edge along the neckline, a plackart
            // over the belly riveted to it, a gorget round the neck
            builder.Tube(new[] { new Vector3(0f, 0.4f, 0.035f), new Vector3(0f, 0.5f, 0.025f), new Vector3(0f, 0.585f, 0.005f) },
                         new[] { 0.243f, 0.24f, 0.198f }, Palette.Steel, 0.35f, sides: 24, squash: new Vector2(1f, 0.86f), roundStart: false, roundEnd: false,
                         wrinkle: 0.008f, toneEnd: 0.8f);

            // Gold runes chased into the breastplate either side of the ridge
            foreach (float side in new[] { -1f, 1f })
            {
                for (int rune = 0; rune < 3; rune++)
                {
                    float y = 0.44f + rune * 0.04f;
                    Vector3 a = new(side * 0.05f, y, 0.238f - rune * 0.008f);
                    Vector3 b = new(side * 0.075f, y + 0.02f, 0.232f - rune * 0.008f);
                    Vector3 c = new(side * 0.1f, y, 0.222f - rune * 0.008f);
                    builder.Tube(new[] { a, b, c }, new[] { 0.004f, 0.004f, 0.004f }, Palette.Gold, 0.7f, sides: 5);
                }
            }
            builder.Tube(new[] { new Vector3(0f, 0.405f, 0.243f), new Vector3(0f, 0.5f, 0.236f), new Vector3(0f, 0.58f, 0.185f) }, new[] { 0.014f, 0.015f, 0.011f },
                         Palette.Steel, 0.8f, sides: 8);
            builder.Torus(new Vector3(0f, 0.585f, 0.005f), Vector3.up, new Vector2(0.198f, 0.17f), 0.01f, Palette.Gold, 0.6f, segments: 32);
            builder.Tube(new[] { new Vector3(0f, 0.37f, 0.04f), new Vector3(0f, 0.42f, 0.04f), new Vector3(0f, 0.46f, 0.035f) },
                         new[] { 0.247f, 0.25f, 0.246f }, Palette.Steel, 0.3f, sides: 24, squash: new Vector2(1f, 0.86f), roundStart: false, roundEnd: false,
                         toneEnd: 0.55f);
            for (int i = -2; i <= 2; i++)
            {
                float angle = i * 0.32f;
                builder.Ball(new Vector3(Mathf.Sin(angle) * 0.25f, 0.455f, 0.035f + Mathf.Cos(angle) * 0.218f), Vector3.one * 0.009f, Palette.Gold, 0.75f);
            }
            builder.Tube(new[] { new Vector3(0f, 0.6f, 0f), new Vector3(0f, 0.645f, 0f) }, new[] { 0.115f, 0.095f }, Palette.Steel, 0.5f, sides: 18,
                         roundStart: false, roundEnd: false);
            builder.Torus(new Vector3(0f, 0.62f, 0f), Vector3.up, new Vector2(0.112f, 0.112f), 0.008f, Palette.Iron, 0.45f, segments: 24);

            // Fur mantle round his shoulders and back under the pauldrons, open at the front where the beard falls: a roll
            // of fur lumps with tufts falling over its edge, dark at the roots and frosted at the tips
            for (int lump = 0; lump < 20; lump++)
            {
                float angle = lump * Mathf.PI * 2 / 20;
                if (Mathf.Cos(angle) > 0.6f)
                    continue;
                Vector3 around = new(Mathf.Sin(angle) * 0.17f, 0.6f + 0.006f * (lump % 2), -0.005f + Mathf.Cos(angle) * 0.15f);
                builder.Ball(around, new Vector3(0.055f, 0.045f, 0.055f), Palette.Fur, 0.35f + 0.08f * (lump % 3));
            }
            for (int tuft = 0; tuft < 26; tuft++)
            {
                float angle = tuft * Mathf.PI * 2 / 26;
                // None at the front, where the beard falls
                if (Mathf.Cos(angle) > 0.6f)
                    continue;
                Vector3 outward = new(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                Vector3 root = new Vector3(0f, 0.62f, -0.005f) + Vector3.Scale(outward, new Vector3(0.18f, 0f, 0.16f));
                float length = 0.045f + 0.02f * (tuft % 3);
                builder.Tube(new[] { root, root + outward * 0.04f + Vector3.down * 0.015f, root + outward * 0.05f + Vector3.down * length },
                             new[] { 0.022f, 0.018f, 0.003f }, Palette.Fur, 0.3f, sides: 6, toneEnd: 0.9f);
            }

            foreach (float side in new[] { -1f, 1f })
                DwarfPauldron(builder, side);

            BuildWarHammer(builder);
            BuildDwarfHead(builder);

            return builder.ToMesh();
        }

        // One of the dwarf's arms in its own space, turning round the shoulder at the origin, reaching down and forward to
        // the crank handle in front of him: a mail sleeve with plate lames over it, an elbow cop with a fan, a steel
        // vambrace, a flared gauntlet cuff and a heavy glove wrapped round the peg. Left is side -1, right side 1
        public static Mesh BuildDwarfArm(float side)
        {
            Builder builder = new(side > 0f ? 181 : 187, shadeNoise: 0.85f, noiseScale: 12f);
            Vector3 shoulder = Vector3.zero;
            Vector3 elbow = new(side * 0.035f, -0.17f, 0.02f);
            Vector3 grip = new(-side * dwarfGripIn, dwarfGripRise, dwarfReach);
            Vector3 wrist = grip + new Vector3(0f, 0.005f, -0.06f);

            Vector3 upper = (elbow - shoulder).normalized;
            builder.Tube(new[] { shoulder, Vector3.Lerp(shoulder, elbow, 0.5f) + new Vector3(side * 0.01f, 0f, 0f), elbow }, new[] { 0.07f, 0.068f, 0.062f },
                         Palette.Mail, 0.6f, sides: 14, wrinkle: 0.07f, toneEnd: 0.3f);
            for (int lame = 0; lame < 2; lame++)
                builder.Torus(Vector3.Lerp(shoulder, elbow, 0.45f + lame * 0.22f), upper, new Vector2(0.071f, 0.071f), 0.014f, Palette.Steel, 0.55f - lame * 0.08f,
                              segments: 16, sides: 6);

            // Elbow cop: a rounded plate over the joint with a fan on its outer side
            builder.Sculpt(elbow, new Vector3(0.06f, 0.06f, 0.06f), new[] { (new Vector3(side, -0.3f, -0.6f), 0.5f, 0.25f) }, Palette.Steel,
                           direction => 0.55f + 0.1f * direction.y, 2);
            builder.Sculpt(elbow + new Vector3(side * 0.05f, 0f, 0f), new Vector3(0.012f, 0.05f, 0.045f), new[] { (Vector3.up, 0.6f, 0.2f) }, Palette.Steel,
                           direction => 0.45f, 2);
            builder.Ball(elbow + new Vector3(side * 0.062f, 0f, 0f), Vector3.one * 0.01f, Palette.Gold, 0.8f);

            // Vambrace round the forearm, a ridge along its top, and the flared cuff of the gauntlet at the wrist
            Vector3 along = (wrist - elbow).normalized;
            Vector3 vambraceStart = Vector3.Lerp(elbow, wrist, 0.18f);
            Vector3 vambraceMiddle = Vector3.Lerp(elbow, wrist, 0.55f);
            builder.Tube(new[] { vambraceStart, vambraceMiddle, wrist - along * 0.02f }, new[] { 0.058f, 0.062f, 0.054f }, Palette.Steel, 0.5f, sides: 14,
                         roundStart: false, roundEnd: false, wrinkle: 0.01f);
            builder.Tube(new[] { vambraceStart + Vector3.up * 0.056f, vambraceMiddle + Vector3.up * 0.06f, wrist + Vector3.up * 0.05f - along * 0.02f },
                         new[] { 0.008f, 0.009f, 0.008f }, Palette.Steel, 0.8f, sides: 6);
            builder.Tube(new[] { wrist - along * 0.025f, wrist + along * 0.01f }, new[] { 0.062f, 0.07f }, Palette.Steel, 0.45f, sides: 14, roundStart: false,
                         roundEnd: false);
            builder.Torus(wrist + along * 0.01f, along, new Vector2(0.07f, 0.07f), 0.008f, Palette.Gold, 0.6f, segments: 16);

            DwarfFist(builder, wrist, grip, side);

            return builder.ToMesh();
        }

        // Heavy glove wrapped round a handle running across in front of him (along X) at grip: the back of the hand on
        // top, four fingers curling over the handle and under it, the thumb round from below, steel plates over the back
        // of the hand and the knuckles
        private static void DwarfFist(Builder builder, Vector3 wrist, Vector3 grip, float side)
        {
            Vector3 palm = grip + new Vector3(0f, 0.025f, -0.035f);
            builder.Tube(new[] { wrist, palm }, new[] { 0.036f, 0.034f }, Palette.Leather, 0.45f, sides: 10);
            builder.Sculpt(palm, new Vector3(0.048f, 0.03f, 0.04f), new[] { (Vector3.up, 0.6f, 0.08f) }, Palette.Leather, direction => 0.45f, 2);

            for (int i = 0; i < 4; i++)
            {
                float x = (i - 1.5f) * 0.022f;
                Vector3 Ring(float angle) => grip + new Vector3(x, Mathf.Cos(angle) * 0.04f, Mathf.Sin(angle) * 0.04f);
                builder.Tube(new[] { Ring(-0.4f), Ring(0.6f), Ring(1.6f), Ring(2.5f) }, new[] { 0.018f, 0.018f, 0.017f, 0.014f }, Palette.Leather,
                             0.5f + 0.04f * (i % 2), sides: 8);
                builder.Ball(Ring(0.1f) + new Vector3(0f, 0.008f, 0f), new Vector3(0.011f, 0.012f, 0.016f), Palette.Steel, 0.6f);
            }
            builder.Tube(new[] { grip + new Vector3(-side * 0.045f, -0.01f, -0.04f), grip + new Vector3(-side * 0.04f, -0.045f, -0.005f),
                                 grip + new Vector3(-side * 0.025f, -0.03f, 0.03f) },
                         new[] { 0.02f, 0.018f, 0.015f }, Palette.Leather, 0.55f, sides: 8);
            builder.Sculpt(palm + new Vector3(0f, 0.022f, 0f), new Vector3(0.045f, 0.015f, 0.04f), new[] { (Vector3.up, 0.6f, 0.15f) },
                           Palette.Steel, direction => 0.55f, 2);
        }

        // Leg at x: a thick thigh in dark breeches under the mail, a knee cop with a fan on its outer side, a greave down
        // the shin, a heavy leather boot with a turned down cuff, plated toes of overlapping lames and a thick sole
        private static void DwarfLeg(Builder builder, float x)
        {
            float outer = Mathf.Sign(x);

            builder.Tube(new[] { new Vector3(x, 0.26f, 0f), new Vector3(x, 0.2f, 0.01f), new Vector3(x, 0.15f, 0.01f) }, new[] { 0.085f, 0.088f, 0.078f },
                         Palette.Cloth, 0.55f, sides: 16, roundStart: false, wrinkle: 0.08f, toneEnd: 0.25f);

            Vector3 knee = new(x, 0.165f, 0.06f);
            builder.Sculpt(knee, new Vector3(0.058f, 0.052f, 0.04f), new[] { (Vector3.forward, 0.5f, 0.25f) }, Palette.Steel,
                           direction => 0.5f + 0.15f * direction.y, 2);
            builder.Sculpt(knee + new Vector3(outer * 0.05f, 0f, -0.02f), new Vector3(0.012f, 0.045f, 0.04f), new[] { (Vector3.up, 0.6f, 0.15f) }, Palette.Steel,
                           direction => 0.42f, 2);
            builder.Ball(knee + new Vector3(0f, 0f, 0.045f), Vector3.one * 0.01f, Palette.Gold, 0.8f);

            builder.Tube(new[] { new Vector3(x, 0.135f, 0.015f), new Vector3(x, 0.1f, 0.02f), new Vector3(x, 0.07f, 0.015f) }, new[] { 0.072f, 0.07f, 0.075f },
                         Palette.Steel, 0.7f, sides: 16, roundStart: false, roundEnd: false, toneEnd: 0.3f);
            builder.Tube(new[] { new Vector3(x, 0.035f, 0.01f), new Vector3(x, 0.06f, 0f), new Vector3(x, 0.085f, 0f) }, new[] { 0.082f, 0.085f, 0.092f },
                         Palette.Leather, 0.25f, sides: 16, roundStart: false, roundEnd: false, wrinkle: 0.06f, toneEnd: 0.5f);
            builder.Torus(new Vector3(x, 0.085f, 0f), Vector3.up, new Vector2(0.094f, 0.094f), 0.016f, Palette.Leather, 0.5f, waviness: 0.006f);

            builder.Tube(new[] { new Vector3(x, 0.04f, -0.06f), new Vector3(x, 0.042f, 0.05f), new Vector3(x, 0.05f, 0.13f) }, new[] { 0.08f, 0.085f, 0.065f },
                         Palette.Leather, 0.3f, squash: new Vector2(1.05f, 0.62f));
            for (int lame = 0; lame < 3; lame++)
            {
                float z = 0.05f + lame * 0.035f;
                builder.Tube(new[] { new Vector3(x - 0.06f, 0.05f - lame * 0.004f, z), new Vector3(x, 0.072f - lame * 0.006f, z + 0.005f),
                                     new Vector3(x + 0.06f, 0.05f - lame * 0.004f, z) },
                             new[] { 0.016f, 0.02f, 0.016f }, Palette.Steel, 0.6f - lame * 0.05f, sides: 8, squash: new Vector2(1f, 0.45f));
            }
            builder.Sculpt(new Vector3(x, 0.05f, 0.142f), new Vector3(0.058f, 0.035f, 0.03f), new[] { (Vector3.forward, 0.6f, 0.15f) }, Palette.Steel,
                           direction => 0.55f + 0.15f * direction.y, 2);
            builder.Tube(new[] { new Vector3(x, 0.012f, -0.07f), new Vector3(x, 0.012f, 0.05f), new Vector3(x, 0.016f, 0.16f) }, new[] { 0.084f, 0.09f, 0.07f },
                         Palette.Leather, 0.08f, squash: new Vector2(1.05f, 0.18f));
        }

        // Pauldron on the left (side -1) or right (side 1) shoulder: three overlapping steel plates, each smaller and
        // lower than the one above, drooping out over the arm, the top one edged in gold with a ridge and rivets
        private static void DwarfPauldron(Builder builder, float side)
        {
            Vector3 shoulder = new(0.205f * side, 0.6f, 0f);

            for (int lame = 2; lame >= 0; lame--)
            {
                Vector3 center = shoulder + new Vector3((0.01f + 0.03f * lame) * side, 0.03f - 0.035f * lame, 0f);
                Vector3 radii = new Vector3(0.115f, 0.055f, 0.11f) * (1f - 0.12f * lame);
                builder.Sculpt(center, radii, new[]
                {
                    (new Vector3(side, -0.6f, 0f), 0.55f, 0.25f),
                    (new Vector3(-side, 0.2f, 0f), 0.5f, -0.25f),
                    (Vector3.up, 0.5f, 0.15f),
                }, Palette.Steel, direction => 0.5f - 0.08f * lame + 0.28f * direction.y, 3);
                builder.Ball(center + new Vector3(0.06f * side * (1f - 0.12f * lame), 0.026f, 0.06f), Vector3.one * 0.008f, Palette.Gold, 0.8f);
            }

            Vector3 top = shoulder + new Vector3(0.01f * side, 0.03f, 0f);
            builder.Torus(top + new Vector3(0f, -0.02f, 0f), new Vector3(side * 0.4f, 1f, 0f), new Vector2(0.112f, 0.104f), 0.008f, Palette.Gold, 0.55f,
                          segments: 24);
            builder.Tube(new[] { top + new Vector3(-0.05f * side, 0.05f, 0f), top + new Vector3(0.025f * side, 0.058f, 0f), top + new Vector3(0.09f * side, 0.025f, 0f) },
                         new[] { 0.01f, 0.012f, 0.008f }, Palette.Steel, 0.75f, sides: 6);
        }

        // War hammer slung across his back: a long handle with a leather wrapped grip and iron rings, a pommel, steel
        // langets running down from the head, an eight sided double faced head with a gold rune band, a spike on top
        private static void BuildWarHammer(Builder builder)
        {
            Vector3 bottom = new(-0.2f, 0.14f, -0.27f);
            Vector3 top = new(0.17f, 0.84f, -0.285f);
            Vector3 along = (top - bottom).normalized;
            Vector3 Along(float t) => Vector3.Lerp(bottom, top, t);

            builder.Tube(new[] { bottom, Along(0.5f), top }, new[] { 0.017f, 0.019f, 0.02f }, Palette.DarkWood, 0.5f, sides: 10);
            builder.Tube(new[] { Along(0.05f), Along(0.3f) }, new[] { 0.023f, 0.023f }, Palette.Leather, 0.4f, sides: 10, wrinkle: 0.08f);
            for (int i = 0; i < 6; i++)
                builder.Torus(Along(0.06f + i * 0.045f), along, new Vector2(0.024f, 0.024f), 0.004f, Palette.Leather, 0.2f, segments: 10, sides: 4);
            foreach (float t in new[] { 0.32f, 0.6f })
                builder.Torus(Along(t), along, new Vector2(0.021f, 0.021f), 0.007f, Palette.Iron, 0.5f, segments: 12, sides: 5);
            builder.Ball(bottom - along * 0.02f, new Vector3(0.03f, 0.03f, 0.03f), Palette.Steel, 0.6f);
            builder.Torus(bottom, along, new Vector2(0.024f, 0.024f), 0.007f, Palette.Gold, 0.65f, segments: 12, sides: 5);

            // The head lies across the handle in the plane of his back
            Vector3 across = Vector3.ProjectOnPlane(Vector3.right, along).normalized;
            Vector3 head = top - along * 0.05f;
            foreach (float s in new[] { -1f, 1f })
                builder.Tube(new[] { head - along * 0.02f + Vector3.Cross(along, across) * (s * 0.02f), head - along * 0.15f + Vector3.Cross(along, across) * (s * 0.02f) },
                             new[] { 0.008f, 0.006f }, Palette.Steel, 0.6f, sides: 6, squash: new Vector2(1.6f, 0.5f));
            builder.Cylinder(head, across, 0.048f, 0.2f, 8, Palette.Steel, 0.55f, false);
            foreach (float s in new[] { -1f, 1f })
            {
                builder.Cylinder(head + across * (s * 0.105f), across, 0.056f, 0.025f, 8, Palette.Steel, 0.7f, false);
                builder.Cylinder(head + across * (s * 0.075f), across, 0.052f, 0.012f, 8, Palette.Iron, 0.4f, false);
            }
            builder.Cylinder(head, across, 0.05f, 0.035f, 8, Palette.Gold, 0.6f, false);
            builder.Torus(head, along, new Vector2(0.03f, 0.03f), 0.008f, Palette.Iron, 0.5f, segments: 12, sides: 5);
            builder.Tube(new[] { head + along * 0.04f, head + along * 0.1f }, new[] { 0.022f, 0.002f }, Palette.Steel, 0.7f, sides: 6, roundStart: false,
                         roundEnd: false);

            // The strap holding it, across his back from shoulder to hip, with a buckle
            builder.Tube(new[] { new Vector3(-0.16f, 0.6f, -0.16f), new Vector3(0f, 0.45f, -0.235f), new Vector3(0.17f, 0.3f, -0.2f) },
                         new[] { 0.012f, 0.012f, 0.012f }, Palette.Leather, 0.3f, sides: 8, squash: new Vector2(2.2f, 0.5f));
            builder.Torus(new Vector3(0f, 0.45f, -0.245f), new Vector3(0f, 0.3f, -1f), new Vector2(0.018f, 0.014f), 0.005f, Palette.Gold, 0.7f, segments: 12, sides: 5);
        }

        // Head sculpted in one piece: a big bulbous nose, heavy brow ridges over deep set eyes, broad cheeks, the mouth
        // open in a shout under the moustache. Under a round steel helmet with a spike, crossing riveted bands, a rim,
        // cheek guards and goggles on a strap; bushy brows drawn down in a scowl, a long moustache, a long flowing beard
        // past his belt with two braids ending in gold rings
        private static void BuildDwarfHead(Builder builder)
        {
            Vector3 head = new(0f, 0.72f, 0.015f);
            Vector3 headRadii = new(0.115f, 0.12f, 0.11f);
            Vector3 nose = new(0f, -0.1f, 1f);
            Vector3 noseTip = new(0f, -0.22f, 1f);
            Vector3 eyeSocket = new(0.38f, 0.12f, 0.9f);
            Vector3 cheek = new(0.55f, -0.15f, 0.82f);
            Vector3 browRidge = new(0.38f, 0.3f, 0.88f);
            Vector3 mouth = new(0f, -0.52f, 0.86f);
            builder.Sculpt(head, headRadii, new[]
            {
                (nose, 0.28f, 0.5f),
                (noseTip, 0.17f, 0.2f),
                (new Vector3(0.15f, -0.22f, 1f), 0.1f, 0.1f),
                (new Vector3(-0.15f, -0.22f, 1f), 0.1f, 0.1f),
                (browRidge, 0.26f, 0.14f),
                (Mirror(browRidge), 0.26f, 0.14f),
                (new Vector3(0f, 0.22f, 0.97f), 0.14f, 0.1f),
                (eyeSocket, 0.13f, -0.1f),
                (Mirror(eyeSocket), 0.13f, -0.1f),
                (cheek, 0.28f, 0.14f),
                (Mirror(cheek), 0.28f, 0.14f),
                (mouth, 0.22f, -0.12f),
            }, Palette.Face, direction => 0.5f
                - 0.25f * (Falloff(direction, eyeSocket, 0.15f) + Falloff(direction, Mirror(eyeSocket), 0.15f))
                + 0.4f * (Falloff(direction, cheek, 0.22f) + Falloff(direction, Mirror(cheek), 0.22f))
                + 0.45f * Falloff(direction, noseTip, 0.16f)
                - 0.4f * Falloff(direction, mouth, 0.18f));

            // The open mouth: a dark hollow, a row of teeth along its top, the tongue at its bottom
            Vector3 mouthPoint = head + Vector3.Scale(mouth.normalized * 0.86f, headRadii);
            builder.Ball(mouthPoint, new Vector3(0.042f, 0.03f, 0.02f), Palette.Iron, 0f);
            builder.Tube(new[] { mouthPoint + new Vector3(-0.03f, 0.018f, 0.008f), mouthPoint + new Vector3(0f, 0.022f, 0.014f), mouthPoint + new Vector3(0.03f, 0.018f, 0.008f) },
                         new[] { 0.008f, 0.009f, 0.008f }, Palette.Bone, 0.85f, sides: 6, squash: new Vector2(1f, 0.6f));
            builder.Ball(mouthPoint + new Vector3(0f, -0.016f, 0.008f), new Vector3(0.026f, 0.01f, 0.014f), Palette.Face, 0.9f);

            foreach (float side in new[] { -1f, 1f })
            {
                // Small deep set eyes glaring out: white, a grey-blue iris, a dark pupil, a heavy upper lid
                Vector3 socket = new(eyeSocket.x * side, eyeSocket.y, eyeSocket.z);
                Vector3 eye = head + Vector3.Scale(socket.normalized * 0.92f, headRadii);
                Vector3 look = (socket.normalized + Vector3.forward * 2f).normalized;
                builder.Ball(eye, Vector3.one * 0.024f, Palette.Bone, 0.95f);
                builder.Ball(eye + look * 0.019f, new Vector3(0.014f, 0.014f, 0.008f), Palette.Mail, 0.55f);
                builder.Ball(eye + look * 0.023f, new Vector3(0.008f, 0.008f, 0.004f), Palette.Iron, 0f);
                builder.Ball(eye + new Vector3(0f, 0.014f, 0.004f), new Vector3(0.03f, 0.012f, 0.025f), Palette.Face, 0.4f);

                // Bushy brows of thick strands drawn down at the nose in a scowl, the ends flaring up and out
                for (int strand = 0; strand < 4; strand++)
                {
                    Vector3 inner = new((0.018f + 0.012f * strand) * side, 0.752f + 0.005f * strand, 0.128f - 0.006f * strand);
                    Vector3 outer = new((0.1f + 0.012f * strand) * side, 0.765f + 0.006f * strand, 0.09f - 0.012f * strand);
                    builder.Tube(new[] { inner, Vector3.Lerp(inner, outer, 0.5f) + Vector3.up * 0.006f, outer }, new[] { 0.02f, 0.023f, 0.006f },
                                 Palette.Beard, 0.35f + 0.07f * strand, sides: 8, toneEnd: 0.75f);
                }

                // Long moustache from under the nose sweeping out and down past the corners of the mouth into the beard
                for (int strand = 0; strand < 4; strand++)
                {
                    Vector3 root = new(0.012f * side, 0.667f - 0.005f * strand, 0.143f - 0.004f * strand);
                    Vector3 middle = new((0.06f + 0.01f * strand) * side, 0.645f - 0.01f * strand, 0.135f - 0.006f * strand);
                    Vector3 tip = new((0.11f + 0.015f * strand) * side, 0.5f - 0.03f * strand, 0.215f - 0.01f * strand);
                    builder.Tube(new[] { root, middle, Vector3.Lerp(middle, tip, 0.5f) + new Vector3(0.015f * side, 0f, 0.01f), tip },
                                 new[] { 0.026f, 0.026f, 0.02f, 0.005f }, Palette.Beard, 0.45f - 0.05f * strand, sides: 10, toneEnd: 0.85f);
                }

                // Braids from the sides of the beard down past the belt, banded, each ending in a gold ring and a tuft
                Vector3[] braid =
                {
                    new(0.1f * side, 0.6f, 0.09f),
                    new(0.125f * side, 0.48f, 0.2f),
                    new(0.12f * side, 0.36f, 0.24f),
                    new(0.105f * side, 0.25f, 0.25f),
                };
                builder.Tube(braid, new[] { 0.04f, 0.036f, 0.032f, 0.028f }, Palette.Beard, 0.35f, sides: 10, wrinkle: 0.22f, toneEnd: 0.75f);
                for (int band = 1; band < 5; band++)
                    builder.Torus(Vector3.Lerp(braid[0], braid[3], band / 5f), braid[3] - braid[0], new Vector2(0.034f, 0.034f), 0.006f, Palette.Beard, 0.35f,
                                  segments: 12, sides: 5);
                builder.Torus(braid[3], braid[3] - braid[2], new Vector2(0.03f, 0.03f), 0.012f, Palette.Gold, 0.75f, segments: 14, sides: 6);
                builder.Tube(new[] { braid[3], braid[3] + new Vector3(0.005f * side, -0.06f, 0.005f) }, new[] { 0.027f, 0.006f }, Palette.Beard, 0.6f, sides: 8,
                             toneEnd: 0.9f);

                // Hair at the sides falling from under the helmet
                builder.Tube(new[] { new Vector3(0.11f * side, 0.745f, -0.02f), new Vector3(0.122f * side, 0.69f, 0f), new Vector3(0.11f * side, 0.62f, 0.04f) },
                             new[] { 0.03f, 0.036f, 0.03f }, Palette.Beard, 0.3f, sides: 10, wrinkle: 0.12f, toneEnd: 0.65f);

                // Cheek guards hanging from the helmet rim along his cheeks, riveted
                Vector3 guard = new(0.122f * side, 0.735f, 0.04f);
                builder.Sculpt(guard, new Vector3(0.022f, 0.06f, 0.055f), new[] { (new Vector3(side, 0f, 0f), 0.6f, 0.1f), (Vector3.down, 0.5f, 0.2f) },
                               Palette.Steel, direction => 0.5f + 0.15f * direction.y, 2);
                builder.Ball(guard + new Vector3(side * 0.022f, 0.035f, 0f), Vector3.one * 0.008f, Palette.Gold, 0.8f);
            }

            // The beard: a broad mass under the chin, of locks flowing down past the belt and ending in uneven points
            // The body of the beard stays behind the locks, so they are what shows
            builder.Tube(new[] { new Vector3(0f, 0.66f, 0.08f), new Vector3(0f, 0.57f, 0.15f), new Vector3(0f, 0.46f, 0.19f), new Vector3(0f, 0.36f, 0.2f) },
                         new[] { 0.1f, 0.125f, 0.11f, 0.06f }, Palette.Beard, 0.2f, sides: 20, squash: new Vector2(1f, 0.5f), wrinkle: 0.14f, toneEnd: 0.45f);
            for (int lock_ = 0; lock_ < 9; lock_++)
            {
                float across = (lock_ - 4) / 4f;
                float edge = Mathf.Abs(across);
                float width = 1f - 0.3f * edge;
                float curl = (lock_ % 2 == 0 ? 1f : -1f) * 0.02f;
                float end = 0.22f + edge * 0.08f + (lock_ % 3) * 0.015f;
                Vector3[] path =
                {
                    new(across * 0.09f, 0.635f - edge * 0.03f, 0.11f - edge * 0.03f),
                    new(across * 0.115f, 0.53f, 0.165f - edge * 0.035f),
                    new(across * 0.085f + curl, 0.41f + edge * 0.03f, 0.19f - edge * 0.025f),
                    new(across * 0.05f + curl * 1.6f, end + 0.06f, 0.195f - edge * 0.02f),
                    new(across * 0.04f + curl * 2.4f, end, 0.185f),
                };
                // Lying over the breastplate, out in front of the body of the beard
                for (int k = 1; k < path.Length; k++)
                    path[k] += Vector3.forward * (k == 1 ? 0.05f : 0.07f);
                builder.Tube(path, new[] { 0.042f * width, 0.048f * width, 0.04f * width, 0.026f * width, 0.006f }, Palette.Beard, 0.3f + 0.06f * (lock_ % 3),
                             sides: 10, squash: new Vector2(1f, 0.75f), wrinkle: 0.16f, toneEnd: 0.75f + 0.08f * (lock_ % 2));

                // Finer strands laid over the lock, so the beard reads as hair rather than as a few thick ropes
                for (int strand = -1; strand <= 1; strand += 2)
                {
                    Vector3 offset = new(strand * 0.012f * width, 0f, 0.012f);
                    builder.Tube(new[] { path[0] + offset, path[1] + offset * 1.4f, path[2] + offset * 1.2f, path[3] + offset * 0.6f + new Vector3(0f, 0.03f, 0f) },
                                 new[] { 0.012f, 0.014f, 0.011f, 0.003f }, Palette.Beard, 0.45f, sides: 6, toneEnd: 0.9f);
                }
                if (lock_ == 2 || lock_ == 6)
                    builder.Ball(path[2] + new Vector3(0f, 0f, 0.03f), new Vector3(0.022f, 0.016f, 0.022f), Palette.Gold, 0.7f);
            }

            // Hair at the back of the neck
            builder.Tube(new[] { new Vector3(0f, 0.745f, -0.075f), new Vector3(0f, 0.68f, -0.09f), new Vector3(0f, 0.6f, -0.08f) }, new[] { 0.095f, 0.1f, 0.07f },
                         Palette.Beard, 0.25f, squash: new Vector2(1f, 0.6f), wrinkle: 0.14f, toneEnd: 0.55f);

            // Helmet: a dome rising to a spike, two riveted bands crossing over it, a thick rim
            builder.Tube(new[] { new Vector3(0f, 0.785f, 0.01f), new Vector3(0f, 0.84f, 0.01f), new Vector3(0f, 0.895f, 0.005f), new Vector3(0f, 0.93f, 0f) },
                         new[] { 0.132f, 0.127f, 0.095f, 0.03f }, Palette.Steel, 0.35f, sides: 24, roundStart: false, toneEnd: 0.9f);
            builder.Tube(new[] { new Vector3(0f, 0.92f, 0f), new Vector3(0f, 0.96f, 0f), new Vector3(0f, 1.015f, 0f) }, new[] { 0.028f, 0.014f, 0.002f },
                         Palette.Steel, 0.75f, sides: 8, roundEnd: false);
            builder.Torus(new Vector3(0f, 0.925f, 0f), Vector3.up, new Vector2(0.03f, 0.03f), 0.008f, Palette.Gold, 0.6f, segments: 12, sides: 5);
            builder.Torus(new Vector3(0f, 0.788f, 0.01f), Vector3.up, new Vector2(0.135f, 0.135f), 0.017f, Palette.Iron, 0.45f, segments: 32);
            foreach (Vector3 axis in new[] { Vector3.right, Vector3.forward })
            {
                builder.Torus(new Vector3(0f, 0.79f, 0.01f), axis, new Vector2(0.132f, 0.14f), 0.011f, Palette.Iron, 0.5f, segments: 24, sides: 5,
                              arcStart: axis == Vector3.right ? 90f : 180f, arcDegrees: 180f);
            }
            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI * 2 / 12;
                builder.Ball(new Vector3(Mathf.Sin(angle) * 0.15f, 0.788f, 0.01f + Mathf.Cos(angle) * 0.15f), Vector3.one * 0.008f, Palette.Gold, 0.8f);
            }

            // Goggles pushed up on the front of the helmet on a leather strap
            builder.Torus(new Vector3(0f, 0.83f, 0.01f), Vector3.up, new Vector2(0.13f, 0.13f), 0.01f, Palette.Leather, 0.3f, segments: 28, waviness: 0.004f);
            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 goggle = new(0.045f * side, 0.835f, 0.127f);
                Vector3 facing = new Vector3(0.25f * side, 0.15f, 1f).normalized;
                builder.Torus(goggle, facing, new Vector2(0.027f, 0.027f), 0.009f, Palette.Gold, 0.55f, segments: 16);
                builder.Ball(goggle + facing * 0.004f, new Vector3(0.024f, 0.024f, 0.01f), Palette.Mail, 0.85f);
            }
            builder.Tube(new[] { new Vector3(-0.018f, 0.837f, 0.135f), new Vector3(0.018f, 0.837f, 0.135f) }, new[] { 0.007f, 0.007f }, Palette.Gold, 0.5f, sides: 6);
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
