using UnityEditor;
using UnityEngine;

using TowerDefense.Storage;


namespace TowerDefense.EditorTools.Icons
{
    public static class EnemyIconsBaker
    {
        private const string iconsFolder = "Assets/Sprites/Icons/Enemies";
        private const string storagePath = "Assets/Resources/" + nameof(EnemiesStorage) + ".asset";

        private const float cameraYaw = 135f;
        private const float zoom = 1.1f;


        [MenuItem("Tools/Enemies/Bake Enemy Icons")]
        public static void BakeAll()
        {
            EnemiesStorage storage = AssetDatabase.LoadAssetAtPath<EnemiesStorage>(storagePath);
            if (storage == null)
            {
                Debug.LogError($"Enemies storage not found: {storagePath}");
                return;
            }

            foreach (GameObject enemy in storage.Enemies)
            {
                if (enemy != null)
                    BakeIcon(enemy);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Enemy icons baked");
        }

        public static void BakeIcon(GameObject enemy)
        {
            IconRenderer.EnsureFolder(iconsFolder);
            Transform model = enemy.transform.Find("Model");
            IconRenderer.Render(model != null ? model.gameObject : enemy, IconPath(enemy.name), cameraYaw, zoom);
        }

        public static string IconPath(string enemyName)
        {
            return $"{iconsFolder}/Icon{enemyName}.png";
        }
    }

}
