using UnityEngine;

using TowerDefense.Main.Enemies;


namespace TowerDefense.Main.Projectiles
{
    public class StraightProjectile : Projectile
    {
        [Header("Components")]
        [SerializeField]
        private ParticleSystem trail;

        [Header("Effect")]
        [SerializeField]
        private float effectLifetime = 1.5f;

        private Transform targetEnemy;
        private Vector3 offsetTarget;


        public override void Seek(Transform target, Vector3 offset)
        {
            targetEnemy = target;
            offsetTarget = offset;

            if (targetEnemy != null)
                transform.LookAt(targetEnemy.position + offsetTarget);
        }

        void Update()
        {
            if (targetEnemy != null)
            {
                Vector3 direction = GetDirectionToTarget();
                float distanceFrame = speed * Time.deltaTime;

                if (direction.magnitude <= distanceFrame)
                {
                    HitTarget();
                    return;
                }

                transform.rotation = Quaternion.LookRotation(direction);
                transform.Translate(direction.normalized * distanceFrame, Space.World);
            }
            else
            {
                ReleaseTrail();
                Destroy(gameObject);
            }
        }


        protected override void HitTarget()
        {
            GameObject effect = Instantiate(effectHitPrefab, transform.position, transform.rotation);
            Destroy(effect, effectLifetime);

            targetEnemy.GetComponent<Enemy>().TakeDamage(Damage);

            ReleaseTrail();
            Destroy(gameObject);
        }

        private Vector3 GetDirectionToTarget()
        {
            return targetEnemy.position + offsetTarget - transform.position;
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
