using System;
using System.Collections.Generic;
using Simulation.Geometry;


namespace Simulation.InputData.Map
{
    [Serializable]
    public class MapData
    {
        public float GroundHeight;
        public Vec3 SpawnPosition;
        public int StartNodeId;
        public List<PathNodeData> PathNodes = new List<PathNodeData>();
        public List<TowerTileData> TowerTiles = new List<TowerTileData>();
    }
}
