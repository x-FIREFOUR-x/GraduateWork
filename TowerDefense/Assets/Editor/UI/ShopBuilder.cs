using System;
using System.Collections.Generic;
using System.Linq;

using TMPro;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using TowerDefense.Main.UI;
using TowerDefense.Main.UI.EnemyMenu;
using TowerDefense.Main.UI.TowerMenu;

using static TowerDefense.EditorTools.UI.SpriteRaster;
using static TowerDefense.EditorTools.UI.UILayout;


namespace TowerDefense.EditorTools.UI
{
    // Dresses the shop as the money plaque over it: a dark column with a golden edge, and dark cards with a thin frame,
    // the name over a golden line, the icon in an arched window, a pill for the stats, the price by a coin, and for the
    // units a row of round tokens round the count in a small shield. The objects already in the cards are kept and only
    // set anew and moved, since the scenes hold their icons, names and towers; the scenes are put back on the layout
    public static class ShopBuilder
    {
        private const string towerCardPath = "Assets/Prefabs/UI/TowerShopComponent.prefab";
        private const string enemyCardPath = "Assets/Prefabs/UI/EnemyShopComponent.prefab";
        private const string spriteFolder = "Assets/Sprites/UI/Shop/";
        private static readonly string[] scenePaths = { "Assets/Scenes/DefenderGameScene.unity", "Assets/Scenes/AttackerGameScene.unity" };
        private const string shopName = "MenuShop";

        private const float pixels = 3f;
        private const float cardWidth = 126f;
        private const float towerCardHeight = 204f;
        private const float enemyCardHeight = 229f;
        private const float glowMargin = 6f;
        private const float tokenSize = 26f;
        private const float tokenGap = 1f;
        private const float counterHeight = tokenSize * 2f + tokenGap;
        private const float priceRowHeight = 22f;

        private static readonly Color columnColor = Hex("141f17", 0.94f);
        private static readonly Color cardColor = Hex("1c2a1f");
        private static readonly Color windowColor = Hex("0f1912");
        private static readonly Color pillColor = Hex("16201a");
        private static readonly Color shieldColor = Hex("22301f");
        private static readonly Color gold = Hex("e2c172");
        private static readonly Color darkGold = Hex("8a6a2e");
        private static readonly Color goldEdge = Hex("5a3e12");
        private static readonly Color ink = Hex("f3e6c0");
        private static readonly Color muted = Hex("bfae86");
        private static readonly Color statsBack = Hex("0f1912", 0.94f);
        private static readonly Color unselectedTint = new(0.8f, 0.8f, 0.8f, 1f);
        private static readonly Color failTint = new(1f, 0.45f, 0.4f, 1f);

        private static readonly (float, Color)[] polishedGold =
        {
            (0f, Hex("fff2c0")), (0.35f, Hex("e6bf62")), (0.62f, Hex("9a6e22")), (1f, Hex("f0d488")),
        };
        private static readonly (float, Color)[] coinFace = { (0f, Hex("fff8dc")), (0.45f, Hex("ecc25a")), (1f, Hex("8a5a12")) };
        private static readonly (float, Color)[] tokenFace = { (0f, Hex("3a4a34")), (1f, Hex("141f17")) };

        // The parts of the cards the scenes moved and sized while the cards were laid out by a layout group
        private static readonly string[] layoutProperties = { "m_SizeDelta", "m_AnchoredPosition", "m_AnchorMin", "m_AnchorMax", "m_Pivot" };


        [MenuItem("Tools/UI/Build Shop")]
        public static void Build()
        {
            Sprites sprites = BakeSprites();

            EditPrefab(towerCardPath, root => LayOutTowerCard(root, sprites));
            EditPrefab(enemyCardPath, root => LayOutEnemyCard(root, sprites));
            foreach (string scenePath in scenePaths)
                EditScene(scenePath, scene => DressScene(scene, sprites));

            AssetDatabase.SaveAssets();
            Debug.Log("Shop built");
        }


        private class Sprites
        {
            public Sprite Column, Card, Selected, Diamond, NameLine, WindowBack, WindowFrame, Pill, Coin, Token, CountShield, White;
        }

