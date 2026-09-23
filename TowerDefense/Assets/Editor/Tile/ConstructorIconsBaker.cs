using System.IO;

using UnityEditor;
using UnityEngine;


namespace TowerDefense.EditorTools.Tile
{
    // Renders the components the map constructor offers into sprites for its buttons, seen from the
    // pitch the game camera looks at the map with, so a button shows what the cell will look like.
    // SelectMenu picks the baked sprites up through its inspector fields.
    public static class ConstructorIconsBaker
    {
        private const string iconsFolder = "Assets/Sprites/Icons/MapConstructor";
        private const int iconSize = 256;
        // Rendered this many times larger and averaged down in code. Hardware anti aliasing resolves
        // edge coverage a little differently from run to run, which rewrote the icons on every bake.
        private const int supersample = 4;

        // Seen from above and off to the side, so the object shows a corner rather than a flat face
        private const float cameraPitch = 30f;
        private const float cameraYaw = 45f;
        private const float fieldOfView = 60f;
        // Leaves a little air around the object
        private const float framing = 1.05f;

        private static readonly (string prefabPath, string iconName)[] icons =
        {
            ("Assets/Prefabs/MapComponents/Tile/PathTile/PathTile_Straight_NS.prefab", "IconPathTile"),
            ("Assets/Prefabs/MapComponents/Tile/BlockedTile/BlockedTile_00.prefab", "IconBlockedTile"),
            ("Assets/Prefabs/MapComponents/Building/StartBuilding.prefab", "IconStartBuilding"),
            ("Assets/Prefabs/MapComponents/Building/EndBuilding.prefab", "IconEndBuilding"),
        };


        [MenuItem("Tools/Tile/Bake Constructor Icons")]
        public static void Bake()
        {
            EnsureFolder(iconsFolder);

            foreach (var icon in icons)
                BakeIcon(icon.prefabPath, icon.iconName);

            AssetDatabase.SaveAssets();
            Debug.Log("Constructor icons baked");
        }


        private static void BakeIcon(string prefabPath, string iconName)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Icon source prefab not found: {prefabPath}");
                return;
            }

            GameObject instance = null;
            GameObject rig = null;
            RenderTexture target = null;

            try
            {
                instance = Object.Instantiate(prefab);
                instance.hideFlags = HideFlags.HideAndDontSave;
                // Well away from whatever the open scene holds
                instance.transform.position = new Vector3(0, 10000, 0);

                if (!TryGetBounds(instance, out Bounds bounds))
                {
                    Debug.LogWarning($"Nothing to render for {iconName}");
                    return;
                }

                rig = new GameObject("IconRig") { hideFlags = HideFlags.HideAndDontSave };

                Light light = new GameObject("IconLight").AddComponent<Light>();
                light.transform.SetParent(rig.transform);
                light.type = LightType.Directional;
                light.transform.rotation = Quaternion.Euler(50, -30, 0);
                light.intensity = 1.1f;

                Camera camera = new GameObject("IconCamera").AddComponent<Camera>();
                camera.transform.SetParent(rig.transform);

                Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
                float distance = DistanceToFit(bounds, rotation) * framing;
                camera.transform.SetPositionAndRotation(bounds.center - rotation * Vector3.forward * distance, rotation);

                camera.fieldOfView = fieldOfView;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0, 0, 0, 0);
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = distance * 4f;
                camera.enabled = false;

