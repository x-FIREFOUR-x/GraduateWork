using System;
using System.Collections.Generic;

using UnityEngine;

using TowerDefense.EditorTools.Baking;
using static TowerDefense.EditorTools.Baking.MeshSculpting;


namespace TowerDefense.EditorTools.Enemies
{
    public static class BatMeshBuilder
    {
        public enum Palette { Fur, DarkFur, Skin, Membrane, Bone, Mouth, Iron, Leather, Ember }

        private static readonly Color32[][] paletteRamps =
        {
            new Color32[] { new(20, 15, 14, 255), new(38, 28, 24, 255), new(60, 44, 36, 255), new(86, 64, 50, 255), new(116, 90, 70, 255), new(150, 124, 98, 255) },
            new Color32[] { new(10, 8, 8, 255), new(22, 17, 16, 255), new(36, 28, 25, 255), new(52, 41, 36, 255), new(70, 56, 48, 255), new(92, 76, 64, 255) },
            new Color32[] { new(34, 27, 27, 255), new(56, 44, 42, 255), new(80, 64, 58, 255), new(106, 86, 76, 255), new(132, 110, 96, 255), new(156, 132, 116, 255) },
            new Color32[] { new(26, 18, 22, 255), new(46, 32, 36, 255), new(72, 50, 50, 255), new(102, 70, 62, 255), new(136, 96, 80, 255), new(168, 124, 102, 255) },
            new Color32[] { new(44, 36, 28, 255), new(90, 78, 60, 255), new(146, 132, 104, 255), new(194, 182, 154, 255), new(228, 220, 198, 255) },
            new Color32[] { new(26, 8, 10, 255), new(56, 18, 22, 255), new(92, 34, 36, 255), new(130, 56, 56, 255) },
            new Color32[] { new(30, 32, 38, 255), new(56, 60, 68, 255), new(92, 96, 104, 255), new(138, 140, 146, 255) },
            new Color32[] { new(40, 26, 18, 255), new(70, 46, 32, 255), new(104, 74, 52, 255), new(138, 104, 76, 255) },
            new Color32[] { new(120, 20, 10, 255), new(190, 50, 20, 255), new(240, 110, 40, 255), new(255, 190, 110, 255) },
        };
        private const int rampWidth = 64;
        private const int rowHeight = 4;
        private const int paletteRows = 9;

        public const float ModelScale = 1.7f;

        public static readonly Vector3 WingShoulder = new(0.155f, 0.08f, 0.13f);
        public static readonly Vector3 WingWrist = new(0.68f, 0.04f, 0.02f);
        public static readonly Vector3 EarBase = new(0.1f, 0.25f, 0.5f);
        public static readonly Vector3 TailBase = new(0f, -0.015f, -0.31f);

        private static readonly Vector3 headCenter = new(0f, 0.12f, 0.55f);
        private static readonly Vector3 headRadii = new(0.17f, 0.16f, 0.2f);
        private static readonly Vector3 eyeSocket = new(0.42f, 0.32f, 0.85f);

        private static readonly Vector3 fifthFingerTip = new(-0.05f, -0.04f, -0.44f);


        public static Texture2D CreatePalette()
        {
            return MeshSculpting.CreatePalette(paletteRamps, rampWidth, paletteRows, rowHeight);
        }

        public static (Mesh body, Mesh glow) BuildBody()
        {
            Builder builder = new(401, shadeNoise: 0.35f, noiseScale: 7f);
            Builder glow = new(409, shadeNoise: 0.15f, noiseScale: 9f);
            System.Random rng = new(403);

            Vector3 torsoRadii = new(0.195f, 0.18f, 0.31f);
            builder.Sculpt(Vector3.zero, torsoRadii,
                           new[] { (new Vector3(0f, 0.75f, 0.55f), 0.55f, 0.22f), (new Vector3(0f, -0.6f, 0.6f), 0.5f, 0.1f),
                                   (Vector3.back, 0.6f, -0.12f), (new Vector3(0.8f, 0.2f, 0.4f), 0.4f, 0.1f), (new Vector3(-0.8f, 0.2f, 0.4f), 0.4f, 0.1f) },
                           Palette.Fur, CounterShade, 3);

            builder.Tube(new[] { new Vector3(0f, 0.05f, 0.22f), new Vector3(0f, 0.09f, 0.36f), headCenter + new Vector3(0f, -0.02f, -0.06f) },
                         new[] { 0.15f, 0.15f, 0.14f }, Palette.Fur, 0.36f, sides: 14, squash: new Vector2(1f, 0.9f), toneEnd: 0.5f);

            FurClumps(builder, rng, 110, Vector3.zero, torsoRadii, direction => direction.z < 0.7f, 0.075f);

            BuildMane(builder, rng);
            BuildHarness(builder);
            BuildHead(builder, glow, rng);

            foreach (float side in new[] { -1f, 1f })
                BuildHindLeg(builder, side, rng);

            return (builder.ToMesh(), glow.ToMesh());
        }

