using System;
using System.Collections.Generic;

using GeneticAlgorithms.Fitness;

using TowerDefense.Main.Units;
using TowerDefense.Opponents.Attacker.State;


namespace TowerDefense.Opponents.Attacker.Evaluation
{
    public sealed class WaveMultiObjectiveFitness : IFitnessFunction<UnitType>
    {
        private readonly AttackerSnapshot _snapshot;
        private readonly WaveHeuristicFitness _waveHeuristicFitness;

        public int FitnessValueCount => 4;

        public bool PrefersFeasibleIndividuals => false;


        public WaveMultiObjectiveFitness(AttackerSnapshot snapshot)
        {
            _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            _waveHeuristicFitness = new WaveHeuristicFitness(snapshot);
        }

        public double[] Evaluate(IReadOnlyList<UnitType> genes)
        {
            int count = genes.Count;
            double strength = _waveHeuristicFitness.Value(genes);
            double remainingBudget = _snapshot.WaveBudget - _snapshot.Cost(genes);
            double meanStrength = count == 0 ? 0 : (strength - count) / count;
            return new[] { strength, remainingBudget, meanStrength, Entropy(genes) };
        }

        private static double Entropy(IReadOnlyList<UnitType> genes)
        {
            if (genes.Count == 0)
                return 0;

            var countsOfUnitType = new Dictionary<UnitType, int>();
            foreach (UnitType unit in genes)
                countsOfUnitType[unit] = countsOfUnitType.TryGetValue(unit, out int count) ? count + 1 : 1;

            double entropy = 0;
            foreach (int count in countsOfUnitType.Values)
            {
                double share = (double)count / genes.Count;
                entropy -= share * Math.Log(share);
            }
            return entropy;
        }
    }
}
