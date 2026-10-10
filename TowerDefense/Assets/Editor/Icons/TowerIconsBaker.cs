using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using TowerDefense.Storage;


namespace TowerDefense.EditorTools.Icons
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
        // Towers that fill their box more, by prefab name. Kept here so the menu item and a tower's own baker frame
        // the icon alike; with two zooms every bake of both would rewrite the icon back and forth
        private static readonly Dictionary<string, float> zoomByTower = new()
        {
            // Tall, with ice flowing down to the ground all round
            { "IceMageTower", 1.15f },
        };


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

        // The tower may be prefab contents its baker is still editing, so its own name gives the icon name and
        // its zoom
        public static void BakeIcon(GameObject tower)
        {
            IconRenderer.EnsureFolder(iconsFolder);
            float towerZoom = zoomByTower.TryGetValue(tower.name, out float own) ? own : zoom;
            IconRenderer.Render(tower, IconPath(tower.name), cameraYaw, towerZoom);
        }

        public static string IconPath(string towerName)
        {
            return $"{iconsFolder}/Icon{towerName}.png";
        }
    }

}
