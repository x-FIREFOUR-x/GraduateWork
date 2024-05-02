using System.Collections;

using UnityEngine;

using TowerDefense.Main.Projectiles;


namespace TowerDefense.Main.Towers
{
    public class ProjectileEntityTower : Tower
    {
        [Header("Prefabs")]
        [SerializeField]
        private GameObject ProjectileEntityPrefab;


        void Update()
        {
            if (target != null)
            {
                RotateToTarget();
                if (timeToNextFire <= 0f)
                {
                    StartCoroutine(Shoot());
                    timeToNextFire = timeBetweenShoots;
                }
            }
            timeToNextFire -= Time.deltaTime;
        }

        private IEnumerator Shoot()
        {
            for (int i = 0; i < countProjectileEntitys; i++)
            {
                GameObject bulletObject = Instantiate(ProjectileEntityPrefab, pointStartFire.position, pointStartFire.rotation);
                Projectile bullet = bulletObject.GetComponent<Projectile>();

                if (bullet != null)
                {
                    bullet.Seek(target, offsetTarget);
                }
                yield return new WaitForSeconds(0.2f);
            }
        }

        public override float DamageInSecond()
        {
            return ProjectileEntityPrefab.GetComponent<Projectile>().Damage * countProjectileEntitys / timeBetweenShoots;
        }
    }

}
