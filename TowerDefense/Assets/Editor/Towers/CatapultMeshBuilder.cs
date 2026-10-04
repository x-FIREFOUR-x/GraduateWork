using System.Collections.Generic;

using UnityEngine;

using Random = System.Random;


namespace TowerDefense.EditorTools.Towers
{
    // Chunky toy-like catapult (a mangonel): thick bright beams, big wheels with light tyres, a twisted rope bundle
    // and a boulder sitting in the bucket. It stands on its wheels, the ground under them is at zero.
    // It faces +Z: the arm lies back along -Z on a rest pad and swings up against the stop bar to throw forwards.
    // Parts are meshes of their own, because the frame turns towards the target and the arm swings inside it.
    // Colors come from a gradient palette texture like the tile decor: one ramp per material, faces pick a shade
    public static class CatapultMeshBuilder
    {
        public enum Palette { Wood, DarkWood, Stone, Rope, Iron, Leather, Banner, Rock, Tyre, Skin, Tunic, Beard, Hat, Gold, Cloth, Face }

        // Every ramp runs from a deep, cooler shadow through the colour itself to a warmer highlight, so the shades
        // across a surface read as light and wear rather than as one flat paint. The colours are muted and the
        // highlights held back, earthy rather than bright, so nothing glows next to the grass and stone of the map
        private static readonly Color32[][] paletteRamps =
        {
            new Color32[] { new(88, 52, 30, 255), new(140, 86, 48, 255), new(178, 120, 74, 255), new(205, 156, 108, 255) },
            new Color32[] { new(62, 38, 24, 255), new(98, 62, 38, 255), new(136, 92, 60, 255), new(165, 122, 84, 255) },
            new Color32[] { new(64, 66, 70, 255), new(104, 104, 104, 255), new(146, 144, 138, 255), new(178, 174, 164, 255) },
            new Color32[] { new(110, 92, 66, 255), new(156, 134, 98, 255), new(190, 170, 128, 255), new(212, 196, 156, 255) },
            new Color32[] { new(40, 42, 48, 255), new(74, 78, 88, 255), new(114, 118, 126, 255), new(160, 162, 166, 255) },
            new Color32[] { new(52, 34, 24, 255), new(86, 56, 38, 255), new(122, 86, 60, 255), new(152, 114, 82, 255) },
            new Color32[] { new(30, 58, 38, 255), new(52, 88, 58, 255), new(86, 124, 80, 255), new(122, 154, 104, 255) },
            new Color32[] { new(62, 60, 58, 255), new(98, 94, 88, 255), new(138, 132, 122, 255), new(170, 164, 152, 255) },
            new Color32[] { new(96, 100, 108, 255), new(136, 140, 146, 255), new(172, 174, 176, 255), new(196, 196, 194, 255) },
            new Color32[] { new(196, 132, 100, 255), new(255, 208, 172, 255) },
            new Color32[] { new(30, 40, 64, 255), new(52, 70, 108, 255), new(82, 104, 146, 255), new(118, 138, 172, 255) },
            new Color32[] { new(130, 132, 136, 255), new(172, 172, 172, 255), new(204, 202, 198, 255), new(226, 222, 214, 255) },
            new Color32[] { new(78, 26, 24, 255), new(126, 44, 36, 255), new(162, 70, 52, 255), new(186, 104, 78, 255) },
            new Color32[] { new(100, 74, 32, 255), new(150, 114, 52, 255), new(186, 152, 80, 255), new(210, 184, 120, 255) },
            new Color32[] { new(58, 44, 32, 255), new(90, 70, 50, 255), new(124, 100, 74, 255), new(150, 126, 96, 255) },
            // Skin from shadow through its own colour and light to a flush, for the sculpted face and hands
            new Color32[] { new(120, 78, 66, 255), new(196, 146, 118, 255), new(214, 170, 140, 255), new(200, 124, 106, 255) },
        };
        private const int rampWidth = 32;
        private const int rowHeight = 4;
        // Fixed, so adding a color never shifts the rows meshes were baked with
        private const int paletteRows = 16;

        // The stone platform the catapult stands on, in world units: it is not scaled with the catapult, so it
        // can fill the tile exactly. Its top is where the wheels stand
        public const float PlatformRadius = 2.45f;
        public const float PlatformHeight = 0.45f;
        // Sunk into the tile, so uneven grass does not show under it
        private const float platformSink = 0.15f;

        // The gnome engineer stands beside the catapult, outside the wheels and clear of the arm and the stone,
        // facing forward like the catapult. In the frame space; he is built at his own size and drawn larger.
        // Sized against the attacking units: a foot soldier is about 3.4 units tall in the world, the gnome is
        // about two thirds of that: 1.04 tall as built, times this scale, times the catapult scale of 1.5
        public static readonly Vector3 GnomePosition = new(-1.3f, 0f, -0.42f);
        public const float GnomeYaw = 0f;
        public const float GnomeScale = 1.55f;
        // Where his right arm, swinging the wrench, turns, in his own space
        public static readonly Vector3 GnomeShoulder = new(0.18f, 0.4f, 0f);

        // Where the arm turns, in the frame space, and how far it reaches from there to the middle of the bucket
        public static readonly Vector3 ArmPivot = new(0f, 0.72f, -0.35f);
        private const float armLength = 1f;
        private const float armRootThickness = 0.34f;
        private const float armTipThickness = 0.28f;

        // Arm angles around its X axis: lying on the rest post and standing against the stop bar.
        // The post and the bar are placed so the arm meets them at exactly these angles. The arm rests raised
        // high, so the boulder hangs over the frame instead of far out behind the tile
        public const float ArmRestAngle = 45f;
        public const float ArmFireAngle = 82f;

        // The stone sits in the bucket, sticking out of it. Swinging, it passes over the A-frames and the bar,
        // which is why those stand low
        public const float StoneRadius = 0.56f;
        private const float stoneFlatness = 0.9f;
        private const float bucketFloor = 0.05f;

        // The stone in the bucket, in the arm space
        public static readonly Vector3 LoadedStoneCenter = new(0f, armTipThickness * 0.5f + bucketFloor + StoneRadius * stoneFlatness * 0.8f, -armLength);

        private const float railX = 0.52f;
        private const float railY = 0.6f;
        private const float railHeight = 0.3f;
        private const float railTop = railY + railHeight * 0.5f;

        // The stop bar sits low on the arm, so the bucket and the boulder at the top of the swing clear it
        private const float stopBarY = 1f;
        private const float stopBarDepth = 0.26f;
        private const float stopPadDepth = 0.08f;


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
                    // A ramp may have more than two stops, spread evenly along the row
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

        // Where the stone is when the arm stands against the stop bar, in the frame space: the shot leaves from here
        public static Vector3 ReleasePoint()
        {
            return ArmPivot + Quaternion.Euler(ArmFireAngle, 0f, 0f) * LoadedStoneCenter;
        }

