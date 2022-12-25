using UnityEngine;

public class MapConstructor : MonoBehaviour
{
    public static MapConstructor instance;

    private GameObject selectedComponent;

    [Header("Map")]
    [SerializeField]
    private GameObject map;

    [Header("Attributes")]
    [SerializeField]
    private Vector3Int indexesStartMap = new Vector3Int(0, 0 ,0);
    [SerializeField]
    private int size = 16;

    [SerializeField]
    private Vector3 offsetBuild = new Vector3(0, (float)2.5, 0);


    [Header("Prefabs")]
    [SerializeField]
    private GameObject towerTilePrefab;
    [SerializeField]
    private GameObject pathTilePrefab;
    [SerializeField]
    private GameObject startBuildingPrefab;
    [SerializeField]
    private GameObject endBuildingPrefab;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        selectedComponent = null;

        map.GetComponent<Map>().Initialize(size, indexesStartMap, offsetBuild);
    }


    public void SetSelectedComponent(GameObject component)
    {
        selectedComponent = component;
    }

    public void BuildComponent(GameObject mapTile)
    {
        Vector2Int indexes = IndexsOfMapTile(mapTile);

        if(selectedComponent != null)
        {
            map.GetComponent<Map>().SetTile(indexes.x, indexes.y, pathTilePrefab);

            if (selectedComponent == startBuildingPrefab)
                map.GetComponent<Map>().SetStartBuilding(indexes.x, indexes.y, selectedComponent);

            if (selectedComponent == endBuildingPrefab)
                map.GetComponent<Map>().SetEndBuilding(indexes.x, indexes.y, selectedComponent);
        }
        
    }

    public void BuildTowerTile(GameObject mapTile)
    {
        Vector2Int indexes = IndexsOfMapTile(mapTile);

        if (map.GetComponent<Map>().GetTile(indexes.x, indexes.y).GetComponent<ConstructorPathTile>() != null)
        {
            map.GetComponent<Map>().SetTile(indexes.x, indexes.y, towerTilePrefab);

            GameObject startBuilding = map.GetComponent<Map>().GetStartBuilding();
            if (startBuilding != null)
            {
                Vector2Int buildIndexes = IndexsOfMapTile(startBuilding);
                if (indexes == buildIndexes)
                    Destroy(startBuilding);
            }

            GameObject endBuilding = map.GetComponent<Map>().GetEndBuilding();
            if (endBuilding != null)
            {
                Vector2Int buildIndexes = IndexsOfMapTile(endBuilding);
                if (indexes == buildIndexes)
                    Destroy(endBuilding);
            }
        }
    }

    private Vector2Int IndexsOfMapTile(GameObject mapTile)
    {
        Vector2Int indexes = new((int)((mapTile.transform.position.x - indexesStartMap.x) / (mapTile.transform.localScale.x + 1)),
                                 (int)((mapTile.transform.position.z - indexesStartMap.z) / (mapTile.transform.localScale.z + 1)));

        return indexes;
    }
}
