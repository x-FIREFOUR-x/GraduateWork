using System;
using System.Collections.Generic;
using System.Linq;

using GeneticAlgorithms.Fitness;

using TowerDefense.Main.Towers;
using TowerDefense.Main.Units;
using TowerDefense.Opponents.Defender.State;


namespace TowerDefense.Opponents.Defender.Evaluation
{
    public sealed class DefenceMultiObjectiveFitness : IFitnessFunction<DefenderGene>
    {
        public const int CoverageObjectiveIndex = 0;

        private readonly DefenderSnapshot _snapshot;
        private readonly DefenceHeuristicFitness _heuristicFitness;
        private readonly IReadOnlyList<UnitType> _threatUnits;

        public int FitnessValueCount => 4;

        public bool PrefersFeasibleIndividuals => false;


        public DefenceMultiObjectiveFitness(DefenderSnapshot snapshot)
        {
            _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            _heuristicFitness = new DefenceHeuristicFitness(snapshot, 0);
            _threatUnits = snapshot.UnitTypesUnlockedByAttacker.Count > 0
                ? snapshot.UnitTypesUnlockedByAttacker
                : Enum.GetValues(typeof(UnitType)).Cast<UnitType>().ToList();
        }

        public double[] Evaluate(IReadOnlyList<DefenderGene> genes)
        {
            double coverageGain = _heuristicFitness.CoverageGain(genes);
            double balance = _heuristicFitness.Balance(genes);
            double remainingBudget = _snapshot.Budget - _snapshot.Cost(genes);
            return new[] { coverageGain, balance, remainingBudget, Robustness(genes) };
        }

        private double Robustness(IReadOnlyList<DefenderGene> genes)
        {
            int[] countByType = _heuristicFitness.CountByTypeAfter(genes);

            double weakestResistance = double.MaxValue;
            foreach (UnitType unit in _threatUnits)
            {
                double resistance = 0;
                for (int type = 0; type < countByType.Length; type++)
                    if (countByType[type] > 0)
                        resistance += countByType[type] * (AdvantageMatrix.MaxAdvantagePoints - AdvantageMatrix.GetAdvantagePointsUnitOverTower(unit, (TowerType)type));
                weakestResistance = Math.Min(weakestResistance, resistance);
            }
            return weakestResistance == double.MaxValue ? 0 : weakestResistance;
        }
    }
}
