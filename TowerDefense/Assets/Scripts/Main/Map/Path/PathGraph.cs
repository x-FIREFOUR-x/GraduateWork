using System.Collections.Generic;


namespace TowerDefense.Main.Map.Path
{
    public class PathGraph
    {
        public PathGraphNode Start { get; }
        public PathGraphNode End { get; }
        public IReadOnlyList<PathGraphNode> Nodes { get; }


        public PathGraph(PathGraphNode start, PathGraphNode end, IReadOnlyList<PathGraphNode> nodes)
        {
            Start = start;
            End = end;
            Nodes = nodes;
        }
    }

}
