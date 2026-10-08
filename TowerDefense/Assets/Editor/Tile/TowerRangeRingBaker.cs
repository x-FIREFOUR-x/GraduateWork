using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEngine;

using TowerDefense.Main.Map.Tile;


namespace TowerDefense.EditorTools.Tile
{
    // Bakes the ring that shows a tower's range over the tile it would be built on: a golden circle with an inscription
    // in letters after the tengwar of Middle-earth, drawn into a sprite and put on the clickable tower tile base prefab
    // with a slow turn. The outer circle is the range itself: TowerRangeRing scales the ring so it lies on the range
    public static class TowerRangeRingBaker
    {
        private const string spritePath = "Assets/Sprites/TowerRangeRing.png";
        private const string basePrefabPath = "Assets/Prefabs/Map/Tile/ClickableTowerTile/ClickableTowerTileBase.prefab";
        private const string ringName = "TowerRangeRing";

        private const int size = 1024;
        // Twice the pixels of the old 512 pixel ring at 100 per unit, so the sprite is as large in the world
        private const float pixelsPerUnit = 200f;

        // Radii and widths as parts of half the sprite. The outer circle is the range itself
        private const float outerRadius = TowerRangeRing.OuterRadius;
        private const float outerHalfWidth = 0.0065f;
        private const float innerRadius = 0.75f;
        private const float innerHalfWidth = 0.0028f;
        private const float letterRadius = (outerRadius + innerRadius) / 2f;
        private const float letterHeight = 0.085f;
        private const float letterHalfWidth = 0.0032f;
        private const int letterCount = 48;

        // Dark edging under the gold, so the lines read on light grass as well as on the road
        private const float shadowExtra = 0.0035f;
        private const float shadowAlpha = 0.55f;
        private const float glowWidth = 0.03f;
        private const float glowAlpha = 0.35f;
        private const float fillAlpha = 0.12f;

        private static readonly Color gold = new(1f, 0.82f, 0.42f);
        private static readonly Color shadow = new(0.16f, 0.1f, 0.04f);
        private static readonly Color fill = new(1f, 0.9f, 0.62f);

        // Letters after the tengwar, each in a box one letter high, x along the line of writing and y outwards: a stem
        // with one or two bowls, or one of a few curved letters, and above many of them a tehta of dots or a stroke
        private static readonly Vector2[][][] letters = BuildLetters();
        private static readonly Vector2[][][] tehtar =
        {
            new[] { Dot(0f, 0.4f) },
            new[] { Dot(-0.1f, 0.36f), Dot(0.1f, 0.36f), Dot(0f, 0.47f) },
            new[] { Line(-0.06f, 0.32f, 0.08f, 0.47f) },
            new[] { Arc(0f, 0.32f, 0.1f, 0.08f, 180f, 0f) },
            new[] { Dot(-0.07f, 0.4f), Dot(0.07f, 0.4f) },
        };


        [MenuItem("Tools/Tile/Bake Tower Range Ring")]
        public static void Bake()
        {
            Sprite sprite = BakeSprite();
            if (sprite == null)
            {
                Debug.LogError($"Tower range ring sprite could not be loaded: {spritePath}");
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(basePrefabPath);
            try
            {
                Transform ring = root.transform.Find(ringName);
                SpriteRenderer renderer = ring != null ? ring.GetComponent<SpriteRenderer>() : null;
                if (renderer == null)
                {
                    Debug.LogError($"{ringName} with a sprite renderer not found in {basePrefabPath}");
                    return;
                }

                renderer.sprite = sprite;
                renderer.color = Color.white;
                if (ring.GetComponent<TowerRangeRing>() == null)
                    ring.gameObject.AddComponent<TowerRangeRing>();

                PrefabUtility.SaveAsPrefabAsset(root, basePrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Tower range ring baked");
        }


        private static Sprite BakeSprite()
        {
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false);
            texture.SetPixels32(Draw());
            texture.Apply();
            byte[] png = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);

            // Written only when it comes out different, so a bake that changes nothing leaves the file alone
            bool unchanged = File.Exists(spritePath) && System.Linq.Enumerable.SequenceEqual(File.ReadAllBytes(spritePath), png);
            if (!unchanged)
            {
                File.WriteAllBytes(spritePath, png);
                AssetDatabase.ImportAsset(spritePath);
            }

            // The import settings are only touched when they differ, so the .meta file stays as it is too
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(spritePath);
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single ||
                importer.spritePixelsPerUnit != pixelsPerUnit || !importer.alphaIsTransparency || !importer.mipmapEnabled ||
                importer.wrapMode != TextureWrapMode.Clamp || importer.textureCompression != TextureImporterCompression.CompressedHQ)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        }