        private static Sprites BakeSprites()
        {
            return new Sprites
            {
                Column = BakeColumn(),
                Card = BakeCard(),
                Selected = BakeSelected(),
                Diamond = BakeDiamond(),
                NameLine = BakeNameLine(),
                WindowBack = BakeWindow("WindowBack", false),
                WindowFrame = BakeWindow("WindowFrame", true),
                Pill = BakePill(),
                Coin = BakeCoin(),
                Token = BakeToken(),
                CountShield = BakeCountShield(),
                White = BakeWhite(),
            };
        }

        // Dark, with a golden line down its right edge and a thin one inside it
        private static Sprite BakeColumn()
        {
            const float size = 64f;
            Rect all = new(0f, 0f, size, size);
            SpriteRaster raster = new(all, pixels);
            raster.Draw(_ => -1f, all, Solid(columnColor));
            raster.Draw(p => Mathf.Abs(p.x - (size - 0.75f)) - 0.75f, all, Solid(gold));
            raster.Draw(p => Mathf.Abs(p.x - (size - 4.2f)) - 0.4f, all, Solid(darkGold));
            return raster.Save(spriteFolder + "ShopColumn.png", pixels * 100f, new Vector4(3f, 3f, 8f * pixels, 3f));
        }

        private static float CardDistance(Vector2 p, float inset)
        {
            return RoundBoxDistance(p - new Vector2(32f, 32f), Vector2.one * (32f - inset),
                Mathf.Max(7f - inset, 0.5f), Mathf.Max(2f - inset, 0.5f), Mathf.Max(7f - inset, 0.5f), Mathf.Max(2f - inset, 0.5f));
        }

        private static Sprite BakeCard()
        {
            Rect all = new(0f, 0f, 64f, 64f);
            SpriteRaster raster = new(all, pixels);
            raster.Draw(p => CardDistance(p, 0f), all, Solid(cardColor));
            raster.Draw(p => Mathf.Abs(CardDistance(p, 0.6f)) - 0.6f, all, Solid(darkGold));
            float border = 10f * pixels;
            return raster.Save(spriteFolder + "Card.png", pixels * 100f, new Vector4(border, border, border, border));
        }

        // The frame of a chosen card: a golden line with a thin one inside, and a soft golden light round it
        private static Sprite BakeSelected()
        {
            Rect all = new(-glowMargin, -glowMargin, 64f + glowMargin * 2f, 64f + glowMargin * 2f);
            SpriteRaster raster = new(all, pixels);
            raster.Draw(p => CardDistance(p, -glowMargin), all, p =>
            {
                float d = CardDistance(p, 0f);
                Color c = gold;
                c.a = d <= 0f ? 0f : 0.4f * Mathf.Pow(1f - Mathf.Clamp01(d / glowMargin), 2f);
                return c;
            });
            raster.Draw(p => Mathf.Abs(CardDistance(p, 1f)) - 1f, all, Solid(gold));
            raster.Draw(p => Mathf.Abs(CardDistance(p, 3.5f)) - 0.3f, all, Solid(darkGold));
            float border = (10f + glowMargin) * pixels;
            return raster.Save(spriteFolder + "CardSelected.png", pixels * 100f, new Vector4(border, border, border, border));
        }

        private static Sprite BakeDiamond()
        {
            SpriteRaster raster = new(new Rect(0f, 0f, 14f, 9f), pixels);
            List<Vector2> diamond = Points(7, 0.3f, 13.7f, 4.5f, 7, 8.7f, 0.3f, 4.5f);
            raster.Fill(diamond, Vertical(0f, 9f, polishedGold));
            raster.Stroke(diamond, 0.4f, Solid(goldEdge), true);
            return raster.Save(spriteFolder + "SelectedDiamond.png", 100f);
        }

