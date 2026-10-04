using System.IO;

using UnityEditor;
using UnityEngine;

using TowerDefense.Main.Towers;

using static TowerDefense.EditorTools.Towers.TowerBakeUtility;

using MinMaxCurve = UnityEngine.ParticleSystem.MinMaxCurve;
using MinMaxGradient = UnityEngine.ParticleSystem.MinMaxGradient;


namespace TowerDefense.EditorTools.Towers
{
    // Bakes the ice mage tower: the palette texture, a matte material, a glowing one for ice and runes and one for
    // the frost beam, the part meshes, the effects (the beam, a frost burst with its light where the beam hits, mist
    // round the foot of the tower and snow falling about it), the tower prefab itself and its shop icon.
    // The tower prefab is rebuilt in place, so the TowersStorage and the shop keep pointing at it; its price, range,
    // damage and slowing are left as they are
    public static class IceMagePrefabBaker
    {
        private const string towerPrefabPath = "Assets/Prefabs/Towers/IceMageTower.prefab";
        private const string meshesFolder = "Assets/Models/Towers/IceMageTower";
        private const string materialsFolder = "Assets/Materials/Tower/IceMageTower";
        private const string texturePath = "Assets/Textures/Towers/IceMageTowerPalette.png";
        private const string materialPath = materialsFolder + "/IceMageTower.mat";
        private const string glowMaterialPath = materialsFolder + "/IceMageTowerGlow.mat";
        private const string iceMaterialPath = materialsFolder + "/IceMageTowerIce.mat";
        private const string beamMaterialPath = materialsFolder + "/IceMageTowerBeam.mat";

        // Soft particle materials the tower building effect already uses
        private const string smokeMaterialPath = "Assets/Materials/Effects/BuildingTower/Smoke26.mat";
        private const string pointMaterialPath = "Assets/Materials/Effects/BuildingTower/Point.mat";

        // How strongly the ice glows: it multiplies the palette colour of the glowing parts
        private static readonly Color glowColor = new Color(0.55f, 0.8f, 1f, 1f) * 1.3f;
        private static readonly Color frostLightColor = new(0.55f, 0.8f, 1f);
        // The ice lights itself faintly, so its shaded faces stay icy blue rather than going grey
        private static readonly Color iceColor = new(0.24f, 0.3f, 0.36f, 1f);

        // The plinth stands on the tile top, which lies this far above the tile centre the tower is put on
        private static readonly Vector3 offsetTower = new(0f, 0.5f, 0f);

        // The tower is tall and fills its bounding box, so its icon is zoomed less than a low, wide one
        private const float iconZoom = 1.15f;


        [MenuItem("Tools/Towers/Bake Ice Mage Tower")]
        public static void Bake()
        {
            GameObject towerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(towerPrefabPath);
            if (towerPrefab == null || towerPrefab.GetComponent<IceMageTower>() == null)
            {
                Debug.LogError($"Ice mage tower prefab with an {nameof(IceMageTower)} not found: {towerPrefabPath}");
                return;
            }

            Material smoke = LoadMaterial(smokeMaterialPath);
            Material point = LoadMaterial(pointMaterialPath);
            if (smoke == null || point == null)
                return;

            EnsureFolder(meshesFolder);
            EnsureFolder(materialsFolder);

            Texture2D palette = BakePaletteTexture(texturePath, IceMageMeshBuilder.CreatePalette());
            Material material = BakePaletteMaterial(materialPath, palette);
            Material glowMaterial = BakePaletteMaterial(glowMaterialPath, palette, glowColor);
            Material iceMaterial = BakePaletteMaterial(iceMaterialPath, palette, iceColor);
            Material beamMaterial = BakeBeamMaterial();

            (Mesh tower, Mesh towerGlow, Mesh towerIce) = IceMageMeshBuilder.BuildTower();
            (Mesh mage, Mesh mageGlow) = IceMageMeshBuilder.BuildMage();
            Meshes meshes = new()
            {
                Tower = SaveAsset(tower, MeshPath("Tower")),
                TowerGlow = SaveAsset(towerGlow, MeshPath("TowerGlow")),
                TowerIce = SaveAsset(towerIce, MeshPath("TowerIce")),
                Mage = SaveAsset(mage, MeshPath("Mage")),
                MageGlow = SaveAsset(mageGlow, MeshPath("MageGlow")),
                StaffArm = SaveAsset(IceMageMeshBuilder.BuildStaffArm(), MeshPath("StaffArm")),
                StaffCrystal = SaveAsset(IceMageMeshBuilder.BuildStaffCrystal(), MeshPath("StaffCrystal")),
                Shards = SaveAsset(IceMageMeshBuilder.BuildShards(), MeshPath("Shards")),
                IceShard = SaveAsset(IceMageMeshBuilder.BuildIceShard(), MeshPath("IceShard")),
            };

            BakeTower(meshes, material, glowMaterial, iceMaterial, beamMaterial, smoke, point);

            AssetDatabase.SaveAssets();

            Debug.Log("Ice mage tower baked");
        }


