using TowerDefense.Main.Map.Path;


namespace TowerDefense.Main.Units.ForkPathChoice
{
    public interface IForkPathChoiceStrategy
    {
        PathGraphNode ChooseNextNodeAtFork(Unit unit, PathGraphNode forkStartNode);
    }

}
