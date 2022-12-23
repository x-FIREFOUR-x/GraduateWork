using System;
using UnityEngine;

public class MapConstructor : MonoBehaviour
{
    public static MapConstructor instance;

    private GameObject[,] tileMap;

    private GameObject selectedComponent;

    [Header("Attributes")]
    [SerializeField]
    private Vector3Int indexesStartMap = new Vector3Int(0, 0 ,0);
    [SerializeField]
    private int size = 16;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject constructorTilePrefab;
    [SerializeField]
    private GameObject pathTilePrefab;
    [SerializeField]
    private GameObject startBuilding;
    [SerializeField]
    private GameObject endBuilding;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        selectedComponent = null;

        tileMap = new GameObject[size, size];
        
        Quaternion rotation = this.transform.rotation;
        var t = constructorTilePrefab.transform.localScale.x;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Vector3 position = getCoordinate(i, j);
                tileMap[i,j] = Instantiate(constructorTilePrefab, position, rotation, this.transform);
            }
        }
    }

    public void setSelectedComponent(GameObject component)
    {
        selectedComponent = component;
    }

    public void BuildComponent(GameObject mapTile)
    {
        Vector2Int indexes = IndexsOfMapTile(mapTile);
        Vector3 position = getCoordinate(indexes.x, indexes.y);

        Quaternion rotation = this.transform.rotation;

        if(selectedComponent != null)
        {
            if(selectedComponent == pathTilePrefab)
            {
                Destroy(tileMap[indexes.x, indexes.y]);
                tileMap[indexes.x, indexes.y] = Instantiate(selectedComponent, position, rotation, this.transform);
            }
        }
        
    }


    private Vector3 getCoordinate(int i, int j)
    {
        return new Vector3(indexesStartMap.x + (constructorTilePrefab.transform.localScale.x + 1) * i,
                           indexesStartMap.y,
                           indexesStartMap.z + (constructorTilePrefab.transform.localScale.z + 1) * j);
    }

    private Vector2Int IndexsOfMapTile(GameObject mapTile)
    {
        Vector2Int indexes = new((int)((mapTile.transform.position.x - indexesStartMap.x) / (mapTile.transform.localScale.x + 1)),
                                 (int)((mapTile.transform.position.z - indexesStartMap.z) / (mapTile.transform.localScale.z + 1)));

        return indexes;
    }
}
