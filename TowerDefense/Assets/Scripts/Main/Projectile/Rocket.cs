using UnityEngine;

public class Rocket : Projectile
{
    private Vector3 targetStartPosition;

    [SerializeField]
    protected float explosionRadius = 0f;

    public override void Seek(Transform target, Vector3 offsetTarget)
    {
        targetStartPosition = target.position + offsetTarget;

        if (target != null)
            transform.LookAt(target.position + offsetTarget);
    }

    void Update()
    {
        Vector3 direction = GetDiractionToTarget();
        float distanceFrame = speed * Time.deltaTime;

        if (direction.magnitude <= distanceFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(direction.normalized * distanceFrame, Space.World);
    }


    protected override void HitTarget()
    {
        GameObject effect = Instantiate(effectHitPrefab, transform.position, transform.rotation);
        Destroy(effect, 1f);

        ExplodeDamage();
        Destroy(this.gameObject);
    }

    private void ExplodeDamage()
    {
        Collider[] explodedItems = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider explodedItem in explodedItems)
        {
            if (explodedItem.tag == Enemy.enemyTag)
            {
                explodedItem.transform.GetComponent<Enemy>().TakeDamage(Damage);
            }
        }
    }

    private Vector3 GetDiractionToTarget()
    {
        return targetStartPosition - transform.position;
    }
}
