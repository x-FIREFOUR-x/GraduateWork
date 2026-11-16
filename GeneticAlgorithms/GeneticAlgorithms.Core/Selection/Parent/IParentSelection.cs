using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;


namespace GeneticAlgorithms.Selection.Parent
{
    // Parent selection: selects the requested number of parent pairs for one generation.
    // Each pair is crossed over to produce one offspring.
    public interface IParentSelection<TGene>
    {
        IReadOnlyList<ParentPair<TGene>> SelectParents(
            IReadOnlyList<Individual<TGene>> population,
            IComparer<Individual<TGene>> comparer,
            int parentPairCount,
            IRandom random);
    }
}
