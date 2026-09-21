using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Storage;
using TowerDefense.Main.Map.Tile;
using TowerDefense.Main.Map.Buildings;
using TowerDefense.Main.Managers.WaveSpawners;


namespace TowerDefense.Main.Map
{
    public class MapComponentsController : MonoBehaviour
    {
        [SerializeField]
        private GameObject waveSpawner;
        [SerializeField]
        private GameObject startBuilding;
        [SerializeField]
        private GameObject endBuilding;

        public TilesMap TilesMap { get; private set; }
        private WayPoints wayPoints;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject tilesMapPrefab;
        [SerializeField]
        private GameObject wayPointsPrefab;

        private PathGenerator pathGenerator;
        private BlockedTilesGenerator blockedTilesGenerator;

        private MapSizeParamsStorage mapSizeParams;

        void Start()
        {
            mapSizeParams = Resources.Load<MapSizeParamsStorage>($"{nameof(MapSizeParamsStorage)}");

            Vector2Int indexesStartBuilding = mapSizeParams.IndexesStartBuilding;
            Vector2Int indexesEndBuilding = mapSizeParams.IndexesEndBuilding;

            pathGenerator = new PathGenerator();
            blockedTilesGenerator = new BlockedTilesGenerator();

            List<Vector2Int> generatedPath;
            HashSet<Vector2Int> blockedTiles;
            if (MapSaver.instance.IsSave)
            {
                indexesStartBuilding = MapSaver.instance.GetIndexesStart();
                indexesEndBuilding = MapSaver.instance.GetIndexesEnd();

                TileKind[,] tileMatrix = MapSaver.instance.GetTileMatrix();
                generatedPath = pathGenerator.GetPathWithMatrix(tileMatrix, indexesStartBuilding, indexesEndBuilding);
                blockedTiles = BlockedTilesFromMatrix(tileMatrix);
            }
            else
            {
                generatedPath = pathGenerator.GeneratePath(mapSizeParams.CountTile, indexesStartBuilding, indexesEndBuilding);
                blockedTiles = blockedTilesGenerator.Generate(mapSizeParams.CountTile, generatedPath, mapSizeParams.BlockedTilesShare);
            }

            TilesMap = Instantiate(tilesMapPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
                .GetComponent<TilesMap>();
            TilesMap.Initialize(mapSizeParams.CountTile, generatedPath, blockedTiles, mapSizeParams.OffsetTile);

            wayPoints = Instantiate(wayPointsPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
                .GetComponent<WayPoints>();
            wayPoints.Initialize(generatedPath);

            startBuilding.GetComponent<Building>().Initialize(
                indexesStartBuilding, mapSizeParams.SizeTile, mapSizeParams.OffsetTile, mapSizeParams.OffsetBuilding, WayPoints.Points[0].localPosition);

            Vector3 targetDirEndBuilding = WayPoints.Points.Count > 1 ? WayPoints.Points[WayPoints.Points.Count - 2].localPosition : startBuilding.transform.position;
            endBuilding.GetComponent<EndBuilding>().Initialize(
                indexesEndBuilding, mapSizeParams.SizeTile, mapSizeParams.OffsetTile, mapSizeParams.OffsetBuilding, targetDirEndBuilding);

            waveSpawner.GetComponent<WaveSpawner>().Initialize(startBuilding.transform);
        }


        private static HashSet<Vector2Int> BlockedTilesFromMatrix(TileKind[,] tileMatrix)
        {
            HashSet<Vector2Int> blockedTiles = new();

            for (int x = 0; x < tileMatrix.GetLength(0); x++)
            {
                for (int y = 0; y < tileMatrix.GetLength(1); y++)
                {
                    if (tileMatrix[x, y] == TileKind.Blocked)
                        blockedTiles.Add(new Vector2Int(y, x));
                }
            }

            return blockedTiles;
        }
    }

}
