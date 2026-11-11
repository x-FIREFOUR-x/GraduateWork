namespace TowerDefense.Main.Units.ForkPathChoice
{
    public static class ForkPathChoiceStrategies
    {
        private static readonly IForkPathChoiceStrategy RandomStrategy = new RandomForkPathChoiceStrategy();
        private static readonly IForkPathChoiceStrategy ShortestPathStrategy = new PathLengthForkPathChoiceStrategy(ForkChoiceGoal.Minimize);
        private static readonly IForkPathChoiceStrategy LongestPathStrategy = new PathLengthForkPathChoiceStrategy(ForkChoiceGoal.Maximize);
        private static readonly IForkPathChoiceStrategy SafestStrategy = new TowerDamageForkPathChoiceStrategy(ForkChoiceGoal.Minimize);
        private static readonly IForkPathChoiceStrategy MostDangerousStrategy = new TowerDamageForkPathChoiceStrategy(ForkChoiceGoal.Maximize);
        private static readonly IForkPathChoiceStrategy LeastCrowdedStrategy = new UnitCountForkPathChoiceStrategy(ForkChoiceGoal.Minimize);
        private static readonly IForkPathChoiceStrategy MostCrowdedStrategy = new UnitCountForkPathChoiceStrategy(ForkChoiceGoal.Maximize);


        public static IForkPathChoiceStrategy GetByUnitType(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Goblin => RandomStrategy,
                UnitType.Bat => LeastCrowdedStrategy,
                UnitType.OrcWargRider => LeastCrowdedStrategy,
                UnitType.Troll => MostDangerousStrategy,
                UnitType.WraithDragonRider => SafestStrategy,
                UnitType.Wraith => MostCrowdedStrategy,
                UnitType.UrukStandardBearer => SafestStrategy,
                UnitType.OrcAlphaWargRider => ShortestPathStrategy,
                _ => RandomStrategy
            };
        }
    }

}
