using System;
using System.Collections.Generic;
using System.Linq;
using Simulation.Geometry;
using Simulation.InputData.Map;


namespace Simulation.Combat.Paths
{
    internal sealed class PathGraph
    {
        private readonly Dictionary<PathNode, List<PathNode>> reachableOrdered = new Dictionary<PathNode, List<PathNode>>();
        private readonly Dictionary<PathNode, HashSet<PathNode>> reachable = new Dictionary<PathNode, HashSet<PathNode>>();
        private readonly Dictionary<PathNode, PathNode> forkEnds = new Dictionary<PathNode, PathNode>();

        public PathNode Start { get; }

        public IReadOnlyList<PathNode> Nodes { get; }

        public int EdgeCount { get; }

        public float ShortestRouteLength { get; private set; }

        private PathGraph(PathNode start, List<PathNode> nodes)
        {
            Start = start;
            Nodes = nodes;

            int edgeCount = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Index = i;
                for (int j = 0; j < nodes[i].Next.Count; j++)
                    nodes[i].AddEdgeIndex(edgeCount++);
            }
            EdgeCount = edgeCount;

            foreach (PathNode node in nodes)
            {
                List<PathNode> ordered = FindReachableOrdered(node);
                reachableOrdered[node] = ordered;
                reachable[node] = new HashSet<PathNode>(ordered);
            }

            foreach (PathNode node in nodes)
            {
                if (node.IsFork)
                    forkEnds[node] = FindForkEnd(node);
            }
        }

        public static PathGraph Build(MapData map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));
            if (map.PathNodes == null || map.PathNodes.Count == 0)
                throw new ArgumentException("Map has no path nodes.", nameof(map));

            var nodesById = new Dictionary<int, PathNode>();
            var nodes = new List<PathNode>();
            foreach (PathNodeData data in map.PathNodes)
            {
                var node = new PathNode(data.Position);
                if (!nodesById.TryAdd(data.Id, node))
                    throw new ArgumentException($"Path node id {data.Id} is duplicated.", nameof(map));
                nodes.Add(node);
            }

            foreach (PathNodeData data in map.PathNodes)
            {
                if (data.Next == null)
                    continue;
                foreach (int nextId in data.Next)
                {
                    if (!nodesById.TryGetValue(nextId, out PathNode next))
                        throw new ArgumentException($"Path node {data.Id} refers to missing node {nextId}.", nameof(map));
                    nodesById[data.Id].Connect(next);
                }
            }

            if (!nodesById.TryGetValue(map.StartNodeId, out PathNode start))
                throw new ArgumentException($"Start node {map.StartNodeId} is missing.", nameof(map));

            var graph = new PathGraph(start, nodes);
            if (graph.HasCycle())
                throw new ArgumentException("Path graph contains a cycle.", nameof(map));
            if (!graph.CanReachEnd(start))
                throw new ArgumentException("End of the path is not reachable from the start.", nameof(map));
            graph.ShortestRouteLength = graph.ShortestLengthToEnd(start, new Dictionary<PathNode, float>());
            return graph;
        }

        public bool CanReach(PathNode from, PathNode to) => reachable[from].Contains(to);

        public PathNode ForkEnd(PathNode fork) => forkEnds.TryGetValue(fork, out PathNode end) ? end : null;

        private float ShortestLengthToEnd(PathNode node, Dictionary<PathNode, float> lengths)
        {
            if (node.IsEnd)
                return 0f;
            if (lengths.TryGetValue(node, out float cached))
                return cached;

            float shortest = float.PositiveInfinity;
            foreach (PathNode next in node.Next)
            {
                if (next.IsEnd || reachable[next].Any(reachableNode => reachableNode.IsEnd))
                    shortest = Math.Min(shortest, Vec3.Distance(node.Position, next.Position) + ShortestLengthToEnd(next, lengths));
            }
            lengths[node] = shortest;
            return shortest;
        }

        private bool HasCycle()
        {
            foreach (PathNode node in Nodes)
            {
                foreach (PathNode next in node.Next)
                {
                    if (next == node || reachable[next].Contains(node))
                        return true;
                }
            }
            return false;
        }

        private bool CanReachEnd(PathNode from)
        {
            if (from.IsEnd)
                return true;
            foreach (PathNode node in reachableOrdered[from])
            {
                if (node.IsEnd)
                    return true;
            }
            return false;
        }

        private PathNode FindForkEnd(PathNode fork)
        {
            HashSet<PathNode> common = null;
            foreach (PathNode branch in fork.Next)
            {
                var branchNodes = new HashSet<PathNode>(reachable[branch]) { branch };
                if (common == null)
                    common = branchNodes;
                else
                    common.IntersectWith(branchNodes);
            }

            foreach (PathNode candidate in reachableOrdered[fork])
            {
                if (common.Contains(candidate) && !CanReachEndAvoiding(fork, candidate))
                    return candidate;
            }
            return null;
        }

        private static List<PathNode> FindReachableOrdered(PathNode from)
        {
            var ordered = new List<PathNode>();
            var reached = new HashSet<PathNode> { from };
            var queue = new Queue<PathNode>();
            queue.Enqueue(from);
            while (queue.Count > 0)
            {
                foreach (PathNode next in queue.Dequeue().Next)
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

        private static bool CanReachEndAvoiding(PathNode from, PathNode avoided)
        {
            var reached = new HashSet<PathNode> { from, avoided };
            var queue = new Queue<PathNode>();
            queue.Enqueue(from);
            while (queue.Count > 0)
            {
                PathNode node = queue.Dequeue();
                if (node.IsEnd)
                    return true;
                foreach (PathNode next in node.Next)
                {
                    if (reached.Add(next))
                        queue.Enqueue(next);
                }
            }
            return false;
        }
    }
}