        private class Meshes
        {
            public Mesh Tower, TowerGlow, TowerIce, Mage, MageGlow, StaffArm, StaffCrystal, Shards, IceShard;
        }

        private static void BakeTower(Meshes meshes, Material material, Material glowMaterial, Material iceMaterial, Material beamMaterial,
                                      Material smoke, Material point)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(towerPrefabPath);
            try
            {
                root.name = Path.GetFileNameWithoutExtension(towerPrefabPath);

                // The tower stands still; its glowing parts and its ice are meshes of their own for their materials
                MeshPart(root.transform, "Tower", meshes.Tower, material);
                MeshPart(root.transform, "TowerGlow", meshes.TowerGlow, glowMaterial);
                MeshPart(root.transform, "TowerIce", meshes.TowerIce, iceMaterial);

                Transform shards = MeshPart(root.transform, "Shards", meshes.Shards, glowMaterial);
                shards.localPosition = Vector3.up * IceMageMeshBuilder.ShardsHeight;

                // The mage turns towards the target on top of the tower, his staff arm and its crystal inside him
                Transform rotatePart = Child(root.transform, "RotatePart");
                rotatePart.localPosition = Vector3.up * IceMageMeshBuilder.MageStandHeight;

                Transform mage = MeshPart(rotatePart, "Mage", meshes.Mage, material);
                mage.localScale = Vector3.one * IceMageMeshBuilder.MageScale;
                MeshPart(mage, "MageGlow", meshes.MageGlow, glowMaterial);

                Transform staffArm = MeshPart(mage, "StaffArm", meshes.StaffArm, material);
                staffArm.localPosition = IceMageMeshBuilder.StaffShoulder;

                Transform staffCrystal = MeshPart(staffArm, "StaffCrystal", meshes.StaffCrystal, glowMaterial);
                staffCrystal.localPosition = IceMageMeshBuilder.StaffCrystalPosition;

                // The beam leaves from the middle of the crystal, wherever the crystal floats to
                Transform pointStartFire = Child(staffCrystal, "PointStartFire");

                Light crystalLight = BakeLight(staffCrystal, "CrystalLight", 3f, 1.2f);

                ParticleSystem hitEffect = BakeHitEffect(root.transform, glowMaterial, meshes.IceShard, smoke, point);
                Light hitLight = hitEffect.GetComponent<Light>();
                BakeFrostMist(root.transform, smoke);
                BakeSnowfall(root.transform, point);

                LineRenderer frostBeam = root.GetComponent<LineRenderer>();
                if (frostBeam == null)
                    frostBeam = root.AddComponent<LineRenderer>();
                SetUpBeam(frostBeam, beamMaterial);

                // Whatever else the prefab holds goes, the laser turret's model and effects included
                KeepOnly(root.transform, "Tower", "TowerGlow", "TowerIce", "Shards", "RotatePart", "FrostHit", "FrostMist", "Snowfall");
                KeepOnly(rotatePart, "Mage");
                KeepOnly(mage, "MageGlow", "StaffArm");
                KeepOnly(staffArm, "StaffCrystal");
                KeepOnly(staffCrystal, "PointStartFire", "CrystalLight");

                SerializedObject tower = new(root.GetComponent<IceMageTower>());
                tower.FindProperty("frostBeam").objectReferenceValue = frostBeam;
                tower.FindProperty("hitEffect").objectReferenceValue = hitEffect;
                tower.FindProperty("hitLight").objectReferenceValue = hitLight;
                tower.FindProperty("crystalLight").objectReferenceValue = crystalLight;
                tower.FindProperty("mage").objectReferenceValue = mage;
                tower.FindProperty("staffArm").objectReferenceValue = staffArm;
                tower.FindProperty("staffCrystal").objectReferenceValue = staffCrystal;
                tower.FindProperty("shards").objectReferenceValue = shards;
                tower.FindProperty("pointStartFire").objectReferenceValue = pointStartFire;
                tower.FindProperty("rotatePart").objectReferenceValue = rotatePart;
                tower.FindProperty("offsetTower").vector3Value = offsetTower;
                tower.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, towerPrefabPath);
                // Written before the icon, so a failing icon never costs the baked model
                AssetDatabase.SaveAssets();

