using System;
using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent.Implementations.Proportional
{
    // Roulette wheel (fitness-proportional) selection (Holland, 1975): the selection probability is proportional
    // to the fitness value with FitnessIndex. Values must be non-negative; if all are zero, selection is uniform.
    // Sensitive to the fitness scale; suitable for single-objective maximization only.
    //
    // Complexity: time O(n + m * log n), memory O(n + m).
    // n - the population size;
    // m - the number of selected parents (2 * parentPairCount).
    public sealed class RouletteWheelParentSelection<TGene> : IParentSelection<TGene>
    {
        // Null: the selection reads the fitness value with FitnessIndex directly instead of comparing individuals.
        public IIndividualComparer<TGene> Comparer => null;

        public bool UsesFitnessValues => true;

        // Index of the fitness value the selection probabilities are computed from; non-negative.
        public int FitnessIndex { get; }


        public RouletteWheelParentSelection(int fitnessIndex = 0)
        {
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

            var selectionProbabilityWeights = ParentSelectionUtilities.GetFitnessValues(population, FitnessIndex);
            return ParentSelectionUtilities.ToPairs(ParentSelectionUtilities.SelectParentsByProbabilityWeights(population, selectionProbabilityWeights, parentCount, random));
        }
    }
}
