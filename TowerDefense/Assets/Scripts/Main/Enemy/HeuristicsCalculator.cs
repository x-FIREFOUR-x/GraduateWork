using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Standard,
    Fast,
    Tank
}

public enum TowerType
{
    Turret,
    PanelsTurret,
    RocketLauncher,
    LaserTurret
}

public class HeuristicsCalculator
{
    [SerializeField]
    private string towerTag = "Tower";
    [SerializeField]
    private string pathTileTag = "PathTile";

    private GameObject[] currentTowers;

    private Dictionary<(EnemyType, TowerType), int> advantagePoints;

    public HeuristicsCalculator()
    {
        advantagePoints = new Dictionary<(EnemyType, TowerType), int>();

        advantagePoints[(EnemyType.Standard, TowerType.Turret)] = 5;
        advantagePoints[(EnemyType.Standard, TowerType.PanelsTurret)] = 0;
        advantagePoints[(EnemyType.Standard, TowerType.RocketLauncher)] = 0;
        advantagePoints[(EnemyType.Standard, TowerType.LaserTurret)] = 5;

        advantagePoints[(EnemyType.Fast, TowerType.Turret)] = 5;
        advantagePoints[(EnemyType.Fast, TowerType.PanelsTurret)] = 5;
        advantagePoints[(EnemyType.Fast, TowerType.RocketLauncher)] = 10;
        advantagePoints[(EnemyType.Fast, TowerType.LaserTurret)] = 0;

        advantagePoints[(EnemyType.Tank, TowerType.Turret)] = 5;
        advantagePoints[(EnemyType.Tank, TowerType.PanelsTurret)] = 0;
        advantagePoints[(EnemyType.Tank, TowerType.RocketLauncher)] = 10;
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

        currentTowers = GameObject.FindGameObjectsWithTag(towerTag);
        foreach (var tower in currentTowers)
        {
            totalMoney += tower.GetComponent<Tower>().Price;
        }

        totalMoney += PlayerStats.Money;

        return totalMoney;
    }

    public int GetHeuristicsValue(List<EnemyType> enemys)
    {
        int heuristicsValue = 1;

        for (int i = 0; i < enemys.Count; i++)
        {
            for (int j = 0; j < currentTowers.Length; j++)
            {
                heuristicsValue += advantagePoints[(enemys[i], currentTowers[j].GetComponent<Tower>().Type)];
            }
        }

        return heuristicsValue;
    }

}
