using System.Collections.Generic;
using UnityEngine;

public class MapComponentsController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField]
    private GameObject TilesMapPrefab;
    [SerializeField]
    private GameObject WayPointsPrefab;
    [SerializeField]
    private GameObject StartBuildingPrefab;
    [SerializeField]
    private GameObject EndBuildingPrefab;

    private TilesMap tilesMap;
    private WayPoints wayPoints;
    private Transform startBuilding;
    private Transform endBuilding;

    [Header("Map Attributes")]
    [SerializeField]
    Vector2Int indexesStart = new Vector2Int(1, 1);
    [SerializeField]
    Vector2Int indexesEnd = new Vector2Int(14, 14);
    [SerializeField]
    private int size = 16;

    void Start()
    {
        List<Vector2Int> generatedPath = PathGenerator.GeneratePath(size, indexesStart, indexesEnd);

        tilesMap = Instantiate(TilesMapPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
            .GetComponent<TilesMap>();
        tilesMap.Initialize(size, generatedPath);

        startBuilding = Instantiate(StartBuildingPrefab, new Vector3(5, (float)2.5, 5), new Quaternion(0, 0, 0, 1)).transform;
        endBuilding = Instantiate(EndBuildingPrefab, new Vector3(70, (float)2.5, 70), new Quaternion(0, 0, 0, 1)).transform;

        wayPoints = Instantiate(WayPointsPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
            .GetComponent<WayPoints>();
        wayPoints.Initialize(generatedPath);
    }
}
