using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Units;
using TowerDefense.Main.Projectiles;


namespace TowerDefense.Main.Towers
{
    public abstract class Tower : MonoBehaviour
    {
        public static string towerTag = "Tower";

        [field: SerializeField]
        public TowerType Type { get; private set; }

        [Header("Targets")]
        [SerializeField]
        protected List<UnitType> targetableUnitTypes = new((UnitType[])System.Enum.GetValues(typeof(UnitType)));
        [SerializeField]
        protected bool aimAtGround;
        protected Vector3 TargetPoint { get { return target.AimPoint(aimAtGround); } }

        [Header("Setup Fields")]
        [SerializeField]
        protected GameObject projectilePrefab;
        // One projectile leaves from each of these points per shot
        [SerializeField]
        protected List<Transform> pointStartFire = new();
        public int CountProjectileEntitys { get { return pointStartFire.Count; } }
        [SerializeField]
        protected Transform rotatePart;
        [SerializeField]
        protected Vector3 offsetTower;

        [field: Header("Attributes")]
        [field: SerializeField]
        public int Price { get; private set; } = 100;
        [field: SerializeField]
        public float TimeBetweenShoots { get; private set; } = 1f;
        [SerializeField]
        protected float timeToNextFire = 0f;
        [field: SerializeField]
        public float ShootRange { get; private set; } = 15f;
        [SerializeField]
        protected float rotateSpeed = 8f;


        protected Unit target;
        // While a shot plays out, from the release to being loaded again, the tower does not start another
        protected bool isReloading;


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
            GameObject[] units = GameObject.FindGameObjectsWithTag(Unit.unitTag);

            float shortestDistance = Mathf.Infinity;
            GameObject nearestUnit = null;


            foreach (GameObject unit in units)
            {
                if (!IsTargetable(unit.GetComponent<Unit>()))
                    continue;

                float distanceToUnit = Vector3.Distance(transform.position, unit.transform.position);
                if (shortestDistance > distanceToUnit)
                {
                    shortestDistance = distanceToUnit;
                    nearestUnit = unit;
                }
            }

            if (nearestUnit != null && shortestDistance <= ShootRange)
            {
                target = nearestUnit.GetComponent<Unit>();
            }
            else
            {
                target = null;
            }
        }

        protected void RotateToTarget()
        {
            Vector3 direction = target.transform.position - transform.position;
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
                    timeToNextFire = TimeBetweenShoots;
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
                {
                    projectile.SetTargetFilter(IsTargetable);
                    projectile.Seek(target, aimAtGround);
                }
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
            Gizmos.DrawWireSphere(transform.position, ShootRange);
        }

        public bool IsTargetable(Unit unit)
        {
            return unit != null && targetableUnitTypes.Contains(unit.UnitType);
        }

        public virtual float DamageInSecond()
        {
            return projectilePrefab.GetComponent<Projectile>().Damage * CountProjectileEntitys / TimeBetweenShoots;
        }
    }

}
