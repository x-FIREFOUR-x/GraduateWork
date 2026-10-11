using System.Collections.Generic;


namespace TowerDefense.Main.Map.Path
{
    public class PathGraph
    {
        public WayPointNode Start { get; }
        public WayPointNode End { get; }
        public IReadOnlyList<WayPointNode> Nodes { get; }


        public PathGraph(WayPointNode start, WayPointNode end, IReadOnlyList<WayPointNode> nodes)
        {
            Start = start;
            End = end;
            Nodes = nodes;
        }
    }

}
