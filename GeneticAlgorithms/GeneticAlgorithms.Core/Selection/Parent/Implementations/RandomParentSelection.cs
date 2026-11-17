using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent.Implementations
{
    // Random selection: a parent is drawn uniformly from the population, with no selection pressure.
    //
    // Complexity: time O(m), memory O(m).
    // m - the number of selected parents (2 * parentPairCount).
    public sealed class RandomParentSelection<TGene> : IParentSelection<TGene>
    {
        public IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            IIndividualComparer<TGene> comparer,
            int parentPairCount,
            IRandom random)
        {
            ParentSelectionUtilities.Validate(population, comparer, parentPairCount, random);

            int parentCount = 2 * parentPairCount;
            var parents = new Individual<TGene>[parentCount];

            for (int i = 0; i < parentCount; i++)
                parents[i] = population[random.Next(population.Count)];

            return ParentSelectionUtilities.ToPairs(parents);
        }
    }
}
