using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTower : Tower
{
    [Header("Attributes")]
    [SerializeField]
    private float damage = 10f;

    [Header("Prefabs")]
    [SerializeField]
    private LineRenderer lineLaser;
    [SerializeField]
    private ParticleSystem hitEffect;
    [SerializeField]
    private Light lightHitEffect;

    void Update()
    {
        if (target != null)
        {
            RotateToTarget();
            ActiveLaser();

            Vector3 direction = pointStartFire.position - target.position;
            hitEffect.transform.position = target.position + direction.normalized;
            hitEffect.transform.rotation = Quaternion.LookRotation(direction);


            target.GetComponent<Enemy>().TakeDamage(damage * Time.deltaTime);
        }
        else
        {
            DisactiveLaser();
        }
    }

    private void ActiveLaser()
    {
        if (!lineLaser.enabled)
        {
            lineLaser.enabled = true;
            hitEffect.Play();
            lightHitEffect.enabled = true;
        }

        lineLaser.SetPosition(0, pointStartFire.position);
        lineLaser.SetPosition(1, target.position);
    }

    private void DisactiveLaser()
    {
        if (lineLaser.enabled)
        {
            lineLaser.enabled = false;
            hitEffect.Stop();
            lightHitEffect.enabled = false;
        }
    }

}
