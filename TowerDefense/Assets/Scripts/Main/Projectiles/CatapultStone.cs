using UnityEngine;

using TowerDefense.Main.Enemies;


namespace TowerDefense.Main.Projectiles
{
    public class CatapultStone : Projectile
    {
        [SerializeField]
        protected float explosionRadius = 4f;

        [Header("Flight")]
        [SerializeField]
        private float arcHeightPerDistance = 0.3f;
        [SerializeField]
        private float minArcHeight = 2f;
        [SerializeField]
        private float tumbleSpeed = 400f;

        private Vector3 startPosition;
        private Vector3 aimPoint;
        private float arcHeight;
        private float flightTime;
        private float elapsed;
        private Vector3 tumbleAxis;
        private bool isLaunched;

        [Header("Components")]
        [SerializeField]
        private Transform graphic;
        [SerializeField]
        private ParticleSystem trail;

        private const float effectLifetime = 2.5f;


        public override void Seek(Transform target, Vector3 offsetTarget)
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            Launch(target.position + offsetTarget, 0f);
        }

        public void Launch(Vector3 point, float timeSpent)
        {
            startPosition = transform.position;
            aimPoint = point;

            arcHeight = Mathf.Max(minArcHeight, HorizontalDistance(startPosition, aimPoint) * arcHeightPerDistance);
            flightTime = Mathf.Max(Vector3.Distance(startPosition, aimPoint) / speed - timeSpent, 0.1f);
            elapsed = 0f;
            tumbleAxis = Random.onUnitSphere;
            isLaunched = true;
        }

        void Update()
        {
            if (!isLaunched)
                return;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flightTime);

            transform.position = Vector3.Lerp(startPosition, aimPoint, t) + Vector3.up * (arcHeight * 4f * t * (1f - t));

            if (graphic != null)
                graphic.Rotate(tumbleAxis, tumbleSpeed * Time.deltaTime, Space.World);

            if (t >= 1f)
                HitTarget();
        }


        protected override void HitTarget()
        {
            GameObject effect = Instantiate(effectHitPrefab, aimPoint, Quaternion.identity);
            Destroy(effect, effectLifetime);

            ExplodeDamage();
            ReleaseTrail();

            Destroy(gameObject);
        }

        private void ExplodeDamage()
        {
            Collider[] hitItems = Physics.OverlapSphere(aimPoint, explosionRadius);
            foreach (Collider hitItem in hitItems)
            {
                if (hitItem.tag == Enemy.enemyTag)
                    hitItem.transform.GetComponent<Enemy>().TakeDamage(Damage);
            }
        }

        private void ReleaseTrail()
        {
            if (trail == null)
                return;

            trail.transform.SetParent(null, true);
            trail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Destroy(trail.gameObject, trail.main.startLifetime.constantMax);
        }

        private static float HorizontalDistance(Vector3 from, Vector3 to)
        {
            return new Vector2(to.x - from.x, to.z - from.z).magnitude;
        }
    }

}
