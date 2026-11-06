namespace TowerDefense.Main.Units.ForkPathChoice
{
    public static class ForkPathChoiceStrategies
    {
        private static readonly IForkPathChoiceStrategy RandomStrategy = new RandomForkPathChoiceStrategy();


        public static IForkPathChoiceStrategy GetByUnitType(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Goblin => RandomStrategy,
                UnitType.Bat => RandomStrategy,
                UnitType.OrcWargRider => RandomStrategy,
                UnitType.Troll => RandomStrategy,
                UnitType.WraithDragonRider => RandomStrategy,
                UnitType.Wraith => RandomStrategy,
                _ => RandomStrategy
            };
        }
    }

}
