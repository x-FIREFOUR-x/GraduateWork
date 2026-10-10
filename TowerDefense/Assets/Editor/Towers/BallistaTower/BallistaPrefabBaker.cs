using System.IO;

using UnityEditor;
using UnityEngine;

using TowerDefense.Main.Projectiles;
using TowerDefense.EditorTools.Icons;
using TowerDefense.Main.Towers;

using static TowerDefense.EditorTools.Towers.TowerBakeUtility;

using MinMaxCurve = UnityEngine.ParticleSystem.MinMaxCurve;
using MinMaxGradient = UnityEngine.ParticleSystem.MinMaxGradient;


namespace TowerDefense.EditorTools.Towers
{
    // Bakes the ballista tower: the palette texture and material, the part meshes, the bolt with its trail, the hit
    // effect (splinters, dust and grit), the tower prefab itself with the dwarf and its shop icon.
    // The tower prefab is rebuilt in place, so the TowersStorage and the shop keep pointing at it; its price, range
    // and fire rate are left as they are. The bolt keeps the damage and speed it was given
    public static class BallistaPrefabBaker
    {
        private const string towerPrefabPath = "Assets/Prefabs/Towers/BallistaTower/BallistaTower.prefab";
        private const string boltPrefabPath = "Assets/Prefabs/Towers/BallistaTower/BallistaBolt.prefab";
        private const string hitEffectPath = "Assets/Prefabs/Towers/BallistaTower/BoltHitEffect.prefab";
        private const string meshesFolder = "Assets/Models/Towers/BallistaTower";
        private const string materialsFolder = "Assets/Materials/Tower/BallistaTower";
        private const string texturePath = "Assets/Textures/Towers/BallistaTowerPalette.png";
        private const string materialPath = materialsFolder + "/BallistaTower.mat";

        // Soft particle materials the tower building effect already uses
        private const string smokeMaterialPath = "Assets/Materials/Effects/BuildingTower/Smoke26.mat";
        private const string pointMaterialPath = "Assets/Materials/Effects/BuildingTower/Point.mat";

        // What a new bolt starts with: the numbers of the turret's bullet, which the tower balance and the genetic
        // algorithm were tuned for
        private const float defaultDamage = 50f;
        private const float defaultSpeed = 70f;

        // The platform stands on the tile top, which lies this far above the tile centre the tower is put on
        private static readonly Vector3 offsetTower = new(0f, 0.5f, 0f);

        private static readonly Color dustColor = new(0.66f, 0.6f, 0.5f, 0.7f);


        [MenuItem("Tools/Towers/Bake Ballista")]
        public static void Bake()
        {
            GameObject towerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(towerPrefabPath);
            if (towerPrefab == null || towerPrefab.GetComponent<BallistaTower>() == null)
            {
                Debug.LogError($"Ballista tower prefab with a {nameof(BallistaTower)} not found: {towerPrefabPath}");
                return;
            }

            Material smoke = LoadMaterial(smokeMaterialPath);
            Material point = LoadMaterial(pointMaterialPath);
            if (smoke == null || point == null)
                return;

            EnsureFolder(meshesFolder);
            EnsureFolder(materialsFolder);

            Material material = BakePaletteMaterial(materialPath, BakePaletteTexture(texturePath, BallistaMeshBuilder.CreatePalette()));

            Meshes meshes = new()
            {
                Platform = SaveAsset(BallistaMeshBuilder.BuildPlatform(), MeshPath("Platform")),
                Ballista = SaveAsset(BallistaMeshBuilder.BuildBallista(), MeshPath("Ballista")),
                BowArmLeft = SaveAsset(BallistaMeshBuilder.BuildBowArm(-1f), MeshPath("BowArmLeft")),
                BowArmRight = SaveAsset(BallistaMeshBuilder.BuildBowArm(1f), MeshPath("BowArmRight")),
                String = SaveAsset(BallistaMeshBuilder.BuildCord(0.016f), MeshPath("String")),
                Rope = SaveAsset(BallistaMeshBuilder.BuildCord(0.022f), MeshPath("Rope")),
                Slider = SaveAsset(BallistaMeshBuilder.BuildSlider(), MeshPath("Slider")),
                Winch = SaveAsset(BallistaMeshBuilder.BuildWinch(), MeshPath("Winch")),
                Bolt = SaveAsset(BallistaMeshBuilder.BuildBolt(), MeshPath("Bolt")),
                Splinter = SaveAsset(BallistaMeshBuilder.BuildSplinter(), MeshPath("Splinter")),
                Dwarf = SaveAsset(BallistaMeshBuilder.BuildDwarf(), MeshPath("Dwarf")),
                DwarfArmLeft = SaveAsset(BallistaMeshBuilder.BuildDwarfArm(-1f), MeshPath("DwarfArmLeft")),
                DwarfArmRight = SaveAsset(BallistaMeshBuilder.BuildDwarfArm(1f), MeshPath("DwarfArmRight")),
            };

            (float damage, float speed) = BoltStats();

            GameObject hitEffect = BakeHitEffect(material, meshes.Splinter, smoke, point);
            GameObject bolt = BakeBolt(material, meshes.Bolt, point, hitEffect, damage, speed);
            BakeTower(meshes, material, smoke, bolt);

            AssetDatabase.SaveAssets();

            Debug.Log("Ballista baked");
        }


