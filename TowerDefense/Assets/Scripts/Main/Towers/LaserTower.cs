using UnityEngine;

using TowerDefense.Main.Enemies;


namespace TowerDefense.Main.Towers
{
    public class LaserTower : Tower
    {
        [Header("Attributes")]
        [SerializeField]
        private float damage = 10f;
        [SerializeField]
        private float percentSlowing = 0.5f;

        [Header("Prefabs")]
        [SerializeField]
        private LineRenderer lineLaser;
        [SerializeField]
        private ParticleSystem hitEffect;
        [SerializeField]
        private Light lightHitEffect;

        void Update()
        {
            if (target != null)
            {
                RotateToTarget();
                ActiveLaser();

                Vector3 direction = pointStartFire.position - (target.position + offsetTarget);
                hitEffect.transform.position = (target.position + offsetTarget) + direction.normalized;
                hitEffect.transform.rotation = Quaternion.LookRotation(direction);

                target.GetComponent<Enemy>().TakeDamage(damage * Time.deltaTime);

                target.GetComponent<Enemy>().Slow(percentSlowing);
            }
            else
            {
                DisactiveLaser();
            }
        }

        private void ActiveLaser()
        {
            if (!lineLaser.enabled)
            {
                lineLaser.enabled = true;
                hitEffect.Play();
                lightHitEffect.enabled = true;
            }

            lineLaser.SetPosition(0, pointStartFire.position);
            lineLaser.SetPosition(1, target.position + offsetTarget);
        }

        private void DisactiveLaser()
        {
            if (lineLaser.enabled)
            {
                lineLaser.enabled = false;
                hitEffect.Stop();
                lightHitEffect.enabled = false;
            }
        }

        public override float DamageInSecond()
        {
            return damage;
        }

    }
}
