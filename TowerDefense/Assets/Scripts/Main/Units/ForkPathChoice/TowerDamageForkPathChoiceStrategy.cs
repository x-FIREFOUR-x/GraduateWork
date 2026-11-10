using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Map.Path;
using TowerDefense.Main.Towers;


namespace TowerDefense.Main.Units.ForkPathChoice
{
    public class TowerDamageForkPathChoiceStrategy : IForkPathChoiceStrategy
    {
        private readonly ForkChoiceGoal goal;


        public TowerDamageForkPathChoiceStrategy(ForkChoiceGoal goal)
        {
            this.goal = goal;
        }

        public PathGraphNode ChooseNextNodeAtFork(Unit unit, PathGraphNode forkStartNode)
        {
            List<Tower> towers = FindTowersTargeting(unit);
            float height = unit.TravelHeight;
            PathGraphNode forkEndNode = ForkChoiceUtility.FindForkEndNode(forkStartNode);
            Dictionary<PathGraphNode, float> branchMetricsCache = new();

            float CalculateSegmentDamage(PathGraphNode from, PathGraphNode to) => CalculateSegmentDamageFromTowers(from, to, towers, height);

            return ForkChoiceUtility.ChooseBestBranch(forkStartNode, nextBranchNode => ForkChoiceUtility.CalculateBranchMetric(forkStartNode, nextBranchNode, forkEndNode, CalculateSegmentDamage, goal, branchMetricsCache), goal);
        }

        private static List<Tower> FindTowersTargeting(Unit unit)
        {
            List<Tower> towers = new();

            foreach (GameObject towerObject in GameObject.FindGameObjectsWithTag(Tower.towerTag))
            {
                Tower tower = towerObject.GetComponent<Tower>();
                if (tower != null && tower.IsTargetable(unit))
                    towers.Add(tower);
            }

            return towers;
        }

        private static float CalculateSegmentDamageFromTowers(PathGraphNode from, PathGraphNode to, List<Tower> towers, float height)
        {
            Vector2 start = new(from.Position.x, from.Position.z);
            Vector2 end = new(to.Position.x, to.Position.z);
            float damage = 0f;

            foreach (Tower tower in towers)
            {
                Vector3 towerPosition = tower.transform.position;
                float heightDifference = height - towerPosition.y;
                if (Mathf.Abs(heightDifference) >= tower.ShootRange)
                    continue;

                float shootRangeAtUnitHeight = Mathf.Sqrt(tower.ShootRange * tower.ShootRange - heightDifference * heightDifference);
                float lengthInTowerRange = CalculateLengthInTowerRange(start, end, new Vector2(towerPosition.x, towerPosition.z), shootRangeAtUnitHeight);
                damage += tower.DamageInSecond() * lengthInTowerRange;
            }

            return damage;
        }

        private static float CalculateLengthInTowerRange(Vector2 segmentStart, Vector2 segmentEnd, Vector2 towerCenter, float shootRange)
        {
            Vector2 segmentVector = segmentEnd - segmentStart;
            float segmentLength = segmentVector.magnitude;
            if (segmentLength < 0.0001f)
                return 0f;

            Vector2 segmentDirection = segmentVector / segmentLength;
            Vector2 startToTowerCenter = towerCenter - segmentStart;
            float distanceToClosestPoint = Vector2.Dot(startToTowerCenter, segmentDirection);
            float towerToLineDistanceSquared = startToTowerCenter.sqrMagnitude - distanceToClosestPoint * distanceToClosestPoint;
            float halfChordLengthSquared = shootRange * shootRange - towerToLineDistanceSquared;
            if (halfChordLengthSquared <= 0f)
                return 0f;

            float halfChordLength = Mathf.Sqrt(halfChordLengthSquared);
            float rangeEnterDistance = Mathf.Max(0f, distanceToClosestPoint - halfChordLength);
            float rangeExitDistance = Mathf.Min(segmentLength, distanceToClosestPoint + halfChordLength);
            return Mathf.Max(0f, rangeExitDistance - rangeEnterDistance);
        }
    }

}
