using UnityEngine;


namespace TowerDefense.Main.Map.Tiles
{
    public class TowerTile : MonoBehaviour
    {
        protected Renderer render;

        public GameObject Tower { get; set; }

        public Vector3 GetTowerBuildPosition()
        {
            return transform.position;
        }

        private void Awake()
        {
            render = GetComponent<Renderer>();
        }
    }
}
