using UnityEngine;

using TowerDefense.Main.Map.Path;


namespace TowerDefense.Main.Enemies.ForkPathChoice
{
    public class RandomForkPathChoiceStrategy : IForkPathChoiceStrategy
    {
        public PathGraphNode ChooseNextNodeAtFork(Enemy enemy, PathGraphNode fork)
        {
            return fork.Next[Random.Range(0, fork.Next.Count)];
        }
    }

}
