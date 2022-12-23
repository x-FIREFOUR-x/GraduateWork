using UnityEngine;

public class MapConstructor : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField]
    private Vector3Int indexesStartMap = new Vector3Int(0, 0 ,0);
    [SerializeField]
    private int size = 16;

    private GameObject[,] tileMap;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject constructorTilePrefab;
    [SerializeField]
    private GameObject pathTilePrefab;
    [SerializeField]
    private GameObject startBuilding;
    [SerializeField]
    private GameObject endBuilding;

    // Start is called before the first frame update
    void Start()
    {
        tileMap = new GameObject[size, size];

        
        Quaternion rotation = this.transform.rotation;
        var t = constructorTilePrefab.transform.localScale.x;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Vector3 position = new Vector3(
                    indexesStartMap.x + constructorTilePrefab.transform.localScale.x * indexesStartMap.x,
                    indexesStartMap.y,
                    indexesStartMap.z + constructorTilePrefab.transform.localScale.z * indexesStartMap.z);
                tileMap[i,j] = Instantiate(constructorTilePrefab, position, rotation, this.transform);
            }
        }
    }

    void Update()
    {
        
    }
}
