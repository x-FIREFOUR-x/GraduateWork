using System.Collections.Generic;
using UnityEngine;

using Storage;

public class MapComponentsController : MonoBehaviour
{
    [SerializeField]
    private GameObject waveSpawner;
    [SerializeField]
    private GameObject startBuilding;
    [SerializeField]
    private GameObject endBuilding;

    private TilesMap tilesMap;
    private WayPoints wayPoints;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject tilesMapPrefab;
    [SerializeField]
    private GameObject wayPointsPrefab;

    private PathGenerator pathGenerator;

    void Start()
    {
        var mapSizeParams = Resources.Load<MapSizeParamsStorage>($"{nameof(MapSizeParamsStorage)}");

        pathGenerator = new PathGenerator();

        List<Vector2Int> generatedPath;
        if (MapSaver.instance.IsSave)
        {
            mapSizeParams.IndexesStartBuilding = MapSaver.instance.GetIndexesStart();
            mapSizeParams.IndexesEndBuilding = MapSaver.instance.GetIndexesEnd();
            generatedPath = pathGenerator.GetPathWithMatrix(MapSaver.instance.GetTileMatrix(), mapSizeParams.IndexesStartBuilding, mapSizeParams.IndexesEndBuilding);
        }
        else
        {
            generatedPath = pathGenerator.GeneratePath(mapSizeParams.CountTile, mapSizeParams.IndexesStartBuilding, mapSizeParams.IndexesEndBuilding);
        }

        tilesMap = Instantiate(tilesMapPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
            .GetComponent<TilesMap>();
        tilesMap.Initialize(mapSizeParams.CountTile, generatedPath, mapSizeParams.OffsetTile);

        wayPoints = Instantiate(wayPointsPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
            .GetComponent<WayPoints>();
        wayPoints.Initialize(generatedPath);

        waveSpawner.GetComponent<WaveSpawner>().Initialize(startBuilding.transform);

        startBuilding.transform.SetPositionAndRotation(
            new Vector3(
                (mapSizeParams.SizeTile.x + mapSizeParams.OffsetTile.x) * mapSizeParams.IndexesStartBuilding.x,
                mapSizeParams.OffsetBuilding.y, 
                (mapSizeParams.SizeTile.z + mapSizeParams.OffsetTile.z) * mapSizeParams.IndexesStartBuilding.y),
            new Quaternion(0, 0, 0, 1));

        endBuilding.transform.SetPositionAndRotation(
            new Vector3(
                (mapSizeParams.SizeTile.x + mapSizeParams.OffsetTile.x) * mapSizeParams.IndexesEndBuilding.x,
                mapSizeParams.OffsetBuilding.y,
                (mapSizeParams.SizeTile.z + mapSizeParams.OffsetTile.z) * mapSizeParams.IndexesEndBuilding.y),
            new Quaternion(0, 0, 0, 1));
    }
}