        // Round stone platform of two block tiers, paved on top with slabs; it does not turn, so it is round.
        // The slabs are a shade apart and stand a hair above the darker top, which shows between them as grout
        public static Mesh BuildPlatform()
        {
            Builder builder = new(11, shadeNoise: 0.7f, noiseScale: 1.5f);
            const int sides = 12;
            const float lowerTop = 0.2f;

            builder.Frustum(new Vector3(0f, -platformSink, 0f), PlatformRadius, PlatformRadius - 0.05f, lowerTop + platformSink,
                            sides, Palette.Stone, 0.42f, 0.1f, 0f);
            builder.Frustum(new Vector3(0f, lowerTop, 0f), PlatformRadius - 0.12f, PlatformRadius - 0.17f, PlatformHeight - lowerTop,
                            sides, Palette.Stone, 0.55f, 0.1f, 360f / sides * 0.5f, -0.3f);
            builder.Paving(new Vector3(0f, PlatformHeight + 0.01f, 0f), PlatformRadius - 0.25f, 0.75f, sides, Palette.Stone, 0.7f);

            // Stones knocked loose at the foot of the platform, in the tile corners it leaves free
            builder.Rock(new Vector3(2.05f, 0f, 1.95f), 0.22f, 0.7f, 1, Palette.Stone, 0.45f);
            builder.Rock(new Vector3(1.8f, 0f, 2.25f), 0.14f, 0.7f, 1, Palette.Stone, 0.5f);
            builder.Rock(new Vector3(-2.0f, 0f, -2.0f), 0.18f, 0.65f, 1, Palette.Stone, 0.4f);

            return builder.ToMesh();
        }

        // Everything that turns with the catapult except the arm: a heavy chassis on six wheels with a plank floor and
        // iron straps, rope bundle, the A-frames carrying the stop bar, a pennant and a pile of stones ready to throw
        public static Mesh BuildFrame()
        {
            Builder builder = new(23, shadeNoise: 0.6f, noiseScale: 2.5f);

            // Big wheels: wooden disc, fat light tyre, light hub
            const float wheelRadius = 0.42f;
            foreach (float z in new[] { -0.85f, 0f, 0.85f })
            {
                builder.Cylinder(new Vector3(0f, wheelRadius, z), Vector3.right, 0.07f, 1.76f, 6, Palette.Iron, 0.45f, true);

                foreach (float x in new[] { -0.8f, 0.8f })
                {
                    Vector3 center = new(x, wheelRadius, z);
                    builder.Cylinder(center, Vector3.right, wheelRadius - 0.08f, 0.2f, 12, Palette.DarkWood, 0.6f, false);
                    builder.Cylinder(center, Vector3.right, wheelRadius, 0.16f, 12, Palette.Tyre, 0.7f, false);
                    builder.Cylinder(center, Vector3.right, 0.13f, 0.3f, 8, Palette.Tyre, 0.85f, false);
                }
            }

            // Chassis: two thick rails, a plank floor between them and cross beams at both ends
            foreach (float x in new[] { -railX, railX })
                builder.Box(new Vector3(x, railY, 0f), Quaternion.identity, new Vector3(0.28f, railHeight, 2.3f), Palette.Wood, 0.6f);

            builder.Box(new Vector3(0f, railY - 0.08f, 0.1f), Quaternion.identity, new Vector3(0.78f, 0.1f, 1.7f), Palette.DarkWood, 0.45f);
            builder.Box(new Vector3(0f, railY, 0.98f), Quaternion.identity, new Vector3(1.32f, 0.26f, 0.26f), Palette.DarkWood, 0.55f);
            builder.Box(new Vector3(0f, railY, -1f), Quaternion.identity, new Vector3(1.32f, 0.26f, 0.26f), Palette.DarkWood, 0.55f);

            // Iron straps around the rails and the ends of the cross beams
            foreach (float x in new[] { -railX, railX })
            {
                foreach (float z in new[] { -1.12f, -0.45f, 0.45f, 1.12f })
                    builder.Box(new Vector3(x, railY, z), Quaternion.identity, new Vector3(0.32f, railHeight + 0.04f, 0.08f), Palette.Iron, 0.5f);
            }

            // The post the arm rests on, topped with leather, set to the height of the arm lying at its rest angle
            const float padZ = -1f;
            const float padLeather = 0.07f;
            float rest = ArmRestAngle * Mathf.Deg2Rad;
            float restAlong = (ArmPivot.z - padZ) / Mathf.Cos(rest);
            float padTop = ArmPivot.y + (ArmPivot.z - padZ) * Mathf.Tan(rest) - ArmHalfThickness(restAlong) / Mathf.Cos(rest);
            float padBottom = railY + 0.13f;
            float postTop = padTop - padLeather;
            builder.Box(new Vector3(0f, (postTop + padBottom) * 0.5f, padZ), Quaternion.identity,
                        new Vector3(0.36f, postTop - padBottom, 0.26f), Palette.DarkWood, 0.5f);
            builder.Box(new Vector3(0f, postTop + padLeather * 0.5f, padZ), Quaternion.identity,
                        new Vector3(0.46f, padLeather, 0.32f), Palette.Leather, 0.5f);

            // Twisted rope bundle the arm is pushed through: short bands of alternating shade, iron caps at the rails
            const int ropeBands = 6;
            const float ropeLength = 0.76f;
            for (int i = 0; i < ropeBands; i++)
            {
                float x = -ropeLength * 0.5f + (i + 0.5f) * ropeLength / ropeBands;
                builder.Cylinder(new Vector3(x, ArmPivot.y, ArmPivot.z), Vector3.right, i % 2 == 0 ? 0.25f : 0.235f,
                                 ropeLength / ropeBands, 10, Palette.Rope, i % 2 == 0 ? 0.6f : 0.4f, true);
            }
            foreach (float x in new[] { -0.36f, 0.36f })
                builder.Cylinder(new Vector3(x, ArmPivot.y, ArmPivot.z), Vector3.right, 0.29f, 0.06f, 8, Palette.Iron, 0.5f, false);

            // Stop bar with the leather pad the arm hits on its rear face; both are placed from the arm standing at
            // its fire angle, the bar right in front of the pad
            float fire = ArmFireAngle * Mathf.Deg2Rad;
            float fireAlong = (stopBarY - ArmPivot.y) / Mathf.Sin(fire);
            float armAtBar = ArmPivot.z - (stopBarY - ArmPivot.y) / Mathf.Tan(fire);
            float padRear = armAtBar + ArmHalfThickness(fireAlong) / Mathf.Sin(fire) + 0.01f;
            // Both reach a little into each other, so no gap shows between them
            float stopBarZ = padRear + stopPadDepth + stopBarDepth * 0.5f - 0.03f;

            Vector3 apex = new(0f, stopBarY, stopBarZ);
            builder.Box(apex, Quaternion.identity, new Vector3(1.4f, 0.26f, stopBarDepth), Palette.DarkWood, 0.55f);
            builder.Box(new Vector3(0f, stopBarY, padRear + stopPadDepth * 0.5f), Quaternion.identity,
                        new Vector3(0.44f, 0.32f, stopPadDepth), Palette.Leather, 0.5f);

            // A-frames on both rails carrying the bar, with a brace halfway up
            Vector3 rearFoot = new(0f, railTop, stopBarZ - 0.5f);
            Vector3 frontFoot = new(0f, railTop, stopBarZ + 0.45f);
            foreach (float x in new[] { -railX, railX })
            {
                Vector3 side = Vector3.right * x;
                builder.Beam(rearFoot + side, apex + side + Vector3.up * 0.08f, 0.22f, 0.22f, Palette.Wood, 0.65f);
                builder.Beam(frontFoot + side, apex + side + Vector3.up * 0.08f, 0.22f, 0.22f, Palette.Wood, 0.65f);

                const float braceT = 0.45f;
                builder.Beam(Vector3.Lerp(rearFoot, apex, braceT) + side, Vector3.Lerp(frontFoot, apex, braceT) + side,
                             0.15f, 0.15f, Palette.DarkWood, 0.5f);
            }

            // Pennant at the front left corner, out of the way of the boulder
            Vector3 poleFoot = new(-railX - 0.1f, railTop, 1.05f);
            Vector3 poleTop = poleFoot + Vector3.up * 1.2f;
            builder.Cylinder((poleFoot + poleTop) * 0.5f, Vector3.up, 0.05f, 1.2f, 6, Palette.DarkWood, 0.4f, true);
            builder.Flag(poleTop, poleTop + Vector3.down * 0.38f, poleTop + new Vector3(0f, -0.15f, -0.6f), Palette.Banner, 0.55f);

            // Stones ready to throw, on the floor in front
            float floorTop = railY - 0.03f;
            builder.Rock(new Vector3(0f, floorTop, 0.72f), 0.2f, 0.85f, 1, Palette.Rock, 0.5f);
            builder.Rock(new Vector3(0.18f, floorTop, 0.48f), 0.16f, 0.85f, 1, Palette.Rock, 0.45f);
            builder.Rock(new Vector3(-0.17f, floorTop, 0.5f), 0.16f, 0.85f, 1, Palette.Rock, 0.55f);
            builder.Rock(new Vector3(0.02f, floorTop + 0.17f, 0.56f), 0.14f, 0.85f, 1, Palette.Rock, 0.6f);

            return builder.ToMesh();
        }

