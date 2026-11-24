using Simulation;
using System;
using System.Collections.Generic;
using System.Linq;



namespace TowerDefense.Opponents.SimulationBridge
{
    public sealed class SimulationRequest
    {
        public string InputPath { get; }
        public IReadOnlyList<PlacedTower> BuiltTowers { get; }
        public int MaxWaveUnits { get; }
        public int Seed { get; }


        public SimulationRequest(string inputPath, IEnumerable<PlacedTower> builtTowers, int maxWaveUnits, int seed)
        {
            if (string.IsNullOrEmpty(inputPath))
                throw new ArgumentException("Simulation input path is empty.", nameof(inputPath));
            if (builtTowers == null)
                throw new ArgumentNullException(nameof(builtTowers));
            if (maxWaveUnits <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxWaveUnits), maxWaveUnits, "Max wave units must be positive.");

            InputPath = inputPath;
            BuiltTowers = builtTowers.ToList();
            MaxWaveUnits = maxWaveUnits;
            Seed = seed;
        }
    }
}
