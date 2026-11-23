using System;

namespace GeneticAlgorithms.StopCriteria.Implementations
{
    public sealed class StagnationStopCriterion : IStopCriterion
    {
        public int IterationsLimitWithoutImprovement { get; }


        public StagnationStopCriterion(int iterationsLimitWithoutImprovement)
        {
            if (iterationsLimitWithoutImprovement < 1)
                throw new ArgumentOutOfRangeException(nameof(iterationsLimitWithoutImprovement), iterationsLimitWithoutImprovement, "Iterations limit without improvement must be at least 1.");
            IterationsLimitWithoutImprovement = iterationsLimitWithoutImprovement;
        }

        public bool ShouldStop(SearchProgress progress) => progress.IterationsCountWithoutImprovement >= IterationsLimitWithoutImprovement;
    }
}
