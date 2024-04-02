using UnityEngine;

public abstract class Tower : MonoBehaviour
{
    protected Transform target;

    [field:SerializeField]
    public TowerType Type { get; private set; }

    [Header("Attributes")]
    [SerializeField]
    protected int countProjectiles = 1;

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
    [SerializeField]
    protected Transform pointStartFire;
    [SerializeField]
    protected Transform rotatePart;
    [SerializeField]
    protected Vector3 offsetTower;
    [SerializeField]
    protected Vector3 offsetTarget;

    public static string towerTag = "Tower";


    public float CountProjectiles{ get{ return countProjectiles;}}
    public float ShootRange { get{ return shootRange; }}
    public float TimeBetweenShoots { get { return timeBetweenShoots; } }

    void Start()
    {
        target = null;

        transform.position = new Vector3(
            transform.position.x + offsetTower.x,
            transform.position.y + offsetTower.y,
            transform.position.z + offsetTower.z);

        InvokeRepeating("UpdateTarget", 0f, 0.5f);
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


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }

    public abstract float DamageInSecond();
}
