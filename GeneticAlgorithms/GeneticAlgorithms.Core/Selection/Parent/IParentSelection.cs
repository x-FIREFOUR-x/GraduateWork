using System.Collections.Generic;
using GeneticAlgorithms.Comparison;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent
{
    // Parent selection: selects the requested number of parent pairs for one generation.
    // Each pair is crossed over to produce one offspring.
    public interface IParentSelection<TGene>
    {
        // parentPairCount: number of parent pairs, one per offspring.
        IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            IIndividualComparer<TGene> comparer,
            int parentPairCount,
            IRandom random);
    }
}
