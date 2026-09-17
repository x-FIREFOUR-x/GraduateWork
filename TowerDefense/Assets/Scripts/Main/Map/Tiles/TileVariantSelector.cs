using UnityEngine;

using TowerDefense.Storage;


namespace TowerDefense.Main.Map.Tiles
{
    public static class TileVariantSelector
    {
        private const string lookTowerTileName = "TowerTile";

        private static TileVariantsStorage storage;


        public static GameObject GetTowerTile(GameObject basePrefab, int row, int column)
        {
            GameObject variant = GetStorage() != null ? storage.GetTowerTile(basePrefab.name, Hash(row, column)) : null;
            return variant != null ? variant : basePrefab;
        }

        public static GameObject GetPathTile(GameObject basePrefab, int bitMaskForPathsConnections, int row, int column)
        {
            GameObject variant = GetStorage() != null ? storage.GetPathTile(bitMaskForPathsConnections, Hash(row, column)) : null;
            return variant != null ? variant : basePrefab;
        }


        public static void ApplyTowerTileLook(GameObject tile, int row, int column)
        {
            CopyLook(GetStorage() != null ? storage.GetTowerTile(lookTowerTileName, Hash(row, column)) : null, tile);
        }

        public static void ApplyPathTileLook(GameObject tile, int bitMaskForPathsConnections, int row, int column)
        {
            CopyLook(GetStorage() != null ? storage.GetPathTile(bitMaskForPathsConnections, Hash(row, column)) : null, tile);
        }


        private static void CopyLook(GameObject source, GameObject target)
        {
            if (source == null)
                return;

            target.GetComponent<MeshFilter>().sharedMesh = source.GetComponent<MeshFilter>().sharedMesh;
            target.GetComponent<Renderer>().sharedMaterials = source.GetComponent<Renderer>().sharedMaterials;
        }

        private static int Hash(int row, int column)
        {
            return ((row * 73856093) ^ (column * 19349663)) & int.MaxValue;
        }

        private static TileVariantsStorage GetStorage()
        {
            if (storage == null)
                storage = Resources.Load<TileVariantsStorage>($"{nameof(TileVariantsStorage)}");

            return storage;
        }
    }

}
