using UnityEngine;

public class MapConstructor : MonoBehaviour
{
    public static MapConstructor instance;

    private GameObject[,] tilesMap;
    private GameObject endBuilding;
    private GameObject startBuilding;

    private GameObject selectedComponent;

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

        endBuilding = null;
        startBuilding = null;

        tilesMap = new GameObject[size, size];
        
        Quaternion rotation = this.transform.rotation;
        var t = towerTilePrefab.transform.localScale.x;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Vector3 position = getCoordinate(i, j);
                tilesMap[i,j] = Instantiate(towerTilePrefab, position, rotation, this.transform);
            }
        }
    }


    public void SetSelectedComponent(GameObject component)
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
                Destroy(tilesMap[indexes.x, indexes.y]);
                tilesMap[indexes.x, indexes.y] = Instantiate(selectedComponent, position, rotation, this.transform);
            }

            if(selectedComponent == startBuildingPrefab)
                BuildBuilding(indexes, position, rotation, ref startBuilding);

            if (selectedComponent == endBuildingPrefab)
                BuildBuilding(indexes, position, rotation, ref endBuilding);
        }
        
    }

    private void BuildBuilding(Vector2Int indexes, Vector3 position, Quaternion rotation, ref GameObject build)
    {
        if(build != null)
        {
            Destroy(build);

            Vector2Int oldIndexes = IndexsOfMapTile(build);
            Destroy(tilesMap[oldIndexes.x, oldIndexes.y]);

            tilesMap[oldIndexes.x, oldIndexes.y] =
                Instantiate(towerTilePrefab, build.transform.position - offsetBuild, rotation, this.transform);
        }

        Destroy(tilesMap[indexes.x, indexes.y]);
        tilesMap[indexes.x, indexes.y] = Instantiate(pathTilePrefab, position, rotation, this.transform);
        build = Instantiate(selectedComponent, position + offsetBuild, rotation, this.transform);
    }

    public void BuildTowerTile(GameObject mapTile)
    {
        Vector2Int indexes = IndexsOfMapTile(mapTile);

        if (tilesMap[indexes.x, indexes.y].GetComponent<ConstructorPathTile>() != null)
        {
            Vector3 position = getCoordinate(indexes.x, indexes.y);
            Quaternion rotation = this.transform.rotation;

            Destroy(tilesMap[indexes.x, indexes.y]);
            tilesMap[indexes.x, indexes.y] = Instantiate(towerTilePrefab, position, rotation, this.transform);

            if (startBuilding != null)
            {
                Vector2Int buildIndexes = IndexsOfMapTile(startBuilding);
                if (indexes == buildIndexes)
                    Destroy(startBuilding);
            }

            if (endBuilding != null)
            {
                Vector2Int buildIndexes = IndexsOfMapTile(endBuilding);
                if (indexes == buildIndexes)
                    Destroy(endBuilding);
            }
        }
    }

    private Vector3 getCoordinate(int i, int j)
    {
        return new Vector3(indexesStartMap.x + (towerTilePrefab.transform.localScale.x + 1) * i,
                           indexesStartMap.y,
                           indexesStartMap.z + (towerTilePrefab.transform.localScale.z + 1) * j);
    }

    private Vector2Int IndexsOfMapTile(GameObject mapTile)
    {
        Vector2Int indexes = new((int)((mapTile.transform.position.x - indexesStartMap.x) / (mapTile.transform.localScale.x + 1)),
                                 (int)((mapTile.transform.position.z - indexesStartMap.z) / (mapTile.transform.localScale.z + 1)));

        return indexes;
    }
}
