using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Map.Path;


namespace TowerDefense.Main.Units.ForkPathChoice
{
    public class PathLengthForkPathChoiceStrategy : IForkPathChoiceStrategy
    {
        private readonly ForkChoiceGoal goal;


        public PathLengthForkPathChoiceStrategy(ForkChoiceGoal goal)
        {
            this.goal = goal;
        }

        public PathGraphNode ChooseNextNodeAtFork(Unit unit, PathGraphNode forkStartNode)
        {
            PathGraphNode forkEndNode = ForkChoiceUtility.FindForkEndNode(forkStartNode);
            Dictionary<PathGraphNode, float> branchMetricsCache = new();
            return ForkChoiceUtility.ChooseBestBranch(forkStartNode, nextBranchNode => ForkChoiceUtility.CalculateBranchMetric(forkStartNode, nextBranchNode, forkEndNode, CalculateSegmentLength, goal, branchMetricsCache), goal);
        }

        private static float CalculateSegmentLength(PathGraphNode from, PathGraphNode to)
        {
            return Vector3.Distance(from.Position, to.Position);
        }
    }

}
