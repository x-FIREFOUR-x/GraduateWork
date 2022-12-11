using System.Collections.Generic;
using UnityEngine;

public class WayPoints : MonoBehaviour
{

    [SerializeField]
    private GameObject WayPointRefab;

    public static List<Transform> points = new List<Transform>();

    /*void Awake()
    {
        points = new Transform[transform.childCount];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transform.GetChild(i);
        }
    }
    */

    public void Initialize(List<List<int>> generatedTiles, Vector2 indexesStart, Vector2 indexesEnd)
    {
        Vector2 prevIndexes = indexesStart;
        Vector2 currIndexes = new Vector2();
        Vector2 nextIndexes = new Vector2();

        int[] rowNum = new[] { 1, 0, -1, 0 };
        int[] columnNum = new[] { 0, 1, 0, -1 };
        
        for (int i = 0; i < rowNum.Length; i++)
        {
            if (generatedTiles[(int)prevIndexes.x + rowNum[i]][(int)prevIndexes.y + columnNum[i]] == 1)
            {
                currIndexes.x = (int)prevIndexes.x + rowNum[i];
                currIndexes.y = (int)prevIndexes.y + columnNum[i];
            }
        }

        while (currIndexes != indexesEnd)
        {
            for (int i = 0; i < rowNum.Length; i++)
            {
                Vector2 newNextIndexes = new((int)currIndexes.x + rowNum[i], (int)currIndexes.y + columnNum[i]);

                if ((generatedTiles[(int)newNextIndexes.x][(int)newNextIndexes.y] == 1) && 
                    newNextIndexes != prevIndexes)
                {
                    nextIndexes = newNextIndexes;
                }
            }

            if (prevIndexes.x != nextIndexes.x && prevIndexes.y != nextIndexes.y)
            {
                points.Add(Instantiate(WayPointRefab,
                               new Vector3(currIndexes.y * 5, (float)2.5, currIndexes.x * 5),
                               new Quaternion(0, 0, 0, 1), this.transform).transform);
            }

            prevIndexes = currIndexes;
            currIndexes = nextIndexes;
        }

        points.Add(Instantiate(WayPointRefab,
                           new Vector3(indexesEnd.y * 5, (float)2.5, indexesEnd.x * 5),
                           new Quaternion(0, 0, 0, 1), this.transform).transform);
    }

}
