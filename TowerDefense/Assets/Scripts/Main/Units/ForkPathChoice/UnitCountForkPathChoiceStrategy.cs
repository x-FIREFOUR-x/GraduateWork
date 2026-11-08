using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Map.Path;


namespace TowerDefense.Main.Units.ForkPathChoice
{
    public class UnitCountForkPathChoiceStrategy : IForkPathChoiceStrategy
    {
        private readonly ForkChoiceGoal goal;


        public UnitCountForkPathChoiceStrategy(ForkChoiceGoal goal)
        {
            this.goal = goal;
        }

        public PathGraphNode ChooseNextNodeAtFork(Unit unit, PathGraphNode forkStartNode)
        {
            Dictionary<(PathGraphNode, PathGraphNode), int> unitCountBySegments = CountUnitsBySegments(unit);
            PathGraphNode forkEndNode = ForkChoiceUtility.FindForkEndNode(forkStartNode);
            Dictionary<PathGraphNode, float> branchMetricsCache = new();

            float CalculateSegmentUnitCount(PathGraphNode from, PathGraphNode to) => unitCountBySegments.TryGetValue((from, to), out int unitCount) ? unitCount : 0f;

            return ForkChoiceUtility.ChooseBestBranch(forkStartNode, nextBranchNode => ForkChoiceUtility.CalculateBranchMetric(forkStartNode, nextBranchNode, forkEndNode, CalculateSegmentUnitCount, goal, branchMetricsCache), goal);
        }

        private static Dictionary<(PathGraphNode, PathGraphNode), int> CountUnitsBySegments(Unit unit)
        {
            Dictionary<(PathGraphNode, PathGraphNode), int> unitCountBySegments = new();

            foreach (GameObject unitObject in GameObject.FindGameObjectsWithTag(Unit.unitTag))
            {
                Unit otherUnit = unitObject.GetComponent<Unit>();
                if (otherUnit == null || otherUnit == unit || otherUnit.PreviousNode == null || otherUnit.TargetNode == null)
                    continue;

                (PathGraphNode, PathGraphNode) segment = (otherUnit.PreviousNode, otherUnit.TargetNode);
                unitCountBySegments.TryGetValue(segment, out int unitCount);
                unitCountBySegments[segment] = unitCount + 1;
            }

            return unitCountBySegments;
        }
    }

}
