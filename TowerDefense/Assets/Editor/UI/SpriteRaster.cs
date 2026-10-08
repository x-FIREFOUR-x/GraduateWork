using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEngine;


namespace TowerDefense.EditorTools.UI
{
    // Draws flat vector shapes into a sprite: each shape is a signed distance (negative inside) over drawing units with
    // y going down, as in the mockups the shapes come from, and is laid over what is drawn already with a soft edge
    public class SpriteRaster
    {
        private readonly int width;
        private readonly int height;
        private readonly float scale;
        private readonly Vector2 origin;

        // Colour kept premultiplied by its alpha while the shapes are laid one over another
        private readonly Vector4[] colour;


        public SpriteRaster(Rect area, float pixelsPerUnit)
        {
            width = Mathf.CeilToInt(area.width * pixelsPerUnit);
            height = Mathf.CeilToInt(area.height * pixelsPerUnit);
            scale = pixelsPerUnit;
            origin = area.min;
            colour = new Vector4[width * height];
        }


        public void Draw(Func<Vector2, float> distance, Rect bounds, Func<Vector2, Color> paint, float alpha = 1f)
        {
            float edge = 1f / scale;
            int fromX = Mathf.Max(0, Mathf.FloorToInt((bounds.xMin - edge - origin.x) * scale));
            int toX = Mathf.Min(width - 1, Mathf.CeilToInt((bounds.xMax + edge - origin.x) * scale));
            int fromRow = Mathf.Max(0, Mathf.FloorToInt((bounds.yMin - edge - origin.y) * scale));
            int toRow = Mathf.Min(height - 1, Mathf.CeilToInt((bounds.yMax + edge - origin.y) * scale));

            for (int row = fromRow; row <= toRow; row++)
            {
                for (int x = fromX; x <= toX; x++)
                {
                    Vector2 point = origin + new Vector2((x + 0.5f) / scale, (row + 0.5f) / scale);
                    float coverage = Mathf.Clamp01(0.5f - distance(point) * scale);
                    if (coverage <= 0f)
                        continue;

                    Color layer = paint(point);
                    float a = layer.a * coverage * alpha;
                    // Rows of the drawing go down, rows of a texture go up
                    int index = (height - 1 - row) * width + x;
                    colour[index] = new Vector4(layer.r * a, layer.g * a, layer.b * a, a) + colour[index] * (1f - a);
                }
            }
        }

        public void Fill(List<Vector2> polygon, Func<Vector2, Color> paint, float alpha = 1f)
        {
            Draw(p => PolygonDistance(p, polygon), BoundsOf(polygon, 0f), paint, alpha);
        }

        public void Stroke(List<Vector2> line, float lineWidth, Func<Vector2, Color> paint, bool closed = false, float alpha = 1f)
        {
            Draw(p => LineDistance(p, line, closed) - lineWidth / 2f, BoundsOf(line, lineWidth), paint, alpha);
        }

