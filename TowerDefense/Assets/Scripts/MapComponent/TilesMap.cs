using System.Collections.Generic;
using UnityEngine;

public class TilesMap : MonoBehaviour
{
    [SerializeField]
    private GameObject TowerTilePrefab;
    [SerializeField]
    private GameObject PathTilePrefab;

    private List<List<GameObject>> tiles;
    private int size;


    public void Initialize(int _size, List<Vector2Int> generatedPath)
    {
        size = _size;

        Vector3 position = this.transform.position;
        Quaternion rotation = this.transform.rotation;

        tiles = new List<List<GameObject>>();

        for (int i = 0; i < size; i++)
        {
            position.x = 0;
            List<GameObject> lineTiles = new();

            for (int j = 0; j < size; j++)
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
            tiles.Add(lineTiles);
        }
    }

}
