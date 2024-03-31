using UnityEngine;

using Storage;

public class MapConstructor : MonoBehaviour
{
    public static MapConstructor instance;

    private GameObject selectedComponent;

    private MapSizeParamsStorage mapSizeParams;

    [Header("Map")]
    [SerializeField]
    private GameObject map;

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

        mapSizeParams = Resources.Load<MapSizeParamsStorage>($"{nameof(MapSizeParamsStorage)}");

        map.GetComponent<Map>().Initialize(mapSizeParams.CountTile, mapSizeParams.StartMap, mapSizeParams.OffsetBuilding);
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
            if (indexes.x > 0 && indexes.x < mapSizeParams.CountTile - 1 &&
                indexes.y > 0 && indexes.y < mapSizeParams.CountTile - 1)
            {
                map.GetComponent<Map>().SetTile(indexes.x, indexes.y, pathTilePrefab);

                if (selectedComponent == startBuildingPrefab)
                    map.GetComponent<Map>().SetStartBuilding(indexes.x, indexes.y, selectedComponent);

                if (selectedComponent == endBuildingPrefab)
                    map.GetComponent<Map>().SetEndBuilding(indexes.x, indexes.y, selectedComponent);
            }
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
        Vector2Int indexes = new((int)((mapTile.transform.position.x - mapSizeParams.StartMap.x) / (mapTile.transform.localScale.x + 1)),
                                 (int)((mapTile.transform.position.z - mapSizeParams.StartMap.z) / (mapTile.transform.localScale.z + 1)));

        return indexes;
    }
}
