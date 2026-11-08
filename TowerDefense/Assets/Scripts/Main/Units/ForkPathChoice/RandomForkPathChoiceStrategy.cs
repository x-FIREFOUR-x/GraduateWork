using UnityEngine;

using TowerDefense.Main.Map.Path;


namespace TowerDefense.Main.Units.ForkPathChoice
{
    public class RandomForkPathChoiceStrategy : IForkPathChoiceStrategy
    {
        public PathGraphNode ChooseNextNodeAtFork(Unit unit, PathGraphNode fork)
        {
            return fork.Next[Random.Range(0, fork.Next.Count)];
        }
    }

}