        // The throwing arm in its own space: it turns around the origin and reaches out along -Z,
        // the bucket opening towards +Y, which faces forward once the arm stands up
        public static Mesh BuildArm()
        {
            Builder builder = new(37, shadeNoise: 0.6f, noiseScale: 2.5f);

            builder.TaperedBeam(new Vector3(0f, 0f, 0.2f), new Vector3(0f, 0f, -armLength - 0.1f),
                                armRootThickness, armRootThickness, armTipThickness, armTipThickness, Palette.Wood, 0.65f);

            builder.Cylinder(Vector3.zero, Vector3.right, 0.15f, 0.34f, 8, Palette.Iron, 0.5f, false);

            foreach (float along in new[] { 0.42f, 0.78f })
            {
                float thickness = ArmHalfThickness(along) * 2f + 0.03f;
                builder.Box(new Vector3(0f, 0f, -along), Quaternion.identity, new Vector3(thickness, thickness, 0.08f), Palette.Iron, 0.5f);
            }

            builder.Cup(new Vector3(0f, armTipThickness * 0.5f, -armLength), 0.32f, 0.52f, 0.26f, bucketFloor, 12, Palette.DarkWood, 0.55f);

            return builder.ToMesh();
        }

        // Gnome engineer in his own space, standing on the origin, facing +Z. Built from smooth shapes only:
        // curved tubes for the body, limbs, beard and hat, rings for trims and rounded lumps for the head and hands,
        // so nothing reads as a box or a cone. Chunky like a strategy game unit: patched baggy trousers in tall laced
        // boots, a tunic with a little belly under a riveted cuirass, layered pauldrons, iron bracers, hands with
        // fingers, a pouch and a hammer on the belt, a big moustache and beard, bushy brows, pointed ears, a floppy
        // red hat with goggles, and a strapped pack with a bedroll and a rolled up drawing on his back.
        // The right arm with the wrench is a mesh of its own, so it can swing
        public static Mesh BuildGnome()
        {
            Builder builder = new(91, shadeNoise: 0.45f, noiseScale: 8f);

            foreach (float side in new[] { -1f, 1f })
                GnomeLeg(builder, 0.095f * side);

            // Tunic with a belly, its uneven hem high enough to show the legs, a folded collar at the neck
            builder.Tube(new[] { new Vector3(0f, 0.2f, 0.01f), new Vector3(0f, 0.29f, 0.025f), new Vector3(0f, 0.38f, 0.01f),
                                 new Vector3(0f, 0.46f, 0f), new Vector3(0f, 0.52f, 0f) },
                         new[] { 0.205f, 0.215f, 0.19f, 0.13f, 0.06f }, Palette.Tunic, 0.5f, sides: 20, squash: new Vector2(1f, 0.85f), roundStart: false,
                         wrinkle: 0.025f);
            builder.Torus(new Vector3(0f, 0.2f, 0.01f), Vector3.up, new Vector2(0.207f, 0.176f), 0.016f, Palette.Tunic, 0.3f, segments: 32, waviness: 0.01f);
            builder.Torus(new Vector3(0f, 0.5f, 0f), Vector3.up, new Vector2(0.085f, 0.075f), 0.022f, Palette.Tunic, 0.35f, waviness: 0.006f);

            // Belt with a buckle, a pouch with a buttoned flap on the left, a hammer in a loop on the right
            builder.Torus(new Vector3(0f, 0.27f, 0.022f), Vector3.up, new Vector2(0.217f, 0.185f), 0.024f, Palette.Leather, 0.4f, segments: 32, waviness: 0.008f);
            builder.Ball(new Vector3(0f, 0.262f, 0.212f), new Vector3(0.036f, 0.03f, 0.016f), Palette.Gold, 0.7f);

            Vector3 pouch = new(-0.175f, 0.215f, 0.1f);
            builder.Sculpt(pouch, new Vector3(0.05f, 0.05f, 0.035f), new[] { (Vector3.down, 0.6f, 0.15f) }, Palette.Leather, direction => 0.45f, 2);
            builder.Ball(pouch + new Vector3(0f, 0.03f, 0.012f), new Vector3(0.052f, 0.024f, 0.033f), Palette.Leather, 0.3f);
            builder.Ball(pouch + new Vector3(0f, 0.012f, 0.045f), Vector3.one * 0.011f, Palette.Gold, 0.8f);

            builder.Torus(new Vector3(0.222f, 0.265f, 0.06f), Vector3.up, new Vector2(0.022f, 0.022f), 0.007f, Palette.Leather, 0.3f);
            builder.Tube(new[] { new Vector3(0.222f, 0.33f, 0.06f), new Vector3(0.228f, 0.15f, 0.07f) }, new[] { 0.012f, 0.013f }, Palette.DarkWood, 0.55f, sides: 8);
            builder.Tube(new[] { new Vector3(0.228f, 0.145f, 0.035f), new Vector3(0.228f, 0.145f, 0.115f) }, new[] { 0.022f, 0.02f }, Palette.Iron, 0.55f, sides: 8);

            // Cuirass wrapped around the chest, worn a little uneven, its lower edge rolled in gold
            builder.Tube(new[] { new Vector3(0f, 0.31f, 0.025f), new Vector3(0f, 0.37f, 0.015f), new Vector3(0f, 0.44f, 0f) },
                         new[] { 0.226f, 0.216f, 0.172f }, Palette.Iron, 0.55f, sides: 20, squash: new Vector2(1f, 0.86f), roundStart: false, roundEnd: false,
                         wrinkle: 0.012f);
            builder.Torus(new Vector3(0f, 0.31f, 0.025f), Vector3.up, new Vector2(0.226f, 0.194f), 0.01f, Palette.Gold, 0.6f, segments: 32, waviness: 0.006f);

            foreach (float side in new[] { -1f, 1f })
                GnomePauldron(builder, side);

            // Left arm hanging at his side, the hand open and relaxed
            Vector3 leftWrist = new(-0.285f, 0.2f, 0.04f);
            GnomeArm(builder, new Vector3(-0.19f, 0.43f, 0f), new Vector3(-0.27f, 0.31f, 0f), leftWrist, Vector3.left);
            GnomeRelaxedHand(builder, leftWrist);

            // Pack on the back: a leather bag with a flap held by two straps with gold buckles, side pockets, straps
            // up to the shoulders, a bedroll tied on top and a rolled up drawing sticking out
            builder.Tube(new[] { new Vector3(0f, 0.29f, -0.24f), new Vector3(0f, 0.37f, -0.248f), new Vector3(0f, 0.44f, -0.24f) },
                         new[] { 0.11f, 0.12f, 0.105f }, Palette.Leather, 0.45f, sides: 14, squash: new Vector2(1.25f, 0.6f), wrinkle: 0.02f);
            builder.Ball(new Vector3(0f, 0.48f, -0.265f), new Vector3(0.14f, 0.06f, 0.07f), Palette.Leather, 0.32f);
            builder.Tube(new[] { new Vector3(-0.13f, 0.445f, -0.315f), new Vector3(0.13f, 0.445f, -0.315f) }, new[] { 0.008f, 0.008f }, Palette.Leather, 0.2f, sides: 6);
            foreach (float side in new[] { -1f, 1f })
            {
                builder.Tube(new[] { new Vector3(side * 0.06f, 0.5f, -0.31f), new Vector3(side * 0.06f, 0.4f, -0.328f), new Vector3(side * 0.06f, 0.31f, -0.325f) },
                             new[] { 0.01f, 0.01f, 0.01f }, Palette.Leather, 0.22f, sides: 8, squash: new Vector2(2.2f, 0.5f));
                builder.Torus(new Vector3(side * 0.06f, 0.37f, -0.332f), Vector3.forward, new Vector2(0.02f, 0.016f), 0.006f, Palette.Gold, 0.75f);

                builder.Sculpt(new Vector3(side * 0.155f, 0.32f, -0.245f), new Vector3(0.035f, 0.06f, 0.055f), new[] { (Vector3.down, 0.7f, 0.12f) },
                               Palette.Leather, direction => 0.38f, 2);
                builder.Ball(new Vector3(side * 0.157f, 0.37f, -0.245f), new Vector3(0.037f, 0.02f, 0.057f), Palette.Leather, 0.28f);

                builder.Tube(new[] { new Vector3(side * 0.1f, 0.47f, -0.2f), new Vector3(side * 0.13f, 0.5f, -0.12f), new Vector3(side * 0.15f, 0.47f, -0.04f) },
                             new[] { 0.014f, 0.014f, 0.014f }, Palette.Leather, 0.3f, sides: 8);
            }

            builder.Tube(new[] { new Vector3(-0.14f, 0.565f, -0.235f), new Vector3(0.14f, 0.565f, -0.235f) }, new[] { 0.055f, 0.055f }, Palette.Rope, 0.55f,
                         sides: 14, roundStart: false, roundEnd: false, wrinkle: 0.03f);
            foreach (float side in new[] { -1f, 1f })
            {
                // Rolled layers showing at the ends of the bedroll, and the straps tying it
                builder.Ball(new Vector3(side * 0.14f, 0.565f, -0.235f), new Vector3(0.012f, 0.055f, 0.055f), Palette.Rope, 0.45f);
                builder.Torus(new Vector3(side * 0.142f, 0.565f, -0.235f), Vector3.right, new Vector2(0.032f, 0.032f), 0.007f, Palette.Rope, 0.3f);
                builder.Torus(new Vector3(side * 0.08f, 0.565f, -0.235f), Vector3.right, new Vector2(0.058f, 0.058f), 0.008f, Palette.Leather, 0.25f);
            }

            Vector3 scrollBottom = new(0.11f, 0.43f, -0.29f);
            Vector3 scrollTop = new(0.15f, 0.66f, -0.3f);
            builder.Tube(new[] { scrollBottom, scrollTop }, new[] { 0.022f, 0.022f }, Palette.Rope, 0.85f, sides: 10);
            builder.Torus(Vector3.Lerp(scrollBottom, scrollTop, 0.6f), scrollTop - scrollBottom, new Vector2(0.024f, 0.024f), 0.006f, Palette.Hat, 0.5f);

            // Head sculpted in one piece: a big round nose, brow ridges over sunken eye sockets, full cheeks, a lower
            // lip and chin under the beard. Shadowed in the sockets, flushed on the cheeks and the tip of the nose
            Vector3 head = new(0f, 0.6f, 0f);
            Vector3 headRadii = new(0.13f, 0.135f, 0.125f);
            Vector3 nose = new(0f, -0.08f, 1f);
            Vector3 noseTip = new(0f, -0.18f, 1f);
            Vector3 eyeSocket = new(0.36f, 0.14f, 0.92f);
            Vector3 cheek = new(0.52f, -0.14f, 0.84f);
            Vector3 browRidge = new(0.36f, 0.33f, 0.9f);
            builder.Sculpt(head, headRadii, new[]
            {
                (nose, 0.24f, 0.42f),
                (noseTip, 0.13f, 0.14f),
                (new Vector3(0.13f, -0.2f, 1f), 0.1f, 0.1f),
                (new Vector3(-0.13f, -0.2f, 1f), 0.1f, 0.1f),
                (browRidge, 0.24f, 0.09f),
                (Mirror(browRidge), 0.24f, 0.09f),
                (new Vector3(0f, 0.3f, 0.95f), 0.2f, 0.05f),
                (eyeSocket, 0.13f, -0.09f),
                (Mirror(eyeSocket), 0.13f, -0.09f),
                (cheek, 0.26f, 0.13f),
                (Mirror(cheek), 0.26f, 0.13f),
                (new Vector3(0f, -0.48f, 0.88f), 0.16f, 0.05f),
                (new Vector3(0f, -0.7f, 0.7f), 0.3f, 0.06f),
            }, Palette.Face, direction => 0.5f
                - 0.22f * (Falloff(direction, eyeSocket, 0.15f) + Falloff(direction, Mirror(eyeSocket), 0.15f))
                + 0.4f * (Falloff(direction, cheek, 0.22f) + Falloff(direction, Mirror(cheek), 0.22f))
                + 0.35f * Falloff(direction, noseTip, 0.14f));

            foreach (float side in new[] { -1f, 1f })
            {
                // Eye set into its socket: white, a blue iris, a dark pupil and a heavy upper lid
                Vector3 socket = new(eyeSocket.x * side, eyeSocket.y, eyeSocket.z);
                Vector3 eye = head + Vector3.Scale(socket.normalized * 0.93f, headRadii);
                Vector3 look = (socket.normalized + Vector3.forward * 2f).normalized;
                builder.Ball(eye, Vector3.one * 0.028f, Palette.Beard, 1f);
                builder.Ball(eye + look * 0.022f, new Vector3(0.017f, 0.017f, 0.009f), Palette.Tunic, 0.35f);
                builder.Ball(eye + look * 0.027f, new Vector3(0.009f, 0.009f, 0.005f), Palette.Iron, 0f);
                builder.Ball(eye + new Vector3(0f, 0.017f, 0.005f), new Vector3(0.032f, 0.013f, 0.027f), Palette.Face, 0.42f);

                // Pointed ears, thin and leaning back
                builder.Tube(new[] { new Vector3(0.12f * side, 0.6f, -0.01f), new Vector3(0.165f * side, 0.635f, -0.035f), new Vector3(0.19f * side, 0.68f, -0.06f) },
                             new[] { 0.04f, 0.028f, 0.006f }, Palette.Face, 0.5f, squash: new Vector2(1f, 0.45f));

                // Bushy brows of a few strands, low at the nose and flaring up at the ends, which reads as a scowl
                for (int strand = 0; strand < 3; strand++)
                {
                    Vector3 inner = new((0.025f + 0.015f * strand) * side, 0.645f + 0.004f * strand, 0.132f - 0.006f * strand);
                    Vector3 outer = new((0.09f + 0.014f * strand) * side, 0.662f + 0.012f * strand, 0.1f - 0.012f * strand);
                    builder.Tube(new[] { inner, Vector3.Lerp(inner, outer, 0.5f) + Vector3.up * 0.008f, outer }, new[] { 0.017f, 0.02f, 0.005f },
                                 Palette.Beard, 0.3f + 0.08f * strand, sides: 8);
                }

                // Moustache of a few strands sweeping out and down from under the nose
                for (int strand = 0; strand < 3; strand++)
                {
                    Vector3 root = new(0.012f * side, 0.556f - 0.006f * strand, 0.152f - 0.004f * strand);
                    Vector3 tip = new((0.1f + 0.022f * strand) * side, 0.5f - 0.02f * strand, 0.09f - 0.012f * strand);
                    builder.Tube(new[] { root, new Vector3((0.055f + 0.01f * strand) * side, 0.545f - 0.008f * strand, 0.135f - 0.006f * strand), tip },
                                 new[] { 0.026f, 0.024f, 0.005f }, Palette.Beard, 0.85f - 0.06f * strand, sides: 10);
                }

                // Sideburns running down from under the hat into the beard
                builder.Tube(new[] { new Vector3(0.112f * side, 0.665f, 0.02f), new Vector3(0.12f * side, 0.6f, 0.05f), new Vector3(0.11f * side, 0.54f, 0.08f) },
                             new[] { 0.028f, 0.034f, 0.03f }, Palette.Beard, 0.68f, sides: 10);
            }

            // Hair at the back of the head, under the hat
            builder.Ball(new Vector3(0f, 0.63f, -0.045f), new Vector3(0.13f, 0.075f, 0.105f), Palette.Beard, 0.62f);

            // Full beard: a body under the chin with locks falling over it to the belly, curling at the ends,
            // the middle one tied with a gold ring
            builder.Tube(new[] { new Vector3(0f, 0.56f, 0.07f), new Vector3(0f, 0.48f, 0.11f), new Vector3(0f, 0.38f, 0.13f), new Vector3(0f, 0.3f, 0.12f) },
                         new[] { 0.1f, 0.13f, 0.11f, 0.06f }, Palette.Beard, 0.7f, squash: new Vector2(1f, 0.6f));
            for (int lock_ = 0; lock_ < 7; lock_++)
            {
                float across = (lock_ - 3) / 3f;
                float edge = Mathf.Abs(across);
                float width = 1f - 0.35f * edge;
                // Neighbouring locks curl the opposite way, so the ends do not line up
                float curl = (lock_ % 2 == 0 ? 1f : -1f) * 0.025f;

                Vector3[] path =
                {
                    new(across * 0.095f, 0.535f - edge * 0.03f, 0.105f - edge * 0.035f),
                    new(across * 0.115f, 0.43f, 0.15f - edge * 0.035f),
                    new(across * 0.07f + curl, 0.31f + edge * 0.05f, 0.155f - edge * 0.025f),
                    new(across * 0.03f + curl * 1.6f, 0.2f + edge * 0.08f, 0.135f),
                };
                builder.Tube(path, new[] { 0.05f * width, 0.055f * width, 0.04f * width, 0.008f }, Palette.Beard, 0.68f + 0.05f * (lock_ % 3), sides: 10,
                             squash: new Vector2(1f, 0.75f));
            }
            builder.Torus(new Vector3(0f, 0.255f, 0.145f), new Vector3(0f, -1f, -0.25f), new Vector2(0.03f, 0.03f), 0.012f, Palette.Gold, 0.75f);

            // Floppy hat widening into its own soft brim, its tip flopping over to the back, goggles on a strap around it
            builder.Tube(new[] { new Vector3(0f, 0.672f, 0f), new Vector3(0f, 0.715f, -0.004f), new Vector3(0f, 0.83f, -0.015f), new Vector3(0f, 0.95f, -0.05f),
                                 new Vector3(0f, 1.03f, -0.12f), new Vector3(0.01f, 1.06f, -0.2f), new Vector3(0.035f, 1.035f, -0.26f) },
                         new[] { 0.162f, 0.14f, 0.115f, 0.08f, 0.046f, 0.024f, 0.01f }, Palette.Hat, 0.55f, sides: 20, roundStart: false, wrinkle: 0.06f);
            builder.Torus(new Vector3(0f, 0.765f, -0.006f), Vector3.up, new Vector2(0.128f, 0.128f), 0.007f, Palette.Leather, 0.3f, waviness: 0.006f);
            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 goggle = new(0.05f * side, 0.765f, 0.118f);
                builder.Torus(goggle, Vector3.forward, new Vector2(0.03f, 0.03f), 0.009f, Palette.Leather, 0.35f, segments: 16);
                builder.Ball(goggle + Vector3.forward * 0.003f, new Vector3(0.026f, 0.026f, 0.009f), Palette.Tyre, 0.95f);
            }

