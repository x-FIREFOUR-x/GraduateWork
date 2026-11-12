using TowerDefense.Main.Map.Path;


namespace TowerDefense.Main.Units.ForkPathChoice
{
    public interface IForkPathChoiceStrategy
    {
        string Name { get; }

        PathGraphNode ChooseNextNodeAtFork(Unit unit, PathGraphNode forkStartNode);
    }

}
