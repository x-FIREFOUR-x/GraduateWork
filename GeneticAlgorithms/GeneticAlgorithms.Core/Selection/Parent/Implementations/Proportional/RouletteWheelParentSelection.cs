using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent.Implementations.Proportional
{
    // Roulette wheel (fitness-proportional) selection (Holland, 1975): the selection probability is proportional
    // to the fitness value used by the FitnessComparer. Values must be non-negative; if all are zero, selection is uniform.
    // Sensitive to the fitness scale; suitable for single-objective maximization only.
    public sealed class RouletteWheelParentSelection<TGene> : IParentSelection<TGene>
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
            return ParentSelectionUtilities.ToPairs(ParentSelectionUtilities.SelectParentsByProbabilityWeights(population, selectionProbabilityWeights, parentCount, random));
        }
    }
}
