using System;
using System.Collections.Generic;
using System.Linq;

using GeneticAlgorithms.Fitness;

using TowerDefense.Main.Units;
using TowerDefense.Opponents.Attacker.State;


namespace TowerDefense.Opponents.Attacker.Evaluation
{
    public sealed class AttackHeuristicFitness : IFitnessFunction<UnitType>
    {
        private readonly Dictionary<UnitType, double> _heuristicValueByUnit;

        public int FitnessValueCount => 1;

        public bool PrefersFeasibleIndividuals => false;


        public AttackHeuristicFitness(AttackerSnapshot snapshot)
        {
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            _heuristicValueByUnit = snapshot.AvailableUnits.ToDictionary(unit => unit, unit => 1.0 + AdvantageMatrix.GetSumAdvantagePointsUnitOverAllTowers(unit, snapshot.BuiltTowers));
        }

        public double[] Evaluate(IReadOnlyList<UnitType> genes) => new[] { Value(genes) };

        public double UnitValue(UnitType unit) => _heuristicValueByUnit[unit];

        public double Value(IReadOnlyList<UnitType> genes)
        {
            double value = 0;
            foreach (UnitType unit in genes)
                value += _heuristicValueByUnit[unit];
            return value;
        }
    }
}
