using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Tank,
    Standard,
    Fast
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
    static public HeuristicsCalculator instance;

    private float distancePath;
    private List<float> distanceMovedEnemies;

    private float percentChangeForQuarter = (float)0.1;
    private float percentChangeForFinishedPath = (float)0.04;

    private GameObject[] currentTowers;

    private Dictionary<(EnemyType, TowerType), int> advantagePoints;

    public HeuristicsCalculator()
    {
        instance = this;

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


        GameObject[] pathTiles = GameObject.FindGameObjectsWithTag(PathTile.pathTileTag);
        distancePath = (pathTiles.Length - 1) * (pathTiles[0].transform.localScale.x + 1);
        distanceMovedEnemies = new List<float>();
    }

    public int TotalPlayerMoney()
    {
        currentTowers = GameObject.FindGameObjectsWithTag(Tower.towerTag);
        int moneyForBalance =(int)(PlayerStats.TotalMoney * PercentChangeTotalPrice());

        return PlayerStats.TotalMoney + moneyForBalance;
    }

    public int GetHeuristicsValue(List<EnemyType> enemys)
    {
        int heuristicsValue = 0;

        for (int i = 0; i < enemys.Count; i++)
        {
            heuristicsValue++;
            for (int j = 0; j < currentTowers.Length; j++)
            {
                heuristicsValue += advantagePoints[(enemys[i], currentTowers[j].GetComponent<Tower>().Type)];
            }
        }

        return heuristicsValue;
    }

    public void AddDistanceMovedEnemy(float distance)
    {
        distanceMovedEnemies.Add(distance);
    }

    private float PercentChangeTotalPrice()
    {
        if(distanceMovedEnemies.Count > 0)
        {
            float percent = 0; ;

            /*
            float averageDistance = 0;
            for (int i = 0; i < distanceMovedEnemies.Count; i++)
            {
                averageDistance += distanceMovedEnemies[i];
            }
            averageDistance = averageDistance / distanceMovedEnemies.Count;

            float percentMoved = averageDistance / distancePath;
            if (percentMoved >= 0.75)
                percent -= 2 * percentChangeForQuarter;
            else if (percentMoved >= 0.5)
                percent -= percentChangeForQuarter;
            else if (percentMoved >= 0.25)
                percent += percentChangeForQuarter;
            else if (percentMoved >= 0)
                percent += 2 * percentChangeForQuarter;
            */
            int countFinishedEnemies = 0;
            for (int i = 0; i < distanceMovedEnemies.Count; i++)
            {
                if (distanceMovedEnemies[i] >= distancePath)
                    countFinishedEnemies++;
            }
            percent -= countFinishedEnemies * percentChangeForFinishedPath;

            distanceMovedEnemies = new List<float>();
            return percent;
        }
        else
        {
            return 0;
        }
    }
}
