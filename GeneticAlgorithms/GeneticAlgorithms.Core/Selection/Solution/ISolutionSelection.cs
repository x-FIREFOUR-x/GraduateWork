using System.Collections.Generic;
using GeneticAlgorithms.Individuals;


namespace GeneticAlgorithms.Selection.Solution
{
    // Solution selection: chooses one individual from a Pareto front, for example GeneticAlgorithmResult.ParetoFront,
    // when several objectives are optimized and one solution has to be used.
    public interface ISolutionSelection<TGene>
    {
        Individual<TGene> Select(IReadOnlyList<Individual<TGene>> front);
    }
}
