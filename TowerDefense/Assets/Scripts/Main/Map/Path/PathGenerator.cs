using System.Collections.Generic;

using UnityEngine;


namespace TowerDefense.Main.Map.Path
{
    public class PathGenerator
    {
        private const int MaxMainPathAttempts = 20;
        private const int MaxBranchAttempts = 50;
        private const int MinBranchLength = 3;

        private static readonly Vector2Int[] Directions = { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) };


        public HashSet<Vector2Int> GenerateBranchedPath(int size, Vector2Int indexesStart, Vector2Int indexesEnd,
                                                        int minBranchesCount, int maxBranchesCount, int minBranchStartEndDistance, int maxBranchLength)
        {
            int minCount = Mathf.Max(minBranchesCount, 1);
            int maxCount = Mathf.Max(maxBranchesCount, minCount);
            int additionalBranchesCount = Random.Range(minCount, maxCount) - 1;

            HashSet<Vector2Int> bestPathCells = null;
            int bestAddedBranchesCount = -1;

            for (int attempt = 0; attempt < MaxMainPathAttempts; attempt++)
            {
                List<Vector2Int> mainPath;
                do
                {
                    mainPath = TryBuildPathByBacktracking(size, indexesStart, indexesEnd, false, new HashSet<Vector2Int>(), 0, int.MaxValue);
                }
                while (mainPath == null);
                HashSet<Vector2Int> pathCells = new(mainPath);

                int addedBranchesCount = AddBranches(size, mainPath, pathCells, additionalBranchesCount, Mathf.Max(minBranchStartEndDistance, 2), maxBranchLength);

                if (addedBranchesCount == additionalBranchesCount)
                    return pathCells;

                if (addedBranchesCount > bestAddedBranchesCount)
                {
                    bestPathCells = pathCells;
                    bestAddedBranchesCount = addedBranchesCount;
                }
            }

            return bestPathCells;
        }

        private List<Vector2Int> TryBuildPathByBacktracking(int size, Vector2Int start, Vector2Int end, bool isFinishNextToTarget,
                                                            HashSet<Vector2Int> pathCells, int minLength, int maxLength)
        {
            HashSet<Vector2Int> visitedNode = new() { start };
            List<Vector2Int> pathNode = new() { start };

            while (pathNode.Count > 0)
            {
                Vector2Int currentNode = pathNode[pathNode.Count - 1];
                List<Vector2Int> neighboringNode = new();

                if (pathNode.Count - 1 < maxLength)
                {
                    foreach (Vector2Int direction in Directions)
                    {
                        Vector2Int node = currentNode + direction;

                        if (IsPathNodeAllowed(node, currentNode, end, size, pathCells, visitedNode)
                            && (!IsFinish(node, end, isFinishNextToTarget) || pathNode.Count >= minLength))
                        {
                            neighboringNode.Add(node);
                        }
                    }
                }

                if (neighboringNode.Count > 0)
                {
                    Vector2Int nextNode = neighboringNode[Random.Range(0, neighboringNode.Count)];
                    visitedNode.Add(nextNode);
                    pathNode.Add(nextNode);

                    if (IsFinish(nextNode, end, isFinishNextToTarget))
                        return pathNode;
                }
                else
                {
                    pathNode.RemoveAt(pathNode.Count - 1);
                }
            }

            return null;
        }

        private bool IsFinish(Vector2Int node, Vector2Int target, bool isFinishNextToTarget)
        {
            return isFinishNextToTarget ? IsNeighbors(node, target) : node == target;
        }

        private bool IsNeighbors(Vector2Int first, Vector2Int second)
        {
            return Mathf.Abs(first.x - second.x) + Mathf.Abs(first.y - second.y) == 1;
        }

        private bool IsPathNodeAllowed(Vector2Int node, Vector2Int currentNode, Vector2Int target, int size,
                                       HashSet<Vector2Int> pathCells, HashSet<Vector2Int> visitedNode)
        {
            if (node.x <= 0 || node.x >= size - 1 || node.y <= 0 || node.y >= size - 1)
                return false;

            if (pathCells.Contains(node) || visitedNode.Contains(node))
                return false;

            foreach (Vector2Int direction in Directions)
            {
                Vector2Int neighbor = node + direction;

                if (neighbor != currentNode && neighbor != target && (pathCells.Contains(neighbor) || visitedNode.Contains(neighbor)))
                    return false;
            }

            return true;
        }

        private int AddBranches(int size, List<Vector2Int> mainPath, HashSet<Vector2Int> pathCells, int branchesCount, int minBranchStartEndDistance, int maxBranchLength)
        {
            List<(int StartIndex, int EndIndex)> branchesStartEndIndexes = new();

            for (int attempt = 0; attempt < MaxBranchAttempts && branchesStartEndIndexes.Count < branchesCount; attempt++)
            {
                TryAddBranch(size, mainPath, pathCells, branchesStartEndIndexes, minBranchStartEndDistance, maxBranchLength);
            }

            return branchesStartEndIndexes.Count;
        }

        private bool TryAddBranch(int size, List<Vector2Int> mainPath, HashSet<Vector2Int> pathCells, List<(int StartIndex, int EndIndex)> branchesStartEndIndexes,
                                  int minBranchStartEndDistance, int maxBranchLength)
        {
            int lastInnerIndex = mainPath.Count - 2;
            if (lastInnerIndex - minBranchStartEndDistance < 1)
                return false;

            int startIndex = Random.Range(1, lastInnerIndex - minBranchStartEndDistance + 1);
            int endIndex = Random.Range(startIndex + minBranchStartEndDistance, lastInnerIndex + 1);

            foreach ((int StartIndex, int EndIndex) addedBranch in branchesStartEndIndexes)
            {
                bool isSeparate = endIndex <= addedBranch.StartIndex || startIndex >= addedBranch.EndIndex;
                bool isNested = (startIndex > addedBranch.StartIndex  && endIndex < addedBranch.EndIndex) || (startIndex < addedBranch.StartIndex && endIndex > addedBranch.EndIndex);

                if (!isSeparate && !isNested)
                    return false;
            }

            Vector2Int start = mainPath[startIndex];
            Vector2Int end = mainPath[endIndex];

            List<Vector2Int> branch = TryBuildPathByBacktracking(size, start, end, true, pathCells, MinBranchLength, maxBranchLength);
            if (branch == null)
                return false;

            pathCells.UnionWith(branch);
            branchesStartEndIndexes.Add((startIndex, endIndex));
            return true;
        }

    }

}