        public Sprite Save(string path, float pixelsPerUnit, Vector4 border = default)
        {
            Texture2D texture = new(width, height, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[colour.Length];
            for (int i = 0; i < colour.Length; i++)
            {
                Vector4 c = colour[i];
                pixels[i] = c.w <= 0f ? new Color32(0, 0, 0, 0) : (Color32)new Color(c.x / c.w, c.y / c.w, c.z / c.w, c.w);
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            byte[] png = texture.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(texture);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // Written only when it comes out different, so a bake that changes nothing leaves the file alone
            bool unchanged = File.Exists(path) && System.Linq.Enumerable.SequenceEqual(File.ReadAllBytes(path), png);
            if (!unchanged)
            {
                File.WriteAllBytes(path, png);
                AssetDatabase.ImportAsset(path);
            }

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single ||
                importer.spritePixelsPerUnit != pixelsPerUnit || importer.spriteBorder != border || !importer.alphaIsTransparency ||
                importer.mipmapEnabled || importer.wrapMode != TextureWrapMode.Clamp ||
                importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.spriteBorder = border;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }


        public static Func<Vector2, Color> Solid(Color color)
        {
            return _ => color;
        }

        // Colour stops down the height of a shape, from its top to its bottom
        public static Func<Vector2, Color> Vertical(float top, float bottom, params (float at, Color color)[] stops)
        {
            return p =>
            {
                float t = Mathf.InverseLerp(top, bottom, p.y);
                for (int i = 1; i < stops.Length; i++)
                {
                    if (t <= stops[i].at)
                        return Color.Lerp(stops[i - 1].color, stops[i].color, Mathf.InverseLerp(stops[i - 1].at, stops[i].at, t));
                }
                return stops[stops.Length - 1].color;
            };
        }

        public static Color Hex(string hex, float alpha = 1f)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            color.a = alpha;
            return color;
        }


        public static Rect BoundsOf(List<Vector2> points, float grow)
        {
            Vector2 min = points[0];
            Vector2 max = points[0];
            foreach (Vector2 p in points)
            {
                min = Vector2.Min(min, p);
                max = Vector2.Max(max, p);
            }
            return Rect.MinMaxRect(min.x - grow, min.y - grow, max.x + grow, max.y + grow);
        }

        public static float PolygonDistance(Vector2 p, List<Vector2> v)
        {
            float distance = Vector2.Dot(p - v[0], p - v[0]);
            float sign = 1f;
            for (int i = 0, j = v.Count - 1; i < v.Count; j = i, i++)
            {
                Vector2 e = v[j] - v[i];
                Vector2 w = p - v[i];
                Vector2 b = w - e * Mathf.Clamp01(Vector2.Dot(w, e) / Vector2.Dot(e, e));
                distance = Mathf.Min(distance, Vector2.Dot(b, b));

                bool above = p.y >= v[i].y;
                bool below = p.y < v[j].y;
                bool left = e.x * w.y > e.y * w.x;
                if ((above && below && left) || (!above && !below && !left))
                    sign = -sign;
            }
            return sign * Mathf.Sqrt(distance);
        }

        public static float LineDistance(Vector2 p, List<Vector2> line, bool closed)
        {
            float distance = float.MaxValue;
            int count = closed ? line.Count + 1 : line.Count;
            for (int i = 1; i < count; i++)
                distance = Mathf.Min(distance, SegmentDistance(p, line[i - 1], line[i % line.Count]));
            return distance;
        }

        public static float SegmentDistance(Vector2 p, Vector2 from, Vector2 to)
        {
            Vector2 span = to - from;
            float t = span.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(p - from, span) / span.sqrMagnitude) : 0f;
            return (p - (from + span * t)).magnitude;
        }

        // A box with its own radius at each corner, centred on the origin, in drawing units where y goes down
        public static float RoundBoxDistance(Vector2 p, Vector2 halfSize, float topLeft, float topRight, float bottomRight, float bottomLeft)
        {
            float radius = p.x > 0f ? (p.y > 0f ? bottomRight : topRight) : (p.y > 0f ? bottomLeft : topLeft);
            Vector2 q = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) - halfSize + Vector2.one * radius;
            return Mathf.Min(Mathf.Max(q.x, q.y), 0f) + Vector2.Max(q, Vector2.zero).magnitude - radius;
        }


        // Curves laid out as lines of short segments, each starting where the path is, as in an svg path
        public static void Quad(List<Vector2> path, Vector2 control, Vector2 to, int segments = 12)
        {
            Vector2 from = path[path.Count - 1];
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                path.Add((1 - t) * (1 - t) * from + 2 * (1 - t) * t * control + t * t * to);
            }
        }

        public static void Cubic(List<Vector2> path, Vector2 first, Vector2 second, Vector2 to, int segments = 16)
        {
            Vector2 from = path[path.Count - 1];
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float u = 1 - t;
                path.Add(u * u * u * from + 3 * u * u * t * first + 3 * u * t * t * second + t * t * t * to);
            }
        }

        public static List<Vector2> Points(params float[] coordinates)
        {
            List<Vector2> points = new();
            for (int i = 0; i + 1 < coordinates.Length; i += 2)
                points.Add(new Vector2(coordinates[i], coordinates[i + 1]));
            return points;
        }
    }

}
