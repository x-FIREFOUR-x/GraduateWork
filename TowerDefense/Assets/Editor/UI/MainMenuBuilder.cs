using System.Collections.Generic;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using static TowerDefense.EditorTools.UI.SpriteRaster;
using static TowerDefense.EditorTools.UI.UILayout;


namespace TowerDefense.EditorTools.UI
{
    // Paints the background of the main menu: a dark elven hall with a great arch in a golden frame and a golden vine,
    // and through the arch the forest of Lothlórien in the sun, its silver trunks and its green and golden crowns cut
    // in facets as the trees of the map are, golden leaves falling and grass with small flowers as on the tiles.
    // The picture is put behind the buttons of the main menu, covering the screen
    public static class MainMenuBuilder
    {
        private const string scenePath = "Assets/Scenes/MainMenuScene.unity";
        private const string spritePath = "Assets/Sprites/UI/MainMenu/Background.png";
        private const string canvasName = "Canvas";
        private const string menuName = "Menu";
        private const string backgroundName = "Background";

        // The picture is drawn in units of the mockup, widened to the shape of the screen
        private static readonly Rect area = new(-12f, 2f, 384f, 216f);
        private const float pixels = 5f;

        private static readonly Color forestColor = Hex("7aaa48");
        private static readonly Color hallColor = Hex("203a26");
        private static readonly Color green = Hex("3e7a2c");
        private static readonly Color greenShade = Hex("2e6020");
        private static readonly Color golden = Hex("d8b848");
        private static readonly Color goldenShade = Hex("b89a30");
        private static readonly Color grassColor = Hex("5a9038");
        private static readonly Color bladeColor = Hex("4a7a2c");
        private static readonly Color flowerColor = Hex("f0d060");
        private static readonly Color leafColor = Hex("e8c048");
        private static readonly Color gold = Hex("e2c172");
        private static readonly Color darkGold = Hex("8a6a2e");

        private static readonly (float, Color)[] polishedGold =
        {
            (0f, Hex("fff2c0")), (0.35f, Hex("e6bf62")), (0.62f, Hex("9a6e22")), (1f, Hex("f0d488")),
        };
        private static readonly (float, Color)[] silver = { (0f, Hex("8a948a")), (0.45f, Hex("e6ece2")), (1f, Hex("7a847a")) };
        private static readonly (float, Color)[] sunlight = { (0f, Hex("fff0b0", 0.55f)), (1f, Hex("fff0b0", 0f)) };


        [MenuItem("Tools/UI/Build Main Menu")]
        public static void Build()
        {
            Sprite background = BakeBackground();

            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedHere = !scene.isLoaded;
            if (openedHere)
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                DressScene(scene, background);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                if (openedHere)
                    EditorSceneManager.CloseScene(scene, true);
            }

            Debug.Log("Main menu built");
        }


        private static Sprite BakeBackground()
        {
            SpriteRaster r = new(area, pixels);

            r.Draw(_ => -1f, area, Solid(forestColor));

            // The crowns behind, green and golden by turns, and the silver trunks before them
            (float x, float y, float radius)[] crowns = { (50, 60, 46), (130, 40, 52), (210, 44, 50), (300, 58, 46), (90, 110, 34), (260, 108, 34) };
            for (int i = 0; i < crowns.Length; i++)
                Crown(r, crowns[i].x, crowns[i].y, crowns[i].radius, i % 2 == 1);

            foreach ((float x, float width) in new[] { (110f, 14f), (170f, 18f), (240f, 14f) })
            {
                Rect trunk = new(x - width / 2f, area.yMin, width, area.height);
                r.Draw(p => BoxDistance(p, trunk), trunk, Horizontal(trunk.xMin, trunk.xMax, silver), 0.85f);
            }

            r.Draw(_ => -1f, area, Radial(area, new Vector2(0.5f, 0.2f), 0.8f, sunlight));

            Grass(r);
            Hall(r);
            Vine(r);

            for (int k = 0; k < 20; k++)
                Leaf(r, (k * 53 * 3) % 360, (k * 29 + 21) % 220, k * 37f);

            return r.Save(spritePath, 100f);
        }

        // A crown cut in facets, as the trees of the map are: seven corners and one side in its own shade
        private static void Crown(SpriteRaster r, float x, float y, float radius, bool isGolden)
        {
            List<Vector2> corners = new();
            for (int k = 0; k < 7; k++)
            {
                float angle = k / 7f * Mathf.PI * 2f + 0.3f;
                float rx = radius * (k % 2 == 1 ? 1f : 0.86f);
                float ry = radius * (k % 2 == 1 ? 0.86f : 1f);
                corners.Add(new Vector2(x + Mathf.Cos(angle) * rx, y + Mathf.Sin(angle) * ry));
            }
            r.Fill(corners, Solid(isGolden ? golden : green));
            r.Fill(new List<Vector2> { corners[4], corners[5], corners[6], new Vector2(x, y) }, Solid(isGolden ? goldenShade : greenShade));
        }

