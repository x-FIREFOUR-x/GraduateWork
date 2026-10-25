using System;
using System.IO;

using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using TowerDefense.Main.Enemies;
using TowerDefense.Main.UI.EnemyMenu;
using TowerDefense.Storage;
using TowerDefense.EditorTools.Icons;
using static TowerDefense.EditorTools.Baking.BakeUtility;

using MinMaxCurve = UnityEngine.ParticleSystem.MinMaxCurve;
using MinMaxGradient = UnityEngine.ParticleSystem.MinMaxGradient;
using Object = UnityEngine.Object;


namespace TowerDefense.EditorTools.Enemies
{
    public static class BatPrefabBaker
    {
        private const string templatePrefabPath = "Assets/Prefabs/Enemy/StandardEnemy.prefab";
        private const string batPrefabPath = "Assets/Prefabs/Enemy/Bat.prefab";
        private const string deathEffectPath = "Assets/Prefabs/Effects/Bat/BatDeath.prefab";
        private const string meshesFolder = "Assets/Models/Enemy/Bat";
        private const string materialsFolder = "Assets/Materials/Enemy/Bat";
        private const string texturePath = "Assets/Textures/Enemy/Bat/BatPalette.png";
        private const string materialPath = materialsFolder + "/Bat.mat";
        private const string glowMaterialPath = materialsFolder + "/BatGlow.mat";
        private const string storagePath = "Assets/Resources/" + nameof(EnemiesStorage) + ".asset";
        private const string shopScenePath = "Assets/Scenes/AttackerGameScene.unity";
        private const string shopCardPath = "Assets/Prefabs/UI/EnemyShopComponent.prefab";
        private const int shopPosition = 1;

        private const string smokeMaterialPath = "Assets/Materials/Effects/BuildingTower/Smoke26.mat";
        private const string pointMaterialPath = "Assets/Materials/Effects/BuildingTower/Point.mat";

        private static readonly Color glowColor = new Color(1f, 0.35f, 0.15f, 1f) * 1.6f;


        [MenuItem("Tools/Enemies/Bake Bat")]
        public static void Bake()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(batPrefabPath) == null && AssetDatabase.LoadAssetAtPath<GameObject>(templatePrefabPath) == null)
            {
                Debug.LogError($"Enemy prefab to start the bat from not found: {templatePrefabPath}");
                return;
            }

            Material smoke = LoadMaterial(smokeMaterialPath);
            Material point = LoadMaterial(pointMaterialPath);
            if (smoke == null || point == null)
                return;

            EnsureFolder(meshesFolder);
            EnsureFolder(materialsFolder);
            EnsureFolder(Path.GetDirectoryName(deathEffectPath).Replace('\\', '/'));

            Texture2D palette = BakePaletteTexture(texturePath, BatMeshBuilder.CreatePalette());
            Material material = BakePaletteMaterial(materialPath, palette);
            Material glowMaterial = BakePaletteMaterial(glowMaterialPath, palette, glowColor);

            (Mesh body, Mesh bodyGlow) = BatMeshBuilder.BuildBody();
            Meshes meshes = new()
            {
                Body = SaveAsset(body, MeshPath("Body")),
                BodyGlow = SaveAsset(bodyGlow, MeshPath("BodyGlow")),
                LeftWing = SaveAsset(BatMeshBuilder.BuildWingInner(-1f), MeshPath("LeftWing")),
                LeftWingOuter = SaveAsset(BatMeshBuilder.BuildWingOuter(-1f), MeshPath("LeftWingOuter")),
                RightWing = SaveAsset(BatMeshBuilder.BuildWingInner(1f), MeshPath("RightWing")),
                RightWingOuter = SaveAsset(BatMeshBuilder.BuildWingOuter(1f), MeshPath("RightWingOuter")),
                LeftEar = SaveAsset(BatMeshBuilder.BuildEar(-1f), MeshPath("LeftEar")),
                RightEar = SaveAsset(BatMeshBuilder.BuildEar(1f), MeshPath("RightEar")),
                Tail = SaveAsset(BatMeshBuilder.BuildTail(), MeshPath("Tail")),
                SwarmBat = SaveAsset(BatMeshBuilder.BuildSwarmBat(), MeshPath("SwarmBat")),
            };

            GameObject deathEffect = BakeDeathEffect(meshes.SwarmBat, material, smoke, point);
            Bat bat = BakeBat(meshes, material, glowMaterial, smoke, point, deathEffect);

            AddToStorage(bat.gameObject);
            AddShopCard(bat);