                int renderSize = iconSize * supersample;
                target = new RenderTexture(renderSize, renderSize, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
                camera.targetTexture = target;
                camera.Render();

                Texture2D rendered = ReadPixels(target);
                Texture2D icon = Downsample(rendered, iconSize);
                Object.DestroyImmediate(rendered);

                SavePng(icon, $"{iconsFolder}/{iconName}.png");
            }
            finally
            {
                if (target != null)
                {
                    RenderTexture.active = null;
                    target.Release();
                    Object.DestroyImmediate(target);
                }
                if (rig != null)
                    Object.DestroyImmediate(rig);
                if (instance != null)
                    Object.DestroyImmediate(instance);
            }
        }

        // The nearest distance at which every corner of the bounds is still inside the view.
        // A bounding sphere would push a flat tile far away and leave it tiny in the icon.
        private static float DistanceToFit(Bounds bounds, Quaternion rotation)
        {
            // The icon is square, so both half angles are the same
            float halfExtent = Mathf.Tan(Mathf.Deg2Rad * fieldOfView * 0.5f);
            Quaternion inverse = Quaternion.Inverse(rotation);

            float distance = 0;
            foreach (Vector3 corner in Corners(bounds))
            {
                Vector3 local = inverse * (corner - bounds.center);

                distance = Mathf.Max(distance, local.z + Mathf.Abs(local.y) / halfExtent);
                distance = Mathf.Max(distance, local.z + Mathf.Abs(local.x) / halfExtent);
            }

            return distance;
        }

        private static Vector3[] Corners(Bounds bounds)
        {
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;

            Vector3[] corners = new Vector3[8];
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = center + new Vector3((i & 1) == 0 ? -extents.x : extents.x,
                                                  (i & 2) == 0 ? -extents.y : extents.y,
                                                  (i & 4) == 0 ? -extents.z : extents.z);
            }

            return corners;
        }

        private static Texture2D ReadPixels(RenderTexture target)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;

            Texture2D texture = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            texture.Apply();

            RenderTexture.active = previous;

            return texture;
        }

        // Averaged with premultiplied alpha, so transparent pixels do not bleed darkness into the edges
        private static Texture2D Downsample(Texture2D source, int size)
        {
            int scale = source.width / size;
            int samples = scale * scale;

            Color32[] pixels = source.GetPixels32();
            Color32[] result = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int r = 0, g = 0, b = 0, a = 0;
                    for (int sampleY = 0; sampleY < scale; sampleY++)
                    {
                        int row = (y * scale + sampleY) * source.width + x * scale;
                        for (int sampleX = 0; sampleX < scale; sampleX++)
                        {
                            Color32 pixel = pixels[row + sampleX];
                            r += pixel.r * pixel.a;
                            g += pixel.g * pixel.a;
                            b += pixel.b * pixel.a;
                            a += pixel.a;
                        }
                    }

                    result[y * size + x] = a == 0
                        ? new Color32(0, 0, 0, 0)
                        : new Color32((byte)(r / a), (byte)(g / a), (byte)(b / a), (byte)(a / samples));
                }
            }

            Texture2D downsampled = new Texture2D(size, size, TextureFormat.RGBA32, false);
            downsampled.SetPixels32(result);
            downsampled.Apply();

            return downsampled;
        }

        private static void SavePng(Texture2D texture, string path)
        {
            byte[] png = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);

            // Leave the asset and its import settings alone when the icon comes out the same
            if (Unchanged(path, png))
                return;

            File.WriteAllBytes(path, png);

            AssetDatabase.ImportAsset(path);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            // Without this the project's default import mode leaves the icon as a sprite sheet of one
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static bool Unchanged(string path, byte[] png)
        {
            if (!File.Exists(path))
                return false;

            byte[] existing = File.ReadAllBytes(path);
            if (existing.Length != png.Length)
                return false;

            for (int i = 0; i < png.Length; i++)
            {
                if (existing[i] != png[i])
                    return false;
            }

            return true;
        }

        private static bool TryGetBounds(GameObject instance, out Bounds bounds)
        {
            bounds = new Bounds();

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return false;

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            return true;
        }

        private static void EnsureFolder(string folder)
        {
            string parent = "Assets";
            foreach (string part in folder.Substring("Assets/".Length).Split('/'))
            {
                string current = $"{parent}/{part}";
                if (!AssetDatabase.IsValidFolder(current))
                    AssetDatabase.CreateFolder(parent, part);

                parent = current;
            }
        }
    }

}
