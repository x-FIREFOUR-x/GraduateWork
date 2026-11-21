using System;
using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent.Implementations.Proportional
{
    // Roulette wheel selection with sigma scaling (Forrest, 1985): the weight of an individual is
    // max(0, 1 + (f - mean) / (c * sigma)), where mean and sigma are the mean and standard deviation of fitness
    // in the population and c = ScalingFactor. The selection pressure is independent of the fitness scale and stays
    // constant during the search. If sigma = 0, selection is uniform. Allows negative fitness values.
    //
    // Complexity: time O(n + m * log n), memory O(n + m).
    // n - the population size;
    // m - the number of selected parents (2 * parentPairCount).
    public sealed class SigmaScalingParentSelection<TGene> : IParentSelection<TGene>
    {
        // Null: the selection reads the fitness value with FitnessIndex directly instead of comparing individuals.
        public IIndividualComparer<TGene> Comparer => null;

        public bool UsesFitnessValues => true;

        // Index of the fitness value the selection probabilities are computed from; non-negative.
        public int FitnessIndex { get; }

        public double ScalingFactor { get; }


        public SigmaScalingParentSelection(double scalingFactor = 2, int fitnessIndex = 0)
        {
            if (!(scalingFactor > 0) || double.IsInfinity(scalingFactor))
                throw new ArgumentOutOfRangeException(nameof(scalingFactor), scalingFactor, "Scaling factor must be positive and finite.");
            ScalingFactor = scalingFactor;
            if (fitnessIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(fitnessIndex), fitnessIndex, "Fitness index must be non-negative.");
            FitnessIndex = fitnessIndex;
        }

        public IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            int parentPairCount,
            IRandom random)
        {
            ParentSelectionUtilities.Validate(population, parentPairCount, random);

            int parentCount = 2 * parentPairCount;
            var fitnessValues = ParentSelectionUtilities.GetFitnessValues(population, FitnessIndex);

            double meanFitness = 0;
            foreach (double fitness in fitnessValues)
                meanFitness += fitness;
            meanFitness /= fitnessValues.Length;

            double variance = 0;
            foreach (double fitness in fitnessValues)
                variance += (fitness - meanFitness) * (fitness - meanFitness);
            double standardDeviation = Math.Sqrt(variance / fitnessValues.Length);

            var selectionProbabilityWeights = new double[fitnessValues.Length];
            for (int i = 0; i < fitnessValues.Length; i++)
            {
                selectionProbabilityWeights[i] = standardDeviation > 0
                    ? Math.Max(0, 1 + (fitnessValues[i] - meanFitness) / (ScalingFactor * standardDeviation))
                    : 1;
            }

            return ParentSelectionUtilities.ToPairs(ParentSelectionUtilities.SelectParentsByProbabilityWeights(population, selectionProbabilityWeights, parentCount, random));
        }
    }
}