            return builder.ToMesh();
        }

        // The gnome's right arm in its own space: turning around the shoulder at the origin, bent at the elbow, the
        // hand in front of the chest wrapped round an adjustable wrench held upright
        public static Mesh BuildGnomeArm()
        {
            Builder builder = new(97, shadeNoise: 0.45f, noiseScale: 8f);

            Vector3 wrist = new(0.065f, -0.16f, 0.12f);
            Vector3 grip = new(0.03f, -0.165f, 0.19f);
            GnomeArm(builder, Vector3.zero, new Vector3(0.08f, -0.11f, 0f), wrist, Vector3.right);
            GnomeGripHand(builder, wrist, grip);

            // Adjustable wrench held upright with its head tipped forward. Laid out along the handle (along), across it
            // towards the jaw opening (opening) and through its thickness (thin), all from the point in the fist
            Vector3 along = new Vector3(0.12f, 0.96f, 0.25f).normalized;
            // The jaws open sideways, so the wrench shows its profile to a viewer in front of the gnome
            Vector3 opening = Vector3.ProjectOnPlane(Vector3.right, along).normalized;
            Vector3 thin = Vector3.Cross(along, opening);
            Vector3 At(float u, float v, float w = 0f) => grip + along * u + opening * v + thin * w;

            // Flat steel handle, thin through its thickness, with a hanging hole at its end and a leather wrapped grip
            builder.Tube(new[] { At(-0.13f, 0f), At(0f, 0f), At(0.12f, 0f), At(0.18f, 0.004f) }, new[] { 0.022f, 0.024f, 0.024f, 0.028f },
                         Palette.Iron, 0.62f, sides: 10, squash: new Vector2(1f, 0.4f));
            builder.Torus(At(-0.118f, 0f), thin, new Vector2(0.01f, 0.01f), 0.0045f, Palette.Iron, 0.15f, segments: 12, sides: 6);
            builder.Tube(new[] { At(-0.1f, 0f), At(0.06f, 0f) }, new[] { 0.027f, 0.027f }, Palette.Leather, 0.4f, sides: 12,
                         squash: new Vector2(1.05f, 0.6f), wrinkle: 0.04f);

            // Head: a rounded block with the fixed jaw on one side and the sliding jaw on the other, a gap between them
            builder.Sculpt(At(0.215f, 0.004f), new Vector3(0.062f, 0.055f, 0.024f), new[] { (Vector3.right, 0.6f, 0.15f) },
                           Palette.Iron, direction => 0.55f, 2);
            builder.Tube(new[] { At(0.2f, 0.04f), At(0.26f, 0.058f), At(0.32f, 0.06f) }, new[] { 0.036f, 0.032f, 0.024f },
                         Palette.Iron, 0.58f, sides: 10, squash: new Vector2(1f, 0.6f));
            builder.Tube(new[] { At(0.205f, -0.04f), At(0.26f, -0.052f), At(0.31f, -0.053f) }, new[] { 0.032f, 0.028f, 0.021f },
                         Palette.Iron, 0.45f, sides: 10, squash: new Vector2(1f, 0.6f));
            builder.Ball(At(0.215f, 0f, 0.024f), Vector3.one * 0.009f, Palette.Gold, 0.75f);

            // Brass worm screw under the sliding jaw, knurled round
            builder.Tube(new[] { At(0.165f, -0.055f), At(0.165f, -0.008f) }, new[] { 0.017f, 0.017f }, Palette.Gold, 0.55f, sides: 10);
            for (int ridge = 0; ridge < 4; ridge++)
                builder.Torus(At(0.165f, -0.047f + ridge * 0.011f), opening, new Vector2(0.0175f, 0.0175f), 0.0045f, Palette.Gold, 0.35f, segments: 12, sides: 5);

            return builder.ToMesh();
        }

