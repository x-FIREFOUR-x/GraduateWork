using System.Collections.Generic;

using TMPro;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;


namespace TowerDefense.EditorTools.UI
{
    // Puts the lettering of Middle-earth on every text of the game: makes a TextMesh Pro font asset of Cinzel, sets it
    // as the default one for new texts and gives it to each text in the prefabs and the scenes. The font the texts had
    // before stays behind it as a fallback, for the signs Cinzel does not have
    public static class FontsBuilder
    {
        private const string fontPath = "Assets/Fonts/Cinzel/Cinzel-Variable.ttf";
        private const string fontAssetPath = "Assets/Fonts/Cinzel/Cinzel SDF.asset";
        private const string prefabsFolder = "Assets/Prefabs";
        private const string scenesFolder = "Assets/Scenes";

        private const int samplingPointSize = 90;
        private const int atlasPadding = 9;
        private const int atlasSize = 1024;


        [MenuItem("Tools/UI/Build Fonts")]
        public static void Build()
        {
            TMP_FontAsset cinzel = MakeFontAsset();
            if (cinzel == null)
                return;

            SetAsDefault(cinzel);

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { prefabsFolder }))
                DressPrefab(AssetDatabase.GUIDToAssetPath(guid), cinzel);
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { scenesFolder }))
                DressScene(AssetDatabase.GUIDToAssetPath(guid), cinzel);

            AssetDatabase.SaveAssets();
            Debug.Log("Fonts built");
        }


        // A dynamic font asset, which draws each letter into its atlas the first time a text needs it
        private static TMP_FontAsset MakeFontAsset()
        {
            TMP_FontAsset made = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontAssetPath);
            if (made != null)
                return made;

            Font font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (font == null)
            {
                Debug.LogError($"The font is not at {fontPath}");
                return null;
            }

            made = TMP_FontAsset.CreateFontAsset(font, samplingPointSize, atlasPadding, GlyphRenderMode.SDFAA,
                atlasSize, atlasSize, AtlasPopulationMode.Dynamic, true);
            made.name = "Cinzel SDF";

            TMP_FontAsset previous = TMP_Settings.defaultFontAsset;
            if (previous != null && previous != made)
                made.fallbackFontAssetTable = new List<TMP_FontAsset> { previous };

            AssetDatabase.CreateAsset(made, fontAssetPath);
            made.material.name = made.name + " Material";
            AssetDatabase.AddObjectToAsset(made.material, made);
            foreach (Texture2D atlas in made.atlasTextures)
            {
                atlas.name = made.name + " Atlas";
                AssetDatabase.AddObjectToAsset(atlas, made);
            }
            AssetDatabase.SaveAssets();
            return made;
        }

        private static void SetAsDefault(TMP_FontAsset cinzel)
        {
            SerializedObject settings = new(TMP_Settings.instance);
            SerializedProperty defaultFont = settings.FindProperty("m_defaultFontAsset");
            if (defaultFont.objectReferenceValue == cinzel)
                return;

            defaultFont.objectReferenceValue = cinzel;
            settings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(TMP_Settings.instance);
        }

        // Only the texts with another font are touched, so a bake that changes nothing leaves the files alone
        private static bool Dress(IEnumerable<TMP_Text> texts, TMP_FontAsset cinzel)
        {
            bool changed = false;
            foreach (TMP_Text text in texts)
            {
                if (text.font == cinzel)
                    continue;

                text.font = cinzel;
                text.fontSharedMaterial = cinzel.material;
                changed = true;
            }
            return changed;
        }

        private static void DressPrefab(string path, TMP_FontAsset cinzel)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (Dress(root.GetComponentsInChildren<TMP_Text>(true), cinzel))
                    PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void DressScene(string path, TMP_FontAsset cinzel)
        {
            Scene scene = SceneManager.GetSceneByPath(path);
            bool openedHere = !scene.isLoaded;
            if (openedHere)
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            try
            {
                List<TMP_Text> texts = new();
                foreach (GameObject root in scene.GetRootGameObjects())
                    texts.AddRange(root.GetComponentsInChildren<TMP_Text>(true));

                if (Dress(texts, cinzel))
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
            finally
            {
                if (openedHere)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }
    }

}
