using System.Collections.Generic;

using UnityEngine;


namespace TowerDefense.Main.Map.Tile
{
    public class TilesMap : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField]
        private GameObject towerTilePrefab;
        [SerializeField]
        private GameObject pathTilePrefab;
        [SerializeField]
        private GameObject blockedTilePrefab;

        private List<List<GameObject>> tiles;
        public int Size { get; private set; }


        public void Initialize(int _size, List<Vector2Int> generatedPath, HashSet<Vector2Int> blockedTiles, Vector3 offsetTile)
        {
            Size = _size;

            Vector3 position = this.transform.position;
            Quaternion rotation = this.transform.rotation;

            tiles = new List<List<GameObject>>();

            for (int i = 0; i < Size; i++)
            {
                position.x = 0;
                List<GameObject> lineTiles = new();

                for (int j = 0; j < Size; j++)
                {
                    GameObject prefab = PrefabForTile(i, j, generatedPath, blockedTiles);
                    GameObject tile = Instantiate(prefab, position, rotation, this.transform);
                    lineTiles.Add(tile);

                    position.x += towerTilePrefab.transform.localScale.x + offsetTile.z;
                }
                position.z += towerTilePrefab.transform.localScale.z + offsetTile.z;
                tiles.Add(lineTiles);
            }
        }

        public GameObject GetTileAt(int row, int column)
        {
            return tiles[row][column];
        }

        public TowerTile GetTowerTileAt(int row, int column)
        {
            return GetTileAt(row, column).GetComponent<TowerTile>();
        }


        private GameObject PrefabForTile(int row, int column, List<Vector2Int> generatedPath, HashSet<Vector2Int> blockedTiles)
        {
            int indexInPath = generatedPath.IndexOf(new Vector2Int(row, column));
            if (indexInPath >= 0)
            {
                int bitMaskForPathsConnections = TilePathConnections.GetBitMaskFromPathNeighbors(generatedPath, indexInPath);
                return TileVariantSelector.GetPathTile(pathTilePrefab, bitMaskForPathsConnections, row, column);
            }

            if (blockedTiles.Contains(new Vector2Int(row, column)))
                return TileVariantSelector.GetBlockedTile(blockedTilePrefab, row, column);

            return TileVariantSelector.GetTowerTile(towerTilePrefab, row, column);
        }
    }

}
