using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Storage;
using TowerDefense.Main.Map.Tiles;
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

        private MapSizeParamsStorage mapSizeParams;

        void Start()
        {
            mapSizeParams = Resources.Load<MapSizeParamsStorage>($"{nameof(MapSizeParamsStorage)}");

            Vector2Int indexesStartBuilding = mapSizeParams.IndexesStartBuilding;
            Vector2Int indexesEndBuilding = mapSizeParams.IndexesEndBuilding;

            pathGenerator = new PathGenerator();

            List<Vector2Int> generatedPath;
            if (MapSaver.instance.IsSave)
            {
                indexesStartBuilding = MapSaver.instance.GetIndexesStart();
                indexesEndBuilding = MapSaver.instance.GetIndexesEnd();
                generatedPath = pathGenerator.GetPathWithMatrix(MapSaver.instance.GetTileMatrix(), indexesStartBuilding, indexesEndBuilding);
            }
            else
            {
                generatedPath = pathGenerator.GeneratePath(mapSizeParams.CountTile, indexesStartBuilding, indexesEndBuilding);
            }

            TilesMap = Instantiate(tilesMapPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
                .GetComponent<TilesMap>();
            TilesMap.Initialize(mapSizeParams.CountTile, generatedPath, mapSizeParams.OffsetTile);

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
    }

}
