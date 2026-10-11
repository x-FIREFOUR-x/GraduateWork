using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Storage;
using TowerDefense.Main.Map.Tile;
using TowerDefense.Main.Map.Path;
using TowerDefense.Main.Map.Buildings;
using TowerDefense.Main.Map.Background;
using TowerDefense.Main.Managers.WaveSpawners;


namespace TowerDefense.Main.Map
{
    public class MapController : MonoBehaviour
    {
        [SerializeField]
        private GameObject waveSpawner;
        [SerializeField]
        private GameObject startBuilding;
        [SerializeField]
        private GameObject endBuilding;

        public TilesMap TilesMap { get; private set; }
        private PathGraphHolder pathGraphHolder;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject tilesMapPrefab;
        [SerializeField]
        private GameObject pathGraphHolderPrefab;

        private PathGenerator pathGenerator;
        private BlockedTilesGenerator blockedTilesGenerator;

        private MapSizeParamsStorage mapSizeParams;


        [Header("Background")]
        [SerializeField]
        private Material backgroundMountainsMaterial;
        [SerializeField]
        private Material backgroundMountainsFarMaterial;

        private const int MountainsCount = 16;
        private const float MountainsRingRadiusMultiplier = 0.55f;

        private const int FarMountainsCount = 10;
        private const float FarMountainsRingRadiusMultiplier = 1.05f;
        private const float FarMountainsScale = 1.8f;

        [SerializeField]
        private Material backgroundGroundMaterial;
        [SerializeField]
        private Material backgroundGroundDecorMaterial;

        private const float GroundDecorDistance = 35f;

        private const float FarMountainSpireMargin = 18f * FarMountainsScale * 1.8f;


        void Start()
        {
            mapSizeParams = Resources.Load<MapSizeParamsStorage>($"{nameof(MapSizeParamsStorage)}");

            pathGenerator = new PathGenerator();
            blockedTilesGenerator = new BlockedTilesGenerator();

            HashSet<Vector2Int> pathCells;
            HashSet<Vector2Int> blockedTiles;
            Vector2Int startCell;
            Vector2Int endCell;
            if (MapSaver.instance != null && MapSaver.instance.IsSave)
            {
                startCell = MapSaver.instance.GetStartCell();
                endCell = MapSaver.instance.GetEndCell();

                pathCells = MapSaver.instance.GetCells(TileKind.Path);
                blockedTiles = MapSaver.instance.GetCells(TileKind.Blocked);
            }
            else
            {
                startCell = mapSizeParams.IndexesStartBuilding;
                endCell = mapSizeParams.IndexesEndBuilding;

                pathCells = pathGenerator.GenerateBranchedPath(mapSizeParams.CountTile, startCell, endCell,
                                                               mapSizeParams.MinPathBranchesCount, mapSizeParams.MaxPathBranchesCount, mapSizeParams.MinPathBranchStartEndDistance, mapSizeParams.MaxPathBranchLength);
                blockedTiles = blockedTilesGenerator.Generate(mapSizeParams.CountTile, pathCells, mapSizeParams.BlockedTilesShare);
            }

            if (!PathGraphBuilder.TryBuild(pathCells, startCell, endCell, mapSizeParams.CellToPosition, out PathGraph pathGraph))
            {
                Debug.LogError("Path graph can not be built from the path cells");
                return;
            }

            TilesMap = Instantiate(tilesMapPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
                .GetComponent<TilesMap>();
            TilesMap.Initialize(mapSizeParams.CountTile, pathCells, blockedTiles, mapSizeParams.OffsetTile);

            GenerateBackgroundGround(mapSizeParams.CountTile, mapSizeParams.SizeTile);
            GenerateBackgroundMountains(mapSizeParams.CountTile, mapSizeParams.SizeTile);

            pathGraphHolder = Instantiate(pathGraphHolderPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
                .GetComponent<PathGraphHolder>();
            pathGraphHolder.Initialize(pathGraph);

            startBuilding.GetComponent<Building>().Initialize(
                startCell, mapSizeParams.SizeTile, mapSizeParams.OffsetTile, mapSizeParams.OffsetBuilding, pathGraph.Start.Next[0].Position);

            Vector3 targetDirEndBuilding = pathGraph.End.Previous.Count > 0 ? pathGraph.End.Previous[0].Position : startBuilding.transform.position;
            endBuilding.GetComponent<EndBuilding>().Initialize(
                endCell, mapSizeParams.SizeTile, mapSizeParams.OffsetTile, mapSizeParams.OffsetBuilding, targetDirEndBuilding);

            waveSpawner.GetComponent<WaveSpawner>().Initialize(startBuilding.transform);
        }


        private void GenerateBackgroundGround(int countTile, Vector3 sizeTile)
        {
            Vector3 center = new Vector3((countTile - 1) * sizeTile.x / 2f, 0, (countTile - 1) * sizeTile.z / 2f);
            float gridHalfWidth = countTile * sizeTile.x / 2f;
            float gridSize = countTile * sizeTile.x;

            float farRingRadius = gridSize * FarMountainsRingRadiusMultiplier;
            float coverageRadius = (farRingRadius * 1.05f + FarMountainSpireMargin + 10f) / 3f * 2f;

            int seed = System.Environment.TickCount + 2;
            BackgroundGroundGenerator.Generate(TilesMap.transform, center, gridHalfWidth, sizeTile.y / 2f, coverageRadius,
                                               backgroundGroundMaterial, seed, out System.Func<float, float, float> groundHeight);

            BackgroundGroundDecorGenerator.Generate(TilesMap.transform, backgroundGroundDecorMaterial, center, gridHalfWidth, sizeTile,
                                                    GroundDecorDistance, groundHeight, seed + 1);
        }

        private void GenerateBackgroundMountains(int countTile, Vector3 sizeTile)
        {
            Vector3 center = new Vector3((countTile - 1) * sizeTile.x / 2f, 0, (countTile - 1) * sizeTile.z / 2f);
            float gridHalfWidth = countTile * sizeTile.x / 2f;
            float gridSize = countTile * sizeTile.x;

            int seed = System.Environment.TickCount;

            float farRingRadius = gridSize * FarMountainsRingRadiusMultiplier;
            BackgroundMountainsGenerator.Generate(TilesMap.transform, center, gridHalfWidth, farRingRadius, FarMountainsCount, seed + 1,
                                                   backgroundMountainsFarMaterial, FarMountainsScale);

            float ringRadius = gridSize * MountainsRingRadiusMultiplier;
            BackgroundMountainsGenerator.Generate(TilesMap.transform, center, gridHalfWidth, ringRadius, MountainsCount, seed,
                                                   backgroundMountainsMaterial);
        }
    }

}
