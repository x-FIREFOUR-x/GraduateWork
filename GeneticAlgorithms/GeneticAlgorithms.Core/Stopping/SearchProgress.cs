using System;
using System.Collections.Generic;

namespace GeneticAlgorithms.Stopping
{
    public sealed class SearchProgress
    {
        public int Iteration { get; internal set; }

        public int FitnessFunctionCallCount { get; internal set; }

        public TimeSpan ElapsedTime { get; internal set; }

        public int LastImprovementIteration { get; internal set; }

        public int IterationsWithoutImprovement => Iteration - LastImprovementIteration;

        public IReadOnlyList<double> BestFitnessValues { get; internal set; }

        public double IndividualsShareConstraintSatisfying { get; internal set; }

        public bool Cancelled { get; internal set; }
    }
}
