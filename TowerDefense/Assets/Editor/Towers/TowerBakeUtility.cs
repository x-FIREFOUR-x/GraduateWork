using System.IO;

using UnityEditor;
using UnityEngine;

using MinMaxCurve = UnityEngine.ParticleSystem.MinMaxCurve;


namespace TowerDefense.EditorTools.Towers
{
    // Steps every tower baker takes: saving meshes, textures and materials in place, building prefab parts and
    // particle systems. A baker brings its own meshes and decides what goes where
    public static class TowerBakeUtility
    {
        // A one shot system in world space; burst 0 leaves emission to the caller
        public static ParticleSystem NewParticles(string name, Transform parent, Material material, float minLifetime, float maxLifetime, int burst)
        {
            GameObject particlesObject = new(name);
            particlesObject.transform.SetParent(parent, false);

            ParticleSystem particles = particlesObject.AddComponent<ParticleSystem>();
            // The duration can only be changed on a system that is not playing
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = particles.main;
            main.duration = 1f;
            main.loop = false;
            main.playOnAwake = true;
            main.startLifetime = new MinMaxCurve(minLifetime, maxLifetime);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            if (burst > 0)
                emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burst) });

            particlesObject.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;

            return particles;
        }

        // Cone opening upwards: a cone emits along its local Z, turned here to point at the sky
        public static void ConeUp(ParticleSystem particles, float angle, float radius)
        {
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = angle;
            shape.radius = radius;
            shape.rotation = new Vector3(-90f, 0f, 0f);
        }

        // Grows from one size factor to another while fading in fast and out slowly
        public static void GrowAndFade(ParticleSystem particles, float startSize, float endSize)
        {
            ParticleSystem.SizeOverLifetimeModule size = particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, startSize, 1f, endSize));

            Gradient fade = new();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                         new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.08f), new GradientAlphaKey(0f, 1f) });

            ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
            color.enabled = true;
            color.color = fade;
        }


        // Writes a palette texture made by a mesh builder and imports it as a clean, unfiltered colour table
        public static Texture2D BakePaletteTexture(string path, Texture2D texture)
        {
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));

            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.sRGBTexture = true;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // Matte like the castle and the tiles: the look comes from the palette, not from highlights.
        // With a glow colour the palette also lights the surface itself, scaled by that colour, for parts that
        // give off light of their own such as ice crystals
        public static Material BakePaletteMaterial(string path, Texture2D palette, Color? glow = null)
        {
            Material material = new(Shader.Find("Standard"))
            {
                color = Color.white,
                mainTexture = palette,
                enableInstancing = true
            };
            material.SetFloat("_Glossiness", 0f);
            material.SetFloat("_Metallic", 0f);

            if (glow.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetTexture("_EmissionMap", palette);
                material.SetColor("_EmissionColor", glow.Value);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            return SaveAsset(material, path);
        }

        public static Material LoadMaterial(string path)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
                Debug.LogError($"Particle material not found: {path}");

            return material;
        }


        // Child holding a mesh, reused when it is already there so the prefab keeps its objects
        public static Transform MeshPart(Transform parent, string name, Mesh mesh, Material material)
        {
            Transform part = Child(parent, name);

            MeshFilter filter = part.GetComponent<MeshFilter>();
            if (filter == null)
                filter = part.gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            MeshRenderer renderer = part.GetComponent<MeshRenderer>();
            if (renderer == null)
                renderer = part.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[] { material };

            return part;
        }

        // Child with a clean transform, found by name or created
        public static Transform Child(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child == null)
            {
                child = new GameObject(name).transform;
                child.SetParent(parent, false);
            }

            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;

            return child;
        }

        public static void KeepOnly(Transform parent, params string[] names)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (System.Array.IndexOf(names, child.name) < 0)
                    Object.DestroyImmediate(child.gameObject);
            }
        }

        // Removes a child built on an earlier bake, so it can be built afresh
        public static void Remove(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);
        }


        // Creates the asset or overwrites the existing one in place, so references to it stay valid
        public static T SaveAsset<T>(T asset, string path) where T : Object
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(asset, path);
                return asset;
            }

            // A mesh copied over with CopySerialized changes on disk, but the editor keeps drawing the old one
            // until it is restarted; filled through the mesh API it is drawn new right away
            if (asset is Mesh mesh && existing is Mesh existingMesh)
            {
                CopyMesh(mesh, existingMesh);
                EditorUtility.SetDirty(existingMesh);
                Object.DestroyImmediate(mesh);

                return existing;
            }

            string name = existing.name;
            EditorUtility.CopySerialized(asset, existing);
            existing.name = name;
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(asset);

            return existing;
        }

        private static void CopyMesh(Mesh source, Mesh target)
        {
            target.Clear();
            target.indexFormat = source.indexFormat;
            target.vertices = source.vertices;
            target.normals = source.normals;
            target.tangents = source.tangents;
            target.uv = source.uv;

            target.subMeshCount = source.subMeshCount;
            for (int i = 0; i < source.subMeshCount; i++)
                target.SetTriangles(source.GetTriangles(i), i);

            target.RecalculateBounds();
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
