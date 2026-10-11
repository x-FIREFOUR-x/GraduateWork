using UnityEngine;


namespace TowerDefense.Main.Map.Path
{
    public class WayPoints : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField]
        private GameObject wayPointRefab;

        public static PathGraph Graph { get; private set; }

        public void Initialize(PathGraph graph)
        {
            Graph = graph;

            foreach (WayPointNode node in graph.Nodes)
            {
                Instantiate(wayPointRefab, node.Position, new Quaternion(0, 0, 0, 1), this.transform);
            }
        }

    }

}
