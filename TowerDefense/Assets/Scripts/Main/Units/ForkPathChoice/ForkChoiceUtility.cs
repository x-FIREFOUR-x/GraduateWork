using System;
using System.Collections.Generic;

using UnityEngine;

using TowerDefense.Main.Map.Path;


namespace TowerDefense.Main.Units.ForkPathChoice
{
    public static class ForkChoiceUtility
    {
        private const float tieTolerance = 0.001f;


        public static PathGraphNode ChooseBestBranch(PathGraphNode forkStartNode, Func<PathGraphNode, float> calculateBranchMetric, ForkChoiceGoal goal)
        {
            PathGraphNode bestNextBranchNode = null;
            float bestMetric = 0f;

            foreach (PathGraphNode nextBranchNode in forkStartNode.Next)
            {
                float metric = goal == ForkChoiceGoal.Minimize ? calculateBranchMetric(nextBranchNode) : -calculateBranchMetric(nextBranchNode);

                if (bestNextBranchNode == null || metric < bestMetric - tieTolerance)
                {
                    bestNextBranchNode = nextBranchNode;
                    bestMetric = metric;
                }
            }

            return bestNextBranchNode;
        }

        public static float CalculateBranchMetric(PathGraphNode fromNode, PathGraphNode toNode, PathGraphNode forkEndNode,
                                                  Func<PathGraphNode, PathGraphNode, float> calculateSegmentMetric,
                                                  ForkChoiceGoal goal, Dictionary<PathGraphNode, float> branchMetricsCache)
        {
            return calculateSegmentMetric(fromNode, toNode) + CalculateRemainingBranchPartMetric(toNode, forkEndNode, calculateSegmentMetric, goal, branchMetricsCache);
        }

        private static float CalculateRemainingBranchPartMetric(PathGraphNode node, PathGraphNode forkEndNode, Func<PathGraphNode, PathGraphNode, float> calculateSegmentMetric,
                                                            ForkChoiceGoal goal, Dictionary<PathGraphNode, float> branchMetricsCache)
        {
            if (node == forkEndNode || node.IsEnd)
                return 0f;

            if (branchMetricsCache.TryGetValue(node, out float cached))
                return cached;

            float result = goal == ForkChoiceGoal.Minimize ? float.MaxValue : float.MinValue;
            foreach (PathGraphNode next in node.Next)
            {
                float metric = CalculateBranchMetric(node, next, forkEndNode, calculateSegmentMetric, goal, branchMetricsCache);
                result = goal == ForkChoiceGoal.Minimize ? Mathf.Min(result, metric) : Mathf.Max(result, metric);
            }

            branchMetricsCache[node] = result;
            return result;
        }

        public static PathGraphNode FindForkEndNode(PathGraphNode forkStartNode)
        {
            HashSet<PathGraphNode> commonReachableNodes = null;
            foreach (PathGraphNode nextBranchNode in forkStartNode.Next)
            {
                HashSet<PathGraphNode> reachableNodes = FindReachableNodes(nextBranchNode);
                reachableNodes.Add(nextBranchNode);
                if (commonReachableNodes == null)
                    commonReachableNodes = reachableNodes;
                else
                    commonReachableNodes.IntersectWith(reachableNodes);
            }

            foreach (PathGraphNode candidate in FindReachableNodesOrderedByDistance(forkStartNode))
            {
                if (commonReachableNodes.Contains(candidate) && !CanReachEndAvoiding(forkStartNode, candidate))
                    return candidate;
            }

            return null;
        }

        public static HashSet<PathGraphNode> FindReachableNodes(PathGraphNode from)
        {
            return new HashSet<PathGraphNode>(FindReachableNodesOrderedByDistance(from));
        }

        private static List<PathGraphNode> FindReachableNodesOrderedByDistance(PathGraphNode from)
        {
            List<PathGraphNode> ordered = new();
            HashSet<PathGraphNode> reached = new() { from };
            Queue<PathGraphNode> queue = new();
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                foreach (PathGraphNode next in queue.Dequeue().Next)
                {
                    if (reached.Add(next))
                    {
                        ordered.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }

            return ordered;
        }

        private static bool CanReachEndAvoiding(PathGraphNode from, PathGraphNode avoided)
        {
            HashSet<PathGraphNode> reached = new() { from, avoided };
            Queue<PathGraphNode> queue = new();
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                PathGraphNode node = queue.Dequeue();
                if (node.IsEnd)
                    return true;

                foreach (PathGraphNode next in node.Next)
                {
                    if (reached.Add(next))
                        queue.Enqueue(next);
                }
            }

            return false;
        }
    }

}
