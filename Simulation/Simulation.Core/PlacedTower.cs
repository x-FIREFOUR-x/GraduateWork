using System;
using Simulation.Geometry;


namespace Simulation
{
    [Serializable]
    public struct PlacedTower
    {
        public string Type;
        public Cell Cell;

        public PlacedTower(string type, Cell cell)
        {
            Type = type;
            Cell = cell;
        }
    }
}