                // Taken by the camera from the model just built, not from the prefab asset, which may still be
                // the one loaded before this bake
                TowerIconsBaker.BakeIcon(root, iconZoom);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // A stream of frost: bright and almost white in the middle of its length, icy blue towards the target,
        // fading in at the crystal. The tower script switches it on, places its ends and makes it flicker
        private static void SetUpBeam(LineRenderer beam, Material beamMaterial)
        {
            beam.sharedMaterial = beamMaterial;
            beam.positionCount = 2;
            beam.useWorldSpace = true;
            beam.widthCurve = new AnimationCurve(new Keyframe(0f, 0.12f), new Keyframe(0.15f, 0.24f), new Keyframe(1f, 0.16f));
            beam.widthMultiplier = 1f;

            Gradient colors = new();
            colors.SetKeys(new[] { new GradientColorKey(new Color(0.9f, 0.98f, 1f), 0f), new GradientColorKey(new Color(0.75f, 0.92f, 1f), 0.4f),
                                   new GradientColorKey(new Color(0.45f, 0.75f, 1f), 1f) },
                           new[] { new GradientAlphaKey(0.4f, 0f), new GradientAlphaKey(0.95f, 0.15f), new GradientAlphaKey(0.85f, 1f) });
            beam.colorGradient = colors;

            beam.numCapVertices = 4;
            beam.alignment = LineAlignment.View;
            beam.textureMode = LineTextureMode.Stretch;
            beam.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            beam.receiveShadows = false;
            beam.enabled = false;
        }

        // The burst of frost where the beam hits: icy mist spraying back along the beam, splinters of ice flung off
        // and sparkles, with a cold light. It loops while the beam is on; the tower plays and stops it and moves it
        // to the target. Rebuilt on every bake
        private static ParticleSystem BakeHitEffect(Transform root, Material glowMaterial, Mesh iceShard, Material smoke, Material point)
        {
            Remove(root, "FrostHit");

            ParticleSystem mist = NewParticles("FrostHit", root, smoke, 0.5f, 0.8f, 0);
            Looping(mist, 14f, playOnAwake: false);
            ParticleSystem.MainModule mistMain = mist.main;
            mistMain.startSpeed = new MinMaxCurve(1f, 2.5f);
            mistMain.startSize = new MinMaxCurve(0.5f, 1f);
            mistMain.startRotation = new MinMaxCurve(0f, Mathf.PI * 2);
            mistMain.startColor = new Color(0.78f, 0.9f, 1f, 0.5f);
            ParticleSystem.ShapeModule mistShape = mist.shape;
            mistShape.shapeType = ParticleSystemShapeType.Cone;
            mistShape.angle = 45f;
            mistShape.radius = 0.15f;
            GrowAndFade(mist, 0.5f, 1.6f);

            ParticleSystem splinters = NewParticles("Splinters", mist.transform, glowMaterial, 0.5f, 0.9f, 0);
            Looping(splinters, 8f, playOnAwake: false);
            ParticleSystem.MainModule splintersMain = splinters.main;
            splintersMain.startSpeed = new MinMaxCurve(2f, 4f);
            splintersMain.startSize = new MinMaxCurve(0.12f, 0.25f);
            splintersMain.startRotation3D = true;
            splintersMain.startRotationX = new MinMaxCurve(0f, Mathf.PI * 2);
            splintersMain.startRotationY = new MinMaxCurve(0f, Mathf.PI * 2);
            splintersMain.startRotationZ = new MinMaxCurve(0f, Mathf.PI * 2);
            splintersMain.gravityModifier = 0.8f;
            ParticleSystem.ShapeModule splintersShape = splinters.shape;
            splintersShape.shapeType = ParticleSystemShapeType.Cone;
            splintersShape.angle = 60f;
            splintersShape.radius = 0.1f;
            ParticleSystem.RotationOverLifetimeModule spin = splinters.rotationOverLifetime;
            spin.enabled = true;
            spin.separateAxes = true;
            spin.x = new MinMaxCurve(-6f, 6f);
            spin.y = new MinMaxCurve(-6f, 6f);
            spin.z = new MinMaxCurve(-6f, 6f);
            ParticleSystemRenderer splintersRenderer = splinters.GetComponent<ParticleSystemRenderer>();
            splintersRenderer.renderMode = ParticleSystemRenderMode.Mesh;
            splintersRenderer.mesh = iceShard;

            ParticleSystem sparkles = NewParticles("Sparkles", mist.transform, point, 0.3f, 0.6f, 0);
            Looping(sparkles, 30f, playOnAwake: false);
            ParticleSystem.MainModule sparklesMain = sparkles.main;
            sparklesMain.startSpeed = new MinMaxCurve(1f, 3f);
            sparklesMain.startSize = new MinMaxCurve(0.05f, 0.12f);
            sparklesMain.startColor = new MinMaxGradient(new Color(0.85f, 0.97f, 1f), new Color(0.55f, 0.8f, 1f));
            ParticleSystem.ShapeModule sparklesShape = sparkles.shape;
            sparklesShape.shapeType = ParticleSystemShapeType.Sphere;
            sparklesShape.radius = 0.3f;

            Light light = mist.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = frostLightColor;
            light.range = 5f;
            light.intensity = 2.5f;
            light.enabled = false;

            return mist;
        }

