using System.Collections.Generic;
using UnityEngine;

public class WayPoints : MonoBehaviour
{

    [SerializeField]
    private GameObject WayPointRefab;

    public static List<Transform> points;

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
        Vector2 currIndexes = indexesStart;
        Vector2 nextIndexes = new Vector2();

        int[] rowNum = new[] { 1, 0, -1, 0 };
        int[] columnNum = new[] { 0, 1, 0, -1 };

        while (currIndexes != indexesEnd)
        {
            for (int i = 0; i < rowNum.Length; i++)
            {
                if(generatedTiles[(int)currIndexes.y + rowNum[i]][(int)currIndexes.x + columnNum[i]] == 1)
                {
                    nextIndexes.y = (int)currIndexes.y + rowNum[i];
                    nextIndexes.x = (int)currIndexes.x + columnNum[i];
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
