using System;
using System.Collections.Generic;

using GeneticAlgorithms.Fitness;

using TowerDefense.Main.Units;
using TowerDefense.Opponents.Attacker.State;
using TowerDefense.Opponents.SimulationBridge;


namespace TowerDefense.Opponents.Attacker.Evaluation
{
    public sealed class AttackSimulationFitness : IFitnessFunction<UnitType>, INamedFitnessValues
    {
        public const int AttackObjectiveIndex = 0;

        private static readonly string[] ValueNames = { "strength", "remainingBudget", "meanStrength", "entropy" };

        private readonly AttackerSnapshot _snapshot;
        private readonly SimulationEvaluator _evaluator;

        public int FitnessValueCount => ValueNames.Length;

        public IReadOnlyList<string> FitnessValueNames => ValueNames;

        public bool PrefersFeasibleIndividuals => false;


        public AttackSimulationFitness(AttackerSnapshot snapshot)
        {
            _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            _evaluator = new SimulationEvaluator(snapshot.Simulation);
        }

        public double[] Evaluate(IReadOnlyList<UnitType> genes)
        {
            int count = genes.Count;
            double strength = _evaluator.AttackScore(genes);
            double remainingBudget = _snapshot.WaveBudget - _snapshot.Cost(genes);
            double meanStrength = count == 0 ? 0 : strength / count;
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
