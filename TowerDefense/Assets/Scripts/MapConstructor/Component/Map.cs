using UnityEngine;

using TowerDefense.Main.Map.Tile;


namespace TowerDefense.MapConstructor.Component
{
    public class Map : MonoBehaviour
    {
        private int size = 16;

        private GameObject[,] tiles;
        private GameObject endBuilding;
        private GameObject startBuilding;

        private Vector3 offsetBuild;

        private Vector3 stepTile;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject towerTilePrefab;
        [SerializeField]
        private GameObject pathTilePrefab;
        [SerializeField]
        private GameObject blockedTilePrefab;


        public void Initialize(int size, Vector3 startPosition, Vector3 offsetBuild, Vector3 stepTile)
        {
            this.transform.position = startPosition;
            this.size = size;
            this.offsetBuild = offsetBuild;
            this.stepTile = stepTile;

            endBuilding = null;
            startBuilding = null;

            tiles = new GameObject[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    PlaceTile(i, j, TileKind.Tower);
                }
            }
        }

        public void SetTile(int i, int j, GameObject newTile)
        {
            PlaceTile(i, j, KindOfPrefab(newTile));

            // Neighbor path tiles change shape when this cell becomes or stops being a path
            RefreshPathTile(i + 1, j);
            RefreshPathTile(i - 1, j);
            RefreshPathTile(i, j + 1);
            RefreshPathTile(i, j - 1);
        }

        // i grows along +X (east), j along +Z (north)
        private void PlaceTile(int i, int j, TileKind kind)
        {
            if (tiles[i, j] != null)
                Destroy(tiles[i, j]);

            GameObject prefab = kind switch
            {
                TileKind.Path => pathTilePrefab,
                TileKind.Blocked => blockedTilePrefab,
                _ => towerTilePrefab
            };

            GameObject tile = Instantiate(prefab, getCoordinate(i, j), this.transform.rotation, this.transform);
            tiles[i, j] = tile;

            // Constructor tiles look the same as the game tiles on this cell
            switch (kind)
            {
                case TileKind.Path:
                    TileVariantSelector.ApplyPathTileLook(tile, GetBitMaskForPathsConnections(i, j), j, i);
                    break;
                case TileKind.Blocked:
                    TileVariantSelector.ApplyBlockedTileLook(tile, j, i);
                    break;
                default:
                    TileVariantSelector.ApplyTowerTileLook(tile, j, i);
                    break;
            }
        }

        private TileKind KindOfPrefab(GameObject prefab)
        {
            if (prefab.GetComponent<ConstructorPathTile>() != null)
                return TileKind.Path;

            return prefab.GetComponent<ConstructorBlockedTile>() != null ? TileKind.Blocked : TileKind.Tower;
        }

        private void RefreshPathTile(int i, int j)
        {
            if (IsPathTile(i, j))
                PlaceTile(i, j, TileKind.Path);
        }

        private int GetBitMaskForPathsConnections(int i, int j)
        {
            return TilePathConnections.GetBitMaskFromConnectedSides(IsPathTile(i, j + 1), IsPathTile(i + 1, j), IsPathTile(i, j - 1), IsPathTile(i - 1, j));
        }

        private bool IsPathTile(int i, int j)
        {
            return IsInside(i, j) && tiles[i, j] != null && tiles[i, j].GetComponent<ConstructorPathTile>() != null;
        }

        private bool IsInside(int i, int j)
        {
            return i >= 0 && i < size && j >= 0 && j < size;
        }

        public GameObject GetTile(int i, int j)
        {
            return tiles[i, j];
        }

        public void SetStartBuilding(int i, int j, GameObject building)
        {
            Quaternion rotation = this.transform.rotation;
            Vector3 position = getCoordinate(i, j);

            if (startBuilding == null)
            {
                startBuilding = Instantiate(building, position + offsetBuild, rotation, this.transform);
            }
            else
            {
                Vector2Int indexesOld = IndexesOf(startBuilding);
                SetTile(indexesOld.x, indexesOld.y, towerTilePrefab);

                startBuilding.transform.position = position + offsetBuild;
            }
        }

        public GameObject GetStartBuilding()
        {
            return startBuilding;
        }

        public void SetEndBuilding(int i, int j, GameObject building)
        {
            Quaternion rotation = this.transform.rotation;
            Vector3 position = getCoordinate(i, j);

            if (endBuilding == null)
            {
                endBuilding = Instantiate(building, position + offsetBuild, rotation, this.transform);
            }
            else
            {
                Vector2Int indexesOld = IndexesOf(endBuilding);
                SetTile(indexesOld.x, indexesOld.y, towerTilePrefab);

                endBuilding.transform.position = position + offsetBuild;
            }
        }

        public GameObject GetEndBuilding()
        {
            return endBuilding;
        }

        private Vector3 getCoordinate(int i, int j)
        {
            return new Vector3(this.transform.position.x + stepTile.x * i,
                               this.transform.position.y,
                               this.transform.position.z + stepTile.z * j);
        }

        private Vector2Int IndexesOf(GameObject mapObject)
        {
            Vector2Int indexes = new(Mathf.RoundToInt((mapObject.transform.position.x - this.transform.position.x) / stepTile.x),
                                     Mathf.RoundToInt((mapObject.transform.position.z - this.transform.position.z) / stepTile.z));

            return indexes;
        }

        public bool IsAllBuilds()
        {
            return startBuilding != null && endBuilding != null;
        }

        public TileKind[,] GetTileArray()
        {
            TileKind[,] array = new TileKind[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    array[i, j] = KindOfPrefab(tiles[i, j]);
                }
            }

            return array;
        }

        public Vector2Int GetIndexesStartBuild()
        {
            return IndexesOf(startBuilding);
        }

        public Vector2Int GetIndexesEndBuild()
        {
            return IndexesOf(endBuilding);
        }
    }

}