        // The grass at the bottom with blades and small flowers along it
        private static void Grass(SpriteRaster r)
        {
            List<Vector2> ground = new() { new Vector2(area.xMin, 196f) };
            Quad(ground, new Vector2(180f, 186f), new Vector2(area.xMax, 196f));
            ground.Add(new Vector2(area.xMax, area.yMax));
            ground.Add(new Vector2(area.xMin, area.yMax));
            r.Fill(ground, Solid(grassColor));

            for (float x = area.xMin + 4f; x < area.xMax; x += 9f)
            {
                float y = 202f + Mathf.Repeat(x, 3f);
                r.Fill(Points(x, y, x + 2f, y - 6f, x + 4f, y), Solid(bladeColor));
            }
            for (float x = area.xMin + 12f; x < area.xMax; x += 37f)
                r.Fill(Points(x, 208f, x + 1.5f, 206.5f, x + 3f, 208f, x + 1.5f, 209.5f), Solid(flowerColor));
        }

        // The arch the forest shows through: open at the bottom, round at the top
        private static List<Vector2> Arch(float side, float top, float shoulder)
        {
            List<Vector2> arch = new() { new Vector2(side, area.yMax), new Vector2(side, shoulder) };
            Quad(arch, new Vector2(side, top + 6f), new Vector2(180f, top), 24);
            Quad(arch, new Vector2(360f - side, top + 6f), new Vector2(360f - side, shoulder), 24);
            arch.Add(new Vector2(360f - side, area.yMax));
            return arch;
        }

        // The hall round the arch, its golden frame and a thin line inside it
        private static void Hall(SpriteRaster r)
        {
            List<Vector2> opening = Arch(28f, 8f, 70f);
            r.Draw(p => -PolygonDistance(p, opening), area, Solid(hallColor));

            List<Vector2> frame = opening.GetRange(0, opening.Count);
            r.Stroke(frame, 2.4f, Vertical(area.yMin, area.yMax, polishedGold));
            r.Stroke(Arch(36f, 16f, 72f), 0.8f, Solid(darkGold));
        }

        // A golden vine along the inside of the arch with leaves on it
        private static void Vine(SpriteRaster r)
        {
            float ArchY(float x)
            {
                float t = (x - 40f) / 280f;
                return 18f + Mathf.Pow(2f * t - 1f, 2f) * 40f;
            }

            List<Vector2> vine = new();
            for (float x = 40f; x <= 320f; x += 2f)
                vine.Add(new Vector2(x, ArchY(x) + Mathf.Sin(x / 18f * Mathf.PI * 2f) * 2.5f));
            r.Stroke(vine, 0.9f, Solid(gold));

            for (float x = 50f; x < 320f; x += 22f)
            {
                float y = ArchY(x);
                List<Vector2> leaf = new() { new Vector2(x, y) };
                Quad(leaf, new Vector2(x + 2f, y - 4f), new Vector2(x + 6f, y - 3f));
                Quad(leaf, new Vector2(x + 4f, y), new Vector2(x, y));
                r.Fill(leaf, Solid(gold));
            }
        }

        private static void Leaf(SpriteRaster r, float x, float y, float degrees)
        {
            Vector2 stem = new(x, y);
            List<Vector2> leaf = new() { stem };
            Quad(leaf, new Vector2(x + 3f, y - 3f), new Vector2(x + 6f, y));
            Quad(leaf, new Vector2(x + 3f, y + 3f), stem);
            r.Fill(Rotate(leaf, stem, degrees), Solid(leafColor), 0.9f);
        }

        private static float BoxDistance(Vector2 p, Rect box)
        {
            Vector2 d = new(Mathf.Abs(p.x - box.center.x) - box.width / 2f, Mathf.Abs(p.y - box.center.y) - box.height / 2f);
            return Vector2.Max(d, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0f);
        }


        // The picture covers the screen behind the buttons, keeping its shape and cutting off what does not fit
        private static void DressScene(Scene scene, Sprite background)
        {
            GameObject canvas = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == canvasName)
                    canvas = root;
            }
            if (canvas == null)
            {
                Debug.LogWarning($"{canvasName} not found in {scenePath}");
                return;
            }

            Transform existing = canvas.transform.Find(backgroundName);
            RectTransform rect = existing != null ? (RectTransform)existing : new GameObject(backgroundName, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(canvas.transform, false);
            rect.gameObject.layer = canvas.layer;
            rect.SetAsFirstSibling();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            Image image = GetOrAdd<Image>(rect.gameObject);
            image.sprite = background;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.raycastTarget = false;

            AspectRatioFitter fitter = GetOrAdd<AspectRatioFitter>(rect.gameObject);
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = area.width / area.height;

            Transform menu = canvas.transform.Find(menuName);
            if (menu != null && menu.TryGetComponent(out Image menuImage))
                menuImage.color = Color.clear;
        }
    }

}
