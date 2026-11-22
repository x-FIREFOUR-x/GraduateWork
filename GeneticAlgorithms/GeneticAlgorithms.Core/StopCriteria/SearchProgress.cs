using System;
using System.Collections.Generic;

namespace GeneticAlgorithms.StopCriteria
{
    public sealed class SearchProgress
    {
        public int IterationCount { get; internal set; }

        public int FitnessFunctionCallCount { get; internal set; }

        public TimeSpan ElapsedTime { get; internal set; }

        public int NumberLastImprovementIteration { get; internal set; }

        public int IterationsCountWithoutImprovement => IterationCount - NumberLastImprovementIteration;

        public IReadOnlyList<double> BestFitnessValues { get; internal set; }

        public double IndividualsShareConstraintSatisfying { get; internal set; }

        public bool Cancelled { get; internal set; }
    }
}
