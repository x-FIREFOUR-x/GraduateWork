using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Serialization;


namespace TowerDefense.Storage
{
    public class TileVariantsStorage : ScriptableObject
    {
        [Serializable]
        public class TowerTileVariants
        {
            public string BasePrefabName;
            public List<GameObject> Prefabs = new();
        }

        [Serializable]
        public class PathTileVariants
        {
            [FormerlySerializedAs("Mask"), FormerlySerializedAs("Connections")]
            public int BitMaskForPathsConnections;
            public List<GameObject> Prefabs = new();
        }

        [field: SerializeField] public List<TowerTileVariants> TowerTiles { get; private set; } = new();
        [field: SerializeField] public List<PathTileVariants> PathTiles { get; private set; } = new();
        [field: SerializeField] public List<GameObject> BlockedTiles { get; private set; } = new();


        public void SetTiles(List<TowerTileVariants> towerTiles, List<PathTileVariants> pathTiles, List<GameObject> blockedTiles)
        {
            TowerTiles = towerTiles;
            PathTiles = pathTiles;
            BlockedTiles = blockedTiles;
        }

        public GameObject GetTowerTile(string basePrefabName, int hash)
        {
            TowerTileVariants variants = TowerTiles.Find(towerTileVariants => towerTileVariants.BasePrefabName == basePrefabName);
            return variants != null && variants.Prefabs.Count > 0 ? variants.Prefabs[hash % variants.Prefabs.Count] : null;
        }

        public GameObject GetBlockedTile(int hash)
        {
            return BlockedTiles.Count > 0 ? BlockedTiles[hash % BlockedTiles.Count] : null;
        }

        public GameObject GetPathTile(int bitMaskForPathsConnections, int hash)
        {
            PathTileVariants variants = PathTiles.Find(pathTileVariants => pathTileVariants.BitMaskForPathsConnections == bitMaskForPathsConnections);
            return variants != null && variants.Prefabs.Count > 0 ? variants.Prefabs[hash % variants.Prefabs.Count] : null;
        }
    }

}