            AssetDatabase.SaveAssets();

            Debug.Log("Bat baked");
        }


        private class Meshes
        {
            public Mesh Body, BodyGlow, LeftWing, LeftWingOuter, RightWing, RightWingOuter, LeftEar, RightEar, Tail, SwarmBat;
        }

        private static Bat BakeBat(Meshes meshes, Material material, Material glowMaterial, Material smoke, Material point, GameObject deathEffect)
        {
            bool isNew = AssetDatabase.LoadAssetAtPath<GameObject>(batPrefabPath) == null;
            GameObject root = PrefabUtility.LoadPrefabContents(isNew ? templatePrefabPath : batPrefabPath);
            try
            {
                root.name = Path.GetFileNameWithoutExtension(batPrefabPath);

                Animator animator = root.GetComponent<Animator>();
                if (animator != null)
                    Object.DestroyImmediate(animator);

                Bat bat = ReplaceWithBat(root);

                Transform model = Child(root.transform, "Model");
                model.localScale = Vector3.one * BatMeshBuilder.ModelScale;

                Transform body = MeshPart(model, "Body", meshes.Body, material);
                MeshPart(body, "BodyGlow", meshes.BodyGlow, glowMaterial);

                Transform leftWing = MeshPart(body, "LeftWing", meshes.LeftWing, material);
                leftWing.localPosition = Mirror(BatMeshBuilder.WingShoulder);
                Transform leftWingOuter = MeshPart(leftWing, "LeftWingOuter", meshes.LeftWingOuter, material);
                leftWingOuter.localPosition = Mirror(BatMeshBuilder.WingWrist);

                Transform rightWing = MeshPart(body, "RightWing", meshes.RightWing, material);
                rightWing.localPosition = BatMeshBuilder.WingShoulder;
                Transform rightWingOuter = MeshPart(rightWing, "RightWingOuter", meshes.RightWingOuter, material);
                rightWingOuter.localPosition = BatMeshBuilder.WingWrist;

                Transform leftEar = MeshPart(body, "LeftEar", meshes.LeftEar, material);
                leftEar.localPosition = Mirror(BatMeshBuilder.EarBase);
                Transform rightEar = MeshPart(body, "RightEar", meshes.RightEar, material);
                rightEar.localPosition = BatMeshBuilder.EarBase;

                Transform tail = MeshPart(body, "Tail", meshes.Tail, material);
                tail.localPosition = BatMeshBuilder.TailBase;

                BakeHaze(body, smoke);
                BakeWingDust(body, point);

                KeepOnly(root.transform, "Model", "Health");
                KeepOnly(model, "Body");
                KeepOnly(body, "BodyGlow", "LeftWing", "RightWing", "LeftEar", "RightEar", "Tail", "Haze", "WingDust");
                KeepOnly(leftWing, "LeftWingOuter");
                KeepOnly(rightWing, "RightWingOuter");

                SerializedObject enemy = new(bat);
                enemy.FindProperty("model").objectReferenceValue = model;
                enemy.FindProperty("deathEffect").objectReferenceValue = deathEffect;
                enemy.FindProperty("<EnemyType>k__BackingField").intValue = (int)EnemyType.Bat;
                enemy.FindProperty("body").objectReferenceValue = body;
                enemy.FindProperty("leftWing").objectReferenceValue = leftWing;
                enemy.FindProperty("leftWingOuter").objectReferenceValue = leftWingOuter;
                enemy.FindProperty("rightWing").objectReferenceValue = rightWing;
                enemy.FindProperty("rightWingOuter").objectReferenceValue = rightWingOuter;
                enemy.FindProperty("leftEar").objectReferenceValue = leftEar;
                enemy.FindProperty("rightEar").objectReferenceValue = rightEar;
                enemy.FindProperty("tail").objectReferenceValue = tail;
                enemy.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, batPrefabPath);
                AssetDatabase.SaveAssets();

                EnemyIconsBaker.BakeIcon(root);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(batPrefabPath).GetComponent<Bat>();
        }

        private static Bat ReplaceWithBat(GameObject root)
        {
            Enemy enemy = root.GetComponent<Enemy>();
            if (enemy is Bat existing)
                return existing;

            string[] kept = { "healthBar", "fullHealthBar", "angleX" };
            SerializedObject from = new(enemy);

            Bat bat = root.AddComponent<Bat>();
            SerializedObject to = new(bat);
            foreach (string property in kept)
                to.CopyFromSerializedProperty(from.FindProperty(property));
            to.ApplyModifiedPropertiesWithoutUndo();

            Object.DestroyImmediate(enemy);

            return bat;
        }

        private static void BakeHaze(Transform body, Material smoke)
        {
            ParticleSystem haze = NewParticles("Haze", body, smoke, 0.8f, 1.3f, 0);
            Looping(haze, 7f, playOnAwake: true);
            ParticleSystem.MainModule main = haze.main;
            main.prewarm = false;
            main.startSpeed = new MinMaxCurve(0.05f, 0.2f);
            main.startSize = new MinMaxCurve(0.4f, 0.7f);
            main.startRotation = new MinMaxCurve(0f, Mathf.PI * 2);
            main.startColor = new Color(0.12f, 0.09f, 0.12f, 0.35f);

            ParticleSystem.ShapeModule shape = haze.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.25f;

            GrowAndFade(haze, 0.6f, 1.4f);
        }

        private static void BakeWingDust(Transform body, Material point)
        {
            ParticleSystem dust = NewParticles("WingDust", body, point, 0.6f, 1f, 0);
            Looping(dust, 10f, playOnAwake: true);
            ParticleSystem.MainModule main = dust.main;
            main.prewarm = false;
            main.startSpeed = 0f;
            main.startSize = new MinMaxCurve(0.03f, 0.06f);
            main.startColor = new MinMaxGradient(new Color(0.36f, 0.29f, 0.25f, 0.8f), new Color(0.2f, 0.16f, 0.15f, 0.8f));
            main.gravityModifier = 0.25f;

            ParticleSystem.ShapeModule shape = dust.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(2.4f, 0.05f, 0.5f);
        }

        private static GameObject BakeDeathEffect(Mesh swarmBat, Material material, Material smoke, Material point)
        {
            GameObject root = OpenPrefab(deathEffectPath);
            try
            {
                root.name = Path.GetFileNameWithoutExtension(deathEffectPath);

                ParticleSystem cloud = SetUpParticles(root, smoke, 1.3f, 1.7f, 10);
                ParticleSystem.MainModule cloudMain = cloud.main;
                cloudMain.startSpeed = new MinMaxCurve(0.5f, 1.5f);
                cloudMain.startSize = new MinMaxCurve(0.6f, 1.2f);
                cloudMain.startRotation = new MinMaxCurve(0f, Mathf.PI * 2);
                cloudMain.startColor = new Color(0.1f, 0.08f, 0.1f, 0.6f);
                cloudMain.stopAction = ParticleSystemStopAction.Destroy;
                ParticleSystem.ShapeModule cloudShape = cloud.shape;
                cloudShape.shapeType = ParticleSystemShapeType.Sphere;
                cloudShape.radius = 0.35f;
                GrowAndFade(cloud, 0.5f, 1.6f);

                ParticleSystem swarm = NewParticles("Swarm", root.transform, material, 0.7f, 1.1f, 14);
                ParticleSystem.MainModule swarmMain = swarm.main;
                swarmMain.startSpeed = new MinMaxCurve(2.5f, 4.5f);
                swarmMain.startSize = new MinMaxCurve(0.22f, 0.35f);
                swarmMain.gravityModifier = -0.2f;
                ParticleSystem.ShapeModule swarmShape = swarm.shape;
                swarmShape.shapeType = ParticleSystemShapeType.Sphere;
                swarmShape.radius = 0.3f;
                ParticleSystem.NoiseModule swarmNoise = swarm.noise;
                swarmNoise.enabled = true;
                swarmNoise.strength = 1.2f;
                swarmNoise.frequency = 1.5f;
                ParticleSystem.SizeOverLifetimeModule swarmSize = swarm.sizeOverLifetime;
                swarmSize.enabled = true;
                swarmSize.size = new MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.7f, 1f), new Keyframe(1f, 0f)));
                ParticleSystemRenderer swarmRenderer = swarm.GetComponent<ParticleSystemRenderer>();
                swarmRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                swarmRenderer.mesh = swarmBat;
                swarmRenderer.alignment = ParticleSystemRenderSpace.Velocity;

                ParticleSystem embers = NewParticles("Embers", root.transform, point, 0.4f, 0.8f, 12);
                ParticleSystem.MainModule embersMain = embers.main;
                embersMain.startSpeed = new MinMaxCurve(1.5f, 3f);
                embersMain.startSize = new MinMaxCurve(0.04f, 0.08f);
                embersMain.startColor = new MinMaxGradient(new Color(1f, 0.35f, 0.12f), new Color(1f, 0.6f, 0.25f));
                embersMain.gravityModifier = -0.1f;
                ParticleSystem.ShapeModule embersShape = embers.shape;
                embersShape.shapeType = ParticleSystemShapeType.Sphere;
                embersShape.radius = 0.2f;

                KeepOnly(root.transform, "Swarm", "Embers");

                PrefabUtility.SaveAsPrefabAsset(root, deathEffectPath);
            }
            finally
            {
                ClosePrefab(root);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(deathEffectPath);
        }

        private static void AddToStorage(GameObject bat)
        {
            EnemiesStorage storage = AssetDatabase.LoadAssetAtPath<EnemiesStorage>(storagePath);
            if (storage == null)
            {
                Debug.LogError($"Enemies storage not found: {storagePath}");
                return;
            }

            SerializedObject serialized = new(storage);
            SerializedProperty enemies = serialized.FindProperty("<Enemies>k__BackingField");
            int index = (int)EnemyType.Bat;
            if (enemies.arraySize <= index)
                enemies.arraySize = index + 1;
            enemies.GetArrayElementAtIndex(index).objectReferenceValue = bat;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddShopCard(Bat bat)
        {
            GameObject cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(shopCardPath);
            Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(EnemyIconsBaker.IconPath(bat.name));

            EditScene(shopScenePath, scene =>
            {
                EnemyShopComponent template = null;
                EnemyShopComponent card = null;
                foreach (GameObject sceneRoot in scene.GetRootGameObjects())
                {
                    foreach (EnemyShopComponent found in sceneRoot.GetComponentsInChildren<EnemyShopComponent>(true))
                    {
                        if (new SerializedObject(found).FindProperty("enemy").objectReferenceValue == bat)
                            card = found;
                        else
                            template = found;
                    }
                }

                if (template == null && card == null)
                {
                    Debug.LogError($"No enemy shop card in {shopScenePath} to put the bat card next to");
                    return;
                }

                if (card == null)
                {
                    GameObject cardObject = (GameObject)PrefabUtility.InstantiatePrefab(cardPrefab, template.transform.parent);
                    card = cardObject.GetComponent<EnemyShopComponent>();
                }

                card.transform.SetSiblingIndex(shopPosition);

                card.gameObject.name = "BatShopComponent";

                SerializedObject serializedCard = new(card);
                serializedCard.FindProperty("enemy").objectReferenceValue = bat;
                if (template != null)
                    serializedCard.FindProperty("enemyShopMenu").objectReferenceValue = new SerializedObject(template).FindProperty("enemyShopMenu").objectReferenceValue;
                serializedCard.ApplyModifiedPropertiesWithoutUndo();

                TextMeshProUGUI name = card.transform.Find("TextNameEnemy")?.GetComponent<TextMeshProUGUI>();
                if (name != null)
                {
                    name.text = "Bat";
                    PrefabUtility.RecordPrefabInstancePropertyModifications(name);
                }

                Image image = FindChild(card.transform, "ButtonImage")?.GetComponent<Image>();
                if (image != null && icon != null)
                {
                    image.sprite = icon;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(image);
                }

                EnemyShopMenu menu = (EnemyShopMenu)serializedCard.FindProperty("enemyShopMenu").objectReferenceValue;
                if (menu != null)
                {
                    SerializedObject serializedMenu = new(menu);
                    SerializedProperty cards = serializedMenu.FindProperty("EnemyShopComponents");
                    for (int i = cards.arraySize - 1; i >= 0; i--)
                    {
                        if (cards.GetArrayElementAtIndex(i).objectReferenceValue == card)
                            cards.DeleteArrayElementAtIndex(i);
                    }

                    int index = Mathf.Min(shopPosition, cards.arraySize);
                    cards.InsertArrayElementAtIndex(index);
                    cards.GetArrayElementAtIndex(index).objectReferenceValue = card;
                    serializedMenu.ApplyModifiedPropertiesWithoutUndo();
                }
            });
        }

        private static void EditScene(string path, Action<Scene> edit)
        {
            Scene scene = SceneManager.GetSceneByPath(path);
            bool openedHere = !scene.isLoaded;
            if (openedHere)
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            try
            {
                edit(scene);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                if (openedHere)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Transform FindChild(Transform parent, string name)
        {
            foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                    return child;
            }
            return null;
        }

        private static Vector3 Mirror(Vector3 point)
        {
            return new Vector3(-point.x, point.y, point.z);
        }

        private static string MeshPath(string part)
        {
            return $"{meshesFolder}/Bat_{part}.asset";
        }
    }

}
