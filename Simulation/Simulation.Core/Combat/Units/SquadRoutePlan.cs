using System.Collections.Generic;
using Simulation.Combat.Paths;


namespace Simulation.Combat.Units
{
    internal sealed class SquadRoutePlan
    {
        private readonly PathGraph graph;
        private readonly Dictionary<PathNode, PathNode> nextByFork = new Dictionary<PathNode, PathNode>();

        public SquadRoutePlan(PathGraph graph)
        {
            this.graph = graph;
        }

        public void Record(PathNode fork, PathNode next) => nextByFork[fork] = next;

        public bool TryGetNext(PathNode fork, out PathNode next) => nextByFork.TryGetValue(fork, out next);

        public bool HasPlannedForkAtOrAfter(PathNode node)
        {
            foreach (PathNode fork in nextByFork.Keys)
            {
                if (fork == node || graph.CanReach(node, fork))
                    return true;
            }
            return false;
        }
    }
}
