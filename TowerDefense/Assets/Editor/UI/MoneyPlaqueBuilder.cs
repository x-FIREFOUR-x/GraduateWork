using System.Collections.Generic;

using TMPro;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using TowerDefense.Main.UI;

using static TowerDefense.EditorTools.UI.SpriteRaster;
using static TowerDefense.EditorTools.UI.UILayout;


namespace TowerDefense.EditorTools.UI
{
    // Builds the plaque at the top of the shop that shows the player's money: a dark elven plaque with a golden vine
    // over it, and a leather pouch full of gold with a few coins spilled beside it. The sprites are drawn here, the
    // MoneyPlaque prefab is laid out from them and put into the game scenes in place of the plain money text
    public static class MoneyPlaqueBuilder
    {
        private const string prefabPath = "Assets/Prefabs/UI/MoneyPlaque.prefab";
        private const string spriteFolder = "Assets/Sprites/UI/Money/";
        private static readonly string[] scenePaths = { "Assets/Scenes/DefenderGameScene.unity", "Assets/Scenes/AttackerGameScene.unity" };
        private const string shopName = "MenuShop";
        private const string oldMoneyName = "CountMoney";
        // RenderUI wrote the old money text from here; its script is gone, and so is the component it left behind
        private const string managerName = "UIMenuManager";

        // The drawings are in their own units, the plaque shown as wide as the shop
        private static readonly Rect plaqueArea = new(0f, -2f, 160f, 56f);
        private static readonly Rect pouchArea = new(4f, 14f, 46f, 38f);
        private const float shownScale = 150f / 160f;
        private const float pixels = 3f;

        private static readonly Color plaqueColor = Hex("141f17");
        private static readonly Color gold = Hex("e2c172");
        private static readonly Color darkGold = Hex("8a6a2e");
        private static readonly Color goldEdge = Hex("5a3e12");
        private static readonly Color ink = Hex("f3e6c0");
        private static readonly Color leatherEdge = Hex("1e1006");
        private static readonly Color coinEdge = Hex("4a3010");
        private static readonly Color coinRing = Hex("9a6a1a");

        private static readonly (float, Color)[] polishedGold =
        {
            (0f, Hex("fff2c0")), (0.35f, Hex("e6bf62")), (0.62f, Hex("9a6e22")), (1f, Hex("f0d488")),
        };
        private static readonly (float, Color)[] coinFace = { (0f, Hex("fff8dc")), (0.45f, Hex("ecc25a")), (1f, Hex("8a5a12")) };
        private static readonly (float, Color)[] coinSide = { (0f, Hex("f4d070")), (1f, Hex("8a5a12")) };
        private static readonly (float, Color)[] leather = { (0f, Hex("cf9a60")), (0.55f, Hex("84512a")), (1f, Hex("3a1e0c")) };
        private static readonly (float, Color)[] neckLeather = { (0f, Hex("6a3e1a")), (0.45f, Hex("b07440")), (1f, Hex("5a3216")) };
        private static readonly (float, Color)[] pouchInside = { (0f, Hex("140a03")), (1f, Hex("3a2010")) };


        [MenuItem("Tools/UI/Build Money Plaque")]
        public static void Build()
        {
            Sprite plaque = BakePlaque();
            Sprite pouch = BakePouch();

            BuildPrefab(plaque, pouch);
            foreach (string scenePath in scenePaths)
                PlaceInScene(scenePath);

            AssetDatabase.SaveAssets();
            Debug.Log("Money plaque built");
        }


