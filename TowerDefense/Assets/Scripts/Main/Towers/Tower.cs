using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Enemies;
using TowerDefense.Main.Projectiles;


namespace TowerDefense.Main.Towers
{
    public abstract class Tower : MonoBehaviour
    {
        protected Transform target;

        [field: SerializeField]
        public TowerType Type { get; private set; }

        [Header("Prefabs")]
        // What the tower shoots; a tower that does not shoot projectiles leaves it empty
        [SerializeField]
        protected GameObject projectilePrefab;

        [Header("Attributes")]
        [SerializeField]
        protected float timeBetweenShoots = 1f;
        [SerializeField]
        protected float timeToNextFire = 0f;

        [SerializeField]
        protected float shootRange = 15f;

        [SerializeField]
        protected float rotateSpeed = 8f;

        [field: SerializeField]
        public int Price { get; private set; } = 100;


        [Header("Setup Fields")]
        // One projectile leaves from each of these points per shot
        [SerializeField]
        protected List<Transform> pointStartFire = new();
        [SerializeField]
        protected Transform rotatePart;
        [SerializeField]
        protected Vector3 offsetTower;
        [SerializeField]
        protected Vector3 offsetTarget;

        public static string towerTag = "Tower";

        // While a shot plays out, from the release to being loaded again, the tower does not start another
        protected bool isReloading;


        public int CountProjectileEntitys { get { return pointStartFire.Count; } }
        public float ShootRange { get { return shootRange; } }
        public float TimeBetweenShoots { get { return timeBetweenShoots; } }

        void Start()
        {
            target = null;

            transform.position = new Vector3(
                transform.position.x + offsetTower.x,
                transform.position.y + offsetTower.y,
                transform.position.z + offsetTower.z);

            InvokeRepeating("UpdateTarget", 0f, 0.1f);
        }

        protected void UpdateTarget()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(Enemy.enemyTag);

            float shortestDistance = Mathf.Infinity;
            GameObject nearestEnemy = null;


            foreach (GameObject enemy in enemies)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
                if (shortestDistance > distanceToEnemy)
                {
                    shortestDistance = distanceToEnemy;
                    nearestEnemy = enemy;
                }
            }

            if (nearestEnemy != null && shortestDistance <= shootRange)
            {
                target = nearestEnemy.transform;
            }
            else
            {
                target = null;
            }
        }

        protected void RotateToTarget()
        {
            Vector3 direction = target.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Vector3 rotation = Quaternion.Lerp(rotatePart.rotation, lookRotation, Time.deltaTime * rotateSpeed).eulerAngles;
            rotatePart.rotation = Quaternion.Euler(0f, rotation.y, 0f);
        }

        // Called every frame by a shooting tower: turns to the target and starts a shot once the time between shots
        // has run out and the last one has played out
        protected void AimAndShoot()
        {
            if (target != null)
            {
                RotateToTarget();

                if (timeToNextFire <= 0f && !isReloading)
                {
                    StartCoroutine(ShootAndReload());
                    timeToNextFire = timeBetweenShoots;
                }
            }
            timeToNextFire -= Time.deltaTime;
        }

        private IEnumerator ShootAndReload()
        {
            isReloading = true;
            yield return StartCoroutine(Shoot());
            isReloading = false;
        }

        // The whole shot with its animation: the release and the reloading after it
        protected virtual IEnumerator Shoot()
        {
            yield break;
        }

        // A projectile from each fire point, sent after the target
        protected void Fire()
        {
            foreach (Transform start in pointStartFire)
            {
                if (start == null)
                    continue;

                GameObject projectileObject = Instantiate(projectilePrefab, start.position, start.rotation);
                Projectile projectile = projectileObject.GetComponent<Projectile>();
                if (projectile != null)
                    projectile.Seek(target, offsetTarget);
            }
        }

        // Lays a unit long piece, such as a string or a rope, from one point to the other
        protected static void Stretch(Transform piece, Vector3 from, Vector3 to)
        {
            Vector3 span = to - from;
            piece.localPosition = from;
            piece.localRotation = Quaternion.LookRotation(span);
            piece.localScale = new Vector3(1f, 1f, span.magnitude);
        }


        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, shootRange);
        }

        public virtual float DamageInSecond()
        {
            return projectilePrefab.GetComponent<Projectile>().Damage * CountProjectileEntitys / timeBetweenShoots;
        }
    }

}
