using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField]
    private Transform target;


    [Header("Attributes")]

    [SerializeField]
    private float fireRate = 1f;
    [SerializeField]
    private float timeToNextFire = 0f;
    [SerializeField]
    private float shootRange = 15f;


    [Header("Setup Fields")]

    [SerializeField]
    private string enemyTag = "Enemy";

    [SerializeField]
    private float rotateSpeed = 8f;

    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private Transform pointStartFire;

    void Start()
    {
        InvokeRepeating("UpdateTarget", 0f, 0.5f);   
    }

    void UpdateTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

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

    void Update()
    {
        if(target != null)
        {
            Vector3 direction = target.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed).eulerAngles;

            transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);

            if(timeToNextFire <= 0f)
            {
                Shoot();
                timeToNextFire = 1 / fireRate;
            }

            timeToNextFire -= Time.deltaTime;
        }
    }

    void Shoot()
    {
        GameObject bulletObject = Instantiate(bulletPrefab, pointStartFire.position, pointStartFire.rotation);
        Bullet bullet = bulletObject.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.Seek(target);
        }
           
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}
