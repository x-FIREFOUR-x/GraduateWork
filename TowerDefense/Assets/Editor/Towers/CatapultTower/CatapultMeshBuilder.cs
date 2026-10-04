using UnityEngine;

using static TowerDefense.EditorTools.Towers.MeshSculpting;


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
            return MeshSculpting.CreatePalette(paletteRamps, rampWidth, paletteRows, rowHeight);
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

        // Half the arm's thickness at a distance along it from the pivot
        private static float ArmHalfThickness(float along)
        {
            return Mathf.Lerp(armRootThickness, armTipThickness, along / armLength) * 0.5f;
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
