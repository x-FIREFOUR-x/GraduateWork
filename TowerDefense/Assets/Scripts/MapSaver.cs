using UnityEngine;

public class MapSaver : MonoBehaviour
{
    public static MapSaver instance;

    private int[,] tilesMatrix;
    private Vector2Int indexesStart;
    private Vector2Int indexesEnd;

    public bool IsSave { get; private set; }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        IsSave = false;

        DontDestroyOnLoad(this.gameObject);  
    }

    public bool SetData(int[,] tiles, Vector2Int start, Vector2Int end)
    {
        tilesMatrix = tiles;
        indexesStart = start;
        indexesEnd = end;

        if (ÑonstructedMapIsCorrect())
        {
            IsSave = true;
            return true;
        }
        else
        {
            tilesMatrix = null;
            indexesStart = new Vector2Int();
            indexesEnd = new Vector2Int();
            return false;
        }
        
    }


    private bool ÑonstructedMapIsCorrect()
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

                if (tilesMatrix[newIndexes.x, newIndexes.y] == 1
                    && newIndexes != prevIndexes
                    && (newIndexes.x > 0 && newIndexes.x < tilesMatrix.Length - 1)
                    && (newIndexes.y > 0 && newIndexes.y < tilesMatrix.Length - 1))
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

    public int[,] GetTileMatrix()
    {
        return tilesMatrix;
    }

    public Vector2Int GetIndexesStart()
    {
        return indexesStart;
    }

    public Vector2Int GetIndexesEnd()
    {
        return indexesEnd;
    }
}
