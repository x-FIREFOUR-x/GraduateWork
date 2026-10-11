namespace TowerDefense.Main.Enemies.ForkPathChoice
{
    public static class ForkPathChoiceStrategies
    {
        private static readonly IForkPathChoiceStrategy RandomStrategy = new RandomForkPathChoiceStrategy();


        public static IForkPathChoiceStrategy GetByEnemyType(EnemyType enemyType)
        {
            return enemyType switch
            {
                EnemyType.Standard => RandomStrategy,
                EnemyType.Fast => RandomStrategy,
                EnemyType.Tank => RandomStrategy,
                _ => RandomStrategy
            };
        }
    }

}
