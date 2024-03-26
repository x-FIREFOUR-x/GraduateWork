using System.Collections.Generic;
using UnityEngine;

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

    [Header("Attributes")]
    [SerializeField]
    private Vector2Int indexesStart = new Vector2Int(1, 1);
    [SerializeField]
    private Vector2Int indexesEnd = new Vector2Int(14, 14);
    [SerializeField]
    private int size = 16;
    [SerializeField]
    private Vector3 sizeTile = new(4, 1, 4);
    [SerializeField]
    private Vector3 offsetTile = new(1, 0, 1);
    [SerializeField]
    private float hightBuilding = 0.5f;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject tilesMapPrefab;
    [SerializeField]
    private GameObject wayPointsPrefab;

    private PathGenerator pathGenerator;

    void Start()
    {
        pathGenerator = new PathGenerator();

        List<Vector2Int> generatedPath;
        if (MapSaver.instance.IsSave)
        {
            indexesStart = MapSaver.instance.GetIndexesStart();
            indexesEnd = MapSaver.instance.GetIndexesEnd();
            generatedPath = pathGenerator.GetPathWithMatrix(MapSaver.instance.GetTileMatrix(), indexesStart, indexesEnd);
        }
        else
        {
            generatedPath = pathGenerator.GeneratePath(size, indexesStart, indexesEnd);
        }

        tilesMap = Instantiate(tilesMapPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
            .GetComponent<TilesMap>();
        tilesMap.Initialize(size, generatedPath, offsetTile);

        wayPoints = Instantiate(wayPointsPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
            .GetComponent<WayPoints>();
        wayPoints.Initialize(generatedPath);

        waveSpawner.GetComponent<WaveSpawner>().Initialize(startBuilding.transform);

        startBuilding.transform.SetPositionAndRotation(
            new Vector3((sizeTile.x + offsetTile.x) * indexesStart.x, hightBuilding, (sizeTile.z + offsetTile.z) * indexesStart.y),
            new Quaternion(0, 0, 0, 1));

        endBuilding.transform.SetPositionAndRotation(
            new Vector3((sizeTile.x + offsetTile.x) * indexesEnd.x, hightBuilding, (sizeTile.z + offsetTile.z) * indexesEnd.y),
            new Quaternion(0, 0, 0, 1));
    }
}
