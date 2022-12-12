using System.Collections;
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
    Vector2Int indexesStart = new Vector2Int(1, 1);
    [SerializeField]
    Vector2Int indexesEnd = new Vector2Int(14, 14);
    [SerializeField]
    private int Size = 16;

    void Start()
    {
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
        
        //List<Vector2Int> generatedPath = GeneratePath();

        TilesMap = Instantiate(TilesMapPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
            .GetComponent<TilesMap>();
        TilesMap.Initialize(Size, generatedPath);

        StartBuilding = Instantiate(StartBuildingPrefab, new Vector3(5, (float)2.5, 5), new Quaternion(0, 0, 0, 1)).transform;
        EndBuilding = Instantiate(EndBuildingPrefab, new Vector3(70, (float)2.5, 70), new Quaternion(0, 0, 0, 1)).transform;

        WayPoints = Instantiate(WayPointsPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1))
            .GetComponent<WayPoints>();
        WayPoints.Initialize(generatedPath);
        
    }

    private List<Vector2Int> GeneratePath()
    {
        bool[,] visitedNode = new bool[Size, Size];
        List<Vector2Int> pathNode = new List<Vector2Int>();

        Vector2Int currentNode = indexesStart;
        visitedNode[currentNode.x, currentNode.y] = true;
        pathNode.Add(currentNode);

        int[] rowNum = new[] { 1, 0, -1, 0 };
        int[] columnNum = new[] { 0, 1, 0, -1 };

        while (currentNode != indexesEnd)
        {
            List<Vector2Int> neighboringNode = new List<Vector2Int>();

            for (int i = 0; i < rowNum.Length; i++)
            {
                Vector2Int node = new Vector2Int(currentNode.x + rowNum[i], currentNode.y + columnNum[i]);

                if (visitedNode[node.x, node.y] == false && (node.x > 0 && node.x < Size - 1) && (node.y > 0 && node.y < Size - 1))
                {
                    neighboringNode.Add(node);
                }
            }

            if (neighboringNode.Count > 0)
            {
                int index = Random.Range(0, neighboringNode.Count);
                currentNode = neighboringNode[index];

                visitedNode[currentNode.x, currentNode.y] = true;
                pathNode.Add(currentNode);
            }
            else
            {
                pathNode.RemoveAt(pathNode.Count - 1);
                currentNode = pathNode[pathNode.Count - 1];
            }
            
        }

        return pathNode;
    }

}