        // Leg standing at x: baggy trousers bulging at the knee with a stitched patch and a seam down the outer side,
        // tucked into a tall soft boot flaring at the top, laces crossing up its front, a rounded toe and a thick sole
        private static void GnomeLeg(Builder builder, float x)
        {
            float outer = Mathf.Sign(x);

            builder.Tube(new[] { new Vector3(x, 0.27f, 0f), new Vector3(x, 0.18f, 0.01f), new Vector3(x, 0.135f, 0f) },
                         new[] { 0.075f, 0.088f, 0.072f }, Palette.Cloth, 0.45f, sides: 16, roundStart: false, wrinkle: 0.07f);

            Vector3 knee = new(x, 0.18f, 0.095f);
            builder.Ball(knee, new Vector3(0.042f, 0.036f, 0.012f), Palette.Cloth, 0.3f);
            const int stitches = 8;
            for (int i = 0; i < stitches; i++)
            {
                float angle = i * Mathf.PI * 2 / stitches;
                builder.Ball(knee + new Vector3(Mathf.Cos(angle) * 0.04f, Mathf.Sin(angle) * 0.034f, 0.008f), Vector3.one * 0.005f, Palette.Rope, 0.75f);
            }
            builder.Tube(new[] { new Vector3(x + outer * 0.076f, 0.26f, 0f), new Vector3(x + outer * 0.089f, 0.18f, 0.01f), new Vector3(x + outer * 0.074f, 0.14f, 0f) },
                         new[] { 0.005f, 0.005f, 0.005f }, Palette.Cloth, 0.25f, sides: 6);

            builder.Tube(new[] { new Vector3(x, 0.05f, 0.01f), new Vector3(x, 0.12f, 0f), new Vector3(x, 0.155f, 0f) }, new[] { 0.07f, 0.074f, 0.09f },
                         Palette.Leather, 0.4f, sides: 16, roundStart: false, roundEnd: false, wrinkle: 0.05f);

            for (int i = 0; i < 3; i++)
            {
                float y = 0.055f + i * 0.025f;
                builder.Tube(new[] { new Vector3(x - 0.025f, y, 0.078f), new Vector3(x + 0.025f, y + 0.018f, 0.078f) }, new[] { 0.005f, 0.005f },
                             Palette.Rope, 0.6f, sides: 6);
                builder.Tube(new[] { new Vector3(x + 0.025f, y, 0.078f), new Vector3(x - 0.025f, y + 0.018f, 0.078f) }, new[] { 0.005f, 0.005f },
                             Palette.Rope, 0.6f, sides: 6);
            }

            builder.Tube(new[] { new Vector3(x, 0.045f, -0.055f), new Vector3(x, 0.045f, 0.05f), new Vector3(x, 0.055f, 0.125f) },
                         new[] { 0.07f, 0.075f, 0.058f }, Palette.Leather, 0.3f, squash: new Vector2(1.05f, 0.62f));
            builder.Tube(new[] { new Vector3(x, 0.012f, -0.062f), new Vector3(x, 0.012f, 0.05f), new Vector3(x, 0.016f, 0.132f) }, new[] { 0.072f, 0.077f, 0.06f },
                         Palette.Leather, 0.08f, squash: new Vector2(1.05f, 0.18f));
        }

