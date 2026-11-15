using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.Randomness;

namespace GeneticAlgorithms.Selection.Parent
{
    public interface IParentSelection<TGene>
    {
        Individual<TGene> Select(
            IReadOnlyList<Individual<TGene>> population,
            IComparer<Individual<TGene>> comparer,
            IRandom random);
    }
}
