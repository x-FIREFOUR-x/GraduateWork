using UnityEngine;
using UnityEngine.UI;

using TowerDefense.Main.Map;
using TowerDefense.Main.Map.Buildings;
using TowerDefense.Algorithms.GeneticAlgorithm.Heuristics;

namespace TowerDefense.Main.Enemies
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField]
        private Transform model;

        [field: SerializeField]
        public EnemyType EnemyType { get; private set; }

        [field: Header("Attributes")]
        [field: SerializeField]
        public float StartHealth { get; private set; } = 100f;
        private float health;

        [field: SerializeField]
        public float StartSpeed { get; private set; } = 10f;
        private float speed;

        [SerializeField]
        private float timeReturnSpeed = 1f;
        private float currentTimeReturnSpeed = 0;

        [field: SerializeField]
        public int Damage { get; private set; } = 10;

        [field: SerializeField]
        public int Price { get; private set; } = 25;


        private Transform target;
        public int WayPointIndex { get; private set; } = 0;
        private float movedDistance = 0;

        public static string enemyTag = "Enemy";

        [Header("SetUpHealthBar")]
        [SerializeField]
        private float startPosY;

        [Header("SetUpHealthBar")]
        [SerializeField]
        private Image healthBar;
        [SerializeField]
        private Transform fullHealthBar;
        [SerializeField]
        private float angleX;

        void Start()
        {
            target = WayPoints.Points[0];
            health = StartHealth;
            speed = StartSpeed;

            transform.SetPositionAndRotation(
                new Vector3(transform.position.x, startPosY, transform.position.z),
                transform.rotation);

            InitializeHealthBar();
        }

        void Update()
        {
            Vector3 targetPosition = new Vector3(target.position.x, 0, target.position.z);
            Vector3 position = new Vector3(transform.position.x, 0, transform.position.z);

            Vector3 direction = targetPosition - position;
            transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);

            movedDistance += (direction.normalized * speed * Time.deltaTime).magnitude;

            if (Vector3.Distance(position, targetPosition) <= 0.2f)
            {
                if (!GetNextWayPoint())
                {
                    DamageEndBuilding();
                    Die();
                }

                Rotate();
            }

            UpdateSpeed();
        }

        private bool GetNextWayPoint()
        {
            WayPointIndex++;
            if (WayPointIndex > WayPoints.Points.Count - 1)
            {
                return false;
            }
            else
            {
                target = WayPoints.Points[WayPointIndex];
                return true;
            }
        }

        private void Die()
        {
            Destroy(gameObject);

            if (EnemiesHeuristicsCalculator.instance != null)
                EnemiesHeuristicsCalculator.instance.AddDistanceMovedEnemy(movedDistance);
        }

        private void DamageEndBuilding()
        {
            GameObject[] endBuilding = GameObject.FindGameObjectsWithTag(EndBuilding.endBuildingTag);

            if (endBuilding[0] != null)
            {
                endBuilding[0].GetComponent<EndBuilding>().TakeDamage(Damage);
            }


        }

        private void UpdateSpeed()
        {
            currentTimeReturnSpeed -= Time.deltaTime;

            if (currentTimeReturnSpeed <= 0f)
            {
                speed = StartSpeed;
            }
        }

        private void InitializeHealthBar()
        {
            Vector3 angles = transform.localEulerAngles;
            angles.x = angleX;
            angles.y = -angles.y;
            fullHealthBar.localEulerAngles = angles;
        }

        private void Rotate()
        {
            Vector3 dir = target.position - model.position;
            dir.Normalize();
            Quaternion quaternion = Quaternion.LookRotation(dir, Vector3.up);
            model.rotation = quaternion;
        }


        public void TakeDamage(float damage)
        {
            health -= damage;

            healthBar.fillAmount = health / StartHealth;

            if (health <= 0)
            {
                Die();
            }
        }

        public void Slow(float percentSlowing)
        {
            speed = StartSpeed * (1 - percentSlowing);

            currentTimeReturnSpeed = timeReturnSpeed;
        }

        public void UpgradeHealth(float upByPercentage)
        {
            StartHealth += StartHealth * upByPercentage;
        }
    }

}
