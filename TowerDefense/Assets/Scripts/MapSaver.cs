using UnityEngine;

public class MapSaver : MonoBehaviour
{
    public static MapSaver instance;

    private int[,] tilesArray;
    private Vector2Int indexesStart;
    private Vector2Int indexesEnd;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        DontDestroyOnLoad(this.gameObject);  
    }

    public void SetData(int[,] tiles, Vector2Int start, Vector2Int end)
    {
        tilesArray = tiles;
        indexesStart = start;
        indexesEnd = end;
    }

    public bool ÑonstructedMapIsCorrect()
    {
        bool isFull = true;

        int[] rowNum = new[] { 1, 0, -1, 0 };
        int[] columnNum = new[] { 0, 1, 0, -1 };

        Vector2Int prevIndexes = new Vector2Int(-1, -1);
        Vector2Int currentIndexes = indexesStart;
        while (isFull && currentIndexes != indexesEnd)
        {
            int countNeighbor = 0;
            Vector2Int nextIndexes = new Vector2Int(-1, -1);
            for (int i = 0; i < rowNum.Length; i++)
            {
                Vector2Int newIndexes = new Vector2Int(currentIndexes.x + rowNum[i], currentIndexes.y + columnNum[i]);

                if (tilesArray[newIndexes.x, newIndexes.y] == 1
                    && newIndexes != prevIndexes
                    && (newIndexes.x > 0 && newIndexes.x < tilesArray.Length - 1)
                    && (newIndexes.y > 0 && newIndexes.y < tilesArray.Length - 1))
                {
                    nextIndexes = newIndexes;
                    countNeighbor++;
                }
            }

            prevIndexes = currentIndexes;
            currentIndexes = nextIndexes;

            if (countNeighbor != 1)
            {
                isFull = false;
            }
        }

        return isFull;
    }
}
