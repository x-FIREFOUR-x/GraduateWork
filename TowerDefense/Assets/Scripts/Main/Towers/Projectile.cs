using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform targetEnemy;

    private Vector3 targetStartPosition;

    [Header("Attributes")]
    [SerializeField]
    private float speed = 70f;
    [SerializeField]
    private float explosionRadius = 0f;
    [field:SerializeField]
    public float Damage { get; private set; } = 100f;

    [Header("Prefabs")]
    [SerializeField]
    public GameObject effectHitPrefab;


    public void Seek(Transform target)
    {
        targetEnemy = target;
        if (target != null)
            targetStartPosition = target.position;
        transform.LookAt(targetEnemy);
    }

    void Update()
    {
        if(targetEnemy != null)
        {
            Vector3 direction = GetDiractionToTarget();
            float distanceFrame = speed * Time.deltaTime;

            if(direction.magnitude <= distanceFrame)
            {
                HitTarget();
                return;
            }

            transform.Translate(direction.normalized * distanceFrame, Space.World);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void HitTarget()
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

    private void DamageOneEnemy(Transform enemy)
    {
        enemy.GetComponent<Enemy>().TakeDamage(Damage);
    }

    private void ExplodeDamage()
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

    private Vector3 GetDiractionToTarget()
    {
        if(explosionRadius > 0)
        {
            return targetStartPosition - transform.position;
        }
        else
        {
            return targetEnemy.position - transform.position;
        }
    }
}