        private class Meshes
        {
            public Mesh Platform, Ballista, BowArmLeft, BowArmRight, String, Rope, Slider, Winch, Bolt, Splinter, Dwarf, DwarfArmLeft, DwarfArmRight;
        }

        private static void BakeTower(Meshes meshes, Material material, Material smoke, GameObject bolt)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(towerPrefabPath);
            try
            {
                root.name = Path.GetFileNameWithoutExtension(towerPrefabPath);

                // The platform stays put, the ballista and the dwarf turn on top of it
                MeshPart(root.transform, "Platform", meshes.Platform, material);

                Transform rotatePart = Child(root.transform, "RotatePart");
                rotatePart.localPosition = Vector3.up * BallistaMeshBuilder.PlatformHeight;

                // The ballista kicks back on the shot with all its moving parts
                Transform ballista = MeshPart(rotatePart, "Ballista", meshes.Ballista, material);
                ballista.localScale = Vector3.one * BallistaMeshBuilder.BallistaScale;

                Transform bowArmLeft = MeshPart(ballista, "BowArmLeft", meshes.BowArmLeft, material);
                bowArmLeft.localPosition = BallistaMeshBuilder.LeftBowPivot;
                Transform bowArmRight = MeshPart(ballista, "BowArmRight", meshes.BowArmRight, material);
                bowArmRight.localPosition = BallistaMeshBuilder.RightBowPivot;

                Transform stringLeft = MeshPart(ballista, "StringLeft", meshes.String, material);
                Transform stringRight = MeshPart(ballista, "StringRight", meshes.String, material);
                Transform winchRope = MeshPart(ballista, "WinchRope", meshes.Rope, material);

                Transform slider = MeshPart(ballista, "Slider", meshes.Slider, material);
                slider.localPosition = new Vector3(0f, BallistaMeshBuilder.StringHeight, BallistaMeshBuilder.SliderDrawnZ);

                Transform winch = MeshPart(ballista, "Winch", meshes.Winch, material);
                winch.localPosition = BallistaMeshBuilder.WinchCenter;

                Transform loadedBolt = MeshPart(ballista, "LoadedBolt", meshes.Bolt, material);
                loadedBolt.localPosition = BallistaMeshBuilder.LoadedBoltTip;
                loadedBolt.localScale = Vector3.one * (BallistaMeshBuilder.BoltScale / BallistaMeshBuilder.BallistaScale);

                Transform pointStartFire = Child(ballista, "PointStartFire");
                pointStartFire.localPosition = BallistaMeshBuilder.LoadedBoltTip;

                ParticleSystem releaseDust = BakeReleaseDust(ballista, smoke);

                // The dwarf at the windlass, his arms turning inside him
                Transform dwarf = MeshPart(rotatePart, "Dwarf", meshes.Dwarf, material);
                dwarf.localPosition = BallistaMeshBuilder.DwarfPosition;
                dwarf.localRotation = Quaternion.Euler(0f, BallistaMeshBuilder.DwarfYaw, 0f);
                dwarf.localScale = Vector3.one * BallistaMeshBuilder.DwarfScale;

                Transform dwarfArmLeft = MeshPart(dwarf, "ArmLeft", meshes.DwarfArmLeft, material);
                dwarfArmLeft.localPosition = BallistaMeshBuilder.DwarfLeftShoulder;
                Transform dwarfArmRight = MeshPart(dwarf, "ArmRight", meshes.DwarfArmRight, material);
                dwarfArmRight.localPosition = BallistaMeshBuilder.DwarfRightShoulder;

                // Whatever else the prefab holds goes, the turret's model included
                KeepOnly(root.transform, "Platform", "RotatePart");
                KeepOnly(rotatePart, "Ballista", "Dwarf");
                KeepOnly(ballista, "BowArmLeft", "BowArmRight", "StringLeft", "StringRight", "WinchRope", "Slider", "Winch", "LoadedBolt", "PointStartFire",
                         "ReleaseDust");
                KeepOnly(dwarf, "ArmLeft", "ArmRight");

                SerializedObject tower = new(root.GetComponent<BallistaTower>());
                tower.FindProperty("projectilePrefab").objectReferenceValue = bolt;
                tower.FindProperty("ballista").objectReferenceValue = ballista;
                tower.FindProperty("bowArmLeft").objectReferenceValue = bowArmLeft;
                tower.FindProperty("bowArmRight").objectReferenceValue = bowArmRight;
                tower.FindProperty("stringLeft").objectReferenceValue = stringLeft;
                tower.FindProperty("stringRight").objectReferenceValue = stringRight;
                tower.FindProperty("winchRope").objectReferenceValue = winchRope;
                tower.FindProperty("slider").objectReferenceValue = slider;
                tower.FindProperty("winch").objectReferenceValue = winch;
                tower.FindProperty("loadedBolt").objectReferenceValue = loadedBolt.gameObject;
                tower.FindProperty("releaseDust").objectReferenceValue = releaseDust;
                tower.FindProperty("bowArmLength").floatValue = BallistaMeshBuilder.BowArmLength;
                tower.FindProperty("bowReleasedAngle").floatValue = BallistaMeshBuilder.BowReleasedAngle;
                tower.FindProperty("bowDrawnAngle").floatValue = BallistaMeshBuilder.BowDrawnAngle;
                tower.FindProperty("sliderReleasedZ").floatValue = BallistaMeshBuilder.SliderReleasedZ;
                tower.FindProperty("sliderDrawnZ").floatValue = BallistaMeshBuilder.SliderDrawnZ;
                tower.FindProperty("ropeAnchor").vector3Value = BallistaMeshBuilder.RopeAnchor;
                tower.FindProperty("winchTurns").floatValue = BallistaMeshBuilder.WinchTurns;
                // Kept small, so neither the kick of the ballista nor his leaning into the cranks brings the two together
                tower.FindProperty("recoilDistance").floatValue = 0.12f;
                tower.FindProperty("crankLean").floatValue = 4f;
                tower.FindProperty("dwarf").objectReferenceValue = dwarf;
                tower.FindProperty("dwarfArmLeft").objectReferenceValue = dwarfArmLeft;
                tower.FindProperty("dwarfArmRight").objectReferenceValue = dwarfArmRight;
                SetFirePoints(tower.FindProperty("pointStartFire"), pointStartFire);
                tower.FindProperty("rotatePart").objectReferenceValue = rotatePart;
                tower.FindProperty("offsetTower").vector3Value = offsetTower;
                tower.ApplyModifiedPropertiesWithoutUndo();

                // The strings and the rope are laid out for the drawn ballista, the way it stands loaded
                LayOutDrawn(ballista, bowArmLeft, bowArmRight, stringLeft, stringRight, winchRope, slider);

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

        // The pose the tower takes in Awake, so the prefab and the icon show the ballista drawn and loaded
        private static void LayOutDrawn(Transform ballista, Transform bowArmLeft, Transform bowArmRight, Transform stringLeft, Transform stringRight,
                                        Transform winchRope, Transform slider)
        {
            bowArmLeft.localRotation = Quaternion.Euler(0f, -BallistaMeshBuilder.BowDrawnAngle, 0f);
            bowArmRight.localRotation = Quaternion.Euler(0f, BallistaMeshBuilder.BowDrawnAngle, 0f);

            Vector3 claw = slider.localPosition;
            Stretch(stringLeft, bowArmLeft.localPosition + bowArmLeft.localRotation * (Vector3.left * BallistaMeshBuilder.BowArmLength), claw);
            Stretch(stringRight, bowArmRight.localPosition + bowArmRight.localRotation * (Vector3.right * BallistaMeshBuilder.BowArmLength), claw);
            Stretch(winchRope, BallistaMeshBuilder.RopeAnchor, claw + Vector3.back * 0.3f);
        }

        private static void Stretch(Transform piece, Vector3 from, Vector3 to)
        {
            Vector3 span = to - from;
            piece.localPosition = from;
            piece.localRotation = Quaternion.LookRotation(span);
            piece.localScale = new Vector3(1f, 1f, span.magnitude);
        }

        private static GameObject BakeBolt(Material material, Mesh boltMesh, Material point, GameObject hitEffect, float damage, float speed)
        {
            GameObject root = OpenPrefab(boltPrefabPath);
            try
            {
                StraightProjectile bolt = GetOrAdd<StraightProjectile>(root);
                // As big as the one lying in the groove
                Transform graphic = MeshPart(root.transform, "Graphic", boltMesh, material);
                graphic.localScale = Vector3.one * BallistaMeshBuilder.BoltScale;

                // A faint streak behind the bolt, so its flight reads at its speed
                ParticleSystem trail = NewParticles("Trail", root.transform, point, 0.15f, 0.25f, 0);
                trail.transform.localPosition = Vector3.back * (BallistaMeshBuilder.BoltLength * BallistaMeshBuilder.BoltScale);
                ParticleSystem.MainModule main = trail.main;
                main.loop = true;
                main.duration = 5f;
                main.startSpeed = 0f;
                main.startSize = new MinMaxCurve(0.12f, 0.2f);
                main.startColor = new Color(0.9f, 0.88f, 0.82f, 0.45f);

                ParticleSystem.EmissionModule emission = trail.emission;
                emission.rateOverDistance = 5f;

                ParticleSystem.ShapeModule shape = trail.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.02f;

                GrowAndFade(trail, 1f, 0.3f);

                SerializedObject serialized = new(bolt);
                serialized.FindProperty("effectHitPrefab").objectReferenceValue = hitEffect;
                serialized.FindProperty("speed").floatValue = speed;
                serialized.FindProperty("<Damage>k__BackingField").floatValue = damage;
                serialized.FindProperty("trail").objectReferenceValue = trail;
                serialized.FindProperty("effectLifetime").floatValue = 1.5f;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                return PrefabUtility.SaveAsPrefabAsset(root, boltPrefabPath);
            }
            finally
            {
                ClosePrefab(root);
            }
        }

        // The effect is put where the bolt hits, turned the way it flew: splinters of its shaft and grit spray back
        // and up from there, a little dust hangs a moment
        private static GameObject BakeHitEffect(Material material, Mesh splinterMesh, Material smoke, Material point)
        {
            GameObject root = OpenPrefab(hitEffectPath);
            try
            {
                ParticleSystem splinters = NewParticles("Splinters", root.transform, material, 0.5f, 0.8f, 7);
                ParticleSystem.MainModule splintersMain = splinters.main;
                splintersMain.startSpeed = new MinMaxCurve(3f, 6f);
                splintersMain.startSize = new MinMaxCurve(0.2f, 0.35f);
                splintersMain.startRotation3D = true;
                splintersMain.startRotationX = new MinMaxCurve(0f, Mathf.PI * 2);
                splintersMain.startRotationY = new MinMaxCurve(0f, Mathf.PI * 2);
                splintersMain.startRotationZ = new MinMaxCurve(0f, Mathf.PI * 2);
                splintersMain.gravityModifier = 1.8f;
                BackAndUp(splinters, 45f);

                ParticleSystem.RotationOverLifetimeModule spin = splinters.rotationOverLifetime;
                spin.enabled = true;
                spin.separateAxes = true;
                spin.x = new MinMaxCurve(-10f, 10f);
                spin.y = new MinMaxCurve(-10f, 10f);
                spin.z = new MinMaxCurve(-10f, 10f);

                ParticleSystem.SizeOverLifetimeModule splintersSize = splinters.sizeOverLifetime;
                splintersSize.enabled = true;
                splintersSize.size = new MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.7f, 1f), new Keyframe(1f, 0f)));

