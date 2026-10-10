using TMPro;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using TowerDefense.MapConstructor.UI;

using static TowerDefense.EditorTools.UI.UILayout;


namespace TowerDefense.EditorTools.UI
{
    // Dresses the menu of the map constructor as the shop of the game: the dark columns with a golden edge, the play
    // and back buttons as golden pills, and each component a card with its name over a golden line, its icon in an
    // arched window and a golden frame when it is chosen; and the message that the map cannot be played a dark window
    // with a golden frame. The sprites are the ones it shares with the shop and the rest
    // of the UI, baked through SideMenuSprites and CommonSprites
    public static class ConstructorMenuBuilder
    {
        private const string scenePath = "Assets/Scenes/MapConstructorScene.unity";
        private const string menuName = "Menu";
        private const string selectMenuName = "SelectMenu";
        private const string messageName = "Message";
        private static readonly string[] menuButtonNames = { "StartPlayDefender", "StartPlayAttacker", "BackMainMenu" };

        // Low enough for all four cards to fit the column, which does not scroll
        private const float cardWidth = 136f;
        private const float cardHeight = 112f;
        private const float glowMargin = SideMenuSprites.GlowMargin;
        private const float menuButtonHeight = 34f;
        private const float messageWidth = 500f;
        private const float messageHeight = 170f;

        private static readonly Color ink = SpriteRaster.Hex("f3e6c0");
        private static readonly Color unchosenTint = new(0.8f, 0.8f, 0.8f, 1f);
        // Darkens the map behind a message and keeps the clicks off it
        private static readonly Color messageBackdrop = new(0.02f, 0.03f, 0.02f, 0.6f);

        // The buttons of the components and their icons, as the select menu holds them
        private static readonly (string button, string icon)[] components =
        {
            ("pathTileButton", "pathTileIcon"),
            ("blockedTileButton", "blockedTileIcon"),
            ("startBuildingButton", "startBuildingIcon"),
            ("endBuildingButton", "endBuildingIcon"),
        };


        [MenuItem("Tools/UI/Build Constructor Menu")]
        public static void Build()
        {
            Sprites sprites = BakeSprites();

            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedHere = !scene.isLoaded;
            if (openedHere)
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            try
            {
                DressScene(scene, sprites);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                if (openedHere)
                    EditorSceneManager.CloseScene(scene, true);
            }

            Debug.Log("Constructor menu built");
        }


        private class Sprites
        {
            public Sprite Column, Card, Selected, Diamond, NameLine, WindowBack, WindowFrame, Pill, Dialog;
        }

        private static Sprites BakeSprites()
        {
            return new Sprites
            {
                Column = SideMenuSprites.Column(),
                Card = CommonSprites.Card(),
                Selected = SideMenuSprites.Selected(),
                Diamond = SideMenuSprites.Diamond(),
                NameLine = SideMenuSprites.NameLine(),
                WindowBack = SideMenuSprites.WindowBack(),
                WindowFrame = SideMenuSprites.WindowFrame(),
                Pill = CommonSprites.Pill(),
                Dialog = CommonSprites.Dialog(),
            };
        }