        public static Mesh BuildWingInner(float side)
        {
            Builder builder = new(side > 0 ? 421 : 423, shadeNoise: 0.3f, noiseScale: 6f);
            System.Random rng = new(side > 0 ? 425 : 427);

            Vector3 elbow = new(side * 0.22f, 0.07f, 0.09f);
            Vector3 wrist = Side(WingWrist, side);

            Vector3 shoulder = new(side * 0.04f, 0.01f, 0f);
            Vector3 shoulderRadii = new(0.09f, 0.08f, 0.09f);
            builder.Sculpt(shoulder, shoulderRadii, new[] { (Side(Vector3.right, side), 0.6f, 0.15f) }, Palette.Fur, CounterShade, 2);
            FurClumps(builder, rng, 12, shoulder, shoulderRadii, direction => direction.y > -0.3f, 0.07f);

            builder.Tube(new[] { Vector3.zero, elbow, Vector3.Lerp(elbow, wrist, 0.5f) + Vector3.up * 0.015f, wrist },
                         new[] { 0.07f, 0.05f, 0.04f, 0.034f }, Palette.Skin, 0.32f, sides: 10, toneEnd: 0.5f);
            builder.Ball(elbow, Vector3.one * 0.052f, Palette.Skin, 0.38f);
            builder.Ball(wrist, Vector3.one * 0.042f, Palette.Skin, 0.48f);

            Vector3 thumb = wrist + new Vector3(side * 0.02f, 0.03f, 0.08f);
            builder.Tube(new[] { wrist, thumb, thumb + new Vector3(side * 0.01f, -0.03f, 0.03f) }, new[] { 0.018f, 0.012f, 0.002f }, Palette.Bone, 0.75f, sides: 6,
                         toneEnd: 0.1f);

            Vector3 flank = new(side * -0.055f, -0.06f, -0.39f);
            Vector3 trailingEnd = wrist + Side(fifthFingerTip, side);

            Vector3 Leading(float u) => u < 0.35f ? Vector3.Lerp(Vector3.zero, elbow, u / 0.35f) : Vector3.Lerp(elbow, wrist, (u - 0.35f) / 0.65f);
            Vector3 Trailing(float u) => Vector3.Lerp(flank, trailingEnd, u) + Vector3.forward * (0.12f * Mathf.Sin(Mathf.PI * u));

            HashSet<(int, int)> holes = side > 0 ? new() { (7, 7) } : new() { (4, 7), (5, 7) };
            Vector3[,] grid = Membrane(builder, Leading, Trailing, 12, 8, 0.07f, (u, v) => Mathf.Sin(Mathf.PI * v) * Mathf.Sin(Mathf.PI * u), 0f, Palette.Membrane,
                                       (u, v) => 0.3f + 0.3f * Mathf.Sin(Mathf.PI * v) * Mathf.Sin(Mathf.PI * Mathf.Min(u * 1.3f, 1f)) - 0.12f * v * v, holes);

            Vein(builder, grid, 3, 0, 11, 3);
            Vein(builder, grid, 6, 0, 11, 5);
            Vein(builder, grid, 5, 2, 12, 7);

            return builder.ToMesh();
        }

        public static Mesh BuildWingOuter(float side)
        {
            Builder builder = new(side > 0 ? 431 : 433, shadeNoise: 0.3f, noiseScale: 6f);

            Vector3[] tips =
            {
                Side(new Vector3(0.76f, 0.03f, 0.1f), side),
                Side(new Vector3(0.82f, -0.01f, -0.16f), side),
                Side(new Vector3(0.6f, -0.03f, -0.4f), side),
                Side(fifthFingerTip, side),
            };

            Vector3 Joint(Vector3 tip) => tip * 0.48f + Vector3.up * 0.03f;

            for (int i = 0; i < tips.Length; i++)
            {
                Vector3 joint = Joint(tips[i]);
                builder.Tube(new[] { Vector3.zero, joint }, new[] { 0.026f, 0.019f }, Palette.Skin, 0.45f, sides: 7, toneEnd: 0.35f);
                builder.Tube(new[] { joint, tips[i] }, new[] { 0.019f, 0.007f }, Palette.Skin, 0.35f, sides: 7, toneEnd: 0.2f);
                builder.Ball(joint, Vector3.one * 0.023f, Palette.Skin, 0.5f);
            }
            builder.Ball(Vector3.zero, Vector3.one * 0.04f, Palette.Skin, 0.45f);

            Vector3 claw = tips[0];
            builder.Tube(new[] { claw, claw + Side(new Vector3(0.05f, 0f, 0.02f), side), claw + Side(new Vector3(0.07f, -0.03f, -0.01f), side) },
                         new[] { 0.01f, 0.007f, 0.001f }, Palette.Bone, 0.7f, sides: 5, toneEnd: 0.1f);

            HashSet<(int, int)>[] holes =
            {
                new(),
                side > 0 ? new() { (9, 2), (9, 3) } : new() { (8, 1) },
                side > 0 ? new() : new() { (9, 3) },
            };

            for (int i = 0; i < tips.Length - 1; i++)
            {
                Vector3 from = tips[i];
                Vector3 to = tips[i + 1];
                Vector3 FingerA(float u) => FingerPoint(from, u);
                Vector3 FingerB(float u) => FingerPoint(to, u);

                float panel = 0.04f * i;
                Vector3[,] grid = Membrane(builder, FingerA, FingerB, 10, 6, 0.05f, (u, v) => Mathf.Sin(Mathf.PI * v) * u, 0.16f, Palette.Membrane,
                                           (u, v) => 0.28f + panel + 0.32f * Mathf.Sin(Mathf.PI * v) * Mathf.Sin(Mathf.PI * Mathf.Min(u * 1.2f, 1f)) - 0.1f * u * u, holes[i]);

                Vein(builder, grid, 3, 1, 8, 3);
            }

            return builder.ToMesh();

            Vector3 FingerPoint(Vector3 tip, float u)
            {
                Vector3 joint = Joint(tip);
                return u < 0.5f ? Vector3.Lerp(Vector3.zero, joint, u * 2f) : Vector3.Lerp(joint, tip, u * 2f - 1f);
            }
        }

