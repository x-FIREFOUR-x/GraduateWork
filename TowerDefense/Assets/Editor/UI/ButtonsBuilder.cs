using TMPro;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using TowerDefense.CameraControl.UI;

using static TowerDefense.EditorTools.UI.UILayout;


namespace TowerDefense.EditorTools.UI
{
    // Gives the buttons outside the side menus its look: the golden pill with the lettering of the shop for the main
    // menu, the pause and the end of the game, the same pill for the camera controls on a dark card the
    // map shows through, with a broad golden frame on the button that opens them, and a golden token to close the menu that
    // sells a tower. The sprites are the ones all of the UI shares, baked
    // through CommonSprites
    public static class ButtonsBuilder
    {
        private const string sellMenuPath = "Assets/Prefabs/UI/UITowerTile.prefab";

        private static readonly (string scene, string[] buttons)[] sceneButtons =
        {
            ("Assets/Scenes/MainMenuScene.unity", new[]
            {
                "Canvas/Menu/ButtonPlayDefender", "Canvas/Menu/ButtonPlayAttacker", "Canvas/Menu/ButtonConstructor", "Canvas/Menu/ButtonQuit",
            }),
            ("Assets/Scenes/DefenderGameScene.unity", GameButtons()),
            ("Assets/Scenes/AttackerGameScene.unity", GameButtons()),
            ("Assets/Scenes/MapConstructorScene.unity", new string[0]),
        };

        private static readonly Color ink = SpriteRaster.Hex("f3e6c0");
        // The card behind the camera controls, let through so the map shows under it
        private static readonly Color cameraBackgroundTint = new(1f, 1f, 1f, 0.5f);
        private static readonly Color highlighted = new(1f, 0.93f, 0.78f, 1f);
        private static readonly Color pressed = new(0.78f, 0.72f, 0.6f, 1f);
        private static readonly Color disabled = new(0.6f, 0.6f, 0.6f, 0.6f);
        private const float closeTokenSize = 28f;


        [MenuItem("Tools/UI/Build Buttons")]
        public static void Build()
        {
            Sprite pill = CommonSprites.Pill();
            Sprite card = CommonSprites.Card();
            Sprite highlightedPill = CommonSprites.PillHighlighted();
            Sprite token = CommonSprites.Token();

            TMP_FontAsset font = FindFont();

            foreach ((string scenePath, string[] buttons) in sceneButtons)
                EditScene(scenePath, scene => DressScene(scene, buttons, pill, highlightedPill, card, font));

            DressSellMenu(pill, token, font);

            AssetDatabase.SaveAssets();
            Debug.Log("Buttons built");
        }

        private static string[] GameButtons()
        {
            return new[]
            {
                "Canvas/ButtonPause", "Canvas/PauseMenu/ButtonContinue", "Canvas/PauseMenu/ButtonMenu",
                "Canvas/GameOverMenu/ButtonMenu", "Canvas/GameWinMenu/ButtonMenu",
            };
        }


        private static void DressScene(Scene scene, string[] buttons, Sprite pill, Sprite highlightedPill, Sprite card, TMP_FontAsset font)
        {
            foreach (string path in buttons)
            {
                Transform button = FindByPath(scene, path);
                if (button == null)
                {
                    Debug.LogWarning($"{path} not found in {scene.path}");
                    continue;
                }
                Pill(button, pill, font);
            }

            // The camera controls make their buttons as the game starts, so they are only handed the look to make them with
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (CameraControlsUI controls in root.GetComponentsInChildren<CameraControlsUI>(true))
                {
                    SerializedObject serialized = new(controls);
                    serialized.FindProperty("buttonSprite").objectReferenceValue = pill;
                    serialized.FindProperty("backgroundSprite").objectReferenceValue = card;
                    serialized.FindProperty("backgroundColor").colorValue = cameraBackgroundTint;
                    serialized.FindProperty("labelColor").colorValue = ink;
                    serialized.FindProperty("openCloseButtonSprite").objectReferenceValue = highlightedPill;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        // The menu over a built tower: the sell button a pill, and the close button a golden token with a cross
        private static void DressSellMenu(Sprite pill, Sprite token, TMP_FontAsset font)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(sellMenuPath);
            try
            {
                Transform sell = root.transform.Find("Canvas/ButtonSell");
                if (sell != null)
                    Pill(sell, pill, font);

                Transform close = root.transform.Find("Canvas/ButtonClose");
                if (close != null)
                {
                    Transform mark = close.Find("Image");
                    if (mark != null)
                    {
                        Image image = mark.GetComponent<Image>();
                        image.sprite = token;
                        image.type = Image.Type.Simple;
                        image.color = Color.white;
                        ((RectTransform)mark).sizeDelta = new Vector2(closeTokenSize, closeTokenSize);
                    }

                    TextMeshProUGUI cross = close.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (cross != null)
                    {
                        Style(cross, font);
                        cross.text = "X";
                        cross.fontSize = 14f;
                        cross.transform.SetAsLastSibling();
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(root, sellMenuPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }


        private static void Pill(Transform button, Sprite pill, TMP_FontAsset font)
        {
            Image image = GetOrAdd<Image>(button.gameObject);
            image.sprite = pill;
            image.type = Image.Type.Sliced;
            image.color = Color.white;

            Button clickable = button.GetComponent<Button>();
            if (clickable != null)
            {
                ColorBlock colors = clickable.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = highlighted;
                colors.pressedColor = pressed;
                colors.selectedColor = Color.white;
                colors.disabledColor = disabled;
                clickable.colors = colors;
            }

            foreach (TextMeshProUGUI label in button.GetComponentsInChildren<TextMeshProUGUI>(true))
                Style(label, font);
        }

        // The lettering and its colour, leaving the size and what it says as they are
        private static void Style(TextMeshProUGUI label, TMP_FontAsset font)
        {
            if (font != null)
                label.font = font;
            label.color = ink;
            label.raycastTarget = false;
        }


        private static Transform FindByPath(Scene scene, string path)
        {
            int slash = path.IndexOf('/');
            string rootName = slash < 0 ? path : path.Substring(0, slash);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name != rootName)
                    continue;
                Transform found = slash < 0 ? root.transform : root.transform.Find(path.Substring(slash + 1));
                if (found != null)
                    return found;
            }
            return null;
        }

        private static void EditScene(string path, System.Action<Scene> edit)
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
    }

}
