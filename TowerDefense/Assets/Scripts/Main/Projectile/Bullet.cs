using UnityEngine;

public class Bullet : Projectile
{
    private Transform targetEnemy;

    public override void Seek(Transform target)
    {
        targetEnemy = target;
        transform.LookAt(targetEnemy);
    }

    void Update()
    {
        if (targetEnemy != null)
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
        else
        {
            Destroy(gameObject);
        }
    }


    protected override void HitTarget()
    {
        GameObject effect = Instantiate(effectHitPrefab, transform.position, transform.rotation);
        Destroy(effect, 1f);

        targetEnemy.GetComponent<Enemy>().TakeDamage(Damage);

        Destroy(this.gameObject);
    }

    private Vector3 GetDiractionToTarget()
    {
        return targetEnemy.position - transform.position;
    }
}
