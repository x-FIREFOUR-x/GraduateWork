using System.Collections.Generic;

using UnityEngine;


namespace TowerDefense.Main.Map.Path
{
    public class WayPointNode
    {
        private readonly List<WayPointNode> next = new();
        private readonly List<WayPointNode> previous = new();

        public Vector2Int Cell { get; }
        public Vector3 Position { get; }

        public IReadOnlyList<WayPointNode> Next => next;
        public IReadOnlyList<WayPointNode> Previous => previous;

        public bool IsEnd => next.Count == 0;
        public bool IsFork => next.Count > 1;


        public WayPointNode(Vector2Int cell, Vector3 position)
        {
            Cell = cell;
            Position = position;
        }

        public void ConnectTo(WayPointNode node)
        {
            if (next.Contains(node))
                return;

            next.Add(node);
            node.previous.Add(this);
        }
    }

}