        // A golden line fading out at both ends
        private static Sprite BakeNameLine()
        {
            Rect all = new(0f, 0f, 66f, 2f);
            SpriteRaster raster = new(all, pixels);
            raster.Draw(p => Mathf.Abs(p.y - 1f) - 0.5f, all, p =>
            {
                Color c = gold;
                c.a = Mathf.Pow(1f - Mathf.Abs(p.x - 33f) / 33f, 1.5f);
                return c;
            });
            return raster.Save(spriteFolder + "NameLine.png", 100f);
        }

        // The arched window the icon shows through: round at the top, nearly square at the bottom
        private static float WindowDistance(Vector2 p)
        {
            const float radius = 45f;
            float arch = (p - new Vector2(radius, radius)).magnitude - radius;
            float box = RoundBoxDistance(p - new Vector2(radius, (radius + 84f) / 2f), new Vector2(radius, (84f - radius) / 2f), 0.5f, 0.5f, 4f, 4f);
            return Mathf.Min(arch, box);
        }

        private static Sprite BakeWindow(string name, bool frame)
        {
            Rect all = new(0f, 0f, 90f, 84f);
            SpriteRaster raster = new(all, pixels);
            if (frame)
                raster.Draw(p => Mathf.Abs(WindowDistance(p) + 0.6f) - 0.6f, all, Vertical(0f, 84f, polishedGold));
            else
                raster.Draw(WindowDistance, all, Solid(windowColor));
            return raster.Save(spriteFolder + name + ".png", 100f);
        }

        private static Sprite BakePill()
        {
            Rect all = new(0f, 0f, 40f, 30f);
            SpriteRaster raster = new(all, pixels);
            float Pill(Vector2 p) => RoundBoxDistance(p - new Vector2(20f, 15f), new Vector2(20f, 15f), 15f, 15f, 15f, 15f);
            raster.Draw(Pill, all, Solid(pillColor));
            raster.Draw(p => Mathf.Abs(Pill(p) + 0.5f) - 0.5f, all, Solid(darkGold));
            float border = 15f * pixels;
            return raster.Save(spriteFolder + "Pill.png", pixels * 100f, new Vector4(border, border, border, border));
        }

        private static Sprite BakeCoin()
        {
            Rect all = new(0f, 0f, 12f, 12f);
            SpriteRaster raster = new(all, pixels * 2f);
            Vector2 centre = new(6f, 6f);
            raster.Draw(p => (p - centre).magnitude - 5.4f, all, Radial(new Rect(0.6f, 0.6f, 10.8f, 10.8f), new Vector2(0.35f, 0.3f), 0.8f, coinFace));
            raster.Draw(p => Mathf.Abs((p - centre).magnitude - 5.4f) - 0.3f, all, Solid(Hex("4a3010")));
            raster.Draw(p => Mathf.Abs((p - centre).magnitude - 3.2f) - 0.2f, all, Solid(Hex("9a6a1a")));
            return raster.Save(spriteFolder + "Coin.png", 100f);
        }

        private static Sprite BakeToken()
        {
            Rect all = new(0f, 0f, 20f, 20f);
            SpriteRaster raster = new(all, pixels * 2f);
            Vector2 centre = new(10f, 10f);
            raster.Draw(p => (p - centre).magnitude - 10f, all, Radial(all, new Vector2(0.35f, 0.3f), 0.8f, tokenFace));
            raster.Draw(p => Mathf.Abs((p - centre).magnitude - 9.4f) - 0.6f, all, Solid(gold));
            return raster.Save(spriteFolder + "Token.png", 100f);
        }

        private static Sprite BakeCountShield()
        {
            Rect all = new(0f, 0f, 24f, 28f);
            SpriteRaster raster = new(all, pixels * 2f);
            float Shield(Vector2 p) => RoundBoxDistance(p - new Vector2(12f, 14f), new Vector2(12f, 14f), 3f, 3f, 12f, 12f);
            raster.Draw(Shield, all, Solid(shieldColor));
            raster.Draw(p => Mathf.Abs(Shield(p) + 0.6f) - 0.6f, all, Solid(gold));
            return raster.Save(spriteFolder + "CountShield.png", 100f);
        }

