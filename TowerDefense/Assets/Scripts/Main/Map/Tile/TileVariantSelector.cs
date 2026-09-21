using System;

using UnityEngine;

using TowerDefense.Storage;


namespace TowerDefense.Main.Map.Tile
{
    public static class TileVariantSelector
    {
        private const string towerTileBasePrefabName = "TowerTileBase";

        private static TileVariantsStorage storage;

        private static int seedForRandomVariantSelection;

        public static void Reseed()
        {
            seedForRandomVariantSelection = Environment.TickCount;
        }

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


        public static GameObject GetBlockedTile(GameObject basePrefab, int row, int column)
        {
            GameObject variant = GetStorage() != null ? storage.GetBlockedTile(Hash(row, column)) : null;
            return variant != null ? variant : basePrefab;
        }


        public static void ApplyTowerTileLook(GameObject tile, int row, int column)
        {
            CopyLook(GetStorage() != null ? storage.GetTowerTile(towerTileBasePrefabName, Hash(row, column)) : null, tile);
        }

        public static void ApplyBlockedTileLook(GameObject tile, int row, int column)
        {
            CopyLook(GetStorage() != null ? storage.GetBlockedTile(Hash(row, column)) : null, tile);
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
            unchecked
            {
                int hash = row * 73856093 ^ column * 19349663 ^ seedForRandomVariantSelection * 83492791;

                hash ^= hash >> 16;
                hash *= (int)0x85ebca6b;
                hash ^= hash >> 13;

                return hash & int.MaxValue;
            }
        }

        private static TileVariantsStorage GetStorage()
        {
            if (storage == null)
                storage = Resources.Load<TileVariantsStorage>($"{nameof(TileVariantsStorage)}");

            return storage;
        }
    }

}
