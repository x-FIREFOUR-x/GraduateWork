using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Managers;
using TowerDefense.Main.Map.Tile;
using TowerDefense.Storage;


namespace TowerDefense.Opponents.Attacker.State
{
    public sealed class AttackerBudget
    {
        private const float PercentChangeForFinishedPath = 0.04f;

        public static AttackerBudget Instance { get; private set; }

        private readonly float distancePath;
        private readonly List<float> distanceMovedUnits = new List<float>();


        public AttackerBudget()
        {
            Instance = this;

            GameObject[] pathTiles = GameObject.FindGameObjectsWithTag(PathTile.pathTileTag);
            MapSizeParamsStorage mapSizeParams = Resources.Load<MapSizeParamsStorage>($"{nameof(MapSizeParamsStorage)}");
            distancePath = (pathTiles.Length - 1) * (mapSizeParams.SizeTile.x + mapSizeParams.OffsetTile.x);
        }

        public int MoneyForWave()
        {
            int moneyForBalance = (int)(PlayerStats.MoneyAttacker * PercentChangeTotalPrice());
            return PlayerStats.MoneyAttacker + moneyForBalance;
        }

        public void AddDistanceMovedUnit(float distance)
        {
            distanceMovedUnits.Add(distance);
        }

        private float PercentChangeTotalPrice()
        {
            if (distanceMovedUnits.Count == 0)
                return 0;

            int countFinishedUnits = 0;
            foreach (float distance in distanceMovedUnits)
                if (distance >= distancePath)
                    countFinishedUnits++;

            float percent = -countFinishedUnits * PercentChangeForFinishedPath;
            if (countFinishedUnits == 0)
                percent += 20 * PercentChangeForFinishedPath;

            distanceMovedUnits.Clear();
            return percent;
        }
    }
}
