namespace TowerDefense.Main.Enemies.ForkPathChoice
{
    public static class ForkPathChoiceStrategies
    {
        private static readonly IForkPathChoiceStrategy RandomStrategy = new RandomForkPathChoiceStrategy();


        public static IForkPathChoiceStrategy GetByEnemyType(EnemyType enemyType)
        {
            return enemyType switch
            {
                EnemyType.Goblin => RandomStrategy,
                EnemyType.Bat => RandomStrategy,
                EnemyType.Fast => RandomStrategy,
                EnemyType.Troll => RandomStrategy,
                _ => RandomStrategy
            };
        }
    }

}
