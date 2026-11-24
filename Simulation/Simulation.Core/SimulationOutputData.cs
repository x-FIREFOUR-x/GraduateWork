using System.Collections.Generic;


namespace Simulation
{
    public sealed class SimulationOutputData
    {
        public int CastleDamage { get; internal set; }
        public int ReachedCount { get; internal set; }
        public int KilledCount { get; internal set; }
        public bool CastleDestroyed { get; internal set; }
        public float Duration { get; internal set; }
        public List<float> DistanceMoved { get; } = new List<float>();
    }
}