        private static void DressScene(Scene scene, Sprites sprites)
        {
            TMP_FontAsset font = FindFont();

            Transform menu = FindInScene(scene, menuName);
            if (menu != null)
            {
                Column(menu, sprites);
                foreach (string buttonName in menuButtonNames)
                {
                    Transform button = menu.Find(buttonName);
                    if (button != null)
                        MenuButton(button, sprites, font);
                }
            }

            Transform message = FindInScene(scene, messageName);
            if (message != null)
                DressMessage(message, sprites, font);

            Transform select = FindInScene(scene, selectMenuName);
            SelectMenu selectMenu = select != null ? select.GetComponent<SelectMenu>() : null;
            if (selectMenu == null)
            {
                Debug.LogWarning($"{selectMenuName} not found in {scenePath}");
                return;
            }

            Column(select, sprites);

            SerializedObject serialized = new(selectMenu);
            foreach ((string button, string icon) in components)
            {
                GameObject card = serialized.FindProperty(button).objectReferenceValue as GameObject;
                Sprite iconSprite = serialized.FindProperty(icon).objectReferenceValue as Sprite;
                if (card != null)
                    LayOutCard(card, iconSprite, sprites, font);
            }
            serialized.FindProperty("unactiveColor").colorValue = unchosenTint;
            serialized.FindProperty("activeColor").colorValue = Color.white;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Column(Transform column, Sprites sprites)
        {
            Image image = GetOrAdd<Image>(column.gameObject);
            image.sprite = sprites.Column;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }

        private static void MenuButton(Transform button, Sprites sprites, TMP_FontAsset font)
        {
            ((RectTransform)button).sizeDelta = new Vector2(cardWidth, menuButtonHeight);

            Image pill = GetOrAdd<Image>(button.gameObject);
            pill.sprite = sprites.Pill;
            pill.type = Image.Type.Sliced;
            pill.color = Color.white;

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
                Style(label, font, 15f);
        }

        // A card the select menu tints when it is chosen or not, and shows its golden frame on when it is chosen.
        // The icon goes into the window here, so the menu finds it there rather than adding its own
        private static void LayOutCard(GameObject card, Sprite icon, Sprites sprites, TMP_FontAsset font)
        {
            Begin(card.transform);
            ((RectTransform)card.transform).sizeDelta = new Vector2(cardWidth, cardHeight);

            Image background = GetOrAdd<Image>(card);
            background.sprite = sprites.Card;
            background.type = Image.Type.Sliced;
            background.raycastTarget = true;

            Style(GetOrAdd<TextMeshProUGUI>(Place("Text (TMP)", card.transform, 4f, 4f, cardWidth - 8f, 16f).gameObject), font, 13f);
            AddImage(Place("NameLine", card.transform, (cardWidth - 66f) / 2f, 21f, 66f, 2f), sprites.NameLine, Color.white);

            const float windowWidth = 90f;
            const float windowHeight = 84f;
            const float windowY = 24f;
            float windowX = (cardWidth - windowWidth) / 2f;
            RectTransform window = Place("Window", card.transform, windowX, windowY, windowWidth, windowHeight);
            AddImage(window, sprites.WindowBack, Color.white);
            GetOrAdd<Mask>(window.gameObject).showMaskGraphic = true;
            Image iconImage = AddImage(Place("Icon", window, 7f, 4f, 76f, 76f), icon, Color.white);
            iconImage.preserveAspect = true;
            AddImage(Place("WindowFrame", card.transform, windowX, windowY, windowWidth, windowHeight), sprites.WindowFrame, Color.white);

            RectTransform selected = Place(SelectMenu.selectedFramePath, card.transform, -glowMargin, -glowMargin, cardWidth + glowMargin * 2f, cardHeight + glowMargin * 2f);
            AddImage(selected, sprites.Selected, Color.white).type = Image.Type.Sliced;
            AddImage(Place("Diamond", selected, (cardWidth + glowMargin * 2f - 14f) / 2f, glowMargin - 4.5f, 14f, 9f), sprites.Diamond, Color.white);
            selected.gameObject.SetActive(false);

            RemoveLeftOvers(card.transform);
        }

        // The message that the map cannot be played: a dark window with a golden frame over the darkened map, the text
        // under a diamond on its frame, a golden line under the text and the close button under that
        private static void DressMessage(Transform message, Sprites sprites, TMP_FontAsset font)
        {
            Image backdrop = GetOrAdd<Image>(message.gameObject);
            backdrop.color = messageBackdrop;
            backdrop.raycastTarget = true;

            Transform panel = message.Find("Panel");
            if (panel == null)
                return;

            Begin(panel);
            RectTransform panelRect = (RectTransform)panel;
            panelRect.sizeDelta = new Vector2(messageWidth, messageHeight);

            Image window = GetOrAdd<Image>(panel.gameObject);
            window.sprite = sprites.Dialog;
            window.type = Image.Type.Sliced;
            window.color = Color.white;

            AddImage(Place("Diamond", panel, (messageWidth - 14f) / 2f, -4f, 14f, 9f), sprites.Diamond, Color.white);

            TextMeshProUGUI text = GetOrAdd<TextMeshProUGUI>(Place("Text (TMP)", panel, 30f, 26f, messageWidth - 60f, 64f).gameObject);
            Style(text, font, 26f);
            text.textWrappingMode = TextWrappingModes.Normal;

            AddImage(Place("Divider", panel, (messageWidth - 200f) / 2f, 98f, 200f, 2f), sprites.NameLine, Color.white);

            RectTransform button = Place("Button", panel, (messageWidth - 160f) / 2f, 114f, 160f, 40f);
            Image pill = GetOrAdd<Image>(button.gameObject);
            pill.sprite = sprites.Pill;
            pill.type = Image.Type.Sliced;
            pill.color = Color.white;
            Style(GetOrAdd<TextMeshProUGUI>(Stretch("Text (TMP)", button).gameObject), font, 18f);

            RemoveLeftOvers(panel);
        }

        private static void Style(TextMeshProUGUI label, TMP_FontAsset font, float size)
        {
            if (font != null)
                label.font = font;
            label.fontSize = size;
            label.enableAutoSizing = false;
            label.color = ink;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
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
