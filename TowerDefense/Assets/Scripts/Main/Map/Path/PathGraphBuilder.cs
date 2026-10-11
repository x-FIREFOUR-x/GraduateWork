using System;
using System.Collections.Generic;

using UnityEngine;


namespace TowerDefense.Main.Map.Path
{
    public static class PathGraphBuilder
    {
        private const int MaxRoutesCount = 1000;

        private static readonly Vector2Int[] Directions = { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) };


        public static bool TryBuild(HashSet<Vector2Int> pathCells, Vector2Int start, Vector2Int end, Func<Vector2Int, Vector3> cellToPosition, out PathGraph graph)
        {
            graph = null;

            if (start == end)
                return false;

            HashSet<Vector2Int> cells = new(pathCells) { start, end };
            HashSet<Vector2Int> branchingCells = FindBranchingCells(cells, start, end);
            Dictionary<Vector2Int, List<List<Vector2Int>>> pathSegments = FindPathSegments(cells, branchingCells);

            HashSet<List<Vector2Int>> routesPathSegments = new();
            if (!TryCollectRoutesFromPathSegments(pathSegments, start, end, routesPathSegments))
                return false;

            if (HasCellsOutsideRoutes(routesPathSegments, cells))
                return false;

            Dictionary<Vector2Int, PathGraphNode> nodes = CreateNodes(routesPathSegments, cellToPosition);
            if (HasCycle(nodes.Values, nodes.Count))
                return false;

            graph = new PathGraph(nodes[start], nodes[end], new List<PathGraphNode>(nodes.Values));
            return true;
        }


        private static HashSet<Vector2Int> FindBranchingCells(HashSet<Vector2Int> cells, Vector2Int start, Vector2Int end)
        {
            HashSet<Vector2Int> branchingCells = new() { start, end };

            foreach (Vector2Int cell in cells)
            {
                if (CountNeighbors(cells, cell) != 2)
                    branchingCells.Add(cell);
            }

            return branchingCells;
        }

        private static int CountNeighbors(HashSet<Vector2Int> cells, Vector2Int cell)
        {
            int countNeighbors = 0;

            foreach (Vector2Int direction in Directions)
            {
                if (cells.Contains(cell + direction))
                    countNeighbors++;
            }

            return countNeighbors;
        }

        private static Dictionary<Vector2Int, List<List<Vector2Int>>> FindPathSegments(HashSet<Vector2Int> cells, HashSet<Vector2Int> branchingCells)
        {
            Dictionary<Vector2Int, List<List<Vector2Int>>> pathSegmentsByBranchingCell = new();

            foreach (Vector2Int branchingCell in branchingCells)
            {
                List<List<Vector2Int>> pathSegmentsFromBranchingCell = new();

                foreach (Vector2Int direction in Directions)
                {
                    if (cells.Contains(branchingCell + direction))
                        pathSegmentsFromBranchingCell.Add(WalkPathSegment(cells, branchingCells, branchingCell, branchingCell + direction));
                }

                pathSegmentsByBranchingCell[branchingCell] = pathSegmentsFromBranchingCell;
            }

            return pathSegmentsByBranchingCell;
        }

        private static List<Vector2Int> WalkPathSegment(HashSet<Vector2Int> cells, HashSet<Vector2Int> branchingCells, Vector2Int segmentStart, Vector2Int firstStep)
        {
            List<Vector2Int> pathSegment = new() { segmentStart, firstStep };

            Vector2Int previousCell = segmentStart;
            Vector2Int currentCell = firstStep;

            while (!branchingCells.Contains(currentCell))
            {
                Vector2Int nextCell = currentCell;

                foreach (Vector2Int direction in Directions)
                {
                    Vector2Int neighbor = currentCell + direction;
                    if (neighbor != previousCell && cells.Contains(neighbor))
                    {
                        nextCell = neighbor;
                        break;
                    }
                }

                previousCell = currentCell;
                currentCell = nextCell;
                pathSegment.Add(currentCell);
            }

            return pathSegment;
        }

        private static bool TryCollectRoutesFromPathSegments(Dictionary<Vector2Int, List<List<Vector2Int>>> pathSegmentsByBranchingCell, Vector2Int start, Vector2Int end,
                                                     HashSet<List<Vector2Int>> routesPathSegments)
        {
            int routesCount = 0;
            HashSet<Vector2Int> visitedBranchingCells = new() { start };
            List<List<Vector2Int>> currentRoute = new();

            bool isWithinLimit = CollectRoutes(start, end, pathSegmentsByBranchingCell, visitedBranchingCells, currentRoute, routesPathSegments, ref routesCount);

            return isWithinLimit && routesCount > 0;
        }