        private static Sprite BakePlaque()
        {
            SpriteRaster raster = new(plaqueArea, pixels);

            List<Vector2> frame = Points(14, 6, 156, 6, 160, 10, 160, 42);
            Quad(frame, new Vector2(160, 50), new Vector2(152, 50));
            frame.AddRange(Points(8, 50, 4, 46, 4, 14));
            Quad(frame, new Vector2(4, 6), new Vector2(14, 6));
            raster.Fill(frame, Solid(plaqueColor));
            raster.Stroke(frame, 1.6f, Vertical(6f, 50f, polishedGold), true);

            List<Vector2> inner = Points(15, 9.5f, 154.5f, 9.5f, 156.5f, 11.5f, 156.5f, 41);
            Quad(inner, new Vector2(156.5f, 46.5f), new Vector2(151, 46.5f));
            inner.AddRange(Points(9.5f, 46.5f, 7.5f, 44.5f, 7.5f, 15));
            Quad(inner, new Vector2(7.5f, 9.5f), new Vector2(15, 9.5f));
            raster.Stroke(inner, 0.7f, Solid(darkGold), true);

            // The vine along the top, with a leaf at each step growing away from the plaque or into it
            float VineY(float x) => 4f + Mathf.Sin(x / 23f * Mathf.PI * 2f) * 2.5f;
            List<Vector2> vine = new();
            for (float x = 56f; x <= 150f; x += 1f)
                vine.Add(new Vector2(x, VineY(x)));
            raster.Stroke(vine, 1f, Vertical(1.5f, 6.5f, polishedGold));

            for (float x = 62f; x < 146f; x += 11.5f)
            {
                float y = VineY(x);
                float up = Mathf.Cos(x / 23f * Mathf.PI * 2f) > 0f ? -1f : 1f;
                List<Vector2> leaf = new() { new Vector2(x, y) };
                Quad(leaf, new Vector2(x + 2f, y - 4f * up), new Vector2(x + 6f, y - 3.5f * up));
                Quad(leaf, new Vector2(x + 4f, y - 0.5f * up), new Vector2(x, y));
                Rect bounds = BoundsOf(leaf, 0f);
                raster.Fill(leaf, Vertical(bounds.yMin, bounds.yMax, polishedGold));
                raster.Stroke(leaf, 0.4f, Solid(goldEdge), true);
            }

            // A line under the sum with a diamond at its end
            raster.Stroke(Points(70, 42, 114, 42), 0.6f, Solid(darkGold));
            raster.Fill(Points(114, 42, 116, 40.4f, 118, 42, 116, 43.6f), Solid(gold));

            return raster.Save(spriteFolder + "MoneyPlaque.png", 100f);
        }

        private static Sprite BakePouch()
        {
            SpriteRaster r = new(pouchArea, pixels);

            Vector2 shadowCentre = new(26f, 49.5f);
            Vector2 shadowRadius = new(16f, 2f);
            r.Draw(p => EllipseDistance(p, shadowCentre, shadowRadius), EllipseBounds(shadowCentre, shadowRadius), Solid(Color.black), 0.38f);
            TiltedCoin(r, 9.5f, 48.6f, 2.8f, 1.1f, -5f);

            List<Vector2> body = Points(18.8f, 31.8f);
            Cubic(body, new Vector2(12, 33.6f), new Vector2(7.8f, 38), new Vector2(8.6f, 42.6f));
            Cubic(body, new Vector2(9.4f, 47.4f), new Vector2(15, 49.4f), new Vector2(24, 49.4f));
            Cubic(body, new Vector2(33, 49.4f), new Vector2(38.6f, 47.4f), new Vector2(39.4f, 42.6f));
            Cubic(body, new Vector2(40.2f, 38), new Vector2(36, 33.6f), new Vector2(29.2f, 31.8f));
            Rect bodyBounds = BoundsOf(body, 0f);
            r.Fill(body, Radial(bodyBounds, new Vector2(0.34f, 0.38f), 0.8f, leather));
            r.Stroke(body, 1f, Solid(leatherEdge), true);

            // Folds drawn together towards the neck, and the light on the left side
            foreach ((Vector2 from, Vector2 control, Vector2 to) in new[]
            {
                (new Vector2(20.2f, 33), new Vector2(16.4f, 37.6f), new Vector2(16.6f, 42.4f)),
                (new Vector2(27.8f, 33), new Vector2(31.6f, 37.4f), new Vector2(31.4f, 42)),
                (new Vector2(24, 33.4f), new Vector2(23.4f, 38.6f), new Vector2(24.4f, 43.4f)),
                (new Vector2(11.6f, 43.6f), new Vector2(24, 47), new Vector2(36.4f, 43.6f)),
            })
            {
                List<Vector2> fold = new() { from };
                Quad(fold, control, to);
                r.Stroke(fold, 0.5f, Solid(Hex("2a1406")), false, 0.55f);
            }
            List<Vector2> shine = new() { new Vector2(12.6f, 39.2f) };
            Quad(shine, new Vector2(11.6f, 42.4f), new Vector2(13.6f, 45));
            r.Stroke(shine, 0.9f, Solid(Hex("f0c090")), false, 0.28f);

            List<Vector2> neck = Points(18.8f, 32.4f);
            Quad(neck, new Vector2(18.5f, 29.4f), new Vector2(18, 27));
            neck.Add(new Vector2(30, 27));
            Quad(neck, new Vector2(29.5f, 29.4f), new Vector2(29.2f, 32.4f));
            r.Fill(neck, Horizontal(18f, 30f, neckLeather));
            r.Stroke(neck, 0.7f, Solid(leatherEdge), true);

            Vector2 mouthCentre = new(24f, 27f);
            Vector2 mouthRadius = new(6.1f, 1.8f);
            Rect mouthBounds = EllipseBounds(mouthCentre, mouthRadius);
            r.Draw(p => EllipseDistance(p, mouthCentre, mouthRadius), mouthBounds, Vertical(25.2f, 28.8f, pouchInside));
            r.Draw(p => Mathf.Abs(EllipseDistance(p, mouthCentre, mouthRadius)) - 0.3f, mouthBounds, Solid(leatherEdge));

            // The gold heaped in the mouth: two coins lying back, two facing out, one more on them and one on its edge
            TiltedCoin(r, 21.7f, 25.9f, 2.2f, 1.15f, -6f);
            TiltedCoin(r, 26.5f, 25.7f, 2.2f, 1.15f, 8f);
            FacingCoin(r, 23f, 24f, 2f);
            FacingCoin(r, 26f, 23.6f, 1.95f);
            EdgeCoin(r, 24.5f, 21.8f, -12f);

            List<Vector2> rim = new() { new Vector2(18, 27.2f) };
            Quad(rim, new Vector2(24, 29.2f), new Vector2(30, 27.2f));
            r.Stroke(rim, 1.1f, Solid(Hex("a86c38")));

            // The cord loose round the neck, its end hanging with a tassel
            List<Vector2> tie = new() { new Vector2(18.6f, 31.4f) };
            Quad(tie, new Vector2(24, 33.4f), new Vector2(29.8f, 31.4f));
            r.Stroke(tie, 1.5f, Vertical(30.6f, 33f, polishedGold));
            List<Vector2> cord = new() { new Vector2(29.6f, 31.8f) };
            Quad(cord, new Vector2(33.8f, 33.2f), new Vector2(33.4f, 38.2f));
            Quad(cord, new Vector2(33.1f, 41f), new Vector2(34.9f, 42.2f));
            r.Stroke(cord, 1f, Vertical(31f, 43f, polishedGold));
            Vector2 knot = new(34.9f, 42.2f);
            foreach (Vector2 end in new[] { new Vector2(34f, 44.3f), new Vector2(35.8f, 44.3f), new Vector2(34.9f, 44.5f) })
                r.Stroke(new List<Vector2> { knot, end }, 0.6f, Solid(gold));

            // The coins that spilled: a little stack, one leaning on its edge and one lying apart
            TiltedCoin(r, 39.8f, 48.4f, 3.1f, 1.2f, 0f);
            TiltedCoin(r, 40.4f, 47.3f, 3.1f, 1.2f, 0f);
            TiltedCoin(r, 39.8f, 46.2f, 3.1f, 1.2f, 0f);
            EdgeCoin(r, 46f, 46.4f, -14f);
            TiltedCoin(r, 35.5f, 48.8f, 2.6f, 1f, 4f);

            r.Fill(Points(25, 16, 25.65f, 18.15f, 27.8f, 18.8f, 25.65f, 19.45f, 25, 21.6f, 24.35f, 19.45f, 22.2f, 18.8f, 24.35f, 18.15f), Solid(Hex("fffbe0")));

            return r.Save(spriteFolder + "MoneyPouch.png", 100f);
        }