        // Pauldron on the left (side -1) or right (side 1) shoulder: one plate shaped to the shoulder, thin towards
        // the neck and drooping out over the arm, with a single rivet on top
        private static void GnomePauldron(Builder builder, float side)
        {
            Vector3 shoulder = new(0.19f * side, 0.44f, 0f);

            builder.Sculpt(shoulder + new Vector3(0.012f * side, 0.025f, 0f), new Vector3(0.115f, 0.06f, 0.105f), new[]
            {
                (new Vector3(side, -0.6f, 0f), 0.55f, 0.25f),
                (new Vector3(-side, 0.2f, 0f), 0.5f, -0.2f),
                (Vector3.up, 0.5f, 0.15f),
                (new Vector3(0.3f * side, -0.2f, 1f), 0.45f, 0.08f),
            }, Palette.Iron, direction => 0.5f + 0.1f * direction.y, 3);
            builder.Ball(shoulder + new Vector3(0.03f * side, 0.09f, 0f), Vector3.one * 0.012f, Palette.Gold, 0.8f);
        }

        // Arm from the shoulder to the wrist: a creased sleeve rolled up at the elbow, a leather elbow pad, and a soft
        // leather bracer round the forearm with a strap buckled on its outer side. outward points away from the body.
        // The hand is added on its own, it differs between the arms
        private static void GnomeArm(Builder builder, Vector3 shoulder, Vector3 elbow, Vector3 wrist, Vector3 outward)
        {
            Vector3 upper = (elbow - shoulder).normalized;
            builder.Tube(new[] { shoulder, Vector3.Lerp(shoulder, elbow, 0.5f) + outward * 0.01f, elbow }, new[] { 0.062f, 0.06f, 0.056f }, Palette.Tunic, 0.45f,
                         sides: 14, wrinkle: 0.05f);
            builder.Torus(elbow - upper * 0.02f, upper, new Vector2(0.06f, 0.06f), 0.017f, Palette.Tunic, 0.3f, waviness: 0.006f);
            builder.Ball(elbow, Vector3.one * 0.045f, Palette.Leather, 0.45f);

            Vector3 along = (wrist - elbow).normalized;
            Vector3 bracerStart = Vector3.Lerp(elbow, wrist, 0.15f);
            Vector3 bracerMiddle = Vector3.Lerp(elbow, wrist, 0.55f);
            builder.Tube(new[] { bracerStart, bracerMiddle, wrist }, new[] { 0.06f, 0.064f, 0.056f }, Palette.Leather, 0.42f, sides: 14,
                         roundStart: false, roundEnd: false, wrinkle: 0.03f);
            builder.Torus(bracerMiddle, along, new Vector2(0.065f, 0.065f), 0.007f, Palette.Leather, 0.22f, waviness: 0.003f);
            builder.Ball(bracerMiddle + outward.normalized * 0.066f, new Vector3(0.012f, 0.012f, 0.012f), Palette.Gold, 0.8f);
        }

