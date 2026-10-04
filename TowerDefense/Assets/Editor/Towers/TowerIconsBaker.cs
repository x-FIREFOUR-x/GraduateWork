using UnityEditor;
using UnityEngine;

using TowerDefense.EditorTools.Icons;
using TowerDefense.Storage;


namespace TowerDefense.EditorTools.Towers
{
    // Renders the towers of the TowersStorage into the sprites of the tower shop, one per tower, named after its
    // prefab: Icon<Prefab name>.png. Towers are seen from the front left, so the side facing the enemies shows.
    // A tower baker calls BakeIcon for its own tower right after building it; the menu item rebakes them all
    public static class TowerIconsBaker
    {
        private const string iconsFolder = "Assets/Sprites/Icons/Towers";
        private const string storagePath = "Assets/Resources/" + nameof(TowersStorage) + ".asset";

        // A tower faces +Z; this camera stands in front of it and to its left
        private const float cameraYaw = 135f;
        // A tower is narrower than its bounding box, seen from the side the box leaves much empty space around it
        private const float zoom = 1.4f;


        [MenuItem("Tools/Towers/Bake Tower Icons")]
        public static void BakeAll()
        {
            TowersStorage storage = AssetDatabase.LoadAssetAtPath<TowersStorage>(storagePath);
            if (storage == null)
            {
                Debug.LogError($"Towers storage not found: {storagePath}");
                return;
            }

            foreach (GameObject tower in storage.Towers)
            {
                if (tower != null)
                    BakeIcon(tower);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Tower icons baked");
        }

        // The tower may be prefab contents its baker is still editing, so its own name gives the icon name.
        // A tower much taller than wide fills more of its bounding box and may ask for less zoom, so it is not cut off
        public static void BakeIcon(GameObject tower, float towerZoom = zoom)
        {
            IconRenderer.EnsureFolder(iconsFolder);
            IconRenderer.Render(tower, IconPath(tower.name), cameraYaw, towerZoom);
        }

        public static string IconPath(string towerName)
        {
            return $"{iconsFolder}/Icon{towerName}.png";
        }
    }

}
