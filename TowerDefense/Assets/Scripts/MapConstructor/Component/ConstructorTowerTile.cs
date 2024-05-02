using UnityEngine;


namespace TowerDefense.MapConstructor.Component
{
    public class ConstructorTowerTile : MonoBehaviour
    {
        private Renderer render;

        [Header("Attributes")]
        [SerializeField]
        private Color hoverColor;
        private Color unhoverColor;

        void Awake()
        {
            render = GetComponent<Renderer>();
            unhoverColor = render.material.color;
        }


        private void OnMouseDown()
        {
            MapConstructor.instance.GetComponent<MapConstructor>().BuildComponent(this.gameObject);
        }

        private void OnMouseEnter()
        {
            render.material.color = hoverColor;
        }

        private void OnMouseExit()
        {
            render.material.color = unhoverColor;
        }
    }
}
