using System.IO;
using System.Security.Cryptography;
using System.Text;

using UnityEditor;
using UnityEngine;

using TowerDefense.Main.Projectiles;
using TowerDefense.Main.Towers;

using MinMaxCurve = UnityEngine.ParticleSystem.MinMaxCurve;
using MinMaxGradient = UnityEngine.ParticleSystem.MinMaxGradient;


namespace TowerDefense.EditorTools.Towers
{
    // Bakes the catapult tower: the palette texture and material, the part meshes, the thrown stone with its
    // dust trail, the impact effect (dust, flying debris, grit and a ring as wide as the hit radius), the
    // tower prefab itself and its shop icon.
    // The tower prefab is rebuilt in place, so the TowersStorage and the shop keep pointing at it; its price,
    // range and fire rate are left as they are. The stone keeps the damage, speed and radius it was given
    [InitializeOnLoad]
    public static class CatapultPrefabBaker
    {
        private const string towerPrefabPath = "Assets/Prefabs/Towers/Catapult.prefab";
        private const string stonePrefabPath = "Assets/Prefabs/Projectile/CatapultStone.prefab";
        private const string hitEffectPath = "Assets/Prefabs/Effects/ProjectileHit/StoneHitEffect.prefab";
        private const string meshesFolder = "Assets/Models/Towers/Catapult";
        private const string texturePath = "Assets/Textures/Towers/CatapultPalette.png";
        private const string materialPath = "Assets/Materials/Tower/Catapult.mat";

        // Soft particle materials the tower building effect already uses
        private const string smokeMaterialPath = "Assets/Materials/Effects/BuildingTower/Smoke26.mat";
        private const string ringMaterialPath = "Assets/Materials/Effects/BuildingTower/Circle.mat";
        private const string pointMaterialPath = "Assets/Materials/Effects/BuildingTower/Point.mat";

        // What a new stone starts with: the numbers the tower balance and the genetic algorithm were tuned for
        private const float defaultDamage = 40f;
        private const float defaultSpeed = 20f;
        private const float defaultRadius = 4f;

        // The catapult and its stone are drawn this much larger than the meshes are built. The stone is
        // scaled alike, so the one thrown matches the one lying in the bucket
        // The wheels stay on the platform when the catapult turns its corners towards the platform rim
        private const float catapultScale = 1.5f;

        // Left by bakes from when the catapult stood on a stone plinth
        private const string legacyBaseMeshName = "Base";

        // The platform stands on the tile top, which lies this far above the tile centre the tower is put on
        private static readonly Vector3 offsetTower = new(0f, 0.5f, 0f);

        private static readonly Color dustColor = new(0.62f, 0.55f, 0.45f, 0.9f);
        private static readonly Color darkDustColor = new(0.48f, 0.43f, 0.37f, 0.85f);


        // The bakers' own source, hashed: when it changes, the catapult is rebaked on the next script reload,
        // so a change to the model shows up without running the menu item. Kept in Library, it is per machine
        private const string bakerSourceFolder = "Assets/Editor/Towers";
        private const string bakedHashPath = "Library/CatapultBake.hash";
        // Taken when the scripts are loaded, so it belongs to the code that is running. Taken at bake time instead,
        // a source edited after the last compile would be recorded as baked while the old code baked it
        private static readonly string compiledSourceHash = BakerSourceHash();


        static CatapultPrefabBaker()
        {
            EditorApplication.delayCall += BakeIfOutdated;
        }

        private static void BakeIfOutdated()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            bool isMissing = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath("Frame")) == null
                          || AssetDatabase.LoadAssetAtPath<GameObject>(stonePrefabPath) == null;
            bool isOutdated = !File.Exists(bakedHashPath) || File.ReadAllText(bakedHashPath) != compiledSourceHash;