        private static Color32[] Draw()
        {
            Color32[] pixels = new Color32[size * size];
            float half = size / 2f;
            float pixel = 1f / half;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new((x + 0.5f - half) / half, (y + 0.5f - half) / half);
                    float radius = point.magnitude;

                    // Colour kept premultiplied by its alpha while the layers are laid one over another
                    Vector4 colour = Vector4.zero;

                    if (radius < outerRadius)
                        Over(ref colour, fill, fillAlpha * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.25f, outerRadius, radius)));

                    float fromOuter = radius - outerRadius;
                    float glow = Mathf.Exp(-(fromOuter * fromOuter) / (glowWidth * glowWidth)) * (fromOuter > 0f ? 1f : 0.5f);
                    Over(ref colour, gold, glowAlpha * glow);

                    float lines = LinesDistance(point, radius);
                    Over(ref colour, shadow, shadowAlpha * Coverage(lines - shadowExtra, pixel));
                    Over(ref colour, gold, Coverage(lines, pixel));

                    pixels[y * size + x] = colour.w <= 0f
                        ? new Color32(0, 0, 0, 0)
                        : (Color32)new Color(colour.x / colour.w, colour.y / colour.w, colour.z / colour.w, colour.w);
                }
            }

            return pixels;
        }

        // Signed distance to the nearest gold line: negative inside a line, as parts of half the sprite
        private static float LinesDistance(Vector2 point, float radius)
        {
            float distance = Mathf.Abs(radius - outerRadius) - outerHalfWidth;
            distance = Mathf.Min(distance, Mathf.Abs(radius - innerRadius) - innerHalfWidth);

            if (Mathf.Abs(radius - letterRadius) > letterHeight)
                return distance;

            float step = Mathf.PI * 2f / letterCount;
            float angle = Mathf.Atan2(point.y, point.x);

            // The letter at this angle, in its own box. Read with its top outwards, the writing runs clockwise
            int index = Mathf.RoundToInt(angle / step);
            Vector2 local = new(-(angle - index * step) * radius / letterHeight, (radius - letterRadius) / letterHeight);

            int letter = (((index * 11 + 3) % letters.Length) + letters.Length) % letters.Length;
            distance = Mathf.Min(distance, StrokesDistance(local, letters[letter]));

            int tehtaChoices = tehtar.Length * 2;
            int tehta = (((index * 3 + 1) % tehtaChoices) + tehtaChoices) % tehtaChoices;
            if (tehta < tehtar.Length)
                distance = Mathf.Min(distance, StrokesDistance(local, tehtar[tehta]));

            return distance;
        }

        private static float StrokesDistance(Vector2 local, Vector2[][] strokes)
        {
            float distance = float.MaxValue;
            foreach (Vector2[] stroke in strokes)
            {
                for (int i = 1; i < stroke.Length; i++)
                    distance = Mathf.Min(distance, SegmentDistance(local, stroke[i - 1], stroke[i]) * letterHeight - letterHalfWidth);
            }
            return distance;
        }

        private static Vector2[][][] BuildLetters()
        {
            List<Vector2[][]> built = new();

            // Stems go down from the body of the letter, up and down, or stay within it
            float[][] stems = { new[] { -0.42f, 0.2f }, new[] { -0.42f, 0.42f }, new[] { -0.2f, 0.2f } };
            const float stemX = -0.15f;

            foreach (float[] stem in stems)
            {
                foreach (bool twoBowls in new[] { false, true })
                {
                    foreach (bool closed in new[] { false, true })
                    {
                        List<Vector2[]> strokes = new() { Line(stemX, stem[0], stemX, stem[1]) };
                        float end = closed ? -90f : -55f;
                        if (twoBowls)
                        {
                            strokes.Add(Arc(stemX, 0f, 0.14f, 0.2f, 90f, end));
                            strokes.Add(Arc(stemX + 0.14f, 0f, 0.14f, 0.2f, 90f, end));
                        }
                        else
                        {
                            strokes.Add(Arc(stemX, 0f, 0.24f, 0.2f, 90f, end));
                        }

                        built.Add(strokes.ToArray());
                        built.Add(Mirror(strokes.ToArray()));
                    }
                }
            }

            // Letters without a stem: an arch, a cup, a hook and a double curve
            built.Add(new[] { Arc(0f, -0.2f, 0.22f, 0.4f, 180f, 0f), Arc(0.3f, -0.2f, 0.08f, 0.08f, 180f, 360f) });
            built.Add(new[] { Arc(0f, 0.2f, 0.2f, 0.4f, 180f, 360f) });
            built.Add(new[] { Arc(0f, 0.05f, 0.15f, 0.15f, 200f, -40f), Line(0.11f, -0.05f, -0.05f, -0.42f) });
            built.Add(new[] { Arc(0f, 0.1f, 0.13f, 0.1f, 0f, 270f), Arc(0f, -0.1f, 0.13f, 0.1f, 90f, -180f) });

            return built.ToArray();
        }

        private static float SegmentDistance(Vector2 point, Vector2 from, Vector2 to)
        {
            Vector2 span = to - from;
            float t = Mathf.Clamp01(Vector2.Dot(point - from, span) / span.sqrMagnitude);
            return (point - (from + span * t)).magnitude;
        }

        // How much of a pixel a shape covers, from its signed distance: a one pixel wide soft edge
        private static float Coverage(float distance, float pixel)
        {
            return Mathf.Clamp01(0.5f - distance / pixel);
        }

        private static void Over(ref Vector4 colour, Color layer, float alpha)
        {
            colour = new Vector4(layer.r * alpha, layer.g * alpha, layer.b * alpha, alpha) + colour * (1f - alpha);
        }

        private static Vector2[] Line(float fromX, float fromY, float toX, float toY)
        {
            return new[] { new Vector2(fromX, fromY), new Vector2(toX, toY) };
        }

        // A piece of an ellipse from one angle to the other, in degrees, as a line of short segments
        private static Vector2[] Arc(float centreX, float centreY, float radiusX, float radiusY, float from, float to)
        {
            const int segments = 16;
            Vector2[] points = new Vector2[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float angle = Mathf.Lerp(from, to, i / (float)segments) * Mathf.Deg2Rad;
                points[i] = new Vector2(centreX + Mathf.Cos(angle) * radiusX, centreY + Mathf.Sin(angle) * radiusY);
            }
            return points;
        }

        private static Vector2[] Dot(float x, float y)
        {
            return Arc(x, y, 0.035f, 0.035f, 0f, 360f);
        }

        private static Vector2[][] Mirror(Vector2[][] strokes)
        {
            Vector2[][] mirrored = new Vector2[strokes.Length][];
            for (int i = 0; i < strokes.Length; i++)
            {
                mirrored[i] = new Vector2[strokes[i].Length];
                for (int j = 0; j < strokes[i].Length; j++)
                    mirrored[i][j] = new Vector2(-strokes[i][j].x, strokes[i][j].y);
            }
            return mirrored;
        }
    }

}
