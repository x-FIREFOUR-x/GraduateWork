using System.Collections;

using UnityEngine;

using TowerDefense.Main.Map.Tile;


namespace TowerDefense.MapConstructor.Component
{
    public abstract class ConstructorTile : MonoBehaviour
    {
        private Renderer render;

        [Header("Attributes")]
        [SerializeField]
        private Color hoverColor;
        [SerializeField]
        private Color rejectedColor = new(1f, 0.15f, 0.15f);
        [SerializeField]
        private float rejectedHighlightDuration = 0.5f;

        private bool isHovered;
        private Coroutine rejectedHighlightCoroutine;

        protected void Awake()
        {
            render = GetComponent<Renderer>();
        }


        protected void OnMouseDown()
        {
            MapConstructor.instance.GetComponent<MapConstructor>().BuildComponent(this.gameObject);
        }

        protected void OnMouseEnter()
        {
            isHovered = true;

            if (rejectedHighlightCoroutine == null)
                TileHighlight.Apply(render, hoverColor);
        }

        protected void OnMouseExit()
        {
            isHovered = false;

            if (rejectedHighlightCoroutine == null)
                TileHighlight.Clear(render);
        }

        public void HighlightRejected()
        {
            if (rejectedHighlightCoroutine != null)
                StopCoroutine(rejectedHighlightCoroutine);

            rejectedHighlightCoroutine = StartCoroutine(HighlightRejectedForDuration());
        }

        private IEnumerator HighlightRejectedForDuration()
        {
            TileHighlight.Replace(render, rejectedColor);

            yield return new WaitForSecondsRealtime(rejectedHighlightDuration);

            rejectedHighlightCoroutine = null;

            if (isHovered)
                TileHighlight.Apply(render, hoverColor);
            else
                TileHighlight.Clear(render);
        }
    }

}
