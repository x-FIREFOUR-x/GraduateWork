using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Map.Tiles;
using TowerDefense.Main.Enemies;
using TowerDefense.Main.Towers;
using TowerDefense.Main.Managers;


namespace TowerDefense.Algorithms.GeneticAlgorithm.Heuristics
{
    public class EnemiesHeuristicsCalculator : HeuristicsCalculator<EnemyType>
    {
        static public EnemiesHeuristicsCalculator instance;

        private float distancePath;
        private List<float> distanceMovedEnemies;

        private float percentChangeForFinishedPath = 0.04f;

        private List<Tower> currentTowers;

        private Dictionary<(EnemyType, TowerType), int> advantagePoints;

        public EnemiesHeuristicsCalculator()
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
        public void UpdateCurrentTower()
        {
            var objTowers = GameObject.FindGameObjectsWithTag(Tower.towerTag);

            currentTowers = new();
            foreach (var t in objTowers)
            {
                currentTowers.Add(t.GetComponent<Tower>());
            }
        }


        public int MoneyForWave()
        {
            int moneyForBalance = (int)(PlayerStats.MoneyAttacker * PercentChangeTotalPrice());

            return PlayerStats.MoneyAttacker + moneyForBalance;
        }


        public override int GetHeuristicsValue(List<EnemyType> enemys)
        {
            int heuristicsValue = 0;

            for (int i = 0; i < enemys.Count; i++)
            {
                heuristicsValue++;
                for (int j = 0; j < currentTowers.Count; j++)
                {
                    heuristicsValue += advantagePoints[(enemys[i], currentTowers[j].Type)];
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
            if (distanceMovedEnemies.Count > 0)
            {
                float percent = 0; ;

                int countFinishedEnemies = 0;
                for (int i = 0; i < distanceMovedEnemies.Count; i++)
                {
                    if (distanceMovedEnemies[i] >= distancePath)
                        countFinishedEnemies++;
                }
                percent -= countFinishedEnemies * percentChangeForFinishedPath;

                if (countFinishedEnemies == 0)
                    percent += 20 * percentChangeForFinishedPath;

                distanceMovedEnemies = new List<float>();
                return percent;
            }
            else
            {
                return 0;
            }
        }
    }

}
