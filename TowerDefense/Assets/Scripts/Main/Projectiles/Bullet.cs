using UnityEngine;

using TowerDefense.Main.Enemies;


namespace TowerDefense.Main.Projectiles
{
    public class Bullet : Projectile
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
                Vector3 direction = GetDiractionToTarget();
                float distanceFrame = speed * Time.deltaTime;

                if (direction.magnitude <= distanceFrame)
                {
                    HitTarget();
                    return;
                }

                transform.Translate(direction.normalized * distanceFrame, Space.World);
            }
            else
            {
                Destroy(gameObject);
            }
        }


        protected override void HitTarget()
        {
            GameObject effect = Instantiate(effectHitPrefab, transform.position, transform.rotation);
            Destroy(effect, 1f);

            targetEnemy.GetComponent<Enemy>().TakeDamage(Damage);

            Destroy(this.gameObject);
        }

        private Vector3 GetDiractionToTarget()
        {
            return targetEnemy.position + offsetTarget - transform.position;
        }
    }

}