        // Left hand hanging below the wrist, the palm turned to the body: a sculpted palm, four slightly curled
        // fingers of different lengths and the thumb in front
        private static void GnomeRelaxedHand(Builder builder, Vector3 wrist)
        {
            Vector3 palm = wrist + new Vector3(-0.003f, -0.05f, 0.012f);
            builder.Sculpt(palm, new Vector3(0.026f, 0.045f, 0.042f), new[] { (new Vector3(0f, -1f, 0.3f), 0.5f, 0.08f) }, Palette.Face, direction => 0.5f, 2);

            float[] lengths = { 0.85f, 1f, 0.95f, 0.8f };
            for (int i = 0; i < 4; i++)
            {
                Vector3 knuckle = palm + new Vector3(0.004f, -0.035f, (i - 1.5f) * 0.019f + 0.004f);
                Vector3 middle = knuckle + new Vector3(0.006f, -0.03f, 0.004f) * lengths[i];
                Vector3 tip = knuckle + new Vector3(0.022f, -0.05f, 0.006f) * lengths[i];
                builder.Tube(new[] { knuckle, middle, tip }, new[] { 0.013f, 0.012f, 0.01f }, Palette.Face, 0.55f, sides: 8);
            }

            Vector3 thumbBase = palm + new Vector3(0.018f, -0.005f, 0.035f);
            Vector3 thumbMiddle = thumbBase + new Vector3(0.012f, -0.02f, 0.012f);
            builder.Tube(new[] { thumbBase, thumbMiddle, thumbMiddle + new Vector3(0.006f, -0.018f, 0.002f) }, new[] { 0.016f, 0.013f, 0.011f },
                         Palette.Face, 0.55f, sides: 8);
        }

        // Right hand wrapped round an upright handle at grip: the palm on the outer side, four fingers stacked up the
        // handle curling round its front, the thumb folded over the top finger
        private static void GnomeGripHand(Builder builder, Vector3 wrist, Vector3 grip)
        {
            Vector3 palm = grip + new Vector3(0.04f, 0f, -0.01f);
            builder.Tube(new[] { wrist, palm }, new[] { 0.03f, 0.028f }, Palette.Face, 0.5f, sides: 10);
            builder.Sculpt(palm, new Vector3(0.025f, 0.045f, 0.04f), new[] { (Vector3.right, 0.6f, 0.08f) }, Palette.Face, direction => 0.5f, 2);

            for (int i = 0; i < 4; i++)
            {
                Vector3 level = Vector3.up * ((i - 1.5f) * 0.02f);
                builder.Tube(new[] { grip + level + new Vector3(0.04f, 0f, 0.02f), grip + level + new Vector3(0.025f, 0f, 0.045f),
                                     grip + level + new Vector3(-0.01f, 0f, 0.04f), grip + level + new Vector3(-0.03f, 0f, 0.012f) },
                             new[] { 0.014f, 0.014f, 0.013f, 0.011f }, Palette.Face, 0.55f + 0.04f * (i % 2), sides: 8);
            }

            builder.Tube(new[] { grip + new Vector3(0.035f, 0.035f, -0.02f), grip + new Vector3(0.01f, 0.045f, 0f), grip + new Vector3(-0.012f, 0.04f, 0.02f) },
                         new[] { 0.017f, 0.015f, 0.012f }, Palette.Face, 0.58f, sides: 8);
        }

