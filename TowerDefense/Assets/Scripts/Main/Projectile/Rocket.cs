using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : Projectile
{
    private Vector3 targetStartPosition;

    [SerializeField]
    protected float explosionRadius = 0f;

    public override void Seek(Transform target)
    {
        targetStartPosition = target.position;
        transform.LookAt(target);
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
