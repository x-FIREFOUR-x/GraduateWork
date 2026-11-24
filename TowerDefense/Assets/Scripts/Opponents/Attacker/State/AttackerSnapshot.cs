using System;
using System.Collections.Generic;
using System.Linq;

using TowerDefense.Main.Towers;
using TowerDefense.Main.Units;
using TowerDefense.Opponents.SimulationBridge;


namespace TowerDefense.Opponents.Attacker.State
{
    public sealed class AttackerSnapshot
    {
        public IReadOnlyList<UnitType> AvailableUnits { get; }

        public IReadOnlyDictionary<UnitType, int> UnitPrices { get; }

        public IReadOnlyList<TowerType> BuiltTowers { get; }

        public int WaveBudget { get; }
        public SimulationRequest Simulation { get; }


        public AttackerSnapshot(IEnumerable<UnitType> availableUnits, IReadOnlyDictionary<UnitType, int> unitPrices, IEnumerable<TowerType> builtTowers, int waveBudget, SimulationRequest simulation)
        {
            if (availableUnits == null)
                throw new ArgumentNullException(nameof(availableUnits));
            if (unitPrices == null)
                throw new ArgumentNullException(nameof(unitPrices));
            if (builtTowers == null)
                throw new ArgumentNullException(nameof(builtTowers));
            if (waveBudget < 0)
                throw new ArgumentOutOfRangeException(nameof(waveBudget), waveBudget, "Budget must be non-negative.");

            AvailableUnits = availableUnits.Distinct().ToList();
            UnitPrices = AvailableUnits.ToDictionary(unit => unit, unit => PriceOf(unitPrices, unit));
            BuiltTowers = builtTowers.ToList();
            WaveBudget = waveBudget;
            Simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
        }

        public int Cost(IEnumerable<UnitType> units) => units.Sum(unit => UnitPrices[unit]);

        private static int PriceOf(IReadOnlyDictionary<UnitType, int> unitPrices, UnitType unit)
        {
            if (!unitPrices.TryGetValue(unit, out int price))
                throw new ArgumentException($"No price for unit {unit}.", nameof(unitPrices));
            if (price <= 0)
                throw new ArgumentOutOfRangeException(nameof(unitPrices), price, $"Price of unit {unit} must be positive.");
            return price;
        }
    }
}