        public static Mesh BuildEar(float side)
        {
            Builder builder = new(side > 0 ? 441 : 443, shadeNoise: 0.3f, noiseScale: 9f);
            System.Random rng = new(side > 0 ? 445 : 447);

            Vector3[] spine = { Vector3.zero, new(side * 0.05f, 0.16f, -0.02f), new(side * 0.09f, 0.32f, -0.07f) };
            builder.Tube(spine, new[] { 0.095f, 0.075f, 0.006f }, Palette.Fur, 0.2f, sides: 12, squash: new Vector2(1f, 0.3f), toneEnd: 0.58f);

            Vector3[] inner = new Vector3[spine.Length];
            for (int i = 0; i < spine.Length; i++)
                inner[i] = spine[i] + new Vector3(0f, 0.02f, 0.02f);
            builder.Tube(inner, new[] { 0.068f, 0.055f, 0.004f }, Palette.Skin, 0.62f, sides: 12, squash: new Vector2(1f, 0.2f), toneEnd: 0.2f);

            for (int ridge = 0; ridge < 4; ridge++)
            {
                float t = 0.15f + ridge * 0.17f;
                Vector3 center = Vector3.Lerp(spine[0], spine[2], t) + new Vector3(0f, 0.02f, 0.032f);
                float width = Mathf.Lerp(0.04f, 0.012f, t);
                builder.Tube(new[] { center + new Vector3(-width, -0.008f, -0.004f), center, center + new Vector3(width, -0.008f, -0.004f) },
                             new[] { 0.004f, 0.005f, 0.004f }, Palette.Skin, 0.4f - 0.05f * ridge, sides: 5, toneEnd: 0.22f);
            }

            builder.Tube(new[] { new Vector3(-side * 0.03f, 0.01f, 0.035f), new Vector3(-side * 0.025f, 0.07f, 0.045f), new Vector3(-side * 0.015f, 0.1f, 0.04f) },
                         new[] { 0.016f, 0.012f, 0.003f }, Palette.Skin, 0.5f, sides: 6, squash: new Vector2(1f, 0.5f), toneEnd: 0.28f);

            for (int i = 0; i < 6; i++)
            {
                float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                Vector3 root = new(Mathf.Cos(angle) * 0.06f, 0.01f, Mathf.Sin(angle) * 0.02f - 0.01f);
                Vector3 tip = root + new Vector3(Mathf.Cos(angle) * 0.03f, 0.05f + (float)rng.NextDouble() * 0.03f, -0.03f);
                builder.Tube(new[] { root, tip }, new[] { 0.014f, 0.002f }, Palette.Fur, 0.3f, sides: 5, toneEnd: 0.75f);
            }

            return builder.ToMesh();
        }

        public static Mesh BuildTail()
        {
            Builder builder = new(451, shadeNoise: 0.3f, noiseScale: 8f);

            Vector3 tip = new(0f, -0.03f, -0.42f);
            builder.Tube(new[] { Vector3.zero, new Vector3(0f, -0.04f, -0.2f), tip }, new[] { 0.045f, 0.026f, 0.008f }, Palette.Skin, 0.25f, sides: 8, toneEnd: 0.35f);

            System.Random rng = new(457);
            for (int i = 0; i < 9; i++)
            {
                Vector3 root = Vector3.Lerp(new Vector3(0f, -0.03f, -0.04f), tip, i / 9f);
                Vector3 end = root + new Vector3((float)(rng.NextDouble() - 0.5) * 0.08f, 0.02f + (float)rng.NextDouble() * 0.03f, -0.07f);
                builder.Tube(new[] { root, end }, new[] { 0.016f - i * 0.001f, 0.002f }, Palette.Fur, 0.25f, sides: 5, toneEnd: 0.7f);
            }

            return builder.ToMesh();
        }

        public static Mesh BuildSwarmBat()
        {
            Builder builder = new(461, shadeNoise: 0.3f, noiseScale: 12f);

            builder.Ball(Vector3.zero, new Vector3(0.12f, 0.1f, 0.2f), Palette.Fur, 0.4f);
            builder.Ball(new Vector3(0f, 0.04f, 0.18f), Vector3.one * 0.08f, Palette.Fur, 0.45f);
            foreach (float side in new[] { -1f, 1f })
            {
                builder.Tube(new[] { new Vector3(side * 0.04f, 0.09f, 0.2f), new Vector3(side * 0.06f, 0.2f, 0.17f) }, new[] { 0.035f, 0.002f }, Palette.Fur, 0.3f, sides: 5, toneEnd: 0.7f);

                Vector3 shoulder = new(side * 0.08f, 0.03f, 0.04f);
                Vector3 wrist = new(side * 0.3f, 0.12f, 0.06f);
                Vector3 tip = new(side * 0.55f, 0.02f, -0.06f);
                Vector3 back = new(side * 0.32f, 0f, -0.2f);
                Vector3 root = new(side * 0.06f, -0.01f, -0.14f);
                MembraneTriangle(builder, shoulder, wrist, root, Palette.Membrane, 0.45f);
                MembraneTriangle(builder, wrist, back, root, Palette.Membrane, 0.4f);
                MembraneTriangle(builder, wrist, tip, back, Palette.Membrane, 0.5f);
            }

            return builder.ToMesh();
        }