                ParticleSystemRenderer splintersRenderer = splinters.GetComponent<ParticleSystemRenderer>();
                splintersRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                splintersRenderer.mesh = splinterMesh;

                ParticleSystem dust = NewParticles("Dust", root.transform, smoke, 0.4f, 0.7f, 5);
                ParticleSystem.MainModule dustMain = dust.main;
                dustMain.startSpeed = new MinMaxCurve(0.5f, 1.5f);
                dustMain.startSize = new MinMaxCurve(0.5f, 0.9f);
                dustMain.startRotation = new MinMaxCurve(0f, Mathf.PI * 2);
                dustMain.startColor = dustColor;
                BackAndUp(dust, 60f);
                GrowAndFade(dust, 0.6f, 1.4f);

                ParticleSystem grit = NewParticles("Grit", root.transform, point, 0.3f, 0.6f, 10);
                ParticleSystem.MainModule gritMain = grit.main;
                gritMain.startSpeed = new MinMaxCurve(3f, 6f);
                gritMain.startSize = new MinMaxCurve(0.06f, 0.12f);
                gritMain.startColor = new MinMaxGradient(new Color(0.55f, 0.42f, 0.3f), new Color(0.4f, 0.36f, 0.32f));
                gritMain.gravityModifier = 1.5f;
                BackAndUp(grit, 55f);

