using UnityEngine;


namespace TowerDefense.Main.Projectiles
{
    public abstract class Projectile : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField]
        public GameObject effectHitPrefab;

        [Header("Attributes")]
        [SerializeField]
        protected float speed = 70f;
        [field: SerializeField]
        public float Damage { get; private set; } = 100f;

        public abstract void Seek(Transform target, Vector3 offsetTarget);

        protected abstract void HitTarget();
    }

}
