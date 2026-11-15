using System.Collections.Generic;
using GeneticAlgorithms.Individuals;

namespace GeneticAlgorithms.Selection.Survivor
{
    public interface ISurvivorSelection<TGene>
    {
        IIndividualComparer<TGene> Comparer { get; }

        void Initialize(List<Individual<TGene>> population);

        int Replace(List<Individual<TGene>> population, IReadOnlyList<Individual<TGene>> offspring);
    }
}
