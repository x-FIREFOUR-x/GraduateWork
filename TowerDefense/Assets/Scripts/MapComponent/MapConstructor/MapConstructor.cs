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
                    indexesStartMap.x + (constructorTilePrefab.transform.localScale.x + 1) * i,
                    indexesStartMap.y,
                    indexesStartMap.z + (constructorTilePrefab.transform.localScale.z + 1) * j);
                tileMap[i,j] = Instantiate(constructorTilePrefab, position, rotation);
            }
        }
    }

    void Update()
    {
        
    }
}
