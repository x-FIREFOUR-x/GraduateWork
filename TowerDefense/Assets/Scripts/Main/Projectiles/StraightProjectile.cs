using UnityEngine;

using TowerDefense.Main.Units;


namespace TowerDefense.Main.Projectiles
{
    public class StraightProjectile : Projectile
    {
        private Transform targetUnit;
        private Vector3 offsetTarget;


        public override void Seek(Transform target, Vector3 offset)
        {
            targetUnit = target;
            offsetTarget = offset;

            if (targetUnit != null)
                transform.LookAt(targetUnit.position + offsetTarget);
        }

        void Update()
        {
            if (targetUnit != null)
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

            targetUnit.GetComponent<Unit>().TakeDamage(Damage);

            Disappear();
        }

        private Vector3 GetDirectionToTarget()
        {
            return targetUnit.position + offsetTarget - transform.position;
        }
    }

}
