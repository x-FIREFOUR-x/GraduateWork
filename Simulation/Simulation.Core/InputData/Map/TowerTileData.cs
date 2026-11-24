using System;
using Simulation.Geometry;


namespace Simulation.InputData.Map
{
    [Serializable]
    public class TowerTileData
    {
        public Cell Cell;
        public Vec3 BuildPosition;
    }
}
