using System.Collections.Generic;
using UnityEngine;

public class WayPoints : MonoBehaviour
{

    [SerializeField]
    private GameObject WayPointRefab;

    private static List<Transform> points = new List<Transform>();

    public static List<Transform> Points 
    { 
        get { return points; } 
    }

    public void Initialize(List<Vector2Int> generatedPath)
    {
        int indexPathNode = 1;

        while (indexPathNode != generatedPath.Count - 1)
        {
            if (generatedPath[indexPathNode - 1].x != generatedPath[indexPathNode + 1].x 
                && generatedPath[indexPathNode - 1].y != generatedPath[indexPathNode + 1].y)
            {
                points.Add(Instantiate(WayPointRefab,
                               new Vector3(generatedPath[indexPathNode].y * 5, (float)2.5, generatedPath[indexPathNode].x * 5),
                               new Quaternion(0, 0, 0, 1), this.transform).transform);
            }

            indexPathNode++;
        }

        points.Add(Instantiate(WayPointRefab,
                           new Vector3(generatedPath[indexPathNode].y * 5, (float)2.5, generatedPath[indexPathNode].x * 5),
                           new Quaternion(0, 0, 0, 1), this.transform).transform);
    }

}
