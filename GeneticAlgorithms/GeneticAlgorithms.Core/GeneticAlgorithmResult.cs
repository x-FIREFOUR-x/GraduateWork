using System.Collections.Generic;
using GeneticAlgorithms.Individuals;
using GeneticAlgorithms.StopCriteria;

namespace GeneticAlgorithms
{
    public sealed class GeneticAlgorithmResult<TGene>
    {
        internal GeneticAlgorithmResult(Individual<TGene> bestIndividual, IReadOnlyList<Individual<TGene>> population, IReadOnlyList<Individual<TGene>> paretoFront, SearchProgress progress)
        {
            BestIndividual = bestIndividual;
            Population = population;
            ParetoFront = paretoFront;
            Progress = progress;
        }

        // With several objectives one of the non-dominated individuals found; it may have left the final population, so it need not be in ParetoFront.
        public Individual<TGene> BestIndividual { get; }

        public bool IsBestIndividualFeasible => BestIndividual.ConstraintTracker.IsSatisfied;

        public IReadOnlyList<Individual<TGene>> Population { get; }

        // Non-dominated individuals of the final population by the comparer of the survivor selection, without individuals with the same genes;
        // for one objective with FitnessComparer they are the individuals with the best fitness.
        // When the whole final population is infeasible, the front holds the least infeasible individuals, so check ConstraintTracker.IsSatisfied.
        public IReadOnlyList<Individual<TGene>> ParetoFront { get; }

        public SearchProgress Progress { get; }
    }
}
