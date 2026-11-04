using UnityEngine;

using TowerDefense.Main.Units;


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

        [Header("Effects")]
        [SerializeField]
        protected ParticleSystem trail;
        [SerializeField]
        protected float effectLifetime = 1.5f;

        private System.Predicate<Unit> targetFilter;


        public abstract void Seek(Transform target, Vector3 offsetTarget);

        public void SetTargetFilter(System.Predicate<Unit> filter)
        {
            targetFilter = filter;
        }

        protected bool IsTargetable(Unit unit)
        {
            return unit != null && (targetFilter == null || targetFilter(unit));
        }

        protected abstract void HitTarget();

        protected void SpawnHitEffect(Vector3 position, Quaternion rotation)
        {
            GameObject effect = Instantiate(effectHitPrefab, position, rotation);
            Destroy(effect, effectLifetime);
        }

        // The projectile goes; its trail is left behind to fade out on its own
        protected void Disappear()
        {
            ReleaseTrail();
            Destroy(gameObject);
        }

        private void ReleaseTrail()
        {
            if (trail == null)
                return;

            trail.transform.SetParent(null, true);
            trail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Destroy(trail.gameObject, trail.main.startLifetime.constantMax);
        }
    }

}
