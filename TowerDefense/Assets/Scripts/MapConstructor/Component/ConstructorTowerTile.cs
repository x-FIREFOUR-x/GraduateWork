using UnityEngine;

using TowerDefense.Main.Map.Tile;


namespace TowerDefense.MapConstructor.Component
{
    public class ConstructorTowerTile : MonoBehaviour
    {
        private Renderer render;

        [Header("Attributes")]
        [SerializeField]
        private Color hoverColor;

        void Awake()
        {
            render = GetComponent<Renderer>();
        }


        private void OnMouseDown()
        {
            MapConstructor.instance.GetComponent<MapConstructor>().BuildComponent(this.gameObject);
        }

        private void OnMouseEnter()
        {
            TileHighlight.Apply(render, hoverColor);
        }

        private void OnMouseExit()
        {
            TileHighlight.Clear(render);
        }
    }
}