        private static void FacingCoin(SpriteRaster r, float x, float y, float radius)
        {
            Vector2 centre = new(x, y);
            Vector2 size = Vector2.one * radius;
            Rect bounds = EllipseBounds(centre, size);
            r.Draw(p => EllipseDistance(p, centre, size), bounds, Radial(bounds, new Vector2(0.35f, 0.3f), 0.8f, coinFace));
            r.Draw(p => Mathf.Abs(EllipseDistance(p, centre, size)) - 0.21f, bounds, Solid(coinEdge));
            r.Draw(p => Mathf.Abs(EllipseDistance(p, centre, size * 0.58f)) - 0.15f, bounds, Solid(coinRing));
        }

        // A coin lying back, turned by an angle, its side showing under it
        private static void TiltedCoin(SpriteRaster r, float x, float y, float radiusX, float radiusY, float degrees)
        {
            Vector2 face = new(x, y);
            Vector2 side = face + Rotate(new Vector2(0f, 0.55f), degrees);
            Vector2 size = new(radiusX, radiusY);
            Rect bounds = EllipseBounds(face, size + Vector2.one);

            r.Draw(p => EllipseDistance(p, side, size, degrees), bounds, Vertical(side.y - radiusY, side.y + radiusY, coinSide));
            r.Draw(p => Mathf.Abs(EllipseDistance(p, side, size, degrees)) - 0.2f, bounds, Solid(coinEdge));
            r.Draw(p => EllipseDistance(p, face, size, degrees), bounds, Radial(new Rect(face - size, size * 2f), new Vector2(0.35f, 0.3f), 0.8f, coinFace));
            r.Draw(p => Mathf.Abs(EllipseDistance(p, face, size, degrees)) - 0.2f, bounds, Solid(coinEdge));
            r.Draw(p => Mathf.Abs(EllipseDistance(p, face, size * 0.58f, degrees)) - 0.15f, bounds, Solid(coinRing));
        }

