using System.Collections.Generic;
using Simulation.Geometry;


namespace Simulation.Combat.Paths
{
    internal sealed class PathNode
    {
        private readonly List<PathNode> next = new List<PathNode>();
        private readonly List<int> nextEdges = new List<int>();

        public Vec3 Position { get; }

        public IReadOnlyList<PathNode> Next => next;

        public bool IsEnd => next.Count == 0;

        public bool IsFork => next.Count > 1;

        internal int Index { get; set; }

        internal PathNode(Vec3 position)
        {
            Position = position;
        }

        internal int EdgeTo(PathNode node)
        {
            for (int i = 0; i < next.Count; i++)
            {
                if (next[i] == node)
                    return nextEdges[i];
            }
            return -1;
        }

        internal void AddEdgeIndex(int edgeIndex) => nextEdges.Add(edgeIndex);

        internal void Connect(PathNode node)
        {
            if (!next.Contains(node))
                next.Add(node);
        }
    }
}