        private static bool CollectRoutes(Vector2Int currentBranchingCell, Vector2Int end, Dictionary<Vector2Int, List<List<Vector2Int>>> pathSegmentsByBranchingCell,
                                          HashSet<Vector2Int> visitedBranchingCells, List<List<Vector2Int>> currentRoute, HashSet<List<Vector2Int>> routesPathSegments,
                                          ref int routesCount)
        {
            if (currentBranchingCell == end)
            {
                routesCount++;
                routesPathSegments.UnionWith(currentRoute);
                return routesCount <= MaxRoutesCount;
            }

            foreach (List<Vector2Int> pathSegment in pathSegmentsByBranchingCell[currentBranchingCell])
            {
                Vector2Int nextBranchingCell = pathSegment[pathSegment.Count - 1];
                if (visitedBranchingCells.Contains(nextBranchingCell))
                    continue;

                visitedBranchingCells.Add(nextBranchingCell);
                currentRoute.Add(pathSegment);

                bool isWithinLimit = CollectRoutes(nextBranchingCell, end, pathSegmentsByBranchingCell, visitedBranchingCells, currentRoute, routesPathSegments, ref routesCount);

                currentRoute.RemoveAt(currentRoute.Count - 1);
                visitedBranchingCells.Remove(nextBranchingCell);

                if (!isWithinLimit)
                    return false;
            }

            return true;
        }

        private static bool HasCellsOutsideRoutes(HashSet<List<Vector2Int>> routesPathSegments, HashSet<Vector2Int> cells)
        {
            HashSet<Vector2Int> coveredCells = new();

            foreach (List<Vector2Int> pathSegment in routesPathSegments)
                coveredCells.UnionWith(pathSegment);

            return coveredCells.Count != cells.Count;
        }

        

        private static Dictionary<Vector2Int, PathGraphNode> CreateNodes(HashSet<List<Vector2Int>> routesPathSegments, Func<Vector2Int, Vector3> cellToPosition)
        {
            Dictionary<Vector2Int, PathGraphNode> nodes = new();

            foreach (List<Vector2Int> pathSegment in routesPathSegments)
            {
                PathGraphNode previousNode = GetOrCreateNode(nodes, pathSegment[0], cellToPosition);

                for (int i = 1; i < pathSegment.Count - 1; i++)
                {
                    if (pathSegment[i - 1].x != pathSegment[i + 1].x && pathSegment[i - 1].y != pathSegment[i + 1].y)
                    {
                        PathGraphNode cornerNode = GetOrCreateNode(nodes, pathSegment[i], cellToPosition);
                        previousNode.ConnectTo(cornerNode);
                        previousNode = cornerNode;
                    }
                }

                previousNode.ConnectTo(GetOrCreateNode(nodes, pathSegment[pathSegment.Count - 1], cellToPosition));
            }

            return nodes;
        }

        private static PathGraphNode GetOrCreateNode(Dictionary<Vector2Int, PathGraphNode> nodes, Vector2Int cell, Func<Vector2Int, Vector3> cellToPosition)
        {
            if (!nodes.TryGetValue(cell, out PathGraphNode node))
            {
                node = new PathGraphNode(cell, cellToPosition(cell));
                nodes[cell] = node;
            }

            return node;
        }

        private static bool HasCycle(IEnumerable<PathGraphNode> nodes, int nodesCount)
        {
            Dictionary<PathGraphNode, int> countUnvisitedPrevious = new();
            Queue<PathGraphNode> readyNodes = new();

            foreach (PathGraphNode node in nodes)
            {
                countUnvisitedPrevious[node] = node.Previous.Count;
                if (node.Previous.Count == 0)
                    readyNodes.Enqueue(node);
            }

            int countVisited = 0;
            while (readyNodes.Count > 0)
            {
                PathGraphNode node = readyNodes.Dequeue();
                countVisited++;

                foreach (PathGraphNode nextNode in node.Next)
                {
                    countUnvisitedPrevious[nextNode]--;
                    if (countUnvisitedPrevious[nextNode] == 0)
                        readyNodes.Enqueue(nextNode);
                }
            }

            return countVisited != nodesCount;
        }
    }

}
