using System.Collections.Generic;

using TMPro;

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


namespace TowerDefense.EditorTools.UI
{
    // Lays out UI made by the bakers, from the top left corner of each parent with y going down as in the mockups.
    // The objects that are there already are kept and set anew rather than made again, so a bake that changes nothing
    // leaves the prefab or the scene alone
    public static class UILayout
    {
        // What the bake has laid out, so whatever is left over from an older layout can go
        private static readonly HashSet<Transform> laidOut = new();


        public static void Begin(Transform root)
        {
            laidOut.Clear();
            laidOut.Add(root);
        }

        // Cinzel, after the lettering of Middle-earth, once its font asset is in the project; the default one till then
        public static TMP_FontAsset FindFont()
        {
            foreach (string guid in AssetDatabase.FindAssets("Cinzel t:TMP_FontAsset"))
            {
                TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (font != null)
                    return font;
            }
            return TMP_Settings.defaultFontAsset;
        }

        // A box placed from the top left corner of its parent, with y going down
        // The child of that name is taken if the parent has one, and put after the ones laid out before it
        public static RectTransform Place(string name, Transform parent, float x, float y, float width, float height)
        {
            Transform found = parent.Find(name);
            RectTransform rect = found != null && !laidOut.Contains(found) && !PrefabUtility.IsAnyPrefabInstanceRoot(found.gameObject)
                ? found as RectTransform
                : null;
            if (rect == null)
            {
                GameObject placed = new(name, typeof(RectTransform));
                rect = (RectTransform)placed.transform;
                rect.SetParent(parent, false);
            }

            laidOut.Add(rect);
            rect.gameObject.layer = parent.gameObject.layer;
            rect.SetSiblingIndex(PlacedChildren(parent) - 1);
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        public static RectTransform Stretch(string name, Transform parent)
        {
            RectTransform rect = Place(name, parent, 0f, 0f, 0f, 0f);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        public static int PlacedChildren(Transform parent)
        {
            int count = 0;
            foreach (Transform child in parent)
            {
                if (laidOut.Contains(child))
                    count++;
            }
            return count;
        }

        public static void RemoveLeftOvers(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (laidOut.Contains(child))
                    RemoveLeftOvers(child);
                else
                    Object.DestroyImmediate(child.gameObject);
            }
        }

        public static T GetOrAdd<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        public static Image AddImage(RectTransform rect, Sprite sprite, Color color)
        {
            Image image = GetOrAdd<Image>(rect.gameObject);
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        public static Image MakeFilled(Image image)
        {
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
            image.fillAmount = 1f;
            return image;
        }

        public static TextMeshProUGUI AddText(RectTransform rect, TMP_FontAsset font, string text, float size, Color color, TextAlignmentOptions alignment)
        {
            TextMeshProUGUI label = GetOrAdd<TextMeshProUGUI>(rect.gameObject);
            if (font != null)
                label.font = font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = alignment;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            return label;
        }
    }

}
