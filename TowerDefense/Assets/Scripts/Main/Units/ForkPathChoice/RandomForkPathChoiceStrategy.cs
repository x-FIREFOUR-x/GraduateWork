using UnityEngine;

using TowerDefense.Main.Map.Path;


namespace TowerDefense.Main.Units.ForkPathChoice
{
    public class RandomForkPathChoiceStrategy : IForkPathChoiceStrategy
    {
        public string Name { get { return "Random"; } }

        public PathGraphNode ChooseNextNodeAtFork(Unit unit, PathGraphNode forkStartNode)
        {
            return forkStartNode.Next[Random.Range(0, forkStartNode.Next.Count)];
        }
    }

}
