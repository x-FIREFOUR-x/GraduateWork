using System;
using System.Collections.Generic;
using Simulation.Geometry;


namespace Simulation.InputData.Map
{
    [Serializable]
    public class PathNodeData
    {
        public int Id;
        public Vec3 Position;
        public List<int> Next = new List<int>();
    }
}
