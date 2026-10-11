using UnityEngine;


namespace TowerDefense.Main.Map.Path
{
    public class PathGraphHolder : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField]
        private GameObject wayPointPrefab;

        public static PathGraph Graph { get; private set; }

        public void Initialize(PathGraph graph)
        {
            Graph = graph;

            foreach (PathGraphNode node in graph.Nodes)
            {
                Instantiate(wayPointPrefab, node.Position, new Quaternion(0, 0, 0, 1), this.transform);
            }
        }

    }

}
