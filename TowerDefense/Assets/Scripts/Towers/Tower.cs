using System.Collections;
using UnityEngine;

public class Tower : MonoBehaviour
{
    private Transform target;

    [Header("Attributes")]
    [SerializeField]
    private int countProjectiles = 1;
    [SerializeField]
    private float timeBetweenShoots = 1f;
    [SerializeField]
    private float timeToNextFire = 1f;
    [SerializeField]
    private float shootRange = 15f;
    [SerializeField]
    private float rotateSpeed = 8f;

    [Header("Setup Fields")]
    [SerializeField]
    private Transform pointStartFire;
    [SerializeField]
    private Transform rotatePart;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject projectilePrefab;


    void Start()
    {
        target = null;
        InvokeRepeating("UpdateTarget", 0f, 0.5f);   
    }

    void UpdateTarget()
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

    void Update()
    {
        if(target != null)
        {
            Vector3 direction = target.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Vector3 rotation = Quaternion.Lerp(rotatePart.rotation, lookRotation, Time.deltaTime * rotateSpeed).eulerAngles;
            rotatePart.rotation = Quaternion.Euler(0f, rotation.y, 0f);

            if(timeToNextFire <= 0f)
            {
                StartCoroutine(Shoot());
                timeToNextFire = timeBetweenShoots;
            }

            timeToNextFire -= Time.deltaTime;
        }
    }

    IEnumerator Shoot()
    {
        for (int i = 0; i < countProjectiles; i++)
        {
            GameObject bulletObject = Instantiate(projectilePrefab, pointStartFire.position, pointStartFire.rotation);
            Projectile bullet = bulletObject.GetComponent<Projectile>();

            if (bullet != null)
            {
                bullet.Seek(target);
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}
