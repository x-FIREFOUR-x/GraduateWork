using UnityEngine;

using TowerDefense.Main.Units;


namespace TowerDefense.Main.Projectiles
{
    public class StraightProjectile : Projectile
    {
        private Unit target;
        private bool aimAtGround;


        public override void Seek(Unit target, bool aimAtGround)
        {
            this.target = target;
            this.aimAtGround = aimAtGround;

            if (target != null)
                transform.LookAt(target.AimPoint(aimAtGround));
        }

        void Update()
        {
            if (target != null)
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

            target.TakeDamage(Damage);

            Disappear();
        }

        private Vector3 GetDirectionToTarget()
        {
            return target.AimPoint(aimAtGround) - transform.position;
        }
    }

}
