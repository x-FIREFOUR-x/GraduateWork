using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using TowerDefense.Main.Towers;
using TowerDefense.Main.Units;
using TowerDefense.Opponents.SimulationBridge;


namespace TowerDefense.Opponents.Defender.State
{
    public sealed class DefenderSnapshot
    {
        public IReadOnlyList<TowerType> AvailableTowers { get; }

        public IReadOnlyDictionary<TowerType, int> TowerPrices { get; }

        public IReadOnlyList<Vector2Int> FreeCells { get; }

        public IReadOnlyList<DefenderGene> BuiltTowers { get; }

        public IReadOnlyList<UnitType> UnitTypesUnlockedByAttacker { get; }

        public IReadOnlyDictionary<TowerType, double> TargetShares { get; }

        public PathCoverageMap CoverageMap { get; }

        public int Budget { get; }
        public SimulationRequest Simulation { get; }
        public IReadOnlyList<UnitType> ExpectedWave { get; }

        public IReadOnlyList<DefenderGene> CandidateGenes { get; }


        public DefenderSnapshot(
            IEnumerable<TowerType> availableTowers,
            IReadOnlyDictionary<TowerType, int> towerPrices,
            IEnumerable<Vector2Int> freeCells,
            IEnumerable<DefenderGene> builtTowers,
            IEnumerable<UnitType> unitTypesUnlockedByAttacker,
            IReadOnlyDictionary<TowerType, double> targetShares,
            PathCoverageMap coverageMap,
            int budget,
            SimulationRequest simulation,
            IEnumerable<UnitType> expectedWave)
        {
            if (availableTowers == null)
                throw new ArgumentNullException(nameof(availableTowers));
            if (towerPrices == null)
                throw new ArgumentNullException(nameof(towerPrices));
            if (freeCells == null)
                throw new ArgumentNullException(nameof(freeCells));
            if (builtTowers == null)
                throw new ArgumentNullException(nameof(builtTowers));
            if (targetShares == null)
                throw new ArgumentNullException(nameof(targetShares));
            if (targetShares.Values.Any(share => !(share >= 0)))
                throw new ArgumentOutOfRangeException(nameof(targetShares), "Target shares must be non-negative.");
            if (budget < 0)
                throw new ArgumentOutOfRangeException(nameof(budget), budget, "Budget must be non-negative.");

            AvailableTowers = availableTowers.Distinct().ToList();
            TowerPrices = AvailableTowers.ToDictionary(tower => tower, tower => PriceOf(towerPrices, tower));
            FreeCells = freeCells.Distinct().ToList();
            BuiltTowers = builtTowers.ToList();
            UnitTypesUnlockedByAttacker = (unitTypesUnlockedByAttacker ?? Enumerable.Empty<UnitType>()).Distinct().ToList();
            TargetShares = new Dictionary<TowerType, double>(targetShares.ToDictionary(share => share.Key, share => share.Value));
            CoverageMap = coverageMap ?? throw new ArgumentNullException(nameof(coverageMap));
            Budget = budget;
            Simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            ExpectedWave = (expectedWave ?? throw new ArgumentNullException(nameof(expectedWave))).ToList();
            CandidateGenes = AvailableTowers
                .SelectMany(tower => FreeCells.Select(cell => new DefenderGene(tower, cell)))
                .ToList();
        }

        public int Cost(IEnumerable<DefenderGene> towers) => towers.Sum(tower => TowerPrices[tower.Type]);

        private static int PriceOf(IReadOnlyDictionary<TowerType, int> towerPrices, TowerType tower)
        {
            if (!towerPrices.TryGetValue(tower, out int price))
                throw new ArgumentException($"No price for tower {tower}.", nameof(towerPrices));
            if (price <= 0)
                throw new ArgumentOutOfRangeException(nameof(towerPrices), price, $"Price of tower {tower} must be positive.");
            return price;
        }
    }
}