        // Cold mist drifting low round the foot of the tower, always on
        private static void BakeFrostMist(Transform root, Material smoke)
        {
            Remove(root, "FrostMist");

            ParticleSystem mist = NewParticles("FrostMist", root, smoke, 3f, 4.5f, 0);
            mist.transform.localPosition = Vector3.up * 0.15f;
            Looping(mist, 2.5f, playOnAwake: true);
            ParticleSystem.MainModule main = mist.main;
            main.startSpeed = new MinMaxCurve(0.05f, 0.25f);
            main.startSize = new MinMaxCurve(1.4f, 2.4f);
            main.startRotation = new MinMaxCurve(0f, Mathf.PI * 2);
            main.startColor = new Color(0.78f, 0.86f, 0.95f, 0.22f);
            main.gravityModifier = -0.01f;

            ParticleSystem.ShapeModule shape = mist.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1.8f;
            shape.rotation = new Vector3(-90f, 0f, 0f);

            GrowAndFade(mist, 0.6f, 1.3f);
        }

        // Snow drifting down about the tower, always on
        private static void BakeSnowfall(Transform root, Material point)
        {
            Remove(root, "Snowfall");

            ParticleSystem snow = NewParticles("Snowfall", root, point, 4f, 6f, 0);
            snow.transform.localPosition = Vector3.up * 6.5f;
            Looping(snow, 10f, playOnAwake: true);
            ParticleSystem.MainModule main = snow.main;
            main.startSpeed = 0f;
            main.startSize = new MinMaxCurve(0.04f, 0.09f);
            main.startColor = new Color(1f, 1f, 1f, 0.8f);
            main.gravityModifier = 0.04f;

            ParticleSystem.ShapeModule shape = snow.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(4f, 0.2f, 4f);

            // The flakes sway as they fall instead of dropping straight
            ParticleSystem.NoiseModule noise = snow.noise;
            noise.enabled = true;
            noise.strength = 0.3f;
            noise.frequency = 0.4f;
        }

        // Makes a one shot system loop at a steady rate, filled from the start when it plays on its own
        private static void Looping(ParticleSystem particles, float rate, bool playOnAwake)
        {
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.playOnAwake = playOnAwake;
            main.prewarm = playOnAwake;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = rate;
        }

        private static Light BakeLight(Transform parent, string name, float range, float intensity)
        {
            Transform holder = Child(parent, name);
            Light light = holder.GetComponent<Light>();
            if (light == null)
                light = holder.gameObject.AddComponent<Light>();

            light.type = LightType.Point;
            light.color = frostLightColor;
            light.range = range;
            light.intensity = intensity;

            return light;
        }

        // Additive, so the beam brightens what is behind it like light does; its colour comes from the line itself
        private static Material BakeBeamMaterial()
        {
            Shader additive = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Particles/Standard Unlit");
            Material material = new(additive);
            material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 0.5f));

            return SaveAsset(material, beamMaterialPath);
        }

        private static string MeshPath(string part)
        {
            return $"{meshesFolder}/IceMageTower_{part}.asset";
        }
    }

}
