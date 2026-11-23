using System;
using System.Collections.Generic;
using System.Linq;
using GeneticAlgorithms.Individuals;


namespace GeneticAlgorithms.Selection.Solution
{
    public sealed class QuantileSolutionSelection<TGene> : ISolutionSelection<TGene>
    {
        private const double RoundingErrorTolerance = 1e-9;

        public int ObjectiveIndex { get; }

        public double Beta { get; }


        public QuantileSolutionSelection(int objectiveIndex, double beta)
        {
            if (objectiveIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(objectiveIndex), objectiveIndex, "Objective index must be non-negative.");
            ObjectiveIndex = objectiveIndex;

            if (!(beta >= 0 && beta <= 1))
                throw new ArgumentOutOfRangeException(nameof(beta), beta, "Beta must be in [0; 1].");
            Beta = beta;
        }

        public Individual<TGene> Select(IReadOnlyList<Individual<TGene>> front)
        {
            Validate(front);

            var ordered = front.OrderBy(individual => individual.FitnessValues[ObjectiveIndex]).ToList();
            int position = (int)Math.Ceiling(Beta * (ordered.Count - 1) - RoundingErrorTolerance);
            return ordered[position];
        }

        private void Validate(IReadOnlyList<Individual<TGene>> front)
        {
            SolutionSelectionUtilities.Validate(front);

            int objectiveCount = front[0].FitnessValues.Count;
            if (ObjectiveIndex >= objectiveCount)
                throw new ArgumentOutOfRangeException(nameof(front), ObjectiveIndex, $"Objective index {ObjectiveIndex} is out of range for {objectiveCount} objectives.");
        }
    }
}
