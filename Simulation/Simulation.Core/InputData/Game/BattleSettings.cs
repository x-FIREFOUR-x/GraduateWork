using System;


namespace Simulation.InputData.Game
{
    [Serializable]
    public class BattleSettings
    {
        public float FixedTimeStepSeconds;
        public float UnitSpawnIntervalSeconds;
        public float TowerRetargetIntervalSeconds;
        public float PathNodeReachDistance;
        public float CatapultStoneMinFlightSeconds;
    }
}
