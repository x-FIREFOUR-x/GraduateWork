using System.Collections.Generic;
using UnityEngine;

public class TilesMap : MonoBehaviour
{
    [SerializeField]
    private GameObject TowerTilePrefab;
    [SerializeField]
    private GameObject PathTilePrefab;

    private List<List<GameObject>> Tiles;
    private int Size;


    public void Initialize(int size, List<Vector2Int> generatedPath)
    {
        Size = size;

        Vector3 position = this.transform.position;
        Quaternion rotation = this.transform.rotation;

        Tiles = new List<List<GameObject>>();

        for (int i = 0; i < Size; i++)
        {
            position.x = 0;
            List<GameObject> lineTiles = new();

            for (int j = 0; j < Size; j++)
            {
                if (generatedPath.Contains(new Vector2Int(i, j)))
                {
                    GameObject tile = Instantiate(PathTilePrefab, position, rotation, this.transform);
                    lineTiles.Add(tile);
                }
                else
                {
                    GameObject tile = Instantiate(TowerTilePrefab, position, rotation, this.transform);
                    lineTiles.Add(tile);
                }

                position.x += TowerTilePrefab.transform.localScale.x + 1;
            }
            position.z += TowerTilePrefab.transform.localScale.z + 1;
            Tiles.Add(lineTiles);
        }
    }

}
