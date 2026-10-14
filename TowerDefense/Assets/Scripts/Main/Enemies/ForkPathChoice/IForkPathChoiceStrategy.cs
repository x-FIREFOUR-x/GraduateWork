using TowerDefense.Main.Map.Path;


namespace TowerDefense.Main.Enemies.ForkPathChoice
{
    public interface IForkPathChoiceStrategy
    {
        PathGraphNode ChooseNextNodeAtFork(Enemy enemy, PathGraphNode fork);
    }

}
