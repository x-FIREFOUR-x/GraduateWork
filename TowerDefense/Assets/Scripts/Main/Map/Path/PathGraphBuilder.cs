using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Route = System.Collections.Generic.List<System.Collections.Generic.List<UnityEngine.Vector2Int>>;


namespace TowerDefense.Main.Map.Path
{
    // Path - the whole road on the map with all its branches (all path cells).
    // Branching cell - a path cell where a path segment ends: the start, the end, and every cell whose count of neighboring path cells is not 2 (forks, merges, dead ends).
    // Path segment - a part of the path between two neighboring branching cells, its cells go in order from one end to the other.
    // Route - one way to pass the path from the start to the end, a sequence of path segments. The path contains several routes.
    public static class PathGraphBuilder
    {
        private const int MaxRoutesCount = 1000;

        private static readonly Vector2Int[] Directions = { new(1, 0), new(0, 1), new(-1, 0), new(0, -1) };


        public static bool TryBuild(HashSet<Vector2Int> pathCells, Vector2Int start, Vector2Int end, Func<Vector2Int, Vector3> cellToPosition,
                                    out PathGraph graph, out PathGraphBuildError error)
        {
            graph = null;

            if (start == end)
            {
                error = PathGraphBuildError.StartIsEnd;
                return false;
            }

            HashSet<Vector2Int> cells = new(pathCells) { start, end };
            HashSet<Vector2Int> branchingCells = FindBranchingCells(cells, start, end);
            Dictionary<Vector2Int, List<List<Vector2Int>>> pathSegmentsByBranchingCell = FindPathSegments(cells, branchingCells);

            List<Route> routes = new();
            if (!TryCollectRoutesFromPathSegments(pathSegmentsByBranchingCell, start, end, routes, out error))
                return false;

            if (HasCellsOutsideRoutes(CollectUniquePathSegments(routes), cells))
            {
                error = PathGraphBuildError.CellsOutsideRoutes;
                return false;
            }

            HashSet<List<Vector2Int>> pathSegments = CollectUniquePathSegments(SelectRoutesWithoutCycles(routes));
            if (HasCellsOutsideRoutes(pathSegments, cells))
            {
                error = PathGraphBuildError.Cycle;
                return false;
            }

            Dictionary<Vector2Int, PathGraphNode> nodes = CreateNodes(pathSegments, cellToPosition);

            graph = new PathGraph(nodes[start], nodes[end], new List<PathGraphNode>(nodes.Values));
            error = PathGraphBuildError.None;
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
                                                     List<Route> routes, out PathGraphBuildError error)
        {
            HashSet<Vector2Int> visitedBranchingCells = new() { start };
            Route currentRoute = new();

            bool isWithinLimit = CollectRoutes(start, end, pathSegmentsByBranchingCell, visitedBranchingCells, currentRoute, routes);

            if (!isWithinLimit)
                error = PathGraphBuildError.TooManyRoutes;
            else if (routes.Count == 0)
                error = PathGraphBuildError.NoRoute;
            else
                error = PathGraphBuildError.None;

            return error == PathGraphBuildError.None;
        }

        private static bool CollectRoutes(Vector2Int currentBranchingCell, Vector2Int end, Dictionary<Vector2Int, List<List<Vector2Int>>> pathSegmentsByBranchingCell,
                                          HashSet<Vector2Int> visitedBranchingCells, Route currentRoute, List<Route> routes)
        {
            if (currentBranchingCell == end)
            {
                routes.Add(new Route(currentRoute));
                return routes.Count <= MaxRoutesCount;
            }

            foreach (List<Vector2Int> pathSegment in pathSegmentsByBranchingCell[currentBranchingCell])
            {
                Vector2Int nextBranchingCell = pathSegment[pathSegment.Count - 1];
                if (visitedBranchingCells.Contains(nextBranchingCell))
                    continue;

                visitedBranchingCells.Add(nextBranchingCell);
                currentRoute.Add(pathSegment);

                bool isWithinLimit = CollectRoutes(nextBranchingCell, end, pathSegmentsByBranchingCell, visitedBranchingCells, currentRoute, routes);

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

        private static HashSet<List<Vector2Int>> CollectUniquePathSegments(IEnumerable<Route> routes)
        {
            HashSet<List<Vector2Int>> pathSegments = new();

            foreach (Route route in routes)
                pathSegments.UnionWith(route);

            return pathSegments;
        }

        private static List<Route> SelectRoutesWithoutCycles(List<Route> routes)
        {
            List<Route> selectedRoutes = new();
            Dictionary<Vector2Int, HashSet<Vector2Int>> connectionsByBranchingCell = new();

            foreach (Route route in routes.OrderBy(CountRouteCells))
            {
                List<(Vector2Int From, Vector2Int To)> addedConnections = new();

                foreach (List<Vector2Int> pathSegment in route)
                {
                    Vector2Int from = pathSegment[0];
                    Vector2Int to = pathSegment[pathSegment.Count - 1];

                    if (!connectionsByBranchingCell.TryGetValue(from, out HashSet<Vector2Int> nextCells))
                    {
                        nextCells = new HashSet<Vector2Int>();
                        connectionsByBranchingCell[from] = nextCells;
                    }

                    if (nextCells.Add(to))
                        addedConnections.Add((from, to));
                }

                if (HasCycle(connectionsByBranchingCell))
                {
                    foreach ((Vector2Int from, Vector2Int to) in addedConnections)
                        connectionsByBranchingCell[from].Remove(to);

                    continue;
                }

                selectedRoutes.Add(route);
            }

            return selectedRoutes;
        }

        private static int CountRouteCells(Route route)
        {
            int countCells = 0;

            foreach (List<Vector2Int> pathSegment in route)
                countCells += pathSegment.Count;

            return countCells;
        }

        private static bool HasCycle(Dictionary<Vector2Int, HashSet<Vector2Int>> connectionsByBranchingCell)
        {
            Dictionary<Vector2Int, int> countUnvisitedPrevious = new();

            foreach (KeyValuePair<Vector2Int, HashSet<Vector2Int>> branchingCell in connectionsByBranchingCell)
            {
                countUnvisitedPrevious.TryAdd(branchingCell.Key, 0);

                foreach (Vector2Int nextBranchingCell in branchingCell.Value)
                    countUnvisitedPrevious[nextBranchingCell] = countUnvisitedPrevious.GetValueOrDefault(nextBranchingCell) + 1;
            }

            Queue<Vector2Int> branchingCellsWithVisitedPrevious = new();
            foreach (KeyValuePair<Vector2Int, int> branchingCell in countUnvisitedPrevious)
            {
                if (branchingCell.Value == 0)
                    branchingCellsWithVisitedPrevious.Enqueue(branchingCell.Key);
            }

            int countVisited = 0;
            while (branchingCellsWithVisitedPrevious.Count > 0)
            {
                Vector2Int branchingCell = branchingCellsWithVisitedPrevious.Dequeue();
                countVisited++;

                if (!connectionsByBranchingCell.TryGetValue(branchingCell, out HashSet<Vector2Int> nextCells))
                    continue;

                foreach (Vector2Int nextBranchingCell in nextCells)
                {
                    countUnvisitedPrevious[nextBranchingCell]--;
                    if (countUnvisitedPrevious[nextBranchingCell] == 0)
                        branchingCellsWithVisitedPrevious.Enqueue(nextBranchingCell);
                }
            }

            return countVisited != countUnvisitedPrevious.Count;
        }
    }

}
