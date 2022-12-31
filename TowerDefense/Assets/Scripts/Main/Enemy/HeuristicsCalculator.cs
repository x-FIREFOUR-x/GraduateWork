using System;
using System.Collections.Generic;
using UnityEngine;

enum EnemyType
{
    Standard,
    Fast,
    Tank
}

enum TowerType
{
    Turret,
    PanelsTurret,
    RocketLauncher,
    LaserTurret
}

public class HeuristicsCalculator : MonoBehaviour
{
    [SerializeField]
    private string towerTag = "Tower";
    [SerializeField]
    private string pathTileTag = "PathTile";

    private Dictionary<(EnemyType, TowerType), int> advantagePoints;

    void Start()
    {
        advantagePoints = new Dictionary<(EnemyType, TowerType), int>();

        advantagePoints[(EnemyType.Standard, TowerType.Turret)] = 5;
        advantagePoints[(EnemyType.Standard, TowerType.PanelsTurret)] = 5;
        advantagePoints[(EnemyType.Standard, TowerType.RocketLauncher)] = 0;   // 5
        advantagePoints[(EnemyType.Standard, TowerType.LaserTurret)] = 5;

        advantagePoints[(EnemyType.Fast, TowerType.Turret)] = 5;
        advantagePoints[(EnemyType.Fast, TowerType.PanelsTurret)] = 5;
        advantagePoints[(EnemyType.Fast, TowerType.RocketLauncher)] = 10;
        advantagePoints[(EnemyType.Fast, TowerType.LaserTurret)] = 0;

        advantagePoints[(EnemyType.Tank, TowerType.Turret)] = 10;
        advantagePoints[(EnemyType.Tank, TowerType.PanelsTurret)] = 5;
        advantagePoints[(EnemyType.Tank, TowerType.RocketLauncher)] = 15;
        advantagePoints[(EnemyType.Tank, TowerType.LaserTurret)] = 5;
    }

    public float CalculateDamageAllTowers()
    {
        float DPSAllTowers = 0;

        GameObject[] towers = GameObject.FindGameObjectsWithTag(towerTag);
        foreach (var tower in towers)
        {
            DPSAllTowers += tower.GetComponent<Tower>().DamageInSecond();
        }

        /*
        GameObject[] pathTiles = GameObject.FindGameObjectsWithTag(pathTileTag);
        float distance = (pathTiles[0].transform.localScale.x + 1) * (pathTiles.Length - 2);

        float time = distance / standardEnemy.StartSpeed;
        */

        return DPSAllTowers;
    }

    public int TotalPlayerMoney()
    {
        int totalMoney = 0;

        GameObject[] towers = GameObject.FindGameObjectsWithTag(towerTag);
        foreach (var tower in towers)
        {
            totalMoney += tower.GetComponent<Tower>().Price;
        }

        totalMoney += PlayerStats.Money;

        return totalMoney;
    }
}
