using UnityEditor;
using UnityEngine;


namespace TowerDefense.EditorTools.Towers
{
    public static class TowerBakeUtility
    {
        // Fills a tower's list of fire points; the tower fires one projectile from each
        public static void SetFirePoints(SerializedProperty list, params Transform[] points)
        {
            list.arraySize = points.Length;
            for (int i = 0; i < points.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = points[i];
        }
    }

}
