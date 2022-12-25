using UnityEngine;

public class Map : MonoBehaviour
{
    private int size = 16;

    private GameObject[,] tilesMap;
    private GameObject endBuilding;
    private GameObject startBuilding;

    private Vector3 offsetBuild;

    private Vector3 sizeTile;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject towerTilePrefab;
    [SerializeField]
    private GameObject pathTilePrefab;


    public void Initialize(int size, Vector3 startPosition, Vector3 offsetBuild)
    {
        this.transform.position = startPosition;
        this.size = size;
        this.offsetBuild = offsetBuild;

        sizeTile = towerTilePrefab.transform.localScale;

        endBuilding = null;
        startBuilding = null;

        tilesMap = new GameObject[size, size];

        Quaternion rotation = this.transform.rotation;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Vector3 position = getCoordinate(i, j);
                tilesMap[i, j] = Instantiate(towerTilePrefab, position, rotation, this.transform);
            }
        }
    }

    public void SetTile(int i, int j, GameObject newTile)
    {
        Vector3 position = getCoordinate(i, j);
        Quaternion rotation = this.transform.rotation;

        Destroy(tilesMap[i, j]);
        tilesMap[i, j] = Instantiate(newTile, position, rotation, this.transform);
    }

    public GameObject GetTile(int i, int j)
    {
        return tilesMap[i, j];
    }

    public void SetStartBuilding(int i, int j, GameObject building)
    {
        Quaternion rotation = this.transform.rotation;
        Vector3 position = getCoordinate(i, j);

        if (startBuilding == null)
        {
            startBuilding = Instantiate(building, position + offsetBuild, rotation, this.transform);
        }
        else
        {
            Vector2Int indexesOld = IndexsOfMapTile(startBuilding);
            SetTile(indexesOld.x, indexesOld.y, towerTilePrefab);

            startBuilding.transform.position = position + offsetBuild;
        }
    }

    public GameObject GetStartBuilding()
    {
        return startBuilding;
    }

    public void SetEndBuilding(int i, int j, GameObject building)
    {
        Quaternion rotation = this.transform.rotation;
        Vector3 position = getCoordinate(i, j);

        if (endBuilding == null)
        {
            endBuilding = Instantiate(building, position + offsetBuild, rotation, this.transform);
        }
        else
        {
            Vector2Int indexesOld = IndexsOfMapTile(endBuilding);
            SetTile(indexesOld.x, indexesOld.y, towerTilePrefab);

            endBuilding.transform.position = position + offsetBuild;
        }
    }

    public GameObject GetEndBuilding()
    {
        return endBuilding;
    }

    private Vector3 getCoordinate(int i, int j)
    {
        return new Vector3(this.transform.position.x + (sizeTile.x + 1) * i,
                           this.transform.position.y,
                           this.transform.position.z + (sizeTile.z + 1) * j);
    }

    private Vector2Int IndexsOfMapTile(GameObject mapTile)
    {
        Vector2Int indexes = new((int)((mapTile.transform.position.x - this.transform.position.x) / (sizeTile.x + 1)),
                                 (int)((mapTile.transform.position.z - this.transform.position.z) / (sizeTile.z + 1)));

        return indexes;
    }
}
