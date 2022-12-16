using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTower : Tower
{
    [Header("Prefabs")]
    [SerializeField]
    private LineRenderer lineLaser;


    void Update()
    {
        if (target != null)
        {
            rotateToTarget();

            lineLaser.enabled = true;

            lineLaser.SetPosition(0, pointStartFire.position);
            lineLaser.SetPosition(1, target.position);
        }
        else
        {
            lineLaser.enabled = false;
        }
    }
}
