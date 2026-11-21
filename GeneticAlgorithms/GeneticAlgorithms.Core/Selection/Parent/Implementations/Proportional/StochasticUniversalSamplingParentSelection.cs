using System;
using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent.Implementations.Proportional
{
    // Stochastic universal sampling (Baker, 1987): a single wheel with one equally spaced pointer per parent
    // shifted by one random number. Probabilities match the roulette wheel, but the number of copies of each
    // individual deviates from its expectation by at most one. The selected pool is shuffled.
    //
    // Complexity: time O(n + m), memory O(n + m).
    // n - the population size;
    // m - the number of selected parents (2 * parentPairCount).
    public sealed class StochasticUniversalSamplingParentSelection<TGene> : IParentSelection<TGene>
    {
        // Null: the selection reads the fitness value with FitnessIndex directly instead of comparing individuals.
        public IIndividualComparer<TGene> Comparer => null;

        public bool UsesFitnessValues => true;

        // Index of the fitness value the selection probabilities are computed from; non-negative.
        public int FitnessIndex { get; }


        public StochasticUniversalSamplingParentSelection(int fitnessIndex = 0)
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

            return ParentSelectionUtilities.ToPairs(ParentSelectionUtilities.SelectParentsByStochasticUniversalSampling(population, selectionProbabilityWeights, parentCount, random));
        }
    }
}
