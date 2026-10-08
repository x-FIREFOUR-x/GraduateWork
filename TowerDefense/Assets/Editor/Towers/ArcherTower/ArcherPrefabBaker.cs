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
    // Bakes the elven archer tower: the palette texture and material, the part meshes, the arrow with its trail, the
    // hit effect (splinters, a puff of dust and white feather flecks), the tower prefab itself with the two archers and
    // their captain, and its shop icon.
    // The tower prefab is rebuilt in place, so the TowersStorage and the shop keep pointing at it; its price, range
    // and fire rate are left as they are. The arrow keeps the damage and speed it was given
    public static class ArcherPrefabBaker
    {
        private const string towerPrefabPath = "Assets/Prefabs/Towers/ArcherTower/ArcherTower.prefab";
        private const string arrowPrefabPath = "Assets/Prefabs/Towers/ArcherTower/ArcherArrow.prefab";
        private const string hitEffectPath = "Assets/Prefabs/Towers/ArcherTower/ArrowHitEffect.prefab";
        private const string meshesFolder = "Assets/Models/Towers/ArcherTower";
        private const string materialsFolder = "Assets/Materials/Tower/ArcherTower";
        private const string texturePath = "Assets/Textures/Towers/ArcherTowerPalette.png";
        private const string materialPath = materialsFolder + "/ArcherTower.mat";

        // Soft particle materials the tower building effect already uses
        private const string smokeMaterialPath = "Assets/Materials/Effects/BuildingTower/Smoke26.mat";
        private const string pointMaterialPath = "Assets/Materials/Effects/BuildingTower/Point.mat";

        // What a new arrow starts with. Two fly per volley, so the archers outdo the ballista a little, which pays
        // for their higher price
        private const float defaultDamage = 35f;
        private const float defaultSpeed = 70f;

        // The tower stands on the tile top, which lies this far above the tile centre the tower is put on
        private static readonly Vector3 offsetTower = new(0f, 0.5f, 0f);


        [MenuItem("Tools/Towers/Bake Archer Tower")]
        public static void Bake()
        {
            GameObject towerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(towerPrefabPath);
            if (towerPrefab == null || towerPrefab.GetComponent<ArcherTower>() == null)
            {
                Debug.LogError($"Archer tower prefab with an {nameof(ArcherTower)} not found: {towerPrefabPath}");
                return;
            }

            Material smoke = LoadMaterial(smokeMaterialPath);
            Material point = LoadMaterial(pointMaterialPath);
            if (smoke == null || point == null)
                return;

            EnsureFolder(meshesFolder);
            EnsureFolder(materialsFolder);

            Material material = BakePaletteMaterial(materialPath, BakePaletteTexture(texturePath, ArcherMeshBuilder.CreatePalette()));

            Meshes meshes = new()
            {
                Tower = SaveAsset(ArcherMeshBuilder.BuildTower(), MeshPath("Tower")),
                ArcherLeft = SaveAsset(ArcherMeshBuilder.BuildArcher(ArcherMeshBuilder.Palette.DarkWood), MeshPath("ArcherLeft")),
                ArcherRight = SaveAsset(ArcherMeshBuilder.BuildArcher(ArcherMeshBuilder.Palette.Blond), MeshPath("ArcherRight")),
                UpperArm = SaveAsset(ArcherMeshBuilder.BuildDrawUpperArm(), MeshPath("UpperArm")),
                Forearm = SaveAsset(ArcherMeshBuilder.BuildDrawForearm(), MeshPath("Forearm")),
                Captain = SaveAsset(ArcherMeshBuilder.BuildCaptain(), MeshPath("Captain")),
                CommandArm = SaveAsset(ArcherMeshBuilder.BuildCommandArm(), MeshPath("CommandArm")),
                String = SaveAsset(ArcherMeshBuilder.BuildString(), MeshPath("String")),
                Arrow = SaveAsset(ArcherMeshBuilder.BuildArrow(), MeshPath("Arrow")),
                Splinter = SaveAsset(ArcherMeshBuilder.BuildSplinter(), MeshPath("Splinter")),
                ArcherThighLeft = SaveAsset(ArcherMeshBuilder.BuildArcherThigh(-1f), MeshPath("ArcherThighLeft")),
                ArcherThighRight = SaveAsset(ArcherMeshBuilder.BuildArcherThigh(1f), MeshPath("ArcherThighRight")),
                ArcherShinLeft = SaveAsset(ArcherMeshBuilder.BuildArcherShin(-1f), MeshPath("ArcherShinLeft")),
                ArcherShinRight = SaveAsset(ArcherMeshBuilder.BuildArcherShin(1f), MeshPath("ArcherShinRight")),
                CaptainThighLeft = SaveAsset(ArcherMeshBuilder.BuildCaptainThigh(-1f), MeshPath("CaptainThighLeft")),
                CaptainThighRight = SaveAsset(ArcherMeshBuilder.BuildCaptainThigh(1f), MeshPath("CaptainThighRight")),
                CaptainShinLeft = SaveAsset(ArcherMeshBuilder.BuildCaptainShin(-1f), MeshPath("CaptainShinLeft")),
                CaptainShinRight = SaveAsset(ArcherMeshBuilder.BuildCaptainShin(1f), MeshPath("CaptainShinRight")),
            };

            (float damage, float speed) = ArrowStats();

            GameObject hitEffect = BakeHitEffect(material, meshes.Splinter, smoke, point);
            GameObject arrow = BakeArrow(material, meshes.Arrow, point, hitEffect, damage, speed);
            BakeTower(meshes, material, arrow);

            foreach (string legacy in new[] { "Platform", "DrawArm" })
            {
                if (AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath(legacy)) != null)
                    AssetDatabase.DeleteAsset(MeshPath(legacy));
            }

            AssetDatabase.SaveAssets();

            Debug.Log("Archer tower baked");
        }


        private class Meshes
        {
            public Mesh Tower, ArcherLeft, ArcherRight, UpperArm, Forearm, Captain, CommandArm, String, Arrow, Splinter;
            public Mesh ArcherThighLeft, ArcherThighRight, ArcherShinLeft, ArcherShinRight;
            public Mesh CaptainThighLeft, CaptainThighRight, CaptainShinLeft, CaptainShinRight;
        }

        private static void BakeTower(Meshes meshes, Material material, GameObject arrow)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(towerPrefabPath);
            try
            {
                root.name = Path.GetFileNameWithoutExtension(towerPrefabPath);

                // The tower stays put, the three elves turn on its deck
                MeshPart(root.transform, "Tower", meshes.Tower, material);

                Transform rotatePart = Child(root.transform, "RotatePart");
                rotatePart.localPosition = Vector3.up * ArcherMeshBuilder.PlatformHeight;

                Transform leftArcher = BakeArcher(rotatePart, "ArcherLeft", meshes.ArcherLeft, meshes, material, ArcherMeshBuilder.LeftArcherPosition);
                Transform rightArcher = BakeArcher(rotatePart, "ArcherRight", meshes.ArcherRight, meshes, material, ArcherMeshBuilder.RightArcherPosition);

                Transform captain = MeshPart(rotatePart, "Captain", meshes.Captain, material);
                captain.localPosition = ArcherMeshBuilder.CaptainPosition;
                captain.localScale = Vector3.one * ArcherMeshBuilder.CaptainScale;
                Transform commandArm = MeshPart(captain, "CommandArm", meshes.CommandArm, material);
                commandArm.localPosition = ArcherMeshBuilder.CommandShoulder;
                BakeLegs(captain, meshes.CaptainThighLeft, meshes.CaptainThighRight, meshes.CaptainShinLeft, meshes.CaptainShinRight, material,
                         ArcherMeshBuilder.CaptainHip, ArcherMeshBuilder.CaptainKnee);
                KeepOnly(captain, "CommandArm", "ThighLeft", "ThighRight");

                // Whatever else the prefab holds goes, the panels turret's model included
                KeepOnly(root.transform, "Tower", "RotatePart");
                KeepOnly(rotatePart, "ArcherLeft", "ArcherRight", "Captain");

                SerializedObject tower = new(root.GetComponent<ArcherTower>());
                tower.FindProperty("projectilePrefab").objectReferenceValue = arrow;
                SetArcher(tower.FindProperty("leftArcher"), leftArcher);
                SetArcher(tower.FindProperty("rightArcher"), rightArcher);
                SetFirePoints(tower.FindProperty("pointStartFire"), leftArcher.Find("PointStartFire"), rightArcher.Find("PointStartFire"));
                tower.FindProperty("bowTopTip").vector3Value = ArcherMeshBuilder.BowTopTip;
                tower.FindProperty("bowBottomTip").vector3Value = ArcherMeshBuilder.BowBottomTip;
                tower.FindProperty("arrowLength").floatValue = ArcherMeshBuilder.ArrowLength;
                tower.FindProperty("arrowRest").vector3Value = ArcherMeshBuilder.ArrowRest;
                tower.FindProperty("drawShoulder").vector3Value = ArcherMeshBuilder.DrawShoulder;
                tower.FindProperty("upperArmLength").floatValue = ArcherMeshBuilder.UpperArmLength;
                tower.FindProperty("forearmLength").floatValue = ArcherMeshBuilder.ForearmLength;
                tower.FindProperty("elbowRestAngle").floatValue = ArcherMeshBuilder.ElbowRestAngle;
                tower.FindProperty("elbowDrawnAngle").floatValue = ArcherMeshBuilder.ElbowDrawnAngle;
                tower.FindProperty("nockFromWrist").vector3Value = ArcherMeshBuilder.NockFromWrist;
                tower.FindProperty("commandRaisedAngle").floatValue = -100f;
                tower.FindProperty("captain").objectReferenceValue = captain;
                SetLegs(tower.FindProperty("leftArcherLegs"), leftArcher);
                SetLegs(tower.FindProperty("rightArcherLegs"), rightArcher);
                SetLegs(tower.FindProperty("captainLegs"), captain);
                tower.FindProperty("stepLength").floatValue = 0.42f;
                tower.FindProperty("legSwing").floatValue = 22f;
                tower.FindProperty("kneeBend").floatValue = 40f;
                tower.FindProperty("commandArm").objectReferenceValue = commandArm;
                tower.FindProperty("rotatePart").objectReferenceValue = rotatePart;
                tower.FindProperty("offsetTower").vector3Value = offsetTower;
                tower.ApplyModifiedPropertiesWithoutUndo();

                // The prefab and the icon show the archers drawn and the captain's sword raised, the way they stand ready
                Vector3 nock = DrawnNock(out _, out _);
                foreach (Transform archer in new[] { leftArcher, rightArcher })
                {
                    Stretch(archer.Find("StringTop"), ArcherMeshBuilder.BowTopTip, nock);
                    Stretch(archer.Find("StringBottom"), ArcherMeshBuilder.BowBottomTip, nock);
                }
                commandArm.localRotation = Quaternion.Euler(-100f, 0f, 0f);

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

        // An archer with his drawing arm, the two halves of his bowstring, the arrow on his bow and the point his shot
        // leaves from, all in his own space
        private static Transform BakeArcher(Transform rotatePart, string name, Mesh body, Meshes meshes, Material material, Vector3 position)
        {
            Transform archer = MeshPart(rotatePart, name, body, material);
            // Turned to his right as far as the drawn arrow lies across him, so it points straight ahead
            Vector3 drawn = (ArcherMeshBuilder.ArrowRest - DrawnNock(out _, out _)).normalized;
            archer.localPosition = position;
            archer.localRotation = Quaternion.Euler(0f, Mathf.Atan2(-drawn.x, drawn.z) * Mathf.Rad2Deg, 0f);
            archer.localScale = Vector3.one * ArcherMeshBuilder.ArcherScale;

            // The drawing arm in two bones, bent so the hand holds the drawn string
            Transform upperArm = MeshPart(archer, "UpperArm", meshes.UpperArm, material);
            Transform forearm = MeshPart(archer, "Forearm", meshes.Forearm, material);
            Vector3 nock = DrawnNock(out Vector3 elbow, out Vector3 aim);
            upperArm.localPosition = ArcherMeshBuilder.DrawShoulder;
            upperArm.localRotation = Quaternion.LookRotation(elbow - ArcherMeshBuilder.DrawShoulder, Vector3.up);
            forearm.localPosition = elbow;
            forearm.localRotation = Quaternion.LookRotation(aim, Vector3.up);
            Vector3 arrowDirection = (ArcherMeshBuilder.ArrowRest - nock).normalized;
            Vector3 arrowTip = nock + arrowDirection * ArcherMeshBuilder.ArrowLength;
            MeshPart(archer, "StringTop", meshes.String, material);
            MeshPart(archer, "StringBottom", meshes.String, material);
            Transform loadedArrow = MeshPart(archer, "LoadedArrow", meshes.Arrow, material);
            loadedArrow.localPosition = arrowTip;
            loadedArrow.localRotation = Quaternion.LookRotation(arrowDirection);
            Transform pointStartFire = Child(archer, "PointStartFire");
            pointStartFire.localPosition = arrowTip;
            pointStartFire.localRotation = Quaternion.LookRotation(arrowDirection);

            BakeLegs(archer, meshes.ArcherThighLeft, meshes.ArcherThighRight, meshes.ArcherShinLeft, meshes.ArcherShinRight, material,
                     ArcherMeshBuilder.ArcherHip, ArcherMeshBuilder.ArcherKnee);
            KeepOnly(archer, "ThighLeft", "ThighRight", "UpperArm", "Forearm", "StringTop", "StringBottom", "LoadedArrow", "PointStartFire");
            return archer;
        }

        // Where the drawing arm and the nock are with the bow fully drawn, worked out the way the tower does in play
        private static Vector3 DrawnNock(out Vector3 elbow, out Vector3 aim)
        {
            ArcherTower.DrawPose(ArcherMeshBuilder.DrawShoulder, ArcherMeshBuilder.UpperArmLength, ArcherMeshBuilder.ForearmLength,
                                 ArcherMeshBuilder.ElbowDrawnAngle, ArcherMeshBuilder.ArrowRest, ArcherMeshBuilder.NockFromWrist,
                                 out elbow, out aim, out Vector3 nock);
            return nock;
        }

        // Both legs of an elf: each thigh at its hip, its shin inside it at the knee
        private static void BakeLegs(Transform elf, Mesh thighLeft, Mesh thighRight, Mesh shinLeft, Mesh shinRight, Material material, Vector3 hip, Vector3 knee)
        {
            foreach ((string name, Mesh thighMesh, Mesh shinMesh, float side) in new[] { ("Left", thighLeft, shinLeft, -1f), ("Right", thighRight, shinRight, 1f) })
            {
                Transform thigh = MeshPart(elf, "Thigh" + name, thighMesh, material);
                thigh.localPosition = ArcherMeshBuilder.OnSide(hip, side);
                Transform shin = MeshPart(thigh, "Shin", shinMesh, material);
                shin.localPosition = ArcherMeshBuilder.OnSide(knee, side) - ArcherMeshBuilder.OnSide(hip, side);
                KeepOnly(thigh, "Shin");
            }
        }

        private static void SetLegs(SerializedProperty property, Transform elf)
        {
            property.FindPropertyRelative("thighLeft").objectReferenceValue = elf.Find("ThighLeft");
            property.FindPropertyRelative("shinLeft").objectReferenceValue = elf.Find("ThighLeft/Shin");
            property.FindPropertyRelative("thighRight").objectReferenceValue = elf.Find("ThighRight");
            property.FindPropertyRelative("shinRight").objectReferenceValue = elf.Find("ThighRight/Shin");
        }

        private static void SetArcher(SerializedProperty property, Transform archer)
        {
            property.FindPropertyRelative("body").objectReferenceValue = archer;
            property.FindPropertyRelative("upperArm").objectReferenceValue = archer.Find("UpperArm");
            property.FindPropertyRelative("forearm").objectReferenceValue = archer.Find("Forearm");
            property.FindPropertyRelative("stringTop").objectReferenceValue = archer.Find("StringTop");
            property.FindPropertyRelative("stringBottom").objectReferenceValue = archer.Find("StringBottom");
            property.FindPropertyRelative("loadedArrow").objectReferenceValue = archer.Find("LoadedArrow").gameObject;
        }

        private static void Stretch(Transform piece, Vector3 from, Vector3 to)
        {
            Vector3 span = to - from;
            piece.localPosition = from;
            piece.localRotation = Quaternion.LookRotation(span);
            piece.localScale = new Vector3(1f, 1f, span.magnitude);
        }

        private static GameObject BakeArrow(Material material, Mesh arrowMesh, Material point, GameObject hitEffect, float damage, float speed)
        {
            GameObject root = OpenPrefab(arrowPrefabPath);
            try
            {
                StraightProjectile arrow = GetOrAdd<StraightProjectile>(root);

                // As big as the one lying on the bow
                Transform graphic = MeshPart(root.transform, "Graphic", arrowMesh, material);
                graphic.localScale = Vector3.one * ArcherMeshBuilder.ArcherScale;

                // A faint streak behind the arrow, so its flight reads at its speed
                ParticleSystem trail = NewParticles("Trail", root.transform, point, 0.12f, 0.2f, 0);
                trail.transform.localPosition = Vector3.back * (ArcherMeshBuilder.ArrowLength * ArcherMeshBuilder.ArcherScale);
                ParticleSystem.MainModule main = trail.main;
                main.loop = true;
                main.duration = 5f;
                main.startSpeed = 0f;
                main.startSize = new MinMaxCurve(0.06f, 0.1f);
                main.startColor = new Color(0.95f, 0.95f, 0.88f, 0.4f);

                ParticleSystem.EmissionModule emission = trail.emission;
                emission.rateOverDistance = 6f;

                ParticleSystem.ShapeModule shape = trail.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.01f;

                GrowAndFade(trail, 1f, 0.3f);

                SerializedObject serialized = new(arrow);
                serialized.FindProperty("effectHitPrefab").objectReferenceValue = hitEffect;
                serialized.FindProperty("speed").floatValue = speed;
                serialized.FindProperty("<Damage>k__BackingField").floatValue = damage;
                serialized.FindProperty("trail").objectReferenceValue = trail;
                serialized.FindProperty("effectLifetime").floatValue = 1f;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                return PrefabUtility.SaveAsPrefabAsset(root, arrowPrefabPath);
            }
            finally
            {
                ClosePrefab(root);
            }
        }

        // The effect is put where the arrow hits, turned the way it flew: a few splinters of its shaft, white flecks of
        // its feathers drifting down, a little dust
        private static GameObject BakeHitEffect(Material material, Mesh splinterMesh, Material smoke, Material point)
        {
            GameObject root = OpenPrefab(hitEffectPath);
            try
            {
                ParticleSystem splinters = NewParticles("Splinters", root.transform, material, 0.4f, 0.7f, 4);
                ParticleSystem.MainModule splintersMain = splinters.main;
                splintersMain.startSpeed = new MinMaxCurve(2f, 4f);
                splintersMain.startSize = new MinMaxCurve(0.12f, 0.2f);
                splintersMain.startRotation3D = true;
                splintersMain.startRotationX = new MinMaxCurve(0f, Mathf.PI * 2);
                splintersMain.startRotationY = new MinMaxCurve(0f, Mathf.PI * 2);
                splintersMain.startRotationZ = new MinMaxCurve(0f, Mathf.PI * 2);
                splintersMain.gravityModifier = 1.6f;
                BackAndUp(splinters, 45f);

                ParticleSystem.RotationOverLifetimeModule spin = splinters.rotationOverLifetime;
                spin.enabled = true;
                spin.separateAxes = true;
                spin.x = new MinMaxCurve(-10f, 10f);
                spin.y = new MinMaxCurve(-10f, 10f);
                spin.z = new MinMaxCurve(-10f, 10f);

                ParticleSystemRenderer splintersRenderer = splinters.GetComponent<ParticleSystemRenderer>();
                splintersRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                splintersRenderer.mesh = splinterMesh;

                ParticleSystem feathers = NewParticles("Feathers", root.transform, point, 0.6f, 1f, 8);
                ParticleSystem.MainModule feathersMain = feathers.main;
                feathersMain.startSpeed = new MinMaxCurve(1f, 2.5f);
                feathersMain.startSize = new MinMaxCurve(0.05f, 0.1f);
                feathersMain.startColor = new MinMaxGradient(new Color(0.96f, 0.95f, 0.9f), new Color(0.8f, 0.78f, 0.72f));
                feathersMain.gravityModifier = 0.3f;
                BackAndUp(feathers, 60f);

                ParticleSystem dust = NewParticles("Dust", root.transform, smoke, 0.3f, 0.5f, 3);
                ParticleSystem.MainModule dustMain = dust.main;
                dustMain.startSpeed = new MinMaxCurve(0.4f, 1f);
                dustMain.startSize = new MinMaxCurve(0.3f, 0.5f);
                dustMain.startRotation = new MinMaxCurve(0f, Mathf.PI * 2);
                dustMain.startColor = new Color(0.7f, 0.66f, 0.56f, 0.5f);
                BackAndUp(dust, 60f);
                GrowAndFade(dust, 0.6f, 1.3f);

                return PrefabUtility.SaveAsPrefabAsset(root, hitEffectPath);
            }
            finally
            {
                ClosePrefab(root);
            }
        }

        // A cone opening back towards where the arrow came from, tipped upwards
        private static void BackAndUp(ParticleSystem particles, float angle)
        {
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = angle;
            shape.radius = 0.05f;
            shape.rotation = new Vector3(-30f, 180f, 0f);
        }


        // An arrow tuned in the inspector keeps its numbers through a rebake
        private static (float damage, float speed) ArrowStats()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(arrowPrefabPath);
            StraightProjectile arrow = existing != null ? existing.GetComponent<StraightProjectile>() : null;
            if (arrow == null)
                return (defaultDamage, defaultSpeed);

            SerializedObject serialized = new(arrow);
            return (serialized.FindProperty("<Damage>k__BackingField").floatValue, serialized.FindProperty("speed").floatValue);
        }

        private static string MeshPath(string part)
        {
            return $"{meshesFolder}/ArcherTower_{part}.asset";
        }

    }

}
