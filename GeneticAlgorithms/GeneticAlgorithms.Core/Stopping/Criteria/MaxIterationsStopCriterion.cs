using System;

namespace GeneticAlgorithms.Stopping.Criteria
{
    public sealed class MaxIterationsStopCriterion : IStopCriterion
    {
        public int MaxIterations { get; }

        public MaxIterationsStopCriterion(int maxIterations)
        {
            if (maxIterations <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxIterations), maxIterations, "Max iterations must be at least 1.");
            MaxIterations = maxIterations;
        }

        public bool ShouldStop(SearchProgress progress) => progress.IterationCount >= MaxIterations;
    }
}
