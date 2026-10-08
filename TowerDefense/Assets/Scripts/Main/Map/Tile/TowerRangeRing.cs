using UnityEngine;


namespace TowerDefense.Main.Map.Tile
{
    public class TowerRangeRing : MonoBehaviour
    {
        public const float OuterRadius = 0.84f;

        [SerializeField]
        private float turnSpeed = 12f;

        private SpriteRenderer spriteRenderer;


        void Update()
        {
            transform.Rotate(Vector3.forward, turnSpeed * Time.deltaTime, Space.Self);
        }


        public void FitRange(float range)
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            float spriteRadius = spriteRenderer.sprite.bounds.extents.x * OuterRadius;
            Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;

            transform.localScale = new Vector3(
                range / (spriteRadius * parentScale.x),
                range / (spriteRadius * parentScale.z),
                transform.localScale.z);
        }
    }

}
