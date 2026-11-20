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
        public IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            IIndividualComparer<TGene> comparer,
            int parentPairCount,
            IRandom random)
        {
            ParentSelectionUtilities.Validate(population, comparer, parentPairCount, random);

            int parentCount = 2 * parentPairCount;

            var selectionProbabilityWeights = ParentSelectionUtilities.GetFitnessValues(population, comparer);

            return ParentSelectionUtilities.ToPairs(ParentSelectionUtilities.SelectParentsByStochasticUniversalSampling(population, selectionProbabilityWeights, parentCount, random));
        }
    }
}