        // A coin standing on its edge, leaning by an angle
        private static void EdgeCoin(SpriteRaster r, float x, float y, float degrees)
        {
            Vector2 centre = new(x, y);
            Vector2 half = new(0.85f, 2.5f);
            float Distance(Vector2 p) => RoundBoxDistance(Rotate(p - centre, -degrees), half, 0.8f, 0.8f, 0.8f, 0.8f);
            Rect bounds = new(x - 3.5f, y - 3.5f, 7f, 7f);
            r.Draw(Distance, bounds, Vertical(y - 2.5f, y + 2.5f, coinSide));
            r.Draw(p => Mathf.Abs(Distance(p)) - 0.2f, bounds, Solid(coinEdge));
        }


        private static void BuildPrefab(Sprite plaqueSprite, Sprite pouchSprite)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
            {
                GameObject made = new("MoneyPlaque", typeof(RectTransform));
                PrefabUtility.SaveAsPrefabAsset(made, prefabPath);
                Object.DestroyImmediate(made);
            }

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                Begin(root.transform);
                root.layer = LayerMask.NameToLayer("UI");

                RectTransform rootRect = (RectTransform)root.transform;
                rootRect.sizeDelta = plaqueArea.size * shownScale;

                Image plaque = GetOrAdd<Image>(root);
                plaque.sprite = plaqueSprite;
                plaque.type = Image.Type.Simple;
                plaque.color = Color.white;
                plaque.raycastTarget = true;

                TMP_FontAsset font = FindFont();

                // Placed from the top left of the plaque drawing, in its own units
                float X(float x) => (x - plaqueArea.x) * shownScale;
                float Y(float y) => (y - plaqueArea.y) * shownScale;

                // The pouch swells and squeezes from its bottom, where it sits
                RectTransform pouch = Place("Pouch", root.transform, X(pouchArea.x), Y(pouchArea.y), pouchArea.width * shownScale, pouchArea.height * shownScale);
                pouch.pivot = new Vector2(0.5f, 0f);
                pouch.anchoredPosition = new Vector2(X(pouchArea.center.x), -Y(pouchArea.yMax));
                AddImage(pouch, pouchSprite, Color.white);

                TextMeshProUGUI money = AddText(Place("MoneyValue", root.transform, X(56f), Y(22f), X(148f) - X(56f), 20f * shownScale), font, "0", 20f * shownScale, ink, TextAlignmentOptions.Right);
                TextMeshProUGUI delta = AddText(Place("MoneyChange", root.transform, X(56f), Y(9f), X(148f) - X(56f), 10f * shownScale), font, "", 9f * shownScale, ink, TextAlignmentOptions.Right);

                RemoveLeftOvers(root.transform);

                // The settings are taken from the script each bake, so a default changed there is not kept out by the
                // value the prefab held before
                MoneyPanel panel = GetOrAdd<MoneyPanel>(root);
                GameObject defaults = new("MoneyPanelDefaults");
                try
                {
                    EditorUtility.CopySerialized(defaults.AddComponent<MoneyPanel>(), panel);
                }
                finally
                {
                    Object.DestroyImmediate(defaults);
                }

                SerializedObject serialized = new(panel);
                serialized.FindProperty("moneyText").objectReferenceValue = money;
                serialized.FindProperty("deltaMoneyText").objectReferenceValue = delta;
                serialized.FindProperty("pouchSprite").objectReferenceValue = pouch;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        // The plaque goes where the plain money text was, at the top of the shop
        private static void PlaceInScene(string scenePath)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedHere = !scene.isLoaded;
            if (openedHere)
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                Transform shop = FindInScene(scene, shopName);
                if (shop == null)
                {
                    Debug.LogWarning($"{shopName} not found in {scenePath}");
                    return;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                Transform old = shop.Find(oldMoneyName);
                Transform plaque = shop.Find(prefab.name);
                if (plaque == null)
                {
                    plaque = ((GameObject)PrefabUtility.InstantiatePrefab(prefab, shop)).transform;
                    plaque.name = prefab.name;
                    if (old != null)
                        plaque.SetSiblingIndex(old.GetSiblingIndex());
                }

                RectTransform rect = (RectTransform)plaque;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(shop is RectTransform shopRect ? shopRect.rect.width / 2f : 75f, 0f);

                if (old != null)
                    Object.DestroyImmediate(old.gameObject);

                Transform manager = FindInScene(scene, managerName);
                if (manager != null)
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(manager.gameObject);

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                if (openedHere)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Transform FindInScene(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform found = FindDeep(root.transform, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static Transform FindDeep(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;
            foreach (Transform child in parent)
            {
                Transform found = FindDeep(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }
    }

}
