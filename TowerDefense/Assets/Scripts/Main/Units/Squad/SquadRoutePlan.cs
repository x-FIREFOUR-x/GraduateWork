using System.Collections.Generic;

using TowerDefense.Main.Map.Path;


namespace TowerDefense.Main.Units.Squad
{
    public class SquadRoutePlan
    {
        private readonly Dictionary<PathGraphNode, PathGraphNode> nextNodesByForks = new();


        public void RecordNextBranchNodeAtFork(PathGraphNode forkStartNode, PathGraphNode nextBranchNode)
        {
            nextNodesByForks[forkStartNode] = nextBranchNode;
        }

        public bool TryGetNextBranchNodeAtFork(PathGraphNode forkStartNode, out PathGraphNode nextBranchNode)
        {
            return nextNodesByForks.TryGetValue(forkStartNode, out nextBranchNode);
        }
    }

}
