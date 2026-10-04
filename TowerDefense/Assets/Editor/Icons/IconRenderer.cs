using System.IO;

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace TowerDefense.EditorTools.Icons
{
    // Renders an object into a sprite for a UI button, seen from the pitch the game camera looks at the map
    // with, so the button shows what the object will look like in the game. Shared by the icon bakers, which
    // decide what gets an icon and from which side it is seen
    public static class IconRenderer
    {
        private const int iconSize = 256;
        // Rendered this many times larger and averaged down in code. Hardware anti aliasing resolves
        // edge coverage a little differently from run to run, which rewrote the icons on every bake.
        private const int supersample = 4;

        // Seen from above and off to the side, so the object shows a corner rather than a flat face
        private const float cameraPitch = 30f;
        // Seen from behind on the left, unless a baker turns the camera to another side
        public const float DefaultCameraYaw = 45f;
        // The light comes from the camera's left at this angle, whichever side the camera looks from
        private const float lightYawFromCamera = -75f;
        private const float fieldOfView = 60f;
        // Leaves a little air around the object
        private const float framing = 1.05f;

        // zoom brings the camera closer than the distance at which the whole bounding box fits: the object comes
        // out that many times larger. Above 1 the corners of the box leave the frame, which is fine for an
        // object that does not fill its box, such as a tower, and crops one that does, such as a flat tile
        public static void Render(string prefabPath, string iconPath, float cameraYaw = DefaultCameraYaw, float zoom = 1f)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Icon source prefab not found: {prefabPath}");
                return;
            }

            Render(prefab, iconPath, cameraYaw, zoom);
        }

        // Renders any object, also prefab contents a baker is still editing: right after a bake the prefab asset
        // can still hand out its previous version, which would put the old model into the icon
        public static void Render(GameObject source, string iconPath, float cameraYaw = DefaultCameraYaw, float zoom = 1f)
        {
            GameObject instance = null;
            GameObject rig = null;
            RenderTexture target = null;

            try
            {
                instance = Object.Instantiate(source);
                // A copy of prefab contents may land in their preview scene, which the camera does not draw
                if (instance.scene != SceneManager.GetActiveScene())
                    SceneManager.MoveGameObjectToScene(instance, SceneManager.GetActiveScene());
                instance.hideFlags = HideFlags.HideAndDontSave;
                // Well away from whatever the open scene holds
                instance.transform.position = new Vector3(0, 10000, 0);

                if (!TryGetBounds(instance, out Bounds bounds))
                {
                    Debug.LogWarning($"Nothing to render for {iconPath}");
                    return;
                }

                rig = new GameObject("IconRig") { hideFlags = HideFlags.HideAndDontSave };

                Light light = new GameObject("IconLight").AddComponent<Light>();
                light.transform.SetParent(rig.transform);
                light.type = LightType.Directional;
                light.transform.rotation = Quaternion.Euler(50, cameraYaw + lightYawFromCamera, 0);
                light.intensity = 1.1f;

                Camera camera = new GameObject("IconCamera").AddComponent<Camera>();
                camera.transform.SetParent(rig.transform);

                Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
                float distance = DistanceToFit(bounds, rotation) * framing / zoom;
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

                SavePng(icon, iconPath);
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

            if (!TryWrite(path, png))
                return;

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

        // Another program may hold the old icon open for a moment, Windows then refuses the write. A few retries
        // usually get through; if not, the old icon stays and the bake that asked for it goes on
        private static bool TryWrite(string path, byte[] png)
        {
            const int attempts = 5;
            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    File.WriteAllBytes(path, png);
                    return true;
                }
                catch (IOException exception) when (attempt < attempts)
                {
                    Debug.Log($"Icon {path} is busy, retrying: {exception.Message}");
                    System.Threading.Thread.Sleep(200);
                }
                catch (IOException exception)
                {
                    Debug.LogWarning($"Icon {path} could not be written, the old one is kept; bake again to update it. {exception.Message}");
                    return false;
                }
            }
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

            // Only the models frame the icon. Particle systems do not show in a still icon, and lines such as a tower's
            // beam are switched off and keep world positions, so they would stretch the framing far from the model
            bool hasBounds = false;
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled || !(renderer is MeshRenderer || renderer is SkinnedMeshRenderer))
                    continue;

                if (hasBounds)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
                else
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
            }

            return hasBounds;
        }

        public static void EnsureFolder(string folder)
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
