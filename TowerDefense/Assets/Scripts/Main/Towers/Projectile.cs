using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform targetEnemy;

    [Header("Attributes")]
    [SerializeField]
    private float speed = 70f;
    [SerializeField]
    private float explosionRadius = 0f;
    [SerializeField]
    private float damage = 100f;

    [Header("Prefabs")]
    [SerializeField]
    public GameObject effectHitPrefab;


    public void Seek(Transform target)
    {
        targetEnemy = target;
    }

    void Update()
    {
        if(targetEnemy != null)
        {
            Vector3 direction = targetEnemy.position - transform.position;
            float distanceFrame = speed * Time.deltaTime;

            if(direction.magnitude <= distanceFrame)
            {
                HitTarget();
                return;
            }

            transform.Translate(direction.normalized * distanceFrame, Space.World);
            transform.LookAt(targetEnemy);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void HitTarget()
    {
        GameObject effect = Instantiate(effectHitPrefab, transform.position, transform.rotation);
        Destroy(effect, 1f);

        if (explosionRadius > 0f)
        {
            ExplodeDamage();
        }
        else
        {
            DamageOneEnemy(targetEnemy);
        }

        Destroy(this.gameObject);
    }

    void DamageOneEnemy(Transform enemy)
    {
        enemy.GetComponent<Enemy>().TakeDamage(damage);
    }

    void ExplodeDamage()
    {
        Collider[] explodedItems = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach(Collider explodedItem in explodedItems)
        {
            if(explodedItem.tag == Enemy.enemyTag)
            {
                DamageOneEnemy(explodedItem.transform);
            }
        }
    }
}
