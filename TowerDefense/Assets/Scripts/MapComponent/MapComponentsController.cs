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

    private TilesMap TilesMap;
    private WayPoints WayPoints;
    private Transform StartBuilding;
    private Transform EndBuilding;

    [Header("Map Attributes")]
    [SerializeField]
    Vector2Int IndexesStart = new Vector2Int(1, 1);
    [SerializeField]
    Vector2Int IndexesEnd = new Vector2Int(14, 14);
    [SerializeField]
    private int Size = 16;

    void Start()
    {
        /*
        List<List<int>> generatedTilesMatrix = new List<List<int>> {
            new List<int>{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new List<int>{0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0},
            new List<int>{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0},
            new List<int>{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0},
            new List<int>{0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0},
            new List<int>{0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0},
            new List<int>{0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0},
            new List<int>{0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0},
            new List<int>{0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0},
            new List<int>{0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
            new List<int>{0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0},
            new List<int>{0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0},
            new List<int>{0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0},
            new List<int>{0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0},
            new List<int>{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0},
            new List<int>{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        };

        List<Vector2Int> generatedPath = new List<Vector2Int>();
        int[] rowNum = new[] { 1, 0, -1, 0 };
        int[] columnNum = new[] { 0, 1, 0, -1 };

        Vector2Int currIndexes = indexesStart;
        generatedPath.Add(currIndexes);
        while (currIndexes != indexesEnd)
        {
            for (int i = 0; i < rowNum.Length; i++)
            {
                Vector2Int node = new Vector2Int(currIndexes.x + rowNum[i], currIndexes.y + columnNum[i]);
                if (generatedTilesMatrix[node.x][node.y] == 1 && !generatedPath.Contains(node))
                {
                    currIndexes = node;
                    generatedPath.Add(node);
                }
            }
        }
        */
        
        List<Vector2Int> generatedPath = PathGenerator.GeneratePath(Size, IndexesStart, IndexesEnd);

        TilesMap = Instantiate(TilesMapPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
            .GetComponent<TilesMap>();
        TilesMap.Initialize(Size, generatedPath);

        StartBuilding = Instantiate(StartBuildingPrefab, new Vector3(5, (float)2.5, 5), new Quaternion(0, 0, 0, 1)).transform;
        EndBuilding = Instantiate(EndBuildingPrefab, new Vector3(70, (float)2.5, 70), new Quaternion(0, 0, 0, 1)).transform;

        WayPoints = Instantiate(WayPointsPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
            .GetComponent<WayPoints>();
        WayPoints.Initialize(generatedPath);
        
    }

    
}
