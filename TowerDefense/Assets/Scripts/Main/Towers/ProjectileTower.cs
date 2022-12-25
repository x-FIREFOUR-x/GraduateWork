using System.Collections;
using System.Collections.Generic;
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
            rotateToTarget();
            if (timeToNextFire <= 0f)
            {
                StartCoroutine(Shoot());
                timeToNextFire = timeBetweenShoots;
            }
        }
        timeToNextFire -= Time.deltaTime;
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
}
