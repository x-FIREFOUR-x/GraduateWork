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

        private List<List<GameObject>> tiles;
        public int Size { get; private set; }


        public void Initialize(int _size, List<Vector2Int> generatedPath, Vector3 offsetTile)
        {
            Size = _size;

            Vector3 position = this.transform.position;
            Quaternion rotation = this.transform.rotation;

            TileVariantSelector.Reseed();
            tiles = new List<List<GameObject>>();

            for (int i = 0; i < Size; i++)
            {
                position.x = 0;
                List<GameObject> lineTiles = new();

                for (int j = 0; j < Size; j++)
                {
                    int indexInPath = generatedPath.IndexOf(new Vector2Int(i, j));
                    if (indexInPath >= 0)
                    {
                        int bitMaskForPathsConnections = TilePathConnections.GetBitMaskFromPathNeighbors(generatedPath, indexInPath);
                        GameObject prefab = TileVariantSelector.GetPathTile(pathTilePrefab, bitMaskForPathsConnections, i, j);
                        GameObject tile = Instantiate(prefab, position, rotation, this.transform);
                        lineTiles.Add(tile);
                    }
                    else
                    {
                        GameObject prefab = TileVariantSelector.GetTowerTile(towerTilePrefab, i, j);
                        GameObject tile = Instantiate(prefab, position, rotation, this.transform);
                        lineTiles.Add(tile);
                    }

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
    }

}
