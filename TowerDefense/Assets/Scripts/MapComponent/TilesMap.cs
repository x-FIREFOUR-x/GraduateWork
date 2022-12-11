using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilesMap : MonoBehaviour
{
    [SerializeField]
    private GameObject TowerTilePrefab;
    [SerializeField]
    private GameObject PathTilePrefab;

    private List<List<GameObject>> Tiles;

    [SerializeField]
    private int Size = 16;


    public void Initialize(List<List<int>> GeneratedTileMap)
    {
        Vector3 position = this.transform.position;
        Quaternion rotation = this.transform.rotation;

        Tiles = new List<List<GameObject>>();

        for (int i = 0; i < Size; i++)
        {
            position.x = 0;
            List<GameObject> lineTiles = new();

            for (int j = 0; j < Size; j++)
            {
                if (GeneratedTileMap[i][j] == 0)
                {
                    GameObject tile = Instantiate(TowerTilePrefab, position, rotation, this.transform);
                    lineTiles.Add(tile);
                }
                else
                {
                    GameObject tile = Instantiate(PathTilePrefab, position, rotation, this.transform);
                    lineTiles.Add(tile);
                }

                position.x += TowerTilePrefab.transform.localScale.x + 1;
            }
            position.z += TowerTilePrefab.transform.localScale.z + 1;
            Tiles.Add(lineTiles);
        }
    }

}