        private static void BuildHead(Builder builder, Builder glow, System.Random rng)
        {
            Vector3 snout = new(0f, -0.24f, 0.97f);
            List<(Vector3 direction, float width, float height)> features = new()
            {
                (snout, 0.42f, 0.55f),
                (new Vector3(0f, 0.55f, -0.5f), 0.6f, 0.12f),
                (new Vector3(0f, -0.7f, 0.3f), 0.45f, 0.1f),
                (new Vector3(0f, 0.95f, 0.2f), 0.16f, 0.1f),
            };
            List<Vector3> creases = new();

            foreach (float side in new[] { -1f, 1f })
            {
                features.Add((Side(new Vector3(0.36f, 0.55f, 0.76f), side), 0.18f, 0.24f));
                features.Add((Side(eyeSocket, side), 0.15f, -0.12f));
                features.Add((Side(new Vector3(0.72f, -0.05f, 0.55f), side), 0.26f, 0.16f));
                features.Add((Side(new Vector3(0.55f, -0.55f, 0.6f), side), 0.25f, 0.1f));
                features.Add((Side(new Vector3(0.85f, 0.4f, 0f), side), 0.3f, -0.06f));
                features.Add((Side(new Vector3(0.3f, 0.05f, 0.95f), side), 0.12f, 0.06f));

                for (int i = 0; i < 4; i++)
                {
                    Vector3 ridge = Side(Vector3.Lerp(new Vector3(0.14f, 0.12f, 0.98f), new Vector3(0.34f, -0.12f, 0.93f), i / 3f), side);
                    features.Add((ridge, 0.035f, 0.035f));
                    Vector3 crease = Side(Vector3.Lerp(new Vector3(0.18f, 0.09f, 0.97f), new Vector3(0.38f, -0.15f, 0.91f), i / 3f), side);
                    features.Add((crease, 0.025f, -0.02f));
                    creases.Add(crease);
                }
            }

            for (int row = 0; row < 4; row++)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector3 dent = new(Mathf.Lerp(-0.16f, 0.16f, i / 4f), 0.2f + row * 0.07f - Mathf.Abs(i - 2f) * 0.015f, 0.97f - row * 0.04f);
                    features.Add((dent, 0.04f, -0.018f));
                    creases.Add(dent);
                }
            }

            builder.Sculpt(headCenter, headRadii, features.ToArray(), Palette.Fur, direction =>
            {
                float tone = 0.4f
                    - 0.22f * (Falloff(direction, eyeSocket, 0.22f) + Falloff(direction, Mirror(eyeSocket), 0.22f))
                    + 0.14f * Falloff(direction, snout, 0.35f)
                    - 0.2f * direction.y
                    + 0.22f * Mathf.Clamp01(-direction.y)
                    - 0.1f * Mathf.Clamp01(-direction.z);
                foreach (Vector3 crease in creases)
                    tone -= 0.14f * Falloff(direction, crease, 0.035f);
                return tone;
            }, 4);

            FurClumps(builder, rng, 48, headCenter, headRadii, direction => direction.z < 0.35f && direction.y > -0.4f, 0.06f, 0.022f);

            BuildNose(builder);
            BuildMouth(builder);
            BuildFaceFur(builder, rng);

            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 socket = Side(eyeSocket, side);
                Vector3 eye = HeadPoint(socket, -0.1f);
                Vector3 look = (socket.normalized + Vector3.forward * 2f).normalized;
                builder.Ball(eye, Vector3.one * 0.026f, Palette.DarkFur, 0.05f);
                glow.Ball(eye + look * 0.022f, new Vector3(0.024f, 0.02f, 0.012f), Palette.Ember, 0.7f);
                builder.Ball(eye + look * 0.033f, new Vector3(0.0045f, 0.015f, 0.003f), Palette.Mouth, 0.02f);