        // Thrown stone, also the one lying in the bucket
        public static Mesh BuildStone()
        {
            Builder builder = new(53, shadeNoise: 0.55f, noiseScale: 4f);
            builder.Rock(Vector3.zero, StoneRadius, stoneFlatness, 1, Palette.Rock, 0.5f, centered: true);

            return builder.ToMesh();
        }

        // Chunk flying off on impact. Coarse and unit sized, the particle system scales it
        public static Mesh BuildDebris()
        {
            Builder builder = new(71, shadeNoise: 0.55f, noiseScale: 4f);
            builder.Rock(Vector3.zero, 0.5f, 0.75f, 0, Palette.Rock, 0.45f, centered: true);

            return builder.ToMesh();
        }

        // How strongly a feature centred on one direction shows at another: 1 on it, fading off smoothly with the angle
        private static float Falloff(Vector3 direction, Vector3 featureDirection, float width)
        {
            float angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(direction.normalized, featureDirection.normalized), -1f, 1f)) / width;
            return Mathf.Exp(-angle * angle);
        }

        // The same direction on the other side of the face
        private static Vector3 Mirror(Vector3 direction)
        {
            return new Vector3(-direction.x, direction.y, direction.z);
        }

        // Half the arm's thickness at a distance along it from the pivot
        private static float ArmHalfThickness(float along)
        {
            return Mathf.Lerp(armRootThickness, armTipThickness, along / armLength) * 0.5f;
        }


        private class Builder
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
            private readonly float noiseScale;
            private readonly Vector3 noiseOffset;


            public Builder(int seed, float shadeNoise = 0.12f, float noiseScale = 6f)
            {
                rng = new Random(seed);
                this.shadeNoise = shadeNoise;
                this.noiseScale = noiseScale;
                // Not drawn from rng, so the shapes come out the same as without the noise
                noiseOffset = new Vector3(seed * 1.37f, seed * 2.11f, seed * 0.73f);
            }

            public Mesh ToMesh()
            {
                ShadeVertices();

                Mesh mesh = new Mesh();
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uvs);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();

                return mesh;
            }


            public void Box(Vector3 center, Quaternion rotation, Vector3 size, Palette palette, float tone)
            {
                Vector3 half = size * 0.5f;
                Vector3 back = center - rotation * Vector3.forward * half.z;
                Vector3 front = center + rotation * Vector3.forward * half.z;

                Prism(back, front, rotation, new Vector2(half.x, half.y), new Vector2(half.x, half.y), palette, tone);
            }

            // Square beam from one point to another; it keeps its top facing up as well as the slope lets it
            public void Beam(Vector3 from, Vector3 to, float width, float height, Palette palette, float tone)
            {
                TaperedBeam(from, to, width, height, width, height, palette, tone);
            }

            public void TaperedBeam(Vector3 from, Vector3 to, float fromWidth, float fromHeight, float toWidth, float toHeight,
                                    Palette palette, float tone)
            {
                Vector3 direction = (to - from).normalized;
                Vector3 up = Mathf.Abs(Vector3.Dot(direction, Vector3.up)) > 0.99f ? Vector3.forward : Vector3.up;
                Quaternion rotation = Quaternion.LookRotation(direction, up);

                Prism(from, to, rotation, new Vector2(fromWidth, fromHeight) * 0.5f, new Vector2(toWidth, toHeight) * 0.5f, palette, tone);
            }

            // Four sided prism between two rectangles lying across the rotation's forward axis
            private void Prism(Vector3 back, Vector3 front, Quaternion rotation, Vector2 backHalf, Vector2 frontHalf, Palette palette, float tone)
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
            public void Cylinder(Vector3 center, Vector3 axis, float radius, float length, int sides, Palette palette, float tone, bool smooth)
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
                                Palette palette, float tone, float toneJitter, float turnDegrees, float topShade = 0.1f)
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
            public void Paving(Vector3 center, float radius, float innerRadius, int sides, Palette palette, float tone)
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
                            Palette palette, float tone)
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
            public void Rock(Vector3 point, float radius, float flatness, int subdivisions, Palette palette, float tone, bool centered = false)
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
                    Polygon(new[] { points[faces[f]], points[faces[f + 1]], points[faces[f + 2]] }, center, palette,
                            tone + Range(-0.06f, 0.06f));
                }
            }

            // Smooth round lump, squashed by the radii given along each axis
            public void Ball(Vector3 center, Vector3 radii, Palette palette, float tone)
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
            public void Tube(Vector3[] spine, float[] radii, Palette palette, float tone, int sides = 12, Vector2? squash = null,
                             bool roundStart = true, bool roundEnd = true, float wrinkle = 0f)
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
                        AddVertex(positions[r, k], normal, UV(palette, Mathf.Clamp01(tone + normal.y * 0.12f)));
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
            public void Sculpt(Vector3 center, Vector3 radii, (Vector3 direction, float width, float height)[] bumps, Palette palette,
                               System.Func<Vector3, float> toneAt, int subdivisions = 3)
            {
                (Vector3[] sphere, int[] faces) = Icosphere(subdivisions);

                Vector3[] points = new Vector3[sphere.Length];
                for (int i = 0; i < sphere.Length; i++)
                {
                    float push = 0f;
                    foreach (var bump in bumps)
                        push += bump.height * Falloff(sphere[i], bump.direction, bump.width);

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
            public void Torus(Vector3 center, Vector3 axis, Vector2 ringRadii, float thickness, Palette palette, float tone, int segments = 20, int sides = 8,
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
            public void Flag(Vector3 a, Vector3 b, Vector3 c, Palette palette, float tone)
            {
                Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
                PolygonFacing(new[] { a, b, c }, normal, palette, tone);
                PolygonFacing(new[] { a, b, c }, -normal, palette, tone - 0.15f);
            }


            // Flat face turned away from a point inside the shape
            private void Polygon(Vector3[] points, Vector3 inside, Palette palette, float tone)
            {
                PolygonFacing(points, Centroid(points) - inside, palette, tone);
            }

            // Flat face whose normal agrees with the facing given. Faces turned up are a little lighter,
            // the way the tile decor shades them. Points go around a convex outline, it is fanned from the first
            private void PolygonFacing(Vector3[] points, Vector3 facing, Palette palette, float tone)
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
            private void SmoothQuad(Vector3[] points, Vector3[] cornerNormals, Palette palette, float tone)
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
        }
    }

}
