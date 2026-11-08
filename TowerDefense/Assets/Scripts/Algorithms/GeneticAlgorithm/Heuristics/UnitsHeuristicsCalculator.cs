using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Map.Tile;
using TowerDefense.Main.Units;
using TowerDefense.Main.Towers;
using TowerDefense.Main.Managers;
using TowerDefense.Storage;


namespace TowerDefense.Algorithms.GeneticAlgorithm.Heuristics
{
    public class UnitsHeuristicsCalculator : HeuristicsCalculator<UnitType>
    {
        static public UnitsHeuristicsCalculator instance;

        private float distancePath;
        private List<float> distanceMovedUnits;

        private float percentChangeForFinishedPath = 0.04f;

        private List<Tower> currentTowers;

        private Dictionary<(UnitType, TowerType), int> advantagePoints;

        private const int maxAdvantagePoints = 50;
        private const int minAdvantagePoints = 0;

        public UnitsHeuristicsCalculator()
        {
            instance = this;

            advantagePoints = new Dictionary<(UnitType, TowerType), int>();

            advantagePoints[(UnitType.Goblin, TowerType.BallistaTower)] = 5;
            advantagePoints[(UnitType.Goblin, TowerType.ArcherTower)] = 2;
            advantagePoints[(UnitType.Goblin, TowerType.CatapultTower)] = minAdvantagePoints;
            advantagePoints[(UnitType.Goblin, TowerType.IceMageTower)] = 5;

            advantagePoints[(UnitType.Bat, TowerType.BallistaTower)] = maxAdvantagePoints;
            advantagePoints[(UnitType.Bat, TowerType.ArcherTower)] = minAdvantagePoints;
            advantagePoints[(UnitType.Bat, TowerType.CatapultTower)] = maxAdvantagePoints;
            advantagePoints[(UnitType.Bat, TowerType.IceMageTower)] = minAdvantagePoints;

            advantagePoints[(UnitType.OrcWargRider, TowerType.BallistaTower)] = 5;
            advantagePoints[(UnitType.OrcWargRider, TowerType.ArcherTower)] = 5;
            advantagePoints[(UnitType.OrcWargRider, TowerType.CatapultTower)] = 15;
            advantagePoints[(UnitType.OrcWargRider, TowerType.IceMageTower)] = minAdvantagePoints;

            advantagePoints[(UnitType.Troll, TowerType.BallistaTower)] = 5;
            advantagePoints[(UnitType.Troll, TowerType.ArcherTower)] = 5;
            advantagePoints[(UnitType.Troll, TowerType.CatapultTower)] = 5;
            advantagePoints[(UnitType.Troll, TowerType.IceMageTower)] = 5;

            advantagePoints[(UnitType.WraithDragonRider, TowerType.BallistaTower)] = 15;
            advantagePoints[(UnitType.WraithDragonRider, TowerType.ArcherTower)] = 8;
            advantagePoints[(UnitType.WraithDragonRider, TowerType.CatapultTower)] = 15;
            advantagePoints[(UnitType.WraithDragonRider, TowerType.IceMageTower)] = 5;

            advantagePoints[(UnitType.UrukStandardBearer, TowerType.BallistaTower)] = 5;
            advantagePoints[(UnitType.UrukStandardBearer, TowerType.ArcherTower)] = 12;
            advantagePoints[(UnitType.UrukStandardBearer, TowerType.CatapultTower)] = 3;
            advantagePoints[(UnitType.UrukStandardBearer, TowerType.IceMageTower)] = 5;


            GameObject[] pathTiles = GameObject.FindGameObjectsWithTag(PathTile.pathTileTag);
            MapSizeParamsStorage mapSizeParams = Resources.Load<MapSizeParamsStorage>($"{nameof(MapSizeParamsStorage)}");
            distancePath = (pathTiles.Length - 1) * (mapSizeParams.SizeTile.x + mapSizeParams.OffsetTile.x);
            distanceMovedUnits = new List<float>();
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


        public override float GetHeuristicsValue(List<UnitType> units)
        {
            int heuristicsValue = 0;

            for (int i = 0; i < units.Count; i++)
            {
                heuristicsValue++;
                for (int j = 0; j < currentTowers.Count; j++)
                {
                    heuristicsValue += advantagePoints[(units[i], currentTowers[j].Type)];
                }
            }

            return heuristicsValue;
        }

        public void AddDistanceMovedUnit(float distance)
        {
            distanceMovedUnits.Add(distance);
        }

        private float PercentChangeTotalPrice()
        {
            if (distanceMovedUnits.Count > 0)
            {
                float percent = 0; ;

                int countFinishedUnits = 0;
                for (int i = 0; i < distanceMovedUnits.Count; i++)
                {
                    if (distanceMovedUnits[i] >= distancePath)
                        countFinishedUnits++;
                }
                percent -= countFinishedUnits * percentChangeForFinishedPath;

                if (countFinishedUnits == 0)
                    percent += 20 * percentChangeForFinishedPath;

                distanceMovedUnits = new List<float>();
                return percent;
            }
            else
            {
                return 0;
            }
        }
    }

}
