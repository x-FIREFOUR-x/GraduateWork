using System.Collections.Generic;

using TowerDefense.Main.Map.Path;
using TowerDefense.Main.Units.ForkPathChoice;


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

        public bool HasPlannedForkNodeAtOrAfter(PathGraphNode node)
        {
            if (nextNodesByForks.ContainsKey(node))
                return true;

            foreach (PathGraphNode reachableNode in ForkChoiceUtility.FindReachableNodes(node))
            {
                if (nextNodesByForks.ContainsKey(reachableNode))
                    return true;
            }

            return false;
        }
    }

}
