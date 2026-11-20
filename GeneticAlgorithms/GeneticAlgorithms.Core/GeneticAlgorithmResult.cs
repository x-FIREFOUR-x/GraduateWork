using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.StopCriteria;

namespace GeneticAlgorithms
{
    public sealed class GeneticAlgorithmResult<TGene>
    {
        internal GeneticAlgorithmResult(Individual<TGene> bestIndividual, IReadOnlyList<Individual<TGene>> population, SearchProgress progress)
        {
            BestIndividual = bestIndividual;
            Population = population;
            Progress = progress;
        }

        public Individual<TGene> BestIndividual { get; }

        public IReadOnlyList<Individual<TGene>> Population { get; }

        public SearchProgress Progress { get; }
    }
}
