using System.Collections.Generic;
using UnityEngine;

public class MapComponentsController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField]
    private GameObject tilesMapPrefab;
    [SerializeField]
    private GameObject wayPointsPrefab;
    [SerializeField]
    private GameObject startBuildingPrefab;
    [SerializeField]
    private GameObject endBuildingPrefab;

    private TilesMap tilesMap;
    private WayPoints wayPoints;
    private Transform startBuilding;
    private Transform endBuilding;

    [Header("Attributes")]
    [SerializeField]
    Vector2Int indexesStart = new Vector2Int(1, 1);
    [SerializeField]
    Vector2Int indexesEnd = new Vector2Int(14, 14);
    [SerializeField]
    private int size = 16;

    void Start()
    {
        List<Vector2Int> generatedPath = PathGenerator.GeneratePath(size, indexesStart, indexesEnd);

        tilesMap = Instantiate(tilesMapPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
            .GetComponent<TilesMap>();
        tilesMap.Initialize(size, generatedPath);

        startBuilding = Instantiate(startBuildingPrefab, new Vector3(5, (float)2.5, 5), new Quaternion(0, 0, 0, 1)).transform;
        //endBuilding = Instantiate(endBuildingPrefab, new Vector3(70, (float)2.5, 70), new Quaternion(0, 0, 0, 1)).transform;
        endBuildingPrefab.transform.SetPositionAndRotation(new Vector3(70, (float)2.5, 70), new Quaternion(0, 0, 0, 1));

        wayPoints = Instantiate(wayPointsPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
            .GetComponent<WayPoints>();
        wayPoints.Initialize(generatedPath);
    }
}
