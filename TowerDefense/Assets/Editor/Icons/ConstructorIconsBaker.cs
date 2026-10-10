using UnityEditor;
using UnityEngine;


namespace TowerDefense.EditorTools.Icons
{
    // Renders the components the map constructor offers into sprites for its buttons, so a button shows what
    // the cell will look like. SelectMenu picks the baked sprites up through its inspector fields.
    public static class ConstructorIconsBaker
    {
        private const string iconsFolder = "Assets/Sprites/Icons/MapConstructor";

        private static readonly (string prefabPath, string iconName)[] icons =
        {
            ("Assets/Prefabs/Map/Tile/PathTile/PathTile_Straight_NS.prefab", "IconPathTile"),
            ("Assets/Prefabs/Map/Tile/BlockedTile/BlockedTile_00.prefab", "IconBlockedTile"),
            ("Assets/Prefabs/Map/Building/StartBuilding.prefab", "IconStartBuilding"),
            ("Assets/Prefabs/Map/Building/EndBuilding.prefab", "IconEndBuilding"),
        };


        [MenuItem("Tools/Tile/Bake Constructor Icons")]
        public static void Bake()
        {
            IconRenderer.EnsureFolder(iconsFolder);

            foreach (var icon in icons)
                IconRenderer.Render(icon.prefabPath, $"{iconsFolder}/{icon.iconName}.png");

            AssetDatabase.SaveAssets();
            Debug.Log("Constructor icons baked");
        }
    }

}