                return PrefabUtility.SaveAsPrefabAsset(root, hitEffectPath);
            }
            finally
            {
                ClosePrefab(root);
            }
        }

        // A cone opening back towards where the bolt came from, tipped upwards
        private static void BackAndUp(ParticleSystem particles, float angle)
        {
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = angle;
            shape.radius = 0.1f;
            shape.rotation = new Vector3(-30f, 180f, 0f);
        }

        // A puff of dust shaken off the front frame as the bow snaps forward
        private static ParticleSystem BakeReleaseDust(Transform ballista, Material smoke)
        {
            ParticleSystem releaseDust = NewParticles("ReleaseDust", ballista, smoke, 0.4f, 0.7f, 6);
            releaseDust.transform.localPosition = new Vector3(0f, BallistaMeshBuilder.StringHeight, BallistaMeshBuilder.FrameZ + 0.2f);

            ParticleSystem.MainModule main = releaseDust.main;
            main.playOnAwake = false;
            main.startSpeed = new MinMaxCurve(0.8f, 2f);
            main.startSize = new MinMaxCurve(0.35f, 0.6f);
            main.startRotation = new MinMaxCurve(0f, Mathf.PI * 2);
            main.startColor = new Color(0.72f, 0.66f, 0.56f, 0.55f);

            // Spreading out sideways across the front, as wide as the bow
            ParticleSystem.ShapeModule shape = releaseDust.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(2.4f, 0.3f, 0.2f);

            GrowAndFade(releaseDust, 0.6f, 1.3f);

            return releaseDust;
        }


        // A bolt tuned in the inspector keeps its numbers through a rebake
        private static (float damage, float speed) BoltStats()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(boltPrefabPath);
            StraightProjectile bolt = existing != null ? existing.GetComponent<StraightProjectile>() : null;
            if (bolt == null)
                return (defaultDamage, defaultSpeed);

            SerializedObject serialized = new(bolt);
            return (serialized.FindProperty("<Damage>k__BackingField").floatValue, serialized.FindProperty("speed").floatValue);
        }

        private static string MeshPath(string part)
        {
            return $"{meshesFolder}/BallistaTower_{part}.asset";
        }

    }

}
