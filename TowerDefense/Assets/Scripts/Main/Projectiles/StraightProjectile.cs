using UnityEngine;

using TowerDefense.Main.Enemies;


namespace TowerDefense.Main.Projectiles
{
    public class StraightProjectile : Projectile
    {
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
                Disappear();
            }
        }


        protected override void HitTarget()
        {
            SpawnHitEffect(transform.position, transform.rotation);

            targetEnemy.GetComponent<Enemy>().TakeDamage(Damage);

            Disappear();
        }

        private Vector3 GetDirectionToTarget()
        {
            return targetEnemy.position + offsetTarget - transform.position;
        }
    }

}
