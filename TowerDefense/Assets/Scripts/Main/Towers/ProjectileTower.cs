using System.Collections;
using UnityEngine;

public class ProjectileTower : Tower
{
    [Header("Prefabs")]
    [SerializeField]
    private GameObject projectilePrefab;


    void Update()
    {
        if(target != null)
        {
            RotateToTarget();
            if (timeToNextFire <= 0f)
            {
                StartCoroutine(Shoot());
                timeToNextFire = timeBetweenShoots;
            }
        }
        timeToNextFire -= Time.deltaTime;
    }

    private IEnumerator Shoot()
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

    public override float DamageInSecond()
    {
        return  projectilePrefab.GetComponent<Projectile>().Damage * countProjectiles / timeBetweenShoots;
    }
}
