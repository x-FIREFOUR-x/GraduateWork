using System.Collections.Generic;

using UnityEngine;


namespace TowerDefense.Main.Map.Path
{
    public class PathGraphNode
    {
        private readonly List<PathGraphNode> next = new();
        private readonly List<PathGraphNode> previous = new();

        public Vector2Int Cell { get; }
        public Vector3 Position { get; }

        public IReadOnlyList<PathGraphNode> Next => next;
        public IReadOnlyList<PathGraphNode> Previous => previous;

        public bool IsEnd => next.Count == 0;
        public bool IsFork => next.Count > 1;


        public PathGraphNode(Vector2Int cell, Vector3 position)
        {
            Cell = cell;
            Position = position;
        }

        public void ConnectTo(PathGraphNode node)
        {
            if (next.Contains(node))
                return;

            next.Add(node);
            node.previous.Add(this);
        }
    }

}