        private static Sprite BakeWhite()
        {
            Rect all = new(0f, 0f, 4f, 4f);
            SpriteRaster raster = new(all, 2f);
            raster.Draw(_ => -1f, all, Solid(Color.white));
            return raster.Save(spriteFolder + "White.png", 100f);
        }


        private static void LayOutTowerCard(GameObject root, Sprites sprites)
        {
            TMP_FontAsset font = FindFont();
            LayOutCardBase(root, sprites, towerCardHeight);

            Style(Place("TextNameTower", root.transform, 4f, 6f, cardWidth - 8f, 18f), font, 13f, ink, TextAlignmentOptions.Center);
            AddImage(Place("NameLine", root.transform, 30f, 26f, 66f, 2f), sprites.NameLine, Color.white);

            RectTransform window = Window(root.transform, sprites, 8f, 32f, 110f, 104f);
            Adopt(root.transform, window, "ButtonSelect");
            RectTransform icon = Place("ButtonSelect", window, 5f, 3f, 100f, 100f);
            icon.GetComponent<Image>().preserveAspect = true;
            RectTransform stats = Place("CharacterImage", icon, -5f, -3f, 110f, 104f);
            AddImage(stats, sprites.White, statsBack);
            TextMeshProUGUI statsText = Style(Place("TextCharacterTower", stats, 14f, 26f, 84f, 70f), font, 11f, ink, TextAlignmentOptions.TopLeft);
            statsText.lineSpacing = 14f;
            AddImage(Place("WindowFrame", root.transform, 8f, 32f, 110f, 104f), sprites.WindowFrame, Color.white);

            StatsButton(root.transform, sprites, font, 142f);
            Price(root.transform, sprites, font, "TextPriceTower", 177f);

            RectTransform selected = Place("SelectedFrame", root.transform, -glowMargin, -glowMargin, cardWidth + glowMargin * 2f, towerCardHeight + glowMargin * 2f);
            Image selectedImage = AddImage(selected, sprites.Selected, Color.white);
            selectedImage.type = Image.Type.Sliced;
            AddImage(Place("Diamond", selected, (cardWidth + glowMargin * 2f - 14f) / 2f, glowMargin - 4.5f, 14f, 9f), sprites.Diamond, Color.white);
            selected.gameObject.SetActive(false);

            RemoveLeftOvers(root.transform);

            // A click anywhere on the card chooses the tower, as a click on its icon does; the stats button keeps its own
            ClickArea(root, icon.gameObject, true);

            SerializedObject card = new(root.GetComponent<TowerShopComponent>());
            card.FindProperty("selectedFrame").objectReferenceValue = selected.gameObject;
            card.FindProperty("colorSelected").colorValue = Color.white;
            card.FindProperty("colorUnselected").colorValue = unselectedTint;
            card.FindProperty("colorText").colorValue = ink;
            card.FindProperty("colorCharacterText").colorValue = ink;
            card.FindProperty("colorHighlightText").colorValue = gold;
            card.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void LayOutEnemyCard(GameObject root, Sprites sprites)
        {
            TMP_FontAsset font = FindFont();
            LayOutCardBase(root, sprites, enemyCardHeight);

            Style(Place("TextNameEnemy", root.transform, 4f, 6f, cardWidth - 8f, 18f), font, 13f, ink, TextAlignmentOptions.Center);
            AddImage(Place("NameLine", root.transform, 30f, 26f, 66f, 2f), sprites.NameLine, Color.white);

            RectTransform window = Window(root.transform, sprites, 13f, 32f, 100f, 94f);
            Adopt(root.transform, window, "ButtonImage");
            RectTransform icon = Place("ButtonImage", window, 6f, 3f, 88f, 88f);
            icon.GetComponent<Image>().preserveAspect = true;
            RectTransform stats = Place("Character", icon, -6f, -3f, 100f, 94f);
            AddImage(stats, sprites.White, statsBack);
            TextMeshProUGUI statsText = Style(Place("TextCharacter", stats, 14f, 26f, 76f, 60f), font, 11f, ink, TextAlignmentOptions.TopLeft);
            statsText.lineSpacing = 14f;
            AddImage(Place("WindowFrame", root.transform, 13f, 32f, 100f, 94f), sprites.WindowFrame, Color.white);

            StatsButton(root.transform, sprites, font, 132f);

            // The price over the count in its shield, between a column of tokens to take away and a column to add,
            // five over one
            const float counterY = 167f;
            AddImage(Place("Coin", root.transform, 41f, counterY + 4f, 12f, 12f), sprites.Coin, Color.white);
            TextMeshProUGUI price = Style(Place("TextPriceEnemy", root.transform, 56f, counterY, 40f, 20f), font, 15f, ink, TextAlignmentOptions.Left);
            price.fontStyle = FontStyles.Bold;

            RectTransform counter = Place("CountPanel", root.transform, 8f, counterY, cardWidth - 16f, counterHeight);
            Unlay(counter.gameObject);
            RectTransform sub = Place("Sub", counter, 0f, 0f, tokenSize, counterHeight);
            Unlay(sub.gameObject);
            Token(sub, sprites, font, "Button5Sub", 0f);
            Token(sub, sprites, font, "ButtonSub", tokenSize + tokenGap);
            float shieldX = (cardWidth - 16f - 40f) / 2f;
            AddImage(Place("CountShield", counter, shieldX, priceRowHeight, 40f, counterHeight - priceRowHeight), sprites.CountShield, Color.white);
            Style(Place("TextCountEnemy", counter, shieldX, priceRowHeight, 40f, counterHeight - priceRowHeight - 3f), font, 18f, ink, TextAlignmentOptions.Center);
            RectTransform add = Place("Add", counter, cardWidth - 16f - tokenSize, 0f, tokenSize, counterHeight);
            Unlay(add.gameObject);
            Token(add, sprites, font, "Button5Add", 0f);
            Token(add, sprites, font, "ButtonAdd", tokenSize + tokenGap);

            RemoveLeftOvers(root.transform);

            // A right click anywhere on the card opens the stats, as a right click on its icon does
            ClickArea(root, icon.gameObject, false);

            SerializedObject card = new(root.GetComponent<EnemyShopComponent>());
            card.FindProperty("colorText").colorValue = ink;
            card.FindProperty("colorCharacterText").colorValue = ink;
            card.FindProperty("colorHighlightText").colorValue = gold;
            card.FindProperty("failColorButtonsAddAndSub").colorValue = failTint;
            card.ApplyModifiedPropertiesWithoutUndo();
        }

        // The card itself, laid out by hand now rather than by a layout group
        private static void LayOutCardBase(GameObject root, Sprites sprites, float height)
        {
            Begin(root.transform);
            Unlay(root);

            ((RectTransform)root.transform).sizeDelta = new Vector2(cardWidth, height);
            LayoutElement element = root.GetComponent<LayoutElement>();
            if (element != null)
            {
                element.preferredWidth = cardWidth;
                element.preferredHeight = height;
            }

            Image background = GetOrAdd<Image>(root);
            background.sprite = sprites.Card;
            background.type = Image.Type.Sliced;
            background.color = Color.white;
        }

        private static void ClickArea(GameObject card, GameObject icon, bool forwardLeftClick)
        {
            card.GetComponent<Image>().raycastTarget = true;
            SerializedObject clickArea = new(GetOrAdd<CardClickArea>(card));
            clickArea.FindProperty("clickTarget").objectReferenceValue = icon;
            clickArea.FindProperty("forwardLeftClick").boolValue = forwardLeftClick;
            clickArea.FindProperty("forwardRightClick").boolValue = true;
            clickArea.ApplyModifiedPropertiesWithoutUndo();
        }

        // The window holds the icon and shows it only within its arch
        private static RectTransform Window(Transform card, Sprites sprites, float x, float y, float width, float height)
        {
            RectTransform window = Place("Window", card, x, y, width, height);
            AddImage(window, sprites.WindowBack, Color.white);
            GetOrAdd<Mask>(window.gameObject).showMaskGraphic = true;
            return window;
        }

        private static void StatsButton(Transform card, Sprites sprites, TMP_FontAsset font, float y)
        {
            RectTransform button = Place("ButtonCharacter", card, 8f, y, cardWidth - 16f, 30f);
            Image pill = GetOrAdd<Image>(button.gameObject);
            pill.sprite = sprites.Pill;
            pill.type = Image.Type.Sliced;
            pill.color = Color.white;
            TextMeshProUGUI label = Style(Stretch("Text (TMP)", button), font, 14f, muted, TextAlignmentOptions.Center);
            label.text = "Stats";
        }

        private static void Price(Transform card, Sprites sprites, TMP_FontAsset font, string textName, float y)
        {
            AddImage(Place("Coin", card, 40f, y + 4f, 12f, 12f), sprites.Coin, Color.white);
            TextMeshProUGUI price = Style(Place(textName, card, 56f, y, 60f, 20f), font, 15f, ink, TextAlignmentOptions.Left);
            price.fontStyle = FontStyles.Bold;
        }

        private static void Token(Transform parent, Sprites sprites, TMP_FontAsset font, string name, float y)
        {
            RectTransform token = Place(name, parent, 0f, y, tokenSize, tokenSize);
            Image image = GetOrAdd<Image>(token.gameObject);
            image.sprite = sprites.Token;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            Style(Stretch("Text (TMP)", token), font, 12f, ink, TextAlignmentOptions.Center);
        }

        // Moves a child the card had at its top into the box it now sits in, keeping the object and what points at it
        private static void Adopt(Transform from, Transform to, string name)
        {
            Transform child = from.Find(name);
            if (child != null && to.Find(name) == null)
                child.SetParent(to, false);
        }

        // Takes off the layout groups, which placed the parts before, and hides the plain background they had
        private static void Unlay(GameObject target)
        {
            foreach (LayoutGroup group in target.GetComponents<LayoutGroup>())
                UnityEngine.Object.DestroyImmediate(group);
            foreach (ContentSizeFitter fitter in target.GetComponents<ContentSizeFitter>())
                UnityEngine.Object.DestroyImmediate(fitter);

            Image image = target.GetComponent<Image>();
            if (image != null && target.GetComponent<TowerShopComponent>() == null && target.GetComponent<EnemyShopComponent>() == null)
                image.enabled = false;
        }

        // Sets the look of a text and leaves what it says to the scenes and the scripts
        private static TextMeshProUGUI Style(RectTransform rect, TMP_FontAsset font, float size, Color color, TextAlignmentOptions alignment)
        {
            TextMeshProUGUI label = GetOrAdd<TextMeshProUGUI>(rect.gameObject);
            if (font != null)
                label.font = font;
            label.fontSize = size;
            label.enableAutoSizing = false;
            label.color = color;
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.richText = true;
            label.raycastTarget = false;
            return label;
        }


        private static void DressScene(Scene scene, Sprites sprites)
        {
            Transform shop = FindInScene(scene, shopName);
            if (shop != null)
            {
                Image column = GetOrAdd<Image>(shop.gameObject);
                column.sprite = sprites.Column;
                column.type = Image.Type.Sliced;
                column.color = Color.white;
            }

            // The cards take the size and the places of their parts from the prefab again
            IEnumerable<Component> cards = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<TowerShopComponent>(true).Cast<Component>()
                    .Concat(root.GetComponentsInChildren<EnemyShopComponent>(true)));
            foreach (Component card in cards)
            {
                GameObject instance = PrefabUtility.GetNearestPrefabInstanceRoot(card.gameObject);
                if (instance == null)
                    continue;

                PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(instance);
                PropertyModification[] kept = modifications
                    .Where(m => !(m.target is RectTransform && layoutProperties.Any(p => m.propertyPath.StartsWith(p))))
                    .ToArray();
                if (kept.Length != modifications.Length)
                    PrefabUtility.SetPropertyModifications(instance, kept);
            }
        }


        private static void EditPrefab(string path, Action<GameObject> layOut)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                layOut(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
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

        private static Transform FindInScene(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == name)
                        return t;
                }
            }
            return null;
        }
    }

}