                Vector3 inner = new(-side * 0.03f, 0.008f, 0.014f);
                Vector3 outer = new(side * 0.032f, 0.03f, 0.002f);
                builder.Tube(new[] { eye + inner, eye + new Vector3(0f, 0.028f, 0.02f), eye + outer }, new[] { 0.011f, 0.014f, 0.008f }, Palette.Skin, 0.12f, sides: 6,
                             toneEnd: 0.25f);
                builder.Tube(new[] { eye + new Vector3(-side * 0.026f, -0.018f, 0.012f), eye + new Vector3(0f, -0.026f, 0.016f), eye + new Vector3(side * 0.028f, -0.016f, 0.004f) },
                             new[] { 0.005f, 0.007f, 0.004f }, Palette.Skin, 0.2f, sides: 5, toneEnd: 0.34f);
            }

            Vector3[] scar = { HeadPoint(new Vector3(-0.22f, 0.72f, 0.66f), 0.04f), HeadPoint(new Vector3(-0.42f, 0.36f, 0.83f), 0.03f),
                               HeadPoint(new Vector3(-0.56f, 0.02f, 0.83f), 0.04f), HeadPoint(new Vector3(-0.6f, -0.22f, 0.77f), 0.05f) };
            builder.Tube(scar, new[] { 0.004f, 0.008f, 0.007f, 0.003f }, Palette.Skin, 0.62f, sides: 6, squash: new Vector2(1f, 0.6f), toneEnd: 0.5f);
        }

        private static void BuildNose(Builder builder)
        {
            Vector3 nose = new(0f, 0.08f, 0.865f);
            builder.Sculpt(nose, new Vector3(0.042f, 0.03f, 0.026f), new[] { (Vector3.up, 0.5f, 0.2f), (Vector3.forward, 0.4f, 0.15f) }, Palette.Skin,
                           direction => 0.08f + 0.12f * direction.y, 3);
            foreach (float side in new[] { -1f, 1f })
            {
                builder.Ball(nose + new Vector3(side * 0.018f, -0.004f, 0.024f), new Vector3(0.009f, 0.007f, 0.006f), Palette.Mouth, 0.02f);
                builder.Tube(new[] { nose + new Vector3(side * 0.01f, 0.012f, 0.022f), nose + new Vector3(side * 0.03f, 0.004f, 0.018f), nose + new Vector3(side * 0.034f, -0.014f, 0.01f) },
                             new[] { 0.004f, 0.006f, 0.003f }, Palette.Skin, 0.32f, sides: 5, toneEnd: 0.12f);
            }

            SymmetricTube(builder, new[] { nose + new Vector3(-0.038f, -0.004f, 0.002f), nose + new Vector3(-0.026f, -0.022f, 0.016f), nose + new Vector3(0f, -0.028f, 0.022f),
                                           nose + new Vector3(0.026f, -0.022f, 0.016f), nose + new Vector3(0.038f, -0.004f, 0.002f) },
                          new[] { 0.005f, 0.008f, 0.009f, 0.008f, 0.005f }, Palette.Skin, 0.26f, 0.08f, squash: new Vector2(1f, 0.6f));

            Vector3 lancet = nose + new Vector3(0f, 0.06f, -0.008f);
            builder.Sculpt(lancet, new Vector3(0.03f, 0.05f, 0.008f), new[] { (Vector3.up, 0.35f, 0.7f), (Vector3.down, 0.6f, -0.2f) }, Palette.Skin,
                           direction => 0.12f + 0.22f * Mathf.Clamp01(direction.y) + 0.08f * Mathf.Abs(direction.x), 3);
            builder.Tube(new[] { nose + new Vector3(0f, 0.02f, 0f), lancet + new Vector3(0f, 0.01f, 0.006f), lancet + new Vector3(0f, 0.07f, 0.002f) },
                         new[] { 0.005f, 0.004f, 0.001f }, Palette.Skin, 0.3f, sides: 5, toneEnd: 0.15f);
        }

        private static void BuildMouth(Builder builder)
        {
            builder.Ball(new Vector3(0f, -0.01f, 0.74f), new Vector3(0.062f, 0.04f, 0.1f), Palette.Mouth, 0.12f);
            builder.Ball(new Vector3(0f, -0.028f, 0.745f), new Vector3(0.03f, 0.01f, 0.06f), Palette.Mouth, 0.65f);

            Vector3[] upperGum = { new(-0.062f, 0.012f, 0.66f), new(-0.058f, 0.018f, 0.73f), new(-0.036f, 0.026f, 0.8f), new(0f, 0.03f, 0.822f),
                                   new(0.036f, 0.026f, 0.8f), new(0.058f, 0.018f, 0.73f), new(0.062f, 0.012f, 0.66f) };
            SymmetricTube(builder, upperGum, new[] { 0.01f, 0.013f, 0.014f, 0.014f, 0.014f, 0.013f, 0.01f }, Palette.Mouth, 0.65f, 0.3f);
            SymmetricTube(builder, Shift(upperGum, new Vector3(0f, 0.014f, 0.006f), 1.08f), new[] { 0.008f, 0.011f, 0.012f, 0.012f, 0.012f, 0.011f, 0.008f }, Palette.Skin,
                          0.24f, 0.06f, wrinkle: 0.15f);

            builder.Sculpt(new Vector3(0f, -0.068f, 0.7f), new Vector3(0.074f, 0.03f, 0.13f), new[] { (Vector3.forward, 0.5f, 0.12f), (new Vector3(0f, -1f, 0.8f), 0.4f, 0.25f) },
                           Palette.Fur, direction => 0.42f + 0.18f * Mathf.Clamp01(-direction.y), 3);
            Vector3[] lowerGum = { new(-0.056f, -0.045f, 0.65f), new(-0.052f, -0.04f, 0.72f), new(-0.032f, -0.036f, 0.78f), new(0f, -0.034f, 0.8f),
                                   new(0.032f, -0.036f, 0.78f), new(0.052f, -0.04f, 0.72f), new(0.056f, -0.045f, 0.65f) };
            SymmetricTube(builder, lowerGum, new[] { 0.008f, 0.011f, 0.012f, 0.012f, 0.012f, 0.011f, 0.008f }, Palette.Mouth, 0.6f, 0.25f);
            SymmetricTube(builder, Shift(lowerGum, new Vector3(0f, -0.006f, 0.002f), 1.02f), new[] { 0.006f, 0.008f, 0.009f, 0.009f, 0.009f, 0.008f, 0.006f }, Palette.Fur,
                          0.34f, 0.16f);

            foreach (float side in new[] { -1f, 1f })
            {
                builder.Tube(new[] { new Vector3(side * 0.046f, 0.03f, 0.8f), new Vector3(side * 0.046f, -0.02f, 0.815f), new Vector3(side * 0.042f, -0.07f, 0.81f) },
                             new[] { 0.016f, 0.011f, 0.002f }, Palette.Bone, 0.55f, sides: 7, toneEnd: 0.95f);
                builder.Tube(new[] { new Vector3(side * 0.04f, -0.036f, 0.775f), new Vector3(side * 0.038f, -0.002f, 0.785f), new Vector3(side * 0.034f, 0.022f, 0.79f) },
                             new[] { 0.012f, 0.008f, 0.0015f }, Palette.Bone, 0.5f, sides: 6, toneEnd: 0.9f);

                for (int tooth = 0; tooth < 4; tooth++)
                {
                    float z = 0.765f - tooth * 0.03f;
                    Vector3 root = new(side * (0.057f + tooth * 0.001f), 0.012f, z);
                    builder.Tube(new[] { root, root + new Vector3(side * -0.003f, -0.018f - (tooth % 2) * 0.008f, 0.002f) }, new[] { 0.006f, 0.001f }, Palette.Bone, 0.5f,
                                 sides: 5, toneEnd: 0.85f);
                    Vector3 lower = new(side * (0.051f + tooth * 0.001f), -0.038f, z - 0.01f);
                    builder.Tube(new[] { lower, lower + new Vector3(side * -0.002f, 0.014f + (tooth % 2) * 0.006f, 0f) }, new[] { 0.005f, 0.001f }, Palette.Bone, 0.5f,
                                 sides: 5, toneEnd: 0.85f);
                }

                for (int tooth = 0; tooth < 2; tooth++)
                {
                    float x = side * (0.01f + tooth * 0.014f);
                    builder.Tube(new[] { new Vector3(x, 0.024f, 0.826f - tooth * 0.004f), new Vector3(x, 0.006f, 0.83f - tooth * 0.004f) }, new[] { 0.005f, 0.002f },
                                 Palette.Bone, 0.6f, sides: 5, toneEnd: 0.85f);
                    builder.Tube(new[] { new Vector3(x * 0.9f, -0.03f, 0.803f), new Vector3(x * 0.9f, -0.016f, 0.806f) }, new[] { 0.004f, 0.0015f },
                                 Palette.Bone, 0.55f, sides: 5, toneEnd: 0.8f);
                }
            }
        }

        private static void BuildFaceFur(Builder builder, System.Random rng)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                for (int i = 0; i < 12; i++)
                {
                    float t = i / 11f;
                    Vector3 direction = Side(new Vector3(Mathf.Lerp(0.6f, 0.9f, t), Mathf.Lerp(0.25f, -0.45f, t), Mathf.Lerp(0.55f, 0.15f, t)), side);
                    Vector3 root = HeadPoint(direction, -0.05f);
                    float length = 0.07f + (float)rng.NextDouble() * 0.05f + Mathf.Sin(Mathf.PI * t) * 0.04f;
                    Vector3 tip = root + new Vector3(side * length * 0.45f, -length * 0.35f, -length * 0.8f);
                    Vector3 middle = Vector3.Lerp(root, tip, 0.5f) + new Vector3(side * 0.012f, 0.008f, 0f);
                    builder.Tube(new[] { root, middle, tip }, new[] { 0.018f, 0.013f, 0.002f }, Palette.Fur, 0.32f + (float)rng.NextDouble() * 0.08f, sides: 6,
                                 squash: new Vector2(1f, 0.5f), toneEnd: 0.78f);
                }

                for (int i = 0; i < 5; i++)
                {
                    Vector3 root = HeadPoint(Side(new Vector3(0.26f + i * 0.06f, 0.62f - i * 0.02f, 0.74f - i * 0.05f), side), 0.12f);
                    Vector3 tip = root + new Vector3(side * (0.025f + i * 0.008f), 0.04f + i * 0.004f, -0.035f);
                    builder.Tube(new[] { root, Vector3.Lerp(root, tip, 0.5f) + Vector3.up * 0.006f, tip }, new[] { 0.011f, 0.008f, 0.0015f }, Palette.Fur, 0.38f,
                                 sides: 5, toneEnd: 0.85f);
                }

                for (int i = 0; i < 3; i++)
                {
                    Vector3 root = new(side * 0.05f, 0.05f - i * 0.014f, 0.8f - i * 0.022f);
                    builder.Tube(new[] { root, root + new Vector3(side * 0.08f, -0.012f + i * 0.012f, -0.012f) }, new[] { 0.003f, 0.0008f }, Palette.DarkFur, 0.25f, sides: 4, toneEnd: 0.65f);
                }
            }

            for (int i = 0; i < 10; i++)
            {
                float t = i / 9f;
                Vector3 root = new(Mathf.Lerp(-0.07f, 0.07f, t), -0.085f - Mathf.Sin(Mathf.PI * t) * 0.01f, 0.6f + (i % 2) * 0.03f);
                Vector3 tip = root + new Vector3((t - 0.5f) * 0.05f, -0.06f - (float)rng.NextDouble() * 0.03f, -0.08f);
                builder.Tube(new[] { root, Vector3.Lerp(root, tip, 0.5f) + Vector3.back * 0.01f, tip }, new[] { 0.016f, 0.011f, 0.002f }, Palette.Fur, 0.45f, sides: 6,
                             squash: new Vector2(1f, 0.5f), toneEnd: 0.82f);
            }
        }

        private static void SymmetricTube(Builder builder, Vector3[] points, float[] radii, Palette palette, float toneCenter, float toneEnds, Vector2? squash = null,
                                          float wrinkle = 0f)
        {
            int middle = points.Length / 2;
            Vector3[] left = new Vector3[middle + 1];
            float[] leftRadii = new float[middle + 1];
            for (int i = 0; i <= middle; i++)
            {
                left[i] = points[middle - i];
                leftRadii[i] = radii[middle - i];
            }

            builder.Tube(left, leftRadii, palette, toneCenter, sides: 6, squash: squash, wrinkle: wrinkle, toneEnd: toneEnds);
            builder.Tube(points[middle..], radii[middle..], palette, toneCenter, sides: 6, squash: squash, wrinkle: wrinkle, toneEnd: toneEnds);
        }

        private static Vector3 HeadPoint(Vector3 direction, float lift)
        {
            return headCenter + Vector3.Scale(direction.normalized * (1f + lift), headRadii);
        }

        private static Vector3[] Shift(Vector3[] points, Vector3 offset, float spread)
        {
            Vector3[] shifted = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
                shifted[i] = new Vector3(points[i].x * spread, points[i].y, points[i].z) + offset;
            return shifted;
        }

        private static void BuildMane(Builder builder, System.Random rng)
        {
            builder.Torus(new Vector3(0f, 0.1f, 0.36f), new Vector3(0f, 0.3f, 1f), new Vector2(0.17f, 0.155f), 0.065f, Palette.DarkFur, 0.45f,
                          segments: 24, sides: 8, waviness: 0.02f, heightShade: 0.25f);

            const int tufts = 26;
            for (int i = 0; i < tufts; i++)
            {
                float angle = Mathf.Lerp(-110f, 110f, i / (tufts - 1f)) * Mathf.Deg2Rad + (float)(rng.NextDouble() - 0.5) * 0.1f;
                Vector3 around = new(Mathf.Sin(angle), Mathf.Cos(angle), 0f);
                Vector3 root = new Vector3(0f, 0.1f, 0.34f - (i % 2) * 0.04f) + around * 0.17f;
                float length = 0.12f + (float)rng.NextDouble() * 0.1f;
                Vector3 tip = root + around * length * 0.45f + Vector3.back * length;
                builder.Tube(new[] { root, Vector3.Lerp(root, tip, 0.5f) + around * 0.02f, tip }, new[] { 0.05f, 0.035f, 0.004f },
                             Palette.DarkFur, 0.3f + (float)rng.NextDouble() * 0.1f, sides: 6, squash: new Vector2(1f, 0.5f), toneEnd: 0.75f);
            }

            for (int i = 0; i < 14; i++)
            {
                float t = i / 13f;
                Vector3 root = new(Mathf.Lerp(-0.06f, 0.06f, (i % 3) / 2f), 0.17f - t * 0.05f, Mathf.Lerp(0.18f, -0.25f, t));
                Vector3 tip = root + new Vector3((float)(rng.NextDouble() - 0.5) * 0.04f, 0.035f, -0.12f);
                builder.Tube(new[] { root, Vector3.Lerp(root, tip, 0.5f) + Vector3.up * 0.012f, tip }, new[] { 0.045f, 0.03f, 0.004f }, Palette.DarkFur, 0.3f, sides: 6,
                             squash: new Vector2(1f, 0.5f), toneEnd: 0.7f);
            }
        }

        private static void BuildHarness(Builder builder)
        {
            Vector3 strap = new(0f, 0.01f, 0.06f);
            builder.Torus(strap, Vector3.forward, new Vector2(0.205f, 0.195f), 0.024f, Palette.Leather, 0.45f, segments: 28, sides: 6, waviness: 0.006f, heightShade: 0.25f);
            for (int i = 0; i < 8; i++)
            {
                float angle = (i + 0.5f) * Mathf.PI * 2f / 8f;
                Vector3 around = new(Mathf.Cos(angle) * 0.226f, Mathf.Sin(angle) * 0.216f, 0f);
                builder.Ball(strap + around, Vector3.one * 0.014f, Palette.Iron, 0.65f);
            }
            builder.Sculpt(strap + new Vector3(0f, 0.215f, 0f), new Vector3(0.06f, 0.025f, 0.05f), new[] { (Vector3.up, 0.5f, 0.2f) }, Palette.Iron, direction => 0.38f + 0.28f * direction.y + 0.08f * direction.z, 2);

            Vector3 collar = new(0f, 0.11f, 0.44f);
            Vector3 collarAxis = new Vector3(0f, 0.28f, 1f).normalized;
            builder.Torus(collar, collarAxis, new Vector2(0.155f, 0.145f), 0.028f, Palette.Iron, 0.48f, segments: 24, sides: 6, heightShade: 0.3f);

            Vector3 across = Vector3.Cross(collarAxis, Vector3.right).normalized;
            for (int i = 0; i < 7; i++)
            {
                float angle = Mathf.Lerp(-80f, 80f, i / 6f) * Mathf.Deg2Rad;
                Vector3 outward = (Vector3.right * Mathf.Sin(angle) + across * Mathf.Cos(angle)).normalized;
                Vector3 root = collar + outward * 0.165f;
                builder.Tube(new[] { root, root + outward * 0.09f }, new[] { 0.02f, 0.002f }, Palette.Iron, 0.45f, sides: 5, roundEnd: false, toneEnd: 0.8f);
            }
        }

        private static void BuildHindLeg(Builder builder, float side, System.Random rng)
        {
            Vector3 hip = new(side * 0.1f, -0.08f, -0.19f);
            Vector3 knee = new(side * 0.17f, -0.17f, -0.35f);
            Vector3 foot = new(side * 0.13f, -0.21f, -0.5f);

            Vector3 thigh = new(0.075f, 0.065f, 0.09f);
            builder.Sculpt(hip, thigh, new[] { (Vector3.back, 0.6f, 0.15f) }, Palette.Fur, CounterShade, 2);
            FurClumps(builder, rng, 10, hip, thigh, direction => direction.y > -0.5f, 0.06f);
            builder.Tube(new[] { hip, knee, foot }, new[] { 0.06f, 0.035f, 0.024f }, Palette.Skin, 0.3f, sides: 8, toneEnd: 0.45f);

            for (int toe = -1; toe <= 1; toe++)
            {
                Vector3 spread = new(side * 0.02f * toe, 0f, 0f);
                Vector3 bend = foot + spread + new Vector3(side * 0.01f * toe, -0.04f, -0.04f);
                Vector3 tip = bend + new Vector3(0f, 0.01f, 0.035f);
                builder.Tube(new[] { foot + spread * 0.5f, bend, tip }, new[] { 0.012f, 0.008f, 0.001f }, Palette.Bone, 0.7f, sides: 5, toneEnd: 0.1f);
            }
        }

        private static float CounterShade(Vector3 direction)
        {
            return 0.42f - 0.16f * direction.y + 0.06f * Mathf.Max(0f, direction.z);
        }

        private static void FurClumps(Builder builder, System.Random rng, int count, Vector3 center, Vector3 radii, Func<Vector3, bool> where, float length,
                                      float thickness = 0.04f)
        {
            int placed = 0;
            for (int attempt = 0; attempt < count * 8 && placed < count; attempt++)
            {
                Vector3 direction = new((float)rng.NextDouble() * 2f - 1f, (float)rng.NextDouble() * 2f - 1f, (float)rng.NextDouble() * 2f - 1f);
                if (direction.sqrMagnitude > 1f || direction.sqrMagnitude < 0.01f)
                    continue;
                direction.Normalize();
                if (!where(direction))
                    continue;

                Vector3 normal = new Vector3(direction.x / radii.x, direction.y / radii.y, direction.z / radii.z).normalized;
                Vector3 root = center + Vector3.Scale(direction, radii) * 0.97f;
                float size = (0.6f + 0.6f * (float)rng.NextDouble()) * (direction.y < -0.3f ? 0.55f : 1f);
                float clump = length * size;
                Vector3 lay = (normal * 0.22f + Vector3.back * 0.95f + Vector3.down * 0.08f).normalized;
                Vector3 middle = root + lay * clump * 0.5f + normal * clump * 0.12f;
                Vector3 tip = root + lay * clump + normal * clump * 0.05f;

                float tone = CounterShade(direction) - 0.1f + (float)rng.NextDouble() * 0.08f;
                builder.Tube(new[] { root, middle, tip }, new[] { thickness * size, thickness * 0.75f * size, 0.004f }, Palette.Fur, tone, sides: 6, squash: new Vector2(1f, 0.45f),
                             toneEnd: Mathf.Min(tone + 0.22f, 0.8f));
                placed++;
            }
        }

        private static Vector3[,] Membrane(Builder builder, Func<float, Vector3> edgeA, Func<float, Vector3> edgeB, int columns, int rows, float sag,
                                           Func<float, float, float> sagWeight, float scallop, Palette palette, Func<float, float, float> toneAt,
                                           HashSet<(int, int)> holes)
        {
            Vector3[,] grid = new Vector3[columns + 1, rows + 1];
            for (int c = 0; c <= columns; c++)
            {
                float u = c / (float)columns;
                Vector3 a = edgeA(u);
                Vector3 b = edgeB(u);
                for (int r = 0; r <= rows; r++)
                {
                    float v = r / (float)rows;
                    Vector3 point = Vector3.Lerp(a, b, v);
                    float bow = Mathf.Sin(Mathf.PI * v);
                    point += Vector3.down * (sag * sagWeight(u, v));
                    point -= point.normalized * (scallop * bow * Mathf.Pow(u, 4f) * point.magnitude * 0.6f);
                    grid[c, r] = point;
                }
            }

            for (int c = 0; c < columns; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    if (holes.Contains((c, r)))
                        continue;

                    float tone = toneAt((c + 0.5f) / columns, (r + 0.5f) / rows);
                    MembraneTriangle(builder, grid[c, r], grid[c + 1, r], grid[c + 1, r + 1], palette, tone);
                    MembraneTriangle(builder, grid[c, r], grid[c + 1, r + 1], grid[c, r + 1], palette, tone + 0.015f);
                }
            }

            return grid;
        }

        private static void Vein(Builder builder, Vector3[,] grid, int startRow, int fromColumn, int toColumn, int endRow)
        {
            List<Vector3> points = new();
            for (int c = fromColumn; c <= toColumn; c++)
            {
                float row = Mathf.Lerp(startRow, endRow, (c - fromColumn) / (float)Mathf.Max(toColumn - fromColumn, 1));
                int below = Mathf.FloorToInt(row);
                int above = Mathf.Min(below + 1, grid.GetLength(1) - 1);
                points.Add(Vector3.Lerp(grid[c, below], grid[c, above], row - below));
            }

            float[] radii = new float[points.Count];
            for (int i = 0; i < radii.Length; i++)
                radii[i] = Mathf.Lerp(0.0065f, 0.0025f, i / Mathf.Max(radii.Length - 1f, 1f));

            builder.Tube(points.ToArray(), radii, Palette.Skin, 0.4f, sides: 5, toneEnd: 0.28f);
        }

        private static void MembraneTriangle(Builder builder, Vector3 a, Vector3 b, Vector3 c, Palette palette, float tone)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a);
            if (normal.sqrMagnitude < 1e-10f)
                return;

            if (normal.y < 0f)
                (b, c) = (c, b);

            builder.Flag(a, b, c, palette, tone);
        }

        private static Vector3 Side(Vector3 point, float side)
        {
            return new Vector3(point.x * side, point.y, point.z);
        }


        private class Builder : MeshSculptor<Palette>
        {
            public Builder(int seed, float shadeNoise = 0.12f, float noiseScale = 6f) : base(seed, rampWidth, paletteRows, shadeNoise, noiseScale)
            {
            }
        }
    }

}