            if (isMissing || isOutdated)
                Bake();
        }

        private static string BakerSourceHash()
        {
            StringBuilder source = new();
            string[] files = Directory.GetFiles(bakerSourceFolder, "*.cs", SearchOption.TopDirectoryOnly);
            System.Array.Sort(files, System.StringComparer.Ordinal);
            foreach (string file in files)
                source.Append(File.ReadAllText(file));

            using SHA1 sha = SHA1.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(source.ToString()));
            return System.BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        [MenuItem("Tools/Towers/Bake Catapult")]
        public static void Bake()
        {
            GameObject towerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(towerPrefabPath);
            if (towerPrefab == null || towerPrefab.GetComponent<CatapultTower>() == null)
            {
                Debug.LogError($"Catapult tower prefab with a {nameof(CatapultTower)} not found: {towerPrefabPath}");
                return;
            }

            Material smoke = LoadMaterial(smokeMaterialPath);
            Material ring = LoadMaterial(ringMaterialPath);
            Material point = LoadMaterial(pointMaterialPath);
            if (smoke == null || ring == null || point == null)
                return;

            EnsureFolder(meshesFolder);
            EnsureFolder(Path.GetDirectoryName(texturePath).Replace('\\', '/'));

            Material material = BakeMaterial(BakePalette());

            Mesh platformMesh = SaveAsset(CatapultMeshBuilder.BuildPlatform(), MeshPath("Platform"));
            Mesh frameMesh = SaveAsset(CatapultMeshBuilder.BuildFrame(), MeshPath("Frame"));
            Mesh armMesh = SaveAsset(CatapultMeshBuilder.BuildArm(), MeshPath("Arm"));
            Mesh gnomeMesh = SaveAsset(CatapultMeshBuilder.BuildGnome(), MeshPath("Gnome"));
            Mesh gnomeArmMesh = SaveAsset(CatapultMeshBuilder.BuildGnomeArm(), MeshPath("GnomeArm"));
            Mesh stoneMesh = SaveAsset(CatapultMeshBuilder.BuildStone(), MeshPath("Stone"));
            Mesh debrisMesh = SaveAsset(CatapultMeshBuilder.BuildDebris(), MeshPath("Debris"));

            (float damage, float speed, float radius) = StoneStats();

            GameObject hitEffect = BakeHitEffect(material, debrisMesh, smoke, ring, point, radius);
            GameObject stone = BakeStone(material, stoneMesh, smoke, hitEffect, damage, speed, radius);
            BakeTower(material, platformMesh, frameMesh, armMesh, gnomeMesh, gnomeArmMesh, stoneMesh, smoke, stone);

            if (AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath(legacyBaseMeshName)) != null)
                AssetDatabase.DeleteAsset(MeshPath(legacyBaseMeshName));

            AssetDatabase.SaveAssets();

            File.WriteAllText(bakedHashPath, compiledSourceHash);
            Debug.Log("Catapult baked");
        }


        private static void BakeTower(Material material, Mesh platformMesh, Mesh frameMesh, Mesh armMesh, Mesh gnomeMesh, Mesh gnomeArmMesh, Mesh stoneMesh,
                                      Material smoke, GameObject stone)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(towerPrefabPath);
            try
            {
                root.name = Path.GetFileNameWithoutExtension(towerPrefabPath);

                // The platform stays put, the catapult on its wheels turns on top of it
                MeshPart(root.transform, "Platform", platformMesh, material);

                Transform rotatePart = Child(root.transform, "RotatePart");
                rotatePart.localPosition = Vector3.up * CatapultMeshBuilder.PlatformHeight;
                rotatePart.localScale = Vector3.one * catapultScale;

                MeshPart(rotatePart, "Frame", frameMesh, material);

                Transform arm = MeshPart(rotatePart, "Arm", armMesh, material);
                arm.localPosition = CatapultMeshBuilder.ArmPivot;
                arm.localRotation = Quaternion.Euler(CatapultMeshBuilder.ArmRestAngle, 0f, 0f);

                Transform loadedStone = MeshPart(arm, "LoadedStone", stoneMesh, material);

                // A part of his own, so he can jump when the catapult fires, with the wrench arm swinging inside him
                Transform gnome = MeshPart(rotatePart, "Gnome", gnomeMesh, material);
                gnome.localPosition = CatapultMeshBuilder.GnomePosition;
                gnome.localRotation = Quaternion.Euler(0f, CatapultMeshBuilder.GnomeYaw, 0f);
                gnome.localScale = Vector3.one * CatapultMeshBuilder.GnomeScale;

                Transform gnomeArm = MeshPart(gnome, "WrenchArm", gnomeArmMesh, material);
                gnomeArm.localPosition = CatapultMeshBuilder.GnomeShoulder;
                KeepOnly(gnome, "WrenchArm");
                loadedStone.localPosition = CatapultMeshBuilder.LoadedStoneCenter;

                // Not where the stone starts from, it leaves from the bucket; marks where the bucket lets it go
                Transform pointStartFire = Child(rotatePart, "PointStartFire");
                pointStartFire.localPosition = CatapultMeshBuilder.ReleasePoint();

                ParticleSystem launchDust = BakeLaunchDust(rotatePart, smoke);

                // Whatever else the prefab holds goes
                KeepOnly(root.transform, "Platform", "RotatePart");
                KeepOnly(rotatePart, "Frame", "Arm", "Gnome", "PointStartFire", "LaunchDust");
                KeepOnly(arm, "LoadedStone");

                SerializedObject tower = new(root.GetComponent<CatapultTower>());
                tower.FindProperty("stonePrefab").objectReferenceValue = stone;
                tower.FindProperty("arm").objectReferenceValue = arm;
                tower.FindProperty("loadedStone").objectReferenceValue = loadedStone.gameObject;
                tower.FindProperty("launchDust").objectReferenceValue = launchDust;
                tower.FindProperty("gnome").objectReferenceValue = gnome;
                tower.FindProperty("gnomeArm").objectReferenceValue = gnomeArm;
                tower.FindProperty("armRestAngle").floatValue = CatapultMeshBuilder.ArmRestAngle;
                tower.FindProperty("armFireAngle").floatValue = CatapultMeshBuilder.ArmFireAngle;
                tower.FindProperty("pointStartFire").objectReferenceValue = pointStartFire;
                tower.FindProperty("rotatePart").objectReferenceValue = rotatePart;
                tower.FindProperty("offsetTower").vector3Value = offsetTower;
                tower.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, towerPrefabPath);
                // Written before the icon, so a failing icon never costs the baked model
                AssetDatabase.SaveAssets();

                // Taken by the camera from the model just built, not from the prefab asset, which may still be
                // the one loaded before this bake
                TowerIconsBaker.BakeIcon(root);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static GameObject BakeStone(Material material, Mesh stoneMesh, Material smoke, GameObject hitEffect, float damage, float speed, float radius)
        {
            GameObject root = new(Path.GetFileNameWithoutExtension(stonePrefabPath));
            try
            {
                CatapultStone stone = root.AddComponent<CatapultStone>();
                Transform graphic = MeshPart(root.transform, "Graphic", stoneMesh, material);
                graphic.localScale = Vector3.one * catapultScale;

                // A thin streak of dust shaken off the stone along its flight. Kept small and sparse: denser,
                // bigger puffs merge into one grey mass that looks like a far bigger stone
                ParticleSystem trail = NewParticles("Trail", root.transform, smoke, 0.35f, 0.5f, 0);
                ParticleSystem.MainModule main = trail.main;
                main.loop = true;
                main.duration = 5f;
                main.startSpeed = 0f;
                main.startSize = new MinMaxCurve(0.2f, 0.35f);
                main.startRotation = new MinMaxCurve(0f, Mathf.PI * 2);
                main.startColor = new Color(0.75f, 0.7f, 0.62f, 0.35f);

                ParticleSystem.EmissionModule emission = trail.emission;
                emission.rateOverDistance = 3f;

                ParticleSystem.ShapeModule shape = trail.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.05f;

                GrowAndFade(trail, 0.8f, 1.3f);

                SerializedObject serialized = new(stone);
                serialized.FindProperty("effectHitPrefab").objectReferenceValue = hitEffect;
                serialized.FindProperty("speed").floatValue = speed;
                serialized.FindProperty("<Damage>k__BackingField").floatValue = damage;
                serialized.FindProperty("explosionRadius").floatValue = radius;
                serialized.FindProperty("graphic").objectReferenceValue = graphic;
                serialized.FindProperty("trail").objectReferenceValue = trail;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                return PrefabUtility.SaveAsPrefabAsset(root, stonePrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        // The effect is put where the stone lands, about half a unit above the ground, so its parts drop to it
        private static GameObject BakeHitEffect(Material material, Mesh debrisMesh, Material smoke, Material ring, Material point, float radius)
        {
            const float ground = -0.45f;

            GameObject root = new(Path.GetFileNameWithoutExtension(hitEffectPath));
            try
            {
                // Dust cloud thrown up and out, slowing down as it spreads
                ParticleSystem dust = NewParticles("Dust", root.transform, smoke, 0.9f, 1.5f, 14);
                dust.transform.localPosition = Vector3.up * (ground + 0.2f);
                ParticleSystem.MainModule dustMain = dust.main;
                dustMain.startSpeed = new MinMaxCurve(1.5f, 4.5f);
                dustMain.startSize = new MinMaxCurve(1.1f, 1.9f);
                dustMain.startRotation = new MinMaxCurve(0f, Mathf.PI * 2);
                dustMain.startColor = new MinMaxGradient(dustColor, darkDustColor);
                dustMain.gravityModifier = -0.05f;
                ConeUp(dust, 70f, 0.6f);
                GrowAndFade(dust, 0.6f, 1.5f);

                ParticleSystem.LimitVelocityOverLifetimeModule limit = dust.limitVelocityOverLifetime;
                limit.enabled = true;
                limit.limit = 0.4f;
                limit.dampen = 0.12f;

                // Chunks of the stone, bouncing where there is ground to bounce on
                ParticleSystem debris = NewParticles("Debris", root.transform, material, 1.1f, 1.6f, 10);
                debris.transform.localPosition = Vector3.up * (ground + 0.3f);
                ParticleSystem.MainModule debrisMain = debris.main;
                debrisMain.startSpeed = new MinMaxCurve(4f, 8f);
                debrisMain.startSize = new MinMaxCurve(0.15f, 0.3f);
                debrisMain.startRotation3D = true;
                debrisMain.startRotationX = new MinMaxCurve(0f, Mathf.PI * 2);
                debrisMain.startRotationY = new MinMaxCurve(0f, Mathf.PI * 2);
                debrisMain.startRotationZ = new MinMaxCurve(0f, Mathf.PI * 2);
                debrisMain.gravityModifier = 2.2f;
                ConeUp(debris, 40f, 0.4f);

                ParticleSystem.RotationOverLifetimeModule spin = debris.rotationOverLifetime;
                spin.enabled = true;
                spin.separateAxes = true;
                spin.x = new MinMaxCurve(-8f, 8f);
                spin.y = new MinMaxCurve(-8f, 8f);
                spin.z = new MinMaxCurve(-8f, 8f);

                // Full size until near the end, then shrinks away instead of popping out
                ParticleSystem.SizeOverLifetimeModule debrisSize = debris.sizeOverLifetime;
                debrisSize.enabled = true;
                debrisSize.size = new MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.75f, 1f), new Keyframe(1f, 0f)));

                ParticleSystem.CollisionModule collision = debris.collision;
                collision.enabled = true;
                collision.type = ParticleSystemCollisionType.World;
                collision.mode = ParticleSystemCollisionMode.Collision3D;
                collision.dampen = 0.45f;
                collision.bounce = 0.3f;
                collision.radiusScale = 0.5f;
                collision.quality = ParticleSystemCollisionQuality.Medium;

                ParticleSystemRenderer debrisRenderer = debris.GetComponent<ParticleSystemRenderer>();
                debrisRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                debrisRenderer.mesh = debrisMesh;

                // Fine grit sprayed wider and faster than the chunks
                ParticleSystem grit = NewParticles("Grit", root.transform, point, 0.5f, 0.9f, 18);
                grit.transform.localPosition = Vector3.up * (ground + 0.2f);
                ParticleSystem.MainModule gritMain = grit.main;
                gritMain.startSpeed = new MinMaxCurve(3f, 7f);
                gritMain.startSize = new MinMaxCurve(0.08f, 0.18f);
                gritMain.startColor = new MinMaxGradient(new Color(0.55f, 0.45f, 0.33f), new Color(0.4f, 0.36f, 0.32f));
                gritMain.gravityModifier = 1.5f;
                ConeUp(grit, 50f, 0.3f);

                // A ring running out along the ground to the edge of the hit radius
                ParticleSystem shockwave = NewParticles("Shockwave", root.transform, ring, 0.45f, 0.45f, 1);
                shockwave.transform.localPosition = Vector3.up * (ground + 0.05f);
                ParticleSystem.MainModule shockwaveMain = shockwave.main;
                shockwaveMain.startSpeed = 0f;
                shockwaveMain.startSize = radius * 2f;
                shockwaveMain.startColor = new Color(0.85f, 0.78f, 0.65f, 0.7f);

                ParticleSystem.ShapeModule shockwaveShape = shockwave.shape;
                shockwaveShape.enabled = false;

                GrowAndFade(shockwave, 0.1f, 1f);
                shockwave.GetComponent<ParticleSystemRenderer>().renderMode = ParticleSystemRenderMode.HorizontalBillboard;

                return PrefabUtility.SaveAsPrefabAsset(root, hitEffectPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        // A puff of dust kicked up around the wheels as the arm hits the stop bar. Rebuilt on every bake
        private static ParticleSystem BakeLaunchDust(Transform rotatePart, Material smoke)
        {
            Transform existing = rotatePart.Find("LaunchDust");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            ParticleSystem launchDust = NewParticles("LaunchDust", rotatePart, smoke, 0.5f, 0.9f, 8);
            launchDust.transform.localPosition = Vector3.up * 0.1f;

            ParticleSystem.MainModule main = launchDust.main;
            main.playOnAwake = false;
            main.startSpeed = new MinMaxCurve(0.8f, 2f);
            main.startSize = new MinMaxCurve(0.6f, 1.1f);
            main.startRotation = new MinMaxCurve(0f, Mathf.PI * 2);
            main.startColor = new Color(0.7f, 0.64f, 0.55f, 0.6f);

            // A flat ring around the chassis, pushing outwards along the ground
            ParticleSystem.ShapeModule shape = launchDust.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            GrowAndFade(launchDust, 0.7f, 1.4f);

            return launchDust;
        }


        // A one shot system in world space; burst 0 leaves emission to the caller
        private static ParticleSystem NewParticles(string name, Transform parent, Material material, float minLifetime, float maxLifetime, int burst)
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
        private static void ConeUp(ParticleSystem particles, float angle, float radius)
        {
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = angle;
            shape.radius = radius;
            shape.rotation = new Vector3(-90f, 0f, 0f);
        }

        // Grows from one size factor to another while fading in fast and out slowly
        private static void GrowAndFade(ParticleSystem particles, float startSize, float endSize)
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


        // A stone tuned in the inspector keeps its numbers through a rebake
        private static (float damage, float speed, float radius) StoneStats()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(stonePrefabPath);
            CatapultStone stone = existing != null ? existing.GetComponent<CatapultStone>() : null;
            if (stone == null)
                return (defaultDamage, defaultSpeed, defaultRadius);

            SerializedObject serialized = new(stone);
            return (serialized.FindProperty("<Damage>k__BackingField").floatValue,
                    serialized.FindProperty("speed").floatValue,
                    serialized.FindProperty("explosionRadius").floatValue);
        }

        private static Texture2D BakePalette()
        {
            Texture2D texture = CatapultMeshBuilder.CreatePalette();
            File.WriteAllBytes(texturePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(texturePath);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.sRGBTexture = true;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        }

        // Matte like the castle and the tiles: the look comes from the palette, not from highlights
        private static Material BakeMaterial(Texture2D palette)
        {
            Material material = new(Shader.Find("Standard"))
            {
                color = Color.white,
                mainTexture = palette,
                enableInstancing = true
            };
            material.SetFloat("_Glossiness", 0f);
            material.SetFloat("_Metallic", 0f);

            return SaveAsset(material, materialPath);
        }

        private static Material LoadMaterial(string path)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
                Debug.LogError($"Particle material not found: {path}");

            return material;
        }

        // Child holding a mesh, reused when it is already there so the prefab keeps its objects
        private static Transform MeshPart(Transform parent, string name, Mesh mesh, Material material)
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
        private static Transform Child(Transform parent, string name)
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

        private static void KeepOnly(Transform parent, params string[] names)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (System.Array.IndexOf(names, child.name) < 0)
                    Object.DestroyImmediate(child.gameObject);
            }
        }

        // Creates the asset or overwrites the existing one in place, so references to it stay valid
        private static T SaveAsset<T>(T asset, string path) where T : Object
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

        private static string MeshPath(string part)
        {
            return $"{meshesFolder}/Catapult_{part}.asset";
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
